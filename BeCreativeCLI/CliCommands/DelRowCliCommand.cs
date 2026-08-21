// Licensed under AGPL-3.0; see License.md for disclaimer and details.

namespace BeCreativeCLI.CliCommands;

public class DelRowCliCommand : CliCommand {

    #region Properties

    public override string Command => "delrow";
    public override string Description => "Löscht alle adressierten Zeilen.";
    public override string Syntax => "bcr delrow <tabelle> + Zeilenadressierung (--rowkey <key> oder --filtercolumn <spalte> --filtervalue <wert> [--filtertype <typ>])";

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
            // Wie eine Benutzereingabe: Zeilen löschen darf nur ein Tabellen-Administrator
            // (#CLI muss also bei den Tabellen-Administratoren stehen).
            if (!tbl.IsAdministrator()) {
                Console.Error.WriteLine("Keine Rechte zum Löschen: #CLI bei den Tabellen-Administratoren ergänzen.");
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

            var deleted = 0;
            var failed = 0;

            foreach (var r in rows) {
                var opr = RowCollection.Remove(r, "bcr delrow");

                if (opr.IsFailed) {
                    Console.Error.WriteLine("Key: " + r.KeyName + " löschen fehlgeschlagen: " + opr.FailedReason);
                    failed++;
                } else {
                    Console.Out.WriteLine("Key: " + r.KeyName + " gelöscht");
                    deleted++;
                }
            }

            if (deleted > 0) {
                var sr = SaveTable(tbl);

                if (sr != 0) { return sr; }
            }

            return failed > 0 ? 1 : 0;
        } finally {
            Release(tbl);
        }
    }

    #endregion
}