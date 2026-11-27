// Licensed under MIT; see License.md for disclaimer, details, and extended user conditions.

namespace CodeOptimizer.Plugins;

/// <summary>
/// Löscht unbenutzte Methoden und Funktionen in Interfaces, Klassen und Strukturen vollständig inklusive
/// Doc-Kommentaren und Attributen. Überschreibungen und Interface-Implementierungen werden gemeinsam mit
/// ihrer Basis gelöscht; veraltete Marker ('// TODO: Unused') werden entfernt. Methodennamen, die das
/// Framework selbst aufrufen kann, bleiben unangetastet.
/// </summary>
public sealed partial class UnusedMethodPlugin : CodeOptimizerPlugin {

    #region Fields

    /// <summary>
    /// Methodennamen, die das Framework oder externe Schnittstellen selbst aufrufen kann; diese werden nie gelöscht.
    /// </summary>
    private static readonly string[] FrameworkNames = ["Main", "Dispose", "DisposeAsync", "ToString", "Equals", "GetHashCode", "CompareTo", "Compare", "GetEnumerator", "GetAsyncEnumerator", "MoveNext", "Reset", "Clone", "Finalize", "Read", "ReadAsync", "Write", "WriteAsync", "Flush", "Seek", "Close", "Parse", "TryParse", "TryGetValue", "Add", "Clear", "Contains", "ContainsKey", "Remove", "CopyTo", "IndexOf", "Insert", "Deconstruct", "GetObjectData", "BeginInit", "EndInit"];

    #endregion

    #region Properties

    public override string Name => "Ungenutzte Methoden";

    public override int Order => 60;

    public override string QuickInfo => "Löscht unbenutzte Methoden und Funktionen in Interfaces, Klassen und Strukturen inklusive Doc-Kommentaren und Attributen. Beachtet Überschreibungen und Interfaces: Diese werden gemeinsam mit ihrer Basis gelöscht. Der Verwendungscheck zählt reine Code-Vorkommen des Namens im ganzen Projekt (Strings und Kommentare zählen nicht).";

    public override ImageCode Symbol => ImageCode.Papierkorb;

    #endregion

    #region Methods

