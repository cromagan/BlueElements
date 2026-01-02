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

using BlueBasics;
using BlueBasics.Interfaces;
using BlueScript.Enums;
using BlueScript.Structures;
using BlueScript.Variables;
using System;
using System.Collections.Generic;
using static BlueBasics.Constants;
using static BlueBasics.Extensions;

namespace BlueScript.Methods;

public abstract class Method : IReadableTextWithKey {

    #region Fields

    public static readonly List<string> BoolVal = [VariableBool.ShortName_Plain];
    public static readonly List<string> FloatVal = [VariableDouble.ShortName_Plain];
    public static readonly List<string> ListStringVar = [VariableListString.ShortName_Variable];
    public static readonly List<string> StringVal = [VariableString.ShortName_Plain];
    public static readonly List<string> StringVar = [VariableString.ShortName_Variable];

    #endregion

    #region Properties

    public static List<Method> AllMethods {
        get {
            field ??= Generic.GetInstaceOfType<Method>();
            return field;
        }
    }

    public abstract List<List<string>> Args { get; }
    public abstract string Command { get; }
    public abstract List<string> Constants { get; }
    public abstract string Description { get; }

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
    public bool KeyIsCaseSensitive => false;
    public string KeyName => Command;

    /// <summary>
    /// Gibt an, ob und wie oft das letzte Argument wiederholt werden kann bzw. muss.
    ///  -1 = das letzte Argument muss genau 1x vorhanden sein.
    ///   0 = das letzte Argument darf fehlen oder öfters vorhanden sein
    ///   1 = das letzte Argument darf öfters vorhanden sein
    /// > 2 = das letzte Argument muss mindestes so oft vorhanden sein.
    /// </summary>
    public abstract int LastArgMinCount { get; }

    public abstract MethodType MethodLevel { get; }

    //TODO: 0 implementieren
    public abstract bool MustUseReturnValue { get; }

    public string QuickInfo => HintText();
    public abstract string Returns { get; }

    public abstract string StartSequence { get; }

    public abstract string Syntax { get; }

    public List<string> UsesInDB { get; } = [];

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

        var (posek, _) = NextText(scriptText, start, KlammerGeschweiftZu, false, false, KlammernAlle);
        if (posek < start) {
            return (string.Empty, "Kein Codeblock Ende gefunden.");
        }

        var s = scriptText.Substring(start, tmp - start) + scriptText.Substring(tmp + 1, posek - tmp - 1);

