// Licensed under MIT; see License.md for disclaimer, details, and extended user conditions.

using MemberEntry = (string Category, string SortKey, System.Collections.Generic.List<string> Trivia, System.Collections.Generic.List<string> Code);

namespace CodeOptimizer.Plugins;

/// <summary>
/// Baut die Standard-Regionen auf oberster Klassenebene neu auf und sortiert die Elemente alphabetisch.
/// Felder behalten ihre Reihenfolge, da die Initialisierungsreihenfolge wichtig ist.
/// </summary>
public partial class RegionPlugin : CodeOptimizerPlugin {

    #region Fields

    /// <summary>Reihenfolge und Namen der Standard-Regionen.</summary>
    private static readonly string[] RegionOrder = ["Fields", "Delegates", "Constructors", "Events", "Properties", "Indexers", "Methods", "Structs"];

    private static readonly char[] TokenChars = ['(', '{', ';', '='];

    #endregion

    #region Properties

    public override string Name => "Regionen";
    public override ImageCode Symbol => ImageCode.Groupbox;
    public override string QuickInfo => "Baut die Standard-Regionen auf Klassenebene neu auf und sortiert die Elemente alphabetisch. Felder behalten ihre Reihenfolge.";
    public override int Order => 40;

    #endregion

    #region Methods

    public override string Execute(string rootPath) {
        var counterChanged = 0;
        var counterTypes = 0;
        var counterRemoved = 0;
        var counterAdded = 0;

        foreach (var file in SourceFiles.Enumerate(rootPath)) {
            // Designer-Dateien sind generiert und werden nicht angetastet
            if (file.EndsWith(".Designer.cs", StringComparison.OrdinalIgnoreCase)) { continue; }

            var content = File.ReadAllText(file);
            var result = ReorganizeFile(content, out var types, out var removed, out var added);

            if (result is null) { continue; }

            File.WriteAllText(file, result, new UTF8Encoding(true));
            counterChanged++;
            counterTypes += types;
            counterRemoved += removed;
            counterAdded += added;
        }

        var report = new StringBuilder();
        report.AppendLine("=== Regionen ===");
        report.AppendLine(CultureInfo.InvariantCulture, $"Dateien geändert:    {counterChanged}");
        report.AppendLine(CultureInfo.InvariantCulture, $"Typen neu geordnet:  {counterTypes}");
        report.AppendLine(CultureInfo.InvariantCulture, $"Regionen entfernt:   {counterRemoved}");
        report.AppendLine(CultureInfo.InvariantCulture, $"Regionen eingefügt:  {counterAdded}");
        return report.ToString();
    }

    /// <summary>Baut die Elementbereiche aller obersten Typen neu auf. Liefert null, wenn nichts zu ändern ist.</summary>
    private static string? ReorganizeFile(string content, out int typesReorganized, out int regionsRemoved, out int regionsAdded) {
        typesReorganized = 0;
        regionsRemoved = 0;
        regionsAdded = 0;

        var lines = content.Split(["\r\n", "\n"], StringSplitOptions.None).ToList();
        var clean = new CsSanitizer().SanitizeLines(lines);

        var depths = new int[lines.Count];
        var depth = 0;
        for (var i = 0; i < lines.Count; i++) {
            depth += clean[i].Count('{') - clean[i].Count('}');
            depths[i] = depth;
        }

        var output = new List<string>();
        var rebuilt = 0;
        var totalRemoved = 0;
        var totalAdded = 0;
        var nsDepth = -1;
        var i2 = 0;

        while (i2 < lines.Count) {
            var depthBefore = i2 == 0 ? 0 : depths[i2 - 1];

            if ((nsDepth < 0 || depthBefore == nsDepth) && depths[i2] == depthBefore + 1 && IsTypeDeclaration(clean[i2])) {
                var bodyDepth = depthBefore + 1;
                var close = FindTypeClose(depths, lines, i2, bodyDepth);

                if (close > i2) {
                    if (nsDepth < 0) { nsDepth = depthBefore; }
                    var body = ReorganizeBody(lines, clean, depths, i2 + 1, close - 1, bodyDepth, TypeName(clean[i2]), out var removed, out var added);
                    output.Add(lines[i2]);

                    if (body is null) {
                        for (var j = i2 + 1; j <= close; j++) { output.Add(lines[j]); }
                    } else {
                        if (body.Count > 0) { output.Add(string.Empty); }
                        output.AddRange(body);
                        rebuilt++;
                        totalRemoved += removed;
                        totalAdded += added;
                    }

                    i2 = close + 1;
                    continue;
                }
            }

            output.Add(lines[i2]);
            i2++;
        }

        if (rebuilt == 0) { return null; }

        typesReorganized = rebuilt;
        regionsRemoved = totalRemoved;
        regionsAdded = totalAdded;
        return string.Join("\r\n", output);
    }

