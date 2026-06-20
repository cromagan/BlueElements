// Licensed under AGPL-3.0; see License.md for disclaimer and details.

using static BlueBasics.ClassesStatic.Constants;

namespace BlueScript.Methods;

public abstract class Method : IReadableTextWithKey {

    #region Fields

    public static readonly AssemblyAwareCache<Method> AllMethods = new();
    public static readonly List<string> BoolVal = [VariableBool.ShortName_Plain];
    public static readonly List<string> FloatVal = [VariableDouble.ShortName_Plain];
    public static readonly List<string> ListStringVar = [VariableListString.ShortName_Variable];
    public static readonly List<string> StringVal = [VariableString.ShortName_Plain];
    public static readonly List<string> StringVar = [VariableString.ShortName_Variable];

    #endregion

    #region Properties

    public static Dictionary<string, List<Method>> AllMethodByCommand {
        get {
            if (field is not null) { return field; }

            var lookup = new Dictionary<string, List<Method>>(StringComparer.OrdinalIgnoreCase);
            foreach (var m in AllMethods.Instances) {
                if (!lookup.TryGetValue(m.Command, out var list)) {
                    list = [];
                    lookup[m.Command] = list;
                }
                list.Add(m);
            }
            field = lookup;
            return field;
        }
    }

    public virtual List<List<string>> Args => [];
    public abstract string Command { get; }
    public virtual List<string> Constants => [];
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

    public virtual bool GetCodeBlockAfter => false;

    public string KeyName => field ??= Command.ToUpperInvariant();

    /// <summary>
    /// Gibt an, ob und wie oft das letzte Argument wiederholt werden kann bzw. muss.
    /// </summary>
    public virtual LastArgMinCountType LastArgMinCount => LastArgMinCountType.ExactlyOnce;

    public virtual MethodType MethodLevel => MethodType.Standard;

    //TODO: 0 implementieren
    public virtual bool MustUseReturnValue => false;

    public string QuickInfo => HintText();
    public virtual string Returns => string.Empty;

    public virtual string StartSequence => "(";

    public abstract string Syntax { get; }

    public List<string> UsesInDB { get; } = [];

    #endregion

    #region Methods

    /// <summary>
    /// Gibt den Text des Codeblocks zurück. Dabei werden die Zeilenumbrüche vor der { nicht entfernt, aber die Brackets {} selbst schon.
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

        var (posek, _) = NextText(scriptText, start, BracketCurlyClose, false, false, Brackets);
        if (posek < start) {
            return (string.Empty, "Kein Codeblock Ende gefunden.");
        }

        var s = scriptText[start..tmp] + scriptText[(tmp + 1)..posek];

