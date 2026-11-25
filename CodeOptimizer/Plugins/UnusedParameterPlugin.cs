// Licensed under MIT; see License.md for disclaimer, details, and extended user conditions.

using System.Drawing;
using System.Text.Json.Nodes;

namespace CodeOptimizer.Plugins;

/// <summary>
/// Schreibt in Funktionen mit unbenutzten Parametern an erste Stelle des Körpers den Kommentar 'TODO: Unused: Name'.
/// Signaturen und Aufrufe bleiben unverändert - überschriebene und Interface-implementierende Funktionen behalten
/// ihre Parameter und werden deshalb nur markiert. Bei Einzeilern (Expression-Bodied, '{ }') steht der Marker
/// als Kommentar am Zeilenende. Beim Entfernen veralteter Marker werden nur selbst geschriebene Marker gelöscht.
/// </summary>
public sealed partial class UnusedParameterPlugin : CodeOptimizerPlugin {

    #region Fields

    private const string MethodMarker = "// TODO: Unused";

    private const string MarkerPrefix = "// TODO: Unused: ";

    private const int RowHeight = 27;

    /// <summary>
    /// True: Parameter gemäß Event-Handler-Muster (sender, e) werden nicht als unbenutzt markiert.
    /// </summary>
    private bool _skipEventHandlers = true;

    #endregion

    #region Properties

    public override string Name => "Ungenutzte Parameter";

    public override int Order => 70;

    public override string QuickInfo => "Schreibt in Funktionen mit unbenutzten Parametern an erste Stelle 'TODO: Unused: Parametername'. Beachtet Überschreibungen und Interfaces: Die Signatur bleibt nicht anfassbar und wird nur markiert. Die Prüfung zählt reine Code-Vorkommen des Parameternamens im Funktionskörper (Strings und Kommentare zählen nicht). Event-Handler mit sender/e kann die Option ausnehmen.";

    public override ImageCode Symbol => ImageCode.Trichter;

    #endregion

    #region Methods

    private static void AppendTrailingMarker(List<string> lines, Dictionary<int, string> replacements, int line, List<string> names) {
        var current = replacements.TryGetValue(line, out var value) ? value : lines[line];
        var suffix = MarkerPrefix + string.Join(", ", names);
        replacements[line] = current.EndsWith(suffix, StringComparison.Ordinal) ? current : current.TrimEnd() + " " + suffix;
    }

    /// <summary>
    /// Liefert true, wenn der Parametername im bereinigten Member vorkommt; bei Einzeilern startet die Körper-Suche
    /// hinter dem Körper-Kopf, damit die Signatur nicht mitzählt. Argumente von Basis-/Chaining-Aufrufen
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

        for (var j = member.BodyStart; j <= member.BodyEnd; j++) {
            var text = member.SameLineBody && j == member.BodyOpenLine ? clean[j][member.BodyOpenColumn..] : clean[j];

            foreach (Match m in IdentifierRegex().Matches(text)) {
                if (m.Value == name) { return true; }
            }
        }

