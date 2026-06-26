// Licensed under AGPL-3.0; see License.md for disclaimer and details.

using BlueBasics.Enums;
using BlueScript.ClassesStatic;
using System.Diagnostics;
using static BlueBasics.ClassesStatic.Constants;

namespace BlueScript.Classes;

public class Script {

    #region Constructors

    public Script(VariableCollection? variablen, ScriptProperties scp) {
        NormalizedScriptText = string.Empty;

        Variables = variablen ?? [];

        //foreach (var thism in scp.AllowedMethods) {
        //    if (thism.Constants.Count > 0) {
        //        foreach (var thisValue in thism.Constants) {
        //            var varname = "c_" + thisValue.ToUpper().Replace(".", "_").Replace(" ", "_").Replace(":", "_").Replace("/", "_").Replace("\\", "_");

        //            var comment = string.Empty;
        //            if (Variables.Get(varname) is { IsDisposed: false } tmpvar) {
        //                comment = tmpvar.Comment;
        //                Variables.Remove(tmpvar);
        //            }
        //            if (!string.IsNullOrEmpty(comment)) { comment = comment + "\r\n"; }
        //            comment = comment + "Konstante aus " + thism.KeyName.ToUpper();

        //            Variables.Add(new VariableString(varname, thisValue, true, comment));
        //        }
        //    }
        //}

        Properties = scp;
    }

    #endregion

    #region Delegates

    public delegate string AbortReason();

    #endregion

    #region Properties

    public string NormalizedScriptText { get; private set; }
    public ScriptProperties Properties { get; }
    public string ScriptText { get; set; } = string.Empty;

    public VariableCollection Variables { get; }

    #endregion

    #region Methods

    /// <summary>
    /// Entfernt die Variablen Attribut0-9 und fügt diese mit neuen Werten hinzu.
    /// Wenn NULL übergeben wird, werden die vorhanden Variabelen NICHT verändert, aber auf 9 ergänzt
    /// </summary>
    /// <param name="vars"></param>
    /// <param name="args"></param>
    public static void AddAttributes(VariableCollection vars, List<string>? args) {
        if (args is not null) {
            // Attribute nur löschen, wenn neue vorhanden sind.
            // Ansonsten werden bei Try / If / For diese gelöscht
            vars.RemoveWithComment("Attribut");
            for (var z = 0; z < args.Count; z++) {
                vars.Add(new VariableString("Attribut" + z, args[z], true, "Attribut"));
            }
        }

        for (var z = args?.Count ?? 0; z < 10; z++) {
            var nam = "Attribut" + z;

            if (vars.GetByKey(nam) is null) {
                vars.Add(new VariableString("Attribut" + z, string.Empty, true, "Attribut"));
            }
        }
    }

