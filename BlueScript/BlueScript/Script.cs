// Authors:
// Christian Peter
//
// Copyright (c) 2025 Christian Peter
// https://github.com/cromagan/BlueElements
//
// License: GNU Affero General Public License v3.0
// https://github.com/cromagan/BlueElements/blob/master/LICENSE
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL
// THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING
// FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER
// DEALINGS IN THE SOFTWARE.

#nullable enable

using BlueBasics;
using BlueBasics.Enums;
using BlueScript.Methods;
using BlueScript.Structures;
using BlueScript.Variables;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace BlueScript;

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

    public static DoItWithEndedPosFeedback CommandOrVarOnPosition(VariableCollection varCol, ScriptProperties scp, string scriptText, int pos, bool expectedvariablefeedback, LogData? ld) {
        //if (MethodsAll == null) { return new DoItWithEndedPosFeedback("Befehle nicht initialisiert", ld); }

        #region  Einfaches Semikolon prüfen. Kann übrig bleiben, wenn eine Variable berechnet wurde, aber nicht verwendet wurde

        if (scriptText.Length > pos && scriptText.Substring(pos, 1) == ";") {
            return new DoItWithEndedPosFeedback(false, pos + 1, false, false, string.Empty, null, null);
        }

        #endregion

        #region Befehle prüfen mit Überladungsunterstützung

        // Sammle alle passenden Methoden mit ihren CanDo-Ergebnissen
        var candidateMethods = new List<(Method method, CanDoFeedback canDo)>();

        foreach (var thisC in scp.AllowedMethods) {
            var f = thisC.CanDo(scriptText, pos, expectedvariablefeedback, ld);
            if (f.NeedsScriptFix) { return new DoItWithEndedPosFeedback(f.FailedReason, true, null); }

            if (string.IsNullOrEmpty(f.FailedReason)) {
                candidateMethods.Add((thisC, f));
            }
        }

        if (candidateMethods.Count > 0) {
            DoItFeedback? firstResult = null;

            // Versuche alle Kandidaten auszuführen und nimm den ersten erfolgreichen
            foreach (var (method, canDoResult) in candidateMethods) {
                var scx = method.DoIt(varCol, canDoResult, scp);

                firstResult ??= scx;

                // Wenn diese Überladung erfolgreich war, verwende sie
                if (!scx.NeedsScriptFix && string.IsNullOrEmpty(scx.FailedReason)) {
                    return new DoItWithEndedPosFeedback(scx.NeedsScriptFix, canDoResult.ContinueOrErrorPosition, scx.BreakFired, scx.ReturnFired, scx.FailedReason, scx.ReturnValue, null);
                }
            }
            return new DoItWithEndedPosFeedback(firstResult?.NeedsScriptFix ?? true, pos, firstResult?.BreakFired ?? false, firstResult?.ReturnFired ?? false, firstResult?.FailedReason ?? "Interner Fehler", firstResult?.ReturnValue, null);
        }

        #endregion

        #region Variablen prüfen

        if (!expectedvariablefeedback) {
            var maxl = scriptText.Length;

            foreach (var thisV in varCol) {
                var commandtext = thisV.KeyName + "=";
                var l = commandtext.Length;
                if (pos + l < maxl) {
                    if (string.Equals(scriptText.Substring(pos, l), commandtext, StringComparison.OrdinalIgnoreCase)) {
                        var f = Method.GetEnd(scriptText, pos + l - 1, 1, ";", ld);
                        if (f.Failed) {
                            return new DoItWithEndedPosFeedback("Ende der Variableberechnung von '" + thisV.KeyName + "' nicht gefunden.", true, ld);
                        }

                        var scx = Method.VariablenBerechnung(varCol, ld, scp, commandtext + f.NormalizedText + ";", false);
                        return new DoItWithEndedPosFeedback(scx.NeedsScriptFix, f.ContinuePosition, scx.BreakFired, scx.ReturnFired, scx.FailedReason, scx.ReturnValue, ld);
                    }
                }
            }
        }

        #endregion

        #region Prüfen für bessere Fehlermeldung, ob der Rückgabetyp falsch gesetzt wurde

        foreach (var thisC in scp.AllowedMethods) {
            var f = thisC.CanDo(scriptText, pos, !expectedvariablefeedback, ld);
            if (f.NeedsScriptFix) {
                return new DoItWithEndedPosFeedback(f.FailedReason, true, null);
            }

            if (string.IsNullOrEmpty(f.FailedReason)) {
                if (expectedvariablefeedback) {
                    return new DoItWithEndedPosFeedback("Dieser Befehl hat keinen Rückgabewert: " + scriptText.Substring(pos), true, ld);
                }

                //if (thisC.MustUseReturnValue) {
                return new DoItWithEndedPosFeedback("Dieser Befehl hat einen Rückgabewert, der nicht verwendet wird: " + scriptText.Substring(pos), true, ld);
                //}
            }
        }

        #endregion Prüfen für bessere Fehlermeldung, ob der Rückgabetyp falsch gesetzt wurde

        #region Prüfen für bessere Fehlermeldung, alle Befehle prüfen

        foreach (var thisC in Method.AllMethods) {
            var f = thisC.CanDo(scriptText, pos, expectedvariablefeedback, ld);
            if (string.IsNullOrEmpty(f.FailedReason)) {
                return new DoItWithEndedPosFeedback("Dieser Befehl kann in diesen Skript nicht verwendet werden.", true, ld);
            }
        }

        #endregion

        var bef = (scriptText.Substring(pos) + "¶").SplitBy("¶");

        return new DoItWithEndedPosFeedback("Kann nicht geparsed werden: " + bef[0], true, ld);
    }

    public static (string f, string error) NormalizedText(string script) => script.RemoveEscape().NormalizedText(false, true, false, true, '¶');

    public static ScriptEndedFeedback Parse(VariableCollection varCol, ScriptProperties scp, string normalizedScriptText, int lineadd, string subname, List<string>? attributes, AbortReason? abort) {
        var pos = 0;

        var ld = new LogData(subname, lineadd + 1);

        if (attributes != null) {
            // Attribute nur löschen, wenn neue vorhanden sind.
            // Ansonsten werden bei Try / If / For diese gelöscht
            varCol.RemoveWithComment("Attribut");
            for (var z = 0; z < attributes.Count; z++) {
                varCol.Add(new VariableString("Attribut" + z, attributes[z], true, "Attribut"));
            }

            for (var z = attributes.Count; z < 20; z++) {
                varCol.Add(new VariableString("Attribut" + z, string.Empty, true, "Attribut"));
            }
        }

        Develop.Message?.Invoke(ErrorType.DevelopInfo, null, scp.MainInfo, ImageCode.Skript, $"Parsen: {scp.Chain} START", scp.Stufe);

        var t = Stopwatch.StartNew();

        do {
            if (pos >= normalizedScriptText.Length) {
                Develop.Message?.Invoke(ErrorType.DevelopInfo, null, scp.MainInfo, ImageCode.Skript, $"Parsen: {scp.Chain}\\[{pos + 1}] ENDE (Regulär)", scp.Stufe);

                return new ScriptEndedFeedback(varCol, ld.Protocol, false, false, false, string.Empty, null);
            }

            if (normalizedScriptText.Substring(pos, 1) == "¶") {
                pos++;
                ld.LineAdd(1);
            } else {
                var previousPos = pos; // KRITISCHE ÄNDERUNG: Vorherige Position speichern
                var scx = CommandOrVarOnPosition(varCol, scp, normalizedScriptText, pos, false, ld);
                if (scx.Failed) {
                    Develop.Message?.Invoke(ErrorType.DevelopInfo, null, scp.MainInfo, ImageCode.Skript, $"Parsen: {scp.Chain}\\[{pos + 1}] ENDE, da nicht erfolgreich {scx.FailedReason}", scp.Stufe);
                    return new ScriptEndedFeedback(varCol, ld.Protocol, scx.NeedsScriptFix, false, false, scx.FailedReason, null);
                }

                pos = scx.Position;

                // KRITISCHE ÄNDERUNG: Fortschrittsvalidierung
                if (pos <= previousPos) {
                    Develop.Message?.Invoke(ErrorType.DevelopInfo, null, scp.MainInfo, ImageCode.Skript, $"Parsen: {scp.Chain}\\[{pos + 1}] FEHLER - Keine Fortschritt in der Parsing-Position", scp.Stufe);
                    return new ScriptEndedFeedback(varCol, ld.Protocol, true, false, false, "Parsing-Fehler: Position wurde nicht vorwärts bewegt", null);
                }

                ld.LineAdd(normalizedScriptText.CountChar('¶', pos) + 1 - ld.Line + lineadd);
                if (scx.BreakFired) {
                    Develop.Message?.Invoke(ErrorType.DevelopInfo, null, scp.MainInfo, ImageCode.Skript, $"Parsen: {scp.Chain}\\[{pos + 1}] BREAK", scp.Stufe);
                    return new ScriptEndedFeedback(varCol, ld.Protocol, false, true, false, string.Empty, null);
                }

                if (scx.ReturnFired) {
                    Develop.Message?.Invoke(ErrorType.DevelopInfo, null, scp.MainInfo, ImageCode.Skript, $"Parsen: {scp.Chain}\\[{pos + 1}] RETURN", scp.Stufe);
                    return new ScriptEndedFeedback(varCol, ld.Protocol, false, false, true, string.Empty, scx.ReturnValue);
                }
            }

            if (t.ElapsedMilliseconds > 2000) {
                t.Restart();
                var f = abort?.Invoke() ?? string.Empty;

                if (!string.IsNullOrEmpty(f)) {
                    Develop.Message?.Invoke(ErrorType.DevelopInfo, null, scp.MainInfo, ImageCode.Skript, $"Parsen: {scp.Chain}\\[{pos + 1}] Abbruch: {f}", scp.Stufe);
                    return new ScriptEndedFeedback(varCol, ld.Protocol, false, false, false, f, null);
                }
            }
        } while (true);
    }

    public ScriptEndedFeedback Parse(int lineadd, string subname, List<string>? attributes, AbortReason? abort) {
        (NormalizedScriptText, var error) = NormalizedText(ScriptText);

        return !string.IsNullOrEmpty(error)
            ? new ScriptEndedFeedback(error, false, true, subname)
            : Parse(Variables, Properties, NormalizedScriptText, lineadd, subname, attributes, abort);
    }

    #endregion
}