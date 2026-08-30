// Licensed under AGPL-3.0; see License.md for disclaimer and details.

namespace BeCreativeCLI.CliCommands;

public class TableCellSetCliCommand : CliCommand {

    #region Properties

    public override string Command => "table-cellset";
    public override string Description => "Tabellen: Setzt den Wert einer Zelle in allen adressierten Zeilen und speichert die Tabelle.";
    public override string Syntax => "bcr table-cellset <tabelle> --column <spalte> --value <wert> + Zeilenadressierung (--rowkey <key> oder --filtercolumn <spalte> --filtervalue <wert> [--filtertype <typ>])";

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

        if (tbl is null) { return 1; }

        try {
            var fragmentProblem = FragmentEditProblem(tbl);

            if (fragmentProblem is not null) {
                Console.Error.WriteLine(fragmentProblem);
                return 2;
            }

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

            // Kapitelspalte: \r trennt mehrere Kapitel einer Zeile. Existiert
            // SYS_ROWSORTINDEX, ist genau ein Kapitel pro Zeile erzwungen.
            if (value.Contains('\r') && column == ChapterColumnOfView1(tbl) && tbl.Column.SysRowSortIndex is { IsDisposed: false }) {
                Console.Error.WriteLine("\\r ist in der Kapitelspalte nur erlaubt, wenn die Systemspalte SYS_ROWSORTINDEX nicht vorhanden ist.");
                return 2;
            }

            var done = 0;
            var permissionDenied = false;

            foreach (var row in rows) {
                // Wie eine Benutzereingabe: Die Gruppe #CLI muss in den Bearbeitungsrechten der Spalte stehen.
                if (!tbl.PermissionCheck(column.PermissionGroupsChangeCell, row, true)) {
                    Console.Error.WriteLine($"Zeile {row.KeyName}: Keine Rechte, um diesen Wert zu ändern.");
                    permissionDenied = true;
                    continue;
                }

                var failed = row.CellSet(column, value, "bcr table-cellset");

                if (!string.IsNullOrEmpty(failed)) {
                    Console.Error.WriteLine($"Zeile {row.KeyName} konnte nicht gesetzt werden: {failed}");
                } else {
                    Console.Error.WriteLine($"Wert gesetzt in {row.KeyName}");
                    done++;
                }
            }

            if (permissionDenied) {
                Console.Error.WriteLine("Keine Rechte für die Spalte " + column.KeyName + ": #CLI in den Bearbeitungsrechten der Spalte ergänzen.");
            }

            if (done == 0) { return 1; }

            Console.Out.WriteLine(done.ToString1() + " Zeile(n) aktualisiert.");
            return SaveTable(tbl);
        } finally {
            Release(tbl);
        }
    }

    #endregion
}
