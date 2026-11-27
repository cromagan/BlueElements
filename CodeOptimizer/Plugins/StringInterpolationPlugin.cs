// Licensed under MIT; see License.md for disclaimer, details, and extended user conditions.

namespace CodeOptimizer.Plugins;

/// <summary>
/// Wandelt String-Konkatenationen mit '+' in interpolierte Strings um, wenn die Kette mit einem einfachen
/// Literal beginnt; Ketten ohne Variablen-Anteil werden zu einem einzelnen Literal verschmolzen.
/// Verbatim-, Raw- und interpolierte Literale sowie komplexe Operanden (z. B. ternäre Ausdrücke) bleiben unangetastet.
/// Vor const-, case- und ähnlichen konstanten Kontexten wird nicht umgebaut.
/// </summary>
public partial class StringInterpolationPlugin : CodeOptimizerPlugin {

    #region Fields

    /// <summary>
    /// Methodennamen, deren Interpolations-Argumente ohne IFormatProvider den CA1305-Fehler auslösen.
    /// </summary>
    private static readonly string[] AnalyzerSensitiveNames = ["AppendLine", "AppendFormat", "Append", "WriteLine", "Write", "Format"];

    /// <summary>
    /// Einfaches String-Literal (nicht verbatim, nicht interpoliert, kein Raw-String).
    /// </summary>
    private const string PlainLiteral = @"""(?:[^""\\\r\n]|\\.)*""";    /// <summary>
    /// Einfaches String-Literal, das nicht direkt hinter @, $ oder einem anderen Literal steht.
    /// </summary>
    private const string GuardedLiteral = @"(?<![@$""])""(?:[^""\\\r\n]|\\.)*""";

    /// <summary>
    /// Argumentliste mit ausgeglichener Klammerung; verträgt String-Literale und eine weitere Klammer-Ebene.
    /// </summary>
    private const string BalancedArgs = @"(?:" + GuardedLiteral + @"|[^()""]|\((?:[^()]|\([^()]*\))*\))*";

    /// <summary>
    /// Einfacher Empfänger: Bezeichner-Kette mit Indexern und Null-Conditional-Zeichen.
    /// </summary>
    private const string Receiver = @"(?:[\w.\[\]]+\??\.)*[\w.\[\]]+";

    /// <summary>
    /// Aufruf-Kette: Klammern direkt oder nach Punkt bzw. Null-Conditional.
    /// </summary>
    private const string CallTail = @"(?:(?:\??\.\w+)?\(" + BalancedArgs + @"\))*";

    /// <summary>
    /// Erlaubter Operand eines Interpolationslochs: Empfänger optional mit Aufruf-Kette.
    /// </summary>
    private const string Operand = Receiver + CallTail;

    #endregion

    #region Properties

    public override string Name => "String-Interpolation";

    public override ImageCode Symbol => ImageCode.Gänsefüßchen;

    public override string QuickInfo => "Wandelt \"text \" + variable + \" text\" in $\"text {variable} text\" um; reine Literal-Ketten werden zu einem Literal verschmolzen.";

    public override int Order => 35;

    #endregion

    #region Methods

    public override string Execute(string rootPath) {
        var interpolated = 0;
        var merged = 0;
        var totalFiles = 0;
        var skipped = new List<string>();

        foreach (var file in SourceFiles.Enumerate(rootPath)) {
            // Dateien ohne gültiges UTF-8 dürfen nicht umgeschrieben werden - das übernimmt das Encoding-Modul
            if (!SourceFiles.IsUtf8(file)) {
                skipped.Add(file);
                continue;
            }

            var content = IO.ReadAllText(file);
            var original = content;
            var kinds = new CsSanitizer().SanitizeKinds(content);

            // Lambda nötig: der Evaluator braucht Zugriff auf kinds und die Zähler
            content = ChainRegex().Replace(content, m => {
                if (!CsSanitizer.IsCodeWithLiteral(kinds, m)) { return m.Value; }

                // Trenner-Plus müssen echter Code sein; ein Plus aus einem String-Inhalt ist ein Pseudo-Treffer
                foreach (Capture p in m.Groups["plus"].Captures) {
                    if (kinds[p.Index] != CsSanitizer.KindCode) { return m.Value; }
                }

                // Analyzer-sensible Aufrufe (AppendLine, Write, ...) bleiben Konkatenation, sonst löst CA1305 aus
                if (IsAnalyzerSensitiveCall(content, kinds, m.Index)) { return m.Value; }

                var replacement = Fold(m);

                if (replacement == m.Value) { return m.Value; }

                if (replacement[0] == '$') { interpolated++; } else { merged++; }

                return replacement;
            });

            if (content == original) { continue; }

            File.WriteAllText(file, content, new UTF8Encoding(true));
            totalFiles++;
        }

        var report = new StringBuilder();
        report.AppendLine("=== String-Interpolation ===");
        report.AppendLine(CultureInfo.InvariantCulture, $"Konkatenationen interpoliert: {interpolated}");
        report.AppendLine(CultureInfo.InvariantCulture, $"Literale verschmolzen:        {merged}");
        report.AppendLine(CultureInfo.InvariantCulture, $"Dateien geändert:             {totalFiles}");
        AppendSkipped(report, skipped);
        return report.ToString();
    }

