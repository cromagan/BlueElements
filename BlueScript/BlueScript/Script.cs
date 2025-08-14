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

    #region Properties

    public string NormalizedScriptText { get; private set; }
    public ScriptProperties Properties { get; }
    public string ScriptText { get; set; } = string.Empty;

    public VariableCollection Variables { get; }

    #endregion

    #region Methods

    public static DoItFeedback CommandOrVarOnPosition(VariableCollection varCol, ScriptProperties scp, string scriptText, int pos, bool expectedvariablefeedback, CurrentPosition cp) {
        //if (MethodsAll == null) { return new DoItFeedback("Befehle nicht initialisiert", ld); }

        #region  Einfaches Semikolon prüfen. Kann übrig bleiben, wenn eine Variable berechnet wurde, aber nicht verwendet wurde

        if (scriptText.Length > pos && scriptText.Substring(pos, 1) == ";") {
            return new DoItFeedback(false, false, false, string.Empty, null, cp);
        }

        #endregion

        #region Befehle prüfen mit Überladungsunterstützung

        // Sammle alle passenden Methoden mit ihren CanDo-Ergebnissen
        var candidateMethods = new List<(Method method, CanDoFeedback canDo)>();

        foreach (var thisC in scp.AllowedMethods) {
            var f = thisC.CanDo(scriptText, pos, expectedvariablefeedback, cp);
            if (f.NeedsScriptFix) { return new DoItFeedback(f.FailedReason, true, f); }

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
                    return new DoItFeedback(scx.NeedsScriptFix, scx.BreakFired, scx.ReturnFired, scx.FailedReason, scx.ReturnValue, cp);
                }

            }

  
            return new DoItFeedback(firstResult?.NeedsScriptFix ?? true, firstResult?.BreakFired ?? false, firstResult?.ReturnFired ?? false, firstResult?.FailedReason ?? "Interner Fehler", firstResult?.ReturnValue, cp);

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
                        var f = Method.GetEnd(scriptText, new CurrentPosition(cp.Subname, pos + l - 1), 1, ";");
                        if (f.Failed) {
                            return new DoItFeedback("Ende der Variableberechnung von '" + thisV.KeyName + "' nicht gefunden.", true, cp);
                        }

                        return Method.VariablenBerechnung(varCol, cp, scp, commandtext + f.AttributeText + ";", false);

                    }
                }
            }
        }

        #endregion

        #region Prüfen für bessere Fehlermeldung, ob der Rückgabetyp falsch gesetzt wurde

        foreach (var thisC in scp.AllowedMethods) {
            var f = thisC.CanDo(scriptText, pos, !expectedvariablefeedback, cp);
            if (f.NeedsScriptFix) {
                return new DoItFeedback(f.FailedReason, true, cp);
            }

            if (string.IsNullOrEmpty(f.FailedReason)) {
                if (expectedvariablefeedback) {
                    return new DoItFeedback("Dieser Befehl hat keinen Rückgabewert: " + scriptText.Substring(pos), true, cp);
                }

                //if (thisC.MustUseReturnValue) {
                return new DoItFeedback("Dieser Befehl hat einen Rückgabewert, der nicht verwendet wird: " + scriptText.Substring(pos), true, cp);
                //}
            }
        }

        #endregion Prüfen für bessere Fehlermeldung, ob der Rückgabetyp falsch gesetzt wurde

        #region Prüfen für bessere Fehlermeldung, alle Befehle prüfen

        foreach (var thisC in Method.AllMethods) {
            var f = thisC.CanDo(scriptText, pos, expectedvariablefeedback, cp);
            //if (f.ScriptNeedFix) { return new DoItFeedback(f.ErrorMessage, ld); }

            if (string.IsNullOrEmpty(f.FailedReason)) {
                return new DoItFeedback("Dieser Befehl kann in diesen Skript nicht verwendet werden.", true, cp);
            }
        }

        #endregion

        var bef = (scriptText.Substring(pos) + "¶").SplitBy("¶");

        return new DoItFeedback("Kann nicht geparsed werden: " + bef[0], true, cp);
    }

    public static (string f, string error) NormalizedText(string script) => script.RemoveEscape().NormalizedText(false, true, false, true, '¶');

    public static ScriptEndedFeedback Parse(VariableCollection varCol, ScriptProperties scp, string normalizedScriptText, CurrentPosition cp, List<string>? attributes) {
        var pos = cp.Position+1;



        if (attributes != null) {
            // Attribute nur löschen, wenn neue vorhanden sind.
            // Ansonsten werden bei Try / If / For diese gelöscht
            varCol.RemoveWithComment("Attribut");
            for (var z = 0; z < attributes.Count; z++) {
                _ = varCol.Add(new VariableString("Attribut" + z, attributes[z], true, "Attribut"));
            }

            for (var z = attributes.Count; z < 20; z++) {
                _ = varCol.Add(new VariableString("Attribut" + z, string.Empty, true, "Attribut"));
            }
        }

        Develop.Message?.Invoke(ErrorType.DevelopInfo, null, scp.MainInfo, ImageCode.Skript, $"Parsen: {scp.Chain} START", scp.Stufe);

        do {
            if (pos >= normalizedScriptText.Length) {
                Develop.Message?.Invoke(ErrorType.DevelopInfo, null, scp.MainInfo, ImageCode.Skript, $"Parsen: {scp.Chain}\\[{pos + 1}] ENDE (Regulär)", scp.Stufe);

                return new ScriptEndedFeedback(new CurrentPosition(cp.Subname, pos), varCol, false, false, false, string.Empty, null);
            }

            if (normalizedScriptText.Substring(pos, 1) == "¶") {
                pos++;
            } else {

                var scx = CommandOrVarOnPosition(varCol, scp, normalizedScriptText, pos, false, new CurrentPosition(cp.Subname, pos));
                if (scx.Failed) {
                    Develop.Message?.Invoke(ErrorType.DevelopInfo, null, scp.MainInfo, ImageCode.Skript, $"Parsen: {scp.Chain}\\[{pos + 1}] ENDE, da nicht erfolgreich {scx.FailedReason}", scp.Stufe);
                    return new ScriptEndedFeedback(scx, varCol, scx.NeedsScriptFix, false, false, scx.FailedReason, null);
                }

                pos = scx.Position;

                if (scx.BreakFired) {
                    Develop.Message?.Invoke(ErrorType.DevelopInfo, null, scp.MainInfo, ImageCode.Skript, $"Parsen: {scp.Chain}\\[{pos + 1}] BREAK", scp.Stufe);
                    return new ScriptEndedFeedback(scx, varCol, false, true, false, string.Empty, null);
                }

                if (scx.ReturnFired) {
                    Develop.Message?.Invoke(ErrorType.DevelopInfo, null, scp.MainInfo, ImageCode.Skript, $"Parsen: {scp.Chain}\\[{pos + 1}] RETURN", scp.Stufe);
                    return new ScriptEndedFeedback(scx, varCol, false, false, true, string.Empty, scx.ReturnValue);
                }

                pos = scx.Position + 1;
            }
        } while (true);
    }

    public ScriptEndedFeedback Parse(CurrentPosition cp, List<string>? attributes) {
        (NormalizedScriptText, var error) = NormalizedText(ScriptText);

        return !string.IsNullOrEmpty(error)
            ? new ScriptEndedFeedback(cp, error, false, true)
            : Parse(Variables, Properties, NormalizedScriptText,cp, attributes);
    }

    #endregion
}