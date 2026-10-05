// Licensed under MIT; see License.md for disclaimer, details, and extended user conditions.

namespace BeCreativeCLI.CliCommands;

/// <summary>
/// Tabellen: Prüft adressierte Zeilen per prepare_formula-Skript und meldet die Zeilen mit Problemen; Exit-Code 1, wenn eine Zeile Fehler hat oder das Skript scheitert.
/// </summary>
public class TableRowErrorsCliCommand : CliCommand {

    #region Properties

    public override string Command => "table-rowerrors";
    public override List<string> Flags => ["allrows", "nodetails"];
    public override List<string> Options => [.. AddressingOptions, "max", "password"];
    public override string Syntax => "bcr table-rowerrors <tabelle> + Zeilenadressierung (--rowkey <key> oder --filtercolumn <spalte> --filtervalue <wert> [--filtertype <typ>]) [--allrows] [--nodetails] [--max <anzahl>] [--password <kennwort>]";

    public override string? HelpDetails =>
            "Standard werden nur die Zeilen mit Problemen gemeldet, inkl. fehlerhafter Spalten samt Meldung; --allrows zeigt zusätzlich die fehlerfreien Zeilen ('Fehlerfrei'), " +
            "--nodetails blendet die fehlerhaften Spalten samt Meldung aus (nur die Zeilen-Keys). " +
            "--max <anzahl> gibt höchstens so viele fehlerhafte Zeilen aus und bricht dann ab (ohne --max unbegrenzt). " +
            "Jeder Fehlerfund wird vor der Meldung per kompletter Datenüberprüfung bestätigt; erweist sich die Zeile dabei als fehlerfrei, gilt sie als fehlerfrei.";

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

        var (max, maxError) = ResolveMax(args);

        if (maxError is not null) { return UsageError(maxError); }

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

        var showAll = args.Flag("allrows");
        var noDetails = args.Flag("nodetails");
        var withErrors = 0;

        foreach (var row in rows) {
            var check = VerifiedCheckRow(row);

            // Sobald das Ausgabelimit erreicht ist, ist eine weitere (teure) Prüfung sinnlos.
            if (check.ColumnsWithErrors is not { Count: 0 } && max > 0 && withErrors >= max) { break; }

            switch (check.ColumnsWithErrors) {
                case null:
                    withErrors++;
                    Console.Out.WriteLine("Zeile: " + row.KeyName);
                    if (!noDetails) {
                        Console.Out.WriteLine("Skript fehlgeschlagen: " + check.PrepareFormulaFeedback.FailedReason);
                    }
                    Console.Out.WriteLine();
                    break;

                case { Count: > 0 } columns:
                    withErrors++;
                    Console.Out.WriteLine("Zeile: " + row.KeyName);
                    if (!noDetails) {
                        foreach (var colError in columns) {
                            var parts = colError.SplitBy("|");
                            Console.Out.WriteLine("Spalte " + parts[0] + ": " + (parts.Length > 1 ? parts[1] : string.Empty));
                        }
                    }
                    Console.Out.WriteLine();
                    break;

                default:
                    if (showAll) {
                        Console.Out.WriteLine("Zeile: " + row.KeyName);
                        Console.Out.WriteLine("Fehlerfrei");
                        Console.Out.WriteLine();
                    }
                    break;
            }
        }

        return withErrors > 0 ? 1 : 0;
    }

    #endregion
}
