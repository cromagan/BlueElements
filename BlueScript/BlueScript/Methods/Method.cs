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
using BlueBasics;
using BlueBasics.Interfaces;
using BlueScript.Enums;
using BlueScript.Structures;
using BlueScript.Variables;
using static BlueBasics.Extensions;

namespace BlueScript.Methods;

public abstract class Method : IReadableTextWithChangingAndKey, IReadableText {

    #region Events

    public event EventHandler? Changed;

    #endregion

    #region Properties

    public abstract List<List<string>> Args { get; }

    public abstract string Description { get; }

    public abstract bool EndlessArgs { get; }

    public abstract string EndSequence { get; }

    public abstract bool GetCodeBlockAfter { get; }

    /// <summary>
    ///  Gibt die Syntax zurück, weil der Befehl selbst anders sein könnte. Z.B. Bei Variablan
    /// </summary>
    public string KeyName => Syntax;

    public abstract string Returns { get; }

    public abstract string StartSequence { get; }

    public abstract string Syntax { get; }

    #endregion

    #region Methods

    /// <summary>
    /// Gibt den Text des Codeblocks zurück. Dabei werden die Zeilenumbrüche vor der { nicht entfernt, aber die Klammern {} selbst schon.
    /// Das muss berücksichtigt werden, um die Skript-Position richtig zu setzen!
    /// </summary>
    /// <param name="scriptText"></param>
    /// <param name="start"></param>
    /// <returns></returns>
    public static (string, string) GetCodeBlockText(string scriptText, int start) {
        var maxl = scriptText.Length;

        var tmp = start;

        do {
            if (tmp >= maxl) { return (string.Empty, "Keinen nachfolgenden Codeblock gefunden."); }
            if (scriptText.Substring(tmp, 1) == "{") { break; }
            if (scriptText.Substring(tmp, 1) != "¶") { return (string.Empty, "Keinen nachfolgenden Codeblock gefunden."); }
            tmp++;
        } while (true);

        var (posek, _) = NextText(scriptText, start, GeschKlammerZu, false, false, KlammernStd);
        if (posek < start) {
            return (string.Empty, "Kein Codeblock Ende gefunden.");
        }

        var s = scriptText.Substring(start, tmp - start) + scriptText.Substring(tmp + 1, posek - tmp - 1);

        return (s, string.Empty);
    }

    public static GetEndFeedback ReplaceComands(string txt, Script s) {
        if (Script.Comands == null) { return new GetEndFeedback("Interner Fehler: Befehle nicht initialisiert"); }

        List<string> c = new();
        foreach (var thisc in Script.Comands) {
            if (!string.IsNullOrEmpty(thisc.Returns)) {
                c.AddRange(thisc.Comand(s).Select(thiscs => thiscs + thisc.StartSequence));
            }
        }
        var posc = 0;
        do {
            var (pos, _) = NextText(txt, posc, c, true, false, KlammernStd);
            if (pos < 0) { return new GetEndFeedback(0, txt); }
            var f = Script.ComandOnPosition(txt, pos, s, true);
            if (!string.IsNullOrEmpty(f.ErrorMessage)) { return new GetEndFeedback(f.ErrorMessage); }

            if (pos == 0 && txt.Length == f.Position) { return new GetEndFeedback(f.Variable); }
            if (!f.Variable.ToStringPossible) { return new GetEndFeedback("Variable muss als Objekt behandelt werden"); }

            txt = txt.Substring(0, pos) + f.Variable.ValueForReplace + txt.Substring(f.Position);
            posc = pos;
        } while (true);
    }

    /// <summary>
    /// Ersetzt eine Variable an Stelle 0, falls dort eine ist.
    /// Gibt dann den ersetzten Text zurück.
    /// Achtung: nur Stringable Variablen werden berücksichtigt.
    /// </summary>
    /// <param name="txt"></param>
    /// <param name="s"></param>
    /// <returns></returns>
    public static GetEndFeedback ReplaceVariable(string txt, Script s) {
        var posc = 0;
        var v = s.Variables.AllStringableNames();

        do {
            var (pos, which) = NextText(txt, posc, v, true, true, KlammernStd);

            //if (txt.Contains("~")) {
            //    var (postmp, _) = NextText(txt, posc, Tilde, false, false, KlammernStd);
            //    if ((postmp >= 0 && postmp < pos) || pos < 0) { pos = postmp; which = "~"; }
            //}

            if (pos < 0) { return new GetEndFeedback(0, txt); }

            Variable? thisV;
            int endz;

            //if (which == "~") {
            //    var (pose, _) = NextText(txt, pos + 1, Tilde, false, false, KlammernStd);
            //    if (pose <= pos) { return new GetEndFeedback("Variablen-Findung End-~-Zeichen nicht gefunden."); }
            //    var x = Variable.GetVariableByParsing(txt.Substring(pos + 1, pose - pos - 1), s);
            //    if (x.Variable is not VariableString x2) { return new GetEndFeedback("Fehler beim Berechnen des Variablen-Namens."); }
            //    thisV = s.Variables.Get(x2.ValueString);
            //    endz = pose + 1;
            //} else {
            thisV = s.Variables.Get(which);
            endz = pos + which.Length;
            //}
            if (thisV == null) { return new GetEndFeedback("Variablen-Fehler " + which); }

            //if (thisV.Type == VariableDataType.NotDefinedYet) { return new GetEndFeedback("Variable " + thisV.Name + " ist keinem Typ zugeordnet"); }
            //if (thisV is VariableListString vl && !string.IsNullOrEmpty(vl.ValueString) && !vl.ValueString.EndsWith("\r")) { return new GetEndFeedback("List-Variable " + thisV.Name + " fehlerhaft"); }

            txt = txt.Substring(0, pos) + thisV.ValueForReplace + txt.Substring(endz);
            posc = pos;
        } while (true);
    }

