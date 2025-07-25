﻿// Authors:
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
    private static List<Method>? _allMethods;

    #endregion

    #region Properties

    public static List<Method> AllMethods {
        get {
            _allMethods ??= Generic.GetInstaceOfType<Method>();
            return _allMethods;
        }
    }

    public abstract List<List<string>> Args { get; }
    public string ColumnQuickInfo => HintText();
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

    public string KeyName => Command;

    /// <summary>
    /// Gibt an, ob und wie oft das letzte Argument wiederholt werden kann bzw. muss.
    ///  -1 = das letzte Argument muss genau 1x vorhanden sein.
    ///   0 = das letzte Argument darf fehlen oder öfters vorhanden sein
    ///   1 = das letzte Argument darf öfters vorhanden sein
    /// > 2 = das letzte Argument muss mindestes so oft vorhanden sein.
    /// </summary>
    public abstract int LastArgMinCount { get; }

    public abstract MethodType MethodType { get; }

    //TODO: 0 implementieren
    public abstract bool MustUseReturnValue { get; }

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

    public static GetEndFeedback GetEnd(string scriptText, int startpos, int lenghtStartSequence, string endSequence, LogData? ld) {
        //z.B: beim Befehl DO
        if (string.IsNullOrEmpty(endSequence)) {
            return new GetEndFeedback(startpos, string.Empty);
        }

        var (pos, which) = NextText(scriptText, startpos, [endSequence], false, false, KlammernAlle);
        if (pos < startpos) {
            return new GetEndFeedback("Endpunkt '" + endSequence + "' nicht gefunden.", true, ld);
        }

        var txtBtw = scriptText.Substring(startpos + lenghtStartSequence, pos - startpos - lenghtStartSequence);
        return new GetEndFeedback(pos + which.Length, txtBtw);
    }

    public static List<Method> GetMethods(MethodType allowedMethods) {
        var m = new List<Method>();

        foreach (var thism in AllMethods) {
            if (allowedMethods.HasFlag(thism.MethodType)) {
                m.Add(thism);
            }
        }

        return m;
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
                Develop.Message?.Invoke( BlueBasics.Enums.ErrorType.DevelopInfo, null, Develop.MonitorMessage,   BlueBasics.Enums.ImageCode.Kritisch,  "Skript-Fehler: " + scx.FailedReason, scp.Stufe);
                return new GetEndFeedback($"Durch Befehl abgebrochen: {txt} -> {scx.FailedReason}", scx.NeedsScriptFix, ld); 
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

            var thisV = varCol.Get(which);
            var endz = pos + which.Length;

            if (thisV == null) { return new GetEndFeedback("Variablen-Fehler " + which, true, ld); }

            txt = txt.Substring(0, pos) + thisV.ValueForReplace + txt.Substring(endz);
            posc = pos;
        } while (true);
    }

    public static SplittedAttributesFeedback SplitAttributeToVars(VariableCollection? varcol, string attributText, List<List<string>> types, int lastArgMinCount, LogData? ld, ScriptProperties? scp) {
        if (types.Count == 0) {
            return string.IsNullOrEmpty(attributText)
                ? new SplittedAttributesFeedback([])
                : new SplittedAttributesFeedback(ScriptIssueType.AttributAnzahl, "Keine Attribute erwartet, aber erhalten.", true);
        }

        var attributes = SplitAttributeToString(attributText);
        if (attributes is not { Count: not 0 }) { return new SplittedAttributesFeedback(ScriptIssueType.AttributAnzahl, "Allgemeiner Fehler bei den Attributen.", true); }
        if (attributes.Count < types.Count && lastArgMinCount != 0) { return new SplittedAttributesFeedback(ScriptIssueType.AttributAnzahl, "Zu wenige Attribute erhalten.", true); }
        if (attributes.Count < types.Count - 1) { return new SplittedAttributesFeedback(ScriptIssueType.AttributAnzahl, "Zu wenige Attribute erhalten.", true); }
        if (lastArgMinCount < 0 && attributes.Count > types.Count) { return new SplittedAttributesFeedback(ScriptIssueType.AttributAnzahl, "Zu viele Attribute erhalten.", true); }
        if (lastArgMinCount >= 1 && attributes.Count < (types.Count + lastArgMinCount - 1)) { return new SplittedAttributesFeedback(ScriptIssueType.AttributAnzahl, "Zu wenige Attribute erhalten.", true); }

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

                v = varcol?.Get(varn);
                if (v == null) { return new SplittedAttributesFeedback(ScriptIssueType.VariableNichtGefunden, "Variable nicht gefunden bei Attribut " + (n + 1), true); }
            } else {
                var tmp2 = Variable.GetVariableByParsing(attributes[n], ld, varcol, scp);
                if (tmp2.Failed) { return new SplittedAttributesFeedback(ScriptIssueType.BerechnungFehlgeschlagen, $"Berechnungsfehler bei Attribut {n + 1} {tmp2.FailedReason}", tmp2.NeedsScriptFix); }
                if (tmp2.ReturnValue == null) { return new SplittedAttributesFeedback(ScriptIssueType.BerechnungFehlgeschlagen, $"Interner Fehler", true); }

                v = tmp2.ReturnValue;
            }

            // Den Typ der Variable checken
            var ok = false;

            foreach (var thisAt in exceptetType) {
                if (thisAt.TrimStart("*") == v.MyClassId) { ok = true; break; }
                if (thisAt.TrimStart("*") == Variable.Any_Plain) { ok = true; break; }
            }

            if (!ok) { return new SplittedAttributesFeedback(ScriptIssueType.FalscherDatentyp, "Attribut " + (n + 1) + " ist nicht einer der erwarteten Typen '" + exceptetType.JoinWith("' oder '") + "', sondern " + v.MyClassId, true); }

            _ = feedbackVariables.Add(v);

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

        var vari = varCol.Get(varnam);
        if (generateVariable && vari != null) {
            return new DoItFeedback("Variable " + varnam + " ist bereits vorhanden.", true, ld);
        }
        if (!generateVariable && vari == null) {
            return new DoItFeedback("Variable " + varnam + " nicht vorhanden.", true, ld);
        }

        var value = newcommand.Substring(pos + 1, newcommand.Length - pos - 2);

        List<List<string>> sargs = [[Variable.Any_Plain]];

        var attvar = SplitAttributeToVars(varCol, value, sargs, 0, ld, scp);

        if (attvar.Failed) { return new DoItFeedback("Der Wert nach dem '=' konnte nicht berechnet werden: " + attvar.FailedReason, attvar.NeedsScriptFix, ld); }

        if (attvar.Attributes[0] is VariableUnknown) { return new DoItFeedback("Variable unbekannt", true, ld); }

        if (attvar.Attributes[0] is { } v) {
            if (generateVariable) {
                v.KeyName = varnam.ToLowerInvariant();
                v.ReadOnly = false;
                _ = varCol.Add(v);
                return new DoItFeedback(v);
            }

            if (vari == null) {
                // es sollte generateVariable greifen, und hier gar nimmer ankommen. Aber um die IDE zu befriedigen
                return DoItFeedback.InternerFehler(ld);
            }

            return vari.GetValueFrom(v, ld);
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

                return new CanDoFeedback(cont, f.AttributeText, codebltxt, ld);
            }
        }

        return new CanDoFeedback(pos, "Kann nicht geparst werden", false, ld);
    }

    public virtual DoItFeedback DoIt(VariableCollection varCol, CanDoFeedback infos, ScriptProperties scp) {
        var attvar = SplitAttributeToVars(varCol, infos.AttributText, Args, LastArgMinCount, infos.LogData, scp);
        return attvar.Failed
            ? DoItFeedback.AttributFehler(infos.LogData, this, attvar)
            : DoIt(varCol, attvar, scp, infos.LogData);
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
            co += "Aktuelle Verwendung in DATENBANK-Skripten:\r\n";
            co += "~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~\r\n";
            co += UsesInDB.JoinWithCr();
        }

        return co;
    }

    public string ReadableText() => Syntax;

    public QuickImage? SymbolForReadableText() => null;

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

        #endregion Liste der Attribute splitten

        return attributes;
    }

    #endregion
}