// Authors:
// Christian Peter
//
// Copyright (c) 2023 Christian Peter
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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BlueBasics;
using BlueScript.Enums;
using BlueScript.Methods;
using BlueScript.Structures;
using BlueScript.Variables;
using static BlueBasics.Generic;

namespace BlueScript;

public class Script {

    #region Fields

    public static List<Method>? Comands;
    public static List<Variable>? VarTypes;

    #endregion

    #region Constructors

    public Script(VariableCollection variablen, string additionalFilesPath, bool changevalues, MethodType allowedMethods, string attributes) {
        Comands ??= GetInstaceOfType<Method>();
        if (VarTypes == null) {
            VarTypes = GetInstaceOfType<Variable>("NAME");
            VarTypes.Sort();
        }

        ReducedScriptText = string.Empty;
        ChangeValues = changevalues;
        Variables = variablen;
        AllowedMethods = allowedMethods;

        if (!string.IsNullOrEmpty(additionalFilesPath)) {
            variablen.Add(new VariableString("AdditionalFilesPfad", (additionalFilesPath.Trim("\\") + "\\").CheckPath(), true, false, "Der Dateipfad, in dem zusätzliche Daten gespeichert werden."));
        }

        Attributes = attributes;
    }

    #endregion

    #region Properties

    public MethodType AllowedMethods { get; }
    public string Attributes { get; }

    public bool ChangeValues { get; }

    public string ReducedScriptText { get; private set; }
    public string ScriptText { get; set; } = string.Empty;

    public VariableCollection Variables { get; }

    #endregion

    #region Methods

    public static DoItWithEndedPosFeedback ComandOnPosition(string txt, int pos, List<Method> lm, bool expectedvariablefeedback, LogData ld, VariableCollection vs, MethodType allowedMethods, bool changevalues, string scriptAttributes) {
        if (Comands == null) { return new DoItWithEndedPosFeedback("Befehle nicht initialisiert", ld); }

        foreach (var thisC in Comands) {
            var f = thisC.CanDo(txt, pos, expectedvariablefeedback, lm, ld, vs, allowedMethods, changevalues, scriptAttributes);
            if (f.MustAbort) { return new DoItWithEndedPosFeedback(f.ErrorMessage, ld); }

            if (string.IsNullOrEmpty(f.ErrorMessage)) {
                var fn = thisC.DoIt(vs, f);
                return new DoItWithEndedPosFeedback(fn.AllOk, fn.Variable, f.ContinueOrErrorPosition, fn.BreakFired, fn.EndScript);
            }
        }

        #region Prüfen für bessere Fehlermeldung, ob der Rückgabetyp falsch gesetzt wurde

        foreach (var f in Comands.Select(thisC => thisC.CanDo(txt, pos, !expectedvariablefeedback, lm, ld, vs, allowedMethods, changevalues, scriptAttributes))) {
            if (f.MustAbort) {
                return new DoItWithEndedPosFeedback(f.ErrorMessage, ld);
            }

            if (string.IsNullOrEmpty(f.ErrorMessage)) {
                return expectedvariablefeedback
                    ? new DoItWithEndedPosFeedback("Dieser Befehl hat keinen Rückgabewert: " + txt.Substring(pos), ld)
                    : new DoItWithEndedPosFeedback("Dieser Befehl hat einen Rückgabewert, der nicht verwendet wird: " + txt.Substring(pos), ld);
            }
        }

        #endregion Prüfen für bessere Fehlermeldung, ob der Rückgabetyp falsch gesetzt wurde

        var bef = (txt.Substring(pos) + "¶").SplitBy("¶");

        return new DoItWithEndedPosFeedback("Kann nicht geparsed werden: " + bef[0], ld);
    }

    public static int Line(string? txt, int? pos) {
        if (pos == null || txt == null) { return 0; }
        return txt.Substring(0, Math.Min((int)pos, txt.Length)).Count(c => c == '¶') + 1;
    }

    public static ScriptEndedFeedback Parse(string redScriptText, int lineadd, string subname, VariableCollection vs, List<Method> lm, MethodType allowedMethods, bool changevalues, string scriptAttributes) {
        var pos = 0;
        var EndScript = false;

        var ld = new LogData(subname, lineadd + 1);

        do {
            if (pos >= redScriptText.Length || EndScript) {
                return new ScriptEndedFeedback(vs, ld.Protocol, true);
            }

            if (redScriptText.Substring(pos, 1) == "¶") {
                pos++;
                ld.LineAdd(1);
            } else {
                var f = ComandOnPosition(redScriptText, pos, lm, false, ld, vs, allowedMethods, changevalues, scriptAttributes);
                if (!f.AllOk) {
                    return new ScriptEndedFeedback(vs, ld.Protocol, false);
                }

                EndScript = f.EndSkript;

                pos = f.Position;
                ld.LineAdd(Line(redScriptText, pos) - ld.Line + lineadd);
                if (f.BreakFired) { return new ScriptEndedFeedback(vs, ld.Protocol, true); }
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
                        return (t, "Fehler mit Gänsefüschen in Zeile " + Line(t, pos));
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

    public ScriptEndedFeedback Parse(int lineadd, string subname) {
        (ReducedScriptText, string error) = ReduceText(ScriptText);

        if (!string.IsNullOrEmpty(error)) {
            return new ScriptEndedFeedback(error, false, subname);
        }

        return Script.Parse(ReducedScriptText, lineadd, subname, Variables, Comands, AllowedMethods, ChangeValues, Attributes);
    }

    #endregion

    //internal int AddBitmapToCache(Bitmap? bmp) {
    //    BitmapCache.Add(bmp);
    //    return BitmapCache.IndexOf(bmp);
    //}
}