    /// <summary>Gruppiert und sortiert die Member des Typkörpers und baut die Regionen neu auf.</summary>
    private static List<string>? ReorganizeBody(List<string> lines, List<string> clean, int[] depths, int first, int last, int bodyDepth, string typeName, out int regionsRemoved, out int regionsAdded) {
        var items = ParseBody(lines, clean, depths, first, last, bodyDepth, typeName, out regionsRemoved, out var otherDirectives);
        regionsAdded = 0;

        // Konservativ: Bei anderen Präprozessor-Direktiven (#if/#pragma ...) nichts tun
        if (otherDirectives > 0) { return null; }

        var categorized = 0;
        foreach (var item in items) {
            if (item.Category.Length > 0) { categorized++; }
        }

        if (categorized == 0) { return null; }

        var result = new List<string>();
        var indent = new string(' ', bodyDepth * 4);

        foreach (var region in RegionOrder) {
            var group = new List<MemberEntry>();
            foreach (var item in items) {
                if (item.Category == region) { group.Add(item); }
            }

            if (group.Count == 0) { continue; }

            if (region != "Fields") { group = [.. group.OrderBy(SortKeySelector, StringComparer.OrdinalIgnoreCase)]; }

            if (result.Count > 0) { result.Add(string.Empty); }
            result.Add(indent + "#region " + region);
            result.Add(string.Empty);
            AppendItems(result, group);
            result.Add(string.Empty);
            result.Add(indent + "#endregion");
            regionsAdded++;
        }

        var rest = new List<MemberEntry>();
        foreach (var item in items) {
            if (item.Category.Length == 0 && (item.Trivia.Count > 0 || item.Code.Count > 0)) { rest.Add(item); }
        }

        if (rest.Count > 0) {
            if (result.Count > 0) { result.Add(string.Empty); }
            AppendItems(result, rest);
        }

        return result;
    }

    /// <summary>Zerlegt den Typkörper in Member-Blöcke mit vorausgehenden Kommentaren und Attributen.</summary>
    private static List<MemberEntry> ParseBody(List<string> lines, List<string> clean, int[] depths, int first, int last, int bodyDepth, string typeName, out int regionsRemoved, out int otherDirectives) {
        regionsRemoved = 0;
        otherDirectives = 0;
        var items = new List<MemberEntry>();
        var trivia = new List<string>();
        var i = first;

        while (i <= last) {
            var t = lines[i].Trim();

            if (t.Length == 0) { i++; continue; }

            if (t.StartsWith("#region", StringComparison.Ordinal) || t.StartsWith("#endregion", StringComparison.Ordinal)) {
                regionsRemoved++;
                i++;
                continue;
            }

            if (t.StartsWith('#')) {
                otherDirectives++;
                i++;
                continue;
            }

            // Inhalt war reiner Kommentar- oder String-Inhalt (z. B. Blockkommentar-Fortsetzung)
            if (clean[i].Trim().Length == 0) {
                trivia.Add(lines[i]);
                i++;
                continue;
            }

            if (IsTriviaStart(t)) {
                trivia.Add(lines[i]);
                i++;

                // Mehrzeilige Attribute komplett übernehmen
                while (i <= last && BracketBalance(trivia[^1]) > 0) {
                    trivia.Add(lines[i]);
                    i++;
                }

                continue;
            }

            var end = MemberEndIndex(depths, lines, i, last, bodyDepth);
            var count = end - i + 1;
            var code = lines.GetRange(i, count);
            var cleanHeader = string.Join(" ", clean.GetRange(i, count));
            var category = Classify(cleanHeader, typeName);
            items.Add((category, SortKeyOf(category, cleanHeader), trivia, code));
            trivia = [];
            i = end + 1;
        }

        if (trivia.Count > 0) { items.Add((string.Empty, string.Empty, trivia, [])); }

        return items;
    }

    private static void AppendItems(List<string> result, List<MemberEntry> items) {
        var first = true;
        foreach (var item in items) {
            if (!first) { result.Add(string.Empty); }
            first = false;
            result.AddRange(item.Trivia);
            result.AddRange(item.Code);
        }
    }

    /// <summary>Liefert die Zeile, in der der ab start beginnende Member endet.</summary>
    private static int MemberEndIndex(int[] depths, List<string> lines, int start, int last, int bodyDepth) {
        for (var j = start; j <= last; j++) {
            if (depths[j] == bodyDepth) {
                var t = lines[j].Trim();
                if (t.EndsWith(';') || t.EndsWith('}')) { return j; }
            }
        }
        return last;
    }

