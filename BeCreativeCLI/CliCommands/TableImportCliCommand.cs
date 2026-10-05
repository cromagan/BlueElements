// Licensed under MIT; see License.md for disclaimer, details, and extended user conditions.

using System.IO;

namespace BeCreativeCLI.CliCommands;

/// <summary>
/// Tabellen: Überschreibt Zellen bestehender Zeilen aus einer CSV-Datei; die erste Spalte ist immer der Zeilen-Key.
/// </summary>
public class TableImportCliCommand : CliCommand {

    #region Properties

    public override string Command => "table-import";
    public override List<string> Flags => ["noheader", "unescape", "dry-run"];

    public override string? HelpDetails =>
            "Überschreibt Zellen bestehender Zeilen. " +
            "Die erste CSV-Spalte ist immer der Zeilen-Key; mit Kopfzeile muss sie SYS_ROWKEY heißen. " +
            "Die Kopfzeile enthält die übrigen Spaltennamen (wie table-export); leere Zellen leeren die Spalte. " +
            "--noheader ordnet positionsgleich zu: erste Spalte Key, dann Speicherreihenfolge ohne Systemspalten. " +
            "--unescape ist das Gegenstück zu table-export --escape: Literal \\n wird zum Zeilenumbruch, \\\\ zum Backslash. " +
            "Nicht gefundene Zeilen-Keys, abgeschlossene Zellen (SYS_LOCKED) und nicht beschreibbare Spalten " +
            "brechen vorab ab. Benötigt wird das CLI-Recht 'Change cell values' (für SYS_ROWSORTINDEX: 'Move rows'). " +
            "Beispiel: bcr table-export Karten.tblh --withrowkey --no-system-columns > x.csv — danach bcr table-import Karten.tblh x.csv";

    public override List<string> Options => ["sep", "password"];
    public override string Syntax => "bcr table-import <tabelle> <datei> [--sep <trennzeichen>] [--noheader] [--unescape] [--dry-run] [--password <kennwort>]";

    #endregion

    #region Methods

