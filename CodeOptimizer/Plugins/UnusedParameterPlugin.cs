// Licensed under MIT; see License.md for disclaimer, details, and extended user conditions.

using System.Drawing;
using System.Text.Json.Nodes;

namespace CodeOptimizer.Plugins;

/// <summary>
/// Löscht unbenutzte Parameter direkt aus den Signaturen und entfernt das zugehörige Argument an allen
/// Aufrufstellen. Override-, Interface- und partielle Ketten werden gemeinsam behandelt: Ein Parameter wird
/// nur gelöscht, wenn keine Implementierung der Kette ihn benutzt; er wird dann in der Deklaration und in
/// allen Implementierungen entfernt. Delegat-Deklarationen bleiben unverändert. Ein Löschen unterbleibt bei
/// Mehrdeutigkeiten (Überladungen mit gleicher oder Ziel-Arity, benannte Argumente, Methodengruppen-Referenzen,
/// params-Parameter, Konstruktor-Ketten mit unklarer Basis). Veraltete 'TODO: Unused'-Marker werden entfernt.
/// </summary>
public sealed partial class UnusedParameterPlugin : CodeOptimizerPlugin {

    #region Fields

    private const string MarkerPrefix = "// TODO: Unused: ";

    private const int RowHeight = 27;

    /// <summary>
    /// True: Parameter gemäß Event-Handler-Muster (sender, e) werden nicht gelöscht.
    /// </summary>
    private bool _skipEventHandlers = true;

    #endregion

    #region Properties

    public override string Name => "Ungenutzte Parameter";

    public override int Order => 70;

    public override string QuickInfo => "Löscht unbenutzte Parameter direkt aus den Signaturen und passt alle Aufrufstellen an. Override-, Interface- und partielle Ketten werden gemeinsam behandelt. Bei Mehrdeutigkeiten (Überladungen, benannte Argumente, Methodengruppen, params, Konstruktor-Ketten) wird nicht gelöscht. Event-Handler mit sender/e kann die Option ausnehmen.";

    public override ImageCode Symbol => ImageCode.Trichter;

    #endregion

    #region Methods

