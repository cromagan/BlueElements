// Licensed under MIT; see License.md for disclaimer, details, and extended user conditions.

namespace BeCreativeCLI.CliCommands;

/// <summary>
/// Tabellen: Zählt die Zeilen, die einen Suchtext enthalten — dekorierter Text wie table-search, aber ohne Zeilenliste.
/// </summary>
public class TableCountCliCommand : CliCommand {

    #region Properties

    public override string Command => "table-count";
    public override List<string> Flags => ["decode"];
    public override List<string> Options => [.. AddressingOptions, "value", "column", "password"];

    public override string Syntax => "bcr table-count <tabelle> --value <suchtext> [--column <spalte>] [+ optionale Zeilenadressierung] [--decode] [--password <kennwort>]";

    public override string? HelpDetails =>
            "Gezählt werden Trefferzeilen, nicht Fundstellen: Eine Zeile zählt auch bei mehreren Treffern genau einmal. " +
            "Die Suche läuft wie table-search auf dekodiertem Zelltext ('möglich' findet auch als m&#246;glich gespeicherte Werte); der Suchtext darf beide Formen enthalten. " +
            "--decode dekodiert die Zelltexte vollständig vor der Suche (echte Umlaute statt Entities) — das kann die Trefferzahl gegenüber dem Speicherformat verändern. " +
            "Ohne --column werden alle Spalten durchsucht; die optionale Zeilenadressierung (--rowkey <key> oder --filtercolumn <spalte> --filtervalue <wert> [--filtertype equals|exact|contains|startswith]) begrenzt die durchsuchten Zeilen. " +
            "Die Ausgabe nennt zusätzlich die Fundstellen und die Anzahl der durchsuchten Zeilen; bei 0 Treffern wird 0 ausgegeben und Erfolg gemeldet. " +
            "Benötigtes CLI-Recht: keines (rein lesend). " +
            "Beispiel: bcr table-count kategorien.tblh --value Bäcker --column NAME --filtercolumn TYP --filtervalue Haupt --filtertype startswith";

    #endregion

    #region Methods

    public override int DoIt(CliArgs args) {
        if (args.PositionalCount != 1) {
            var hint = args.PositionalCount > 1 && args.HasOption("value") ? " Hinweis: Enthält der Suchtext Leerzeichen, muss er in Anführungszeichen stehen (\"...\" oder '...')." : string.Empty;
            return UsageError($"Erwartet wird genau 1 Positionsargument (<tabelle>), erhalten: {args.PositionalCount}.{hint}");
        }

        if (!args.HasOption("value")) { return UsageError("Es fehlt die Option --value <suchtext>."); }

        var searchValue = System.Net.WebUtility.HtmlDecode((args.Option("value") ?? string.Empty).Trim());

        if (searchValue.Length == 0) { return UsageError("--value erwartet einen Suchtext."); }

        // Zeilenadressierung ist optional; nur eine Teilangabe ist ein Fehler.
        if (args.HasOption("rowkey") || args.HasOption("filtercolumn") || args.HasOption("filtervalue") || args.HasOption("filtertype")) {
            var addressingProblem = RowAddressingProblem(args);

            if (addressingProblem is not null) {
                Console.Error.WriteLine(addressingProblem);
                return 2;
            }
        }

        var tbl = LoadTable(args);

        if (tbl is null) { return 1; }

        List<ColumnItem> columns;

        if (args.HasOption("column")) {
            var column = ColumnOfOption(tbl, args);

            if (column is null) {
                Console.Error.WriteLine("Spalte nicht gefunden: " + args.Option("column"));
                return 1;
            }

            columns = [column];
        } else {
            columns = [.. tbl.Column.Where(c => c is { IsDisposed: false })];
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

        var decode = args.Flag("decode");
        var matchRows = 0;
        var occurrences = 0;

        foreach (var row in rows) {
            var rowHits = 0;

            foreach (var column in columns) {
                var cellText = SearchTextOf(column, row.CellGetString(column));

                if (decode) { cellText = System.Net.WebUtility.HtmlDecode(cellText); }

                var index = cellText.IndexOf(searchValue, StringComparison.OrdinalIgnoreCase);

                while (index >= 0) {
                    rowHits++;
                    index = cellText.IndexOf(searchValue, index + searchValue.Length, StringComparison.OrdinalIgnoreCase);
                }
            }

            if (rowHits > 0) {
                matchRows++;
                occurrences += rowHits;
            }
        }

        var scope = args.HasOption("column") ? " in Spalte " + columns[0].KeyName : " über alle Spalten";
        Console.Out.WriteLine($"{matchRows.ToString1()} Trefferzeile(n) enthalten '{searchValue}'{scope} ({occurrences.ToString1()} Fundstelle(n), {rows.Count.ToString1()} Zeilen durchsucht).");
        return 0;
    }

    #endregion
}
