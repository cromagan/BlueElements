// Licensed under MIT; see License.md for disclaimer, details, and extended user conditions.

namespace CodeOptimizer.Plugins;

/// <summary>
/// Schreibt über unbenutzte Methoden und Funktionen in Interfaces, Klassen und Strukturen den Kommentar 'TODO: Unused'.
/// Überschreibungen und Interface-Implementierungen werden nicht markiert; beim Entfernen veralteter Marker
/// werden nur selbst geschriebene Marker ('// TODO: Unused') wieder gelöscht.
/// </summary>
public sealed partial class UnusedMethodPlugin : CodeOptimizerPlugin {

    #region Fields

    /// <summary>
    /// Methodennamen, die das Framework oder externe Schnittstellen selbst aufrufen kann; diese werden nie markiert.
    /// </summary>
    private static readonly string[] FrameworkNames = ["Main", "Dispose", "DisposeAsync", "ToString", "Equals", "GetHashCode", "CompareTo", "Compare", "GetEnumerator", "GetAsyncEnumerator", "MoveNext", "Reset", "Clone", "Finalize", "Read", "ReadAsync", "Write", "WriteAsync", "Flush", "Seek", "Close", "Parse", "TryParse", "TryGetValue", "Add", "Clear", "Contains", "ContainsKey", "Remove", "CopyTo", "IndexOf", "Insert", "Deconstruct", "GetObjectData", "BeginInit", "EndInit"];

    private const string Marker = "// TODO: Unused";

    #endregion

    #region Properties

    public override string Name => "Ungenutzte Methoden";

    public override int Order => 60;

    public override string QuickInfo => "Schreibt über unbenutzte Methoden und Funktionen in Interfaces, Klassen und Strukturen 'TODO: Unused'. Beachtet Überschreibungen und Interfaces: Diese werden nicht markiert. Der Verwendungscheck zählt reine Code-Vorkommen des Namens im ganzen Projekt (Strings und Kommentare zählen nicht).";

    public override ImageCode Symbol => ImageCode.Papierkorb;

    #endregion

    #region Methods

