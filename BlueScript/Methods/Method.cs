// Authors:
// Christian Peter
//
// Copyright © 2026 Christian Peter
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
using BlueBasics.ClassesStatic;
using BlueScript.Classes;
using BlueScript.Enums;
using BlueScript.Variables;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Reflection;
using static BlueBasics.ClassesStatic.Constants;
using static BlueBasics.Extensions;

namespace BlueScript.Methods;

public abstract class Method {

    #region Fields

    public static readonly List<string> BoolVal = [VariableBool.ShortName_Plain];
    public static readonly List<string> FloatVal = [VariableDouble.ShortName_Plain];
    public static readonly List<string> ListStringVar = [VariableListString.ShortName_Variable];
    public static readonly List<string> StringVal = [VariableString.ShortName_Plain];
    public static readonly List<string> StringVar = [VariableString.ShortName_Variable];

    private static readonly ConcurrentDictionary<Type, List<string>> _usesInDb = new();

    #endregion

    #region Properties

    public static List<Type> AllMethods {
        get {
            field ??= [.. Generic.GetEnumerableOfType<Method>()];
            return field;
        }
    }

    public static List<List<string>> Args => throw new NotImplementedException();

    public static string Command => throw new NotImplementedException();

    public static List<string> Constants => throw new NotImplementedException();

    public static string Description => throw new NotImplementedException();

    public static bool GetCodeBlockAfter => false;

    /// <summary>
    /// Gibt an, ob und wie oft das letzte Argument wiederholt werden kann bzw. muss.
    ///  -1 = das letzte Argument muss genau 1x vorhanden sein.
    ///   0 = das letzte Argument darf fehlen oder öfters vorhanden sein
    ///   1 = das letzte Argument darf öfters vorhanden sein
    /// > 2 = das letzte Argument muss mindestes so oft vorhanden sein.
    /// </summary>
    public static int LastArgMinCount => throw new NotImplementedException();

    public static MethodType MethodLevel => MethodType.Standard;

    public static bool MustUseReturnValue => false;

    public static string Returns => throw new NotImplementedException();

    public static string StartSequence => throw new NotImplementedException();

    public static string Syntax => throw new NotImplementedException();

    #endregion

    #region Methods

    public static void AddUseInDb(Type methodType, string value) {
        _usesInDb.TryAdd(methodType, []);
        _usesInDb[methodType].AddIfNotExists(value);
    }

    public static CanDoFeedback CanDo(Type methodType, string scriptText, int pos, bool expectedvariablefeedback, LogData? ld) {
        var returns = GetReturns(methodType);
        var mustUseReturnValue = GetMustUseReturnValue(methodType);
        var syntax = GetSyntax(methodType);
        var command = GetCommand(methodType);
        var startSequence = GetStartSequence(methodType);
        var getCodeBlockAfter = GetGetCodeBlockAfter(methodType);
        var endSequence = EndSequence(methodType);

        if (!expectedvariablefeedback && !string.IsNullOrEmpty(returns) && mustUseReturnValue) {
            return new CanDoFeedback(pos, "Befehl '" + syntax + "' an dieser Stelle nicht möglich", false, ld);
        }
        if (expectedvariablefeedback && string.IsNullOrEmpty(returns)) {
            return new CanDoFeedback(pos, "Befehl '" + syntax + "' an dieser Stelle nicht möglich", false, ld);
        }
        var maxl = scriptText.Length;

        var commandtext = command + startSequence;
        var l = commandtext.Length;
        if (pos + l < maxl) {
            if (scriptText.AsSpan(pos, l).Equals(commandtext.AsSpan(), StringComparison.OrdinalIgnoreCase)) {
                var f = GetEnd(scriptText, pos + command.Length, startSequence.Length, endSequence, ld);
                if (f.Failed) {
                    return new CanDoFeedback(f.ContinuePosition, "Fehler bei " + commandtext, true, ld);
                }
                var cont = f.ContinuePosition;
                var codebltxt = string.Empty;
                if (getCodeBlockAfter) {
                    var (codeblock, errorreason) = GetCodeBlockText(scriptText, cont);
                    if (!string.IsNullOrEmpty(errorreason)) { return new CanDoFeedback(f.ContinuePosition, errorreason, true, ld); }
                    codebltxt = codeblock;
                    cont = cont + codebltxt.Length + 2;
                }

                return new CanDoFeedback(cont, f.NormalizedText, codebltxt, ld);
            }
        }

        return new CanDoFeedback(pos, "Kann nicht geparst werden", false, ld);
    }

