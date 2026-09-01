// Licensed under AGPL-3.0; see License.md for disclaimer and details.

namespace BeCreativeCLI.CliCommands;

public class TableSwapRowsCliCommand : CliCommand {

    #region Properties

    public override string Command => "table-swaprows";
    public override string Description => "Tabellen: Vertauscht die Positionen zweier Zeilen durch Tausch der Werte der Systemspalte SYS_ROWSORTINDEX (benutzerdefinierte Sortierung muss aktiv sein).";
    public override string Syntax => "bcr table-swaprows <tabelle> --rowkey <key1> --rowkey2 <key2>";

    #endregion

    #region Methods

    public override int DoIt(CliArgs args) {
        if (args.PositionalCount != 1 || !args.HasOption("rowkey") || !args.HasOption("rowkey2")) {
            Console.Error.WriteLine(Syntax);
            return 2;
        }

        var tbl = LoadTable(args);

        if (tbl is null) { return 1; }

        try {
            var fragmentProblem = FragmentEditProblem(tbl);

            if (fragmentProblem is not null) {
                Console.Error.WriteLine(fragmentProblem);
                return 2;
            }

            // Vertauschen ist nur über die benutzerdefinierte Sortierung möglich.
            if (tbl.Column.SysRowSortIndex is not { IsDisposed: false } sortCol) {
                Console.Error.WriteLine("Die Tabelle hat keine Systemspalte SYS_ROWSORTINDEX — die benutzerdefinierte Sortierung ist nicht aktiv.");
                return 1;
            }

            var key1 = args.Option("rowkey") ?? string.Empty;
            var key2 = args.Option("rowkey2") ?? string.Empty;

            var row1 = tbl.Row.GetByKey(key1);
            var row2 = tbl.Row.GetByKey(key2);

            if (row1 is null) {
                Console.Error.WriteLine("Zeile nicht gefunden: " + key1);
                return 1;
            }

            if (row2 is null) {
                Console.Error.WriteLine("Zeile nicht gefunden: " + key2);
                return 1;
            }

            if (row1 == row2) {
                Console.Error.WriteLine("Beide Keys zeigen auf dieselbe Zeile: " + row1.KeyName);
                return 2;
            }

            // Wie eine Benutzereingabe (GUI-Zeilen-Drag): Die Gruppe #CLI muss in
            // den Bearbeitungsrechten der Spalte stehen.
            foreach (var r in (RowItem[])[row1, row2]) {
                if (!tbl.PermissionCheck(sortCol.PermissionGroupsChangeCell, r, true)) {
                    Console.Error.WriteLine($"Zeile {r.KeyName}: Keine Rechte zum Verschieben: #CLI in den Bearbeitungsrechten der Spalte {sortCol.KeyName} ergänzen.");
                    return 1;
                }
            }

            var value1 = row1.CellGetString(sortCol);
            var value2 = row2.CellGetString(sortCol);

            var failed1 = row1.CellSet(sortCol, value2, "bcr table-swaprows");
            var failed2 = row2.CellSet(sortCol, value1, "bcr table-swaprows");

            if (!string.IsNullOrEmpty(failed1) || !string.IsNullOrEmpty(failed2)) {
                if (!string.IsNullOrEmpty(failed1)) { Console.Error.WriteLine($"Zeile {row1.KeyName} konnte nicht gesetzt werden: {failed1}"); }
                if (!string.IsNullOrEmpty(failed2)) { Console.Error.WriteLine($"Zeile {row2.KeyName} konnte nicht gesetzt werden: {failed2}"); }
                return 1;
            }

            Console.Out.WriteLine($"Positionen getauscht: {row1.KeyName} <-> {row2.KeyName}");
            return SaveTable(tbl);
        } finally {
            Release(tbl);
        }
    }

    #endregion
}