    public override string Execute(string rootPath) {
        var skipped = new List<string>();
        var candidates = new List<(string File, List<string> Lines, List<CsMemberInfo> Members)>();
        var tokenCounts = new List<Dictionary<string, int>>();
        var headerCount = new Dictionary<string, int>(StringComparer.Ordinal);
        var interfaceNames = new HashSet<string>(StringComparer.Ordinal);

        // Pass 1: Member sammeln und je Datei die reinen Code-Vorkommen aller Bezeichner zählen
        foreach (var file in SourceFiles.Enumerate(rootPath)) {
            if (!SourceFiles.IsUtf8(file)) {
                skipped.Add(file);
                continue;
            }

            var content = File.ReadAllText(file);
            tokenCounts.Add(TokenCounts(content, new CsSanitizer().SanitizeKindsWithCodeHoles(content)));

            // Designer-Dateien sind generiert: keine Kandidaten, aber Referenzen darin zählen
            if (file.EndsWith(".Designer.cs", StringComparison.OrdinalIgnoreCase)) { continue; }

            var (lines, clean) = CsCodeScan.Load(file);
            var members = CsCodeScan.Members(file, lines, clean);
            candidates.Add((file, lines, members));

            foreach (var member in members) {
                if (member.Category != "Methods" || member.Name is not { Length: > 0 }) { continue; }

                if (member.TypeIsInterface) { interfaceNames.Add(member.Name); }
                headerCount[member.Name] = headerCount.GetValueOrDefault(member.Name) + NameCountInHeader(member, clean);
            }
        }

        // Pass 2: Verwendung prüfen, Marker setzen und veraltete Marker entfernen
        var marked = new List<string>();
        var staleRemoved = 0;
        var filesChanged = 0;

        foreach (var (file, lines, members) in candidates) {
            var edits = new List<(int Line, string? Text)>(); // Text null = Zeile entfernen

            foreach (var member in members) {
                if (member.Category != "Methods" || member.Name is not { Length: > 0 }) { continue; }
                if (FrameworkNames.Contains(member.Name)) { continue; }
                if (member.IsOverride || member.IsExplicitImplementation || member.IsPartial || member.IsExtern) { continue; }

                // Implizite Interface-Implementierungen erben die Verwendung vom Interface-Member
                if (!member.TypeIsInterface && !member.IsStatic && member.IsPublic && !member.IsVirtual && !member.IsAbstract && interfaceNames.Contains(member.Name)) { continue; }

                var total = 0;
                foreach (var counts in tokenCounts) { total += counts.GetValueOrDefault(member.Name); }
                var used = total > headerCount.GetValueOrDefault(member.Name);
                var hasMarker = HasMarker(lines, member);

                if (used) {
                    if (!hasMarker) { continue; }

                    for (var j = member.TriviaStart; j < member.HeaderStart; j++) {
                        if (IsMarkerLine(lines[j])) {
                            edits.Add((j, null));
                            staleRemoved++;
                        }
                    }

                    continue;
                }

                if (hasMarker) { continue; }

                var insertAt = member.TriviaStart;
                if (insertAt > 0 && lines[insertAt - 1].Trim().Length > 0) {
                    edits.Add((insertAt, string.Empty));
                    insertAt++;
                }

                edits.Add((insertAt, IndentOf(lines[insertAt]) + Marker));
                marked.Add(Path.GetFileName(file) + ": " + member.Name);
            }

            if (edits.Count == 0) { continue; }

            foreach (var (line, text) in edits.OrderByDescending(e => e.Line)) {
                if (text is null) { lines.RemoveAt(line); } else { lines.Insert(line, text); }
            }

            File.WriteAllText(file, string.Join("\r\n", lines), new UTF8Encoding(true));
            filesChanged++;
        }

        var report = new StringBuilder();
        report.AppendLine("=== Ungenutzte Methoden ===");
        report.AppendLine(CultureInfo.InvariantCulture, $"Methoden markiert:         {marked.Count}");
        report.AppendLine(CultureInfo.InvariantCulture, $"Veraltete Marker entfernt: {staleRemoved}");
        report.AppendLine(CultureInfo.InvariantCulture, $"Dateien geändert:          {filesChanged}");
        foreach (var entry in marked) { report.AppendLine("  " + entry); }
        AppendSkipped(report, skipped);
        return report.ToString();
    }

    private static bool HasMarker(List<string> lines, CsMemberInfo member) {
        for (var j = member.TriviaStart; j < member.HeaderStart; j++) {
            if (IsMarkerLine(lines[j])) { return true; }
        }

        return false;
    }

    [GeneratedRegex(@"[\p{L}_]\w*")]
    private static partial Regex IdentifierRegex();

    private static string IndentOf(string line) {
        var count = 0;
        while (count < line.Length && line[count] == ' ') { count++; }
        return line[..count];
    }

    private static bool IsMarkerLine(string line) => line.TrimStart().StartsWith(Marker, StringComparison.Ordinal);

    /// <summary>
    /// Zählt die Vorkommen des Deklarationsnamens im bereinigten Signaturkopf des Members; dieser Anteil ist keine Verwendung.
    /// </summary>
    private static int NameCountInHeader(CsMemberInfo member, List<string> clean) {
        var last = member.BodyOpenLine >= 0 ? member.BodyOpenLine : member.EndLine;
        var text = string.Join(" ", clean.GetRange(member.HeaderStart, last - member.HeaderStart + 1));
        var count = 0;

        foreach (Match m in IdentifierRegex().Matches(text)) {
            if (m.Value == member.Name) { count++; }
        }

        return count;
    }

    /// <summary>
    /// Liefert je Datei ein Verzeichnis Bezeichner → Anzahl reiner Code-Vorkommen.
    /// </summary>
    private static Dictionary<string, int> TokenCounts(string content, string kinds) {
        var result = new Dictionary<string, int>(StringComparer.Ordinal);

        foreach (Match m in IdentifierRegex().Matches(content)) {
            if (kinds[m.Index] != CsSanitizer.KindCode) { continue; }
            result[m.Value] = result.GetValueOrDefault(m.Value) + 1;
        }

        return result;
    }

    #endregion
}
