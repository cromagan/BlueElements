// Licensed under MIT; see License.md for disclaimer, details, and extended user conditions.

namespace CodeOptimizer.Plugins;

/// <summary>
/// Ersetzt .Substring(a, b)-Aufrufe durch Range-Syntax, aber nur in echtem Code.
/// </summary>
public partial class SubstringToRangePlugin : CodeOptimizerPlugin {

    #region Fields

    /// <summary>
    /// Argument eines Substring-Aufrufs: ohne Komma, Klammern und Zeilenumbrüche, damit verschachtelte Aufrufe unangetastet bleiben.
    /// </summary>
    private const string SubstringArg = @"[^\r\n(),]+";

    #endregion

    #region Properties

    public override string Name => "Substring zu Range";
    public override ImageCode Symbol => ImageCode.Zuschneiden;
    public override string QuickInfo => "Ersetzt .Substring(a, b) durch Range-Syntax: .Substring(a) wird zu [a..], .Substring(0, b) zu [..b] und .Substring(a, b) zu [a..(a + b)].";
    public override int Order => 36;

    #endregion

    #region Methods

    public override string Execute(string rootPath) {
        var counterFiles = 0;
        var counterReplacements = 0;
        var skipped = new List<string>();

        foreach (var file in SourceFiles.Enumerate(rootPath)) {
            // Dateien ohne gültiges UTF-8 dürfen nicht umgeschrieben werden - das übernimmt das Encoding-Modul
            if (!SourceFiles.IsUtf8(file)) {
                skipped.Add(file);
                continue;
            }

            var content = IO.ReadAllText(file);
            var kinds = new CsSanitizer().SanitizeKinds(content);
            var replacements = 0;

            foreach (Match match in SubstringRegex().Matches(content)) {
                if (CsSanitizer.IsPureCode(kinds, match.Index, match.Length)) { replacements++; }
            }

            if (replacements == 0) { continue; }

            content = CsSanitizer.ReplaceInCode(content, SubstringRegex(), FoldSubstring);
            File.WriteAllText(file, content, new UTF8Encoding(true));
            counterFiles++;
            counterReplacements += replacements;
        }

        var report = new StringBuilder();
        report.AppendLine("=== Substring zu Range ===");
        report.AppendLine(CultureInfo.InvariantCulture, $"Ersetzungen:      {counterReplacements}");
        report.AppendLine(CultureInfo.InvariantCulture, $"Dateien geändert: {counterFiles}");
        AppendSkipped(report, skipped);
        return report.ToString();
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

    [GeneratedRegex(@"\.Substring\(\s*(" + SubstringArg + @")(?:\s*,\s*(" + SubstringArg + @"))?\s*\)")]
    private static partial Regex SubstringRegex();

    #endregion
}