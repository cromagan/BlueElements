// Licensed under AGPL-3.0; see License.md for disclaimer and details.

namespace BlueScript.Classes;

/// <summary>
/// Eigenständige Vorab-Prüfung eines Skript-Textes. Wird vor der
/// Test-Ausführung aufgerufen, um:
/// <list type="bullet">
///   <item>Alle <c>var</c>-Deklarationen zu ermitteln und die
///   Variablennamen zu sammeln.</item>
///   <item>Einen einfachen Syntax-Check durchzuführen: Welche
///   Befehle können nicht geparsed werden?</item>
/// </list>
/// Variablen-Deklarationen lösen niemals einen Fehler aus (nur
/// Protokoll-Einträge), da Variablen zur Laufzeit erzeugt werden
/// können (z.B. durch ImportLinked). Syntaktisch fehlerhafte
/// Methodenaufrufe verhindern die Ausführung.
/// </summary>
public static class ScriptPreCheck {

    #region Methods

    /// <summary>
    /// Prüft den übergebenen Skript-Text mit allen registrierten Methoden.
    /// </summary>
    public static ScriptPreCheckResult Check(string scriptText) => Check(scriptText, Method.AllMethods.Instances);

    /// <summary>
    /// Prüft den übergebenen Skript-Text mit den übergebenen Methoden.
    /// </summary>
    public static ScriptPreCheckResult Check(string scriptText, IEnumerable<Method> availableMethods) {
        var result = new ScriptPreCheckResult();

        var methodList = availableMethods as IList<Method> ?? availableMethods.ToList();

        var lookup = new Dictionary<string, List<Method>>(StringComparer.OrdinalIgnoreCase);
        var emptyStartMethods = new List<Method>();
        foreach (var m in methodList) {
            if (!lookup.TryGetValue(m.Command, out var list)) {
                list = [];
                lookup[m.Command] = list;
            }
            list.Add(m);

            if (m.StartSequence == string.Empty) { emptyStartMethods.Add(m); }
        }

        var nr = Script.NormalizedText(scriptText);
        if (nr.IsFailed) {
            result.SyntaxErrors.Add(nr.FailedReason);
            return result;
        }

        var normalized = (nr.Value as string) ?? string.Empty;
        CheckInternal(normalized, lookup, emptyStartMethods, result, "Main", 0);

        return result;
    }

    private static void CheckInternal(string text, Dictionary<string, List<Method>> lookup, List<Method> emptyStartMethods, ScriptPreCheckResult result, string context, int lineOffset) {
        var pos = 0;

        while (pos < text.Length) {
            if (text[pos] == '¶') {
                pos++;
                continue;
            }

            if (text[pos] == ';') {
                pos++;
                continue;
            }

            var previousPos = pos;

            // Bezeichner an aktueller Position extrahieren
            var idEnd = pos;
            while (idEnd < text.Length && AllowedCharsVariableName.Contains(text[idEnd])) {
                idEnd++;
            }

            var line = text.CountChar('¶', pos) + 1 + lineOffset;

            var needsScriptFixFromPhase2 = string.Empty;
            var matched = false;
            var newPos = pos;

            #region Phase 1: Methoden-Lookup über den exakten Bezeichner

            if (idEnd > pos && lookup.TryGetValue(text[pos..idEnd], out var matchingMethods)) {
                foreach (var thisC in matchingMethods) {
                    var f = thisC.CanDo(text, pos, false, new LogData(context, line));

                    // Phase 1: Exakter Command-Match → NeedsScriptFix ist ein echter Fehler
                    if (f.NeedsScriptFix) {
                        result.SyntaxErrors.Add($"[{context}, Zeile: {line}] {f.FailedReason}");
                        return;
                    }

                    if (!string.IsNullOrEmpty(f.FailedReason)) { continue; }

                    ProcessMatchedMethod(thisC, f, result, lookup, emptyStartMethods, context, line);
                    newPos = f.ContinueOrErrorPosition;
                    matched = true;
                    break;
                }
            }

            #endregion

            #region Phase 2: Methoden mit leerer StartSequence (z.B. var, do)

            if (!matched) {
                foreach (var thisC in emptyStartMethods) {
                    // Bereits in Phase 1 geprüft → überspringen
                    if (idEnd > pos && string.Equals(text[pos..idEnd], thisC.Command, StringComparison.OrdinalIgnoreCase)) { continue; }

                    var f = thisC.CanDo(text, pos, false, new LogData(context, line));

                    if (f.NeedsScriptFix) {
                        // Phase 2: Prefix-Match → könnte ein False Positive sein
                        // (z.B. "document" beginnt mit "do"). Merken, aber nicht
                        // sofort als Fehler behandeln — zuerst Variablen-Zuweisung prüfen.
                        needsScriptFixFromPhase2 = f.FailedReason;
                        continue;
                    }

                    if (!string.IsNullOrEmpty(f.FailedReason)) { continue; }

                    ProcessMatchedMethod(thisC, f, result, lookup, emptyStartMethods, context, line);
                    newPos = f.ContinueOrErrorPosition;
                    matched = true;
                    break;
                }
            }

            #endregion

            #region Phase 3: Variablen-Zuweisung (NAME = Wert;)

            // Unbekannte Variablen sind erlaubt — sie können zur Laufzeit
            // erzeugt werden (z.B. ImportLinked). Nur Command-Namen werden
            // ausgeschlossen, da diese keine Variablen sein können.
            if (!matched && idEnd > pos && idEnd < text.Length && text[idEnd] == '=') {
                if (!lookup.ContainsKey(text[pos..idEnd])) {
                    var f = Method.GetEnd(text, idEnd, 1, ";");
                    if (!f.Failed) {
                        newPos = f.ContinuePosition;
                        matched = true;
                    }
                }
            }

            #endregion

            if (!matched) {
                // Wenn Phase 2 einen NeedsScriptFix gemerkt hat, diesen melden
                // (z.B. "do" ohne Codeblock). Sonst: nicht parsebar.
                var bef = (text[pos..] + "¶").SplitBy("¶");
                var msg = string.IsNullOrEmpty(needsScriptFixFromPhase2)
                    ? "Kann nicht geparsed werden: " + bef[0]
                    : needsScriptFixFromPhase2;
                result.SyntaxErrors.Add($"[{context}, Zeile: {line}] {msg}");
                return;
            }

            // Sicherheits-Check: Position muss vorwärts wandern.
            if (newPos <= previousPos) {
                result.SyntaxErrors.Add($"[{context}, Zeile: {line}] Interner Fehler: Position wurde nicht vorwärts bewegt.");
                return;
            }

            pos = newPos;

            if (result.HasSyntaxErrors) { return; }
        }
    }

