// Licensed under MIT; see License.md for disclaimer, details, and extended user conditions.

using System.Globalization;
using System.IO;
using static BlueBasics.ClassesStatic.IO;

namespace BlueTable.ClassesStatic;

/// <summary>
/// Konvertiert Legacy-Tabellen (.bdb/.nbdb/.mbdb/.tblh) in das neue Tabellen-Format (.btb).
/// Fortsetzbar: bereits vorhandene Zieldateien werden übersprungen, sodass mehrere Läufe
/// und mehrere Rechner sich gegenseitig ergänzen können.
/// </summary>
public static class TableFileConverter {

    #region Fields

    /// <summary>
    /// Suffix der Chunk-Dateien im neuen Format.
    /// </summary>
    private const string SuffixOfChunks = "chk";

    /// <summary>
    /// Suffix der Legacy-Row-Chunk-Dateien.
    /// </summary>
    private const string SuffixOfLegacyChunks = "tblc";

    /// <summary>
    /// Suffixe der Legacy-Tabellen-Formate.
    /// </summary>
    public static readonly string[] LegacySuffixes = ["bdb", "nbdb", "mbdb", "tblh"];

    #endregion

    #region Methods

    /// <summary>
    /// Konvertiert eine Legacy-Tabelle: zuerst die Kopf-Datei, bei .tblh danach die
    /// Row-Chunks einzeln nachziehen. Vorhandene Zieldateien werden übersprungen.
    /// Liefert einen Ergebnis-Text.
    /// </summary>
    /// <param name="headPath">Vollständiger Pfad der Legacy-Tabellen-Datei.</param>
    /// <param name="cancelRequested">Wird zwischen den Arbeitsschritten abgefragt; true bricht ab.</param>
    /// <param name="log">Nimmt Zeilen für das Fortschritts-Protokoll entgegen.</param>
    public static string ConvertTable(string headPath, Func<bool>? cancelRequested, Action<string>? log) {
        var suffix = headPath.FileSuffix().ToLowerInvariant();

        try {
            var headResult = ConvertHead(headPath, suffix);

            if (cancelRequested?.Invoke() is true) { return headResult + " — abgebrochen, Chunks nicht nachgezogen."; }

            var (converted, skipped) = suffix == "tblh" ? ConvertRowChunks(headPath, cancelRequested, log) : (0, 0);

            return suffix == "tblh"
                ? headResult + " — Chunks konvertiert: " + converted.ToString1() + ", übersprungen: " + skipped.ToString1()
                : headResult;
        } catch (Exception ex) {
            return "Fehler — " + ex.Message;
        }
    }

    /// <summary>
    /// Sucht rekursiv alle Legacy-Tabellen unter dem Verzeichnis.
    /// </summary>
    public static List<string> FindLegacyTables(string rootDirectory) => [.. GetFiles(rootDirectory, "*", SearchOption.AllDirectories)
        .Where(f => LegacySuffixes.Contains(f.FileSuffix(), StringComparer.OrdinalIgnoreCase))
        .OrderBy(f => f, StringComparer.OrdinalIgnoreCase)];

    /// <summary>
    /// Konvertiert die neueste .tblc jedes Row-Chunk-Ordners einzeln in das neue Format.
    /// Liefert (konvertiert, übersprungen).
    /// </summary>
    private static (int Converted, int Skipped) ConvertRowChunks(string headPath, Func<bool>? cancelRequested, Action<string>? log) {
        var workFolder = ChunkWorkFolder(headPath);

        if (!DirectoryExists(workFolder)) { return (0, 0); }

        var converted = 0;
        var skipped = 0;

        foreach (var sub in GetDirectories(workFolder).OrderBy(f => f, StringComparer.OrdinalIgnoreCase)) {
            if (cancelRequested?.Invoke() is true) { break; }

            var chunkId = sub.TrimEnd('\\').FileNameWithSuffix().ToLowerInvariant();
            if (!Chunk.IsRowChunk(chunkId)) { continue; }

            var newest = TblcFilesNewestFirst(sub).FirstOrDefault();

            if (newest is null) { continue; }

            var target = PathWithSuffix(newest, SuffixOfChunks);

            // Neuere .chk-Dateien (z. B. leere Aufräum-Dateien eines früheren Speichervorgangs)
            // werden als .chk.old beiseitegelegt, damit die konvertierte Datei beim Laden gewinnt.
            var retired = RetireNewerChkFiles(sub, DateOfFileName(newest), target.FileNameWithSuffix());

            if (retired > 0) { log?.Invoke("    " + retired.ToString1() + " neuere .chk als .old beiseitegelegt: " + chunkId); }

            if (FileExists(target)) {
                skipped++;
                continue;
            }

            var error = ConvertOneChunk(headPath, newest, target);

            if (error is null) {
                converted++;
                log?.Invoke("    Chunk konvertiert: " + newest.FileNameWithSuffix());
            } else {
                log?.Invoke("    Chunk fehlgeschlagen: " + newest.FileNameWithSuffix() + " — " + error);
            }
        }

        return (converted, skipped);
    }

