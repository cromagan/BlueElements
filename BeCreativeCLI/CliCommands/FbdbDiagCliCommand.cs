// Licensed under MIT; see License.md for disclaimer, details, and extended user conditions.

using System.Globalization;
using System.IO;

namespace BeCreativeCLI.CliCommands;

/// <summary>
/// Intern: Lädt eine Tabelle und meldet Ladezustand, Freeze-Grund und
/// Editierbarkeit — zur Fehlersuche bei Ladeproblemen.
/// </summary>
public class FbdbDiagCliCommand : CliCommand {

    #region Properties

    public override string Command => "fbdb-diag";

    public override List<string> Flags => ["dev"];

    public override bool Hidden => true;

    public override List<string> Options => [];

    public override string Syntax => "bcr fbdb-diag <tabelle> [--dev]";

    public override string? HelpDetails => "Lädt die Tabelle und meldet Zustand und Freeze-Grund.";

    #endregion

    #region Methods

    public override int DoIt(CliArgs args) {
        if (args.PositionalCount != 1) { return UsageError("Erwartet wird genau 1 Positionsargument (<tabelle>)."); }

        var quelle = (args[0] ?? string.Empty).NormalizeFile();

        if (!FileExists(quelle)) {
            Console.Error.WriteLine("Datei nicht gefunden: " + quelle);
            return 1;
        }

        Console.Out.WriteLine("Datei: " + quelle + " (" + GetFileInfo(quelle)?.Length + " Bytes)");

        var tbl = Table.Get(quelle);

        if (tbl is not { IsDisposed: false } t) {
            Console.Out.WriteLine("Table.Get: FAILED (null oder disposed)");
            return 1;
        }

        Console.Out.WriteLine("Table.Get: OK");
        Console.Out.WriteLine("  Typ:              " + t.GetType().Name);
        Console.Out.WriteLine("  KeyName:          " + t.KeyName);
        Console.Out.WriteLine("  IsDisposed:       " + t.IsDisposed);
        Console.Out.WriteLine("  IsFreezed:        " + t.IsFreezed);
        Console.Out.WriteLine("  FreezedReason:    " + t.FreezedReason);
        Console.Out.WriteLine("  MainChunkLoadDone:" + t.MainChunkLoadDone);
        Console.Out.WriteLine("  Unlocked:         " + t.Unlocked);
        Console.Out.WriteLine("  Spalten:          " + t.Column.Count);
        Console.Out.WriteLine("  Zeilen:           " + t.Row.Count);
        Console.Out.WriteLine("  IsGenericEditable(false): '" + t.IsGenericEditable(false) + "'");
        Console.Out.WriteLine("  IsGenericEditable(true):  '" + t.IsGenericEditable(true) + "'");

        var upd = t.BeSureToBeUpToDate(false);
        Console.Out.WriteLine("  BeSureToBeUpToDate(false): " + upd);

        if (t is TableFragmentsNew tfn) {
            Console.Out.WriteLine("  FragmengtsPath:   " + tfn.FragmengtsPath());
            var frgms = GetFiles(tfn.FragmengtsPath(), "*.frg", SearchOption.TopDirectoryOnly);
            Console.Out.WriteLine("  Fragmentdateien:  " + frgms.Length);
        }

        t.Dispose();
        DumpMainFileRecords(quelle);
        SimulateReader(quelle);

        return upd ? 0 : 1;
    }

    /// <summary>
    /// Ruft Table.Deserialize direkt auf dem Hauptdatei-Inhalt auf und meldet
    /// die Bruchstelle (Stream-Position) mit den Folgesätzen als Kontext.
    /// </summary>
    private static void SimulateReader(string filename) {
        var chunk = Chunk.Get(filename);
        var data = chunk?.LoadContent();

        if (data is not { Length: > 0 } || data[0] != 0x09) { return; }

        var tableName = BlueBasics.Classes.Formats.SystemNameFormat.MakeValid(filename.FileNameWithoutSuffix().ToUpperInvariant());
        TableFragmentsNew tbl = new(tableName);

        using var ms = new MemoryStream(data);
        var ok = tbl.Deserialize(ms, 1);

        Console.Out.WriteLine("Deserialize-Ergebnis: " + (ok ? "OK" : "FEHLER"));
        Console.Out.WriteLine("Stream-Position bei Ende: " + ms.Position + " von " + data.Length);

        if (!ok) {
            // Sätze rund um die Bruchstelle anzeigen.
            var pointer = 0;
            var lastGood = 0;

            while (pointer < data.Length && pointer < ms.Position + 500) {
                if (!FragmentRecord.TryParse(data, pointer, out var rec, out var next) || rec is null || next <= pointer) { break; }

                if (next > ms.Position - 200 && next < ms.Position + 400) {
                    var content = rec.StringContent.Replace("\r", "\\r").Replace("\n", "\\n").Replace("\t", "\\t");

                    if (content.Length > 60) { content = content[..60] + "…"; }

                    Console.Out.WriteLine($"  Pos={pointer} Depth={rec.Depth} Leaf={rec.LeafType} Ctrl={rec.DataType} Meta={rec.HasMeta} Content='{content}'");
                }

                lastGood = pointer;
                pointer = next;
            }

            Console.Out.WriteLine("  (Letzter Satzbeginn vor Bruchstelle: " + lastGood + ")");
        }

        tbl.Dispose();
    }

