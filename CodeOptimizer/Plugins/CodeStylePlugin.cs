// Licensed under MIT; see License.md for disclaimer, details, and extended user conditions.

namespace CodeOptimizer.Plugins;

/// <summary>
/// Führt die Code-Stil-Transformationen aus: leere Klammern, einzeilige If-Rückgaben,
/// is-null-Muster, redundante ToLower/ToUpper-Ketten, Groß/Klein-unabhängige String-Vergleiche,
/// Summary-Tags, Doc-Kommentar-Einzug, Substring zu Range-Syntax, null-sichere Equals,
/// Format-Strings zu Numeral/Date-Extensions, File.ReadAllText zu IO.ReadAllText
/// und If-Null-Zuweisungen zu Null-Conditional-Zuweisungen.
/// </summary>
public partial class CodeStylePlugin : CodeOptimizerPlugin {

    #region Fields

    private static readonly string[] _keywords = ["if", "for", "while", "switch", "catch", "using", "lock", "foreach", "return", "new", "typeof", "sizeof", "checked", "unchecked", "nameof", "default", "base", "this"];

    /// <summary>
    /// Einfache Anweisungen, die in einzeilige If-Blöcke umgewandelt werden dürfen.
    /// </summary>
    private const string SimpleStatement = @"(?:return\b(?:\s+(?:true|false))?\s*;|(?:continue|break)\s*;)";

    private const string IfHead = @"\b(if\s*\((?:[^()]|\((?:[^()]|\([^()]*\))*\))*\))";

    private const string DisposableTypes = "Table|RowItem|ColumnItem|GenericControl|RowCollection|ColumnCollection|FilterCollection|ColumnViewCollection|CellCollection|ColumnViewItem|ConnectedFormula|AbstractPadItem|AbstractListItem|BlueFont|ExtText|PointM|ExtChar|ScriptDescription|FlexiStrategyBase|CachedFile|BlockableFile|PictureView|QuickPicSelector|Row|Column|Filter";

    /// <summary>
    /// ToLower/ToUpper-Aufruf in beliebiger Schreibweise.
    /// </summary>
    private const string CaseFoldCall = @"\.(?:ToLowerInvariant|ToLower|ToUpperInvariant|ToUpper)\(\)";

    /// <summary>
    /// Optionales .Trim() zwischen Fold und Vergleich, als erfassbare Gruppe.
    /// </summary>
    private const string TrimCapture = @"(?:(\s*\.\s*Trim\(\)))?";

    /// <summary>
    /// Einfacher Empfänger: Punktkette mit optionalen Indexern und Null-Conditional-Zeichen.
    /// </summary>
    private const string SimpleReceiver = @"(?:[\w.\[\]]+\??\.)*[\w.\[\]]+\??";

    /// <summary>
    /// Empfänger ohne Indexer und ohne Null-Conditional, damit die Null-Prüfung sicher durch ?. ersetzt werden darf.
    /// </summary>
    private const string PlainReceiver = @"[A-Za-z_]\w*(?:\.[A-Za-z_]\w*)*";

    /// <summary>
    /// If (x is not null) { x.M = v; } einzeilig als Null-Conditional-Zuweisung.
    /// </summary>
    private const string NullAssignLinePattern = @"\bif\s*\(\s*(" + PlainReceiver + @")\s+is\s+not\s+null\s*\)\s*\{[ \t]*\1\.(" + PlainReceiver + @")\s*(?<![+*/%&|^<>!=?-])=\s*([^;{}\r\n]+);[ \t]*\}(?![ \t]*(?:\r?\n[ \t]*)?else\b)";

    /// <summary>
    /// If (x is not null) { x.M = v; } mehrzeilig als Null-Conditional-Zuweisung.
    /// </summary>
    private const string NullAssignBlockPattern = @"\bif\s*\(\s*(" + PlainReceiver + @")\s+is\s+not\s+null\s*\)\s*\{\s*\r?\n[ \t]*\1\.(" + PlainReceiver + @")\s*(?<![+*/%&|^<>!=?-])=\s*([^;{}\r\n]+);[ \t]*\r?\n[ \t]*\}(?![ \t]*(?:\r?\n[ \t]*)?else\b)";