    public override string Execute(string rootPath) {
        var skipped = new List<string>();
        var candidates = new List<(string File, List<string> Lines, List<CsMemberInfo> Members)>();
        var tokenCounts = new List<Dictionary<string, int>>();
        var headerCount = new Dictionary<string, int>(StringComparer.Ordinal);
        var interfaceNames = new HashSet<string>(StringComparer.Ordinal);
        var methodsByName = new Dictionary<string, List<CsMemberInfo>>(StringComparer.Ordinal);
        var linesByFile = new Dictionary<string, List<string>>(StringComparer.Ordinal);

        // Pass 1: Member sammeln und je Datei die reinen Code-Vorkommen aller Bezeichner zählen
        foreach (var file in SourceFiles.Enumerate(rootPath)) {
            if (!SourceFiles.IsUtf8(file)) {
                skipped.Add(file);
                continue;
            }

            var content = IO.ReadAllText(file);
            tokenCounts.Add(TokenCounts(content, new CsSanitizer().SanitizeKindsWithCodeHoles(content)));

            // Designer-Dateien sind generiert: keine Kandidaten, aber Referenzen darin zählen
            if (file.EndsWith(".Designer.cs", StringComparison.OrdinalIgnoreCase)) { continue; }

            var (lines, clean) = CsCodeScan.Load(file);
            var members = CsCodeScan.Members(file, lines, clean);
            candidates.Add((file, lines, members));
            linesByFile.Add(file, lines);

            foreach (var member in members) {
                if (member.Category != "Methods" || member.Name is not { Length: > 0 }) { continue; }

                if (member.TypeIsInterface) { interfaceNames.Add(member.Name); }
                headerCount[member.Name] = headerCount.GetValueOrDefault(member.Name) + NameCountInHeader(member, clean);

                if (!methodsByName.TryGetValue(member.Name, out var list)) { list = []; methodsByName.Add(member.Name, list); }
                list.Add(member);
            }
        }

        // Pass 2: Verwendung prüfen, veraltete Marker entfernen und unbenutzte Methoden samt Kette löschen
        var deleted = new List<string>();
        var staleRemoved = 0;
        var claimed = new HashSet<CsMemberInfo>();
        var removalsByFile = new Dictionary<string, List<(int Start, int End)>>();
        var staleByFile = new Dictionary<string, List<int>>();

        foreach (var (file, lines, members) in candidates) {
            foreach (var member in members) {
                if (member.Category != "Methods" || member.Name is not { Length: > 0 }) { continue; }
                if (FrameworkNames.Contains(member.Name)) { continue; }
                if (member.IsOverride || member.IsExplicitImplementation || member.IsPartial || member.IsExtern) { continue; }

                // Implizite Interface-Implementierungen erben die Verwendung vom Interface-Member
                if (!member.TypeIsInterface && !member.IsStatic && member.IsPublic && !member.IsVirtual && !member.IsAbstract && interfaceNames.Contains(member.Name)) { continue; }

                var total = 0;
                foreach (var counts in tokenCounts) { total += counts.GetValueOrDefault(member.Name); }
                var used = total > headerCount.GetValueOrDefault(member.Name);

                if (used) {
                    for (var j = member.TriviaStart; j < member.HeaderStart; j++) {
                        if (IsMarkerLine(lines[j])) {
                            if (!staleByFile.TryGetValue(file, out var stale)) { stale = []; staleByFile.Add(file, stale); }
                            stale.Add(j);
                            staleRemoved++;
                        }
                    }

                    continue;
                }

                foreach (var victim in CsCodeScan.ImplementationChain(member, methodsByName, interfaceNames)) {
                    if (!claimed.Add(victim)) { continue; }

                    var victimLines = linesByFile[victim.File];
                    var start = victim.TriviaStart;
                    var end = victim.EndLine;

                    // Eine angrenzende Leerzeile mit entfernen, damit keine Doppel-Leerzeilen entstehen
                    if (start > 0 && victimLines[start - 1].Trim().Length == 0 && end + 1 < victimLines.Count && victimLines[end + 1].Trim().Length == 0) { end++; }

                    if (!removalsByFile.TryGetValue(victim.File, out var removals)) { removals = []; removalsByFile.Add(victim.File, removals); }
                    removals.Add((start, end));
                    deleted.Add(Path.GetFileName(victim.File) + ": " + victim.Name);
                }
            }
        }

        // Pass 3: Dateien schreiben
        var filesChanged = 0;

        foreach (var (file, lines, _) in candidates) {
            var edits = new List<(int Start, int End)>(); // Zeilen inklusive

            if (staleByFile.TryGetValue(file, out var stale)) {
                foreach (var j in stale) { edits.Add((j, j)); }
            }

            if (removalsByFile.TryGetValue(file, out var removals)) { edits.AddRange(removals); }

            if (edits.Count == 0) { continue; }

            foreach (var (start, end) in edits.OrderByDescending(e => e.Start)) {
                lines.RemoveRange(start, end - start + 1);
            }

            File.WriteAllText(file, string.Join("\r\n", lines), new UTF8Encoding(true));
            filesChanged++;
        }

        var report = new StringBuilder();
        report.AppendLine("=== Ungenutzte Methoden ===");
        report.AppendLine(CultureInfo.InvariantCulture, $"Methoden gelöscht:         {deleted.Count}");
        report.AppendLine(CultureInfo.InvariantCulture, $"Veraltete Marker entfernt: {staleRemoved}");
        report.AppendLine(CultureInfo.InvariantCulture, $"Dateien geändert:          {filesChanged}");
        foreach (var entry in deleted) { report.AppendLine("  " + entry); }
        AppendSkipped(report, skipped);
        return report.ToString();
    }

    [GeneratedRegex(@"[\p{L}_]\w*")]
    private static partial Regex IdentifierRegex();

    private static bool IsMarkerLine(string line) => line.TrimStart().StartsWith("// TODO: Unused", StringComparison.Ordinal);

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