    /// <summary>
    /// Konvertiert eine einzelne .tblc-Datei: Legacy-Kopf frisch laden, nur die Zeilen des
    /// Chunks parsen und direkt an der Zieltabelle speichern — die erzeugte .chk-Datei liegt
    /// sofort im richtigen Chunk-Ordner und wird auf den Namen der Quell-.tblc umbenannt.
    /// Liefert null bei Erfolg, sonst die Fehlermeldung.
    /// </summary>
    private static string? ConvertOneChunk(string headPath, string tblcFile, string target) {
        var legacy = new TableChunk(headPath.FileNameWithoutSuffix());
        TableFileNew? newTable = null;
        SaveableFile? saveable = null;

        try {
            legacy.LoadFromFile(headPath, string.Empty);

            if (legacy.IsFreezed) { return "Legacy-Kopf nicht ladbar: " + legacy.FreezedReason; }

            // Nur die Zeilen des Chunks: beim Laden mitgekommene Zeilen (z. B. aus _rowdata) entfernen.
            _ = legacy.Row.Clear();

            if (ReadAllBytes(tblcFile, 5).Value is not byte[] raw || raw.Length == 0) { return "Datei nicht lesbar."; }

            var content = raw.IsZipped() ? raw.UnzipIt() ?? [] : raw;

            if (!TableFile.HasValidEofMarker(content)) { return "Unvollständig (kein EOF-Marker)."; }

            var parsedRows = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            if (!legacy.Parse(content, false, parsedRows)) { return "Parsen fehlgeschlagen."; }

            // Direkt an der Zieltabelle speichern: die Verwaltung schreibt die .chk-Datei
            // selbst in den richtigen Chunk-Ordner.
            var finalHead = PathWithSuffix(headPath, TableFileNew.SuffixMainFile);
            var saveStartUtc = DateTime.UtcNow.AddSeconds(-1);

            saveable = new SaveableFile(finalHead);

            newTable = new TableFileNew(finalHead, legacy, saveable);
            saveable.SaveRequired = true;
            saveable.WriteHeadFile();
            var opr = newTable.Save();
            saveable.WaitDiskOperationFinished();

            if (opr.IsFailed) { return "Speichern fehlgeschlagen: " + opr.FailedReason; }
            if (saveable.IsFreezed) { return "eingefroren: " + saveable.FrozenReason; }
            if (saveable.SaveRequired) { return "Speichern unvollständig."; }

            return RenameGeneratedChunks(target, saveStartUtc);
        } finally {
            newTable?.Dispose();
            if (newTable is null) { saveable?.Dispose(); }
            legacy.Dispose();
        }
    }

    /// <summary>
    /// Konvertiert die Kopf-Datei der Legacy-Tabelle: Legacy laden und direkt an der
    /// Ziel-.btb speichern. Fehlende Chunks bleiben bewusst außerhalb — sie werden beim
    /// Chunk-Lauf nachgezogen. Liefert einen Ergebnis-Text.
    /// </summary>
    private static string ConvertHead(string headPath, string suffix) {
        var target = headPath.FilePath() + headPath.FileNameWithoutSuffix() + "." + TableFileNew.SuffixMainFile;

        if (FileExists(target)) { return "Kopf übersprungen — Ziel existiert bereits."; }

        TableFile? legacy = null;
        TableFileNew? newTable = null;
        SaveableFile? saveable = null;

        try {
            switch (suffix) {
                case "tblh":
                    legacy = new TableChunk(headPath.FileNameWithoutSuffix());
                    break;
                case "mbdb":
                    legacy = new TableFragments(headPath.FileNameWithoutSuffix());
                    break;
                default:
                    legacy = new TableFile(headPath.FileNameWithoutSuffix());
                    break;
            }

            legacy.LoadFromFile(headPath, string.Empty);

            if (legacy.IsFreezed) { return "Fehler — Laden fehlgeschlagen: " + legacy.FreezedReason; }

            // Bei .tblh sind die Chunk-Ordner autoritativ — geladene Zeilen dieser Ordner
            // (z. B. aus _rowdata) würden dort doppelt stehen.
            if (legacy is TableChunk tc) { RemoveRowsOfConvertedFolders(tc, headPath); }

            saveable = new SaveableFile(target);

            newTable = new TableFileNew(target, legacy, saveable);
            saveable.SaveRequired = true;
            saveable.WriteHeadFile();
            var opr = newTable.Save();
            saveable.WaitDiskOperationFinished();

            if (opr.IsFailed) { return "Fehler — Speichern fehlgeschlagen: " + opr.FailedReason; }
            if (saveable.IsFreezed) { return "Fehler — eingefroren: " + saveable.FrozenReason; }
            if (saveable.SaveRequired) { return "Fehler — Speichern unvollständig."; }

            return "OK — Kopf konvertiert.";
        } finally {
            newTable?.Dispose();
            if (newTable is null) { saveable?.Dispose(); }
            legacy?.Dispose();
        }
    }

