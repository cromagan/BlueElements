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
            "Werte mit Leerzeichen gehören in Anführungszeichen. " +
            "Abgeschlossene Zeilen (SYS_LOCKED) werden übersprungen; ausgenommen sind die Sperrspalte selbst und Spalten mit 'Bearbeitbar trotz Zeilensperre'. " +
            "Die Systemspalte SYS_ROWSORTINDEX hält die Sortiernummern lückenlos und braucht das CLI-Recht '" + CliRights.MoveRows + "'.";

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

        // Ein Trockenlauf schreibt nichts und braucht daher den Fragment-Writer nicht.
        var dryRun = args.Flag("dry-run");

        var column = ColumnOfOption(tbl, args);

        if (column is null) {
            Console.Error.WriteLine("Spalte nicht gefunden: " + args.Option("column"));
            return 1;
        }

        // Die CLI vergleicht ausschließlich die CLI-Rechte der Tabelle.
        // SYS_ROWSORTINDEX hält die Sortiernummern lückenlos (Nummern werden verschoben) — dafür ist das eigene Recht 'Move rows' nötig.
        var neededRight = tbl.Column.SysRowSortIndex == column ? CliRights.MoveRows : CliRights.ChangeCellValues;

        var rightProblem = RightProblem(tbl, neededRight);

        if (rightProblem is not null) {
            Console.Error.WriteLine(rightProblem);
            return 1;
        }

        var columnProblem = ColumnWriteProblem(tbl, column);

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

        // Wert in das Speicherformat der Spalte überführen (z. B. HTML-Entities).
        value = StorageTextOf(column, value);

        if (dryRun) {
            var settable = rows.Where(r => !IsLocked(column, r)).ToList();

            if (settable.Count == 0) {
                Console.Error.WriteLine("Keine bearbeitbare Zeile getroffen.");
                return 1;
            }

            Console.Out.WriteLine("Trockenlauf — gesetzt würde in: " + string.Join(", ", settable.Select(r => r.KeyName)));
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
            if (IsLocked(column, row)) {
                Console.Error.WriteLine($"Zeile {row.KeyName} ist abgeschlossen — übersprungen.");
                skipped++;
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

        if (skipped > 0) {
            Console.Error.WriteLine(skipped.ToString1() + " abgeschlossene Zeile(n) übersprungen.");
        }

        if (done == 0) { return 1; }

        Console.Out.WriteLine(done.ToString1() + " Zeile(n) aktualisiert.");
        return SaveTable(tbl);
    }

    #endregion
}
