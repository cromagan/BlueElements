// Licensed under MIT; see License.md for disclaimer, details, and extended user conditions.

namespace CodeOptimizer.Classes;

/// <summary>
/// Findet textbasiert die Methoden- und Konstruktoren oberster Typdeklarationen und liefert je Member
/// Signatur, Parameter, Körperbereich und Modifikatoren. Kommentare und String-Inhalte werden über
/// CsSanitizer ausgeblendet, damit Treffer ausschließlich reinen Code betreffen.
/// Verschachtelte Typen und Member unter #if/#endif werden konservativ übersprungen.
/// </summary>
public static partial class CsCodeScan {

    #region Fields

    private static readonly char[] TokenChars = ['(', '{', ';', '='];

    private static readonly string[] ParameterKeywords = ["params", "ref", "out", "in", "this", "new", "scoped", "default", "where"];

    /// <summary>
    /// Schlüsselwörter, die nie ein Methodenname sein können und damit eine Klammjergruppe als Rückgabetyp kennzeichnen.
    /// </summary>
    private static readonly string[] SignatureKeywords = ["public", "private", "protected", "internal", "static", "virtual", "override", "abstract", "sealed", "async", "unsafe", "partial", "extern", "explicit", "implicit", "new", "event", "get", "set", "init", "add", "remove"];

    #endregion

    #region Methods

    private static int BracketBalance(string line) {
        var balance = 0;
        foreach (var c in line) {
            if (c == '[') { balance++; }
            if (c == ']') { balance--; }
        }
        return balance;
    }

    /// <summary>
    /// Ermittelt Kopfzeile, Körperbereich, Parameternamen und Modifikatoren eines Members.
    /// </summary>
    private static CsMemberInfo BuildMember(string file, List<string> clean, int start, int end, int triviaStart, string category, string typeName, bool typeIsInterface) {
        var bodyOpenLine = -1;
        var bodyOpenColumn = -1;
        var bodyStart = -1;
        var bodyEnd = -1;
        var hasBody = false;
        var sameLineBody = false;
        var expressionBodied = false;

        for (var j = start; j <= end; j++) {
            var t = clean[j];
            var semi = t.IndexOf(';');
            var brace = t.IndexOf('{');
            var arrow = t.IndexOf("=>", StringComparison.Ordinal);
            var hit = FirstHit(semi, brace, arrow);

            if (hit < 0) { continue; }

            if (hit == 0) { break; }

            if (hit == 1) {
                hasBody = true;
                bodyOpenLine = j;
                bodyOpenColumn = brace;
                sameLineBody = t.IndexOf('}', brace + 1) > brace;
                bodyStart = sameLineBody ? j : j + 1;
                bodyEnd = sameLineBody ? j : end;
                break;
            }

            // '=>' zuerst: Expression-Bodied-Member
            hasBody = true;
            expressionBodied = true;
            bodyOpenLine = j;
            bodyOpenColumn = arrow;
            sameLineBody = semi >= 0 && semi > arrow;
            bodyStart = sameLineBody ? j : j + 1;
            bodyEnd = sameLineBody ? j : end;
            break;
        }

        var (parameters, paramOpenLine, paramOpenColumn, paramCloseLine, paramCloseColumn) = ParseParameterList(clean, start, end, bodyOpenLine, bodyOpenColumn);
        var headerPre = TextBefore(clean, start, paramOpenLine >= 0 ? paramOpenLine : end, paramOpenLine >= 0 ? paramOpenColumn : 0);
        var preSlice = WithoutGenericSuffix(headerPre.TrimEnd());
        var nameEnd = preSlice.Length;
        while (nameEnd > 0 && IsWordChar(preSlice[nameEnd - 1])) { nameEnd--; }
        var name = preSlice[nameEnd..];

        return new CsMemberInfo {
            File = file,
            Name = name,
            TypeName = typeName,
            Category = category,
            TypeIsInterface = typeIsInterface,
            TriviaStart = triviaStart,
            HeaderStart = start,
            EndLine = end,
            BodyOpenLine = bodyOpenLine,
            BodyOpenColumn = bodyOpenColumn,
            ParamCloseLine = paramCloseLine,
            ParamCloseColumn = paramCloseColumn,
            BodyStart = bodyStart,
            BodyEnd = bodyEnd,
            HasBody = hasBody,
            SameLineBody = sameLineBody,
            ExpressionBodied = expressionBodied,
            Parameters = parameters,
            IsOverride = ContainsWord(headerPre, "override"),
            IsAbstract = ContainsWord(headerPre, "abstract"),
            IsVirtual = ContainsWord(headerPre, "virtual"),
            IsStatic = ContainsWord(headerPre, "static"),
            IsPartial = ContainsWord(headerPre, "partial"),
            IsExtern = ContainsWord(headerPre, "extern"),
            IsExplicitImplementation = nameEnd > 0 && preSlice[nameEnd - 1] == '.',
            IsPublic = ContainsWord(headerPre, "public")
        };
    }