    public static DoItWithEndedPosFeedback CommandOrVarOnPosition(VariableCollection varCol, ScriptProperties scp, string scriptText, int pos, bool expectedvariablefeedback, LogData ld) {

        #region  Einfaches Semikolon prüfen. Kann übrig bleiben, wenn eine Variable berechnet wurde, aber nicht verwendet wurde

        if (scriptText.Length > pos && scriptText[pos] == ';') {
            return new DoItWithEndedPosFeedback(false, pos + 1, false, false, string.Empty, null, ld);
        }

        #endregion

        #region Bezeichner an Position extrahieren

        var idEnd = pos;
        while (idEnd < scriptText.Length && AllowedCharsVariableName.Contains(scriptText[idEnd])) {
            idEnd++;
        }

        #endregion

        #region Befehle prüfen mit Lookup - CanDo und DoIt kombiniert (erster erfolgreiche Kandidat gewinnt)

        DoItFeedback? firstResult = null;

        if (idEnd > pos && scp.MethodLookup.TryGetValue(scriptText[pos..idEnd], out var matchingMethods)) {
            foreach (var thisC in matchingMethods) {
                var f = thisC.CanDo(scriptText, pos, expectedvariablefeedback, ld);
                if (f.NeedsScriptFix) { return new DoItWithEndedPosFeedback(f.FailedReason, true, ld); }
                if (!string.IsNullOrEmpty(f.FailedReason)) { continue; }

                // CanDo erfolgreich → sofort DoIt ausführen
                var scx = thisC.DoIt(varCol, f, scp);
                firstResult ??= scx;

                if (!scx.NeedsScriptFix && string.IsNullOrEmpty(scx.FailedReason)) {
                    return new DoItWithEndedPosFeedback(scx.NeedsScriptFix, f.ContinueOrErrorPosition, scx.BreakFired, scx.ReturnFired, scx.FailedReason, scx.ReturnValue, ld);
                }
            }
        }

        // Fallback für Methoden mit leerer StartSequence (z.B. 'var'):
        // Da die Normalisierung Leerzeichen entfernt, wird aus 'var t="hallo"' → 'vart="hallo"'.
        // Der Dictionary-Lookup findet dann 'var' nicht mehr unter dem Schlüssel 'vart'.
        // Methoden mit leerer StartSequence müssen per CanDo direkt auf den Text geprüft werden,
        // da sie kein Trennzeichen zwischen Befehl und Argument haben.
        if (firstResult is null) {
            foreach (var thisC in scp.AllowedMethods) {
                if (thisC.StartSequence != string.Empty) { continue; }
                var f = thisC.CanDo(scriptText, pos, expectedvariablefeedback, ld);
                if (f.NeedsScriptFix) { return new DoItWithEndedPosFeedback(f.FailedReason, true, ld); }
                if (!string.IsNullOrEmpty(f.FailedReason)) { continue; }

                var scx = thisC.DoIt(varCol, f, scp);
                firstResult ??= scx;

                if (!scx.NeedsScriptFix && string.IsNullOrEmpty(scx.FailedReason)) {
                    return new DoItWithEndedPosFeedback(scx.NeedsScriptFix, f.ContinueOrErrorPosition, scx.BreakFired, scx.ReturnFired, scx.FailedReason, scx.ReturnValue, ld);
                }
            }
        }

        if (firstResult is not null) {
            return new DoItWithEndedPosFeedback(firstResult.NeedsScriptFix, pos, firstResult.BreakFired, firstResult.ReturnFired, firstResult.FailedReason, firstResult.ReturnValue, ld);
        }

        #endregion

        #region Variablen prüfen per Dictionary-Lookup

        if (!expectedvariablefeedback && idEnd > pos && idEnd + 1 < scriptText.Length && scriptText[idEnd] == '=') {
            var varnam = scriptText[pos..idEnd];
            if (varCol.GetByKey(varnam) is { } thisV) {
                var f = Method.GetEnd(scriptText, idEnd, 1, ";", ld);
                if (f.Failed) {
                    return new DoItWithEndedPosFeedback("Ende der Variableberechnung von '" + thisV.KeyName + "' nicht gefunden.", true, ld);
                }

                var scx = Method.VariablenBerechnung(varCol, ld, scp, varnam + "=" + f.NormalizedText + ";", false);
                return new DoItWithEndedPosFeedback(scx.NeedsScriptFix, f.ContinuePosition, scx.BreakFired, scx.ReturnFired, scx.FailedReason, scx.ReturnValue, ld);
            }
        }

        #endregion

        #region Prüfen für bessere Fehlermeldung, ob der Rückgabetyp falsch gesetzt wurde

        if (idEnd > pos && scp.MethodLookup.TryGetValue(scriptText[pos..idEnd], out var errorMethods)) {
            foreach (var thisC in errorMethods) {
                var f = thisC.CanDo(scriptText, pos, !expectedvariablefeedback, ld);
                if (f.NeedsScriptFix) {
                    return new DoItWithEndedPosFeedback(f.FailedReason, true, ld);
                }

                if (string.IsNullOrEmpty(f.FailedReason)) {
                    if (expectedvariablefeedback) {
                        return new DoItWithEndedPosFeedback("Dieser Befehl hat keinen Rückgabewert: " + scriptText[pos..], true, ld);
                    }

                    return new DoItWithEndedPosFeedback("Dieser Befehl hat einen Rückgabewert, der nicht verwendet wird: " + scriptText[pos..], true, ld);
                }
            }
        }

        #endregion

        #region Prüfen für bessere Fehlermeldung, alle Befehle prüfen

        if (idEnd > pos && Method.AllMethodByCommand.TryGetValue(scriptText[pos..idEnd], out var allCandidates)) {
            foreach (var thisC in allCandidates) {
                var f = thisC.CanDo(scriptText, pos, expectedvariablefeedback, ld);
                if (string.IsNullOrEmpty(f.FailedReason)) {
                    return new DoItWithEndedPosFeedback("Dieser Befehl kann in diesen Skript nicht verwendet werden.", true, ld);
                }
            }
        }

        #endregion

        var bef = (scriptText[pos..] + "¶").SplitBy("¶");

        return new DoItWithEndedPosFeedback("Kann nicht geparsed werden: " + bef[0], true, ld);
    }

    public static (string f, string error) NormalizedText(string script) => script.RemoveEscape().NormalizedText(false, true, false, true, '¶');