    public override string Execute(string rootPath) {
        var skipped = new List<string>();
        var deleted = new List<string>();
        var staleRemoved = 0;
        var files = new List<(string File, List<string> Lines, string Content, string Kinds, int[] Offsets, List<CsMemberInfo> Members)>();
        var interfaceNames = new HashSet<string>(StringComparer.Ordinal);
        var methodsByName = new Dictionary<string, List<CsMemberInfo>>(StringComparer.Ordinal);
        var ctorsByType = new Dictionary<string, List<CsMemberInfo>>(StringComparer.Ordinal);
        var contentByFile = new Dictionary<string, string>(StringComparer.Ordinal);
        var kindsByFile = new Dictionary<string, string>(StringComparer.Ordinal);
        var offsetsByFile = new Dictionary<string, int[]>(StringComparer.Ordinal);
        var linesByFile = new Dictionary<string, List<string>>(StringComparer.Ordinal);
        var visibleByFile = new Dictionary<string, List<string>>(StringComparer.Ordinal);
        var baseByType = new Dictionary<string, string>(StringComparer.Ordinal);

        // Pass 1: Dateien und Member sammeln
        foreach (var file in SourceFiles.Enumerate(rootPath)) {
            // Designer-Dateien sind generiert und werden nicht angetastet
            if (file.EndsWith(".Designer.cs", StringComparison.OrdinalIgnoreCase)) { continue; }

            // Dateien ohne gültiges UTF-8 dürfen nicht umgeschrieben werden - das übernimmt das Encoding-Modul
            if (!SourceFiles.IsUtf8(file)) { skipped.Add(file); continue; }

            var content = IO.ReadAllText(file);
            var lines = content.Split(["\r\n", "\n"], StringSplitOptions.None).ToList();
            var clean = new CsSanitizer().SanitizeLines(lines);
            var kinds = new CsSanitizer().SanitizeKinds(content);
            var offsets = CsCodeScan.LineOffsets(content);
            var members = CsCodeScan.Members(file, lines, clean);

            files.Add((file, lines, content, kinds, offsets, members));
            contentByFile.Add(file, content);
            kindsByFile.Add(file, kinds);
            offsetsByFile.Add(file, offsets);
            linesByFile.Add(file, lines);
            visibleByFile.Add(file, new CsSanitizer().SanitizeLinesWithCodeHoles(lines));

            foreach (var baseType in CsCodeScan.BaseTypes(clean)) { baseByType.TryAdd(baseType.Key, baseType.Value); }

            foreach (var member in members) {
                if (member.TypeIsInterface && member.Category == "Methods" && member.Name is { Length: > 0 }) { interfaceNames.Add(member.Name); }

                if (member.Category == "Methods" && member.Name is { Length: > 0 }) {
                    if (!methodsByName.TryGetValue(member.Name, out var list)) { list = []; methodsByName.Add(member.Name, list); }
                    list.Add(member);
                } else if (member.Category == "Constructors") {
                    if (!ctorsByType.TryGetValue(member.TypeName, out var list)) { list = []; ctorsByType.Add(member.TypeName, list); }
                    list.Add(member);
                }
            }
        }

        // Pass 2: Lösch-Kandidaten je Member ermitteln; Ketten und Mehrdeutigkeiten prüfen
        var specs = new List<(CsMemberInfo Member, List<int> Indices, List<CsMemberInfo> Chain)>();

        foreach (var entry in files) {
            foreach (var member in entry.Members) {
                if (member.Category == "Delegates" || member.IsOverride) { continue; }

                if (member.Category == "Constructors") {
                    if (!member.HasBody) { continue; }
                } else if (member.Category == "Methods") {
                    if (member.Name is not { Length: > 0 }) { continue; }
                    if (!member.HasBody && !member.TypeIsInterface && !member.IsAbstract) { continue; }
                } else {
                    continue;
                }

                var visible = visibleByFile[member.File];
                var indices = new List<int>();

                for (var i = 0; i < member.Parameters.Count; i++) {
                    var name = member.Parameters[i];
                    if (name == "_") { continue; }
                    if (_skipEventHandlers && IsEventParameter(name)) { continue; }
                    if (member.HasBody && BodyContains(visible, member, name)) { continue; }
                    indices.Add(i);
                }

                if (indices.Count == 0) { continue; }

                var chain = CsCodeScan.ImplementationChain(member, methodsByName, interfaceNames);
                var acceptable = new List<int>();

                foreach (var i in indices) {
                    if (IsDeletable(member, i, chain, methodsByName, ctorsByType, visibleByFile, contentByFile, kindsByFile, offsetsByFile)) { acceptable.Add(i); }
                }

                if (acceptable.Count > 0) { specs.Add((member, acceptable, chain)); }
            }
        }

        // Pass 3: Aufrufstellen prüfen und alle Text-Bereiche sammeln; mehrdeutige Specs entfallen komplett
        var headerRangesByFile = new Dictionary<string, List<(int Start, int End)>>(StringComparer.Ordinal);

        foreach (var entry in files) {
            var ranges = new List<(int Start, int End)>();

            foreach (var member in entry.Members) {
                var start = entry.Offsets[member.TriviaStart];
                var end = member.BodyOpenLine >= 0 ? entry.Offsets[member.BodyOpenLine] + member.BodyOpenColumn : entry.Offsets[member.EndLine] + entry.Lines[member.EndLine].Length;
                ranges.Add((start, end));
            }

            headerRangesByFile.Add(entry.File, ranges);
        }

        var editsByFile = new Dictionary<string, List<(int Start, int End)>>(StringComparer.Ordinal);
        var claimed = new HashSet<CsMemberInfo>();

        foreach (var (member, indices, chain) in specs) {
            if (chain.Exists(claimed.Contains)) { continue; }

            var sites = member.Category == "Constructors"
                ? CtorCallSpans(member, indices, files, baseByType, headerRangesByFile)
                : NameReferencesAreCalls(member.Name, files) ? MethodCallSpans(member, indices, files, headerRangesByFile) : null;

            if (sites is null) { continue; }

            foreach (var c in chain) { claimed.Add(c); }

            foreach (var c in chain) {
                AddSpans(editsByFile, c.File, SignatureDropSpans(contentByFile[c.File], kindsByFile[c.File], offsetsByFile[c.File], c, indices));
                AddSpans(editsByFile, c.File, ParamDocSpans(linesByFile[c.File], offsetsByFile[c.File], c, indices));
            }

            foreach (var (file, spans) in sites) { AddSpans(editsByFile, file, spans); }

            var names = indices.Select(i => member.Parameters[i]);
            deleted.Add(Path.GetFileName(member.File) + ": " + MemberLabel(member) + "(" + string.Join(", ", names) + ")");
        }

        // Pass 4: veraltete Marker entfernen
        foreach (var entry in files) {
            foreach (var member in entry.Members) {
                if (member.Category == "Delegates") {
                    staleRemoved += TrailingMarkerSpan(entry, member.EndLine, editsByFile);
                    continue;
                }

                if (member.BodyStart >= 0) {
                    for (var j = member.BodyStart; j <= member.BodyEnd; j++) {
                        var line = entry.Lines[j];
                        var trimmed = line.TrimStart();

                        if (!trimmed.StartsWith(MarkerPrefix, StringComparison.Ordinal)) { continue; }

                        var start = entry.Offsets[j] + line.Length - trimmed.Length;
                        if (entry.Kinds[start] != CsSanitizer.KindComment) { continue; }

                        AddSpans(editsByFile, entry.File, [(entry.Offsets[j], entry.Offsets[j + 1])]);
                        staleRemoved++;
                    }
                }

                if (member.BodyOpenLine >= 0 && member.SameLineBody) {
                    staleRemoved += TrailingMarkerSpan(entry, member.BodyOpenLine, editsByFile);
                }
            }
        }

        // Pass 5: Dateien schreiben
        var filesChanged = 0;

        foreach (var (file, spans) in editsByFile) {
            var content = contentByFile[file];
            var result = ApplySpans(content, spans);

            if (result == content) { continue; }

            File.WriteAllText(file, result, new UTF8Encoding(true));
            filesChanged++;
        }

        var report = new StringBuilder();
        report.AppendLine("=== Ungenutzte Parameter ===");
        report.AppendLine(CultureInfo.InvariantCulture, $"Parameter gelöscht:        {deleted.Count}");
        report.AppendLine(CultureInfo.InvariantCulture, $"Veraltete Marker entfernt: {staleRemoved}");
        report.AppendLine(CultureInfo.InvariantCulture, $"Dateien geändert:          {filesChanged}");
        foreach (var entry in deleted) { report.AppendLine("  " + entry); }
        AppendSkipped(report, skipped);
        return report.ToString();
    }