    /// <summary>
    /// Baut den Ersatz für eine Konkatenations-Kette: interpoliertes Literal oder verschmolzenes Literal ohne Variablen-Anteil.
    /// </summary>
    private static string Fold(Match m) {
        var parts = m.Groups["part"].Captures;
        var hasOperand = false;

        for (var i = 0; i < parts.Count; i++) {
            if (parts[i].Value[0] != '"') { hasOperand = true; break; }
        }

        var result = new StringBuilder(m.Groups["first"].Value[1..^1]);

        if (!hasOperand) {
            for (var i = 0; i < parts.Count; i++) {
                result.Append(parts[i].Value[1..^1]);
            }
            result.Insert(0, "\"").Append('"');
            return result.ToString();
        }

        result.Replace("{", "{{").Replace("}", "}}").Insert(0, "$\"");

        for (var i = 0; i < parts.Count; i++) {
            var part = parts[i].Value;

            if (part[0] == '"') {
                result.Append(EscapeBraces(part[1..^1]));
                continue;
            }

            if (!IsSafeOperand(part)) { return m.Value; }

            result.Append('{').Append(part).Append('}');
        }

        result.Append('"');
        return result.ToString();
    }

    /// <summary>
    /// Verdoppelt geschweifte Klammern im Textanteil eines interpolierten Literals.
    /// </summary>
    private static string EscapeBraces(string text) => text.Replace("{", "{{").Replace("}", "}}");

    /// <summary>
    /// Liefert true, wenn der Treffer direkt Argument eines Analyzer-sensiblen Aufrufs ist;
    /// dort bleibt die Konkatenation stehen, damit CA1305 nicht auslöst.
    /// </summary>
    private static bool IsAnalyzerSensitiveCall(string content, string kinds, int index) {
        var b = index - 1;
        while (b >= 0 && char.IsWhiteSpace(content[b])) { b--; }

        foreach (var name in AnalyzerSensitiveNames) {
            var start = b - name.Length;

            if (start < 0 || content[start..b] != name) { continue; }
            if (kinds[start] != CsSanitizer.KindCode) { continue; }
            if (start > 0 && IsWordChar(content[start - 1])) { continue; }

            return true;
        }

        return false;
    }

    /// <summary>
    /// Liefert true, wenn das Zeichen ein Wortzeichen (Buchstabe, Ziffer, Unterstrich) ist.
    /// </summary>
    private static bool IsWordChar(char c) => char.IsLetterOrDigit(c) || c == '_';

    /// <summary>
    /// Liefert true, wenn der Operand in einem Interpolationsloch stehen darf: ausgeglichene Klammern und
    /// kein top-level Doppelpunkt (wäre als Format gelesen).
    /// </summary>
    private static bool IsSafeOperand(string operand) {
        var depth = 0;
        var inString = false;
        var i = 0;

        while (i < operand.Length) {
            var c = operand[i];
            i++;

            if (inString) {
                if (c == '\\') { i++; } else if (c == '"') { inString = false; }
                continue;
            }

            switch (c) {
                case '"':
                    inString = true;
                    break;
                case '(' or '[':
                    depth++;
                    break;
                case ')' or ']':
                    depth--;
                    if (depth < 0) { return false; }
                    break;
                case '{' or '}' or ';':
                    return false;
                case ':' when depth == 0:
                    return false;
            }
        }

        return depth == 0;
    }

    [GeneratedRegex(@"(?<!const[ \t\r\n]+)(?<!case[ \t\r\n]+)(?<![@$""])(?<first>" + PlainLiteral + @")(?:[ \t\r\n]*(?<plus>\+)[ \t\r\n]*(?>(?<part>" + GuardedLiteral + @"|" + Operand + @")))+(?!\+\+)(?!--)(?![ \t\r\n]*[.\[<(?])(?![ \t\r\n]*!(?!=))")]
    private static partial Regex ChainRegex();

    #endregion
}
