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
using BlueBasics.Interfaces;
using BlueScript.Enums;
using BlueScript.Structures;
using BlueScript.Variables;
using System;
using System.CodeDom;
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

    public static GetEndFeedback GetEnd(CanDoFeedback cdf, int lenghtStartSequence, string endSequence) {
        //z.B: beim Befehl DO
        if (string.IsNullOrEmpty(endSequence)) {
            return new GetEndFeedback(cdf, string.Empty);
        }

        var (pos, which) = NextText(cdf.NormalizedText, cdf.Position, [endSequence], false, false, KlammernAlle);
        if (pos < cdf.Position) {
            return new GetEndFeedback(cdf, "Endpunkt '" + endSequence + "' nicht gefunden.", true);
        }

        var txtBtw = cdf.NormalizedText.Substring(cdf.Position + lenghtStartSequence, pos - cdf.Position - lenghtStartSequence);
        return new GetEndFeedback(cdf, txtBtw);
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

    public static DoItFeedback GetVariableByParsing(string toParse, CanDoFeedback cdf, VariableCollection? varCol, ScriptProperties scp) {
        if (string.IsNullOrEmpty(toParse)) { return new DoItFeedback("Kein Wert zum Parsen angekommen.", true, cdf); }

        if (toParse.StartsWith("(")) {
            var (pose, _) = NextText(toParse, 0, KlammerRundZu, false, false, KlammernAlle);
            if (pose < toParse.Length - 1) {
                // Wir haben so einen Fall: (true) || (true)
                var scx = GetVariableByParsing(toParse.Substring(1, pose - 1), cdf, varCol, scp);
                if (scx.Failed) {
                    scx.ChangeFailedReason("Befehls-Berechnungsfehler in ()");
                    return scx;
                }
                if (scx.ReturnValue == null) {
                    scx.ChangeFailedReason("Allgemeiner Befehls-Berechnungsfehler");
                    return scx;
                }
                if (!scx.ReturnValue.ToStringPossible) {
                    scx.ChangeFailedReason("Falscher Variablentyp: " + scx.ReturnValue.MyClassId);
                    return scx;
                }
                return GetVariableByParsing(scx.ReturnValue.ValueForReplace + toParse.Substring(pose + 1), cdf, varCol, scp);
            }
        }

        toParse = toParse.Trim(KlammernRund);

        var (uu, _) = NextText(toParse, 0, Method_If.UndUnd, false, false, KlammernAlle);
        if (uu > 0) {
            var scx = GetVariableByParsing(toParse.Substring(0, uu), cdf, varCol, scp);
            if (scx.Failed || scx.ReturnValue is null or VariableUnknown) {
                scx.ChangeFailedReason($"Befehls-Berechnungsfehler vor &&: {toParse.Substring(0, uu)}");
                return scx;
            }

            if (scx.ReturnValue.ValueForReplace == "false") { return scx; }
            return GetVariableByParsing(toParse.Substring(uu + 2), cdf, varCol, scp);
        }

        var (oo, _) = NextText(toParse, 0, Method_If.OderOder, false, false, KlammernAlle);
        if (oo > 0) {
            var txt1 = GetVariableByParsing(toParse.Substring(0, oo), cdf, varCol, scp);
            if (txt1.Failed || txt1.ReturnValue is null or VariableUnknown) {
                return new DoItFeedback("Befehls-Berechnungsfehler vor ||", txt1.NeedsScriptFix, cdf);
            }

            if (txt1.ReturnValue.ValueForReplace == "true") {
                return txt1;
            }
            return GetVariableByParsing(toParse.Substring(oo + 2), cdf, varCol, scp);
        }

        // Variablen nur ersetzen, wenn Variablen auch vorhanden sind.
        if (varCol is { }) {
            var t = Method.ReplaceVariable(toParse, varCol, cdf);
            if (t.Failed) { return new DoItFeedback("Variablen-Berechnungsfehler", t.NeedsScriptFix, cdf); }
            if (t.ReturnValue != null) { return new DoItFeedback(t.ReturnValue, new CurrentPosition(cdf, cdf.Position + toParse.Length)); }
            if (toParse != t.NormalizedText) { return GetVariableByParsing(t.NormalizedText, cdf, varCol, scp); }

            var t2 = Method.ReplaceCommandsAndVars(varCol, cdf, scp);
            if (t2.Failed) { return new DoItFeedback($"Befehls-Berechnungsfehler: {t2.FailedReason}", t2.NeedsScriptFix, cdf); }
            if (t2.ReturnValue != null) { return new DoItFeedback(t2.ReturnValue, new CurrentPosition(cdf, cdf.Position + toParse.Length)); }
            if (toParse != t2.NormalizedText) { return GetVariableByParsing(t2.NormalizedText, cdf, varCol, scp); }
        }

        var (posa, _) = NextText(toParse, 0, KlammerRundAuf, false, false, KlammernAlle);
        if (posa > -1) {
            var (pose, _) = NextText(toParse, posa, KlammerRundZu, false, false, KlammernAlle);
            if (pose <= posa) { return DoItFeedback.KlammerFehler(cdf); }

            var tmptxt = toParse.Substring(posa + 1, pose - posa - 1);
            if (!string.IsNullOrEmpty(tmptxt)) {
                var scx = GetVariableByParsing(tmptxt, cdf, varCol, scp);
                if (scx.Failed) {
                    scx.ChangeFailedReason("Befehls-Berechnungsfehler in ()");
                    return scx;
                }
                if (scx.ReturnValue == null) {
                    scx.ChangeFailedReason("Allgemeiner Berechnungsfehler in ()");
                    return scx;
                }
                if (!scx.ReturnValue.ToStringPossible) {
                    scx.ChangeFailedReason("Falscher Variablentyp: " + scx.ReturnValue.MyClassId);
                    return scx;
                }
                return GetVariableByParsing(toParse.Substring(0, posa) + scx.ReturnValue.ValueForReplace + toParse.Substring(pose + 1), cdf, varCol, scp);
            }
        }

        var (cando, result) = TryParse(toParse, varCol, scp);
        if (cando) {
            return new DoItFeedback(result, cdf);
        }

        foreach (var thisVt in Variable.VarTypes) {
            if (thisVt.GetFromStringPossible) {
                if (thisVt.TryParse(toParse, out var v) && v != null) {
                    return new DoItFeedback(v, new CurrentPosition(cdf, cdf.Position + toParse.Length));
                }
            }
        }

        return new DoItFeedback("Wert kann nicht geparsed werden: " + toParse, true, cdf);
    }

    public static GetEndFeedback ReplaceCommandsAndVars(VariableCollection varCol, CanDoFeedback cdf, ScriptProperties scp) {
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
            var (pos, _) = NextText(cdf.NormalizedText, posc, toSearch, true, false, KlammernAlle);
            if (pos < 0) { return new GetEndFeedback(cdf, cdf.NormalizedText); }

            var scx = Script.CommandOrVarOnPosition(varCol, scp, true, cdf);
            if (scx.Failed) {
                Develop.Message?.Invoke(BlueBasics.Enums.ErrorType.DevelopInfo, null, Develop.MonitorMessage, BlueBasics.Enums.ImageCode.Kritisch, "Skript-Fehler: " + scx.FailedReason, cdf.Stufe);
                return new GetEndFeedback(cdf, $"Durch Befehl abgebrochen: {cdf.NormalizedText} -> {scx.FailedReason}", scx.NeedsScriptFix);
            }

            if (pos == 0 && cdf.NormalizedText.Length == scx.Position) { return new GetEndFeedback(cdf, scx.ReturnValue); }
            if (scx.ReturnValue == null) { return new GetEndFeedback(cdf, "Variablenfehler", true); }
            if (!scx.ReturnValue.ToStringPossible) { return new GetEndFeedback(cdf, "Variable muss als Objekt behandelt werden", true); }

            cdf = new CanDoFeedback(cdf, pos, cdf.NormalizedText.Substring(0, pos) + scx.ReturnValue.ValueForReplace + cdf.NormalizedText.Substring(scx.Position));
        } while (true);
    }

    /// <summary>
    /// Ersetzt eine Variable an Stelle 0, falls dort eine ist.
    /// Gibt dann den ersetzten Text zurück.
    /// Achtung: nur Stringable Variablen werden berücksichtigt.
    /// </summary>
    /// <param name="toParse"></param>
    /// <param name="varCol"></param>
    /// <param name="cdf"></param>
    /// <returns></returns>
    public static GetEndFeedback ReplaceVariable(string toParse, VariableCollection? varCol, CanDoFeedback cdf) {
        if (varCol is not { }) { return new GetEndFeedback(cdf, "Interner Variablen-Fehler", true); }

        var posc = 0;
        var allVarNames = varCol.AllStringableNames();

        do {
            var (pos, which) = NextText(toParse, posc, allVarNames, true, true, KlammernAlle);

            if (pos < 0) { return new GetEndFeedback(cdf, toParse); }

            var thisV = varCol.Get(which);
            var endz = pos + which.Length;

            if (thisV == null) { return new GetEndFeedback(cdf, "Variablen-Fehler " + which, true); }

            toParse = toParse.Substring(0, pos) + thisV.ValueForReplace + toParse.Substring(endz);
            posc = pos;
        } while (true);
    }

    public static SplittedAttributesFeedback SplitAttributeToVars(VariableCollection? varcol, List<List<string>> types, int lastArgMinCount, CanDoFeedback cdf, ScriptProperties? scp) {
        if (types.Count == 0) {
            return string.IsNullOrEmpty(cdf.NormalizedText)
                ? new SplittedAttributesFeedback([])
                : new SplittedAttributesFeedback(ScriptIssueType.AttributAnzahl, "Keine Attribute erwartet, aber erhalten.", true);
        }

        var attributes = SplitAttributeToString(cdf.NormalizedText);
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
                var tmp2 = GetVariableByParsing(attributes[n], cdf, varcol, scp);
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
    /// <param name="cdf"></param>
    /// <param name="scp"></param>
    /// <param name="newcommand">Erwartet wird: X=5;</param>
    /// <param name="generateVariable"></param>
    /// <returns></returns>
    public static DoItFeedback VariablenBerechnung(VariableCollection varCol, CanDoFeedback cdf, ScriptProperties scp, bool generateVariable) {
        var (pos, _) = NextText(cdf.NormalizedText, 0, Gleich, false, false, null);

        if (pos < 1 || pos > cdf.NormalizedText.Length - 2) { return new DoItFeedback("Fehler mit = - Zeichen", true, cdf); }

        var varnam = cdf.NormalizedText.Substring(0, pos);

        if (!Variable.IsValidName(varnam)) { return new DoItFeedback(varnam + " ist kein gültiger Variablen-Name", true, cdf); }

        var vari = varCol.Get(varnam);
        if (generateVariable && vari != null) {
            return new DoItFeedback($"Variable {varnam} ist bereits vorhanden.", true, cdf);
        }
        if (!generateVariable && vari == null) {
            return new DoItFeedback($"Variable {varnam} nicht vorhanden.", true, cdf);
        }

        var value = cdf.NormalizedText.Substring(pos + 1, cdf.NormalizedText.Length - pos - 2);

        List<List<string>> sargs = [[Variable.Any_Plain]];

        var attvar = SplitAttributeToVars(varCol, sargs, 0, cdf, scp);

        if (attvar.Failed) { return new DoItFeedback("Der Wert nach dem '=' konnte nicht berechnet werden: " + attvar.FailedReason, attvar.NeedsScriptFix, cdf); }

        if (attvar.Attributes[0] is VariableUnknown) { return new DoItFeedback("Variable unbekannt", true, cdf); }

        if (attvar.Attributes[0] is { } v) {
            if (generateVariable) {
                v.KeyName = varnam.ToLowerInvariant();
                v.ReadOnly = false;
                _ = varCol.Add(v);
                return new DoItFeedback(v, cdf);
            }

            if (vari == null) {
                // es sollte generateVariable greifen, und hier gar nimmer ankommen. Aber um die IDE zu befriedigen
                return DoItFeedback.InternerFehler(cdf);
            }

            var f = vari.GetValueFrom(v);
            if (!string.IsNullOrEmpty(f)) { return new DoItFeedback(f, true, cdf); }

            return new DoItFeedback(v, new CurrentPosition(cdf, cdf.Position + cdf.NormalizedText.Length));
        }
        // attvar.Attributes[0] müsste immer eine Variable sein...
        return DoItFeedback.InternerFehler(cdf);
    }

    public CanDoFeedback CanDo(bool expectedvariablefeedback, CanDoFeedback cdf) {
        if (!expectedvariablefeedback && !string.IsNullOrEmpty(Returns) && MustUseReturnValue) {
            return new CanDoFeedback(cdf, "Befehl '" + Syntax + "' an dieser Stelle nicht möglich", false);
        }
        if (expectedvariablefeedback && string.IsNullOrEmpty(Returns)) {
            return new CanDoFeedback(cdf, "Befehl '" + Syntax + "' an dieser Stelle nicht möglich", false);
        }
        var maxl = cdf.NormalizedText.Length;

        var commandtext = Command + StartSequence;
        var l = commandtext.Length;
        if (cdf.Position + l < maxl) {
            if (string.Equals(cdf.NormalizedText.Substring(cdf.Position, l), commandtext, StringComparison.OrdinalIgnoreCase)) {
                var f = GetEnd(new CanDoFeedback(cdf, cdf.Position + Command.Length, cdf.NormalizedText), StartSequence.Length, EndSequence);
                if (f.Failed) {
                    return new CanDoFeedback(f, "Fehler bei " + commandtext, true);
                }
                var cont = f.Position;
                var codebltxt = string.Empty;
                if (GetCodeBlockAfter) {
                    var (codeblock, errorreason) = GetCodeBlockText(cdf.NormalizedText, cont);
                    if (!string.IsNullOrEmpty(errorreason)) { return new CanDoFeedback(cdf, errorreason, true); }
                    codebltxt = codeblock;
                    cont = cont + codebltxt.Length + 2;
                }

                return new CanDoFeedback(cdf, cont, f.NormalizedText, codebltxt);
            }
        }

        return new CanDoFeedback(cdf, "Kann nicht geparst werden", false);
    }

    public virtual DoItFeedback DoIt(VariableCollection varCol, CanDoFeedback cdf, ScriptProperties scp) {
        var attvar = SplitAttributeToVars(varCol, Args, LastArgMinCount, cdf, scp);
        return attvar.Failed
            ? DoItFeedback.AttributFehler(cdf, this, attvar)
            : DoIt(varCol, attvar, scp, cdf);
    }

    public abstract DoItFeedback DoIt(VariableCollection varCol, SplittedAttributesFeedback attvar, ScriptProperties scp, CanDoFeedback cdf);

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

    private static List<string>? SplitAttributeToString(string AttributeText) {
        if (string.IsNullOrEmpty(AttributeText)) { return null; }
        List<string> attributes = [];

        #region Liste der Attribute splitten

        var posc = 0;
        do {
            var (pos, _) = NextText(AttributeText, posc, Komma, false, false, KlammernAlle);
            if (pos < 0) {
                attributes.Add(AttributeText.Substring(posc).Trim(KlammernRund));
                break;
            }
            attributes.Add(AttributeText.Substring(posc, pos - posc).Trim(KlammernRund));
            posc = pos + 1;
        } while (true);

        #endregion Liste der Attribute splitten

        return attributes;
    }

    private static (bool cando, bool result) TryParse(string txt, VariableCollection varCol, ScriptProperties scp) {
        if (VariableBool.TryParseValue<VariableBool>(txt, out var result) && result is bool b) { return (true, b); }

        #region Auf Restliche Boolsche Operationen testen

        //foreach (var check in Method_if.VergleichsOperatoren) {
        var (i, check) = NextText(txt, 0, Method_If.VergleichsOperatoren, false, false, KlammernAlle);
        if (i > -1) {
            if (i < 1 && check != "!") {
                return (false, false);//new DoItFeedback(infos.LogData, s, "Operator (" + check + ") am String-Start nicht erlaubt: " + txt);
            } // <1, weil ja mindestens ein Zeichen vorher sein MUSS!

            if (i >= txt.Length - 1) {
                return (false, false);//new DoItFeedback(infos.LogData, s, "Operator (" + check + ") am String-Ende nicht erlaubt: " + txt);
            } // siehe oben

            #region Die Werte vor und nach dem Trennzeichen in den Variablen v1 und v2 ablegen

            #region Ersten Wert als s1 ermitteln

            var s1 = txt.Substring(0, i);
            Variable? v1 = null;
            if (!string.IsNullOrEmpty(s1)) {
                var tmp1 = GetVariableByParsing(s1, null, varCol, scp);
                if (tmp1.Failed) { return (false, false); }//new DoItFeedback(infos.LogData, s, "Befehls-Berechnungsfehler in ():" + tmp1.ErrorMessage);

                v1 = tmp1.ReturnValue;
            } else {
                if (check != "!") { return (false, false); }//new DoItFeedback(infos.LogData, s, "Wert vor Operator (" + check + ") nicht gefunden: " + txt);
            }

            #endregion

            #region Zweiten Wert als s2 ermitteln

            var s2 = txt.Substring(i + check.Length);
            if (string.IsNullOrEmpty(s2)) { return (false, false); }//new DoItFeedback(infos.LogData, s, "Wert nach Operator (" + check + ") nicht gefunden: " + txt);

            var tmp2 = GetVariableByParsing(s2, null, varCol, scp);
            if (tmp2.Failed) {
                return (false, false);//new DoItFeedback(infos.LogData, s, "Befehls-Berechnungsfehler in ():" + tmp1.ErrorMessage);
            }

            var v2 = tmp2.ReturnValue;

            #endregion

            // V2 braucht nicht peprüft werden, muss ja eh der gleiche TYpe wie V1 sein
            if (v1 != null) {
                if (v1.MyClassId != v2?.MyClassId) { return (false, false); }// return new DoItFeedback(infos.LogData, s, "Typen unterschiedlich: " + txt);

                if (!v1.ToStringPossible) { return (false, false); }// return new DoItFeedback(infos.LogData, s, "Datentyp nicht zum Vergleichen geeignet: " + txt);
            } else {
                if (v2 is not VariableBool) { return (false, false); }// return new DoItFeedback(infos.LogData, s, "Datentyp nicht zum Vergleichen geeignet: " + txt);
            }

            #endregion

            switch (check) {
                case "==": {
                        if (v1 == null) { return (false, false); }
                        return (true, v1.ValueForReplace == v2.ValueForReplace);
                    }

                case "!=": {
                        if (v1 == null) { return (false, false); }
                        return (true, v1.ValueForReplace != v2.ValueForReplace);
                    }

                case ">=": {
                        if (v1 is not VariableDouble v1Fl) { return (false, false); } //  return new DoItFeedback(infos.LogData, s, "Datentyp nicht zum Vergleichen geeignet: " + txt);
                        if (v2 is not VariableDouble v2Fl) { return (false, false); }  //  return new DoItFeedback(infos.LogData, s, "Datentyp nicht zum Vergleichen geeignet: " + txt);
                        return (true, v1Fl.ValueNum >= v2Fl.ValueNum);
                    }

                case "<=": {
                        if (v1 is not VariableDouble v1Fl) { return (false, false); } //  return new DoItFeedback(infos.LogData, s, "Datentyp nicht zum Vergleichen geeignet: " + txt);
                        if (v2 is not VariableDouble v2Fl) { return (false, false); }  //  return new DoItFeedback(infos.LogData, s, "Datentyp nicht zum Vergleichen geeignet: " + txt);
                        return (true, v1Fl.ValueNum <= v2Fl.ValueNum);
                    }

                case "<": {
                        if (v1 is not VariableDouble v1Fl) { return (false, false); } //  return new DoItFeedback(infos.LogData, s, "Datentyp nicht zum Vergleichen geeignet: " + txt);
                        if (v2 is not VariableDouble v2Fl) { return (false, false); }  //  return new DoItFeedback(infos.LogData, s, "Datentyp nicht zum Vergleichen geeignet: " + txt);
                        return (true, v1Fl.ValueNum < v2Fl.ValueNum);
                    }

                case ">": {
                        if (v1 is not VariableDouble v1Fl) { return (false, false); } //  return new DoItFeedback(infos.LogData, s, "Datentyp nicht zum Vergleichen geeignet: " + txt);
                        if (v2 is not VariableDouble v2Fl) { return (false, false); }  //  return new DoItFeedback(infos.LogData, s, "Datentyp nicht zum Vergleichen geeignet: " + txt);
                        return (true, v1Fl.ValueNum > v2Fl.ValueNum);
                    }

                case "||": {
                        if (v1 is not VariableBool v1Bo) { return (false, false); }                            // return new DoItFeedback(infos.LogData, s, "Datentyp nicht zum Vergleichen geeignet: " + txt);
                        if (v2 is not VariableBool v2Bo) { return (false, false); }// return new DoItFeedback(infos.LogData, s, "Datentyp nicht zum Vergleichen geeignet: " + txt);
                        return (true, v1Bo.ValueBool || v2Bo.ValueBool);
                    }

                case "&&": {
                        if (v1 is not VariableBool v1Bo) { return (false, false); }  // return new DoItFeedback(infos.LogData, s, "Datentyp nicht zum Vergleichen geeignet: " + txt);
                        if (v2 is not VariableBool v2Bo) { return (false, false); }                                // return new DoItFeedback(infos.LogData, s, "Datentyp nicht zum Vergleichen geeignet: " + txt);
                        return (true, v1Bo.ValueBool && v2Bo.ValueBool);
                    }

                case "!": {
                        // S1 dürfte eigentlich nie was sein: !False||!false
                        // entweder ist es ganz am anfang, oder direkt nach einem Trenneichen
                        if (v2 is not VariableBool v2Bo) { return (false, false); }   // return new DoItFeedback(infos.LogData, s, "Datentyp nicht zum Vergleichen geeignet: " + txt);
                        return (true, !v2Bo.ValueBool);
                    }
            }
        }

        #endregion

        return (false, false);
    }

    #endregion
}