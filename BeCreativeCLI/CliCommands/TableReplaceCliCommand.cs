// Licensed under MIT; see License.md for disclaimer, details, and extended user conditions.

namespace BeCreativeCLI.CliCommands;

/// <summary>
/// Tabellen: Ersetzt Text in Zellwerten (entity-bewusst: die Suche läuft auf dekodiertem Text) und speichert die Tabelle.
/// </summary>
public class TableReplaceCliCommand : CliCommand {

    #region Properties

    public override string Command => "table-replace";
    public override List<string> Flags => ["dry-run"];

    public override string? HelpDetails =>
            "Die Suche läuft auf dekodiertem Zelltext: 'gewürfelten' trifft auch als gew&#252;rfelten gespeicherte Werte; --find und --replace dürfen selbst Entities enthalten. " +
            "Ohne --column werden alle Spalten durchsucht (datenverwaltete Systemspalten wie SYS_DATECREATED ausgenommen), ohne Zeilenadressierung alle Zeilen. " +
            "Der neue Zellwert wird wie in der App gespeichert (Sonderzeichen als HTML-Entities). " +
            "Mit --dry-run werden nur die betroffenen Zellen angezeigt, nichts geändert und nichts gespeichert.";

    public override List<string> Options => [.. AddressingOptions, "find", "replace", "column", "password"];
    public override string Syntax => "bcr table-replace <tabelle> --find <text> --replace <ersatz> [--column <spalte>] [+ optionale Zeilenadressierung] [--dry-run]";

    #endregion

    #region Methods

    public override int DoIt(CliArgs args) {
        if (args.PositionalCount != 1) {
            var hint = args.PositionalCount > 1 && args.HasOption("find") ? " Hinweis: Enthält der Suchtext Leerzeichen, muss er in Anführungszeichen stehen (\"...\" oder '...')." : string.Empty;
            return UsageError($"Erwartet wird genau 1 Positionsargument (<tabelle>), erhalten: {args.PositionalCount}.{hint}");
        }

        if (!args.HasOption("find")) { return UsageError("Es fehlt die Option --find <text>."); }

        if (!args.HasOption("replace")) { return UsageError("Es fehlt die Option --replace <ersatz>."); }

        var find = System.Net.WebUtility.HtmlDecode(args.Option("find") ?? string.Empty);

        if (find.Length == 0) { return UsageError("--find erwartet einen Suchtext."); }

        var replace = System.Net.WebUtility.HtmlDecode(args.Option("replace") ?? string.Empty);

        // Zeilenadressierung ist optional; nur eine Teilangabe ist ein Fehler.
        if (args.HasOption("rowkey") || args.HasOption("filtercolumn") || args.HasOption("filtervalue") || args.HasOption("filtertype")) {
            var problem = RowAddressingProblem(args);

            if (problem is not null) {
                Console.Error.WriteLine(problem);
                return 2;
            }
        }

        var dryRun = args.Flag("dry-run");
        var tbl = LoadTable(args);

        if (tbl is null) { return 1; }

        try {
            // Ein Trockenlauf schreibt nichts und braucht daher den Fragment-Writer nicht.
            if (!dryRun) {
                var fragmentProblem = FragmentEditProblem(tbl);

                if (fragmentProblem is not null) {
                    Console.Error.WriteLine(fragmentProblem);
                    return 2;
                }
            }

            List<ColumnItem> columns;

            if (args.HasOption("column")) {
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

                columns = [column];
            } else {
                // Datenverwaltete Systemspalten still auslassen; sonstige nicht
                // änderbare Spalten dem Benutzer melden.
                columns = [.. tbl.Column.Where(c => c is { IsDisposed: false } && !c.IsSystemColumn() && ColumnWriteProblem(c) is null)];

                var skipped = tbl.Column.Where(c => c is { IsDisposed: false } && !c.IsSystemColumn() && ColumnWriteProblem(c) is not null).ToList();

                if (skipped.Count > 0) {
                    Console.Error.WriteLine("Übersprungen (nicht per CLI änderbar): " + string.Join(", ", skipped.Select(c => c.KeyName)));
                }
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

            var changedCells = 0;
            var changedRows = 0;
            var denied = false;

            foreach (var row in rows) {
                var rowChanged = 0;

                foreach (var column in columns) {
                    var searchtext = SearchTextOf(column, row.CellGetString(column));
                    var count = CountOccurrences(searchtext, find);

                    if (count == 0) { continue; }

                    var newValue = StorageTextOf(column, searchtext.Replace(find, replace, StringComparison.OrdinalIgnoreCase));

                    if (newValue == row.CellGetString(column)) { continue; }

                    if (dryRun) {
                        Console.Out.WriteLine($"Trockenlauf Zeile {row.KeyName} Spalte {column.KeyName}: {count} Fundstelle(n)");
                        changedCells += count;
                        rowChanged++;
                        continue;
                    }

                    // Wie eine Benutzereingabe: Die Gruppe #CLI muss in den Bearbeitungsrechten der Spalte stehen.
                    if (!tbl.PermissionCheck(column.PermissionGroupsChangeCell, row, true)) {
                        Console.Error.WriteLine($"Zeile {row.KeyName} Spalte {column.KeyName}: Keine Rechte, um diesen Wert zu ändern.");
                        denied = true;
                        continue;
                    }

                    var failed = row.CellSet(column, newValue, "bcr table-replace");

                    if (!string.IsNullOrEmpty(failed)) {
                        Console.Error.WriteLine($"Zeile {row.KeyName} Spalte {column.KeyName} konnte nicht gesetzt werden: {failed}");
                        continue;
                    }

                    Console.Out.WriteLine($"Zeile {row.KeyName} Spalte {column.KeyName}: {count} Ersetzung(en)");
                    changedCells += count;
                    rowChanged++;
                }

                if (rowChanged > 0) { changedRows++; }
            }

            if (denied) {
                Console.Error.WriteLine("Keine Rechte für mindestens eine Spalte: #CLI in deren Bearbeitungsrechten ergänzen.");
            }

            if (dryRun) {
                Console.Out.WriteLine($"Trockenlauf beendet: {changedCells} Fundstelle(n) in {changedRows} Zeile(n) — nichts geändert.");
                return 0;
            }

            if (changedCells == 0) {
                Console.Out.WriteLine("Keine Fundstellen.");
                return 0;
            }

            Console.Out.WriteLine($"{changedCells} Ersetzung(en) in {changedRows} Zeile(n).");
            return SaveTable(tbl);
        } finally {
            Release(tbl);
        }
    }

    /// <summary>
    /// Zählt die Vorkommen eines Texts (Groß-/Kleinschreibung egal).
    /// </summary>
    private static int CountOccurrences(string text, string value) {
        var count = 0;
        var index = 0;

        while ((index = text.IndexOf(value, index, StringComparison.OrdinalIgnoreCase)) >= 0) {
            count++;
            index += value.Length;
        }

        return count;
    }

    #endregion
}