// Licensed under AGPL-3.0; see License.md for disclaimer and details.

namespace BeCreativeCLI.CliCommands;

public class CellGetCliCommand : CliCommand {

    #region Properties

    public override string Command => "cellget";
    public override string Description => "Zeigt den Wert einer Zelle an. Die Zeile muss eindeutig adressiert sein.";
    public override string Syntax => "bcr cellget <tabelle> --column <spalte> + Zeilenadressierung (--rowkey <key> oder --filtercolumn <spalte> --filtervalue <wert> [--filtertype <typ>])";

    #endregion

    #region Methods

    public override int DoIt(CliArgs args) {
        if (args.PositionalCount != 1 || !args.HasOption("column")) {
            Console.Error.WriteLine(Syntax);
            return 2;
        }

        var problem = RowAddressingProblem(args);

        if (problem is not null) {
            Console.Error.WriteLine(problem);
            return 2;
        }

        var tbl = LoadTable(args);

        if (tbl is null) {
            Console.Error.WriteLine("Tabelle nicht gefunden: " + args[0]);
            return 1;
        }

        try {
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

            Console.Out.WriteLine(rows[0].CellGetString(column));
            return 0;
        } finally {
            tbl.Dispose();
        }
    }

    #endregion
}