    public static DoItFeedback DoIt(Type methodType, VariableCollection varCol, CanDoFeedback infos, ScriptProperties scp) {
        try {
            var command = GetCommand(methodType);
            var args = GetArgs(methodType);
            var lastArgMinCount = GetLastArgMinCount(methodType);
            var attvar = SplitAttributeToVars(command, varCol, infos.AttributText, args, lastArgMinCount, infos.LogData, scp);
            return attvar.Failed
                ? DoItFeedback.AttributFehler(infos.LogData, attvar)
                : DoItSplitted(methodType, varCol, attvar, scp, infos.LogData);
        } catch (Exception ex) {
            return new DoItFeedback("Interner Programmfehler: " + ex.Message, false, infos.LogData);
        }
    }

    public static DoItFeedback DoItSplitted(Type methodType, VariableCollection varCol, SplittedAttributesFeedback attvar, ScriptProperties scp, LogData ld) {
        var method = methodType.GetMethod(nameof(DoItSplitted), [typeof(VariableCollection), typeof(SplittedAttributesFeedback), typeof(ScriptProperties), typeof(LogData)]);
        if (method is null) { return DoItFeedback.InternerFehler(ld); }
        return (DoItFeedback?)method.Invoke(null, [varCol, attvar, scp, ld]) ?? DoItFeedback.InternerFehler(ld);
    }

    public static DoItFeedback DoItVirtual(Type methodType, VariableCollection varCol, CanDoFeedback infos, ScriptProperties scp) {
        var method = methodType.GetMethod(nameof(DoItVirtual), [typeof(VariableCollection), typeof(CanDoFeedback), typeof(ScriptProperties)]);
        if (method is null) {
            return DoIt(methodType, varCol, infos, scp);
        }
        return (DoItFeedback?)method.Invoke(null, [varCol, infos, scp]) ?? DoItFeedback.InternerFehler(infos.LogData);
    }

    public static string EndSequence(Type methodType) {
        var ss = GetStartSequence(methodType);
        var ret = GetReturns(methodType);
        var gcba = GetGetCodeBlockAfter(methodType);

        if (ss == "(") {
            if (!string.IsNullOrEmpty(ret)) { return ")"; }
            if (gcba) { return ")"; }
        }
        if (gcba) { return string.Empty; }
        if (ss == "(") { return ");"; }

        return ";";
    }