    /// <summary>
    /// Legt .chk-Dateien des Ordners, die jünger sind als die .tblc, als .chk.old ab —
    /// erkennbar am Zeitstempel im Dateinamen. Nur so gewinnt die konvertierte Datei
    /// beim Laden. Die Zieldatei selbst bleibt unberührt. Liefert die Anzahl.
    /// </summary>
    private static int RetireNewerChkFiles(string folder, DateTime tblcDate, string targetFileName) {
        var retired = 0;

        foreach (var f in GetFiles(folder)) {
            if (!string.Equals(f.FileSuffix(), SuffixOfChunks, StringComparison.OrdinalIgnoreCase)) { continue; }
            if (string.Equals(f.FileNameWithSuffix(), targetFileName, StringComparison.OrdinalIgnoreCase)) { continue; }
            if (DateOfFileName(f) <= tblcDate) { continue; }

            if (MoveFile(f, f + ".old", false)) { retired++; }
        }

        return retired;
    }

    /// <summary>
    /// Benennt die beim Speichern erzeugte .chk-Datei im Ziel-Ordner auf den Namen der
    /// Quell-.tblc um. Liefert null bei Erfolg, sonst die Fehlermeldung.
    /// </summary>
    private static string? RenameGeneratedChunks(string target, DateTime saveStartUtc) {
        var found = false;

        foreach (var f in GetFiles(target.FilePath())) {
            if (!string.Equals(f.FileSuffix(), SuffixOfChunks, StringComparison.OrdinalIgnoreCase)) { continue; }
            if (DateOfFileName(f) < saveStartUtc) { continue; }

            if (string.Equals(f.FileNameWithSuffix(), target.FileNameWithSuffix(), StringComparison.OrdinalIgnoreCase)) {
                found = true;
                continue;
            }

            if (MoveFile(f, target, false) || FileExists(target)) { found = true; }
        }

        return found ? null : "Es wurde keine Chunk-Datei erzeugt.";
    }

    /// <summary>
    /// Entfernt geladene Zeilen, deren Chunk-Id auf einen bereits vorhandenen Row-Chunk-Ordner
    /// trifft — deren Inhalt liegt in den dortigen .chk-Dateien.
    /// </summary>
    private static void RemoveRowsOfConvertedFolders(TableChunk legacy, string headPath) {
        var workFolder = ChunkWorkFolder(headPath);

        if (!DirectoryExists(workFolder)) { return; }

        var chunkType = legacy.Column.ChunkValueColumn?.Value_for_Chunk ?? ChunkType.None;

        List<string> remove = [];

        foreach (var row in legacy.Row) {
            var chunkId = TableChunk.ChunkValueToId(chunkType, row.ChunkValue);

            if (DirectoryExists(workFolder + chunkId + "\\")) { remove.Add(row.KeyName); }
        }

        foreach (var key in remove) { _ = legacy.Row.Remove(key); }
    }

    /// <summary>
    /// Listet die .tblc-Dateien des Ordners nach Datum im Dateinamen auf (neueste zuerst).
    /// </summary>
    private static List<string> TblcFilesNewestFirst(string folder) => [.. GetFiles(folder)
        .Where(f => string.Equals(f.FileSuffix(), SuffixOfLegacyChunks, StringComparison.OrdinalIgnoreCase))
        .OrderByDescending(DateOfFileName)];

    /// <summary>
    /// UTC-Datum aus einem Chunk-Dateinamen (yyyy-MM-dd-HH-mm-ss-fff_...);
    /// DateTime.MinValue bei ungültigem Namen.
    /// </summary>
    private static DateTime DateOfFileName(string filePath) {
        var fileName = filePath.FileNameWithoutSuffix();

        if (fileName.Length < 23) { return DateTime.MinValue; }

        return DateTime.TryParseExact(fileName[..23], "yyyy-MM-dd-HH-mm-ss-fff", CultureInfo.InvariantCulture, DateTimeStyles.None, out var result)
            ? DateTime.SpecifyKind(result, DateTimeKind.Utc)
            : DateTime.MinValue;
    }

    /// <summary>
    /// Arbeitsordner einer Tabelle: [Verzeichnis]\[Name]\
    /// </summary>
    private static string ChunkWorkFolder(string headPath) => headPath.FilePath() + headPath.FileNameWithoutSuffix() + "\\";

    /// <summary>
    /// Ersetzt das Suffix des Pfades; Verzeichnis und Basisname bleiben unverändert.
    /// </summary>
    private static string PathWithSuffix(string path, string suffix) => path.FilePath() + path.FileNameWithoutSuffix() + "." + suffix;

    #endregion
}