    /// <summary>
    /// Extrahiert den Variablennamen aus dem AttributText einer
    /// <c>var</c>-Deklaration. Der AttributText hat die Form
    /// <c>Name=Wert</c>. Der Name ist alles vor dem ersten <c>=</c>.
    /// </summary>
    private static string ExtractVarName(string attributText) {
        if (string.IsNullOrEmpty(attributText)) { return string.Empty; }

        var (eqPos, _) = NextText(attributText, 0, EqualsSign, false, false, null);
        if (eqPos < 1) { return string.Empty; }

        return attributText[..eqPos];
    }

    private static void ProcessMatchedMethod(Method thisC, CanDoFeedback f, ScriptPreCheckResult result, Dictionary<string, List<Method>> lookup, List<Method> emptyStartMethods, string context, int line) {
        // var-Deklaration: Variablennamen sammeln — KEINE Attribut-Prüfung,
        // da var VariablenBerechnung nutzt, nicht SplitAttributeToVars.
        if (thisC.Command.Equals(Method_Var.CommandText, StringComparison.OrdinalIgnoreCase)) {
            var varName = ExtractVarName(f.AttributText);
            if (varName is { Length: > 0 } && Variable.IsValidName(varName)) {
                result.VariableNames.Add(varName);
            } else {
                result.Protocol.Add($"[{context}, Zeile: {line}] Var-Deklaration konnte nicht aufgelöst werden: {f.AttributText}");
            }
        } else {
            // Alle anderen Methoden: Attribut-Anzahl prüfen.
            var countError = Method.CheckArgumentCount(thisC, f.AttributText);
            if (countError is { Length: > 0 }) {
                result.SyntaxErrors.Add($"[{context}, Zeile: {line}] {countError}");
                return;
            }
        }

        // Attribut-Text nach verschachtelten Methodenaufrufen durchsuchen.
        // Methoden können als Attribute anderer Methoden oder als Wert
        // einer var-Deklaration verwendet werden (z.B. Calculate("x")).
        // Diese müssen ebenfalls auf korrekte Attribut-Anzahl geprüft werden.
        if (f.AttributText is { Length: > 0 }) {
            ScanExpressionForMethodCalls(f.AttributText, lookup, result, context, line);
            if (result.HasSyntaxErrors) { return; }
        }

        // Code-Block (if, foreach, do) rekursiv prüfen
        if (!string.IsNullOrEmpty(f.CodeBlockAfterText)) {
            var blockText = f.CodeBlockAfterText;
            var blockLineOffset = line + blockText.CountChar('¶', blockText.IndexOf('{'));
            CheckInternal(blockText, lookup, emptyStartMethods, result, context + "/" + thisC.Command, blockLineOffset);
        }
    }

    /// <summary>
    /// Durchsucht einen Ausdrucks-Text nach Methodenaufrufen
    /// (Muster: <c>Bezeichner(...)</c>) und prüft deren Attribut-Anzahl.
    /// String-Literale werden dabei übersprungen. Verschachtelte Aufrufe
    /// wie <c>ToUpper(Calculate("1+1",0))</c> werden vollständig erfasst.
    /// </summary>
    private static void ScanExpressionForMethodCalls(string text, Dictionary<string, List<Method>> lookup, ScriptPreCheckResult result, string context, int line) {
        var pos = 0;
        var inString = false;

        while (pos < text.Length) {
            var ch = text[pos];

            // String-Literale verfolgen
            if (ch == '"') {
                inString = !inString;
                pos++;
                continue;
            }

            if (inString) {
                pos++;
                continue;
            }

            // Bezeichner suchen
            if (!AllowedCharsVariableName.Contains(ch)) {
                pos++;
                continue;
            }

            var idEnd = pos;
            while (idEnd < text.Length && AllowedCharsVariableName.Contains(text[idEnd])) {
                idEnd++;
            }

            // Nur Bezeichner gefolgt von "(" sind Methodenaufrufe
            if (idEnd < text.Length && text[idEnd] == '(' && lookup.TryGetValue(text[pos..idEnd], out var methods)) {
                foreach (var m in methods) {
                    if (m.StartSequence != "(") { continue; }

                    // Matching ")" finden und Argumente extrahieren
                    var f = Method.GetEnd(text, idEnd, 1, ")");
                    if (f.Failed) { break; }

                    var countError = Method.CheckArgumentCount(m, f.NormalizedText);
                    if (countError is { Length: > 0 }) {
                        result.SyntaxErrors.Add($"[{context}, Zeile: {line}] {countError}");
                        return;
                    }
                    break;
                }
            }

            pos = idEnd;
        }
    }

    #endregion
}