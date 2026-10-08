// Licensed under MIT; see License.md for disclaimer, details, and extended user conditions.

namespace BeCreativeCLI.CliCommands;

/// <summary>
/// Tabellen: Setzt Werte einer oder mehrerer Spalten in allen adressierten Zeilen und speichert die Tabelle.
/// </summary>
public class TableCellSetCliCommand : CliCommand {

    #region Properties

    public override string Command => "table-cellset";
    public override List<string> Flags => ["dry-run"];

    public override List<string> Options => [.. AddressingOptions, "set", "password"];

    public override string Syntax => "bcr table-cellset <tabelle> --set <spalte>=<wert> [--set <spalte>=<wert> ...] + Zeilenadressierung (--rowkey <key> oder --filtercolumn <spalte> --filtervalue <wert> [--filtertype <typ>]) [--dry-run] [--password <kennwort>]";

    public override string? HelpDetails =>
            "--set setzt eine Spalte und darf mehrfach angegeben werden, um mehrere Spalten in einem Aufruf zu ändern: --set KATEGORIE=Regeln --set ANLEITUNG=\"Text mit Leerzeichen\". " +
            "--dry-run zeigt nur die Keys der Zeilen, in denen gesetzt würde — ohne zu ändern und ohne zu speichern. " +
            "Werte mit Leerzeichen gehören in Anführungszeichen; der Wert darf leer sein. " +
            "Abgeschlossene Zeilen (SYS_LOCKED) werden übersprungen; ausgenommen sind die Sperrspalte selbst und Spalten mit 'Bearbeitbar trotz Zeilensperre'. " +
            "Die Systemspalte SYS_ROWSORTINDEX hält die Sortiernummern lückenlos und braucht das CLI-Recht '" + CliRights.MoveRows + "'.";

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

        // Zielspalten einsammeln: wiederholbare --set-Angaben.
        List<(ColumnItem Column, string Value)> targets = [];

        foreach (var set in args.AllOptions("set")) {
            var eq = set.IndexOf('=');

            if (eq <= 0) {
                return UsageError("--set erwartet <spalte>=<wert>, erhalten: " + set);
            }

            var columnName = set[..eq];
            var column = tbl.Column[columnName];

            if (column is not { IsDisposed: false }) {
                return UsageError("Spalte nicht gefunden: " + columnName);
            }

            targets.Add((column, set[(eq + 1)..]));
        }

        if (targets.Count == 0) {
            return UsageError("Es fehlt mindestens ein --set <spalte>=<wert>. Werte mit Leerzeichen gehören in Anführungszeichen; der Wert darf leer sein.");
        }

        // Spalten- und Rechteprüfung vorab für alle Zielspalten.
        foreach (var (column, _) in targets) {
            var columnProblem = ColumnWriteProblem(tbl, column);

            if (columnProblem is not null) {
                Console.Error.WriteLine(columnProblem);
                return 1;
            }
        }

        // Die CLI vergleicht ausschließlich die CLI-Rechte der Tabelle.
        // SYS_ROWSORTINDEX hält die Sortiernummern lückenlos (Nummern werden verschoben) — dafür ist das eigene Recht 'Move rows' nötig.
        foreach (var neededRight in targets.Select(t => tbl.Column.SysRowSortIndex == t.Column ? CliRights.MoveRows : CliRights.ChangeCellValues).Distinct()) {
            var rightProblem = RightProblem(tbl, neededRight);

            if (rightProblem is not null) {
                Console.Error.WriteLine(rightProblem);
                return 1;
            }
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

        // Werte in das Speicherformat der jeweiligen Spalte überführen (z. B. HTML-Entities).
        var prepared = targets.ConvertAll(t => (Column: t.Column, Value: StorageTextOf(t.Column, t.Value)));

        // Ein Trockenlauf schreibt nichts und braucht daher den Fragment-Writer nicht.
        if (args.Flag("dry-run")) {
            var settable = rows.Where(r => !prepared.Exists(t => IsLocked(t.Column, r))).ToList();

            if (settable.Count == 0) {
                Console.Error.WriteLine("Keine bearbeitbare Zeile getroffen.");
                return 1;
            }

            Console.Out.WriteLine("Trockenlauf — Spalten: " + string.Join(", ", prepared.Select(t => t.Column.KeyName)));
            Console.Out.WriteLine("Gesetzt würde in: " + string.Join(", ", settable.Select(r => r.KeyName)));
            Console.Out.WriteLine($"{settable.Count.ToString1()} Zeile(n), nichts gespeichert.");
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
        var skipped = 0;

        foreach (var row in rows) {
            var lockedColumns = prepared.Where(t => IsLocked(t.Column, row)).Select(t => t.Column.KeyName).ToList();

            if (lockedColumns.Count > 0) {
                Console.Error.WriteLine($"Zeile {row.KeyName} ist abgeschlossen ({string.Join(", ", lockedColumns)}) — übersprungen.");
                skipped++;
                continue;
            }

            var failedColumns = 0;

            foreach (var (column, value) in prepared) {
                var failed = row.CellSet(column, value, "bcr table-cellset");

                if (!string.IsNullOrEmpty(failed)) {
                    Console.Error.WriteLine($"Zeile {row.KeyName}, Spalte {column.KeyName} konnte nicht gesetzt werden: {failed}");
                    failedColumns++;
                }
            }

            if (failedColumns == 0) {
                Console.Out.WriteLine($"Wert(e) gesetzt in {row.KeyName}");
                done++;
            }
        }

        if (skipped > 0) {
            Console.Error.WriteLine(skipped.ToString1() + " abgeschlossene Zeile(n) übersprungen.");
        }

        if (done == 0) { return 1; }

        Console.Out.WriteLine(done.ToString1() + " Zeile(n) aktualisiert.");
        return SaveTable(tbl);
    }

    #endregion
}