        return false;
    }

    public override string Execute(string rootPath) {
        var skipped = new List<string>();
        var marked = new List<string>();
        var staleRemoved = 0;
        var filesChanged = 0;

        foreach (var file in SourceFiles.Enumerate(rootPath)) {
            // Designer-Dateien sind generiert und werden nicht angetastet
            if (file.EndsWith(".Designer.cs", StringComparison.OrdinalIgnoreCase)) { continue; }

            // Dateien ohne gültiges UTF-8 dürfen nicht umgeschrieben werden - das übernimmt das Encoding-Modul
            if (!SourceFiles.IsUtf8(file)) {
                skipped.Add(file);
                continue;
            }

            var (lines, clean) = CsCodeScan.Load(file);
            var members = CsCodeScan.Members(file, lines, clean);
            var visible = new CsSanitizer().SanitizeLinesWithCodeHoles(lines);
            var edits = new List<(int Line, int Seq, string? Text)>(); // Text null = Zeile entfernen
            var replacements = new Dictionary<int, string>();

            foreach (var member in members) {
                if (!member.HasBody || member.Parameters.Count == 0) { continue; }
                if (HasMethodMarker(lines, member)) { continue; } // Member ist ohnehin als unbenutzt markiert

                var skipEventParameters = _skipEventHandlers && IsEventHandler(member);
                var trailingNames = new List<string>();

                foreach (var parameter in member.Parameters) {
                    if (parameter == "_") { continue; }

                    var used = BodyContains(visible, member, parameter) || (skipEventParameters && IsEventParameter(parameter));
                    var markers = MarkerLines(lines, member, parameter);

                    if (used) {
                        foreach (var j in markers) {
                            edits.Add((j, edits.Count, null));
                            staleRemoved++;
                        }

                        if (member.SameLineBody && RemoveTrailingName(lines, replacements, member.BodyOpenLine, parameter)) { staleRemoved++; }

                        continue;
                    }

                    if (markers.Count > 0) { continue; }

                    if (member.SameLineBody) {
                        // Marker als Zeilenenden-Kommentar; alle Parameter eines Members landen in einem Marker
                        if (trailingNames.Contains(parameter) || TrailingNames(lines[member.BodyOpenLine]).Contains(parameter)) { continue; }
                        trailingNames.Add(parameter);
                    } else {
                        // Alle Marker direkt hintereinander nach dem Körper-Kopf
                        edits.Add((member.BodyOpenLine + 1, edits.Count, IndentOf(lines[member.BodyOpenLine]) + "    " + MarkerPrefix + parameter));
                    }

                    marked.Add(Path.GetFileName(file) + ": " + MemberLabel(member) + "(" + parameter + ")");
                }

                if (trailingNames.Count > 0) { AppendTrailingMarker(lines, replacements, member.BodyOpenLine, trailingNames); }
            }

            if (edits.Count == 0 && replacements.Count == 0) { continue; }

            var original = string.Join("\r\n", lines);

            foreach (var line in replacements.Keys.OrderByDescending(l => l)) { lines[line] = replacements[line]; }

            // Absteigend anwenden; bei gleicher Zeile in umgekehrter Queue-Reihenfolge, damit die Queue-Reihenfolge erhalten bleibt
            foreach (var (line, _, text) in edits.OrderByDescending(e => e.Line).ThenByDescending(e => e.Seq)) {
                if (text is null) { lines.RemoveAt(line); } else { lines.Insert(line, text); }
            }

            var result = string.Join("\r\n", lines);

            // No-op-Änderungen (z. B. identischer Zeilenenden-Marker) dürfen die Datei nicht anfassen
            if (result == original) { continue; }

            File.WriteAllText(file, result, new UTF8Encoding(true));
            filesChanged++;
        }

        var report = new StringBuilder();
        report.AppendLine("=== Ungenutzte Parameter ===");
        report.AppendLine(CultureInfo.InvariantCulture, $"Parameter markiert:        {marked.Count}");
        report.AppendLine(CultureInfo.InvariantCulture, $"Veraltete Marker entfernt: {staleRemoved}");
        report.AppendLine(CultureInfo.InvariantCulture, $"Dateien geändert:          {filesChanged}");
        foreach (var entry in marked) { report.AppendLine("  " + entry); }
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
    /// True, wenn eine Zeile der Trivia des Members den Methoden-Marker 'TODO: Unused' trägt.
    /// </summary>
    private static bool HasMethodMarker(List<string> lines, CsMemberInfo member) {
        for (var j = member.TriviaStart; j < member.HeaderStart; j++) {
            if (lines[j].TrimStart().StartsWith(MethodMarker, StringComparison.Ordinal)) { return true; }
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

    /// <summary>
    /// Liefert true, wenn die Signatur dem Event-Handler-Muster (sender, e) entspricht.
    /// </summary>
    private static bool IsEventHandler(CsMemberInfo member) => member.Parameters is ["sender", "e"];

    /// <summary>
    /// Liefert true, wenn der Parameter nach dem Event-Handler-Muster heißt (sender oder e).
    /// </summary>
    private static bool IsEventParameter(string parameter) => parameter is "sender" or "e";

    /// <summary>
    /// Liefert die Zeilen des Körpers, die einen vollständigen Marker für den Parameter enthalten.
    /// </summary>
    private static List<int> MarkerLines(List<string> lines, CsMemberInfo member, string name) {
        var result = new List<int>();

        for (var j = member.BodyStart; j <= member.BodyEnd; j++) {
            if (lines[j].Trim().Equals(MarkerPrefix + name, StringComparison.Ordinal)) { result.Add(j); }
        }

        return result;
    }

    /// <summary>
    /// Liefert das Anzeige-Label des Members; Operatoren haben keinen Namen und fallen auf den Typnamen zurück.
    /// </summary>
    private static string MemberLabel(CsMemberInfo member) => member.Name is { Length: > 0 } ? member.Name : member.TypeName;

    /// <summary>
    /// Entfernt einen Parameternamen aus dem Zeilenenden-Marker; ohne verbleibende Namen verschwindet der Marker ganz.
    /// Liefert true, wenn der Name enthalten war und entfernt wurde.
    /// </summary>
    private static bool RemoveTrailingName(List<string> lines, Dictionary<int, string> replacements, int line, string name) {
        var source = replacements.TryGetValue(line, out var value) ? value : lines[line];

        var m = TrailingMarkerRegex().Match(source);
        if (!m.Success) { return false; }

        var names = m.Groups[1].Value.Split(',').Select(n => n.Trim()).ToList();

        if (!names.Contains(name)) { return false; }

        var remaining = names.Where(n => n != name).ToList();

        replacements[line] = remaining.Count == 0 ? source[..m.Index].TrimEnd() : source[..m.Index] + MarkerPrefix + string.Join(", ", remaining);
        return true;
    }

    [GeneratedRegex(@"// TODO: Unused: ([\p{L}_]\w*(?:, [\p{L}_]\w*)*)\s*$")]
    private static partial Regex TrailingMarkerRegex();

    /// <summary>
    /// Liefert die Parameternamen aus dem Zeilenenden-Marker eines Einzeilers; ohne Marker eine leere Liste.
    /// </summary>
    private static List<string> TrailingNames(string line) {
        var m = TrailingMarkerRegex().Match(line);

        if (!m.Success) { return []; }

        return [.. m.Groups[1].Value.Split(',').Select(n => n.Trim())];
    }

    private void BtnSkip_Click(object? sender, EventArgs e) {
        if (sender is not Button btn) { return; }

        _skipEventHandlers = btn.Checked;
        OnOptionsChanged();
    }

    #endregion
}
