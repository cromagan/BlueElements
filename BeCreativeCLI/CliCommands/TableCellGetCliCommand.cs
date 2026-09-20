// Licensed under MIT; see License.md for disclaimer, details, and extended user conditions.

namespace BeCreativeCLI.CliCommands;

/// <summary>
/// Tabellen: Zeigt den Wert einer Zelle an. Die Zeile muss eindeutig adressiert sein.
/// </summary>
public class TableCellGetCliCommand : CliCommand {

    #region Properties

    public override string Command => "table-cellget";
    public override List<string> Options => [.. AddressingOptions, "column", "password"];
    public override string Syntax => "bcr table-cellget <tabelle> --column <spalte> + Zeilenadressierung (--rowkey <key> oder --filtercolumn <spalte> --filtervalue <wert> [--filtertype <typ>])";

    #endregion

    #region Methods

    public override int DoIt(CliArgs args) {
        if (args.PositionalCount != 1) {
            return UsageError($"Erwartet wird genau 1 Positionsargument (<tabelle>), erhalten: {args.PositionalCount}.");
        }

        if (!args.HasOption("column")) { return UsageError("Es fehlt die Option --column <spalte>."); }

        var problem = RowAddressingProblem(args);

        if (problem is not null) {
            Console.Error.WriteLine(problem);
            return 2;
        }

        var tbl = LoadTable(args);

        if (tbl is null) { return 1; }

        var column = ColumnOfOption(tbl, args);

        if (column is null) {
            Console.Error.WriteLine("Spalte nicht gefunden: " + args.Option("column"));
            return 1;
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

        // Zellen trennen Zeilen mit \r; für die Konsole/Ausgabe in \n überführen.
        Console.Out.WriteLine(rows[0].CellGetString(column).Replace("\r", "\n"));
        return 0;
    }

    #endregion
}
