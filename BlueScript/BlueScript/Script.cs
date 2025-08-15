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

    public static DoItFeedback CommandOrVarOnPosition(VariableCollection varCol, ScriptProperties scp, bool expectedvariablefeedback, CanDoFeedback cdf) {
        //if (MethodsAll == null) { return new DoItFeedback("Befehle nicht initialisiert", ld); }

        #region  Einfaches Semikolon prüfen. Kann übrig bleiben, wenn eine Variable berechnet wurde, aber nicht verwendet wurde

        if (cdf.NormalizedText.Length > cdf.Position && cdf.NormalizedText.Substring(cdf.Position, 1) == ";") {
            return new DoItFeedback(cdf.Subname, cdf.Position + 1, cdf.Protocol, cdf.Chain, false, false, false, string.Empty, null);
        }

        #endregion

        #region Befehle prüfen mit Überladungsunterstützung

        // Sammle alle passenden Methoden mit ihren CanDo-Ergebnissen
        var candidateMethods = new List<(Method method, CanDoFeedback canDo)>();

        foreach (var thisC in scp.AllowedMethods) {
            var f = thisC.CanDo(expectedvariablefeedback, cdf);
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
                    return new DoItFeedback(scx.Subname, scx.Position, scx.Protocol, scx.Chain, scx.NeedsScriptFix, scx.BreakFired, scx.ReturnFired, scx.FailedReason, scx.ReturnValue);
                }
            }

            return new DoItFeedback(cdf.Subname, cdf.Position, cdf.Protocol, cdf.Chain, firstResult?.NeedsScriptFix ?? true, firstResult?.BreakFired ?? false, firstResult?.ReturnFired ?? false, firstResult?.FailedReason ?? "Interner Fehler", firstResult?.ReturnValue);
        }

        #endregion

        #region Variablen prüfen

        if (!expectedvariablefeedback) {
            var maxl = cdf.NormalizedText.Length;

            foreach (var thisV in varCol) {
                var commandtext = thisV.KeyName + "=";
                var l = commandtext.Length;
                if (cdf.Position + l < maxl) {
                    if (string.Equals(cdf.NormalizedText.Substring(cdf.Position, l), commandtext, StringComparison.OrdinalIgnoreCase)) {
                        var f = Method.GetEnd(new CanDoFeedback(cdf, cdf.Position + l - 1), 1, ";");
                        if (f.Failed) {
                            return new DoItFeedback("Ende der Variableberechnung von '" + thisV.KeyName + "' nicht gefunden.", true, cdf);
                        }

                        return Method.VariablenBerechnung(varCol, new CanDoFeedback(cdf, 0, commandtext + f.NormalizedText + ";"), scp, false);
                    }
                }
            }
        }

        #endregion

        #region Prüfen für bessere Fehlermeldung, ob der Rückgabetyp falsch gesetzt wurde

        foreach (var thisC in scp.AllowedMethods) {
            var f = thisC.CanDo(!expectedvariablefeedback, cdf);
            if (f.NeedsScriptFix) {
                return new DoItFeedback(f.FailedReason, true, cdf);
            }

            if (string.IsNullOrEmpty(f.FailedReason)) {
                if (expectedvariablefeedback) {
                    return new DoItFeedback("Dieser Befehl hat keinen Rückgabewert: " + cdf.NormalizedText.Substring(cdf.Position), true, cdf);
                }

                //if (thisC.MustUseReturnValue) {
                return new DoItFeedback("Dieser Befehl hat einen Rückgabewert, der nicht verwendet wird: " + cdf.NormalizedText.Substring(cdf.Position), true, cdf);
                //}
            }
        }

        #endregion Prüfen für bessere Fehlermeldung, ob der Rückgabetyp falsch gesetzt wurde

        #region Prüfen für bessere Fehlermeldung, alle Befehle prüfen

        foreach (var thisC in Method.AllMethods) {
            var f = thisC.CanDo(expectedvariablefeedback, cdf);
            //if (f.ScriptNeedFix) { return new DoItFeedback(f.ErrorMessage, ld); }

            if (string.IsNullOrEmpty(f.FailedReason)) {
                return new DoItFeedback("Dieser Befehl kann in diesen Skript nicht verwendet werden.", true, cdf);
            }
        }

        #endregion

        var bef = (cdf.NormalizedText.Substring(cdf.Position) + "¶").SplitBy("¶");

        return new DoItFeedback("Kann nicht geparsed werden: " + bef[0], true, cdf);
    }

    public static (string f, string error) NormalizedText(string script) => script.RemoveEscape().NormalizedText(false, true, false, true, '¶');

    public static ScriptEndedFeedback Parse(VariableCollection varCol, ScriptProperties scp, string normalizedScriptText, CurrentPosition cp, List<string>? attributes) {
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

        Develop.Message?.Invoke(ErrorType.DevelopInfo, null, scp.MainInfo, ImageCode.Skript, $"Parsen: {cp.Chain} START", cp.Stufe);
        var pos = cp.Position;
        do {
            if (pos >= normalizedScriptText.Length) {
                Develop.Message?.Invoke(ErrorType.DevelopInfo, null, scp.MainInfo, ImageCode.Skript, $"Parsen: {cp.Chain}\\[{pos + 1}] ENDE (Regulär)", cp.Stufe);
                return new ScriptEndedFeedback(cp.Subname, pos, cp.Protocol, cp.Chain, false, false, false, varCol, string.Empty);
            }

            if (normalizedScriptText.Substring(pos, 1) == "¶") {
                pos++;
            } else {
                var dif = CommandOrVarOnPosition(varCol, scp, false, new CanDoFeedback(cp, pos, normalizedScriptText));

                if (dif.Failed) {
                    Develop.Message?.Invoke(ErrorType.DevelopInfo, null, scp.MainInfo, ImageCode.Skript, $"Parsen: {cp.Chain}\\[{pos + 1}] ENDE, da nicht erfolgreich {dif.FailedReason}", cp.Stufe);
                    return new ScriptEndedFeedback(dif, varCol, pos);
                }

                pos = dif.Position;

                if (dif.BreakFired) {
                    Develop.Message?.Invoke(ErrorType.DevelopInfo, null, scp.MainInfo, ImageCode.Skript, $"Parsen: {cp.Chain}\\[{pos + 1}] BREAK", cp.Stufe);
                    return new ScriptEndedFeedback(dif, varCol, pos);
                }

                if (dif.ReturnFired) {
                    Develop.Message?.Invoke(ErrorType.DevelopInfo, null, scp.MainInfo, ImageCode.Skript, $"Parsen: {cp.Chain}\\[{pos + 1}] RETURN", cp.Stufe);
                    return new ScriptEndedFeedback(dif, varCol, pos);
                }

                pos = dif.Position + 1;
            }
        } while (true);
    }

    public ScriptEndedFeedback Parse(List<string>? attributes) {
        (NormalizedScriptText, var error) = NormalizedText(ScriptText);

        return string.IsNullOrEmpty(error)
            ? Parse(Variables, Properties, NormalizedScriptText, new CurrentPosition(), attributes)
            : new ScriptEndedFeedback(error, false, true, string.Empty);
    }

    #endregion
}