// Licensed under MIT; see License.md for disclaimer, details, and extended user conditions.

using System.Globalization;
using System.IO;
using System.IO.Compression;

namespace BlueTable.Tests;

/// <summary>
/// Liest Datei-Bestände als Vergleichs-Inventar (Zip-Eintrag-Datum normalisiert) und vergleicht zwei Stände:
/// relevante Dateien dürfen nicht verschwinden oder abweichen; neue Dateien sind nur in write-once-Datenbereichen
/// erlaubt und müssen dort den Vorher-Stand des jüngsten Inhalts tragen. Jede Abweichung wird mit Datei, Bereich
/// und Byte-Position gemeldet, damit eine andere Instanz gezielt reparieren kann.
/// </summary>
public static class FileInventory {

    #region Fields

    /// <summary>
    /// Feste Referenz für das Zip-Eintrag-Datum: Zwei Speicherungen desselben Inhalts unterscheiden sich ausschließlich in diesem Datum.
    /// </summary>
    private static readonly DateTimeOffset FixedZipDate = new(2000, 1, 1, 0, 0, 0, TimeSpan.Zero);

    #endregion

    #region Methods

    /// <summary>
    /// Liest alle Dateien des Ordners rekursiv; Key ist der relative Pfad in Kleinbuchstaben, der Inhalt mit normalisiertem Zip-Datum.
    /// </summary>
    public static Dictionary<string, byte[]> Of(string folder) {
        var result = new Dictionary<string, byte[]>(StringComparer.OrdinalIgnoreCase);

        if (!Directory.Exists(folder)) { return result; }

        foreach (var file in IO.GetFiles(folder, "*", SearchOption.AllDirectories)) {
            var rel = file[folder.Length..].TrimStart('\\').ToLowerInvariant();
            var raw = IO.ReadAllBytes(file, 5).Value;

            result.Add(rel, raw is byte[] bytes and { Length: > 0 } ? NormalizeZipDate(bytes) ?? [] : []);
        }

        return result;
    }

    /// <summary>
    /// Vergleicht Vorher- und Nachher-Bestand eines Roundtrips und trägt jede Abweichung in die Problemliste ein.
    /// </summary>
    public static void CompareRoundtrip(string context, string mainFileRel, Dictionary<string, byte[]> before, Dictionary<string, byte[]> after, string[] areaSuffixes, string[] ignoreSuffixes, string[] ignoreFolderMarkers, bool stripFragmentHead, List<string> problems) {
        foreach (var (rel, bytes) in before) {
            if (!IsRelevant(rel, mainFileRel, areaSuffixes, ignoreSuffixes, ignoreFolderMarkers)) { continue; }

            if (!after.TryGetValue(rel, out var afterBytes)) {
                problems.Add(context + ": Datei verschwunden: " + rel);
                continue;
            }

            if (!bytes.SequenceEqual(afterBytes)) {
                problems.Add(context + ": " + DiffText("Datei verändert: " + rel, bytes, afterBytes));
            }
        }

        foreach (var rel in after.Keys) {
            if (before.ContainsKey(rel)) { continue; }
            if (!IsRelevant(rel, mainFileRel, areaSuffixes, ignoreSuffixes, ignoreFolderMarkers)) { continue; }

            if (!HasSuffix(rel, areaSuffixes)) {
                problems.Add(context + ": Unerwartete neue Datei außerhalb der Datenbereiche: " + rel);
                continue;
            }

            var folder = FolderOf(rel);
            var beforePayload = NewestPayload(before, folder, areaSuffixes, ignoreSuffixes, ignoreFolderMarkers, stripFragmentHead);

            if (beforePayload is null) {
                problems.Add(context + ": Neuer Datenbereich ohne Vorher-Stand: " + folder);
                continue;
            }

            var afterPayload = NewestPayload(after, folder, areaSuffixes, ignoreSuffixes, ignoreFolderMarkers, stripFragmentHead);

            if (afterPayload is null) {
                problems.Add(context + ": Datenbereich ohne Nachher-Stand: " + folder);
                continue;
            }

            if (!beforePayload.SequenceEqual(afterPayload)) {
                problems.Add(context + ": " + DiffText("Datenbereich geändert: " + folder, beforePayload, afterPayload));
            }
        }
    }

    /// <summary>
    /// Baut die Diff-Analyse zweier Byte-Folgen: Längendifferenz, erste Abweichung mit Kontext und Anzahl unterschiedlicher Bytes.
    /// </summary>
    public static string DiffText(string header, byte[] original, byte[] roundtrip) {
        var sb = new System.Text.StringBuilder(header);
        sb.Append(" — Länge ").Append(original.Length.ToString1()).Append(" → ").Append(roundtrip.Length.ToString1())
            .Append(" (Differenz ").Append((roundtrip.Length - original.Length).ToString1()).Append(" Bytes)");

        var minLen = Math.Min(original.Length, roundtrip.Length);
        var firstDiff = -1;

        for (var i = 0; i < minLen; i++) {
            if (original[i] != roundtrip[i]) {
                firstDiff = i;
                break;
            }
        }

        if (firstDiff < 0) {
            sb.Append("; gemeinsame Bytes identisch, der Unterschied liegt nur in der Länge");
            return sb.ToString();
        }

        var diffCount = 0;

        for (var i = 0; i < minLen; i++) {
            if (original[i] != roundtrip[i]) { diffCount++; }
        }

        sb.Append("; erste Abweichung an Byte-Position ").Append(firstDiff.ToString1())
            .Append(", unterschiedliche Bytes im gemeinsamen Bereich: ").Append(diffCount.ToString1())
            .AppendLine()
            .Append("Original  : ").AppendLine(Printable(original, firstDiff))
            .Append("Roundtrip : ").AppendLine(Printable(roundtrip, firstDiff));

        return sb.ToString();
    }

