// Licensed under MIT; see License.md for disclaimer, details, and extended user conditions.

namespace BeCreativeCLI.CliCommands;

/// <summary>
/// Tabellen: Löscht eine Spalte permanent (nur als Tabellen-Administrator). Systemspalten sind geschützt.
/// </summary>
public class TableDelColumnCliCommand : CliCommand {

    #region Properties

    public override string Command => "table-delcolumn";
    public override string Syntax => "bcr table-delcolumn <tabelle> <spaltenname>";

    #endregion

    #region Methods

    public override int DoIt(CliArgs args) {
        if (args.PositionalCount != 2) {
            Console.Error.WriteLine(Syntax);
            return 2;
        }

        var name = args[1] ?? string.Empty;

        var tbl = LoadTable(args);

        if (tbl is null) { return 1; }

        try {
            var fragmentProblem = FragmentEditProblem(tbl);

            if (fragmentProblem is not null) {
                Console.Error.WriteLine(fragmentProblem);
                return 2;
            }

            // Der Tabellenkopf wird verändert: Nur ein Administrator der Tabelle darf Spalten löschen.
            if (!tbl.IsAdministrator()) {
                Console.Error.WriteLine("Keine Rechte zum Löschen: #CLI bei den Tabellen-Administratoren ergänzen.");
                return 1;
            }

            var column = tbl.Column[name];

            if (column is not { IsDisposed: false }) {
                Console.Error.WriteLine("Spalte nicht gefunden: " + name);
                return 1;
            }

            // Headless gibt es keinen Sicherheitsdialog: Systemspalten niemals löschen.
            if (column.IsSystemColumn()) {
                Console.Error.WriteLine("Systemspalte " + column.KeyName + " kann nicht gelöscht werden.");
                return 1;
            }

            if (!tbl.Column.Remove(column, "bcr table-delcolumn")) {
                Console.Error.WriteLine("Spalte konnte nicht gelöscht werden: " + column.KeyName);
                return 1;
            }

            Console.Out.WriteLine("Spalte gelöscht: " + column.KeyName);
            return SaveTable(tbl);
        } finally {
            Release(tbl);
        }
    }

    #endregion
}
