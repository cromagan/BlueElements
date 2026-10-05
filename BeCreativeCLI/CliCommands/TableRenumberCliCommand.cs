// Licensed under MIT; see License.md for disclaimer, details, and extended user conditions.

namespace BeCreativeCLI.CliCommands;

/// <summary>
/// Tabellen: Nummeriert die adressierten Zeilen aufsteigend zur Sortier-Spalte neu und speichert die Tabelle.
/// </summary>
public class TableRenumberCliCommand : CliCommand {

    #region Properties

    public override string Command => "table-renumber";
    public override List<string> Flags => ["dry-run"];

    public override string? HelpDetails =>
            "Schreibt fortlaufende Nummern in die Spalte --column, sortiert aufsteigend nach der Sortier-Spalte. " +
            "Die Vergabe läuft in zwei Phasen (Temp-Nummern, dann Endnummern), damit ein Unique-Constraint auf der Nummern-Spalte beim Umnummerieren nicht kollidiert. " +
            "--start (Standard 1) und --step (Standard 1) steuern den Nummernkreis. " +
            "Werte werden numerisch verglichen, wenn beide Seiten Zahlen sind, sonst alphabetisch; bei Gleichstand behält die aktuelle Reihenfolge. " +
            "Abgeschlossene Zeilen (SYS_LOCKED) werden übersprungen. " +
            "Je Zeile wird die alte und die neue Nummer gemeldet. " +
            "Die Nummern-Spalte braucht das CLI-Recht 'Change cell values' — bei SYS_ROWSORTINDEX 'Move rows'. " +
            "Mit --dry-run werden nur die geplanten Nummern angezeigt, nichts geändert und nichts gespeichert.";

    public override List<string> Options => [.. AddressingOptions, "column", "sortcolumn", "start", "step", "password"];
    public override string Syntax => "bcr table-renumber <tabelle> --column <nummernspalte> --sortcolumn <sortierspalte> [--start <n>] [--step <n>] [+ optionale Zeilenadressierung] [--dry-run] [--password <kennwort>]";

    #endregion

    #region Methods