    public override List<GenericControl> GetOptions(int widthOfControl) {
        var result = new List<GenericControl> {
            new FlexiControl("Optionen:", widthOfControl, true),
            new FlexiControl(QuickInfo, widthOfControl, false)
        };

        var btnSkip = new Button {
            ButtonStyle = ButtonStyle.Yes_or_No,
            Checked = _skipEventHandlers,
            Size = new Size(widthOfControl, RowHeight),
            Text = "Event-Handler ausnehmen (sender / e)"
        };
        btnSkip.Click += BtnSkip_Click;
        result.Add(btnSkip);

        return result;
    }

    public override JsonObject SaveSettings() {
        var json = new JsonObject();
        json.Set("SkipEventHandlers", _skipEventHandlers);
        return json;
    }

    public override void LoadSettings(JsonObject settings) => _skipEventHandlers = settings.GetBool("SkipEventHandlers", true);

    /// <summary>
    /// Liefert true, wenn der Parametername im bereinigten Member vorkommt; auf der Körper-Kopf-Zeile startet die
    /// Körper-Suche hinter dem Körper-Kopf, damit die Signatur nicht mitzählt. Argumente von Basis-/Chaining-Aufrufen
    /// (z. B. ': this(param)') und Constraints zwischen Parameterliste und Körper zählen als Verwendung.
    /// Marker-Kommentare sind bereinigt und stören nicht.
    /// </summary>
    private static bool BodyContains(List<string> clean, CsMemberInfo member, string name) {
        // Region zwischen Parameterliste und Körper-Kopf: Chaining-Argumente und Constraints
        if (member.ParamCloseLine >= 0 && member.BodyOpenLine >= 0) {
            for (var j = member.ParamCloseLine; j <= member.BodyOpenLine; j++) {
                var from = j == member.ParamCloseLine ? Math.Max(member.ParamCloseColumn, 0) : 0;
                var to = j == member.BodyOpenLine ? Math.Max(member.BodyOpenColumn, 0) : clean[j].Length;
                var text = clean[j][from..to];

                foreach (Match m in IdentifierRegex().Matches(text)) {
                    if (m.Value == name) { return true; }
                }
            }
        }

        // Körper inklusive Kopfzeile; dort erst hinter dem Körper-Kopf, damit die Signatur nicht mitzählt
        for (var j = Math.Min(member.BodyStart, member.BodyOpenLine); j <= member.BodyEnd; j++) {
            var text = j == member.BodyOpenLine ? clean[j][Math.Max(member.BodyOpenColumn, 0)..] : clean[j];

            foreach (Match m in IdentifierRegex().Matches(text)) {
                if (m.Value == name) { return true; }
            }
        }

        return false;
    }

