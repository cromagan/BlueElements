// Licensed under MIT; see License.md for disclaimer, details, and extended user conditions.

namespace CodeOptimizer.Classes;

/// <summary>
/// Ersetzt in C#-Quelltext die Inhalte von Kommentaren und Strings durch Leerzeichen,
/// damit Klammer-Tiefen und Token-Suchen textbasiert ohne Störung durch String-Inhalte laufen.
/// </summary>
public sealed class CsSanitizer {

    #region Fields

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
            SanitizeInto(sb, line);
            result.Add(sb.ToString());
            if (state == StLineComment) { state = StCode; }
        }
        return result;
    }

    /// <summary>
    /// Bereinigt einen zusammenhängenden Textabschnitt (z. B. einen Member-Kopf).
    /// </summary>
    public string Sanitize(string text) {
        var sb = new StringBuilder(text.Length);
        foreach (var part in text.Split(["\r\n", "\n"], StringSplitOptions.None)) {
            SanitizeInto(sb, part);
            sb.Append(' ');
            if (state == StLineComment) { state = StCode; }
        }
        return sb.ToString();
    }

    private void SanitizeInto(StringBuilder sb, string line) {
        var i = 0;
        while (i < line.Length) {
            var c = line[i];
            var next = i + 1 < line.Length ? line[i + 1] : '\0';
            switch (state) {
                case StCode:
                    if (c == '/' && next == '/') {
                        state = StLineComment;
                        sb.Append("  ");
                        i++;
                    } else if (c == '/' && next == '*') {
                        state = StBlockComment;
                        sb.Append("  ");
                        i++;
                    } else if (c == '"') {
                        HandleQuoteStart(sb, line, ref i);
                    } else if (c == '\'') {
                        state = StCharLiteral;
                        sb.Append(' ');
                    } else {
                        sb.Append(c);
                    }
                    break;
                case StLineComment:
                    sb.Append(' ');
                    break;
                case StBlockComment:
                    if (c == '*' && next == '/') {
                        state = StCode;
                        sb.Append("  ");
                        i++;
                    } else {
                        sb.Append(' ');
                    }
                    break;
                case StString:
                    if (c == '\\') {
                        sb.Append("  ");
                        i++;
                    } else if (c == '"') {
                        state = StCode;
                        sb.Append(' ');
                    } else {
                        sb.Append(' ');
                    }
                    break;
                case StVerbatimString:
                    if (c == '"' && next == '"') {
                        sb.Append("  ");
                        i++;
                    } else if (c == '"') {
                        state = StCode;
                        sb.Append(' ');
                    } else {
                        sb.Append(' ');
                    }
                    break;
                case StRawString:
                    if (c == '"' && QuoteRun(line, i) == rawQuotes) {
                        sb.Append(' ', rawQuotes);
                        i += rawQuotes - 1;
                        state = StCode;
                    } else {
                        sb.Append(' ');
                    }
                    break;
                case StCharLiteral:
                    if (c == '\\') {
                        sb.Append("  ");
                        i++;
                    } else if (c == '\'') {
                        state = StCode;
                        sb.Append(' ');
                    } else {
                        sb.Append(' ');
                    }
                    break;
                case StInterpString:
                case StInterpVerbatimString:
                    if ((c == '\\' && state == StInterpString) || (c == '"' && state == StInterpVerbatimString && next == '"') || (c == '{' && next == '{')) {
                        sb.Append("  ");
                        i++;
                    } else if (c == '"') {
                        state = StCode;
                        sb.Append(' ');
                    } else if (c == '{') {
                        saved.Push((state, holeDepth));
                        holeDepth = 0;
                        state = StHole;
                        sb.Append(' ');
                    } else {
                        sb.Append(' ');
                    }
                    break;
                case StHole:
                    if (c == '{') {
                        holeDepth++;
                        sb.Append('{');
                    } else if (c == '}') {
                        if (holeDepth == 0) {
                            (state, holeDepth) = saved.Pop();
                            sb.Append(' ');
                        } else {
                            holeDepth--;
                            sb.Append('}');
                        }
                    } else if (c == '"') {
                        saved.Push((state, holeDepth));
                        state = StString;
                        sb.Append(' ');
                    } else if (c == '\'') {
                        saved.Push((state, holeDepth));
                        state = StCharLiteral;
                        sb.Append(' ');
                    } else {
                        sb.Append(c);
                    }
                    break;
                default:
                    sb.Append(c);
                    break;
            }
            i++;
        }
    }

    private void HandleQuoteStart(StringBuilder sb, string line, ref int i) {
        var run = QuoteRun(line, i);

        if (run >= 3) {
            rawQuotes = run;
            state = StRawString;
            sb.Append(' ', run);
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

        sb.Append(' ');
    }

    private static int QuoteRun(string line, int start) {
        var n = 0;
        while (start + n < line.Length && line[start + n] == '"') { n++; }
        return n;
    }

    #endregion
}
