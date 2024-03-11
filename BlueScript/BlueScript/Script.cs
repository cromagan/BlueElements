// Authors:
// Christian Peter
//
// Copyright (c) 2024 Christian Peter
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
using BlueScript.Methods;
using BlueScript.Structures;
using BlueScript.Variables;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using static BlueBasics.Generic;

namespace BlueScript;

public class Script {

    #region Fields

    private static List<Method>? _commands;

    private static List<Variable>? _varTypes;

    #endregion

    #region Constructors

    public Script(VariableCollection? variablen, string additionalFilesPath, ScriptProperties scp) {
        ReducedScriptText = string.Empty;

        Variables = variablen ?? [];
        Properties = scp;

        if (!string.IsNullOrEmpty(additionalFilesPath)) {
            Variables.Add(new VariableString("AdditionalFilesPfad", (additionalFilesPath.Trim("\\") + "\\").CheckPath(), true, "Der Dateipfad, in dem zusätzliche Daten gespeichert werden."));
        }
    }

    #endregion

    #region Properties

    public static List<Method> Commands {
        get {
            _commands ??= GetInstaceOfType<Method>();
            return _commands;
        }
    }

    public static List<Variable> VarTypes {
        get {
            if (_varTypes == null) {
                _varTypes = GetInstaceOfType<Variable>("NAME");
                _varTypes.Sort();
            }
            return _varTypes;
        }
    }

    public ScriptProperties Properties { get; }
    public string ReducedScriptText { get; private set; }
    public string ScriptText { get; set; } = string.Empty;

    public VariableCollection Variables { get; }

    #endregion

    #region Methods