    /// <summary>
    /// Liefert true, wenn der Parameter der Kette gelöscht werden darf: keine Implementierung benutzt ihn,
    /// keine gleichnamige Überladung mit Ziel-Arity besteht und die Signatur aller Ketten-Member editierbar ist.
    /// </summary>
    private static bool IsDeletable(CsMemberInfo member, int index, List<CsMemberInfo> chain, Dictionary<string, List<CsMemberInfo>> methodsByName, Dictionary<string, List<CsMemberInfo>> ctorsByType, Dictionary<string, List<string>> visibleByFile, Dictionary<string, string> contentByFile, Dictionary<string, string> kindsByFile, Dictionary<string, int[]> offsetsByFile) {
        var name = member.Parameters[index];
        var arity = member.Parameters.Count;

        // Keine Implementierung der Kette darf den Parameter benutzen; die Form muss übereinstimmen,
        // damit der Index in allen Ketten-Membern dasselbe Parameter bezeichnet
        foreach (var c in chain) {
            if (c.Parameters.Count != arity) { return false; }
            if (c.HasBody && BodyContains(visibleByFile[c.File], c, name)) { return false; }
        }

        // Kein gleichnamiger Member mit Ziel-Arity darf die Eindeutigkeit der Aufrufstellen brechen
        var siblings = member.Category == "Constructors" ? ctorsByType.GetValueOrDefault(member.TypeName) ?? [] : methodsByName.GetValueOrDefault(member.Name) ?? [];

        foreach (var other in siblings) {
            if (!chain.Contains(other) && (other.Parameters.Count == arity || other.Parameters.Count == arity - 1)) { return false; }
        }

        // params-Parameter sind textlich nicht sicher zuordenbar; die Signatur muss in allen Ketten-Membern editierbar sein
        foreach (var c in chain) {
            if (SignatureDropSpans(contentByFile[c.File], kindsByFile[c.File], offsetsByFile[c.File], c, [index]).Count == 0) { return false; }
        }

        return true;
    }

    /// <summary>
    /// Liefert die zu entfernenden Textbereiche für die Parameter-Indizes einer Signatur; bei Anomalien
    /// (fremde Segmentnamen, params-Parameter) eine leere Liste.
    /// </summary>
    private static List<(int Start, int End)> SignatureDropSpans(string content, string kinds, int[] offsets, CsMemberInfo member, List<int> indices) {
        if (member.ParamOpenLine < 0 || member.ParamCloseLine < 0) { return []; }

        foreach (var i in indices) {
            if (i < 0 || i >= member.Parameters.Count) { return []; }
        }

        var openPos = offsets[member.ParamOpenLine] + member.ParamOpenColumn;
        var closePos = offsets[member.ParamCloseLine] + member.ParamCloseColumn;
        var (segments, separators) = CsCodeScan.ParamSpans(content, kinds, openPos, closePos);

        if (segments.Count != member.Parameters.Count) { return []; }

        foreach (var i in indices) {
            var segment = content[segments[i].Start..segments[i].End];

            if (CsCodeScan.ParamNameOf(segment) != member.Parameters[i] || FirstWordOf(segment) == "params") { return []; }
        }

        // Zusammenhängende Index-Gräben zu Bereichen verschmelzen
        var result = new List<(int Start, int End)>();
        var runStart = -1;
        var previous = -2;

        foreach (var i in indices.OrderBy(i => i)) {
            if (runStart >= 0 && i != previous + 1) {
                result.Add(RunSpan(runStart, previous, segments, separators));
                runStart = -1;
            }

            if (runStart < 0) { runStart = i; }
            previous = i;
        }

        if (runStart >= 0) { result.Add(RunSpan(runStart, previous, segments, separators)); }

        return result;
    }