        return (s, string.Empty);
    }

    public static GetEndFeedback GetEnd(string scriptText, int startpos, int lengthStartSequence, string endSequence, LogData? ld) {
        //z.B: beim Befehl DO
        if (string.IsNullOrEmpty(endSequence)) {
            return new GetEndFeedback(startpos, string.Empty);
        }

        var (pos, which) = NextText(scriptText, startpos, [endSequence], false, false, Brackets);
        if (pos < startpos) {
            return new GetEndFeedback("Endpunkt '" + endSequence + "' nicht gefunden.", true, ld);
        }

        var txtBtw = scriptText[(startpos + lengthStartSequence)..pos];
        return new GetEndFeedback(pos + which.Length, txtBtw);
    }

    public static List<Method> GetMethods(MethodType maxLevel) {
        var m = new List<Method>();

        foreach (var thism in AllMethods.Instances) {
            if (thism.MethodLevel <= maxLevel) {
                m.Add(thism);
            }
        }

        return m;
    }

    public static DoItFeedback GetVariableByParsing(string txt, LogData ld, VariableCollection varCol, ScriptProperties scp) {
        if (string.IsNullOrEmpty(txt)) { return new DoItFeedback("Kein Wert zum Parsen angekommen.", true, ld); }

        if (txt.StartsWith('(')) {
            var (pose, _) = NextText(txt, 0, BracketRoundClose, false, false, Brackets);
            if (pose < txt.Length - 1 && pose > 0) {
                // Wir haben so einen Fall: (true) || (true)
                var scx = GetVariableByParsing(txt[1..pose], ld, varCol, scp);
                if (scx.Failed) {
                    scx.ChangeFailedReason("Befehls-Berechnungsfehler in ()", true, ld);
                    return scx;
                }
                if (scx.ReturnValue is null) {
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
            var (pose, _) = NextText(txt, 0, BracketSquareClose, false, false, Brackets);
            if (pose == txt.Length - 1) {
                var tl = txt[1..pose];

                if (!string.IsNullOrWhiteSpace(tl)) {
                    var l = SplitAttributeToVars("?", varCol, tl, [[VariableString.ShortName_Plain]], LastArgMinCountType.MinOnce, ld, scp);
                    if (l.Failed) {
                        return new DoItFeedback(l.FailedReason, l.NeedsScriptFix, ld);
                    }
                    txt = "[\"" + string.Join("\",\"", l.Attributes.OfType<VariableString>().Select(vs => vs.ValueString)) + "\"]";
                }
            }
        }

        txt = txt.Trim(BracketRound);

        var (uu, _) = NextText(txt, 0, Method_If.UndUnd, false, false, Brackets);
        if (uu > 0) {
            var scx = GetVariableByParsing(txt[..uu], ld, varCol, scp);
            if (scx.Failed || scx.ReturnValue is null or VariableUnknown) {
                scx.ChangeFailedReason($"Befehls-Berechnungsfehler vor &&: {txt[..uu]}", true, ld);
                return scx;
            }

            if (scx.ReturnValue is VariableBool { ValueBool: false }) {
                if (scp.SyntaxCheck) {
                    var right = GetVariableByParsing(txt[(uu + 2)..], ld, varCol, scp);
                    if (right.Failed) { return right; }
                }
                return scx;
            }
            return GetVariableByParsing(txt[(uu + 2)..], ld, varCol, scp);
        }

        var (oo, _) = NextText(txt, 0, Method_If.OderOder, false, false, Brackets);
        if (oo > 0) {
            var txt1 = GetVariableByParsing(txt[..oo], ld, varCol, scp);
            if (txt1.Failed || txt1.ReturnValue is null or VariableUnknown) {
                return new DoItFeedback("Befehls-Berechnungsfehler vor ||", txt1.NeedsScriptFix, ld);
            }

            if (txt1.ReturnValue is VariableBool { ValueBool: true }) {
                if (scp.SyntaxCheck) {
                    var right = GetVariableByParsing(txt[(oo + 2)..], ld, varCol, scp);
                    if (right.Failed) { return right; }
                }
                return txt1;
            }
            return GetVariableByParsing(txt[(oo + 2)..], ld, varCol, scp);
        }

        // Variablen nur ersetzen, wenn Variablen auch vorhanden sind.

        var t = ReplaceVariable(txt, varCol, ld);
        if (t.Failed) { return new DoItFeedback("Variablen-Berechnungsfehler", t.NeedsScriptFix, ld); }
        if (t.ReturnValue is not null) { return new DoItFeedback(t.ReturnValue); }
        if (txt != t.NormalizedText) { return GetVariableByParsing(t.NormalizedText, ld, varCol, scp); }

        var t2 = ReplaceCommandsAndVars(txt, varCol, ld, scp);
        if (t2.Failed) { return new DoItFeedback(t2.FailedReason, t2.NeedsScriptFix, ld); }
        if (t2.ReturnValue is not null) { return new DoItFeedback(t2.ReturnValue); }
        if (txt != t2.NormalizedText) { return GetVariableByParsing(t2.NormalizedText, ld, varCol, scp); }

        //var (posa, _) = NextText(txt, 0, ["("], false, false, Brackets);
        //if (posa > -1) {
        //    var (pose, _) = NextText(txt, posa, BracketRoundClose, false, false, Brackets);
        //    if (pose <= posa) { return DoItFeedback.KlammerFehler(ld); }

        //    var tmptxt = txt.Substring(posa + 1, pose - posa - 1);
        //    if (!string.IsNullOrEmpty(tmptxt)) {
        //        var scx = GetVariableByParsing(tmptxt, ld, varCol, scp);
        //        if (scx.Failed) {
        //            scx.ChangeFailedReason("Befehls-Berechnungsfehler in ()", true, ld);
        //            return scx;
        //        }
        //        if (scx.ReturnValue is null) {
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

        //if (VarTypes is null) {
        //    return new DoItFeedback(ld, "Variablentypen nicht initialisiert");
        //}

        foreach (var thisVt in Variable.VarTypes.Instances) {
            if (thisVt.GetFromStringPossible) {
                if (thisVt.TryParse(txt, out var v) && v is not null) {
                    return new DoItFeedback(v);
                }
            }
        }

        return new DoItFeedback("Wert kann nicht geparsed werden: " + txt, true, ld);
    }

    public static GetEndFeedback ReplaceCommandsAndVars(string txt, VariableCollection varCol, LogData? ld, ScriptProperties scp) {

        #region Suchbegriffe zusammenstellen

        var toSearch = new List<string>(scp.MethodsWithReturnSearch);

        foreach (var thisv in varCol) {
            toSearch.Add(thisv.KeyName + "=");
        }

        #endregion

        var posc = 0;
        do {
            var (pos, _) = NextText(txt, posc, toSearch, true, false, Brackets);
            if (pos < 0) { return new GetEndFeedback(0, txt); }

            var scx = Script.CommandOrVarOnPosition(varCol, scp, txt, pos, true, ld);
            if (scx.Failed) {
                Develop.Message(BlueBasics.Enums.ErrorType.DevelopInfo, null, Develop.MonitorMessage, BlueBasics.Enums.ImageCode.Kritisch, "Skript-Fehler: " + scx.FailedReason, scp.Stufe);
                return new GetEndFeedback(scx.FailedReason, scx.NeedsScriptFix, ld);
            }

            if (pos == 0 && txt.Length == scx.Position) { return new GetEndFeedback(scx.ReturnValue); }
            if (scx.ReturnValue is null) { return new GetEndFeedback("Variablenfehler", true, ld); }
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
    /// <param name="txt"></param>
    /// <param name="varCol"></param>
    /// <param name="ld"></param>
    /// <returns></returns>
    public static GetEndFeedback ReplaceVariable(string txt, VariableCollection? varCol, LogData? ld) {
        if (varCol is null) { return new GetEndFeedback("Interner Variablen-Fehler", true, ld); }

        var posc = 0;
        var allVarNames = varCol.AllStringableNames();

        do {
            var (pos, which) = NextText(txt, posc, allVarNames, true, true, Brackets);

            if (pos < 0) { return new GetEndFeedback(0, txt); }

            var thisV = varCol.GetByKey(which);
            var endz = pos + which.Length;

            if (thisV is null) { return new GetEndFeedback("Variablen-Fehler " + which, true, ld); }

            txt = string.Concat(txt.AsSpan(0, pos), thisV.ValueForReplace, txt.AsSpan(endz));
            posc = pos;
        } while (true);
    }

    public static SplittedAttributesFeedback SplitAttributeToVars(string comand, VariableCollection? varcol, string attributText, List<List<string>> types, LastArgMinCountType lastArgMinCount, LogData? ld, ScriptProperties? scp) {
        if (types.Count == 0) {
            return string.IsNullOrEmpty(attributText)
                ? new SplittedAttributesFeedback([])
                : new SplittedAttributesFeedback(ScriptIssueType.AttributAnzahl, "Keine Attribute erwartet, aber erhalten.", true);
        }

        var attributes = SplitAttributeToString(attributText);
        if (attributes is not { Count: not 0 }) {
            var syntax = AllMethods.Instances.FirstOrDefault(m => m.Command.Equals(comand, StringComparison.OrdinalIgnoreCase))?.Syntax ?? string.Empty;
            return new SplittedAttributesFeedback(ScriptIssueType.AttributAnzahl, $"Bei '{comand}' wurden keine Attribute übergeben, erwartet wurden {types.Count}. Beispiel: {syntax}", true);
        }
        if (attributes.Count < types.Count && lastArgMinCount != LastArgMinCountType.Optional) { return new SplittedAttributesFeedback(ScriptIssueType.AttributAnzahl, $"Zu wenige Attribute bei '{comand}' erhalten.", true); }
        if (attributes.Count < types.Count - 1) { return new SplittedAttributesFeedback(ScriptIssueType.AttributAnzahl, $"Zu wenige Attribute bei '{comand}' erhalten.", true); }
        if (lastArgMinCount == LastArgMinCountType.ExactlyOnce && attributes.Count > types.Count) { return new SplittedAttributesFeedback(ScriptIssueType.AttributAnzahl, $"Zu viele Attribute bei '{comand}' erhalten.", true); }
        if (lastArgMinCount == LastArgMinCountType.MinOnce && attributes.Count < types.Count) { return new SplittedAttributesFeedback(ScriptIssueType.AttributAnzahl, $"Zu wenige Attribute bei '{comand}' erhalten.", true); }
        if (lastArgMinCount == LastArgMinCountType.MinTwice && attributes.Count < types.Count + 1) { return new SplittedAttributesFeedback(ScriptIssueType.AttributAnzahl, $"Zu wenige Attribute bei '{comand}' erhalten.", true); }

        //  Variablen und Routinen ersetzen
        List<Variable> feedbackVariables = [];
        for (var n = 0; n < attributes.Count; n++) {
            //var lb = attributes[n].Count(c => c == '¶'); // Zeilenzähler weitersetzen
            attributes[n] = attributes[n].RemoveChars("¶"); // Zeilenzähler entfernen

            var exceptetType = n < types.Count ? types[n] : types[types.Count - 1]; // Bei Endlessargs den letzten nehmen

            // Variable ermitteln oder eine Dummy-Variable als Rückgabe ermitteln
            Variable? v;

            var mustBeVar = exceptetType.Count > 0 && exceptetType[0].StartsWith('*');

            if (mustBeVar) {
                var varn = attributes[n];
                if (!Variable.IsValidName(varn)) { return new SplittedAttributesFeedback(ScriptIssueType.VariableErwartet, "Variablenname erwartet bei Attribut " + (n + 1), true); }

                v = varcol?.GetByKey(varn);
                if (v is null) { return new SplittedAttributesFeedback(ScriptIssueType.VariableNichtGefunden, "Variable nicht gefunden bei Attribut " + (n + 1), true); }
            } else {
                if (ld is null || varcol is null || scp is null) {
                    return new SplittedAttributesFeedback(ScriptIssueType.BerechnungFehlgeschlagen, "Interner Fehler: Null-Parameter", true);
                }
                var tmp2 = GetVariableByParsing(attributes[n], ld, varcol, scp);
                if (tmp2.Failed) { return new SplittedAttributesFeedback(ScriptIssueType.BerechnungFehlgeschlagen, tmp2.FailedReason, tmp2.NeedsScriptFix); }
                if (tmp2.ReturnValue is null) { return new SplittedAttributesFeedback(ScriptIssueType.BerechnungFehlgeschlagen, $"Interner Fehler", true); }

                if (tmp2.ReturnValue is VariableUnknown vukn) {
                    foreach (var thisC in AllMethods.Instances) {
                        var f = thisC.CanDo(attributes[n], 0, false, ld);
                        if (string.IsNullOrEmpty(f.FailedReason)) {
                            if (comand.Equals(Method_Var.CommandText, StringComparison.OrdinalIgnoreCase)) {
                                return new SplittedAttributesFeedback(ScriptIssueType.BerechnungFehlgeschlagen, $"Die Variable konnte nicht berechnet werden, dafür verwendte Befehle sind in diesem Skript nicht erlaubt: '{vukn.Value}'", true);
                            }

                            return new SplittedAttributesFeedback(ScriptIssueType.BerechnungFehlgeschlagen, $"Der Befehl '{comand}' kann in diesen Skript nicht verwendet werden.", true);
                        }
                    }
                }

                v = tmp2.ReturnValue;
            }

            // Den Typ der Variable checken
            var ok = false;

            foreach (var thisAt in exceptetType) {
                if (thisAt.TrimStart('*') == v.MyClassId) { ok = true; break; }
                if (thisAt.TrimStart('*') == Variable.Any_Plain) { ok = true; break; }
                // Während des SyntaxChecks gilt eine VariableUnknown (Dummy) als Wildcard für jeden Typ,
                // damit Bodies validierbar bleiben, die über exists()/isNullOrEmpty()/isNullOrZero() abgesichert sind.
                if (scp is { SyntaxCheck: true } && v is VariableUnknown) { ok = true; break; }
            }

            if (!ok) {
                if (v is VariableUnknown ukn) {
                    return new SplittedAttributesFeedback(ScriptIssueType.FalscherDatentyp, $"Bei '{comand}' konnte das Attribut '{n + 1}' nicht aufgelöst werden: {ukn.Value}", true);
                }

                return new SplittedAttributesFeedback(ScriptIssueType.FalscherDatentyp, $"Bei '{comand}' ist das Attribut '{n + 1}' nicht einer der erwarteten Typen '{string.Join("' oder '", exceptetType)}', sondern {v.MyClassId}", true);
            }

            feedbackVariables.Add(v);

            //if (s is not null) { line += lb; }
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
        var (pos, _) = NextText(newcommand, 0, EqualsSign, false, false, null);

        if (pos < 1 || pos > newcommand.Length - 2) { return new DoItFeedback("Fehler mit = - Zeichen", true, ld); }

        var varnam = newcommand[..pos];

        if (!Variable.IsValidName(varnam)) { return new DoItFeedback(varnam + " ist kein gültiger Variablen-Name", true, ld); }

        var vari = varCol.GetByKey(varnam);
        if (generateVariable && vari is not null) {
            // Während des SyntaxChecks darf ein Dummy (VariableUnknown, registriert z. B. durch exists())
            // überschrieben werden, damit nachfolgendes `var x = ...;` keine False-Positive auslst.
            if (!scp.SyntaxCheck || vari is not VariableUnknown) {
                return new DoItFeedback("Variable " + varnam + " ist bereits vorhanden.", true, ld);
            }
            varCol.Remove(vari.KeyName);
            vari = null;
        }
        if (!generateVariable && vari is null) {
            return new DoItFeedback("Variable " + varnam + " nicht vorhanden.", true, ld);
        }

        var value = newcommand[(pos + 1)..^1];

        List<List<string>> sargs = [[Variable.Any_Plain]];

        var attvar = SplitAttributeToVars("var", varCol, value, sargs, 0, ld, scp);

        if (attvar.Failed) { return new DoItFeedback(attvar.FailedReason, attvar.NeedsScriptFix, ld); }

        if (attvar.Attributes[0] is VariableUnknown) { return new DoItFeedback("Variable unbekannt", true, ld); }

        if (attvar.Attributes[0] is { } v) {
            if (generateVariable) {
                v.KeyName = varnam;
                v.ReadOnly = false;
                varCol.Add(v);
                return new DoItFeedback(v);
            }

            if (vari is null) {
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
            if (scriptText.AsSpan(pos, l).Equals(commandtext.AsSpan(), StringComparison.OrdinalIgnoreCase)) {
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
            return new DoItFeedback("Interner Programmfehler: " + ex.Message, false, infos.LogData);
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
            var a = string.Join(", ", Args[z]);
            if (a.Contains('*')) {
                a = a.Replace("*", string.Empty) + " (muss eine vorhandene Variable sein)";
            }

            co = co + "  - Argument " + (z + 1) + ": " + a;

            if (z == Args.Count - 1) {
                switch (LastArgMinCount) {
                    case LastArgMinCountType.ExactlyOnce:
                        break;

                    case LastArgMinCountType.Optional:
                        co += " (darf fehlen; darf mehrfach wiederholt werden)";
                        break;

                    case LastArgMinCountType.MinOnce:
                        co += " (muss angegeben werden; darf mehrfach wiederholt werden)";
                        break;

                    case LastArgMinCountType.MinTwice:
                        co += " (muss mindestens 2x wiederholt werden)";
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
            co += string.Join('\r', Constants) + "\r\n";
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
            co += string.Join('\r', UsesInDB);
        }

        return co;
    }

    public string ReadableText() => Syntax;

    public QuickImage? SymbolForReadableText() => null;

    /// <summary>
    /// Registriert während eines SyntaxChecks eine fehlende Variable als Dummy (VariableUnknown).
    /// Außerhalb des SyntaxChecks oder bei ungültigem Namen passiert nichts.
    /// Dadurch bleibt nachfolgender Code innerhalb eines Bodies - etwa bei if(exists(x), ... x verwenden ...) - validierbar.
    /// </summary>
    protected static void RegisterSyntaxCheckDummyVariable(VariableCollection varCol, ScriptProperties scp, string attributText) {
        if (!scp.SyntaxCheck) { return; }
        if (attributText.Trim() is not { Length: > 0 } varName) { return; }
        if (!Variable.IsValidName(varName)) { return; }
        varCol.Add(new VariableUnknown(varName, true, "Dummy für SyntaxCheck"));
    }

    private static bool? ParseOperators(string txt, VariableCollection varCol, ScriptProperties scp, LogData ld) {
        if (Variable.TryParseValue<VariableBool>(txt, out var result) && result is bool b) { return b; }

        #region Auf Restliche Boolsche Operationen testen

        //foreach (var check in Method_if.VergleichsOperatoren) {
        var (i, check) = NextText(txt, 0, Method_If.VergleichsOperatoren, false, false, Brackets);
        if (i > -1) {
            if (i < 1 && check != "!") { return null; } // <1, weil ja mindestens ein Zeichen vorher sein MUSS!

            if (i >= txt.Length - 1) { return null; } // siehe oben

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

            // V2 braucht nicht peprüft werden, muss ja eh der gleiche TYpe wie V1 sein
            if (v1 is not null) {
                if (v1.MyClassId != v2?.MyClassId) { return null; }
                if (!v1.ToStringPossible) { return null; }
            } else {
                if (v2 is not VariableBool) { return null; }
            }

            #endregion

            switch (check) {
                case "==": {
                        if (v1 is null) { return null; }
                        return v1.ValueForReplace == v2.ValueForReplace;
                    }

                case "!=": {
                        if (v1 is null) { return null; }
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
            var (pos, _) = NextText(attributtext, posc, Comma, false, false, Brackets);
            if (pos < 0) {
                attributes.Add(attributtext[posc..].Trim(BracketRound));
                break;
            }
            attributes.Add(attributtext[posc..pos].Trim(BracketRound));
            posc = pos + 1;
        } while (true);

        #endregion

        return attributes;
    }

    #endregion
}