    /// <summary>
    /// String-Literal, normal oder verbatim.
    /// </summary>
    private const string StringLiteral = @"(?:@""(?:[^""\r\n]|"""")*"")|(?:""(?:[^""\\\r\n]|\\.)*"")";

    /// <summary>
    /// String-Methoden, die case-insensitive über StringComparison arbeiten dürfen.
    /// </summary>
    private const string ComparisonMethods = @"(?:LastIndexOf|IndexOf|Equals|EndsWith|StartsWith|Contains)";

    /// <summary>
    /// Operand eines Groß/Klein-Vergleichs: Literal oder einfacher Empfänger, keine Schlüsselwort-Operanden.
    /// </summary>
    private const string CompareOperand = @"((?:" + StringLiteral + @")|(?!null\b|true\b|false\b)" + SimpleReceiver + @")";

    /// <summary>
    /// Argument eines Substring-Aufrufs: ohne Komma, Klammern und Zeilenumbrüche, damit verschachtelte Aufrufe unangetastet bleiben.
    /// </summary>
    private const string SubstringArg = @"[^\r\n(),]+";

    /// <summary>
    /// Zuordnung exakter Zahlen-Format-Strings zu NumeralExtensions-Methoden.
    /// </summary>
    private static readonly Dictionary<string, string> NumeralFormatMap = new() {
        ["0"] = "ToString1",
        ["0.#"] = "ToString1_1",
        ["0.##"] = "ToString1_2",
        ["0.###"] = "ToString1_3",
        ["0.#####"] = "ToString1_5",
        ["00"] = "ToString2",
        ["000"] = "ToString3",
        ["0000"] = "ToString4",
        ["00000"] = "ToString5",
        ["0000000"] = "ToString7",
        ["0000000000"] = "ToString10",
        ["0000000000.###"] = "ToString10_3"
    };

    /// <summary>
    /// Zuordnung exakter Datums-Format-Strings zu DateExtensions-Methoden.
    /// </summary>
    private static readonly Dictionary<string, string> DateFormatMap = new() {
        ["yyyy/MM/dd HH:mm:ss.fff"] = "ToString1",
        ["yyyy/MM/dd"] = "ToString3",
        ["yyyy-MM-dd_HH-mm-ss"] = "ToString4",
        ["dd.MM.yyyy HH:mm:ss"] = "ToString5",
        ["dd.MM.yyyy"] = "ToString6",
        ["dd.MM.yyyy HH:mm:ss.fff"] = "ToString7",
        ["yyyy-MM-dd HH:mm:ss.fff"] = "ToString9"
    };

    #endregion

    #region Properties

    public override string Name => "Code-Stil";
    public override ImageCode Symbol => ImageCode.Zahnrad;
    public override string QuickInfo => "Wendet die Code-Stil-Regeln an: leere Klammern, einzeilige If-Rückgaben, is-null-Muster, ToLower/ToUpper-Ketten, Groß/Klein-Vergleiche, Summary-Tags, Doc-Kommentar-Einzug, Substring zu Range, null-sichere Equals, Format-Strings zu Extensions, File.ReadAllText zu IO und If-Null-Zuweisungen zu ?.";
    public override int Order => 30;

    #endregion

    #region Methods

    public override string Execute(string rootPath) {
        var braces = 0;
        var ifOneLine = 0;
        var parenSpace = 0;
        var neNull = 0;
        var eqNull = 0;
        var isNotBrace = 0;
        var isBrace = 0;
        var dispNot = 0;
        var disp = 0;
        var summaryOpen = 0;
        var summaryClose = 0;
        var docIndent = 0;
        var foldChain = 0;
        var eqFold = 0;
        var eqFoldRev = 0;
        var substr = 0;
        var eqInst = 0;
        var fmtExt = 0;
        var readIo = 0;
        var nullAssign = 0;
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

            // 1. Leere Klammern über zwei Zeilen -> eine Zeile
            var prev = content;
            content = CsSanitizer.ReplaceInCode(content, EmptyBracesRegex(), CollapseEmptyBraces);
            var c1 = content != prev;

            // 3. Einfaches return;/return true;/return false;/continue;/break; in If ohne Klammern -> If() { ...; }
            prev = content;
            content = CsSanitizer.ReplaceInCode(content, IfSimpleStatementRegex(), AddBracesToSimpleStatement);
            var c3 = content != prev;

            // 3b. If-Block mit nur einer einfachen Anweisung -> eine Zeile
            prev = content;
            content = CsSanitizer.ReplaceInCode(content, IfSimpleStatementBlockRegex(), AddBracesToSimpleStatement);
            c3 = c3 || content != prev;

            // 4. Leerzeichen nach ( bei Methodenaufrufen entfernen
            prev = content;
            content = CsSanitizer.ReplaceInCode(content, ParenSpaceRegex(), RemoveParenSpace);
            var c4 = content != prev;

            // 5. is not null -> is not null
            prev = content;
            content = CsSanitizer.ReplaceInCode(content, NotNullRegex(), static _ => " is not null");
            var c5 = content != prev;

            // 6. is null -> is null
            prev = content;
            content = CsSanitizer.ReplaceInCode(content, NullEqualsRegex(), static _ => " is null");
            var c6 = content != prev;

            // 7a. (Disposable) is not { } -> is not { IsDisposed: false }
            prev = content;
            content = CsSanitizer.ReplaceInCode(content, DisposableNotBraceRegex(), static m => m.Result("$1 is not { IsDisposed: false }"));
            var c7a = content != prev;

            // 7b. (Disposable) is { } -> is { IsDisposed: false }
            prev = content;
            content = CsSanitizer.ReplaceInCode(content, DisposableBraceRegex(), static m => m.Result("$1 is { IsDisposed: false }"));
            var c7b = content != prev;

            // 7. is not { } -> is null (nur vor ) & |)
            prev = content;
            content = CsSanitizer.ReplaceInCode(content, IsNotBraceRegex(), static _ => "is null");
            var c7 = content != prev;

            // 8. is { } -> is not null (nur vor ) & |)
            prev = content;
            content = CsSanitizer.ReplaceInCode(content, IsBraceRegex(), static _ => "is not null");
            var c8 = content != prev;

            // 9a. /// <summary> mit Text -> Tag in eigene Zeile
            prev = content;
            var kinds = new CsSanitizer().SanitizeKinds(content);
            content = SummaryOpenRegex().Replace(content, m => CsSanitizer.ContainsString(kinds, m.Index, m.Length) ? m.Value : SplitSummaryOpen(m));
            var c9a = content != prev;

            // 9b. Text + </summary> am Zeilenende -> Tag in eigene Zeile
            prev = content;
            kinds = new CsSanitizer().SanitizeKinds(content);
            content = SummaryCloseRegex().Replace(content, m => CsSanitizer.ContainsString(kinds, m.Index, m.Length) ? m.Value : SplitSummaryClose(m));
            var c9b = content != prev;

            // 10. Einheitliche Einrückung von ///-Blöcken (an der Folgezeile ausgerichtet)
            prev = content;
            kinds = new CsSanitizer().SanitizeKinds(content);
            content = DocIndentRegex().Replace(content, m => CsSanitizer.ContainsString(kinds, m.Index, m.Length) ? m.Value : FixDocIndent(m));
            var c10 = content != prev;

            // 11. Redundante ToLower/ToUpper-Kette vor EndsWith/StartsWith/Contains/IndexOf/Equals -> StringComparison.OrdinalIgnoreCase
            prev = content;
            kinds = new CsSanitizer().SanitizeKinds(content);
            content = CaseFoldChainRegex().Replace(content, m => CsSanitizer.IsCodeWithLiteral(kinds, m) ? FoldChain(m) : m.Value);
            var c11 = content != prev;

            // 12. x.ToLowerInvariant() == "text" -> string.Equals(x, "text", StringComparison.OrdinalIgnoreCase)
            prev = content;
            kinds = new CsSanitizer().SanitizeKinds(content);
            content = CaseFoldCompareRegex().Replace(content, m => CsSanitizer.IsCodeWithLiteral(kinds, m) ? FoldCompare(m) : m.Value);
            var c12 = content != prev;

            // 13. "text" == x.ToLowerInvariant() -> string.Equals(x, "text", StringComparison.OrdinalIgnoreCase)
            prev = content;
            kinds = new CsSanitizer().SanitizeKinds(content);
            content = CaseFoldCompareReversedRegex().Replace(content, m => CsSanitizer.IsCodeWithLiteral(kinds, m) ? FoldCompareReversed(m) : m.Value);
            var c13 = content != prev;

            // 14. .Substring(a, b) -> Range-Syntax
            prev = content;
            content = CsSanitizer.ReplaceInCode(content, SubstringRegex(), FoldSubstring);
            var c14 = content != prev;

            // 15. x.Equals(y, StringComparison.X) -> string.Equals(x, y, StringComparison.X)
            prev = content;
            content = CsSanitizer.ReplaceInCode(content, InstanceEqualsRegex(), FoldInstanceEquals);
            var c15 = content != prev;

            // 16. .ToString("format") -> Numeral/Date-Extensions; in den Definitions-Dateien selbst nicht anwenden
            prev = content;
            if (!DefinesFormatExtensions(file)) {
                content = CsSanitizer.ReplaceInCode(content, ToStringFormatRegex(), FoldToStringFormat);
            }
            var c16 = content != prev;

            // 17. File.ReadAllText -> IO.ReadAllText (Retry + Cache); in IO.cs selbst nicht anwenden
            prev = content;
            if (Path.GetFileName(file) is not "IO.cs") {
                content = CsSanitizer.ReplaceInCode(content, IoReadTextRegex(), static _ => "IO.ReadAllText(");
            }
            var c17 = content != prev;

            // 18. If (x is not null) { x.M = v; } -> x?.M = v; (Null-Conditional-Zuweisung)
            prev = content;
            kinds = new CsSanitizer().SanitizeKinds(content);
            content = NullAssignLineRegex().Replace(content, m => CsSanitizer.IsCodeWithLiteral(kinds, m) ? FoldNullAssign(m) : m.Value);
            content = NullAssignBlockRegex().Replace(content, m => CsSanitizer.IsCodeWithLiteral(kinds, m) ? FoldNullAssign(m) : m.Value);
            var c18 = content != prev;

            if (c1) { braces++; }
            if (c3) { ifOneLine++; }
            if (c4) { parenSpace++; }
            if (c5) { neNull++; }
            if (c6) { eqNull++; }
            if (c7) { isNotBrace++; }
            if (c8) { isBrace++; }
            if (c7a) { dispNot++; }
            if (c7b) { disp++; }
            if (c9a) { summaryOpen++; }
            if (c9b) { summaryClose++; }
            if (c10) { docIndent++; }
            if (c11) { foldChain++; }
            if (c12) { eqFold++; }
            if (c13) { eqFoldRev++; }
            if (c14) { substr++; }
            if (c15) { eqInst++; }
            if (c16) { fmtExt++; }
            if (c17) { readIo++; }
            if (c18) { nullAssign++; }

            if (content == original) { continue; }

            File.WriteAllText(file, content, new UTF8Encoding(true));
            totalFiles++;
        }

        var report = new StringBuilder();
        report.AppendLine("=== Code-Stil ===");
        report.AppendLine(CultureInfo.InvariantCulture, $"Leere Klammern eingekürzt:  {braces}");
        report.AppendLine(CultureInfo.InvariantCulture, $"If-Rückgaben einzeilig:     {ifOneLine}");
        report.AppendLine(CultureInfo.InvariantCulture, $"Klammer-Leerzeichen:        {parenSpace}");
        report.AppendLine(CultureInfo.InvariantCulture, $" is not null -> is not null:      {neNull}");
        report.AppendLine(CultureInfo.InvariantCulture, $" is null -> is null:          {eqNull}");
        report.AppendLine(CultureInfo.InvariantCulture, $"is not {{ }} -> is null:     {isNotBrace}");
        report.AppendLine(CultureInfo.InvariantCulture, $"is {{ }} -> is not null:     {isBrace}");
        report.AppendLine(CultureInfo.InvariantCulture, $"Disposable is not {{ }}:     {dispNot}");
        report.AppendLine(CultureInfo.InvariantCulture, $"Disposable is {{ }}:         {disp}");
        report.AppendLine(CultureInfo.InvariantCulture, $"Summary-Tags aufgetrennt:   {summaryOpen + summaryClose}");
        report.AppendLine(CultureInfo.InvariantCulture, $"Doc-Einzug korrigiert:      {docIndent}");
        report.AppendLine(CultureInfo.InvariantCulture, $"ToLower/ToUpper-Ketten:     {foldChain}");
        report.AppendLine(CultureInfo.InvariantCulture, $"x == \"text\" -> string.Equals: {eqFold}");
        report.AppendLine(CultureInfo.InvariantCulture, $"\"text\" == x -> string.Equals: {eqFoldRev}");
        report.AppendLine(CultureInfo.InvariantCulture, $"Substring -> Range:         {substr}");
        report.AppendLine(CultureInfo.InvariantCulture, $"Equals -> string.Equals:    {eqInst}");
        report.AppendLine(CultureInfo.InvariantCulture, $"Format -> Extensions:       {fmtExt}");
        report.AppendLine(CultureInfo.InvariantCulture, $"File.ReadAllText -> IO:     {readIo}");
        report.AppendLine(CultureInfo.InvariantCulture, $"If-Zuweisung -> ?.:         {nullAssign}");
        report.AppendLine(CultureInfo.InvariantCulture, $"Dateien geändert:           {totalFiles}");
        AppendSkipped(report, skipped);
        return report.ToString();
    }

    private static string CollapseEmptyBraces(Match m) => m.Groups[1].Success ? " { }" : "{ }";

    private static string AddBracesToSimpleStatement(Match m) => $"{m.Groups[1].Value} {{ {m.Groups[2].Value} }}";

    private static string RemoveParenSpace(Match m) => _keywords.Contains(m.Groups[1].Value) ? m.Value : m.Groups[1].Value + "(";

    private static string SplitSummaryOpen(Match m) {
        var indent = m.Groups[1].Value;
        var text = m.Groups[2].Value;

        if (text is not { Length: > 0 }) { return m.Value; }

        if (m.Groups[3].Success) {
            return $"{indent}/// <summary>\r\n{indent}/// {text}\r\n{indent}/// </summary>";
        }

        return $"{indent}/// <summary>\r\n{indent}/// {text}";
    }

    private static string SplitSummaryClose(Match m) => $"{m.Groups[1].Value}/// {m.Groups[2].Value}\r\n{m.Groups[1].Value}/// </summary>";

    private static string FixDocIndent(Match m) {
        var indent = m.Groups[1].Value;
        var lines = m.Value.Split(["\r\n", "\n"], StringSplitOptions.None);

        for (var i = 0; i < lines.Length; i++) {
            lines[i] = indent + lines[i].TrimStart(' ', '\t');
        }

        return string.Join("\r\n", lines);
    }

    /// <summary>
    /// Baut den Ersatz für eine ToLower/ToUpper-Kette vor einer Vergleichsmethode.
    /// </summary>
    private static string FoldChain(Match m) => m.Groups[1].Value + "." + m.Groups[2].Value + "(" + m.Groups[3].Value + ", StringComparison.OrdinalIgnoreCase)";

    /// <summary>
    /// Baut den Ersatz für x.ToLowerInvariant() == "text".
    /// </summary>
    private static string FoldCompare(Match m) {
        var receiver = m.Groups[1].Value;

        if (!IsCompleteReceiver(receiver)) { return m.Value; }

        if (receiver.EndsWith('?')) {
            // Das Null-Conditional bleibt nur vor einem folgenden .Trim() erhalten; string.Equals selbst ist null-sicher
            receiver = receiver[..^1] + (m.Groups[2].Success ? "?.Trim()" : string.Empty);
        } else if (m.Groups[2].Success) {
            receiver += ".Trim()";
        }

        return (m.Groups[3].Value == "!=" ? "!" : "") + "string.Equals(" + receiver + ", " + m.Groups[4].Value + ", StringComparison.OrdinalIgnoreCase)";
    }

    /// <summary>
    /// Baut den Ersatz für "text" == x.ToLowerInvariant().
    /// </summary>
    private static string FoldCompareReversed(Match m) {
        var receiver = m.Groups[3].Value;

        if (!IsCompleteReceiver(receiver)) { return m.Value; }

        if (receiver.EndsWith('?')) {
            receiver = receiver[..^1] + (m.Groups[4].Success ? "?.Trim()" : string.Empty);
        } else if (m.Groups[4].Success) {
            receiver += ".Trim()";
        }

        return (m.Groups[2].Value == "!=" ? "!" : "") + "string.Equals(" + receiver + ", " + m.Groups[1].Value + ", StringComparison.OrdinalIgnoreCase)";
    }

    /// <summary>
    /// Baut die Range-Syntax für einen Substring-Aufruf.
    /// </summary>
    private static string FoldSubstring(Match m) {
        var start = m.Groups[1].Value.Trim();
        var length = m.Groups[2].Success ? m.Groups[2].Value.Trim() : string.Empty;

        if (length is not { Length: > 0 }) { return start == "0" ? "[..]" : "[" + start + "..]"; }

        if (start == "0") { return "[.." + length + "]"; }

        return "[" + start + "..(" + start + " + " + length + ")]";
    }

    /// <summary>
    /// Prüft, dass ein erfasster Empfänger ein vollständiger Ausdruck ist und kein Fragment eines größeren Ausdrucks.
    /// </summary>
    private static bool IsCompleteReceiver(string receiver) {
        if (!char.IsLetter(receiver[0]) && receiver[0] != '_') { return false; }

        var depth = 0;
        foreach (var c in receiver) {
            if (c == '[') { depth++; }
            if (c == ']') { depth--; }
        }
        return depth == 0;
    }

    /// <summary>
    /// Baut den Ersatz für x.Equals(y, StringComparison.X); der statische Aufruf ist zusätzlich null-sicher.
    /// </summary>
    private static string FoldInstanceEquals(Match m) {
        var receiver = m.Groups[1].Value;

        if (!IsCompleteReceiver(receiver)) { return m.Value; }

        // Statische Equals-Aufrufe nicht erneut umschreiben
        if (receiver is "string" or "StringComparison") { return m.Value; }

        // Das Null-Conditional entfällt, da string.Equals null verträglich ist
        if (receiver.EndsWith('?')) { receiver = receiver[..^1]; }

        return "string.Equals(" + receiver + ", " + m.Groups[2].Value + ", StringComparison." + m.Groups[3].Value + ")";
    }

    /// <summary>
    /// Baut den Ersatz für If (x is not null) { x.M = v; } als Null-Conditional-Zuweisung.
    /// </summary>
    private static string FoldNullAssign(Match m) => m.Groups[1].Value + "?." + m.Groups[2].Value + " = " + m.Groups[3].Value.TrimEnd() + ";";

    /// <summary>
    /// Baut den Ersatz für .ToString("format"), wenn das Format exakt in einer Mapping-Tabelle steht.
    /// </summary>
    private static string FoldToStringFormat(Match m) {
        var receiver = m.Groups[1].Value;
        var format = m.Groups[2].Value;
        var invariant = m.Groups[3].Success;

        if (!IsCompleteReceiver(receiver)) { return m.Value; }

        // Null-Conditional-Empfänger bleiben stehen, da die Extensions nicht null-verträglich sind
        if (receiver.EndsWith('?')) { return m.Value; }

        // Ohne InvariantCulture nur Formate ohne kulturabhängige Separatoren (Datum: / und :, Zahl: .)
        if (DateFormatMap.TryGetValue(format, out var dateMethod) && (invariant || (!format.Contains('/') && !format.Contains(':')))) {
            return receiver + "." + dateMethod + "()";
        }

        if (NumeralFormatMap.TryGetValue(format, out var numeralMethod) && (invariant || !format.Contains('.'))) {
            return receiver + "." + numeralMethod + "()";
        }

        return m.Value;
    }

    /// <summary>
    /// Liefert true für die Definitions-Dateien der Numeral- und Date-Extensions, damit deren Format-Strings nicht auf sich selbst abgebildet werden.
    /// </summary>
    private static bool DefinesFormatExtensions(string file) => file.EndsWith("NumeralExtensions.cs", StringComparison.OrdinalIgnoreCase) || file.EndsWith("DateExtensions.cs", StringComparison.OrdinalIgnoreCase);

    [GeneratedRegex(@"(\r?\n[ \t]*)?\{[ \t]*\r?\n(?:[ \t]*\r?\n)*[ \t]*\}")]
    private static partial Regex EmptyBracesRegex();

    [GeneratedRegex(IfHead + @"\s*(" + SimpleStatement + ")")]
    private static partial Regex IfSimpleStatementRegex();

    [GeneratedRegex(IfHead + @"\s*\{[ \t]*\r?\n[ \t]*(" + SimpleStatement + @")[ \t]*\r?\n[ \t]*\}")]
    private static partial Regex IfSimpleStatementBlockRegex();

    [GeneratedRegex(@"\b(\w+)\(([ \t]+)")]
    private static partial Regex ParenSpaceRegex();

    [GeneratedRegex(@"\s*!=\s*null\b")]
    private static partial Regex NotNullRegex();

    [GeneratedRegex(@"\s*==\s*null\b")]
    private static partial Regex NullEqualsRegex();

    [GeneratedRegex($@"\b({DisposableTypes})\s+is\s+not\s*\{{\s*\}}")]
    private static partial Regex DisposableNotBraceRegex();

    [GeneratedRegex($@"\b({DisposableTypes})\s+is\s*\{{\s*\}}")]
    private static partial Regex DisposableBraceRegex();

    [GeneratedRegex(@"is\s+not\s*\{\s*\}(?=\s*[)&|;,?])")]
    private static partial Regex IsNotBraceRegex();

    [GeneratedRegex(@"is\s*\{\s*\}(?=\s*[)&|;,?])")]
    private static partial Regex IsBraceRegex();

    [GeneratedRegex(@"(?m)^([ \t]*)///[ \t]*<summary>[ \t]*(.*?)[ \t]*(</summary>)?[ \t]*(?=\r?\n)")]
    private static partial Regex SummaryOpenRegex();

    [GeneratedRegex(@"(?m)^([ \t]*)///[ \t]*(.*?\S)[ \t]*</summary>[ \t]*(?=\r?\n)")]
    private static partial Regex SummaryCloseRegex();

    [GeneratedRegex(@"(?m)^[ \t]*///[^\r\n]*(?:\r?\n[ \t]*///[^\r\n]*)*(?=\r?\n([ \t]*)\S)")]
    private static partial Regex DocIndentRegex();

    [GeneratedRegex(CaseFoldCall + TrimCapture + @"\s*\.\s*(" + ComparisonMethods + @")\(\s*(" + StringLiteral + @")\s*(?:,\s*StringComparison\.\w+)?\s*\)")]
    private static partial Regex CaseFoldChainRegex();

    [GeneratedRegex(@"(?<![\w.\[\]\^])(?<!\+\s*)\b(" + SimpleReceiver + @")" + CaseFoldCall + TrimCapture + @"\s*(==|!=)\s*" + CompareOperand + @"(?![ \t]*[.+(?])")]
    private static partial Regex CaseFoldCompareRegex();

    [GeneratedRegex(@"(?<!\+\s*)(" + StringLiteral + @")\s*(==|!=)\s*(?<![\w.\[\]\^])(?<!\+\s*)\b(" + SimpleReceiver + @")" + CaseFoldCall + TrimCapture + @"(?![ \t]*[.+(?])")]
    private static partial Regex CaseFoldCompareReversedRegex();

    [GeneratedRegex(@"\.Substring\(\s*(" + SubstringArg + @")(?:\s*,\s*(" + SubstringArg + @"))?\s*\)")]
    private static partial Regex SubstringRegex();

    [GeneratedRegex(@"(?<![\w.\[\]\^])\b(" + SimpleReceiver + @")\.Equals\(\s*((?:" + StringLiteral + @")|(?:" + SimpleReceiver + @"))\s*,\s*StringComparison\.(\w+)\s*\)")]
    private static partial Regex InstanceEqualsRegex();

    [GeneratedRegex(@"(?<![\w.\[\]\^])\b(" + SimpleReceiver + @")\.ToString\(\s*""([^""\r\n]+)""\s*(?:,\s*(CultureInfo\.InvariantCulture)\s*)?\)")]
    private static partial Regex ToStringFormatRegex();

    [GeneratedRegex(@"(?<!\.)\bFile\.ReadAllText\(")]
    private static partial Regex IoReadTextRegex();

    [GeneratedRegex(NullAssignLinePattern)]
    private static partial Regex NullAssignLineRegex();

    [GeneratedRegex(NullAssignBlockPattern)]
    private static partial Regex NullAssignBlockRegex();

    #endregion
}
