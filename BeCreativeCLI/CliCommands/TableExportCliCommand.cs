// Licensed under MIT; see License.md for disclaimer, details, and extended user conditions.

namespace BeCreativeCLI.CliCommands;

/// <summary>
/// Tabellen: Exportiert die Tabelle oder die adressierten Zeilen als CSV auf die Standardausgabe.
/// </summary>
public class TableExportCliCommand : CliCommand {

    #region Properties

    public override string Command => "table-export";
    public override List<string> Flags => ["no-system-columns", "decode", "withrowkey", "escape"];
    public override List<string> Options => [.. AddressingOptions, "sep", "columns", "password"];
    public override string Syntax => "bcr table-export <tabelle> [--sep <trennzeichen>] [--no-system-columns] [--withrowkey] [--columns <spalten>] [--decode] [--escape] [+ optionale Zeilenadressierung] [--password <kennwort>]";

    public override string? HelpDetails =>
            "--no-system-columns lässt Systemspalten (z. B. SYS_ROWSORTINDEX) weg — auch aus einer --columns-Auswahl. " +
            "--columns wählt Komma-getrennte Spalten in genau der angegebenen Reihenfolge (auch Systemspalten). " +
            "--withrowkey stellt den Zeilen-Key als erste CSV-Spalte (SYS_ROWKEY) voran; ist SYS_ROWKEY zusätzlich in --columns genannt, wird sie dort entfernt, damit der Key nicht doppelt erscheint. " +
            "--decode gibt echte Umlaute statt HTML-Entities aus — gut für Diffs und Reviews; zum Weiterverarbeiten mit bcr-Befehlen besser ohne, da die App Entities speichert. " +
            "Standard: Mehrzeilige Zellen werden RFC-4180-konform gequotet (Anführungszeichen, CRLF-Zeilenumbrüche). " +
            "--escape ersetzt echte Zeilenumbrüche in Zellen durch Literal \\n (Backslashes werden zu \\\\ verdoppelt) — der Output bleibt zeilenstabil und ist Zeile für Zeile in Shell-Schleifen verarbeitbar. " +
            "Das Gegenstück --unescape ist bewusst nicht implementiert: Die Rückwandlung (\\\\ → \\, \\n → Zeilenumbruch) gehört in den Import, z. B. vor table-cellset. " +
            "Mit Zeilenadressierung (--rowkey oder --filtercolumn/--filtervalue) wird nur die adressierte Auswahl exportiert.";

    #endregion

    #region Methods

    public override int DoIt(CliArgs args) {
        if (args.PositionalCount != 1) {
            return UsageError($"Erwartet wird genau 1 Positionsargument (<tabelle>), erhalten: {args.PositionalCount}.");
        }

        if (args.HasOption("out")) {
            return UsageError("--out wird nicht unterstützt. Die Ausgabe erfolgt ausschließlich auf stdout, z. B. mit einer Umleitung in eine Datei.");
        }

        var separator = ';';

        if (args.Option("sep") is { Length: > 0 } sep) {
            if (sep.Length != 1) { return UsageError("--sep erwartet genau ein Trennzeichen, erhalten: '" + sep + "'."); }

            separator = sep[0];
        }

        // Zeilenadressierung ist optional; nur eine Teilangabe ist ein Fehler.
        if (args.HasOption("rowkey") || args.HasOption("filtercolumn") || args.HasOption("filtervalue") || args.HasOption("filtertype")) {
            var problem = RowAddressingProblem(args);

            if (problem is not null) {
                Console.Error.WriteLine(problem);
                return 2;
            }
        }

        var tbl = LoadTable(args);

        if (tbl is null) { return 1; }

        List<ColumnItem> columns;

        if (args.HasOption("columns")) {
            var (selected, columnError) = ResolveColumns(tbl, args.Option("columns") ?? string.Empty);

            if (columnError is not null) {
                Console.Error.WriteLine(columnError);
                return 1;
            }

            columns = selected;
        } else {
            columns = tbl.ColumnsInSaveOrder().Where(c => c.SaveContent).ToList();
        }

        if (args.Flag("no-system-columns")) {
            columns = [.. columns.Where(c => !c.IsSystemColumn())];

            if (columns.Count == 0) {
                Console.Error.WriteLine("Nach --no-system-columns bleibt keine Spalte übrig.");
                return 1;
            }
        }

        var withRowKey = args.Flag("withrowkey");

        if (withRowKey) {
            // Der Key steht vorne; die Systemspalte wäre doppelt in der Ausgabe.
            columns.RemoveAll(c => tbl.Column.SysRowKey == c);
        }

        List<RowItem> rows;

        if (args.HasOption("rowkey") || args.HasOption("filtercolumn") || args.HasOption("filtervalue")) {
            var (resolved, error) = ResolveRows(tbl, args);

            if (error is not null) {
                Console.Error.WriteLine(error);
                return 1;
            }

            rows = resolved;
        } else {
            rows = [.. tbl.RowsInSaveOrder()];
        }

        var addressed = args.HasOption("rowkey") || args.HasOption("filtercolumn") || args.HasOption("filtervalue");

        if (addressed && rows.Count == 0) {
            Console.Error.WriteLine("Keine Zeile getroffen.");
            return 1;
        }

        Console.Out.Write(BuildCsv(columns, rows, separator, args.Flag("decode"), withRowKey, args.Flag("escape")));
        return 0;
    }

    /// <summary>
    /// Baut das CSV nach den Regeln des Standardexports (RFC-4180-Escaping, CRLF), plus
    /// Spalten-/Zeilenauswahl, optionalem Zeilen-Key als erste Spalte, optionaler
    /// Entity-Dekodierung formatierter Spalten und optionaler \\n-Escapung der Zellen.
    /// </summary>
    private static string BuildCsv(List<ColumnItem> columns, List<RowItem> rows, char separator, bool decode, bool withRowKey, bool escape) {
        var sb = new StringBuilder();

        if (withRowKey || columns.Count > 0) {
            List<string> names = [];

            if (withRowKey) { names.Add(SystemColumnKeys.RowKey); }

            names.AddRange(columns.Select(c => c.KeyName));
            sb.AppendJoin(separator, names.Select(n => CsvHelper.EscapeCSVField(n, separator))).AppendLine();
        }

        foreach (var row in rows) {
            List<string> fields = [];

            if (withRowKey) { fields.Add(CsvHelper.EscapeCSVField(row.KeyName, separator)); }

            fields.AddRange(columns.Select(c => CsvHelper.EscapeCSVField(CellTextOf(row, c, decode, escape), separator)));
            sb.AppendJoin(separator, fields).AppendLine();
        }

        return sb.ToString();
    }

    #endregion
}
