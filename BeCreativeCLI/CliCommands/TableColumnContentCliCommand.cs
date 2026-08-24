// Licensed under AGPL-3.0; see License.md for disclaimer and details.

namespace BeCreativeCLI.CliCommands;

public class TableColumnContentCliCommand : CliCommand {

    #region Properties

    public override string Command => "table-columncontent";
    public override string Description => "Tabellen: Zeigt die Werte einer Spalte an, optional begrenzt auf eine maximale Anzahl.";
    public override string Syntax => "bcr table-columncontent <tabelle> --column <spalte> [--max <anzahl>]";

    #endregion

    #region Methods

    public override int DoIt(CliArgs args) {
        if (args.PositionalCount != 1 || !args.HasOption("column")) {
            Console.Error.WriteLine(Syntax);
            return 2;
        }

        var (max, maxError) = ResolveMax(args);

        if (maxError is not null) {
            Console.Error.WriteLine(maxError);
            return 2;
        }

        var tbl = LoadTable(args);

        if (tbl is null) { return 1; }

        try {
            var column = ColumnOfOption(tbl, args);

            if (column is null) {
                Console.Error.WriteLine("Spalte nicht gefunden: " + args.Option("column"));
                return 1;
            }

            var count = 0;

            foreach (var value in column.Contents()) {
                if (max > 0 && count >= max) { break; }

                Console.Out.WriteLine(value);
                count++;
            }

            return 0;
        } finally {
            Release(tbl);
        }
    }

    #endregion
}
