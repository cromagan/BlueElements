// Licensed under MIT; see License.md for disclaimer, details, and extended user conditions.

namespace BeCreativeCLI.CliCommands;

/// <summary>
/// Tabellen: Durchsucht Spalten auf dekodiertem Text; je Treffer Spalte, Zeilen-Key und Kontext, optional mit Wert einer Kontextspalte oder nur den Zeilen-Keys.
/// </summary>
public class TableSearchCliCommand : CliCommand {

    #region Fields

    /// <summary>
    /// Standard-Kontextlänge in Zeichen je Seite eines Treffers.
    /// </summary>
    private const int DefaultContext = 40;

    #endregion

    #region Properties

    public override string Command => "table-search";
    public override List<string> Flags => ["rowkeys-only", "decode"];
    public override List<string> Options => ["value", "column", "max", "context", "contextcolumn", "password"];
    public override string Syntax => "bcr table-search <tabelle> --value <suchtext> [--column <spalte>] [--max <anzahl>] [--context <zeichen>] [--contextcolumn <spalte>] [--rowkeys-only] [--decode] [--password <kennwort>]";

    public override string? HelpDetails =>
            "Die Suche läuft auf dekodiertem Zelltext: 'möglich' findet auch als m&#246;glich gespeicherte Werte; der Suchtext darf beide Formen enthalten. " +
            "Der Kontext umfasst standardmäßig 40 Zeichen je Seite und wird an Wortgrenzen ergänzt, statt Wörter abzureißen; --context <zeichen> ändert die Länge, --max <anzahl> begrenzt die Trefferzahl. " +
            "--contextcolumn <spalte> blendet je Treffer den Wert dieser Spalte derselben Zeile ein (z. B. KATEGORIE) — Titel sind mehrfach vergeben, so bleibt der Treffer der Ziel-Kategorie zuordenbar. " +
            "--rowkeys-only gibt nur die Zeilen-Keys der Treffer aus (einer je Zeile; nicht mit --contextcolumn kombinierbar), um sie z. B. gegen die Key-Liste einer Ziel-Kategorie zu schneiden. " +
            "--decode dekodiert Zelltext und Kontextspalte vollständig vor der Suche und in der Ausgabe (echte Umlaute statt Entities, wie table-export --decode).";

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

        var (max, maxError) = ResolveMax(args);

        if (maxError is not null) { return UsageError(maxError); }

        var context = DefaultContext;

        if (args.HasOption("context")) {
            context = IntParse(args.Option("context") ?? string.Empty);

            if (context <= 0) { return UsageError("--context erwartet eine positive Zeichenzahl."); }
        }

        if (args.Flag("rowkeys-only") && args.HasOption("contextcolumn")) {
            return UsageError("--rowkeys-only und --contextcolumn dürfen nicht kombiniert werden.");
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

        ColumnItem? contextColumn = null;

        if (args.HasOption("contextcolumn")) {
            contextColumn = tbl.Column[args.Option("contextcolumn") ?? string.Empty];

            if (contextColumn is null) {
                Console.Error.WriteLine("Spalte nicht gefunden: " + args.Option("contextcolumn"));
                return 1;
            }
        }

        var withRowKeysOnly = args.Flag("rowkeys-only");
        var decode = args.Flag("decode");
        var matches = 0;
        var limitReached = false;

        foreach (var row in tbl.RowsInSaveOrder()) {
            if (limitReached) { break; }

            foreach (var column in columns) {
                if (limitReached) { break; }

                var cellText = SearchTextOf(column, row.CellGetString(column));

                if (decode) { cellText = System.Net.WebUtility.HtmlDecode(cellText); }

                var index = cellText.IndexOf(searchValue, StringComparison.OrdinalIgnoreCase);

                if (index < 0) { continue; }

                if (withRowKeysOnly) {
                    // Ein Key je Treffer-Zeile genügt; weitere Vorkommen in derselben Zeile würden nur doppeln.
                    if (max > 0 && matches >= max) {
                        limitReached = true;
                        break;
                    }

                    Console.Out.WriteLine(row.KeyName);
                    matches++;
                    break;
                }

                var contextRaw = contextColumn is null ? string.Empty : SearchTextOf(contextColumn, row.CellGetString(contextColumn));

                if (decode) { contextRaw = System.Net.WebUtility.HtmlDecode(contextRaw); }

                var contextValue = string.Join(' ', contextRaw.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries));

                while (index >= 0) {
                    if (max > 0 && matches >= max) {
                        limitReached = true;
                        break;
                    }

                    var prefix = contextColumn is null ? string.Empty : " [" + contextColumn.KeyName + "=" + contextValue + "]";

                    Console.Out.WriteLine("Spalte " + column.KeyName + " Zeile " + row.KeyName + prefix + ": " + BuildContext(cellText, index, searchValue.Length, context));
                    matches++;

                    index = cellText.IndexOf(searchValue, index + searchValue.Length, StringComparison.OrdinalIgnoreCase);
                }
            }
        }

        if (matches == 0) {
            Console.Error.WriteLine("Keine Treffer.");
            return 1;
        }

        return 0;
    }

    /// <summary>
    /// Baut die Kontextausgabe zeichenbasiert: bis zu <paramref name="context"/> Zeichen
    /// vor und nach dem Treffer, wobei angetroffene Wörter bis zur Wortgrenze ergänzt
    /// werden. Gekürzte Seiten werden mit "..." angedeutet, Whitespace zu Leerzeichen
    /// zusammengefasst.
    /// </summary>
    private static string BuildContext(string text, int matchIndex, int matchLength, int context) {
        var start = Math.Max(0, matchIndex - context);
        var end = Math.Min(text.Length, matchIndex + matchLength + context);

        // Fenster nicht mitten im Wort abreißen lassen.
        while (start > 0 && !char.IsWhiteSpace(text[start - 1])) { start--; }

        while (end < text.Length && !char.IsWhiteSpace(text[end])) { end++; }

        var segment = string.Join(' ', text[start..end].Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries));
        var sb = new StringBuilder();

        if (start > 0) { sb.Append("... "); }

        sb.Append(segment);

        if (end < text.Length) { sb.Append(" ..."); }

        return sb.ToString();
    }

    #endregion
}
