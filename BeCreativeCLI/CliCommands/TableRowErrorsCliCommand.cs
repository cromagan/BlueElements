// Licensed under AGPL-3.0; see License.md for disclaimer and details.

namespace BeCreativeCLI.CliCommands;

public class TableRowErrorsCliCommand : CliCommand {

    #region Properties

    public override string Command => "table-rowerrors";
    public override string Description => "Tabellen: Prüft die adressierten Zeilen mit dem prepare_formula-Skript und gibt die Fehler aus. Exit-Code 1, wenn eine Zeile Fehler hat oder das Skript scheitert.";
    public override string Syntax => "bcr table-rowerrors <tabelle> + Zeilenadressierung (--rowkey <key> oder --filtercolumn <spalte> --filtervalue <wert> [--filtertype <typ>])";

    #endregion

    #region Methods

    public override int DoIt(CliArgs args) {
        if (args.PositionalCount != 1) {
            Console.Error.WriteLine(Syntax);
            return 2;
        }

        var problem = RowAddressingProblem(args);

        if (problem is not null) {
            Console.Error.WriteLine(problem);
            return 2;
        }

        var tbl = LoadTable(args);

        if (tbl is null) { return 1; }

        try {
            var (rows, error) = ResolveRows(tbl, args);

            if (error is not null) {
                Console.Error.WriteLine(error);
                return 1;
            }

            if (rows.Count == 0) {
                Console.Error.WriteLine("Keine Zeile getroffen.");
                return 1;
            }

            var withErrors = 0;

            foreach (var row in rows) {
                var check = row.CheckRow();

                Console.Out.WriteLine("Zeile: " + row.KeyName);

                if (check.ColumnsWithErrors is null) {
                    Console.Out.WriteLine("Skript fehlgeschlagen: " + check.PrepareFormulaFeedback.FailedReason);
                    withErrors++;
                } else if (check.ColumnsWithErrors.Count == 0) {
                    Console.Out.WriteLine("Fehlerfrei");
                } else {
                    foreach (var colError in check.ColumnsWithErrors) {
                        var parts = colError.SplitBy("|");
                        Console.Out.WriteLine("Spalte " + parts[0] + ": " + (parts.Length > 1 ? parts[1] : string.Empty));
                    }
                    withErrors++;
                }

                Console.Out.WriteLine();
            }

            return withErrors > 0 ? 1 : 0;
        } finally {
            Release(tbl);
        }
    }

    #endregion
}