    /// <summary>
    /// Liefert den Textbereich eines zusammenhängenden Parameter-Grabens; der erste Graben nimmt den folgenden
    /// Trenner mit, der letzte den vorangehenden, damit keine Komma-Reste bleiben.
    /// </summary>
    private static (int Start, int End) RunSpan(int first, int last, List<(int Start, int End)> segments, List<(int Start, int End)> separators) {
        var lastIx = segments.Count - 1;
        var start = first > 0 ? separators[first - 1].Start : segments[first].Start;
        var end = last < lastIx ? separators[last].End : segments[last].End;
        return (start, end);
    }

    /// <summary>
    /// Liefert Bereiche zum Entfernen der &lt;param&gt;-Doc-Einträge der gelöschten Parameter; verflochtete
    /// Einträge bleiben stehen.
    /// </summary>
    private static List<(int Start, int End)> ParamDocSpans(List<string> lines, int[] offsets, CsMemberInfo member, List<int> indices) {
        var result = new List<(int Start, int End)>();

        foreach (var i in indices) {
            var token = "<param name=\"" + member.Parameters[i] + "\"";

            for (var j = member.TriviaStart; j < member.HeaderStart; j++) {
                if (!lines[j].Contains(token, StringComparison.Ordinal)) { continue; }

                var close = -1;

                for (var k = j; k < member.HeaderStart; k++) {
                    if (lines[k].Contains("</param>", StringComparison.Ordinal)) { close = k; break; }
                }

                if (close < 0) { break; }

                var endLine = lines[close];
                var after = endLine[(endLine.IndexOf("</param>", StringComparison.Ordinal) + 8)..];

                if (after.Trim().Length > 0) { break; } // Der Rest der Zeile gehört anderem Inhalt

                result.Add((offsets[j], offsets[close + 1]));
                break;
            }
        }

        return result;
    }

    /// <summary>
    /// Liefert je Datei die zu entfernenden Argument-Bereiche aller Methoden-Aufrufstellen;
    /// null, wenn eine Stelle nicht sicher umschreibbar ist.
    /// </summary>
    private static List<(string File, List<(int Start, int End)> Spans)>? MethodCallSpans(CsMemberInfo member, List<int> indices, List<(string File, List<string> Lines, string Content, string Kinds, int[] Offsets, List<CsMemberInfo> Members)> files, Dictionary<string, List<(int Start, int End)>> headerRangesByFile) {
        var name = member.Name;
        var result = new List<(string File, List<(int Start, int End)> Spans)>();

        foreach (var entry in files) {
            var content = entry.Content;
            var kinds = entry.Kinds;
            var ranges = headerRangesByFile[entry.File];
            var spans = new List<(int Start, int End)>();
            var pos = 0;

            while ((pos = content.IndexOf(name, pos, StringComparison.Ordinal)) >= 0) {
                var end = pos + name.Length;
                var boundary = (pos == 0 || !IsWordChar(content[pos - 1])) && (end >= content.Length || !IsWordChar(content[end]));

                // Empfänger-Aufrufe (obj.M(...), base.M(...)) sind eingeschlossen; nur Deklarations-Köpfe und Attribute sind ausgenommen
                if (boundary && kinds[pos] == CsSanitizer.KindCode && !InRanges(ranges, pos)) {
                    var j = end;
                    while (j < content.Length && char.IsWhiteSpace(content[j])) { j++; }

                    var attribute = false;
                    var b = pos - 1;
                    while (b >= 0 && char.IsWhiteSpace(content[b])) { b--; }
                    if (b >= 0 && content[b] == '[') { attribute = true; }

                    var openPos = -1;

                    if (!attribute && j < content.Length) {
                        if (content[j] == '(') {
                            openPos = j;
                        } else if (content[j] == '<') {
                            var closeAngle = MatchingAngle(content, kinds, j);

                            if (closeAngle >= 0) {
                                var k = closeAngle + 1;
                                while (k < content.Length && char.IsWhiteSpace(content[k])) { k++; }
                                if (k < content.Length && content[k] == '(') { openPos = k; }
                            }
                        }
                    }

                    if (openPos >= 0) {
                        var (args, closed) = ParseArgs(content, kinds, openPos);

                        if (closed && args.Count == member.Parameters.Count) {
                            var removal = ArgRemovalSpans(args, indices, member.Parameters, content, kinds);
                            if (removal is null) { return null; }
                            spans.AddRange(removal);
                        }
                    }
                }

                pos = end;
            }

            if (spans.Count > 0) { result.Add((entry.File, spans)); }
        }

        return result;
    }

