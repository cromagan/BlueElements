// Licensed under MIT; see License.md for disclaimer, details, and extended user conditions.

namespace BeCreativeCLI.CliCommands;

/// <summary>
/// Tabellen: Zeigt alle Werte einer Zeile lesbar als SPALTENNAME: Wert; die Zeile muss eindeutig adressiert sein.
/// </summary>
public class TableRowGetCliCommand : CliCommand {

    #region Properties

    public override string Command => "table-rowget";

    public override List<string> Options => [.. AddressingOptions, "columns", "password"];

    public override string Syntax => "bcr table-rowget <tabelle> + Zeilenadressierung (--rowkey <key> oder --filtercolumn <spalte> --filtervalue <wert> [--filtertype <typ>]) [--columns <spalten>] [--password <kennwort>]";

    public override string? HelpDetails =>
            "Gibt je Spalte eine Zeile 'SPALTENNAME: Wert' aus — dekodiert (echte Umlaute statt HTML-Entities), mehrzeilige Zellen mit echten Zeilenumbrüchen. " +
            "Ohne --columns erscheinen alle gespeicherten Spalten ohne Systemspalten; --columns wählt Komma-getrennt eigene Spalten in der angegebenen Reihenfolge (auch Systemspalten). " +
            "Die Adressierung muss genau eine Zeile liefern; zum Weiterverarbeiten von Einzelwerten dient table-cellget, zum tab-getrennten Lesen mehrerer Zeilen table-info --rowvalues.";

    #endregion

    #region Methods

    public override int DoIt(CliArgs args) {
        if (args.PositionalCount != 1) {
            return UsageError($"Erwartet wird genau 1 Positionsargument (<tabelle>), erhalten: {args.PositionalCount}.");
        }

        var problem = RowAddressingProblem(args);

        if (problem is not null) {
            Console.Error.WriteLine(problem);
            return 2;
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
            columns = tbl.ColumnsInSaveOrder().Where(c => c.SaveContent && !c.IsSystemColumn()).ToList();
        }

        var (rows, error) = ResolveRows(tbl, args);

        if (error is not null) {
            Console.Error.WriteLine(error);
            return 1;
        }

        if (rows.Count != 1) {
            Console.Error.WriteLine($"Die Adressierung lieferte {rows.Count} Zeilen, erwartet wurde genau eine.");
            return 1;
        }

        foreach (var column in columns) {
            Console.Out.WriteLine(column.KeyName + ": " + ReadableCellTextOf(rows[0], column));
        }

        return 0;
    }

    /// <summary>
    /// Liefert den Zelltext lesbar: Entities dekodiert, HTML-Zeilentrenner und
    /// Zeilentrenner zu \n normiert.
    /// </summary>
    private static string ReadableCellTextOf(RowItem row, ColumnItem column) =>
        DecodedCellText(column, row.CellGetString(column)).Replace("\r", "\n");

    #endregion
}
