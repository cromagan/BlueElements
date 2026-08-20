// Licensed under AGPL-3.0; see License.md for disclaimer and details.

namespace BeCreativeCLI.CliCommands;

public class CellSetCliCommand : CliCommand {

    #region Properties

    public override string Command => "cellset";
    public override string Description => "Setzt den Wert einer Zelle in allen adressierten Zeilen und speichert die Tabelle.";
    public override string Syntax => "bcr cellset <tabelle> --column <spalte> --value <wert> + Zeilenadressierung (--rowkey <key> oder --filtercolumn <spalte> --filtervalue <wert> [--filtertype <typ>])";

    #endregion

    #region Methods

    public override int DoIt(CliArgs args) {
        if (args.PositionalCount != 1 || !args.HasOption("column") || !args.HasOption("value")) {
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

            if (rows.Count == 0) {
                Console.Error.WriteLine("Keine Zeile getroffen.");
                return 1;
            }

            var value = args.Option("value") ?? string.Empty;

            foreach (var row in rows) {
                var failed = row.CellSet(column, value, "bcr set");

                if (!string.IsNullOrEmpty(failed)) {
                    Console.Error.WriteLine($"Zeile {row.KeyName} konnte nicht gesetzt werden: {failed}");
                } else {
                    Console.Error.WriteLine($"Wert gesetzt in {row.KeyName}");
                }
            }

            Console.Out.WriteLine(rows.Count.ToString1() + " Zeile(n) aktualisiert.");
            return SaveTable(tbl);
        } finally {
            tbl.Dispose();
        }
    }

    #endregion
}