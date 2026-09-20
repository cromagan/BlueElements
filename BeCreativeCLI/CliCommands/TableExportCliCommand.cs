// Licensed under MIT; see License.md for disclaimer, details, and extended user conditions.

namespace BeCreativeCLI.CliCommands;

/// <summary>
/// Tabellen: Exportiert die Tabelle als CSV auf die Standardausgabe; optional ohne Systemspalten, auf adressierte Zeilen begrenzt und mit dekodierten Sonderzeichen.
/// </summary>
public class TableExportCliCommand : CliCommand {

    #region Properties

    public override string Command => "table-export";
    public override List<string> Flags => ["noheader", "no-system-columns", "decode"];
    public override List<string> Options => [.. AddressingOptions, "sep", "password"];
    public override string Syntax => "bcr table-export <tabelle> [--sep <trennzeichen>] [--noheader] [--no-system-columns] [--decode] [+ optionale Zeilenadressierung]";

    public override string? HelpDetails =>
            "--no-system-columns lässt Systemspalten (z. B. SYS_ROWSORTINDEX) weg. " +
            "--decode gibt echte Umlaute statt HTML-Entities aus — gut für Diffs und Reviews; zum Weiterverarbeiten mit bcr-Befehlen besser ohne, da die App Entities speichert. " +
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

        var columns = tbl.ColumnsInSaveOrder().Where(c => c.SaveContent).ToList();

        if (args.Flag("no-system-columns")) {
            columns = [.. columns.Where(c => !c.IsSystemColumn())];
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

        Console.Out.Write(BuildCsv(columns, rows, separator, !args.Flag("noheader"), args.Flag("decode")));
        return 0;
    }

    /// <summary>
    /// Baut das CSV nach den Regeln des Standardexports (Escaping, CRLF), plus
    /// Spalten-/Zeilenauswahl und optionaler Entity-Dekodierung formatierter Spalten.
    /// </summary>
    private static string BuildCsv(List<ColumnItem> columns, List<RowItem> rows, char separator, bool header, bool decode) {
        var sb = new StringBuilder();

        if (header && columns.Count > 0) {
            sb.AppendJoin(separator, columns.Select(c => CsvHelper.EscapeCSVField(c.KeyName, separator))).AppendLine();
        }

        foreach (var row in rows) {
            var fields = columns.Select(c => CsvHelper.EscapeCSVField(CellTextOf(row, c, decode), separator));
            sb.AppendJoin(separator, fields).AppendLine();
        }

        return sb.ToString();
    }

    private static string CellTextOf(RowItem row, ColumnItem column, bool decode) {
        var value = row.CellGetString(column);

        if (decode) { value = System.Net.WebUtility.HtmlDecode(value); }

        return value;
    }

    #endregion
}