    /// <summary>Ordnet einen Member anhand seines Kopfes einer Region zu. Leerzeichen = nicht zuordnungsbar.</summary>
    private static string Classify(string header, string typeName) {
        var p = header.IndexOfAny(TokenChars);
        if (p < 0) { return string.Empty; }

        var pre = header[..p];

        if (ContainsWord(pre, "delegate")) { return "Delegates"; }
        if (ContainsWord(pre, "event")) { return "Events"; }
        if (ContainsWord(pre, "struct")) { return "Structs"; }
        if (ContainsWord(pre, "class") || ContainsWord(pre, "interface") || ContainsWord(pre, "record")) { return string.Empty; }
        if (pre.Contains("this[", StringComparison.Ordinal)) { return "Indexers"; }
        if (ContainsWord(pre, "operator")) { return "Methods"; }

        switch (header[p]) {
            case '(':
                return NameBefore(pre) == typeName ? "Constructors" : "Methods";
            case '=':
                return p + 1 < header.Length && header[p + 1] == '>' ? "Properties" : "Fields";
            case '{':
                return "Properties";
            case ';':
                return "Fields";
            default:
                return string.Empty;
        }
    }

    /// <summary>Liefert den Sortierschlüssel des Members. Felder und Konstruktoren behalten die Originalreihenfolge.</summary>
    private static string SortKeyOf(string category, string header) {
        if (category == "Indexers") { return "this"; }
        if (category == "Structs") { return WordAfter(header, "struct"); }
        if (category == "Delegates") {
            var p = header.IndexOf('(');
            return p < 0 ? header : NameBefore(header[..p]);
        }
        if (category == "Events") {
            var p = header.IndexOfAny(['{', ';', '=']);
            return p < 0 ? header : NameBefore(header[..p]);
        }
        if (category == "Properties") {
            var p = header.IndexOfAny(['{', '=']);
            return p < 0 ? header : NameBefore(header[..p]);
        }
        if (category == "Methods") {
            var p = header.IndexOf('(');
            if (p < 0) { return header; }
            var name = NameBefore(header[..p]);
            return name.Length > 0 ? name : header[..p].Trim();
        }

        // Fields und Constructors: Originalreihenfolge
        return string.Empty;
    }

    private static string SortKeySelector(MemberEntry item) => item.SortKey;

    private static bool IsTypeDeclaration(string cleanLine) => cleanLine.TrimEnd().EndsWith('{') && TypeKeywordRegex().IsMatch(cleanLine);

    private static string TypeName(string cleanLine) {
        var m = TypeNameRegex().Match(cleanLine);
        return m.Success ? m.Groups[1].Value : string.Empty;
    }

    private static int FindTypeClose(int[] depths, List<string> lines, int open, int bodyDepth) {
        for (var j = open + 1; j < lines.Count; j++) {
            if (depths[j] == bodyDepth - 1 && lines[j].Trim() == "}") { return j; }
        }
        return -1;
    }

    private static bool IsTriviaStart(string t) => t.StartsWith("//", StringComparison.Ordinal) || t.StartsWith("/*", StringComparison.Ordinal) || t.StartsWith('*') || t.StartsWith('[');

    private static int BracketBalance(string line) {
        var balance = 0;
        foreach (var c in line) {
            if (c == '[') { balance++; }
            if (c == ']') { balance--; }
        }
        return balance;
    }

    /// <summary>Liefert das letzte Wortzeichen-Wort vor dem ermittelten Token.</summary>
    private static string NameBefore(string pre) {
        var s = pre.TrimEnd();
        var end = s.Length;
        while (end > 0 && IsWordChar(s[end - 1])) { end--; }
        return s[end..];
    }

    private static string WordAfter(string header, string word) {
        var idx = header.IndexOf(word, StringComparison.Ordinal);
        if (idx < 0) { return string.Empty; }
        idx += word.Length;
        while (idx < header.Length && !IsWordChar(header[idx])) { idx++; }
        var end = idx;
        while (end < header.Length && IsWordChar(header[end])) { end++; }
        return header[idx..end];
    }

    private static bool ContainsWord(string text, string word) {
        var idx = text.IndexOf(word, StringComparison.Ordinal);
        while (idx >= 0) {
            var before = idx == 0 || !IsWordChar(text[idx - 1]);
            var after = idx + word.Length >= text.Length || !IsWordChar(text[idx + word.Length]);
            if (before && after) { return true; }
            idx = text.IndexOf(word, idx + 1, StringComparison.Ordinal);
        }
        return false;
    }

    private static bool IsWordChar(char c) => char.IsLetterOrDigit(c) || c == '_';

    [GeneratedRegex(@"\b(class|struct|interface|record)\b")]
    private static partial Regex TypeKeywordRegex();

    [GeneratedRegex(@"\b(?:class|struct|interface|record)\s+(?!struct\b)([A-Za-z_]\w*)")]
    private static partial Regex TypeNameRegex();

    #endregion
}