        return (s, string.Empty);
    }

    public static GetEndFeedback GetEnd(string scriptText, int startpos, int lengthStartSequence, string endSequence, LogData? ld) {
        //z.B: beim Befehl DO
        if (string.IsNullOrEmpty(endSequence)) {
            return new GetEndFeedback(startpos, string.Empty);
        }

        var (pos, which) = NextText(scriptText, startpos, [endSequence], false, false, KlammernAlle);
        if (pos < startpos) {
            return new GetEndFeedback("Endpunkt '" + endSequence + "' nicht gefunden.", true, ld);
        }

        var txtBtw = scriptText.Substring(startpos + lengthStartSequence, pos - startpos - lengthStartSequence);
        return new GetEndFeedback(pos + which.Length, txtBtw);
    }

    public static List<Method> GetMethods(MethodType maxLevel) {
        var m = new List<Method>();

        foreach (var thism in AllMethods) {
            if (thism.MethodLevel <= maxLevel) {
                m.Add(thism);
            }
        }

        return m;
    }

    public static DoItFeedback GetVariableByParsing(string txt, LogData ld, VariableCollection varCol, ScriptProperties scp) {
        if (string.IsNullOrEmpty(txt)) { return new DoItFeedback("Kein Wert zum Parsen angekommen.", true, ld); }

        if (txt.StartsWith("(")) {
            var (pose, _) = NextText(txt, 0, KlammerRundZu, false, false, KlammernAlle);
            if (pose < txt.Length - 1 && pose > 0) {
                // Wir haben so einen Fall: (true) || (true)
                var scx = GetVariableByParsing(txt.Substring(1, pose - 1), ld, varCol, scp);
                if (scx.Failed) {
                    scx.ChangeFailedReason("Befehls-Berechnungsfehler in ()", true, ld);
                    return scx;
                }
                if (scx.ReturnValue == null) {
                    scx.ChangeFailedReason("Allgemeiner Befehls-Berechnungsfehler", true, ld);
                    return scx;
                }
                if (!scx.ReturnValue.ToStringPossible) {
                    scx.ChangeFailedReason("Falscher Variablentyp: " + scx.ReturnValue.MyClassId, true, ld);
                    return scx;
                }
                return GetVariableByParsing(scx.ReturnValue.ValueForReplace + txt.Substring(pose + 1), ld, varCol, scp);
            }
        }

        if (txt.StartsWith("[")) {
            var (pose, _) = NextText(txt, 0, KlammerEckigZu, false, false, KlammernAlle);
            if (pose == txt.Length - 1) {
                var tl = txt.Substring(1, pose - 1);

                if (!string.IsNullOrWhiteSpace(tl)) {
                    var l = SplitAttributeToVars("?", varCol, tl, [[VariableString.ShortName_Plain]], 1, ld, scp);
                    if (l.Failed) {
                        return new DoItFeedback(l.FailedReason, l.NeedsScriptFix, ld);
                    }
                    txt = "[\"" + l.Attributes.AllStringValues().JoinWith("\",\"") + "\"]";
                }
            }
        }

        txt = txt.Trim(KlammernRund);

        var (uu, _) = NextText(txt, 0, Method_If.UndUnd, false, false, KlammernAlle);
        if (uu > 0) {
            var scx = GetVariableByParsing(txt.Substring(0, uu), ld, varCol, scp);
            if (scx.Failed || scx.ReturnValue is null or VariableUnknown) {
                scx.ChangeFailedReason($"Befehls-Berechnungsfehler vor &&: {txt.Substring(0, uu)}", true, ld);
                return scx;
            }

            if (scx.ReturnValue is VariableBool { ValueBool: false }) { return scx; }
            return GetVariableByParsing(txt.Substring(uu + 2), ld, varCol, scp);
        }

        var (oo, _) = NextText(txt, 0, Method_If.OderOder, false, false, KlammernAlle);
        if (oo > 0) {
            var txt1 = GetVariableByParsing(txt.Substring(0, oo), ld, varCol, scp);
            if (txt1.Failed || txt1.ReturnValue is null or VariableUnknown) {
                return new DoItFeedback("Befehls-Berechnungsfehler vor ||", txt1.NeedsScriptFix, ld);
            }

            if (txt1.ReturnValue is VariableBool { ValueBool: true }) { return txt1; }
            return GetVariableByParsing(txt.Substring(oo + 2), ld, varCol, scp);
        }

        // Variablen nur ersetzen, wenn Variablen auch vorhanden sind.

        var t = ReplaceVariable(txt, varCol, ld);
        if (t.Failed) { return new DoItFeedback("Variablen-Berechnungsfehler", t.NeedsScriptFix, ld); }
        if (t.ReturnValue != null) { return new DoItFeedback(t.ReturnValue); }
        if (txt != t.NormalizedText) { return GetVariableByParsing(t.NormalizedText, ld, varCol, scp); }

        var t2 = ReplaceCommandsAndVars(txt, varCol, ld, scp);
        if (t2.Failed) { return new DoItFeedback(t2.FailedReason, t2.NeedsScriptFix, ld); }
        if (t2.ReturnValue != null) { return new DoItFeedback(t2.ReturnValue); }
        if (txt != t2.NormalizedText) { return GetVariableByParsing(t2.NormalizedText, ld, varCol, scp); }

        //var (posa, _) = NextText(txt, 0, KlammerRundAuf, false, false, KlammernAlle);
        //if (posa > -1) {
        //    var (pose, _) = NextText(txt, posa, KlammerRundZu, false, false, KlammernAlle);
        //    if (pose <= posa) { return DoItFeedback.KlammerFehler(ld); }

        //    var tmptxt = txt.Substring(posa + 1, pose - posa - 1);
        //    if (!string.IsNullOrEmpty(tmptxt)) {
        //        var scx = GetVariableByParsing(tmptxt, ld, varCol, scp);
        //        if (scx.Failed) {
        //            scx.ChangeFailedReason("Befehls-Berechnungsfehler in ()", true, ld);
        //            return scx;
        //        }
        //        if (scx.ReturnValue == null) {
        //            scx.ChangeFailedReason("Allgemeiner Berechnungsfehler in ()", true, ld);
        //            return scx;
        //        }
        //        if (!scx.ReturnValue.ToStringPossible) {
        //            scx.ChangeFailedReason("Falscher Variablentyp: " + scx.ReturnValue.MyClassId, true, ld);
        //            return scx;
        //        }
        //        return GetVariableByParsing(txt.Substring(0, posa) + scx.ReturnValue.ValueForReplace + txt.Substring(pose + 1), ld, varCol, scp);
        //    }
        //}

        if (ParseOperators(txt, varCol, scp, ld) is { } b) { return new DoItFeedback(b); }

        //if (VarTypes == null) {
        //    return new DoItFeedback(ld, "Variablentypen nicht initialisiert");
        //}

        foreach (var thisVt in Variable.VarTypes) {
            if (thisVt.GetFromStringPossible) {
                if (thisVt.TryParse(txt, out var v) && v != null) {
                    return new DoItFeedback(v);
                }
            }
        }

        return new DoItFeedback("Wert kann nicht geparsed werden: " + txt, true, ld);
    }

    public static GetEndFeedback ReplaceCommandsAndVars(string txt, VariableCollection varCol, LogData? ld, ScriptProperties scp) {
        List<string> toSearch = [];

        #region Mögliche Methoden

        foreach (var thisc in scp.AllowedMethods) {
            if (!string.IsNullOrEmpty(thisc.Returns)) {
                toSearch.Add(thisc.Command + thisc.StartSequence);
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
            var (pos, _) = NextText(txt, posc, toSearch, true, false, KlammernAlle);
            if (pos < 0) { return new GetEndFeedback(0, txt); }

            var scx = Script.CommandOrVarOnPosition(varCol, scp, txt, pos, true, ld);
            if (scx.Failed) {
                Develop.Message?.Invoke(BlueBasics.Enums.ErrorType.DevelopInfo, null, Develop.MonitorMessage, BlueBasics.Enums.ImageCode.Kritisch, "Skript-Fehler: " + scx.FailedReason, scp.Stufe);
                return new GetEndFeedback(scx.FailedReason, scx.NeedsScriptFix, ld);
            }

            if (pos == 0 && txt.Length == scx.Position) { return new GetEndFeedback(scx.ReturnValue); }
            if (scx.ReturnValue == null) { return new GetEndFeedback("Variablenfehler", true, ld); }
            if (!scx.ReturnValue.ToStringPossible) { return new GetEndFeedback("Variable muss als Objekt behandelt werden", true, ld); }

            txt = txt.Substring(0, pos) + scx.ReturnValue.ValueForReplace + txt.Substring(scx.Position);
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
    public static GetEndFeedback ReplaceVariable(string txt, VariableCollection? varCol, LogData? ld) {
        if (varCol is not { }) { return new GetEndFeedback("Interner Variablen-Fehler", true, ld); }

        var posc = 0;
        var allVarNames = varCol.AllStringableNames();

        do {
            var (pos, which) = NextText(txt, posc, allVarNames, true, true, KlammernAlle);

            if (pos < 0) { return new GetEndFeedback(0, txt); }

            var thisV = varCol.GetByKey(which);
            var endz = pos + which.Length;

            if (thisV == null) { return new GetEndFeedback("Variablen-Fehler " + which, true, ld); }

            txt = txt.Substring(0, pos) + thisV.ValueForReplace + txt.Substring(endz);
            posc = pos;
        } while (true);
    }

    public static SplittedAttributesFeedback SplitAttributeToVars(string comand, VariableCollection? varcol, string attributText, List<List<string>> types, int lastArgMinCount, LogData? ld, ScriptProperties? scp) {
        if (types.Count == 0) {
            return string.IsNullOrEmpty(attributText)
                ? new SplittedAttributesFeedback([])
                : new SplittedAttributesFeedback(ScriptIssueType.AttributAnzahl, "Keine Attribute erwartet, aber erhalten.", true);
        }

        var attributes = SplitAttributeToString(attributText);
        if (attributes is not { Count: not 0 }) { return new SplittedAttributesFeedback(ScriptIssueType.AttributAnzahl, "Allgemeiner Fehler bei den Attributen.", true); }
        if (attributes.Count < types.Count && lastArgMinCount != 0) { return new SplittedAttributesFeedback(ScriptIssueType.AttributAnzahl, $"Zu wenige Attribute bei '{comand}' erhalten.", true); }
        if (attributes.Count < types.Count - 1) { return new SplittedAttributesFeedback(ScriptIssueType.AttributAnzahl, $"Zu wenige Attribute bei '{comand}' erhalten.", true); }
        if (lastArgMinCount < 0 && attributes.Count > types.Count) { return new SplittedAttributesFeedback(ScriptIssueType.AttributAnzahl, $"Zu viele Attribute bei '{comand}' erhalten.", true); }
        if (lastArgMinCount >= 1 && attributes.Count < types.Count + lastArgMinCount - 1) { return new SplittedAttributesFeedback(ScriptIssueType.AttributAnzahl, $"Zu wenige Attribute bei '{comand}' erhalten.", true); }

        //  Variablen und Routinen ersetzen
        VariableCollection feedbackVariables = [];
        for (var n = 0; n < attributes.Count; n++) {
            //var lb = attributes[n].Count(c => c == '¶'); // Zeilenzähler weitersetzen
            attributes[n] = attributes[n].RemoveChars("¶"); // Zeilenzähler entfernen

            var exceptetType = n < types.Count ? types[n] : types[types.Count - 1]; // Bei Endlessargs den letzten nehmen

            // Variable ermitteln oder eine Dummy-Variable als Rückgabe ermitteln
            Variable? v;

            var mustBeVar = exceptetType.Count > 0 && exceptetType[0].StartsWith("*");

            if (mustBeVar) {
                var varn = attributes[n];
                if (!Variable.IsValidName(varn)) { return new SplittedAttributesFeedback(ScriptIssueType.VariableErwartet, "Variablenname erwartet bei Attribut " + (n + 1), true); }

                v = varcol?.GetByKey(varn);
                if (v == null) { return new SplittedAttributesFeedback(ScriptIssueType.VariableNichtGefunden, "Variable nicht gefunden bei Attribut " + (n + 1), true); }
            } else {
                var tmp2 = GetVariableByParsing(attributes[n], ld, varcol, scp);
                if (tmp2.Failed) { return new SplittedAttributesFeedback(ScriptIssueType.BerechnungFehlgeschlagen, tmp2.FailedReason, tmp2.NeedsScriptFix); }
                if (tmp2.ReturnValue == null) { return new SplittedAttributesFeedback(ScriptIssueType.BerechnungFehlgeschlagen, $"Interner Fehler", true); }

                if (tmp2.ReturnValue is VariableUnknown) {
                    foreach (var thisC in AllMethods) {
                        var f = thisC.CanDo(attributes[n], 0, false, ld);
                        if (string.IsNullOrEmpty(f.FailedReason)) {
                            return new SplittedAttributesFeedback(ScriptIssueType.BerechnungFehlgeschlagen, $"Der Befehl '{comand}' kann in diesen Skript nicht verwendet werden.", true);
                        }
                    }
                }

                v = tmp2.ReturnValue;
            }

            // Den Typ der Variable checken
            var ok = false;

            foreach (var thisAt in exceptetType) {
                if (thisAt.TrimStart("*") == v.MyClassId) { ok = true; break; }
                if (thisAt.TrimStart("*") == Variable.Any_Plain) { ok = true; break; }
            }

            if (!ok) { return new SplittedAttributesFeedback(ScriptIssueType.FalscherDatentyp, $"Bei '{comand}' ist das Attribut '{n + 1}' nicht einer der erwarteten Typen '{exceptetType.JoinWith("' oder '")}', sondern {v.MyClassId}", true); }

            feedbackVariables.Add(v);

            //if (s != null) { line += lb; }
        }
        return new SplittedAttributesFeedback(feedbackVariables);
    }

    /// <summary>
    ///
    /// </summary>
    /// <param name="varCol"></param>
    /// <param name="ld"></param>
    /// <param name="scp"></param>
    /// <param name="newcommand">Erwartet wird: X=5;</param>
    /// <param name="generateVariable"></param>
    /// <returns></returns>
    public static DoItFeedback VariablenBerechnung(VariableCollection varCol, LogData ld, ScriptProperties scp, string newcommand, bool generateVariable) {
        var (pos, _) = NextText(newcommand, 0, Gleich, false, false, null);

        if (pos < 1 || pos > newcommand.Length - 2) { return new DoItFeedback("Fehler mit = - Zeichen", true, ld); }

        var varnam = newcommand.Substring(0, pos);

        if (!Variable.IsValidName(varnam)) { return new DoItFeedback(varnam + " ist kein gültiger Variablen-Name", true, ld); }

        var vari = varCol.GetByKey(varnam);
        if (generateVariable && vari != null) {
            return new DoItFeedback("Variable " + varnam + " ist bereits vorhanden.", true, ld);
        }
        if (!generateVariable && vari == null) {
            return new DoItFeedback("Variable " + varnam + " nicht vorhanden.", true, ld);
        }

        var value = newcommand.Substring(pos + 1, newcommand.Length - pos - 2);

        List<List<string>> sargs = [[Variable.Any_Plain]];

        var attvar = SplitAttributeToVars("var", varCol, value, sargs, 0, ld, scp);

        if (attvar.Failed) { return new DoItFeedback(attvar.FailedReason, attvar.NeedsScriptFix, ld); }

        if (attvar.Attributes[0] is VariableUnknown) { return new DoItFeedback("Variable unbekannt", true, ld); }

        if (attvar.Attributes[0] is { } v) {
            if (generateVariable) {
                v.KeyName = varnam.ToLowerInvariant();
                v.ReadOnly = false;
                varCol.Add(v);
                return new DoItFeedback(v);
            }

            if (vari == null) {
                // es sollte generateVariable greifen, und hier gar nimmer ankommen. Aber um die IDE zu befriedigen
                return DoItFeedback.InternerFehler(ld);
            }

            var f = vari.GetValueFrom(v);
            return new DoItFeedback(f, !string.IsNullOrWhiteSpace(f), ld);
        }
        // attvar.Attributes[0] müsste immer eine Variable sein...
        return DoItFeedback.InternerFehler(ld);
    }

    public CanDoFeedback CanDo(string scriptText, int pos, bool expectedvariablefeedback, LogData? ld) {
        if (!expectedvariablefeedback && !string.IsNullOrEmpty(Returns) && MustUseReturnValue) {
            return new CanDoFeedback(pos, "Befehl '" + Syntax + "' an dieser Stelle nicht möglich", false, ld);
        }
        if (expectedvariablefeedback && string.IsNullOrEmpty(Returns)) {
            return new CanDoFeedback(pos, "Befehl '" + Syntax + "' an dieser Stelle nicht möglich", false, ld);
        }
        var maxl = scriptText.Length;

        var commandtext = Command + StartSequence;
        var l = commandtext.Length;
        if (pos + l < maxl) {
            if (string.Equals(scriptText.Substring(pos, l), commandtext, StringComparison.OrdinalIgnoreCase)) {
                var f = GetEnd(scriptText, pos + Command.Length, StartSequence.Length, EndSequence, ld);
                if (f.Failed) {
                    return new CanDoFeedback(f.ContinuePosition, "Fehler bei " + commandtext, true, ld);
                }
                var cont = f.ContinuePosition;
                var codebltxt = string.Empty;
                if (GetCodeBlockAfter) {
                    var (codeblock, errorreason) = GetCodeBlockText(scriptText, cont);
                    if (!string.IsNullOrEmpty(errorreason)) { return new CanDoFeedback(f.ContinuePosition, errorreason, true, ld); }
                    codebltxt = codeblock;
                    cont = cont + codebltxt.Length + 2;
                }

                //if (!scp.AllowedMethods.HasFlag(MethodType)) {
                //    return new CanDoFeedback(pos, "Befehl '" + Syntax + "' kann in diesem Skript an der aktuellen Position nicht benutzt werden.", true, ld);
                //}

                return new CanDoFeedback(cont, f.NormalizedText, codebltxt, ld);
            }
        }

        return new CanDoFeedback(pos, "Kann nicht geparst werden", false, ld);
    }

    public virtual DoItFeedback DoIt(VariableCollection varCol, CanDoFeedback infos, ScriptProperties scp) {
        try {
            var attvar = SplitAttributeToVars(Command, varCol, infos.AttributText, Args, LastArgMinCount, infos.LogData, scp);
            return attvar.Failed
                ? DoItFeedback.AttributFehler(infos.LogData, attvar)
                : DoIt(varCol, attvar, scp, infos.LogData);
        } catch (Exception ex) {
            return new DoItFeedback("Interner Programmfehler: " + ex.Message, true, infos.LogData);
        }
    }

    public abstract DoItFeedback DoIt(VariableCollection varCol, SplittedAttributesFeedback attvar, ScriptProperties scp, LogData ld);

    //        feedbackVariables.Add(v);
    public string HintText() {
        var co = "Syntax:\r\n";
        co += "~~~~~~\r\n";
        co = co + Syntax + "\r\n";
        co += "\r\n";
        co += "Argumente:\r\n";
        co += "~~~~~~~~~~\r\n";
        for (var z = 0; z < Args.Count; z++) {
            var a = Args[z].JoinWith(", ");
            if (a.Contains("*")) {
                a = a.Replace("*", string.Empty) + " (muss eine vorhandene Variable sein)";
            }

            co = co + "  - Argument " + (z + 1) + ": " + a;

            if (z == Args.Count - 1) {
                switch (LastArgMinCount) {
                    case -1:
                        break; // genau einmal
                    case 0:
                        co += " (darf fehlen; darf mehrfach wiederholt werden)";
                        break;

                    case 1:
                        co += " (muss angegeben werden; darf mehrfach wiederholt werden)";
                        break;

                    default:

                        co += " (muss mindestens " + LastArgMinCount + "x wiederholt werden)";
                        break;
                }
            }
            co += "\r\n";
        }
        co += "\r\n";
        co += "Rückgabe:\r\n";
        co += "~~~~~~~~\r\n";
        if (string.IsNullOrEmpty(Returns)) {
            co += "  - Rückgabetyp: -\r\n";
        } else {
            co = MustUseReturnValue
                ? co + "  - Rückgabetyp: " + Returns + "(muss verwendet werden)\r\n"
                : co + "  - Rückgabetyp: " + Returns + " (darf verworfen werden)\r\n";
        }

        co += "\r\n";
        co += "Beschreibung:\r\n";
        co += "~~~~~~~~~~~\r\n";
        co = co + Description + "\r\n";

        if (Constants.Count > 0) {
            co += "\r\n";
            co += "Konstanten:\r\n";
            co += "~~~~~~~~~~~~\r\n";
            co += Constants.JoinWithCr() + "\r\n";
        }

        //if (this is IUseableForButton) {
        //    co += "\r\n";
        //    co += "Hinweis:\r\n";
        //    co += "~~~~~~~~~~~~\r\n";
        //    co += "Diese Methode kann auch im Formular durch einen Knopfdruck ausgelöst werden.\r\n";
        //}

        if (UsesInDB.Count > 0) {
            co += "\r\n";
            co += "Aktuelle Verwendung in TABELLEN-Skripten:\r\n";
            co += "~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~\r\n";
            co += UsesInDB.JoinWithCr();
        }

        return co;
    }

    public string ReadableText() => Syntax;

    public QuickImage? SymbolForReadableText() => null;

    private static bool? ParseOperators(string txt, VariableCollection varCol, ScriptProperties scp, LogData ld) {
        if (Variable.TryParseValue<VariableBool>(txt, out var result) && result is bool b) { return b; }

        #region Auf Restliche Boolsche Operationen testen

        //foreach (var check in Method_if.VergleichsOperatoren) {
        var (i, check) = NextText(txt, 0, Method_If.VergleichsOperatoren, false, false, KlammernAlle);
        if (i > -1) {
            if (i < 1 && check != "!") { return null; } // <1, weil ja mindestens ein Zeichen vorher sein MUSS!

            if (i >= txt.Length - 1) { return null; } // siehe oben

            #region Die Werte vor und nach dem Trennzeichen in den Variablen v1 und v2 ablegen

            #region Ersten Wert als s1 ermitteln

            var s1 = txt.Substring(0, i);
            Variable? v1 = null;
            if (!string.IsNullOrEmpty(s1)) {
                var tmp1 = GetVariableByParsing(s1, ld, varCol, scp);
                if (tmp1.Failed) { return null; }
                v1 = tmp1.ReturnValue;
            } else {
                if (check != "!") { return null; }
            }

            #endregion

            #region Zweiten Wert als s2 ermitteln

            var s2 = txt.Substring(i + check.Length);
            if (string.IsNullOrEmpty(s2)) { return null; }

            var tmp2 = GetVariableByParsing(s2, ld, varCol, scp);
            if (tmp2.Failed) { return null; }

            var v2 = tmp2.ReturnValue;

            #endregion

            // V2 braucht nicht peprüft werden, muss ja eh der gleiche TYpe wie V1 sein
            if (v1 != null) {
                if (v1.MyClassId != v2?.MyClassId) { return null; }
                if (!v1.ToStringPossible) { return null; }
            } else {
                if (v2 is not VariableBool) { return null; }
            }

            #endregion

            switch (check) {
                case "==": {
                        if (v1 == null) { return null; }
                        return v1.ValueForReplace == v2.ValueForReplace;
                    }

                case "!=": {
                        if (v1 == null) { return null; }
                        return v1.ValueForReplace != v2.ValueForReplace;
                    }

                case ">=": {
                        if (v1 is not VariableDouble v1Fl) { return null; }
                        if (v2 is not VariableDouble v2Fl) { return null; }
                        return v1Fl.ValueNum >= v2Fl.ValueNum;
                    }

                case "<=": {
                        if (v1 is not VariableDouble v1Fl) { return null; }
                        if (v2 is not VariableDouble v2Fl) { return null; }
                        return v1Fl.ValueNum <= v2Fl.ValueNum;
                    }

                case "<": {
                        if (v1 is not VariableDouble v1Fl) { return null; }
                        if (v2 is not VariableDouble v2Fl) { return null; }
                        return v1Fl.ValueNum < v2Fl.ValueNum;
                    }

                case ">": {
                        if (v1 is not VariableDouble v1Fl) { return null; }
                        if (v2 is not VariableDouble v2Fl) { return null; }
                        return v1Fl.ValueNum > v2Fl.ValueNum;
                    }

                case "||": {
                        if (v1 is not VariableBool v1Bo) { return null; }
                        if (v2 is not VariableBool v2Bo) { return null; }
                        return v1Bo.ValueBool || v2Bo.ValueBool;
                    }

                case "&&": {
                        if (v1 is not VariableBool v1Bo) { return null; }
                        if (v2 is not VariableBool v2Bo) { return null; }
                        return v1Bo.ValueBool && v2Bo.ValueBool;
                    }

                case "!": {
                        // S1 dürfte eigentlich nie was sein: !False||!false
                        // entweder ist es ganz am anfang, oder direkt nach einem Trenneichen
                        if (v2 is not VariableBool v2Bo) { return null; }
                        return !v2Bo.ValueBool;
                    }
            }
        }

        #endregion

        return null;
    }

    private static List<string>? SplitAttributeToString(string attributtext) {
        if (string.IsNullOrEmpty(attributtext)) { return null; }
        List<string> attributes = [];

        #region Liste der Attribute splitten

        var posc = 0;
        do {
            var (pos, _) = NextText(attributtext, posc, Komma, false, false, KlammernAlle);
            if (pos < 0) {
                attributes.Add(attributtext.Substring(posc).Trim(KlammernRund));
                break;
            }
            attributes.Add(attributtext.Substring(posc, pos - posc).Trim(KlammernRund));
            posc = pos + 1;
        } while (true);

        #endregion

        return attributes;
    }

    #endregion
}