    /// <summary>
    /// Liefert je Datei die zu entfernenden Argument-Bereiche aller Konstruktor-Aufrufstellen
    /// ('new Typ(' sowie ': this(' und ': base(' in Konstruktor-Köpfen); null bei nicht sicher umschreibbaren Stellen.
    /// </summary>
    private static List<(string File, List<(int Start, int End)> Spans)>? CtorCallSpans(CsMemberInfo member, List<int> indices, List<(string File, List<string> Lines, string Content, string Kinds, int[] Offsets, List<CsMemberInfo> Members)> files, Dictionary<string, string> baseByType, Dictionary<string, List<(int Start, int End)>> headerRangesByFile) {
        var typeName = member.TypeName;
        var result = new List<(string File, List<(int Start, int End)> Spans)>();
        var pattern = "new " + typeName;

        foreach (var entry in files) {
            var content = entry.Content;
            var kinds = entry.Kinds;
            var spans = new List<(int Start, int End)>();
            var pos = 0;

            while ((pos = content.IndexOf(pattern, pos, StringComparison.Ordinal)) >= 0) {
                var end = pos + pattern.Length;
                var boundary = (pos == 0 || !IsWordChar(content[pos - 1])) && (end >= content.Length || !IsWordChar(content[end]));

                if (boundary) {
                    var j = end;
                    while (j < content.Length && char.IsWhiteSpace(content[j])) { j++; }

                    if (j < content.Length && content[j] == '(') {
                        var (args, closed) = ParseArgs(content, kinds, j);

                        if (closed && args.Count == member.Parameters.Count) {
                            var removal = ArgRemovalSpans(args, indices, member.Parameters, content, kinds);
                            if (removal is null) { return null; }
                            spans.AddRange(removal);
                        }
                    }
                }

                pos = end;
            }

            // Chaining-Aufrufe in Konstruktor-Köpfen
            var members = entry.Members;
            var ranges = headerRangesByFile[entry.File];

            for (var m = 0; m < members.Count; m++) {
                var ctor = members[m];

                if (ctor.Category != "Constructors") { continue; }

                var sameType = ctor.TypeName == typeName;
                var isDerived = baseByType.GetValueOrDefault(ctor.TypeName) == typeName;

                if (!sameType && !isDerived) { continue; }

                var token = sameType ? "this" : "base";
                var range = ranges[m];
                var p = range.Start;

                while (p < range.End) {
                    var hit = content.IndexOf(token, p, StringComparison.Ordinal);
                    if (hit < 0 || hit >= range.End) { break; }

                    var colonOk = false;
                    var b = hit - 1;
                    while (b >= range.Start && char.IsWhiteSpace(content[b])) { b--; }
                    if (b >= range.Start && content[b] == ':') { colonOk = true; }

                    var j = hit + token.Length;
                    while (j < content.Length && char.IsWhiteSpace(content[j])) { j++; }

                    if (colonOk && j < content.Length && content[j] == '(') {
                        var (args, closed) = ParseArgs(content, kinds, j);

                        if (closed && args.Count == member.Parameters.Count) {
                            var removal = ArgRemovalSpans(args, indices, member.Parameters, content, kinds);
                            if (removal is null) { return null; }
                            spans.AddRange(removal);
                        }
                    }

                    p = hit + token.Length;
                }
            }

            if (spans.Count > 0) { result.Add((entry.File, spans)); }
        }

        return result;
    }

    /// <summary>
    /// Liefert false, wenn der Name im Projekt als bloße Referenz ohne Aufrufklammer vorkommt
    /// (Methodengruppe, Delegate-Bindung); solche Stellen brechen beim Löschen.
    /// </summary>
    private static bool NameReferencesAreCalls(string name, List<(string File, List<string> Lines, string Content, string Kinds, int[] Offsets, List<CsMemberInfo> Members)> files) {
        foreach (var entry in files) {
            var content = entry.Content;
            var kinds = entry.Kinds;
            var pos = 0;

            while ((pos = content.IndexOf(name, pos, StringComparison.Ordinal)) >= 0) {
                var end = pos + name.Length;
                var boundary = (pos == 0 || !IsWordChar(content[pos - 1])) && (end >= content.Length || !IsWordChar(content[end]));

                if (boundary && kinds[pos] == CsSanitizer.KindCode && (pos == 0 || content[pos - 1] != '.')) {
                    var j = end;
                    while (j < content.Length && char.IsWhiteSpace(content[j])) { j++; }

                    if (j >= content.Length || content[j] is not ('(' or '<')) { return false; }
                }

                pos = end;
            }
        }

        return true;
    }