    public override int DoIt(CliArgs args) {
        if (args.PositionalCount != 1) {
            return UsageError($"Erwartet wird genau 1 Positionsargument (<tabelle>), erhalten: {args.PositionalCount}.");
        }

        if (!args.HasOption("column")) { return UsageError("Es fehlt --column <nummernspalte>."); }

        if (!args.HasOption("sortcolumn")) { return UsageError("Es fehlt --sortcolumn <sortierspalte>."); }

        var start = 1;
        var step = 1;

        if (args.HasOption("start") && (!int.TryParse(args.Option("start"), out start) || start < 0)) {
            return UsageError("--start erwartet eine ganze Zahl >= 0.");
        }

        if (args.HasOption("step") && (!int.TryParse(args.Option("step"), out step) || step < 1)) {
            return UsageError("--step erwartet eine positive Zahl.");
        }

        // Zeilenadressierung ist optional; nur eine Teilangabe ist ein Fehler.
        if (args.HasOption("rowkey") || args.HasOption("filtercolumn") || args.HasOption("filtervalue") || args.HasOption("filtertype")) {
            var addressingProblem = RowAddressingProblem(args);

            if (addressingProblem is not null) {
                Console.Error.WriteLine(addressingProblem);
                return 2;
            }
        }

        var dryRun = args.Flag("dry-run");
        var tbl = LoadTable(args);

        if (tbl is null) { return 1; }

        var numberColumn = ColumnOfOption(tbl, args);

        if (numberColumn is null) {
            Console.Error.WriteLine("Spalte nicht gefunden: " + args.Option("column"));
            return 1;
        }

        var sortColumn = tbl.Column[args.Option("sortcolumn") ?? string.Empty];

        if (sortColumn is null) {
            Console.Error.WriteLine("Spalte nicht gefunden: " + args.Option("sortcolumn"));
            return 1;
        }

        // Die CLI vergleicht ausschließlich die CLI-Rechte der Tabelle.
        // Wie table-cellset: SYS_ROWSORTINDEX verschiebt Zeilen ('Move rows'), sonst Zellwerte ändern.
        var neededRight = tbl.Column.SysRowSortIndex == numberColumn ? CliRights.MoveRows : CliRights.ChangeCellValues;

        var rightProblem = RightProblem(tbl, neededRight);

        if (rightProblem is not null) {
            Console.Error.WriteLine(rightProblem);
            return 1;
        }

        var columnProblem = ColumnWriteProblem(tbl, numberColumn);

        if (columnProblem is not null) {
            Console.Error.WriteLine(columnProblem);
            return 1;
        }

        List<RowItem> rows;

        if (args.HasOption("rowkey") || args.HasOption("filtercolumn") || args.HasOption("filtervalue")) {
            var (resolved, error) = ResolveRows(tbl, args);

            if (error is not null) {
                Console.Error.WriteLine(error);
                return 1;
            }

            rows = resolved;
        } else {
            rows = [.. tbl.RowsInSaveOrder()];
        }

        if (rows.Count == 0) {
            Console.Error.WriteLine("Keine Zeile getroffen.");
            return 1;
        }

        // Ziel-Reihenfolge: aufsteigend zur Sortier-Spalte.
        // OrderBy ist stabil — Gleichstand behält die aktuelle Reihenfolge.
        var comparer = new CellValueComparer();
        var ordered = rows.OrderBy(r => r.CellGetString(sortColumn), comparer);

        // Geplante Nummern je Zeile merken; die alten Werte müssen vor dem Schreiben gelesen werden.
        var assignments = new List<(RowItem Row, string Old, string New)>();
        var number = start;
        var skipped = 0;

        foreach (var row in ordered) {
            if (IsLocked(numberColumn, row)) {
                Console.Error.WriteLine($"Zeile {row.KeyName} ist abgeschlossen — übersprungen.");
                skipped++;
                continue;
            }

            assignments.Add((row, row.CellGetString(numberColumn), number.ToString1()));
            number += step;
        }

        if (assignments.Count == 0) {
            Console.Error.WriteLine("Keine bearbeitbare Zeile getroffen.");
            return 1;
        }

        if (dryRun) {
            foreach (var (Row, Old, New) in assignments) {
                Console.Out.WriteLine($"{Row.KeyName}: {Old} → {New}");
            }

            Console.Out.WriteLine("Trockenlauf — " + assignments.Count.ToString1() + " Zeile(n), nichts geändert und nichts gespeichert.");
            return 0;
        }

        // Erst nach allen Prüfungen den Fragment-Writer öffnen: Früh gescheiterte
        // Aufrufe sollen keine leere Fortsetzung mit EOF an die Fragment-Datei hängen.
        var fragmentProblem = FragmentEditProblem(tbl);

        if (fragmentProblem is not null) {
            Console.Error.WriteLine(fragmentProblem);
            return 2;
        }

        // Phase 1: Temp-Nummern schreiben, damit eine eindeutige Nummern-Spalte beim
        // Umnummerieren nicht kollidiert. SYS_ROWSORTINDEX hält das System selbst
        // lückenlos — dort ist keine Temp-Phase nötig.
        if (tbl.Column.SysRowSortIndex != numberColumn) {
            var tempNumber = 0;

            foreach (var (Row, Old, New) in assignments) {
                tempNumber--;
                WriteCell(Row, numberColumn, tempNumber.ToString1());
            }
        }

        // Phase 2: Endnummern in Ziel-Reihenfolge.
        var done = 0;
        var failed = 0;

        foreach (var (Row, Old, New) in assignments) {
            var writeFailed = Row.CellSet(numberColumn, New, "bcr table-renumber");

            if (writeFailed is { Length: > 0 }) {
                Console.Error.WriteLine($"Zeile {Row.KeyName} konnte nicht gesetzt werden: {writeFailed}");
                failed++;
            } else {
                Console.Out.WriteLine($"{Row.KeyName}: {Old} → {New}");
                done++;
            }
        }

        if (skipped > 0) {
            Console.Error.WriteLine(skipped.ToString1() + " abgeschlossene Zeile(n) übersprungen.");
        }

        if (failed > 0) {
            Console.Error.WriteLine(failed.ToString1() + " Zeile(n) fehlerhaft.");
        }

        if (done == 0) { return 1; }

        Console.Out.WriteLine(done.ToString1() + " Zeile(n) neu nummeriert.");
        return SaveTable(tbl);
    }

    /// <summary>
    /// Schreibt einen Zellwert; Fehler werden auf stderr gemeldet.
    /// </summary>
    private static void WriteCell(RowItem row, ColumnItem column, string value) {
        var failed = row.CellSet(column, value, "bcr table-renumber");

        if (failed is { Length: > 0 }) {
            Console.Error.WriteLine($"Zeile {row.KeyName} konnte nicht gesetzt werden: {failed}");
        }
    }

    #endregion

    #region Classes

    /// <summary>
    /// Vergleicht Zelltexte numerisch, wenn beide Seiten Zahlen sind, sonst alphabetisch.
    /// </summary>
    private sealed class CellValueComparer : Comparer<string> {

        #region Methods

        public override int Compare(string? x, string? y) {
            var xIsNumber = DoubleTryParse(x, out var xNumber);
            var yIsNumber = DoubleTryParse(y, out var yNumber);

            if (xIsNumber && yIsNumber) { return xNumber.CompareTo(yNumber); }

            return string.Compare(x, y, StringComparison.OrdinalIgnoreCase);
        }

        #endregion
    }

    #endregion
}