    public static DoItWithEndedPosFeedback CommandOrVarOnPosition(VariableCollection varCol, ScriptProperties scp, string scriptText, int pos, bool expectedvariablefeedback, LogData ld) {
        if (Commands == null) { return new DoItWithEndedPosFeedback("Befehle nicht initialisiert", ld); }

        #region  Einfaches Semikolon prüfen. Kann übrig bleiben, wenn eine Variable berechnet wurde, aber nicht verwendet wurde

        if (scriptText.Length > pos && scriptText.Substring(pos, 1) == ";") {
            return new DoItWithEndedPosFeedback(true, null, pos + 1, false, false);
        }

        #endregion

        #region Befehle prüfen

        foreach (var thisC in Commands) {
            var f = thisC.CanDo(scp, scriptText, pos, expectedvariablefeedback, ld);
            if (f.MustAbort) { return new DoItWithEndedPosFeedback(f.ErrorMessage, ld); }

            if (string.IsNullOrEmpty(f.ErrorMessage)) {
                var fn = thisC.DoIt(varCol, f, scp);
                return new DoItWithEndedPosFeedback(fn.AllOk, fn.Variable, f.ContinueOrErrorPosition, fn.BreakFired, fn.EndScript);
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
                        if (!f.AllOk) {
                            return new DoItWithEndedPosFeedback("Ende der Variableberechnung von '" + thisV.KeyName + "' nicht gefunden.", ld);
                        }
                        var infos = new CanDoFeedback(f.ContinuePosition, f.AttributeText, string.Empty, ld);
                        var fn = Method.VariablenBerechnung(varCol, infos, scp, commandtext + f.AttributeText + ";", false);
                        return new DoItWithEndedPosFeedback(fn.AllOk, fn.Variable, f.ContinuePosition, fn.BreakFired, fn.EndScript);
                    }
                }
            }
        }

        #endregion

        #region Prüfen für bessere Fehlermeldung, ob der Rückgabetyp falsch gesetzt wurde

        foreach (var thisC in Commands) {
            var f = thisC.CanDo(scp, scriptText, pos, !expectedvariablefeedback, ld);
            if (f.MustAbort) {
                return new DoItWithEndedPosFeedback(f.ErrorMessage, ld);
            }

            if (string.IsNullOrEmpty(f.ErrorMessage)) {
                if (expectedvariablefeedback) {
                    return new DoItWithEndedPosFeedback("Dieser Befehl hat keinen Rückgabewert: " + scriptText.Substring(pos), ld);
                }

                //if (thisC.MustUseReturnValue) {
                return new DoItWithEndedPosFeedback("Dieser Befehl hat einen Rückgabewert, der nicht verwendet wird: " + scriptText.Substring(pos), ld);
                //}
            }
        }

        #endregion Prüfen für bessere Fehlermeldung, ob der Rückgabetyp falsch gesetzt wurde

        var bef = (scriptText.Substring(pos) + "¶").SplitBy("¶");

        return new DoItWithEndedPosFeedback("Kann nicht geparsed werden: " + bef[0], ld);
    }

    public static int Line(string? txt, int? pos) {
        if (pos == null || txt == null) { return 0; }
        return txt.Substring(0, Math.Min((int)pos, txt.Length)).Count(c => c == '¶') + 1;
    }

    public static ScriptEndedFeedback Parse(VariableCollection varCol, ScriptProperties scp, string redScriptText, int lineadd, string subname, List<string>? attributes) {
        var pos = 0;
        var endScript = false;

        var ld = new LogData(subname, lineadd + 1);

        if (attributes != null) {
            // Attribute nur löschen, wenn neue vorhanden sind.
            // Ansonsten werden bei Try / If / For diese gelöscht
            varCol.RemoveWithComment("Attribut");
            for (var z = 0; z < attributes.Count; z++) {
                varCol.Add(new VariableString("Attribut" + z.ToString(), attributes[z], true, "Attribut"));
            }
        }

        do {
            if (pos >= redScriptText.Length || endScript) {
                return new ScriptEndedFeedback(varCol, ld.Protocol, true, false, false, endScript);
            }

            if (redScriptText.Substring(pos, 1) == "¶") {
                pos++;
                ld.LineAdd(1);
            } else {
                var f = CommandOrVarOnPosition(varCol, scp, redScriptText, pos, false, ld);
                if (!f.AllOk) {
                    return new ScriptEndedFeedback(varCol, ld.Protocol, false, true, false, false);
                }

                endScript = f.EndSkript;

                pos = f.Position;
                ld.LineAdd(Line(redScriptText, pos) - ld.Line + lineadd);
                if (f.BreakFired) { return new ScriptEndedFeedback(varCol, ld.Protocol, true, false, true, false); }
            }
        } while (true);
    }

    public static (string reducedText, string error) ReduceText(string txt) {
        StringBuilder s = new();
        var gänsef = false;
        var comment = false;

        txt = txt.RemoveEscape();// muss am Anfang gemacht werden, weil sonst die Zählweise nicht mehr stimmt

        for (var pos = 0; pos < txt.Length; pos++) {
            var c = txt.Substring(pos, 1);
            var addt = true;
            switch (c) {
                case "\"":
                    if (!comment) { gänsef = !gänsef; }
                    break;

                case "/":
                    if (!gänsef) {
                        if (pos < txt.Length - 1 && txt.Substring(pos, 2) == "//") { comment = true; }
                    }
                    break;

                case "\r":
                    if (gänsef) {
                        var t = s.ToString();
                        return (t, "Fehler mit Gänsefüssschen in Zeile " + Line(t, pos));
                    }
                    _ = s.Append("¶");
                    comment = false;
                    addt = false;
                    break;

                case " ":

                case "\n":

                case "\t":
                    if (!gänsef) { addt = false; }
                    break;
            }
            if (!comment && addt) {
                _ = s.Append(c);
            }
        }
        return (s.ToString(), string.Empty);
    }

    public ScriptEndedFeedback Parse(int lineadd, string subname, List<string>? attributes) {
        (ReducedScriptText, var error) = ReduceText(ScriptText);

        if (!string.IsNullOrEmpty(error)) {
            return new ScriptEndedFeedback(error, false, true, subname);
        }

        return Parse(Variables, Properties, ReducedScriptText, lineadd, subname, attributes);
    }

    #endregion
}