    public static List<string>? SplitAttributeToString(string attributtext) {
        if (string.IsNullOrEmpty(attributtext)) { return null; }
        List<string> attributes = new();

        #region Liste der Attribute splitten

        var posc = 0;
        do {
            var (pos, _) = NextText(attributtext, posc, Komma, false, false, KlammernStd);
            if (pos < 0) {
                attributes.Add(attributtext.Substring(posc).DeKlammere(true, false, false, true));
                break;
            }
            attributes.Add(attributtext.Substring(posc, pos - posc).DeKlammere(true, false, false, true));
            posc = pos + 1;
        } while (true);

        #endregion Liste der Attribute splitten

        return attributes;
    }

    public static SplittedAttributesFeedback SplitAttributeToVars(string attributtext, Script s, List<List<string>> types, bool endlessArgs) {
        if (types.Count == 0) {
            return string.IsNullOrEmpty(attributtext)
                ? new SplittedAttributesFeedback(new List<Variable>())
                : new SplittedAttributesFeedback(ScriptIssueType.AttributAnzahl, "Keine Attribute erwartet, aber erhalten.");
        }

        var attributes = SplitAttributeToString(attributtext);
        if (attributes == null || attributes.Count == 0) { return new SplittedAttributesFeedback(ScriptIssueType.AttributAnzahl, "Allgemeiner Fehler."); }
        if (attributes.Count < types.Count) { return new SplittedAttributesFeedback(ScriptIssueType.AttributAnzahl, "Zu wenige Attribute erhalten."); }
        if (!endlessArgs && attributes.Count > types.Count) { return new SplittedAttributesFeedback(ScriptIssueType.AttributAnzahl, "Zu viele Attribute erhalten."); }

        //  Variablen und Routinen ersetzen
        List<Variable> feedbackVariables = new();
        for (var n = 0; n < attributes.Count; n++) {
            //var lb = attributes[n].Count(c => c == '¶'); // Zeilenzähler weitersetzen
            attributes[n] = attributes[n].RemoveChars("¶"); // Zeilenzähler entfernen

            var exceptetType = n < types.Count ? types[n] : types[types.Count - 1]; // Bei Endlessargs den letzten nehmen

            // Variable ermitteln oder eine Dummy-Variable als Rückgabe ermitteln
            Variable? v = null;

            var mustBeVar = exceptetType.Count > 0 && exceptetType[0].StartsWith("*");

            if (mustBeVar) {
                var varn = attributes[n];
                //if (varn.StartsWith("~") && varn.EndsWith("~")) {
                //    var tmp2 = Variable.GetVariableByParsing(varn.Substring(1, varn.Length - 2), s);
                //    if (tmp2.Variable is not VariableString x) { return new SplittedAttributesFeedback(ScriptIssueType.VariablenNamenBerechnungFehler, "Variablenname konnte nicht berechnet werden bei Attribut " + (n + 1), line); }
                //    varn = x.ValueString;
                //}

                if (!Variable.IsValidName(varn)) { return new SplittedAttributesFeedback(ScriptIssueType.VariableErwartet, "Variablenname erwartet bei Attribut " + (n + 1)); }

                if (s != null) { v = s.Variables.Get(varn); }
                if (v == null) { return new SplittedAttributesFeedback(ScriptIssueType.VariableNichtGefunden, "Variable nicht gefunden bei Attribut " + (n + 1)); }
            } else {
                var tmp2 = Variable.GetVariableByParsing(attributes[n], s);
                if (tmp2.Variable == null) { return new SplittedAttributesFeedback(ScriptIssueType.BerechnungFehlgeschlagen, "Berechnungsfehler bei Attribut " + (n + 1) + "\r\n" + " - " + tmp2.ErrorMessage); }
                v = tmp2.Variable;
            }

            // Den Typ der Variable checken
            var ok = false;

            foreach (var thisAt in exceptetType) {
                if (thisAt.TrimStart("*") == v.MyClassId) { ok = true; break; }
                if (thisAt.TrimStart("*") == Variable.Any_Plain) { ok = true; break; }
            }

            if (!ok) { return new SplittedAttributesFeedback(ScriptIssueType.FalscherDatentyp, "Attribut " + (n + 1) + " ist nicht einer der erwarteten Typen '" + exceptetType.JoinWith("' oder '") + "', sondern " + v.MyClassId); }

            feedbackVariables.Add(v);

            //if (s != null) { line += lb; }
        }
        return new SplittedAttributesFeedback(feedbackVariables);
    }

