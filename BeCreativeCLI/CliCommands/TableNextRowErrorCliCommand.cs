// Licensed under MIT; see License.md for disclaimer, details, and extended user conditions.

namespace BeCreativeCLI.CliCommands;

/// <summary>
/// Tabellen: Liefert die erste Zeile mit bestätigten Fehlermeldungen (Zeilen-Key, darunter die Meldungen) oder die Rückmeldung, dass alle Zeilen fehlerfrei sind.
/// Ein Fehlerfund wird vor der Meldung per kompletter Datenüberprüfung bestätigt; Fehlalarme werden übersprungen.
/// </summary>
public class TableNextRowErrorCliCommand : CliCommand {

    #region Properties

    public override string Command => "table-nextrowerror";
    public override List<string> Options => ["password"];
    public override string Syntax => "bcr table-nextrowerror <tabelle>";

    #endregion

    #region Methods

    public override int DoIt(CliArgs args) {
        if (args.PositionalCount != 1) {
            return UsageError($"Erwartet wird genau 1 Positionsargument (<tabelle>), erhalten: {args.PositionalCount}.");
        }

        var tbl = LoadTable(args);

        if (tbl is null) { return 1; }

        foreach (var row in tbl.RowsInSaveOrder()) {
            var check = VerifiedCheckRow(row);

            if (check.ColumnsWithErrors is { Count: 0 }) { continue; }

            Console.Out.WriteLine("Zeile: " + row.KeyName);

            if (check.ColumnsWithErrors is null) {
                Console.Out.WriteLine("Skript fehlgeschlagen: " + check.PrepareFormulaFeedback.FailedReason);
                return 1;
            }

            foreach (var colError in check.ColumnsWithErrors) {
                var parts = colError.SplitBy("|");
                Console.Out.WriteLine("Spalte " + parts[0] + ": " + (parts.Length > 1 ? parts[1] : string.Empty));
            }

            return 1;
        }

        Console.Out.WriteLine("Alle Zeilen fehlerfrei.");
        return 0;
    }

    #endregion
}
