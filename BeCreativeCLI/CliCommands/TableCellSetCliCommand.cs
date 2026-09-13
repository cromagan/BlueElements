// Licensed under MIT; see License.md for disclaimer, details, and extended user conditions.

namespace BeCreativeCLI.CliCommands;

/// <summary>
/// Tabellen: Setzt den Wert einer Zelle in allen adressierten Zeilen und speichert die Tabelle.
/// </summary>
public class TableCellSetCliCommand : CliCommand {

    #region Properties

    public override string Command => "table-cellset";
    public override List<string> Flags => ["dry-run"];
    public override List<string> Options => [.. AddressingOptions, "column", "value", "password"];
    public override string Syntax => "bcr table-cellset <tabelle> --column <spalte> --value <wert> + Zeilenadressierung (--rowkey <key> oder --filtercolumn <spalte> --filtervalue <wert> [--filtertype <typ>]) [--dry-run]";

    public override string? HelpDetails =>
            "--dry-run zeigt nur die Keys der Zeilen, in denen gesetzt würde — ohne zu ändern und ohne zu speichern. " +
            "Werte mit Leerzeichen gehören in Anführungszeichen.";

    #endregion

    #region Methods

    public override int DoIt(CliArgs args) {
        if (args.PositionalCount != 1) {
            return UsageError($"Erwartet wird genau 1 Positionsargument (<tabelle>), erhalten: {args.PositionalCount}.");
        }

        if (!args.HasOption("column")) {
            return UsageError("Es fehlt --column <spalte>. Werte mit Leerzeichen gehören in Anführungszeichen.");
        }

        if (!args.HasOption("value")) {
            return UsageError("Es fehlt --value <wert>. Werte mit Leerzeichen gehören in Anführungszeichen.");
        }

        var problem = RowAddressingProblem(args);

        if (problem is not null) {
            Console.Error.WriteLine(problem);
            return 2;
        }

        var tbl = LoadTable(args);

        if (tbl is null) { return 1; }

        try {
            // Ein Trockenlauf schreibt nichts und braucht daher den Fragment-Writer nicht.
            var dryRun = args.Flag("dry-run");

            var column = ColumnOfOption(tbl, args);

            if (column is null) {
                Console.Error.WriteLine("Spalte nicht gefunden: " + args.Option("column"));
                return 1;
            }

            var columnProblem = ColumnWriteProblem(column);

            if (columnProblem is not null) {
                Console.Error.WriteLine(columnProblem);
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

            // Wert in das Speicherformat der Spalte überführen (z. B. HTML-Entities).
            value = StorageTextOf(column, value);

            if (dryRun) {
                Console.Out.WriteLine("Trockenlauf — gesetzt würde in: " + string.Join(", ", rows.Select(r => r.KeyName)));
                Console.Out.WriteLine($"{rows.Count.ToString1()} Zeile(n), nichts gespeichert.");
                return 0;
            }

            // Erst nach allen Prüfungen den Fragment-Writer öffnen: Früh gescheiterte
            // Aufrufe sollen keine leere Fortsetzung mit EOF an die Fragment-Datei hängen.
            var fragmentProblem = FragmentEditProblem(tbl);

            if (fragmentProblem is not null) {
                Console.Error.WriteLine(fragmentProblem);
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
                    Console.Out.WriteLine($"Wert gesetzt in {row.KeyName}");
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