    /// <summary>
    /// Ordnet einen Member anhand seines Kopfes einer Region zu. Leerzeichen = nicht zuordnungsbar.
    /// </summary>
    private static string Classify(string header, string typeName) {
        var p = header.IndexOfAny(TokenChars);
        if (p < 0) { return string.Empty; }

        var pre = header[..p];

        if (ContainsWord(pre, "delegate")) { return "Delegates"; }
        if (ContainsWord(pre, "event")) { return "Events"; }
        if (ContainsWord(pre, "struct")) { return "Structs"; }
        if (ContainsWord(pre, "enum")) { return "Enums"; }
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

    /// <summary>
    /// Zerlegt den Typkörper in Member-Blöcke und behält nur Methoden und Konstruktoren.
    /// </summary>
    private static void CollectTypeMembers(string file, List<string> lines, List<string> clean, int[] depths, int first, int last, int bodyDepth, string typeName, bool typeIsInterface, List<CsMemberInfo> result) {
        var triviaStart = -1;
        var i = first;

        while (i <= last) {
            var t = lines[i].Trim();

            if (t.Length == 0) { i++; continue; }

            if (t.StartsWith("#region", StringComparison.Ordinal) || t.StartsWith("#endregion", StringComparison.Ordinal)) { i++; continue; }

            if (t.StartsWith('#')) {
                // Strukturbildende Direktiven machen die Codeauswahl konfigurationsabhängig - konservativ abbrechen
                if (t.StartsWith("#if", StringComparison.Ordinal) || t.StartsWith("#elif", StringComparison.Ordinal) || t.StartsWith("#else", StringComparison.Ordinal) || t.StartsWith("#endif", StringComparison.Ordinal) || t.StartsWith("#line", StringComparison.Ordinal)) { return; }

                if (triviaStart < 0) { triviaStart = i; }
                i++;
                continue;
            }

            // Inhalt war reiner Kommentar- oder String-Inhalt (z. B. Blockkommentar-Fortsetzung)
            if (clean[i].Trim().Length == 0) {
                if (triviaStart < 0) { triviaStart = i; }
                i++;
                continue;
            }

            if (IsTriviaStart(t)) {
                if (triviaStart < 0) { triviaStart = i; }
                i++;

                // Mehrzeilige Attribute komplett übernehmen
                while (i <= last && BracketBalance(lines[i - 1]) > 0) { i++; }

                continue;
            }

            var end = MemberEndIndex(depths, clean, i, last, bodyDepth);
            var header = string.Join(" ", clean.GetRange(i, end - i + 1));
            var category = Classify(header, typeName);

            if (category is "Methods" or "Constructors") {
                result.Add(BuildMember(file, clean, i, end, triviaStart < 0 ? i : triviaStart, category, typeName, typeIsInterface));
            }

            triviaStart = -1;
            i = end + 1;
        }
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

    /// <summary>
    /// Schneidet den Teil hinter dem ersten Zuweisungs-= ab (Optional-Parameter mit Standardwert).
    /// </summary>
    private static string CutDefaultValue(string part) {
        for (var i = 0; i < part.Length; i++) {
            if (part[i] != '=') { continue; }

            var before = i > 0 ? part[i - 1] : ' ';
            var after = i + 1 < part.Length ? part[i + 1] : ' ';

            if (before is '=' or '!' or '<' or '>' or '+' or '-' or '*' or '/' or '%' || after is '=' or '>') { continue; }

            return part[..i];
        }

        return part;
    }

    /// <summary>
    /// Liefert je Zeile die Klammer-Tiefe nach Abarbeitung der Zeile.
    /// </summary>
    public static int[] Depths(List<string> clean) {
        var depths = new int[clean.Count];
        var depth = 0;

        for (var i = 0; i < clean.Count; i++) {
            depth += clean[i].Count('{') - clean[i].Count('}');
            depths[i] = depth;
        }

        return depths;
    }

    private static int FindTypeClose(int[] depths, List<string> clean, int open, int bodyDepth) {
        for (var j = open + 1; j < clean.Count; j++) {
            if (depths[j] == bodyDepth - 1 && clean[j].Trim() == "}") { return j; }
        }
        return -1;
    }

    /// <summary>
    /// Liefert 0 für erstes ';', 1 für erstes '{', 2 für erstes '=>' und -1, wenn keines vorkommt.
    /// </summary>
    private static int FirstHit(int semi, int brace, int arrow) {
        var best = -1;
        var kind = -1;

        if (semi >= 0) { best = semi; kind = 0; }
        if (brace >= 0 && (best < 0 || brace < best)) { best = brace; kind = 1; }

        // Ein Treffer vor dem bisher besten ist der endgültige Erste
        if (arrow >= 0 && (best < 0 || arrow < best)) { return 2; }

        return kind;
    }

    /// <summary>
    /// Liefert den Inhalt der Klammjergruppe als Text (mehrzeilig mit Leerzeichen verbunden).
    /// </summary>
    private static string GroupText(List<string> clean, int openLine, int openColumn, int closeLine, int closeColumn) {
        if (openLine == closeLine) { return clean[openLine][(openColumn + 1)..closeColumn]; }

        var sb = new StringBuilder(clean[openLine][(openColumn + 1)..]);

        for (var j = openLine + 1; j < closeLine; j++) { sb.Append(' ').Append(clean[j]); }

        sb.Append(' ').Append(clean[closeLine][..closeColumn]);
        return sb.ToString();
    }

    /// <summary>
    /// Liefert true, wenn das Wort direkt vor der Klammer ein Bezeichner ist (Methodenname) oder die Klammer
    /// auf ein Operatorsymbol nach 'operator' folgt; Klammern des Rückgabetyps scheitern an der Schlüsselwort-Prüfung.
    /// </summary>
    private static bool IsParameterList(List<string> clean, int start, int openLine, int openColumn) {
        var text = TextBefore(clean, start, openLine, openColumn).TrimEnd();
        var s = text.EndsWith('>') ? WithoutGenericSuffix(text) : text;

        if (s.Length == 0) { return false; }

        if (!IsWordChar(s[^1])) { return ContainsWord(text, "operator"); }

        var wordEnd = s.Length;
        while (wordEnd > 0 && IsWordChar(s[wordEnd - 1])) { wordEnd--; }
        var word = s[wordEnd..];

        return word is { Length: > 0 } && !SignatureKeywords.Contains(word);
    }

    private static bool IsTriviaStart(string t) => t.StartsWith("//", StringComparison.Ordinal) || t.StartsWith("/*", StringComparison.Ordinal) || t.StartsWith('*') || t.StartsWith('[');

    private static bool IsWordChar(char c) => char.IsLetterOrDigit(c) || c == '_';

    private static string LastIdentifier(string text) {
        var end = text.Length;
        while (end > 0 && !IsWordChar(text[end - 1])) { end--; }
        var start = end;
        while (start > 0 && IsWordChar(text[start - 1])) { start--; }
        return text[start..end];
    }

    /// <summary>
    /// Liefert die Zeile, in der der ab start beginnende Member endet.
    /// ';' beendet immer. '}' bei geöffnetem Block, bei Membern ohne Expression-Body oder bei Accessor-Einzeilern
    /// mit '{' vor '=>'. Expression-Bodied-Köpfe mit Property-Pattern am Zeilenende ('=> ... { ... }') laufen weiter.
    /// </summary>
    private static int MemberEndIndex(int[] depths, List<string> clean, int start, int last, int bodyDepth) {
        var blockOpened = false;
        var expressionBodied = false;

        for (var j = start; j <= last; j++) {
            if (depths[j] == bodyDepth) {
                var t = clean[j].TrimEnd();

                if (t.EndsWith(';')) { return j; }

                if (t.EndsWith('}')) {
                    var arrow = t.IndexOf("=>", StringComparison.Ordinal);
                    var brace = arrow >= 0 ? t.IndexOf('{') : -1;

                    // '}'-Zeilen mit '{' vor '=>' sind vollständige Einzeiler (Accessor/Lambda); '=>' vor '{'
                    // markiert den Kopf eines mehrzeiligen Expression-Bodied-Members mit Property-Pattern.
                    var oneLiner = arrow >= 0 && brace >= 0 && brace < arrow;

                    if (blockOpened || oneLiner || (!expressionBodied && arrow < 0)) { return j; }
                }
            }

            if (depths[j] > bodyDepth) { blockOpened = true; }
            if (clean[j].Contains("=>", StringComparison.Ordinal)) { expressionBodied = true; }
        }

        return last;
    }

    /// <summary>
    /// Liefert alle Methoden- und Konstruktoren-Member der obersten Typdeklarationen der Datei.
    /// Bei strukturbildenden Präprozessor-Direktiven (#if/#endif, #line) wird der betroffene Typ übersprungen.
    /// </summary>
    public static List<CsMemberInfo> Members(string file, List<string> lines, List<string> clean) {
        var result = new List<CsMemberInfo>();
        var depths = Depths(clean);
        var nsDepth = -1;
        var i = 0;

        while (i < lines.Count) {
            var depthBefore = i == 0 ? 0 : depths[i - 1];
            var openBrace = nsDepth < 0 || depthBefore == nsDepth ? TypeOpenBrace(i, clean) : -1;

            if (openBrace >= i && depths[openBrace] == depthBefore + 1 && (openBrace == i || depths[i] == depthBefore)) {
                var close = FindTypeClose(depths, clean, openBrace, depthBefore + 1);

                if (close > openBrace) {
                    if (nsDepth < 0) { nsDepth = depthBefore; }
                    var typeHeader = string.Join(" ", clean.GetRange(i, openBrace - i + 1));
                    CollectTypeMembers(file, lines, clean, depths, openBrace + 1, close - 1, depthBefore + 1, TypeName(clean[i]), ContainsWord(typeHeader, "interface"), result);
                    i = close + 1;
                    continue;
                }
            }

            i++;
        }

        return result;
    }

    /// <summary>
    /// Liefert das letzte Wortzeichen-Wort vor dem ermittelten Token.
    /// </summary>
    private static string NameBefore(string pre) {
        var s = pre.TrimEnd();
        var end = s.Length;
        while (end > 0 && IsWordChar(s[end - 1])) { end--; }
        return s[end..];
    }

    /// <summary>
    /// Liefert die Parameternamen der übergebenen Parameterliste; Standardwerte werden abgeschnitten.
    /// </summary>
    private static List<string> ParameterNamesOf(string list) {
        var result = new List<string>();

        // Parameter an Kommas auf oberster Ebene trennen; Klammern und generische Spitzen zählen mit
        var parts = new List<string>();
        var current = new StringBuilder();
        var paren = 0;
        var angle = 0;

        foreach (var ch in list) {
            if (ch == ',' && paren == 0 && angle == 0) {
                parts.Add(current.ToString());
                current.Clear();
                continue;
            }

            switch (ch) {
                case '(': paren++; break;
                case ')': paren--; break;
                case '<': angle++; break;
                case '>' when angle > 0: angle--; break;
            }

            current.Append(ch);
        }

        parts.Add(current.ToString());

        foreach (var part in parts) {
            var name = LastIdentifier(CutDefaultValue(part));

            if (name is { Length: > 0 } && !ParameterKeywords.Contains(name)) { result.Add(name); }
        }

        return result;
    }

    /// <summary>
    /// Ermittelt die Parameterliste des Members als erste Klammjergruppe, deren vorangestelltes Wort ein
    /// Bezeichner ist (nicht Schlüsselwort) bzw. auf ein 'operator'-Symbol folgt. Klammjergruppen des Rückgabetyps
    /// (z. B. '(int Line, int Column) Foo(...)') werden übersprungen. Liefert Namen sowie Öffnungs- und Schlussposition.
    /// </summary>
    private static (List<string> Names, int OpenLine, int OpenColumn, int CloseLine, int CloseColumn) ParseParameterList(List<string> clean, int start, int end, int bodyOpenLine, int bodyOpenColumn) {
        var depth = 0;
        var openLine = -1;
        var openColumn = -1;

        for (var j = start; j <= end; j++) {
            if (bodyOpenLine >= 0 && j > bodyOpenLine) { break; }

            var t = clean[j];
            var limit = j == bodyOpenLine && bodyOpenColumn >= 0 ? bodyOpenColumn : t.Length;

            for (var c = 0; c < limit; c++) {
                if (t[c] == '(') {
                    if (depth == 0) { (openLine, openColumn) = (j, c); }
                    depth++;
                    continue;
                }

                if (t[c] == ')') {
                    depth--;

                    if (depth == 0 && openLine >= 0) {
                        if (IsParameterList(clean, start, openLine, openColumn)) {
                            return (ParameterNamesOf(GroupText(clean, openLine, openColumn, j, c)), openLine, openColumn, j, c);
                        }

                        openLine = -1;
                    }

                    if (depth < 0) { depth = 0; }
                }
            }
        }

        return ([], -1, -1, -1, -1);
    }

    /// <summary>
    /// Liest eine Datei und liefert die Zeilen sowie die bereinigten Zeilen (Kommentare und Strings geleert).
    /// </summary>
    public static (List<string> Lines, List<string> Clean) Load(string file) {
        var lines = File.ReadAllText(file).Split(["\r\n", "\n"], StringSplitOptions.None).ToList();
        return (lines, new CsSanitizer().SanitizeLines(lines));
    }

    /// <summary>
    /// Liefert den bereinigten Text des Members bis zur angegebenen Position (mehrzeilig mit Leerzeichen verbunden).
    /// </summary>
    private static string TextBefore(List<string> clean, int start, int line, int column) {
        var sb = new StringBuilder();

        for (var j = start; j < line; j++) { sb.Append(' ').Append(clean[j]); }

        sb.Append(column <= clean[line].Length ? clean[line][..column] : clean[line]);
        return sb.ToString();
    }

    [GeneratedRegex(@"\b(class|struct|interface|record)\b")]
    private static partial Regex TypeKeywordRegex();

    private static string TypeName(string cleanLine) {
        var m = TypeNameRegex().Match(cleanLine);
        return m.Success ? m.Groups[1].Value : string.Empty;
    }

    [GeneratedRegex(@"\b(?:class|struct|interface|record)\s+(?!struct\b)([\p{L}_]\w*)")]
    private static partial Regex TypeNameRegex();

    /// <summary>
    /// Liefert den Index der öffnenden Klammer des ab start beginnenden Typs, oder -1. Erkennt ägyptische und Allman-Klammern.
    /// </summary>
    private static int TypeOpenBrace(int start, List<string> clean) {
        if (!TypeKeywordRegex().IsMatch(clean[start])) { return -1; }

        var limit = Math.Min(clean.Count, start + 5);
        for (var j = start; j < limit; j++) {
            var t = clean[j].TrimEnd();
            if (t.Length == 0) { continue; }
            if (t.EndsWith('{')) { return j; }
            if (t.EndsWith(';') || t.EndsWith('}')) { return -1; }
        }
        return -1;
    }

    /// <summary>
    /// Entfernt eine generische Parameterliste am Zeilenende, damit der Membername freiliegt.
    /// </summary>
    private static string WithoutGenericSuffix(string slice) {
        if (!slice.TrimEnd().EndsWith('>')) { return slice; }

        var depth = 0;
        for (var i = slice.Length - 1; i >= 0; i--) {
            var c = slice[i];
            if (c == '>') { depth++; }
            if (c == '<') {
                depth--;
                if (depth == 0) { return slice[..i]; }
            }
        }

        return slice;
    }

    #endregion
}