    public CanDoFeedback CanDo(string scriptText, int pos, bool expectedvariablefeedback, Script s, int line) {
        if (!expectedvariablefeedback && !string.IsNullOrEmpty(Returns)) {
            return new CanDoFeedback(pos, "Befehl an dieser Stelle nicht möglich", false, line);
        }
        if (expectedvariablefeedback && string.IsNullOrEmpty(Returns)) {
            return new CanDoFeedback(pos, "Befehl an dieser Stelle nicht möglich", false, line);
        }
        var maxl = scriptText.Length;

        foreach (var thiscomand in Comand(s)) {
            var comandtext = thiscomand + StartSequence;
            var l = comandtext.Length;
            if (pos + l < maxl) {
                if (string.Equals(scriptText.Substring(pos, l), comandtext, StringComparison.OrdinalIgnoreCase)) {
                    var f = GetEnd(scriptText, pos + thiscomand.Length, StartSequence.Length);
                    if (!string.IsNullOrEmpty(f.ErrorMessage)) {
                        return new CanDoFeedback(f.ContinuePosition, "Fehler bei " + comandtext + ": " + f.ErrorMessage, true, line);
                    }
                    var cont = f.ContinuePosition;
                    var codebltxt = string.Empty;
                    if (GetCodeBlockAfter) {
                        var (item1, item2) = GetCodeBlockText(scriptText, cont);
                        if (!string.IsNullOrEmpty(item2)) { return new CanDoFeedback(f.ContinuePosition, item2, true, line); }
                        codebltxt = item1;
                        cont = cont + codebltxt.Length + 2;
                    }
                    return new CanDoFeedback(cont, comandtext, f.AttributeText, codebltxt, line);
                }
            }
        }
        return new CanDoFeedback(pos, "Kann nicht geparst werden", false, line);
    }

    public abstract List<string> Comand(Script? s);

    public abstract DoItFeedback DoIt(CanDoFeedback infos, Script s);

    public string HintText() {
        var co = "Syntax:\r\n";
        co += "~~~~~~\r\n";
        co = co + Syntax + "\r\n";
        co += "\r\n";
        co += "Argumente:\r\n";
        co += "~~~~~~~~~~\r\n";
        for (var z = 0; z < Args.Count; z++) {
            co = co + "  - Argument " + (z + 1) + ": " + Args[z].JoinWith(", ");
            if (z == Args.Count - 1 && EndlessArgs) {
                co += " -> Dieses Argument kann beliebig oft wiederholt werden";
            }
            co += "\r\n";
        }
        co += "\r\n";
        co += "Rückgabe:\r\n";
        co += "~~~~~~~~~\r\n";
        co = co + "  - Rückgabetyp: " + Returns + "\r\n";
        co += "\r\n";
        co += "Beschreibung:\r\n";
        co += "~~~~~~~~~~~~\r\n";
        co = co + Description + "\r\n";
        return co;
    }

    public void OnChanged() => Changed?.Invoke(this, System.EventArgs.Empty);

    public string ReadableText() => Syntax;

    public QuickImage? SymbolForReadableText() => null;

    private GetEndFeedback GetEnd(string scriptText, int startpos, int lenghtStartSequence) {
        //z.B: beim Befehl DO
        if (string.IsNullOrEmpty(EndSequence)) {
            return new GetEndFeedback(startpos, string.Empty);
        }

        var (pos, which) = NextText(scriptText, startpos, new List<string> { EndSequence }, false, false, KlammernStd);
        if (pos < startpos) {
            return new GetEndFeedback("Endpunkt '" + EndSequence + "' nicht gefunden.");
        }

        var txtBTW = scriptText.Substring(startpos + lenghtStartSequence, pos - startpos - lenghtStartSequence);
        return new GetEndFeedback(pos + which.Length, txtBTW);
    }

    #endregion
}