    public static ScriptEndedFeedback Parse(VariableCollection varCol, ScriptProperties scp, string normalizedScriptText, int lineadd, string subname, AbortReason? abort) {
        var ifFound = scp.AllowedMethods.Any(thisC => string.Equals(thisC.Command, "if", StringComparison.Ordinal));

        if (!ifFound) {
            return new ScriptEndedFeedback("Interner Fehler: Programm nicht korrekt gestartet, bitte neu starten!", false, false, scp.ScriptName);
        }

        var pos = 0;

        var ld = new LogData(subname, lineadd + 1);

        Develop.Message(ErrorType.DevelopInfo, null, scp.MainInfo, ImageCode.Skript, $"Parsen: {scp.Chain} START", scp.Stufe);

        var t = Stopwatch.StartNew();

        do {
            if (pos >= normalizedScriptText.Length) {
                Develop.Message(ErrorType.DevelopInfo, null, scp.MainInfo, ImageCode.Skript, $"Parsen: {scp.Chain}\\[{pos + 1}] ENDE (Regulär)", scp.Stufe);

                return new ScriptEndedFeedback(varCol, ld.Protocol, false, false, false, string.Empty, null);
            }

            if (normalizedScriptText[pos] == '¶') {
                pos++;
                ld.LineAdd(1);
            } else {
                var previousPos = pos; // KRITISCHE ÄNDERUNG: Vorherige Position speichern
                var scx = CommandOrVarOnPosition(varCol, scp, normalizedScriptText, pos, false, ld);
                if (scx.Failed) {
                    if (!scp.SyntaxCheck || scx.NeedsScriptFix) {
                        // SyntaxCheck darf bei einfachen Fails nicht aufgeben, sonst werden echte Fehler nicht angezeigt
                        Develop.Message(ErrorType.DevelopInfo, null, scp.MainInfo, ImageCode.Skript, $"Parsen: {scp.Chain}\\[{pos + 1}] ENDE, da nicht erfolgreich {scx.FailedReason}", scp.Stufe);
                        return new ScriptEndedFeedback(varCol, ld.Protocol, scx.NeedsScriptFix, false, false, scx.FailedReason, null);
                    }
                }

                pos = scx.Position;

                // KRITISCHE ÄNDERUNG: Fortschrittsvalidierung
                if (pos <= previousPos) {
                    Develop.Message(ErrorType.DevelopInfo, null, scp.MainInfo, ImageCode.Skript, $"Parsen: {scp.Chain}\\[{pos + 1}] FEHLER - Keine Fortschritt in der Parsing-Position", scp.Stufe);
                    return new ScriptEndedFeedback(varCol, ld.Protocol, true, false, false, "Parsing-Fehler: Position wurde nicht vorwärts bewegt", null);
                }

                ld.LineAdd(normalizedScriptText.CountChar('¶', pos) + 1 - ld.Line + lineadd);
                if (scx.BreakFired) {
                    Develop.Message(ErrorType.DevelopInfo, null, scp.MainInfo, ImageCode.Skript, $"Parsen: {scp.Chain}\\[{pos + 1}] BREAK", scp.Stufe);
                    return new ScriptEndedFeedback(varCol, ld.Protocol, false, true, false, string.Empty, null);
                }

                if (scx.ReturnFired) {
                    Develop.Message(ErrorType.DevelopInfo, null, scp.MainInfo, ImageCode.Skript, $"Parsen: {scp.Chain}\\[{pos + 1}] RETURN", scp.Stufe);
                    return new ScriptEndedFeedback(varCol, ld.Protocol, false, false, true, string.Empty, scx.ReturnValue);
                }
            }

            if (t.ElapsedMilliseconds > 2000) {
                t.Restart();
                var f = abort?.Invoke() ?? string.Empty;

                if (!string.IsNullOrEmpty(f)) {
                    Develop.Message(ErrorType.DevelopInfo, null, scp.MainInfo, ImageCode.Skript, $"Parsen: {scp.Chain}\\[{pos + 1}] Abbruch: {f}", scp.Stufe);
                    return new ScriptEndedFeedback(varCol, ld.Protocol, false, false, false, f, null);
                }
            }
        } while (true);
    }

    public ScriptEndedFeedback Parse(int lineadd, string subname, AbortReason? abort) {
        (NormalizedScriptText, var error) = NormalizedText(ScriptText);

        return !string.IsNullOrEmpty(error)
            ? new ScriptEndedFeedback(error, false, true, subname)
            : Parse(Variables, Properties, NormalizedScriptText, lineadd, subname, abort);
    }

    #endregion
}