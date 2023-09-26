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

    #region Fields

    public static readonly List<string> BoolVal = new() { VariableBool.ShortName_Plain };
    public static readonly List<string> DateTimeVar = new() { VariableDateTime.ShortName_Variable };
    public static readonly List<string> FloatVal = new() { VariableFloat.ShortName_Plain };
    public static readonly List<string> ListStringVar = new() { VariableListString.ShortName_Variable };
    public static readonly List<string> StringVal = new() { VariableString.ShortName_Plain };

    #endregion

    #region Events

    public event EventHandler? Changed;

    #endregion

    #region Properties

    public abstract List<List<string>> Args { get; }

    public abstract string Comand { get; }
    public abstract string Description { get; }

    public abstract bool EndlessArgs { get; }

    public string EndSequence {
        get {
            if (StartSequence == "(") {
                if (!string.IsNullOrEmpty(Returns)) { return ")"; } //  max(10,20)
                if (GetCodeBlockAfter) { return ")"; } // if
            }
            if (GetCodeBlockAfter) { return string.Empty; } // do {}
            if (StartSequence == "(") { return ");"; } // call("kk");

            return ";"; // break;
        }
    }

    public abstract bool GetCodeBlockAfter { get; }

    /// <summary>
    ///  Gibt die Syntax zurück, weil der Befehl selbst anders sein könnte. Z.B. Bei Variablan
    /// </summary>
    public string KeyName => Syntax;

    public abstract MethodType MethodType { get; }
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
    public static (string codeblock, string errorreason) GetCodeBlockText(string scriptText, int start) {
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

    public static GetEndFeedback GetEnd(string scriptText, int startpos, int lenghtStartSequence, string endsequwence, LogData ld) {
        //z.B: beim Befehl DO
        if (string.IsNullOrEmpty(endsequwence)) {
            return new GetEndFeedback(startpos, string.Empty);
        }

        var (pos, which) = NextText(scriptText, startpos, new List<string> { endsequwence }, false, false, KlammernStd);
        if (pos < startpos) {
            return new GetEndFeedback("Endpunkt '" + endsequwence + "' nicht gefunden.", ld);
        }

        var txtBtw = scriptText.Substring(startpos + lenghtStartSequence, pos - startpos - lenghtStartSequence);
        return new GetEndFeedback(pos + which.Length, txtBtw);
    }

    public static GetEndFeedback ReplaceComandsAndVars(string txt, VariableCollection varCol, LogData ld, ScriptProperties scp) {
        if (Script.Comands == null) { return new GetEndFeedback("Interner Fehler: Befehle nicht initialisiert", ld); }

        List<string> toSearch = new();

        #region Mögliche Methoden

        foreach (var thisc in Script.Comands) {
            if (!string.IsNullOrEmpty(thisc.Returns)) {
                toSearch.Add(thisc.Comand + thisc.StartSequence);
            }
        }

        #endregion

        #region Mögliche Variablen

        foreach (var thisv in varCol) {
            toSearch.Add(thisv.KeyName + "=");
        }

        #endregion

        var posc = 0;
        do {
            var (pos, _) = NextText(txt, posc, toSearch, true, false, KlammernStd);
            if (pos < 0) { return new GetEndFeedback(0, txt); }

            var f = Script.ComandOrVarOnPosition(varCol, scp, txt, pos, true, ld);
            if (!f.AllOk) { return new GetEndFeedback("Durch Befehl abgebrochen: " + txt, ld); }

            if (pos == 0 && txt.Length == f.Position) { return new GetEndFeedback(f.Variable); }
            if (f.Variable == null) { return new GetEndFeedback("Variablenfehler", ld); }
            if (!f.Variable.ToStringPossible) { return new GetEndFeedback("Variable muss als Objekt behandelt werden", ld); }

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
    /// <param name="varCol"></param>
    /// <param name="ld"></param>
    /// <returns></returns>
    public static GetEndFeedback ReplaceVariable(string txt, VariableCollection varCol, LogData ld) {
        var posc = 0;
        var v = varCol.AllStringableNames();

        do {
            var (pos, which) = NextText(txt, posc, v, true, true, KlammernStd);

            if (pos < 0) { return new GetEndFeedback(0, txt); }

            var thisV = varCol.Get(which);
            var endz = pos + which.Length;

            if (thisV == null) { return new GetEndFeedback("Variablen-Fehler " + which, ld); }

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

    public static SplittedAttributesFeedback SplitAttributeToVars(VariableCollection varcol, string attributText, List<List<string>> types, bool endlessArgs, LogData ld, ScriptProperties scp) {
        if (types.Count == 0) {
            return string.IsNullOrEmpty(attributText)
                ? new SplittedAttributesFeedback(new VariableCollection())
                : new SplittedAttributesFeedback(ScriptIssueType.AttributAnzahl, "Keine Attribute erwartet, aber erhalten.");
        }

        var attributes = SplitAttributeToString(attributText);
        if (attributes == null || attributes.Count == 0) { return new SplittedAttributesFeedback(ScriptIssueType.AttributAnzahl, "Allgemeiner Fehler."); }
        if (attributes.Count < types.Count) { return new SplittedAttributesFeedback(ScriptIssueType.AttributAnzahl, "Zu wenige Attribute erhalten."); }
        if (!endlessArgs && attributes.Count > types.Count) { return new SplittedAttributesFeedback(ScriptIssueType.AttributAnzahl, "Zu viele Attribute erhalten."); }

        //  Variablen und Routinen ersetzen
        VariableCollection feedbackVariables = new();
        for (var n = 0; n < attributes.Count; n++) {
            //var lb = attributes[n].Count(c => c == '¶'); // Zeilenzähler weitersetzen
            attributes[n] = attributes[n].RemoveChars("¶"); // Zeilenzähler entfernen

            var exceptetType = n < types.Count ? types[n] : types[types.Count - 1]; // Bei Endlessargs den letzten nehmen

            // Variable ermitteln oder eine Dummy-Variable als Rückgabe ermitteln
            Variable? v;

            var mustBeVar = exceptetType.Count > 0 && exceptetType[0].StartsWith("*");

            if (mustBeVar) {
                var varn = attributes[n];
                //if (varn.StartsWith("~") && varn.EndsWith("~")) {
                //    var tmp2 = Variable.GetVariableByParsing(varn.Substring(1, varn.Length - 2), s);
                //    if (tmp2.Variable is not VariableString x) { return new SplittedAttributesFeedback(ScriptIssueType.VariablenNamenBerechnungFehler, "Variablenname konnte nicht berechnet werden bei Attribut " + (n + 1), line); }
                //    varn = x.ValueString;
                //}

                if (!Variable.IsValidName(varn)) { return new SplittedAttributesFeedback(ScriptIssueType.VariableErwartet, "Variablenname erwartet bei Attribut " + (n + 1)); }

                v = varcol.Get(varn);
                if (v == null) { return new SplittedAttributesFeedback(ScriptIssueType.VariableNichtGefunden, "Variable nicht gefunden bei Attribut " + (n + 1)); }
            } else {
                var tmp2 = Variable.GetVariableByParsing(attributes[n], ld, varcol, scp);
                if (tmp2.Variable == null) { return new SplittedAttributesFeedback(ScriptIssueType.BerechnungFehlgeschlagen, "Berechnungsfehler bei Attribut " + (n + 1)); }
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

    /// <summary>
    ///
    /// </summary>
    /// <param name="infos"></param>
    /// <param name="scp"></param>
    /// <param name="newcommand">Erwartet wird: X=5;</param>
    /// <param name="varCol"></param>
    /// <param name="generateVariable"></param>
    /// <returns></returns>
    public static DoItFeedback VariablenBerechnung(CanDoFeedback infos, ScriptProperties scp, string newcommand, VariableCollection varCol, bool generateVariable) {
        //if (s.BerechneVariable == null) { return new DoItFeedback(infos.LogData, s, "Interner Fehler"); }

        var (pos, _) = NextText(newcommand, 0, Gleich, false, false, null);

        if (pos < 1 || pos > newcommand.Length - 2) { return new DoItFeedback(infos.Data, "Fehler mit = - Zeichen"); }

        var varnam = newcommand.Substring(0, pos);

        if (!Variable.IsValidName(varnam)) { return new DoItFeedback(infos.Data, varnam + " ist kein gültiger Variablen-Name"); }

        var vari = varCol.Get(varnam);
        if (generateVariable && vari != null) {
            return new DoItFeedback(infos.Data, "Variable " + varnam + " ist bereits vorhanden.");
        }
        if (!generateVariable && vari == null) {
            return new DoItFeedback(infos.Data, "Variable " + varnam + " nicht vorhanden.");
        }

        var value = newcommand.Substring(pos + 1, newcommand.Length - pos - 2);

        List<List<string>> sargs = new() { new List<string> { Variable.Any_Plain } };

        var attvar = SplitAttributeToVars(varCol, value, sargs, false, infos.Data, scp);

        if (!string.IsNullOrEmpty(attvar.ErrorMessage)) { return new DoItFeedback(infos.Data, attvar.ErrorMessage); }

        if (attvar.Attributes[0] is VariableUnknown) { return new DoItFeedback(infos.Data, "Variable unbekannt"); }

        if (attvar.Attributes[0] is Variable v) {
            if (generateVariable) {
                v.KeyName = varnam.ToLower();
                v.ReadOnly = false;
                varCol.Add(v);
                return new DoItFeedback(v);
            }

            if (vari == null) {
                // es sollte generateVariable greifen, und hier gar nimmer ankommen. Aber um die IDE zu befriedigen
                return new DoItFeedback(infos.Data, "Interner Fehler");
            }

            return vari.GetValueFrom(v, infos.Data);
        }
        // attvar.Attributes[0] müsste immer eine Variable sein...
        return new DoItFeedback(infos.Data, "Interner Fehler");
    }

    //public static SplittedAttributesFeedback SplitAttributeToVars(VariableCollection varcol, ScriptProperties scp, CanDoFeedback infos, List<List<string>> types, bool endlessArgs) {
    //    if (types.Count == 0) {
    //        return string.IsNullOrEmpty(infos.AttributText)
    //            ? new SplittedAttributesFeedback(new VariableCollection())
    //            : new SplittedAttributesFeedback(ScriptIssueType.AttributAnzahl, "Keine Attribute erwartet, aber erhalten.");
    //    }

    //    var attributes = SplitAttributeToString(infos.AttributText);
    //    if (attributes == null || attributes.Count == 0) { return new SplittedAttributesFeedback(ScriptIssueType.AttributAnzahl, "Allgemeiner Fehler."); }
    //    if (attributes.Count < types.Count) { return new SplittedAttributesFeedback(ScriptIssueType.AttributAnzahl, "Zu wenige Attribute erhalten."); }
    //    if (!endlessArgs && attributes.Count > types.Count) { return new SplittedAttributesFeedback(ScriptIssueType.AttributAnzahl, "Zu viele Attribute erhalten."); }

    //    //  Variablen und Routinen ersetzen
    //    VariableCollection feedbackVariables = new();
    //    for (var n = 0; n < attributes.Count; n++) {
    //        attributes[n] = attributes[n].RemoveChars("¶"); // Zeilenzähler entfernen

    //        var exceptetType = n < types.Count ? types[n] : types[types.Count - 1]; // Bei Endlessargs den letzten nehmen
    //        var mustBeVar = exceptetType.Count > 0 && exceptetType[0].StartsWith("*");

    //        // Variable ermitteln oder eine Dummy-Variable als Rückgabe ermitteln
    //        Variable? v;

    //        if (mustBeVar) {
    //            var varn = attributes[n];

    //            if (!Variable.IsValidName(varn)) { return new SplittedAttributesFeedback(ScriptIssueType.VariableErwartet, "Variablenname erwartet bei Attribut " + (n + 1)); }

    //            v = varcol.Get(varn);
    //            if (v == null) { return new SplittedAttributesFeedback(ScriptIssueType.VariableNichtGefunden, "Variable nicht gefunden bei Attribut " + (n + 1)); }
    //        } else {
    //            var tmp2 = Variable.GetVariableByParsing(attributes[n], infos.Data, varcol, scp);
    //            if (tmp2.Variable == null) { return new SplittedAttributesFeedback(ScriptIssueType.BerechnungFehlgeschlagen, "Berechnungsfehler bei Attribut " + (n + 1)); }
    //            v = tmp2.Variable;
    //        }

    //        // Den Typ der Variable checken
    //        var ok = false;

    //        foreach (var thisAt in exceptetType) {
    //            if (thisAt.TrimStart("*") == v.MyClassId) { ok = true; break; }
    //            if (thisAt.TrimStart("*") == Variable.Any_Plain) { ok = true; break; }
    //        }

    //        if (!ok) { return new SplittedAttributesFeedback(ScriptIssueType.FalscherDatentyp, "Attribut " + (n + 1) + " ist nicht einer der erwarteten Typen '" + exceptetType.JoinWith("' oder '") + "', sondern " + v.MyClassId); }

    //        feedbackVariables.Add(v);

    //        //if (s != null) { line += lb; }
    //    }
    //    return new SplittedAttributesFeedback(feedbackVariables);
    //}

    public CanDoFeedback CanDo(VariableCollection varCol, ScriptProperties scp, string scriptText, int pos, bool expectedvariablefeedback, LogData ld) {
        if (!expectedvariablefeedback && !string.IsNullOrEmpty(Returns)) {
            return new CanDoFeedback(scriptText, pos, "Befehl '" + Syntax + "' an dieser Stelle nicht möglich", false, ld);
        }
        if (expectedvariablefeedback && string.IsNullOrEmpty(Returns)) {
            return new CanDoFeedback(scriptText, pos, "Befehl '" + Syntax + "' an dieser Stelle nicht möglich", false, ld);
        }
        var maxl = scriptText.Length;

        var comandtext = Comand + StartSequence;
        var l = comandtext.Length;
        if (pos + l < maxl) {
            if (string.Equals(scriptText.Substring(pos, l), comandtext, StringComparison.OrdinalIgnoreCase)) {
                var f = GetEnd(scriptText, pos + Comand.Length, StartSequence.Length, EndSequence, ld);
                if (!f.AllOk) {
                    return new CanDoFeedback(scriptText, f.ContinuePosition, "Fehler bei " + comandtext, true, ld);
                }
                var cont = f.ContinuePosition;
                var codebltxt = string.Empty;
                if (GetCodeBlockAfter) {
                    var (codeblock, errorreason) = GetCodeBlockText(scriptText, cont);
                    if (!string.IsNullOrEmpty(errorreason)) { return new CanDoFeedback(scriptText, f.ContinuePosition, errorreason, true, ld); }
                    codebltxt = codeblock;
                    cont = cont + codebltxt.Length + 2;
                }

                if (!scp.AllowedMethods.HasFlag(MethodType)) {
                    return new CanDoFeedback(scriptText, pos, "Befehl '" + Syntax + "' kann in diesem Skript nicht benutzt werden.", true, ld);
                }

                return new CanDoFeedback(scriptText, cont, comandtext, f.AttributeText, codebltxt, ld);
            }
        }

        return new CanDoFeedback(scriptText, pos, "Kann nicht geparst werden", false, ld);
    }

    public abstract DoItFeedback DoIt(VariableCollection varCol, CanDoFeedback infos, ScriptProperties scp);

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

    #endregion
}