    /// <summary>
    /// Setzt das Datum aller Zip-Einträge auf einen festen Wert, damit zwei Speicherungen desselben Inhalts byte-identisch verglichen werden können.
    /// </summary>
    public static byte[]? NormalizeZipDate(byte[]? zipped) {
        if (zipped is not { Length: > 0 }) { return null; }
        if (!zipped.IsZipped()) { return zipped; }

        using var ms = new MemoryStream();

        try {
            ms.Write(zipped, 0, zipped.Length);
            ms.Position = 0;

            using var zip = new ZipArchive(ms, ZipArchiveMode.Update, true);
            var entry = zip.GetEntry("Main.bin");

            entry?.LastWriteTime = FixedZipDate;
        } catch {
            return null;
        }

        return ms.ToArray();
    }

    /// <summary>
    /// Liefert den Nutzdaten-Inhalt: Zip-Dateien werden entpackt, alles andere unverändert geliefert.
    /// </summary>
    public static byte[] PayloadOf(byte[] raw) {
        if (!raw.IsZipped()) { return raw; }

        return raw.UnzipIt() ?? [];
    }

    /// <summary>
    /// Entfernt die System-Kopf-Sätze (Erzeuger- und Speicher-Stempel) vom Anfang eines Fragment-Inhalts;
    /// diese Sätze tragen den Zustand des jeweiligen Speichervorgangs und sind wie das Zip-Datum nicht inhaltsbestimmend.
    /// </summary>
    public static byte[] WithoutHeadRecords(byte[] payload) {
        using var ms = new MemoryStream(payload);
        var start = ms.Position;

        while (FragmentRecord.TryReadNext(ms) is { } record) {
            if (!record.IsFileTag) { break; }

            start = ms.Position;
        }

        return payload[(int)start..];
    }

    /// <summary>
    /// Eine Datei ist relevant, wenn sie die Hauptdatei oder ein Datenbereich ist und nicht ignoriert wird.
    /// </summary>
    private static bool IsRelevant(string rel, string mainFileRel, string[] areaSuffixes, string[] ignoreSuffixes, string[] ignoreFolderMarkers) {
        if (string.Equals(rel, mainFileRel, StringComparison.OrdinalIgnoreCase)) { return true; }
        if (HasSuffix(rel, ignoreSuffixes)) { return false; }

        var folder = FolderOf(rel);

        foreach (var marker in ignoreFolderMarkers) {
            if (folder.Contains(marker, StringComparison.OrdinalIgnoreCase)) { return false; }
        }

        return HasSuffix(rel, areaSuffixes);
    }

    private static bool HasSuffix(string rel, string[] suffixes) {
        foreach (var suffix in suffixes) {
            if (rel.EndsWith("." + suffix, StringComparison.OrdinalIgnoreCase)) { return true; }
        }

        return false;
    }

    /// <summary>
    /// Liefert den unmittelbaren Eltern-Ordner des relativen Pfads inkl. abschließendem Backslash, bei Wurzel-Dateien leer.
    /// </summary>
    private static string FolderOf(string rel) {
        var idx = rel.LastIndexOf('\\');
        return idx < 0 ? string.Empty : rel[..(idx + 1)];
    }

    /// <summary>
    /// Liefert den entpackten (und optional vom Kopf befreiten) Inhalt der jüngsten relevanten Datei eines Ordners;
    /// die Dateinamen tragen vorne einen UTC-Zeitstempel, deshalb entspricht die lexikografische Ordnung der zeitlichen.
    /// </summary>
    private static byte[]? NewestPayload(Dictionary<string, byte[]> inventory, string folder, string[] areaSuffixes, string[] ignoreSuffixes, string[] ignoreFolderMarkers, bool stripFragmentHead) {
        string? newest = null;

        foreach (var rel in inventory.Keys) {
            if (!string.Equals(FolderOf(rel), folder, StringComparison.OrdinalIgnoreCase)) { continue; }
            if (!IsRelevant(rel, string.Empty, areaSuffixes, ignoreSuffixes, ignoreFolderMarkers)) { continue; }

            if (newest is null || string.Compare(rel, newest, StringComparison.Ordinal) > 0) { newest = rel; }
        }

        if (newest is null) { return null; }

        var payload = PayloadOf(inventory[newest]);
        return stripFragmentHead ? WithoutHeadRecords(payload) : payload;
    }

    private static string Printable(byte[] bytes, int position) {
        var start = Math.Max(0, position - 40);
        var end = Math.Min(bytes.Length, position + 40);
        var slice = bytes[start..end];

        return "ab Position " + start.ToString1() + ": " + Win1252.GetString(slice).EncodeControlChars();
    }

    #endregion
}