    public static List<List<string>> GetArgs(Type methodType) {
        var prop = methodType.GetProperty("Args", BindingFlags.Public | BindingFlags.Static);
        return (List<List<string>>?)prop?.GetValue(null) ?? [];
    }

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
            if (scriptText[tmp] == '{') { break; }
            if (scriptText[tmp] != '¶') { return (string.Empty, "Keinen nachfolgenden Codeblock gefunden."); }
            tmp++;
        } while (true);

        var (posek, _) = NextText(scriptText, start, KlammerGeschweiftZu, false, false, KlammernAlle);
        if (posek < start) {
            return (string.Empty, "Kein Codeblock Ende gefunden.");
        }

        var s = scriptText[start..tmp] + scriptText[(tmp + 1)..posek];

        return (s, string.Empty);
    }

    public static string GetCommand(Type methodType) {
        var prop = methodType.GetProperty("Command", BindingFlags.Public | BindingFlags.Static);
        return (string?)prop?.GetValue(null) ?? string.Empty;
    }

    public static List<string> GetConstants(Type methodType) {
        var prop = methodType.GetProperty("Constants", BindingFlags.Public | BindingFlags.Static);
        return (List<string>?)prop?.GetValue(null) ?? [];
    }

    public static string GetDescription(Type methodType) {
        var prop = methodType.GetProperty("Description", BindingFlags.Public | BindingFlags.Static);
        return (string?)prop?.GetValue(null) ?? string.Empty;
    }

    public static GetEndFeedback GetEnd(string scriptText, int startpos, int lengthStartSequence, string endSequence, LogData? ld) {
        if (string.IsNullOrEmpty(endSequence)) {
            return new GetEndFeedback(startpos, string.Empty);
        }

        var (pos, which) = NextText(scriptText, startpos, [endSequence], false, false, KlammernAlle);
        if (pos < startpos) {
            return new GetEndFeedback("Endpunkt '" + endSequence + "' nicht gefunden.", true, ld);
        }

        var txtBtw = scriptText[(startpos + lengthStartSequence)..pos];
        return new GetEndFeedback(pos + which.Length, txtBtw);
    }

    public static bool GetGetCodeBlockAfter(Type methodType) {
        var prop = methodType.GetProperty("GetCodeBlockAfter", BindingFlags.Public | BindingFlags.Static);
        return (bool?)prop?.GetValue(null) ?? false;
    }

    public static string GetHintText(Type methodType) {
        var syntax = GetSyntax(methodType);
        var args = GetArgs(methodType);
        var lastArgMinCount = GetLastArgMinCount(methodType);
        var returns = GetReturns(methodType);
        var mustUseReturnValue = GetMustUseReturnValue(methodType);
        var description = GetDescription(methodType);
        var constants = GetConstants(methodType);
        var uses = GetUsesInDb(methodType);

        var co = "Syntax:\r\n";
        co += "~~~~~~\r\n";
        co = co + syntax + "\r\n";
        co += "\r\n";
        co += "Argumente:\r\n";
        co += "~~~~~~~~~~\r\n";
        for (var z = 0; z < args.Count; z++) {
            var a = string.Join(", ", args[z]);
            if (a.Contains('*')) {
                a = a.Replace("*", string.Empty) + " (muss eine vorhandene Variable sein)";
            }

            co = co + "  - Argument " + (z + 1) + ": " + a;

            if (z == args.Count - 1) {
                switch (lastArgMinCount) {
                    case -1:
                        break;

                    case 0:
                        co += " (darf fehlen; darf mehrfach wiederholt werden)";
                        break;

                    case 1:
                        co += " (muss angegeben werden; darf mehrfach wiederholt werden)";
                        break;

                    default:

                        co += " (muss mindestens " + lastArgMinCount + "x wiederholt werden)";
                        break;
                }
            }
            co += "\r\n";
        }
        co += "\r\n";
        co += "Rückgabe:\r\n";
        co += "~~~~~~~~\r\n";
        if (string.IsNullOrEmpty(returns)) {
            co += "  - Rückgabetyp: -\r\n";
        } else {
            co = mustUseReturnValue
                ? co + "  - Rückgabetyp: " + returns + "(muss verwendet werden)\r\n"
                : co + "  - Rückgabetyp: " + returns + " (darf verworfen werden)\r\n";
        }

        co += "\r\n";
        co += "Beschreibung:\r\n";
        co += "~~~~~~~~~~~\r\n";
        co = co + description + "\r\n";

        if (constants.Count > 0) {
            co += "\r\n";
            co += "Konstanten:\r\n";
            co += "~~~~~~~~~~~~\r\n";
            co += string.Join('\r', constants) + "\r\n";
        }

        if (uses.Count > 0) {
            co += "\r\n";
            co += "Aktuelle Verwendung in TABELLEN-Skripten:\r\n";
            co += "~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~\r\n";
            co += string.Join('\r', uses);
        }

        return co;
    }

    public static int GetLastArgMinCount(Type methodType) {
        var prop = methodType.GetProperty("LastArgMinCount", BindingFlags.Public | BindingFlags.Static);
        return (int?)prop?.GetValue(null) ?? -1;
    }

    public static MethodType GetMethodLevel(Type methodType) {
        var prop = methodType.GetProperty("MethodLevel", BindingFlags.Public | BindingFlags.Static);
        var val = prop?.GetValue(null);
        return val is MethodType mt ? mt : MethodType.Standard;
    }

    public static List<Type> GetMethods(MethodType maxLevel) {
        var m = new List<Type>();

        foreach (var thism in AllMethods) {
            if (GetMethodLevel(thism) <= maxLevel) {
                m.Add(thism);
            }
        }

        return m;
    }

    public static bool GetMustUseReturnValue(Type methodType) {
        var prop = methodType.GetProperty("MustUseReturnValue", BindingFlags.Public | BindingFlags.Static);
        return (bool?)prop?.GetValue(null) ?? false;
    }

    public static string GetReturns(Type methodType) {
        var prop = methodType.GetProperty("Returns", BindingFlags.Public | BindingFlags.Static);
        return (string?)prop?.GetValue(null) ?? string.Empty;
    }

    public static string GetStartSequence(Type methodType) {
        var prop = methodType.GetProperty("StartSequence", BindingFlags.Public | BindingFlags.Static);
        return (string?)prop?.GetValue(null) ?? string.Empty;
    }

    public static string GetSyntax(Type methodType) {
        var prop = methodType.GetProperty("Syntax", BindingFlags.Public | BindingFlags.Static);
        return (string?)prop?.GetValue(null) ?? string.Empty;
    }

    public static List<string> GetUsesInDb(Type methodType) => _usesInDb.GetValueOrDefault(methodType, []);

    public static DoItFeedback GetVariableByParsing(string txt, LogData ld, VariableCollection varCol, ScriptProperties scp) {
        if (string.IsNullOrEmpty(txt)) { return new DoItFeedback("Kein Wert zum Parsen angekommen.", true, ld); }

        if (txt.StartsWith('(')) {
            var (pose, _) = NextText(txt, 0, KlammerRundZu, false, false, KlammernAlle);
            if (pose < txt.Length - 1 && pose > 0) {
                var scx = GetVariableByParsing(txt[1..pose], ld, varCol, scp);
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
                return GetVariableByParsing(scx.ReturnValue.ValueForReplace + txt[(pose + 1)..], ld, varCol, scp);
            }
        }

        if (txt.StartsWith('[')) {
            var (pose, _) = NextText(txt, 0, KlammerEckigZu, false, false, KlammernAlle);
            if (pose == txt.Length - 1) {
                var tl = txt[1..pose];

                if (!string.IsNullOrWhiteSpace(tl)) {
                    var l = SplitAttributeToVars("?", varCol, tl, [[VariableString.ShortName_Plain]], 1, ld, scp);
                    if (l.Failed) {
                        return new DoItFeedback(l.FailedReason, l.NeedsScriptFix, ld);
                    }
                    txt = "[\"" + string.Join("\",\"", l.Attributes.AllStringValues()) + "\"]";
                }
            }
        }

        txt = txt.Trim(KlammernRund);

        var (uu, _) = NextText(txt, 0, Method_If.UndUnd, false, false, KlammernAlle);
        if (uu > 0) {
            var scx = GetVariableByParsing(txt[..uu], ld, varCol, scp);
            if (scx.Failed || scx.ReturnValue is null or VariableUnknown) {
                scx.ChangeFailedReason($"Befehls-Berechnungsfehler vor &&: {txt[..uu]}", true, ld);
                return scx;
            }

            if (scx.ReturnValue is VariableBool { ValueBool: false }) { return scx; }
            return GetVariableByParsing(txt[(uu + 2)..], ld, varCol, scp);
        }

        var (oo, _) = NextText(txt, 0, Method_If.OderOder, false, false, KlammernAlle);
        if (oo > 0) {
            var txt1 = GetVariableByParsing(txt[..oo], ld, varCol, scp);
            if (txt1.Failed || txt1.ReturnValue is null or VariableUnknown) {
                return new DoItFeedback("Befehls-Berechnungsfehler vor ||", txt1.NeedsScriptFix, ld);
            }

            if (txt1.ReturnValue is VariableBool { ValueBool: true }) { return txt1; }
            return GetVariableByParsing(txt[(oo + 2)..], ld, varCol, scp);
        }

        var t = ReplaceVariable(txt, varCol, ld);
        if (t.Failed) { return new DoItFeedback("Variablen-Berechnungsfehler", t.NeedsScriptFix, ld); }
        if (t.ReturnValue != null) { return new DoItFeedback(t.ReturnValue); }
        if (txt != t.NormalizedText) { return GetVariableByParsing(t.NormalizedText, ld, varCol, scp); }

        var t2 = ReplaceCommandsAndVars(txt, varCol, ld, scp);
        if (t2.Failed) { return new DoItFeedback(t2.FailedReason, t2.NeedsScriptFix, ld); }
        if (t2.ReturnValue != null) { return new DoItFeedback(t2.ReturnValue); }
        if (txt != t2.NormalizedText) { return GetVariableByParsing(t2.NormalizedText, ld, varCol, scp); }

        if (ParseOperators(txt, varCol, scp, ld) is { } b) { return new DoItFeedback(b); }

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
            var ret = GetReturns(thisc);
            if (!string.IsNullOrEmpty(ret)) {
                toSearch.Add(GetCommand(thisc) + GetStartSequence(thisc));
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
                Develop.Message(BlueBasics.Enums.ErrorType.DevelopInfo, null, Develop.MonitorMessage, BlueBasics.Enums.ImageCode.Kritisch, "Skript-Fehler: " + scx.FailedReason, scp.Stufe);
                return new GetEndFeedback(scx.FailedReason, scx.NeedsScriptFix, ld);
            }

            if (pos == 0 && txt.Length == scx.Position) { return new GetEndFeedback(scx.ReturnValue); }
            if (scx.ReturnValue == null) { return new GetEndFeedback("Variablenfehler", true, ld); }
            if (!scx.ReturnValue.ToStringPossible) { return new GetEndFeedback("Variable muss als Objekt behandelt werden", true, ld); }

            txt = string.Concat(txt.AsSpan(0, pos), scx.ReturnValue.ValueForReplace, txt.AsSpan(scx.Position));
            posc = pos;
        } while (true);
    }

    /// <summary>
    /// Ersetzt eine Variable an Stelle 0, falls dort eine ist.
    /// Gibt dann den ersetzten Text zurück.
    /// Achtung: nur Stringable Variablen werden berücksichtigt.
    /// </summary>
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

            txt = string.Concat(txt.AsSpan(0, pos), thisV.ValueForReplace, txt.AsSpan(endz));
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

        VariableCollection feedbackVariables = [];
        for (var n = 0; n < attributes.Count; n++) {
            attributes[n] = attributes[n].RemoveChars("¶");

            var exceptetType = n < types.Count ? types[n] : types[types.Count - 1];

            Variable? v;

            var mustBeVar = exceptetType.Count > 0 && exceptetType[0].StartsWith('*');

            if (mustBeVar) {
                var varn = attributes[n];
                if (!Variable.IsValidName(varn)) { return new SplittedAttributesFeedback(ScriptIssueType.VariableErwartet, "Variablenname erwartet bei Attribut " + (n + 1), true); }

                v = varcol?.GetByKey(varn);
                if (v == null) { return new SplittedAttributesFeedback(ScriptIssueType.VariableNichtGefunden, "Variable nicht gefunden bei Attribut " + (n + 1), true); }
            } else {
                var tmp2 = GetVariableByParsing(attributes[n], ld, varcol, scp);
                if (tmp2.Failed) { return new SplittedAttributesFeedback(ScriptIssueType.BerechnungFehlgeschlagen, tmp2.FailedReason, tmp2.NeedsScriptFix); }
                if (tmp2.ReturnValue == null) { return new SplittedAttributesFeedback(ScriptIssueType.BerechnungFehlgeschlagen, $"Interner Fehler", true); }

                if (tmp2.ReturnValue is VariableUnknown vukn) {
                    foreach (var thisC in AllMethods) {
                        var f = CanDo(thisC, attributes[n], 0, false, ld);
                        if (string.IsNullOrEmpty(f.FailedReason)) {
                            if (comand.Equals(Method_Var.CommandText, StringComparison.OrdinalIgnoreCase)) {
                                return new SplittedAttributesFeedback(ScriptIssueType.BerechnungFehlgeschlagen, $"Die Variable konnte nicht berechnet werden, dafür verwendete Befehle sind in diesem Skript nicht erlaubt: '{vukn.Value}'", true);
                            }

                            return new SplittedAttributesFeedback(ScriptIssueType.BerechnungFehlgeschlagen, $"Der Befehl '{comand}' kann in diesen Skript nicht verwendet werden.", true);
                        }
                    }
                }

                v = tmp2.ReturnValue;
            }

            var ok = false;

            foreach (var thisAt in exceptetType) {
                if (thisAt.TrimStart('*') == v.MyClassId) { ok = true; break; }
                if (thisAt.TrimStart('*') == Variable.Any_Plain) { ok = true; break; }
            }

            if (!ok) {
                if (v is VariableUnknown ukn) {
                    return new SplittedAttributesFeedback(ScriptIssueType.FalscherDatentyp, $"Bei '{comand}' konnte das Attribut '{n + 1}' nicht aufgelöst werden: {ukn.Value}", true);
                }

                return new SplittedAttributesFeedback(ScriptIssueType.FalscherDatentyp, $"Bei '{comand}' ist das Attribut '{n + 1}' nicht einer der erwarteten Typen '{string.Join("' oder '", exceptetType)}', sondern {v.MyClassId}", true);
            }

            feedbackVariables.Add(v);
        }
        return new SplittedAttributesFeedback(feedbackVariables);
    }

    /// <summary>
    /// Ersetzt eine Variable an Stelle 0, falls dort eine ist.
    /// Gibt dann den ersetzten Text zurück.
    /// Achtung: nur Stringable Variablen werden berücksichtigt.
    /// </summary>
    public static DoItFeedback VariablenBerechnung(VariableCollection varCol, LogData ld, ScriptProperties scp, string newcommand, bool generateVariable) {
        var (pos, _) = NextText(newcommand, 0, Gleich, false, false, null);

        if (pos < 1 || pos > newcommand.Length - 2) { return new DoItFeedback("Fehler mit = - Zeichen", true, ld); }

        var varnam = newcommand[..pos];

        if (!Variable.IsValidName(varnam)) { return new DoItFeedback(varnam + " ist kein gültiger Variablen-Name", true, ld); }

        var vari = varCol.GetByKey(varnam);
        if (generateVariable && vari != null) {
            return new DoItFeedback("Variable " + varnam + " ist bereits vorhanden.", true, ld);
        }
        if (!generateVariable && vari == null) {
            return new DoItFeedback("Variable " + varnam + " nicht vorhanden.", true, ld);
        }

        var value = newcommand[(pos + 1)..^1];

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
                return DoItFeedback.InternerFehler(ld);
            }

            var f = vari.GetValueFrom(v);
            return new DoItFeedback(f, !string.IsNullOrWhiteSpace(f), ld);
        }
        return DoItFeedback.InternerFehler(ld);
    }

    private static bool? ParseOperators(string txt, VariableCollection varCol, ScriptProperties scp, LogData ld) {
        if (Variable.TryParseValue<VariableBool>(txt, out var result) && result is bool b) { return b; }

        #region Auf Restliche Boolsche Operationen testen

        var (i, check) = NextText(txt, 0, Method_If.VergleichsOperatoren, false, false, KlammernAlle);
        if (i > -1) {
            if (i < 1 && check != "!") { return null; }

            if (i >= txt.Length - 1) { return null; }

            #region Die Werte vor und nach dem Trennzeichen in den Variablen v1 und v2 ablegen

            #region Ersten Wert als s1 ermitteln

            var s1 = txt[..i];
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

            var s2 = txt[(i + check.Length)..];
            if (string.IsNullOrEmpty(s2)) { return null; }

            var tmp2 = GetVariableByParsing(s2, ld, varCol, scp);
            if (tmp2.Failed) { return null; }

            var v2 = tmp2.ReturnValue;

            #endregion

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
                attributes.Add(attributtext[posc..].Trim(KlammernRund));
                break;
            }
            attributes.Add(attributtext[posc..pos].Trim(KlammernRund));
            posc = pos + 1;
        } while (true);

        #endregion

        return attributes;
    }

    #endregion
}