    /// <summary>
    /// Zerlegt die Argumentliste ab der öffnenden Klammer; Strings und Kommentare stören die Trennung nicht.
    /// </summary>
    private static (List<(int Start, int End)> Args, bool Closed) ParseArgs(string content, string kinds, int openPos) {
        var args = new List<(int Start, int End)>();
        var paren = 0;
        var brace = 0;
        var bracket = 0;
        var argStart = -1;
        var hasCode = false;

        for (var i = openPos + 1; i < content.Length; i++) {
            if (kinds[i] != CsSanitizer.KindCode) { continue; }

            var c = content[i];

            if (paren == 0 && brace == 0 && bracket == 0) {
                switch (c) {
                    case ')':
                        if (hasCode) { args.Add((argStart, i)); }
                        return (args, true);
                    case ',':
                        args.Add((argStart, i));
                        argStart = -1;
                        hasCode = false;
                        continue;
                }
            }

            switch (c) {
                case '(':
                    paren++;
                    break;
                case ')':
                    paren--;
                    break;
                case '{':
                    brace++;
                    break;
                case '}':
                    brace--;
                    break;
                case '[':
                    bracket++;
                    break;
                case ']':
                    bracket--;
                    break;
            }

            if (!char.IsWhiteSpace(c)) {
                if (argStart < 0) { argStart = i; }
                hasCode = true;
            }
        }

        return (args, false);
    }

    /// <summary>
    /// Liefert die zu entfernenden Bereiche der gelöschten Indizes; bei benannten Argumenten werden die
    /// Stellen über den Namen aufgelöst, gemischte benannte Listen liefern null.
    /// </summary>
    private static List<(int Start, int End)>? ArgRemovalSpans(List<(int Start, int End)> args, List<int> indices, List<string> parameters, string content, string kinds) {
        var named = 0;

        foreach (var arg in args) {
            if (ArgNameOf(arg, content, kinds) is not null) { named++; }
        }

        if (named > 0 && named < args.Count) { return null; }

        var result = new List<(int Start, int End)>();

        foreach (var i in indices) {
            var index = i;

            if (named == args.Count) {
                index = -1;

                for (var a = 0; a < args.Count; a++) {
                    if (ArgNameOf(args[a], content, kinds) == parameters[i]) { index = a; break; }
                }

                if (index < 0) { return null; }
            }

            if (index == 0) {
                // Ohne weitere Argumente endet die Liste an der Klammer, sonst kommt das Komma mit weg
                var end = args.Count == 1 ? args[0].End : args[0].End + 1;
                result.Add((args[0].Start, end));
                continue;
            }

            // Rückwärts bis zum trennenden Komma laufen (Leerraum und Kommentare gehören zur Entfernung)
            var start = args[index].Start;
            var b = start - 1;
            while (b >= 0 && (kinds[b] != CsSanitizer.KindCode || char.IsWhiteSpace(content[b]))) { b--; }

            // b steht auf dem Komma; es gehört mit zur Entfernung
            result.Add((b, args[index].End));
        }

        return result;
    }

    /// <summary>
    /// Liefert den Namen eines benannten Arguments; ohne Namens-präfix null.
    /// </summary>
    private static string? ArgNameOf((int Start, int End) arg, string content, string kinds) {
        var i = arg.Start;
        while (i < arg.End && char.IsWhiteSpace(content[i])) { i++; }

        var start = i;
        while (i < arg.End && IsWordChar(content[i])) { i++; }

        if (i == start || i >= arg.End) { return null; }

        var name = content[start..i];
        while (i < arg.End && char.IsWhiteSpace(content[i])) { i++; }

        if (i < arg.End && content[i] == ':' && kinds[i] == CsSanitizer.KindCode) { return name; }

        return null;
    }

