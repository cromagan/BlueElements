// Licensed under MIT; see License.md for disclaimer, details, and extended user conditions.

namespace CodeOptimizer.Classes;

/// <summary>
/// Ersetzt in C#-Quelltext die Inhalte von Kommentaren und Strings durch Leerzeichen,
/// damit Klammer-Tiefen und Token-Suchen textbasiert ohne Störung durch String-Inhalte laufen.
/// Zusätzlich liefert sie je Eingabezeichen die Art (Code, String, Kommentar) für Treffer-Prüfungen.
/// </summary>
public sealed class CsSanitizer {

    #region Fields

    /// <summary>
    /// Art eines Zeichens: regulärer Code.
    /// </summary>
    public const char KindCode = 'c';

    /// <summary>
    /// Art eines Zeichens: String-, Zeichenliteral- oder Interpolationsinhalt (inkl. Löcher).
    /// </summary>
    public const char KindString = 's';

    /// <summary>
    /// Art eines Zeichens: Kommentar.
    /// </summary>
    public const char KindComment = 'k';

    private const int StCode = 0;
    private const int StLineComment = 1;
    private const int StBlockComment = 2;
    private const int StString = 3;
    private const int StVerbatimString = 4;
    private const int StRawString = 5;
    private const int StInterpString = 6;
    private const int StInterpVerbatimString = 7;
    private const int StCharLiteral = 8;
    private const int StHole = 9;

    private int state = StCode;
    private int holeDepth;
    private int rawQuotes;
    private bool holesAsCode;
    private readonly Stack<(int State, int HoleDepth)> saved = new();

    #endregion

    #region Methods

    /// <summary>
    /// Bereinigt alle Zeilen und liefert zeilengleich die bereinigten Zeilen zurück.
    /// </summary>
    public List<string> SanitizeLines(List<string> lines) {
        var result = new List<string>(lines.Count);
        foreach (var line in lines) {
            var sb = new StringBuilder(line.Length);
            SanitizeInto(sb, null, line);
            result.Add(sb.ToString());
            if (state == StLineComment) { state = StCode; }
        }
        return result;
    }

    // TODO: Unused
    /// <summary>
    /// Bereinigt einen zusammenhängenden Textabschnitt und behält exakt die Zeichenanzahl bei (Positionen bleiben gültig).
    /// </summary>
    public string SanitizeText(string text) {
        var sb = new StringBuilder(text.Length);
        var start = 0;

        while (start < text.Length) {
            var nl = text.IndexOf('\n', start);
            var end = nl < 0 ? text.Length : nl + 1;
            SanitizeInto(sb, null, text[start..end]);

            if (state == StLineComment) { state = StCode; }

            start = end;
        }

        return sb.ToString();
    }

    /// <summary>
    /// Liefert je Eingabezeichen die Art (KindCode, KindString oder KindComment); die Indizes entsprechen dem Originaltext.
    /// </summary>
    public string SanitizeKinds(string text) {
        var kinds = new StringBuilder(text.Length);
        var start = 0;

        while (start < text.Length) {
            var nl = text.IndexOf('\n', start);
            var end = nl < 0 ? text.Length : nl + 1;
            SanitizeInto(null, kinds, text[start..end]);

            if (state == StLineComment) { state = StCode; }

            start = end;
        }

        return kinds.ToString();
    }

    /// <summary>
    /// Wie SanitizeKinds, aber Inhalte von Interpolationslöchern (z. B. '{name}' in $"...") werden als Code gezählt;
    /// String-Inhalte innerhalb der Löcher bleiben String. Nützlich für Verwendungsprüfungen von Bezeichnern.
    /// </summary>
    public string SanitizeKindsWithCodeHoles(string text) {
        holesAsCode = true;
        try {
            return SanitizeKinds(text);
        } finally {
            holesAsCode = false;
        }
    }

    /// <summary>
    /// Wie SanitizeLines, aber Inhalte von Interpolationslöchern (z. B. '{name}' in $"...") bleiben als Code erhalten;
    /// String-Inhalte innerhalb der Löcher werden geleert. Nützlich für Verwendungsprüfungen von Bezeichnern.
    /// </summary>
    public List<string> SanitizeLinesWithCodeHoles(List<string> lines) {
        holesAsCode = true;
        try {
            return SanitizeLines(lines);
        } finally {
            holesAsCode = false;
        }
    }

    /// <summary>
    /// Liefert true, wenn der Bereich ausschließlich Code-Zeichen enthält (kein String, kein Kommentar, keine Interpolation).
    /// </summary>
    public static bool IsPureCode(string kinds, int start, int length) {
        if (start < 0 || start + length > kinds.Length) { return false; }
        return kinds.AsSpan(start, length).IndexOfAny(KindString, KindComment) < 0;
    }

    /// <summary>
    /// Liefert true, wenn der Bereich String- oder Zeichenliteral-Anteile enthält; Kommentare stören nicht.
    /// </summary>
    public static bool ContainsString(string kinds, int start, int length) {
        if (start < 0 || start + length > kinds.Length) { return true; }
        return kinds.AsSpan(start, length).IndexOf(KindString) >= 0;
    }

