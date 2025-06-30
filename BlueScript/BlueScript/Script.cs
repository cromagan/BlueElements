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
using System.Linq;
using System.Text;

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

    public static DoItWithEndedPosFeedback CommandOrVarOnPosition(VariableCollection varCol, ScriptProperties scp, string scriptText, int pos, bool expectedvariablefeedback, LogData? ld) {
        //if (MethodsAll == null) { return new DoItWithEndedPosFeedback("Befehle nicht initialisiert", ld); }

        #region  Einfaches Semikolon prüfen. Kann übrig bleiben, wenn eine Variable berechnet wurde, aber nicht verwendet wurde

        if (scriptText.Length > pos && scriptText.Substring(pos, 1) == ";") {
            return new DoItWithEndedPosFeedback(false, null, pos + 1, false, false, string.Empty, null);
        }

        #endregion

        #region Befehle prüfen

        foreach (var thisC in scp.AllowedMethods) {
            var f = thisC.CanDo(scriptText, pos, expectedvariablefeedback, ld);
            if (f.NeedsScriptFix) { return new DoItWithEndedPosFeedback(f.Message, true, null); }

            if (string.IsNullOrEmpty(f.Message)) {
                var fn = thisC.DoIt(varCol, f, scp);
                return new DoItWithEndedPosFeedback(fn.NeedsScriptFix, fn.Variable, f.ContinueOrErrorPosition, fn.BreakFired, fn.EndScript, fn.FailedReason, null);
            }
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

                        var fn = Method.VariablenBerechnung(varCol, ld, scp, commandtext + f.AttributeText + ";", false);
                        return new DoItWithEndedPosFeedback(fn.NeedsScriptFix, fn.Variable, f.ContinuePosition, fn.BreakFired, fn.EndScript, fn.FailedReason, ld);
                    }
                }
            }
        }

        #endregion

        #region Prüfen für bessere Fehlermeldung, ob der Rückgabetyp falsch gesetzt wurde

        foreach (var thisC in scp.AllowedMethods) {
            var f = thisC.CanDo(scriptText, pos, !expectedvariablefeedback, ld);
            if (f.NeedsScriptFix) {
                return new DoItWithEndedPosFeedback(f.Message, true, null);
            }

            if (string.IsNullOrEmpty(f.Message)) {
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
            //if (f.ScriptNeedFix) { return new DoItWithEndedPosFeedback(f.ErrorMessage, ld); }

            if (string.IsNullOrEmpty(f.Message)) {
                return new DoItWithEndedPosFeedback("Dieser Befehl kann in diesen Skript nicht verwendet werden.", true, ld);
            }
        }

        #endregion

        var bef = (scriptText.Substring(pos) + "¶").SplitBy("¶");

        return new DoItWithEndedPosFeedback("Kann nicht geparsed werden: " + bef[0], true, ld);
    }

    public static (string f, string error) NormalizedText(string script) => script.RemoveEscape().NormalizedText(false, true, false, true, '¶');

    public static ScriptEndedFeedback Parse(VariableCollection varCol, ScriptProperties scp, string normalizedScriptText, int lineadd, string subname, List<string>? attributes) {
        var pos = 0;
        var endScript = false;

        var ld = new LogData(subname, lineadd + 1);

        if (attributes != null) {
            // Attribute nur löschen, wenn neue vorhanden sind.
            // Ansonsten werden bei Try / If / For diese gelöscht
            varCol.RemoveWithComment("Attribut");
            for (var z = 0; z < attributes.Count; z++) {
                _ = varCol.Add(new VariableString("Attribut" + z, attributes[z], true, "Attribut"));
            }
        }

        Develop.Message?.Invoke(ErrorType.DevelopInfo, null, scp.MainInfo, ImageCode.Skript, $"Parsen: {scp.Chain} START", scp.Stufe);

        do {
            if (pos >= normalizedScriptText.Length || endScript) {
                Develop.Message?.Invoke(ErrorType.DevelopInfo, null, scp.MainInfo, ImageCode.Skript, $"Parsen: {scp.Chain}\\[{pos + 1}] ENDE (Regulär)", scp.Stufe);

                return new ScriptEndedFeedback(varCol, ld.Protocol, false, false, endScript, string.Empty);
            }

            if (normalizedScriptText.Substring(pos, 1) == "¶") {
                pos++;
                ld.LineAdd(1);
            } else {
                var f = CommandOrVarOnPosition(varCol, scp, normalizedScriptText, pos, false, ld);
                if (f.Failed) {
                    Develop.Message?.Invoke(ErrorType.DevelopInfo, null, scp.MainInfo, ImageCode.Skript, $"Parsen: {scp.Chain}\\[{pos + 1}] ENDE, da nicht erfolgreich {f.FailedReason}", scp.Stufe);
                    return new ScriptEndedFeedback(varCol, ld.Protocol, f.NeedsScriptFix, false, false, f.FailedReason);
                }

                endScript = f.EndScript;

                pos = f.Position;
                ld.LineAdd(normalizedScriptText.CountChar('¶', pos) + 1 - ld.Line + lineadd);
                if (f.BreakFired) {
                    Develop.Message?.Invoke(ErrorType.DevelopInfo, null, scp.MainInfo, ImageCode.Skript, $"Parsen: {scp.Chain}\\[{pos + 1}] BREAK", scp.Stufe);
                    return new ScriptEndedFeedback(varCol, ld.Protocol, false, true, false, string.Empty);
                }
            }
        } while (true);
    }

    public ScriptEndedFeedback Parse(int lineadd, string subname, List<string>? attributes) {
        (NormalizedScriptText, var error) = NormalizedText(ScriptText);

        return !string.IsNullOrEmpty(error)
            ? new ScriptEndedFeedback(error, false, true, subname)
            : Parse(Variables, Properties, NormalizedScriptText, lineadd, subname, attributes);
    }

    #endregion
}