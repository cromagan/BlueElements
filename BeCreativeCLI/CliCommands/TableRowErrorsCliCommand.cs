// Licensed under MIT; see License.md for disclaimer, details, and extended user conditions.

namespace BeCreativeCLI.CliCommands;

/// <summary>
/// Tabellen: Prüft die adressierten Zeilen mit dem prepare_formula-Skript und gibt die Fehler aus. Exit-Code 1, wenn eine Zeile Fehler hat oder das Skript scheitert.
/// Ein Fehlerfund wird vor der Meldung per kompletter Datenüberprüfung bestätigt; erweist sich die Zeile dabei als fehlerfrei, wird sie als fehlerfrei gemeldet.
/// </summary>
public class TableRowErrorsCliCommand : CliCommand {

    #region Properties

    public override string Command => "table-rowerrors";
    public override List<string> Options => [.. AddressingOptions, "password"];
    public override string Syntax => "bcr table-rowerrors <tabelle> + Zeilenadressierung (--rowkey <key> oder --filtercolumn <spalte> --filtervalue <wert> [--filtertype <typ>])";

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
            var check = VerifiedCheckRow(row);

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
    }

    #endregion
}