    public override int DoIt(CliArgs args) {
        if (args.PositionalCount != 2) {
            return UsageError($"Erwartet werden 2 Positionsargumente (<tabelle> <datei>), erhalten: {args.PositionalCount}.");
        }

        var separator = ';';

        if (args.Option("sep") is { Length: > 0 } sep) {
            if (sep.Length != 1) { return UsageError("--sep erwartet genau ein Trennzeichen, erhalten: '" + sep + "'."); }

            separator = sep[0];
        }

        var importFile = args[1] ?? string.Empty;

        // Relative Pfade auflösen — IO.FileExists verlangt gültige (absolute) Pfade.
        if (!importFile.IsValidFilepathAndName()) {
            try {
                importFile = Path.GetFullPath(importFile);
            } catch {
                Console.Error.WriteLine("Datei nicht gefunden: " + importFile);
                return 1;
            }
        }

        if (!FileExists(importFile)) {
            Console.Error.WriteLine("Datei nicht gefunden: " + importFile);
            return 1;
        }

        var records = CsvHelper.SplitCsvRecords(ReadAllText(importFile));

        if (records.Count == 0 || records.TrueForAll(string.IsNullOrEmpty)) {
            Console.Error.WriteLine("Die CSV-Datei ist leer oder nicht lesbar: " + importFile);
            return 1;
        }

        var tbl = LoadTable(args);

        if (tbl is null) { return 1; }

        var noheader = args.Flag("noheader");
        var (mapping, mappingError) = ResolveMapping(tbl, noheader, records[0], separator);

        if (mappingError is not null) {
            Console.Error.WriteLine(mappingError);
            return 1;
        }

        // Die CLI vergleicht ausschließlich die CLI-Rechte der Tabelle.
        // SYS_ROWSORTINDEX hält die Sortiernummern lückenlos (Nummern werden verschoben) — dafür ist das eigene Recht 'Move rows' nötig.
        var neededRights = new List<string> { CliRights.ChangeCellValues };

        if (mapping.Exists(m => tbl.Column.SysRowSortIndex == m)) { neededRights.Add(CliRights.MoveRows); }

        foreach (var right in neededRights) {
            var rightProblem = RightProblem(tbl, right);

            if (rightProblem is not null) {
                Console.Error.WriteLine(rightProblem);
                return 1;
            }
        }

        foreach (var column in mapping) {
            var columnProblem = ColumnWriteProblem(tbl, column);

            if (columnProblem is not null) {
                Console.Error.WriteLine(columnProblem);
                return 1;
            }
        }

        var (rows, parseError) = ParseRecords(records, noheader ? 0 : 1, mapping.Count + 1, separator);

        if (parseError is not null) {
            Console.Error.WriteLine(parseError);
            return 1;
        }

        // Alle Zeilen und Werte vorab vollständig auflösen und prüfen,
        // bevor irgendetwas geschrieben wird.
        var unescape = args.Flag("unescape");
        var imports = new List<(RowItem Row, List<(ColumnItem Column, string Value)> Cells)>();
        var seenKeys = new List<string>();

        foreach (var fields in rows) {
            var key = (fields[0] ?? string.Empty).Trim();

            if (!key.IsLong()) {
                Console.Error.WriteLine($"Zeilen-Key muss numerisch sein (Zeilen-Keys sind Zeitstempel-artige Longs): {key}");
                return 1;
            }

            if (seenKeys.Contains(key)) {
                Console.Error.WriteLine("Zeilen-Key doppelt in der CSV: " + key);
                return 1;
            }

            seenKeys.Add(key);

            var row = tbl.Row.GetByKey(key);

            if (row is null) {
                Console.Error.WriteLine("Zeile nicht gefunden: " + key);
                return 1;
            }

            List<(ColumnItem Column, string Value)> cells = [];

            for (var i = 0; i < mapping.Count; i++) {
                var value = fields[i + 1] ?? string.Empty;

                if (unescape) { value = UnescapeText(value); }

                if (IsLocked(mapping[i], row)) {
                    Console.Error.WriteLine($"Zeile {key}: Spalte '{mapping[i].KeyName}' ist abgeschlossen.");
                    return 1;
                }

                // Wert in das Speicherformat der Spalte überführen (z. B. HTML-Entities).
                cells.Add((mapping[i], StorageTextOf(mapping[i], value)));
            }

            imports.Add((row, cells));
        }

        if (imports.Count == 0) {
            Console.Error.WriteLine("Keine Datenzeilen in der CSV-Datei gefunden.");
            return 1;
        }

        // Ein Trockenlauf schreibt nichts und braucht daher den Fragment-Writer nicht.
        if (args.Flag("dry-run")) {
            Console.Out.WriteLine("Trockenlauf — importiert würde in: " + string.Join(", ", imports.Select(r => r.Row.KeyName)));
            Console.Out.WriteLine($"{imports.Count.ToString1()} Zeile(n), nichts gespeichert.");
            return 0;
        }

        // Erst nach allen Prüfungen den Fragment-Writer öffnen: Früh gescheiterte
        // Aufrufe sollen keine leere Fortsetzung mit EOF an die Fragment-Datei hängen.
        var fragmentProblem = FragmentEditProblem(tbl);

        if (fragmentProblem is not null) {
            Console.Error.WriteLine(fragmentProblem);
            return 2;
        }

        foreach (var (row, cells) in imports) {
            foreach (var (column, value) in cells) {
                var failed = row.CellSet(column, value, "bcr table-import");

                if (!string.IsNullOrEmpty(failed)) {
                    // Ohne Speichern abbrechen; bereits gesetzte Zellen bleiben nur im Speicher, die Datei bleibt unverändert.
                    Console.Error.WriteLine($"Zeile {row.KeyName}, Spalte {column.KeyName} konnte nicht gesetzt werden: {failed}");
                    return 1;
                }
            }

            Console.Out.WriteLine($"Zeile {row.KeyName} übernommen ({cells.Count} Zelle(n)).");
        }

        if (SaveTable(tbl) != 0) { return 1; }

        Console.Out.WriteLine(imports.Count.ToString1() + " Zeile(n) importiert.");
        return 0;
    }