    /// <summary>
    /// Liest den Hauptdatei-Inhalt und parst die Fragment-Sätze einzeln,
    /// bis ein Satz nicht mehr lesbar ist — meldet Position und Kontext.
    /// </summary>
    private static void DumpMainFileRecords(string filename) {
        var chunk = Chunk.Get(filename);

        if (chunk is null) {
            Console.Out.WriteLine("Chunk: null");
            return;
        }

        var data = chunk.LoadContent();

        if (data is not { Length: > 0 }) {
            Console.Out.WriteLine("Chunk.LoadContent: leer");
            return;
        }

        Console.Out.WriteLine("Chunk.LoadContent: " + data.Length + " Bytes, erstes Byte 0x" + data[0].ToString("X2", CultureInfo.InvariantCulture));

        if (data[0] != 0x09) {
            Console.Out.WriteLine("Kein Fragment-Format (Classic-Inhalt) — kein Satz-Dump.");
            return;
        }

        var pointer = 0;
        var count = 0;
        var showAll = false;

        while (pointer < data.Length) {
            if (!FragmentRecord.TryParse(data, pointer, out var record, out var next)) {
                var contextStart = Math.Max(0, pointer - 30);
                var contextLength = Math.Min(120, data.Length - contextStart);
                var sb = new StringBuilder();

                for (var i = contextStart; i < contextStart + contextLength; i++) {
                    var b = data[i];
                    _ = b is >= 0x20 and < 0x7F ? sb.Append((char)b) : sb.Append("\\x" + b.ToString("X2", CultureInfo.InvariantCulture));
                }

                Console.Out.WriteLine($"SATZ-ENDE UNGÜLTIG bei Position {pointer} (von {data.Length}):");
                Console.Out.WriteLine("  " + sb);
                return;
            }

            count++;

            if (record is not null && (record.Depth <= 1 || showAll || (count >= 805 && count <= 840))) {
                var content = record.StringContent.Replace("\r", "\\r").Replace("\n", "\\n").Replace("\t", "\\t");

                if (content.Length > 70) { content = content[..70] + "…"; }

                Console.Out.WriteLine($"#{count} Pos={pointer} Depth={record.Depth} Leaf={record.LeafType} Ctrl={record.DataType} Meta={record.HasMeta} Content='{content}'");
            }

            // Ab dem Variablen-Container (Blatt-Typ 94) alles ausgeben.
            if (record is not null && record.LeafType == 94) { showAll = true; }

            if (next <= pointer) {
                Console.Out.WriteLine("Pointer bewegt sich nicht bei Position " + pointer);
                return;
            }

            pointer = next;
        }

        // Letzte 25 Sätze gesondert ausgeben.
        var total = count;
        pointer = 0;
        count = 0;

        while (pointer < data.Length) {
            if (!FragmentRecord.TryParse(data, pointer, out var record, out var next) || next <= pointer) { break; }

            count++;

            if (count > total - 25 && record is not null) {
                var content = record.StringContent.Replace("\r", "\\r").Replace("\n", "\\n").Replace("\t", "\\t");

                if (content.Length > 70) { content = content[..70] + "…"; }

                Console.Out.WriteLine($"TAIL #{count}/{total} Pos={pointer} Depth={record.Depth} Leaf={record.LeafType} Ctrl={record.DataType} Meta={record.HasMeta} Content='{content}'");
            }

            pointer = next;
        }

        Console.Out.WriteLine("Alle " + count + " Sätze gelesen, Datei komplett (" + data.Length + " Bytes).");
    }

    #endregion
}
