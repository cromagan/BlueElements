// Licensed under MIT; see License.md for disclaimer, details, and extended user conditions.

namespace CodeOptimizer.Plugins;

/// <summary>
/// Führt die Code-Stil-Transformationen aus: leere Klammern, einzeilige If-Rückgaben,
/// is-null-Muster, Summary-Tags und Doc-Kommentar-Einzug.
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

    #endregion

    #region Properties

    public override string Name => "Code-Stil";
    public override ImageCode Symbol => ImageCode.Zahnrad;
    public override string QuickInfo => "Wendet die Code-Stil-Regeln an: leere Klammern, einzeilige If-Rückgaben, is-null-Muster, Summary-Tags und Doc-Kommentar-Einzug.";
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
        var totalFiles = 0;

        foreach (var file in SourceFiles.Enumerate(rootPath)) {
            var content = File.ReadAllText(file);
            var original = content;

            // 1. Leere Klammern über zwei Zeilen -> eine Zeile
            var prev = content;
            content = EmptyBracesRegex().Replace(content, CollapseEmptyBraces);
            var c1 = content != prev;

            // 3. Einfaches return;/return true;/return false;/continue;/break; in If ohne Klammern -> If() { ...; }
            prev = content;
            content = IfSimpleStatementRegex().Replace(content, AddBracesToSimpleStatement);
            var c3 = content != prev;

            // 3b. If-Block mit nur einer einfachen Anweisung -> eine Zeile
            prev = content;
            content = IfSimpleStatementBlockRegex().Replace(content, AddBracesToSimpleStatement);
            c3 = c3 || content != prev;

            // 4. Leerzeichen nach ( bei Methodenaufrufen entfernen
            prev = content;
            content = ParenSpaceRegex().Replace(content, RemoveParenSpace);
            var c4 = content != prev;

            // 5. is not null -> is not null
            prev = content;
            content = NotNullRegex().Replace(content, " is not null");
            var c5 = content != prev;

            // 6. is null -> is null
            prev = content;
            content = NullEqualsRegex().Replace(content, " is null");
            var c6 = content != prev;

            // 7a. (Disposable) is not { } -> is not { IsDisposed: false }
            prev = content;
            content = DisposableNotBraceRegex().Replace(content, "$1 is not { IsDisposed: false }");
            var c7a = content != prev;

            // 7b. (Disposable) is { } -> is { IsDisposed: false }
            prev = content;
            content = DisposableBraceRegex().Replace(content, "$1 is { IsDisposed: false }");
            var c7b = content != prev;

            // 7. is not { } -> is null (nur vor ) & |)
            prev = content;
            content = IsNotBraceRegex().Replace(content, "is null");
            var c7 = content != prev;

            // 8. is { } -> is not null (nur vor ) & |)
            prev = content;
            content = IsBraceRegex().Replace(content, "is not null");
            var c8 = content != prev;

            // 9a. /// <summary> mit Text -> Tag in eigene Zeile
            prev = content;
            content = SummaryOpenRegex().Replace(content, SplitSummaryOpen);
            var c9a = content != prev;

            // 9b. Text + </summary> am Zeilenende -> Tag in eigene Zeile
            prev = content;
            content = SummaryCloseRegex().Replace(content, SplitSummaryClose);
            var c9b = content != prev;

            // 10. Einheitliche Einrückung von ///-Blöcken (an der Folgezeile ausgerichtet)
            prev = content;
            content = DocIndentRegex().Replace(content, FixDocIndent);
            var c10 = content != prev;

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

            if (content == original) { continue; }

            File.WriteAllText(file, content, new UTF8Encoding(true));
            totalFiles++;
        }

        var report = new StringBuilder();
        report.AppendLine("=== Code-Stil ===");
        report.AppendLine(CultureInfo.InvariantCulture, $"Leere Klammern eingekürzt:  {braces}");
        report.AppendLine(CultureInfo.InvariantCulture, $"If-Rückgaben einzeilig:     {ifOneLine}");
        report.AppendLine(CultureInfo.InvariantCulture, $"Klammer-Leerzeichen:        {parenSpace}");
        report.AppendLine(CultureInfo.InvariantCulture, $" is not null -> is not null:     {neNull}");
        report.AppendLine(CultureInfo.InvariantCulture, $" is null -> is null:         {eqNull}");
        report.AppendLine(CultureInfo.InvariantCulture, $"is not {{ }} -> is null:     {isNotBrace}");
        report.AppendLine(CultureInfo.InvariantCulture, $"is {{ }} -> is not null:     {isBrace}");
        report.AppendLine(CultureInfo.InvariantCulture, $"Disposable is not {{ }}:     {dispNot}");
        report.AppendLine(CultureInfo.InvariantCulture, $"Disposable is {{ }}:         {disp}");
        report.AppendLine(CultureInfo.InvariantCulture, $"Summary-Tags aufgetrennt:   {summaryOpen + summaryClose}");
        report.AppendLine(CultureInfo.InvariantCulture, $"Doc-Einzug korrigiert:      {docIndent}");
        report.AppendLine(CultureInfo.InvariantCulture, $"Dateien geändert:           {totalFiles}");
        return report.ToString();
    }

    private static string CollapseEmptyBraces(Match m) => m.Groups[1].Success ? " { }" : "{}";

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

    [GeneratedRegex(@"is\s+not\s*\{\s*\}(?=\s*[)&|])")]
    private static partial Regex IsNotBraceRegex();

    [GeneratedRegex(@"is\s*\{\s*\}(?=\s*[)&|])")]
    private static partial Regex IsBraceRegex();

    [GeneratedRegex(@"(?m)^([ \t]*)///[ \t]*<summary>[ \t]*(.*?)[ \t]*(</summary>)?[ \t]*(?=\r?\n)")]
    private static partial Regex SummaryOpenRegex();

    [GeneratedRegex(@"(?m)^([ \t]*)///[ \t]*(.*?\S)[ \t]*</summary>[ \t]*(?=\r?\n)")]
    private static partial Regex SummaryCloseRegex();

    [GeneratedRegex(@"(?m)^[ \t]*///[^\r\n]*(?:\r?\n[ \t]*///[^\r\n]*)*(?=\r?\n([ \t]*)\S)")]
    private static partial Regex DocIndentRegex();

    #endregion
}