    private void SanitizeInto(StringBuilder? sb, StringBuilder? kinds, string line) {
        var i = 0;
        while (i < line.Length) {
            var c = line[i];
            var next = i + 1 < line.Length ? line[i + 1] : '\0';
            switch (state) {
                case StCode:
                    if (c == '/' && next == '/') {
                        state = StLineComment;
                        Append(sb, kinds, "  ", KindComment);
                        i++;
                    } else if (c == '/' && next == '*') {
                        state = StBlockComment;
                        Append(sb, kinds, "  ", KindComment);
                        i++;
                    } else if (c == '"') {
                        saved.Push((StCode, holeDepth));
                        HandleQuoteStart(sb, kinds, line, ref i);
                    } else if (c == '\'') {
                        saved.Push((StCode, holeDepth));
                        state = StCharLiteral;
                        Append(sb, kinds, ' ', KindString);
                    } else {
                        Append(sb, kinds, c, KindCode);
                    }
                    break;
                case StLineComment:
                    Append(sb, kinds, ' ', KindComment);
                    break;
                case StBlockComment:
                    if (c == '*' && next == '/') {
                        state = StCode;
                        Append(sb, kinds, "  ", KindComment);
                        i++;
                    } else {
                        Append(sb, kinds, ' ', KindComment);
                    }
                    break;
                case StString:
                    if (c == '\\') {
                        Append(sb, kinds, "  ", KindString);
                        i++;
                    } else if (c == '"') {
                        CloseStringState();
                        Append(sb, kinds, ' ', KindString);
                    } else {
                        Append(sb, kinds, ' ', KindString);
                    }
                    break;
                case StVerbatimString:
                    if (c == '"' && next == '"') {
                        Append(sb, kinds, "  ", KindString);
                        i++;
                    } else if (c == '"') {
                        CloseStringState();
                        Append(sb, kinds, ' ', KindString);
                    } else {
                        Append(sb, kinds, ' ', KindString);
                    }
                    break;
                case StRawString:
                    if (c == '"' && QuoteRun(line, i) == rawQuotes) {
                        AppendSpaces(sb, kinds, rawQuotes, KindString);
                        i += rawQuotes - 1;
                        CloseStringState();
                    } else {
                        Append(sb, kinds, ' ', KindString);
                    }
                    break;
                case StCharLiteral:
                    if (c == '\\') {
                        Append(sb, kinds, "  ", KindString);
                        i++;
                    } else if (c == '\'') {
                        CloseStringState();
                        Append(sb, kinds, ' ', KindString);
                    } else {
                        Append(sb, kinds, ' ', KindString);
                    }
                    break;
                case StInterpString:
                case StInterpVerbatimString:
                    if ((c == '\\' && state == StInterpString) || (c == '"' && state == StInterpVerbatimString && next == '"') || (c == '{' && next == '{')) {
                        Append(sb, kinds, "  ", KindString);
                        i++;
                    } else if (c == '"') {
                        CloseStringState();
                        Append(sb, kinds, ' ', KindString);
                    } else if (c == '{') {
                        saved.Push((state, holeDepth));
                        holeDepth = 0;
                        state = StHole;
                        Append(sb, kinds, ' ', KindString);
                    } else {
                        Append(sb, kinds, ' ', KindString);
                    }
                    break;
                case StHole:
                    if (c == '{') {
                        holeDepth++;
                        Append(sb, kinds, '{', KindString);
                    } else if (c == '}') {
                        if (holeDepth == 0) {
                            (state, holeDepth) = saved.Pop();
                            Append(sb, kinds, ' ', KindString);
                        } else {
                            holeDepth--;
                            Append(sb, kinds, '}', KindString);
                        }
                    } else if (c == '"') {
                        saved.Push((state, holeDepth));
                        state = StString;
                        Append(sb, kinds, ' ', KindString);
                    } else if (c == '\'') {
                        saved.Push((state, holeDepth));
                        state = StCharLiteral;
                        Append(sb, kinds, ' ', KindString);
                    } else {
                        Append(sb, kinds, c, holesAsCode ? KindCode : KindString);
                    }
                    break;
                default:
                    Append(sb, kinds, c, KindCode);
                    break;
            }
            i++;
        }
    }

    /// <summary>
    /// Beendet einen String- oder Char-Zustand und stellt den Zustand vor dem Öffnen wieder her (Code oder Loch).
    /// </summary>
    private void CloseStringState() {
        if (saved.Count > 0) {
            (state, holeDepth) = saved.Pop();
        } else {
            state = StCode;
        }
    }

    private void HandleQuoteStart(StringBuilder? sb, StringBuilder? kinds, string line, ref int i) {
        var run = QuoteRun(line, i);

        if (run >= 3) {
            rawQuotes = run;
            state = StRawString;
            AppendSpaces(sb, kinds, run, KindString);
            i += run - 1;
            return;
        }

        var prev = i > 0 ? line[i - 1] : '\0';
        var prev2 = i > 1 ? line[i - 2] : '\0';
        var isInterp = prev == '$' || (prev == '@' && prev2 == '$');
        var isVerbatim = prev == '@' || (prev == '$' && prev2 == '@');

        if (isInterp) {
            state = isVerbatim ? StInterpVerbatimString : StInterpString;
        } else {
            state = isVerbatim ? StVerbatimString : StString;
        }

        Append(sb, kinds, ' ', KindString);
    }

    private static int QuoteRun(string line, int start) {
        var n = 0;
        while (start + n < line.Length && line[start + n] == '"') { n++; }
        return n;
    }

    private static void Append(StringBuilder? sb, StringBuilder? kinds, char c, char kind) {
        sb?.Append(c);
        kinds?.Append(kind);
    }

    private static void Append(StringBuilder? sb, StringBuilder? kinds, string text, char kind) {
        sb?.Append(text);
        kinds?.Append(kind, text.Length);
    }

    private static void AppendSpaces(StringBuilder? sb, StringBuilder? kinds, int count, char kind) {
        sb?.Append(' ', count);
        kinds?.Append(kind, count);
    }

    #endregion
}