    /// <summary>
    /// Liefert die Position der schließenden generischen Klammer; -1 ohne Balance.
    /// </summary>
    private static int MatchingAngle(string content, string kinds, int openPos) {
        var depth = 0;

        for (var i = openPos; i < content.Length; i++) {
            if (kinds[i] != CsSanitizer.KindCode) { continue; }

            if (content[i] == '<') { depth++; }

            if (content[i] == '>') {
                depth--;
                if (depth == 0) { return i; }
            }
        }

        return -1;
    }

    /// <summary>
    /// Liefert true, wenn die Position innerhalb eines der Bereiche liegt.
    /// </summary>
    private static bool InRanges(List<(int Start, int End)> ranges, int position) {
        foreach (var (start, end) in ranges) {
            if (position >= start && position < end) { return true; }
        }

        return false;
    }

    /// <summary>
    /// Entfernt einen Zeilenenden-Marker der Art 'TODO: Unused: ...' und liefert 1 bei Erfolg.
    /// </summary>
    private static int TrailingMarkerSpan((string File, List<string> Lines, string Content, string Kinds, int[] Offsets, List<CsMemberInfo> Members) entry, int line, Dictionary<string, List<(int Start, int End)>> editsByFile) {
        var text = entry.Lines[line];
        var m = TrailingMarkerRegex().Match(text);

        if (!m.Success) { return 0; }

        var start = entry.Offsets[line] + m.Index;
        if (entry.Kinds[start] != CsSanitizer.KindComment) { return 0; }

        AddSpans(editsByFile, entry.File, [(start, entry.Offsets[line] + text.Length)]);
        return 1;
    }

    /// <summary>
    /// Entfernt die Bereiche aus dem Text; sich überschneidende Bereiche werden verworfen (der äußere gewinnt).
    /// </summary>
    private static string ApplySpans(string content, List<(int Start, int End)> spans) {
        var accepted = new List<(int Start, int End)>();

        foreach (var span in spans.OrderBy(s => s.Start)) {
            if (accepted.Count > 0 && span.Start < accepted[^1].End) { continue; }
            accepted.Add(span);
        }

        if (accepted.Count == 0) { return content; }

        var result = new StringBuilder(content.Length);
        var pos = 0;

        foreach (var (start, end) in accepted) {
            result.Append(content[pos..start]);
            pos = Math.Max(pos, end);
        }

        result.Append(content[pos..]);
        return result.ToString();
    }

    /// <summary>
    /// Hängt die Bereiche je Datei an die Sammlung an.
    /// </summary>
    private static void AddSpans(Dictionary<string, List<(int Start, int End)>> editsByFile, string file, List<(int Start, int End)> spans) {
        if (spans.Count == 0) { return; }

        if (!editsByFile.TryGetValue(file, out var list)) { list = []; editsByFile.Add(file, list); }
        list.AddRange(spans);
    }

    /// <summary>
    /// Liefert das erste Wort eines Texts; ohne Wort einen leeren String.
    /// </summary>
    private static string FirstWordOf(string text) {
        var m = IdentifierRegex().Match(text);
        return m.Success ? m.Value : string.Empty;
    }

    /// <summary>
    /// Liefert true, wenn das Zeichen ein Wortzeichen (Buchstabe, Ziffer, Unterstrich) ist.
    /// </summary>
    private static bool IsWordChar(char c) => char.IsLetterOrDigit(c) || c == '_';

    /// <summary>
    /// Liefert true, wenn der Parameter nach dem Event-Handler-Muster heißt (sender oder e).
    /// </summary>
    private static bool IsEventParameter(string parameter) => parameter is "sender" or "e";

    /// <summary>
    /// Liefert das Anzeige-Label des Members; Operatoren haben keinen Namen und fallen auf den Typnamen zurück.
    /// </summary>
    private static string MemberLabel(CsMemberInfo member) => member.Name is { Length: > 0 } ? member.Name : member.TypeName;

    private void BtnSkip_Click(object? sender, EventArgs e) {
        if (sender is not Button btn) { return; }

        _skipEventHandlers = btn.Checked;
        OnOptionsChanged();
    }

    [GeneratedRegex(@"[\p{L}_]\w*")]
    private static partial Regex IdentifierRegex();

    [GeneratedRegex(@"[ \t]*// TODO: Unused: .*$")]
    private static partial Regex TrailingMarkerRegex();

    #endregion
}
