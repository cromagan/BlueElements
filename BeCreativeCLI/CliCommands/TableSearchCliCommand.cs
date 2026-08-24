// Licensed under AGPL-3.0; see License.md for disclaimer and details.

namespace BeCreativeCLI.CliCommands;

public class TableSearchCliCommand : CliCommand {

    #region Properties

    public override string Command => "table-search";
    public override string Description => "Tabellen: Durchsucht alle Spalten oder nur die mit --column gewählte Spalte. Pro Treffer eine Ausgabezeile: Spalte, Zeilen-Key und der Treffer mit je drei Wörtern Kontext davor und danach. Groß-/Kleinschreibung wird ignoriert.";
    public override string Syntax => "bcr table-search <tabelle> --value <suchtext> [--column <spalte>]";

    #endregion

    #region Methods

    public override int DoIt(CliArgs args) {
        if (args.PositionalCount != 1 || !args.HasOption("value")) {
            Console.Error.WriteLine(Syntax);
            return 2;
        }

        var searchValue = (args.Option("value") ?? string.Empty).Trim();

        if (searchValue.Length == 0) {
            Console.Error.WriteLine("--value erwartet einen Suchtext.");
            return 2;
        }

        var tbl = LoadTable(args);

        if (tbl is null) { return 1; }

        try {
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

            foreach (var row in tbl.RowsInSaveOrder()) {
                foreach (var column in columns) {
                    var cellText = row.CellGetString(column);

                    var index = cellText.IndexOf(searchValue, StringComparison.OrdinalIgnoreCase);

                    while (index >= 0) {
                        Console.Out.WriteLine("Spalte " + column.KeyName + " Zeile " + row.KeyName + ": " + BuildContext(cellText, index, searchValue.Length));

                        index = cellText.IndexOf(searchValue, index + searchValue.Length, StringComparison.OrdinalIgnoreCase);
                    }
                }
            }

            return 0;
        } finally {
            Release(tbl);
        }
    }

    /// <summary>
    /// Baut die Kontextausgabe: der Treffer mit bis zu drei Wörtern davor und danach.
    /// Gekürzte Seiten werden mit "..." angedeutet.
    /// </summary>
    private static string BuildContext(string text, int matchIndex, int matchLength) {
        var words = WordsOf(text);
        var endIndex = matchIndex + matchLength - 1;

        var firstWordIndex = 0;
        var lastWordIndex = 0;

        for (var i = 0; i < words.Count; i++) {
            if (words[i].Start <= matchIndex) { firstWordIndex = i; }
            if (words[i].Start <= endIndex) { lastWordIndex = i; }
        }

        var beforeStart = Math.Max(0, firstWordIndex - 3);
        var afterEnd = Math.Min(words.Count - 1, lastWordIndex + 3);

        List<string> parts = [];

        if (beforeStart > 0) { parts.Add("..."); }

        for (var i = beforeStart; i < firstWordIndex; i++) { parts.Add(words[i].Word); }

        parts.Add(text[matchIndex..(matchIndex + matchLength)]);

        for (var i = lastWordIndex + 1; i <= afterEnd; i++) { parts.Add(words[i].Word); }

        if (afterEnd < words.Count - 1) { parts.Add("..."); }

        return string.Join(" ", parts);
    }

    /// <summary>
    /// Zerlegt den Text in Wörter (Nicht-Whitespace-Abschnitte) mit ihrer Startposition.
    /// </summary>
    private static List<(int Start, string Word)> WordsOf(string text) {
        List<(int Start, string Word)> words = [];

        var i = 0;

        while (i < text.Length) {
            while (i < text.Length && char.IsWhiteSpace(text[i])) { i++; }

            if (i >= text.Length) { break; }

            var start = i;

            while (i < text.Length && !char.IsWhiteSpace(text[i])) { i++; }

            words.Add((start, text[start..i]));
        }

        return words;
    }

    #endregion
}