    /// <summary>
    /// Löst die Spaltenzuordnung auf: Mit Kopfzeile muss die erste Kopfzelle SYS_ROWKEY heißen,
    /// die übrigen Namen werden zu Spalten aufgelöst; mit --noheader ordnet die Speicherreihenfolge
    /// ohne Systemspalten positionsgleich zu.
    /// </summary>
    private static (List<ColumnItem> Mapping, string? Error) ResolveMapping(Table tbl, bool noheader, string headerRecord, char separator) {
        List<ColumnItem> mapping = [];

        if (noheader) {
            mapping.AddRange(tbl.ColumnsInSaveOrder().Where(c => c.SaveContent && !c.IsSystemColumn()));

            if (mapping.Count == 0) { return ([], "Die Tabelle hat keine importierbaren Spalten (Speicherreihenfolge ohne Systemspalten)."); }

            return (mapping, null);
        }

        var names = CsvHelper.ParseCSVLine(headerRecord, separator).ToList();
        var keyName = (names.Count > 0 ? names[0] : string.Empty) ?? string.Empty;

        if (!keyName.Trim().Equals(SystemColumnKeys.RowKey, StringComparison.OrdinalIgnoreCase)) {
            return ([], $"Die erste Kopfzeilen-Spalte muss '{SystemColumnKeys.RowKey}' heißen (Zeilen-Key ist Pflicht), erhalten: '{keyName.Trim()}'");
        }

        foreach (var name in names.Skip(1)) {
            var key = (name ?? string.Empty).Trim();

            if (key.Length == 0) { return ([], "Die Kopfzeile enthält einen leeren Spaltennamen."); }

            var column = tbl.Column[key];

            if (column is not { IsDisposed: false }) { return ([], "Spalte nicht gefunden: " + key); }

            if (column.IsSystemColumn()) { return ([], $"Systemspalten außer '{SystemColumnKeys.RowKey}' werden nicht importiert: {key}"); }

            if (mapping.Contains(column)) { return ([], "Spalte doppelt in der Kopfzeile: " + key); }

            mapping.Add(column);
        }

        if (mapping.Count == 0) { return ([], "Die Kopfzeile enthält außer dem Zeilen-Key keine Spalten."); }

        return (mapping, null);
    }

    /// <summary>
    /// Liefert die Felder aller Datenzeilen: Kopfzeile überspringen (skipFirst), Leerzeilen
    /// überspringen, Feldzahl gegen Zeilen-Key + Spaltenzuordnung prüfen.
    /// </summary>
    private static (List<List<string>> Rows, string? Error) ParseRecords(List<string> records, int skipFirst, int fieldCount, char separator) {
        List<List<string>> rows = [];

        foreach (var record in records.Skip(skipFirst)) {
            var fields = CsvHelper.ParseCSVLine(record, separator).ToList();

            if (fields.Count == 1 && (fields[0] ?? string.Empty).Trim().Length == 0) { continue; } // Leerzeile.

            if (fields.Count != fieldCount) {
                return ([], $"Zeile hat {fields.Count} Felder, erwartet werden {fieldCount} (Zeilen-Key + Spalten): {record}");
            }

            rows.Add(fields);
        }

        return (rows, null);
    }

    /// <summary>
    /// Gegenstück zu table-export --escape: Literal \n wird zum Zeilenumbruch, \\ zum einzelnen
    /// Backslash — als Ein-Pass-Scanner, damit Folgen wie \\n eindeutig rückführbar bleiben.
    /// </summary>
    private static string UnescapeText(string text) {
        var sb = new StringBuilder(text.Length);
        var i = 0;

        while (i < text.Length) {
            if (text[i] != '\\' || i + 1 >= text.Length) {
                sb.Append(text[i]);
                i++;
                continue;
            }

            switch (text[i + 1]) {
                case '\\':
                    sb.Append('\\');
                    i += 2;
                    break;

                case 'n':
                    sb.Append("\r\n");
                    i += 2;
                    break;

                default:
                    sb.Append(text[i]);
                    i++;
                    break;
            }
        }

        return sb.ToString();
    }

    #endregion
}
