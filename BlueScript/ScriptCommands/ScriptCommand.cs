// Licensed under AGPL-3.0; see License.md for disclaimer and details.

namespace BlueScript.ScriptCommands;

public abstract class ScriptCommand : IReadableTextWithKey {

    #region Fields

    public static readonly AssemblyAwareCache<ScriptCommand> AllMethods = new();
    public static readonly List<string> BoolVal = [BoolScriptVariable.ShortName_Plain];
    public static readonly List<string> FloatVal = [DoubleScriptVariable.ShortName_Plain];
    public static readonly List<string> ListStringVar = [ListOfStringsScriptVariable.ShortName_Variable];
    public static readonly List<string> StringVal = [StringScriptVariable.ShortName_Plain];
    public static readonly List<string> StringVar = [StringScriptVariable.ShortName_Variable];

    #endregion

    #region Properties

    public static Dictionary<string, List<ScriptCommand>> AllMethodByCommand {
        get {
            if (field is not null) { return field; }

            var lookup = new Dictionary<string, List<ScriptCommand>>(StringComparer.OrdinalIgnoreCase);
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
    public virtual LastArgMinCountTypeScriptCommand LastArgMinCount => LastArgMinCountTypeScriptCommand.ExactlyOnce;

    //TODO: 0 implementieren
    public virtual bool MustUseReturnValue => false;

    public string QuickInfo => HintText();
    public virtual string Returns => string.Empty;
    public virtual ScriptCommandType ScriptCommandLevel => ScriptCommandType.Standard;
    public virtual string StartSequence => "(";

    public abstract string Syntax { get; }

    public List<string> UsesInDB { get; } = [];

    #endregion

    #region Methods

    /// <summary>
    /// Prüft die Attribut-Anzahl eines Methodenaufrufs gegen die
    /// Deklaration (Args und LastArgMinCount).
    /// Gibt bei Fehler die Meldung zurück, sonst <c>null</c>.
    /// Wird sowohl von SplitAttributeToVars als auch
    /// vom ScriptPreCheck genutzt.
    /// </summary>
    /// <param name="command"></param>
    /// <param name="attributes">Die bereits via SplitAttributeToString
    /// gesplitteten Attribute oder <c>null</c>, wenn der Text leer war.</param>
    /// <param name="argCount"></param>
    /// <param name="lastArgMinCount"></param>
    /// <remarks>
    /// Die für die Fehlermeldung benötigte Syntax wird erst beim Auftreten
    /// eines Fehlers über AllMethods anhand des Befehlsnamens ermittelt.
    /// </remarks>
    public static string? CheckArgumentCount(string command, List<string>? attributes, int argCount, LastArgMinCountTypeScriptCommand lastArgMinCount) {
        string GetSyntax() => AllMethods.Instances.FirstOrDefault(m => m.Command.Equals(command, StringComparison.OrdinalIgnoreCase))?.Syntax ?? string.Empty;

        if (argCount == 0) {
            return attributes is { Count: > 0 }
                ? $"'{command}' erwartet keine Attribute."
                : null;
        }

        if (attributes is not { Count: > 0 }) {
            return $"Bei '{command}' wurden keine Attribute übergeben, erwartet wurden {argCount}. Beispiel: {GetSyntax()}";
        }

        if (attributes.Count < argCount && lastArgMinCount != LastArgMinCountTypeScriptCommand.Optional) {
            return $"Zu wenige Attribute bei '{command}'. Beispiel: {GetSyntax()}";
        }

        if (attributes.Count < argCount - 1) {
            return $"Zu wenige Attribute bei '{command}'. Beispiel: {GetSyntax()}";
        }

        if (lastArgMinCount == LastArgMinCountTypeScriptCommand.ExactlyOnce && attributes.Count > argCount) {
            return $"Zu viele Attribute bei '{command}'. Beispiel: {GetSyntax()}";
        }

        if (lastArgMinCount == LastArgMinCountTypeScriptCommand.MinOnce && attributes.Count < argCount) {
            return $"Zu wenige Attribute bei '{command}'. Beispiel: {GetSyntax()}";
        }

        if (lastArgMinCount == LastArgMinCountTypeScriptCommand.MinTwice && attributes.Count < argCount + 1) {
            return $"Zu wenige Attribute bei '{command}'. Beispiel: {GetSyntax()}";
        }

        return null;
    }

    /// <summary>
    /// Convenience-Überladung: Splittet den Attribut-Text und nutzt die
    /// Deklaration der übergebenen Methode.
    /// </summary>
    public static string? CheckArgumentCount(ScriptCommand thisC, string attributText) =>
        CheckArgumentCount(thisC.Command, SplitAttributeToString(attributText), thisC.Args.Count, thisC.LastArgMinCount);

    /// <summary>
    /// Gibt den Text des Codeblocks zurück. Dabei werden die Zeilenumbrüche vor der { nicht entfernt, aber die Brackets {} selbst schon.
    /// Das muss berücksichtigt werden, um die Skript-Position richtig zu setzen!
    /// </summary>
    /// <param name="scriptText"></param>
    /// <param name="start"></param>
    /// <returns>Ein OperationResult, dessen OperationResult.Value bei Erfolg den Codeblock enthält.</returns>
    public static OperationResult GetCodeBlockText(string scriptText, int start) {
        var maxl = scriptText.Length;

        var tmp = start;

        do {
            if (tmp >= maxl) { return OperationResult.Failed("Keinen nachfolgenden Codeblock gefunden."); }
            if (scriptText[tmp] == '{') { break; }
            if (scriptText[tmp] != '¶') { return OperationResult.Failed("Keinen nachfolgenden Codeblock gefunden."); }
            tmp++;
        } while (true);

        var (posek, _) = NextText(scriptText, start, BracketCurlyClose, false, false, Brackets);
        if (posek < start) {
            return OperationResult.Failed("Kein Codeblock Ende gefunden.");
        }

        var s = scriptText[start..tmp] + scriptText[(tmp + 1)..posek];

        return new OperationResult(s);
    }

    public static GetEndFeedback GetEnd(string scriptText, int startpos, int lengthStartSequence, string endSequence) {
        //z.B: beim Befehl DO
        if (string.IsNullOrEmpty(endSequence)) {
            return new GetEndFeedback(startpos, string.Empty);
        }

        var (pos, which) = NextText(scriptText, startpos, [endSequence], false, false, Brackets);
        if (pos < startpos) {
            return new GetEndFeedback("Endpunkt '" + endSequence + "' nicht gefunden.", true);
        }

        var txtBtw = scriptText[(startpos + lengthStartSequence)..pos];
        return new GetEndFeedback(pos + which.Length, txtBtw);
    }

    public static List<ScriptCommand> GetMethods(ScriptCommandType maxLevel) {
        var m = new List<ScriptCommand>();

        foreach (var thism in AllMethods.Instances) {
            if (thism.ScriptCommandLevel <= maxLevel) {
                m.Add(thism);
            }
        }

        return m;
    }

    public static DoItFeedback GetVariableByParsing(string txt, LogData ld, VariableCollection varCol, ScriptProperties scp) {
        if (string.IsNullOrEmpty(txt)) { return new DoItFeedback("Kein Wert zum Parsen angekommen.", true); }

        if (txt.StartsWith('(')) {
            var (pose, _) = NextText(txt, 0, BracketRoundClose, false, false, Brackets);
            if (pose < txt.Length - 1 && pose > 0) {
                // Wir haben so einen Fall: (true) || (true)
                var scx = GetVariableByParsing(txt[1..pose], ld, varCol, scp);
                if (scx.Failed) {
                    scx.ChangeFailedReason("Befehls-Berechnungsfehler in ()", true);
                    return scx;
                }
                if (scx.ReturnValue is null) {
                    scx.ChangeFailedReason("Allgemeiner Befehls-Berechnungsfehler", true);
                    return scx;
                }
                if (!scx.ReturnValue.ToStringPossible) {
                    scx.ChangeFailedReason("Falscher Variablentyp: " + scx.ReturnValue.MyClassId, true);
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
                    var l = SplitAttributeToVars("?", varCol, tl, [[StringScriptVariable.ShortName_Plain]], LastArgMinCountTypeScriptCommand.MinOnce, ld, scp);
                    if (l.Failed) {
                        return new DoItFeedback(l.FailedReason, l.NeedsScriptFix);
                    }
                    txt = "[\"" + string.Join("\",\"", l.Attributes.OfType<StringScriptVariable>().Select(vs => vs.ValueString)) + "\"]";
                }
            }
        }

        txt = txt.Trim(BracketRound);

        var (uu, _) = NextText(txt, 0, IfScriptCommand.UndUnd, false, false, Brackets);
        if (uu > 0) {
            var scx = GetVariableByParsing(txt[..uu], ld, varCol, scp);
            if (scx.Failed || scx.ReturnValue is null) {
                scx.ChangeFailedReason($"Befehls-Berechnungsfehler vor &&: {txt[..uu]}", true);
                return scx;
            }

            if (scx.ReturnValue is UnknownScriptVariable) {
                scx.ChangeFailedReason($"Befehls-Berechnungsfehler vor &&: {txt[..uu]}", true);
                return scx;
            }

            if (scx.ReturnValue is BoolScriptVariable { ValueBool: false }) { return scx; }
            return GetVariableByParsing(txt[(uu + 2)..], ld, varCol, scp);
        }

        var (oo, _) = NextText(txt, 0, IfScriptCommand.OderOder, false, false, Brackets);
        if (oo > 0) {
            var txt1 = GetVariableByParsing(txt[..oo], ld, varCol, scp);
            if (txt1.Failed || txt1.ReturnValue is null) {
                return new DoItFeedback($"Befehls-Berechnungsfehler vor ||: {txt[..oo]}", txt1.NeedsScriptFix);
            }

            if (txt1.ReturnValue is UnknownScriptVariable) {
                return new DoItFeedback($"Befehls-Berechnungsfehler vor ||: {txt[..oo]}", true);
            }

            if (txt1.ReturnValue is BoolScriptVariable { ValueBool: true }) { return txt1; }
            return GetVariableByParsing(txt[(oo + 2)..], ld, varCol, scp);
        }

        // Variablen nur ersetzen, wenn Variablen auch vorhanden sind.

        var t = ReplaceVariable(txt, varCol);
        if (t.Failed) { return new DoItFeedback("Variablen-Berechnungsfehler", t.NeedsScriptFix); }
        if (t.ReturnValue is not null) { return new DoItFeedback(t.ReturnValue); }
        if (txt != t.NormalizedText) { return GetVariableByParsing(t.NormalizedText, ld, varCol, scp); }

        var t2 = ReplaceCommandsAndVars(txt, varCol, ld, scp);
        if (t2.Failed) { return new DoItFeedback(t2.FailedReason, t2.NeedsScriptFix); }
        if (t2.ReturnValue is not null) { return new DoItFeedback(t2.ReturnValue); }
        if (txt != t2.NormalizedText) { return GetVariableByParsing(t2.NormalizedText, ld, varCol, scp); }

        var (posa, _) = NextText(txt, 0, ["("], false, false, Brackets);
        if (posa > -1) {
            var (pose, _) = NextText(txt, posa, BracketRoundClose, false, false, Brackets);
            if (pose <= posa) { return new DoItFeedback("Klammer-Fehler", true); }

            var tmptxt = txt.Substring(posa + 1, pose - posa - 1);
            if (!string.IsNullOrEmpty(tmptxt)) {
                var scx = GetVariableByParsing(tmptxt, ld, varCol, scp);
                if (scx.Failed) {
                    scx.ChangeFailedReason("Befehls-Berechnungsfehler in ()", true);
                    return scx;
                }
                if (scx.ReturnValue is null) {
                    scx.ChangeFailedReason("Allgemeiner Berechnungsfehler in ()", true);
                    return scx;
                }
                if (!scx.ReturnValue.ToStringPossible) {
                    scx.ChangeFailedReason("Falscher Variablentyp: " + scx.ReturnValue.MyClassId, true);
                    return scx;
                }
                // WICHTIG: Hier muss der neue String wieder von vorne geparsed werden
                return GetVariableByParsing(txt.Substring(0, posa) + scx.ReturnValue.ValueForReplace + txt.Substring(pose + 1), ld, varCol, scp);
            }
        }

        if (ParseOperators(txt, varCol, scp, ld) is { } b) { return new DoItFeedback(b); }

        foreach (var thisVt in ScriptVariable.VarTypes.Instances) {
            if (thisVt.GetFromStringPossible) {
                if (thisVt.TryParse(txt, out var v) && v is not null) {
                    return new DoItFeedback(v);
                }
            }
        }

        return new DoItFeedback("Wert kann nicht geparsed werden: " + txt, true);
    }

    public static GetEndFeedback ReplaceCommandsAndVars(string txt, VariableCollection varCol, LogData ld, ScriptProperties scp) {

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
                return new GetEndFeedback(scx.FailedReason, scx.NeedsScriptFix);
            }

            if (pos == 0 && txt.Length == scx.Position) { return new GetEndFeedback(scx.ReturnValue); }
            if (scx.ReturnValue is null) { return new GetEndFeedback("Variablenfehler", true); }
            if (!scx.ReturnValue.ToStringPossible) { return new GetEndFeedback("Variable muss als Objekt behandelt werden", true); }

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
    public static GetEndFeedback ReplaceVariable(string txt, VariableCollection? varCol) {
        if (varCol is null) { return new GetEndFeedback("Interner Variablen-Fehler", true); }

        var posc = 0;
        var allVarNames = varCol.AllStringableNames();

        do {
            var (pos, which) = NextText(txt, posc, allVarNames, true, true, Brackets);

            if (pos < 0) { return new GetEndFeedback(0, txt); }

            var thisV = varCol.GetByKey(which);
            var endz = pos + which.Length;

            if (thisV is null) { return new GetEndFeedback("Variablen-Fehler " + which, true); }

            txt = string.Concat(txt.AsSpan(0, pos), thisV.ValueForReplace, txt.AsSpan(endz));
            posc = pos;
        } while (true);
    }

    /// <summary>
    /// Splittet den AttributText an Kommas auf Top-Level (Klammern werden respektiert).
    /// </summary>
    public static List<string>? SplitAttributeToString(string attributtext) {
        if (string.IsNullOrEmpty(attributtext)) { return null; }

        List<string> attributes = [];

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

        return attributes;
    }

    public static SplittedAttributesFeedback SplitAttributeToVars(string command, VariableCollection? varcol, string attributText, List<List<string>> types, LastArgMinCountTypeScriptCommand lastArgMinCount, LogData ld, ScriptProperties? scp) {
        var attributes = SplitAttributeToString(attributText);

        var countError = CheckArgumentCount(command, attributes, types.Count, lastArgMinCount);
        if (countError is { Length: > 0 }) { return new SplittedAttributesFeedback(ScriptIssueType.AttributAnzahl, countError, true); }

        if (types.Count == 0) { return new SplittedAttributesFeedback([]); }

        //  Variablen und Routinen ersetzen
        List<ScriptVariable> feedbackVariables = [];
        for (var n = 0; n < attributes.Count; n++) {
            //var lb = attributes[n].Count(c => c == '¶'); // Zeilenzähler weitersetzen
            attributes[n] = attributes[n].RemoveChars("¶"); // Zeilenzähler entfernen

            var exceptetType = n < types.Count ? types[n] : types[types.Count - 1]; // Bei Endlessargs den letzten nehmen

            // Variable ermitteln oder eine Dummy-Variable als Rückgabe ermitteln
            ScriptVariable? v;

            var mustBeVar = exceptetType.Count > 0 && exceptetType[0].StartsWith('*');

            if (mustBeVar) {
                var varn = attributes[n];
                if (!ScriptVariable.IsValidName(varn)) { return new SplittedAttributesFeedback(ScriptIssueType.VariableErwartet, "Variablenname erwartet bei Attribut " + (n + 1), true); }

                v = varcol?.GetByKey(varn);
                if (v is null) {
                    return new SplittedAttributesFeedback(ScriptIssueType.VariableNichtGefunden, "Variable nicht gefunden bei Attribut " + (n + 1), true);
                }
            } else {
                if (varcol is null || scp is null) {
                    return new SplittedAttributesFeedback(ScriptIssueType.BerechnungFehlgeschlagen, "Interner Fehler: Null-Parameter", true);
                }
                var tmp2 = GetVariableByParsing(attributes[n], ld, varcol, scp);
                if (tmp2.Failed) { return new SplittedAttributesFeedback(ScriptIssueType.BerechnungFehlgeschlagen, tmp2.FailedReason, tmp2.NeedsScriptFix); }
                if (tmp2.ReturnValue is null) { return new SplittedAttributesFeedback(ScriptIssueType.BerechnungFehlgeschlagen, $"Interner Fehler", true); }

                if (tmp2.ReturnValue is UnknownScriptVariable vukn) {
                    foreach (var thisC in AllMethods.Instances) {
                        var f = thisC.CanDo(attributes[n], 0, false, ld);
                        if (string.IsNullOrEmpty(f.FailedReason)) {
                            if (command.Equals(VarScriptCommand.CommandText, StringComparison.OrdinalIgnoreCase)) {
                                return new SplittedAttributesFeedback(ScriptIssueType.BerechnungFehlgeschlagen, $"Die Variable konnte nicht berechnet werden, dafür verwendte Befehle sind in diesem Skript nicht erlaubt: '{vukn.Value}'", true);
                            }

                            return new SplittedAttributesFeedback(ScriptIssueType.BerechnungFehlgeschlagen, $"Der Befehl '{command}' kann in diesen Skript nicht verwendet werden.", true);
                        }
                    }
                }

                v = tmp2.ReturnValue;
            }

            // Den Typ der Variable checken
            var ok = false;

            foreach (var thisAt in exceptetType) {
                if (thisAt.TrimStart('*') == v.MyClassId) { ok = true; break; }
                if (thisAt.TrimStart('*') == ScriptVariable.Any_Plain) { ok = true; break; }
            }

            if (!ok) {
                if (v is UnknownScriptVariable ukn) {
                    return new SplittedAttributesFeedback(ScriptIssueType.FalscherDatentyp, $"Bei '{command}' konnte das Attribut '{n + 1}' nicht aufgelöst werden: {ukn.Value}", true);
                }

                return new SplittedAttributesFeedback(ScriptIssueType.FalscherDatentyp, $"Bei '{command}' ist das Attribut '{n + 1}' nicht einer der erwarteten Typen '{string.Join("' oder '", exceptetType)}', sondern {v.MyClassId}", true);
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

        if (pos < 1 || pos > newcommand.Length - 2) { return new DoItFeedback("Fehler mit = - Zeichen", true); }

        var varnam = newcommand[..pos];

        if (!ScriptVariable.IsValidName(varnam)) { return new DoItFeedback(varnam + " ist kein gültiger Variablen-Name", true); }

        var vari = varCol.GetByKey(varnam);
        if (generateVariable && vari is not null) {
            return new DoItFeedback("Variable " + varnam + " ist bereits vorhanden.", true);
        }
        if (!generateVariable && vari is null) {
            return new DoItFeedback("Variable " + varnam + " nicht vorhanden.", true);
        }

        var value = newcommand[(pos + 1)..^1];

        List<List<string>> sargs = [[ScriptVariable.Any_Plain]];

        var attvar = SplitAttributeToVars("var", varCol, value, sargs, 0, ld, scp);

        if (attvar.Failed) { return new DoItFeedback(attvar.FailedReason, attvar.NeedsScriptFix); }

        if (attvar.Attributes[0] is UnknownScriptVariable) {
            return new DoItFeedback("Der Wert '" + value + "' für Variable '" + varnam + "' konnte nicht aufgelöst werden (unbekannte Methode oder Variable im Ausdruck).", true);
        }

        if (attvar.Attributes[0] is { } v) {
            if (generateVariable) {
                if (vari is UnknownScriptVariable) { varCol.Remove(vari.KeyName); }
                v.KeyName = varnam;
                v.ReadOnly = false;
                varCol.Add(v);
                return new DoItFeedback(v);
            }

            if (vari is null) {
                // es sollte generateVariable greifen, und hier gar nimmer ankommen. Aber um die IDE zu befriedigen
                return DoItFeedback.InternerFehler();
            }

            var f = vari.GetValueFrom(v);
            return new DoItFeedback(f, !string.IsNullOrWhiteSpace(f));
        }
        // attvar.Attributes[0] müsste immer eine Variable sein...
        return DoItFeedback.InternerFehler();
    }

    public CanDoFeedback CanDo(string scriptText, int pos, bool expectedvariablefeedback, LogData ld) {
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
                var f = GetEnd(scriptText, pos + Command.Length, StartSequence.Length, EndSequence);
                if (f.Failed) {
                    return new CanDoFeedback(f.ContinuePosition, "Fehler bei " + commandtext, true, ld);
                }
                var cont = f.ContinuePosition;
                var codebltxt = string.Empty;
                if (GetCodeBlockAfter) {
                    var cbr = GetCodeBlockText(scriptText, cont);
                    if (cbr.IsFailed) { return new CanDoFeedback(f.ContinuePosition, cbr.FailedReason, true, ld); }
                    codebltxt = cbr.Value as string ?? string.Empty;
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
                ? DoItFeedback.AttributFehler(attvar)
                : DoIt(varCol, attvar, scp);
        } catch (Exception ex) {
            return new DoItFeedback("Interner Programmfehler: " + ex.Message, false);
        }
    }

    public abstract DoItFeedback DoIt(VariableCollection varCol, SplittedAttributesFeedback attvar, ScriptProperties scp);

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
                    case LastArgMinCountTypeScriptCommand.ExactlyOnce:
                        break;

                    case LastArgMinCountTypeScriptCommand.Optional:
                        co += " (darf fehlen; darf mehrfach wiederholt werden)";
                        break;

                    case LastArgMinCountTypeScriptCommand.MinOnce:
                        co += " (muss angegeben werden; darf mehrfach wiederholt werden)";
                        break;

                    case LastArgMinCountTypeScriptCommand.MinTwice:
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

    private static bool? ParseOperators(string txt, VariableCollection varCol, ScriptProperties scp, LogData ld) {
        if (ScriptVariable.TryParseValue<BoolScriptVariable>(txt, out var result) && result is bool b) { return b; }

        #region Auf Restliche Boolsche Operationen testen

        //foreach (var check in If.VergleichsOperatoren) {
        var (i, check) = NextText(txt, 0, IfScriptCommand.VergleichsOperatoren, false, false, Brackets);
        if (i > -1) {
            if (i < 1 && check != "!") { return null; } // <1, weil ja mindestens ein Zeichen vorher sein MUSS!

            if (i >= txt.Length - 1) { return null; } // siehe oben

            #region Die Werte vor und nach dem Trennzeichen in den Variablen v1 und v2 ablegen

            #region Ersten Wert als s1 ermitteln

            var s1 = txt[..i];
            ScriptVariable? v1 = null;
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
                if (v2 is not BoolScriptVariable) { return null; }
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
                        if (v1 is not DoubleScriptVariable v1Fl) { return null; }
                        if (v2 is not DoubleScriptVariable v2Fl) { return null; }
                        return v1Fl.ValueNum >= v2Fl.ValueNum;
                    }

                case "<=": {
                        if (v1 is not DoubleScriptVariable v1Fl) { return null; }
                        if (v2 is not DoubleScriptVariable v2Fl) { return null; }
                        return v1Fl.ValueNum <= v2Fl.ValueNum;
                    }

                case "<": {
                        if (v1 is not DoubleScriptVariable v1Fl) { return null; }
                        if (v2 is not DoubleScriptVariable v2Fl) { return null; }
                        return v1Fl.ValueNum < v2Fl.ValueNum;
                    }

                case ">": {
                        if (v1 is not DoubleScriptVariable v1Fl) { return null; }
                        if (v2 is not DoubleScriptVariable v2Fl) { return null; }
                        return v1Fl.ValueNum > v2Fl.ValueNum;
                    }

                case "||": {
                        if (v1 is not BoolScriptVariable v1Bo) { return null; }
                        if (v2 is not BoolScriptVariable v2Bo) { return null; }
                        return v1Bo.ValueBool || v2Bo.ValueBool;
                    }

                case "&&": {
                        if (v1 is not BoolScriptVariable v1Bo) { return null; }
                        if (v2 is not BoolScriptVariable v2Bo) { return null; }
                        return v1Bo.ValueBool && v2Bo.ValueBool;
                    }

                case "!": {
                        // S1 dürfte eigentlich nie was sein: !False||!false
                        // entweder ist es ganz am anfang, oder direkt nach einem Trenneichen
                        if (v2 is not BoolScriptVariable v2Bo) { return null; }
                        return !v2Bo.ValueBool;
                    }
            }
        }

        #endregion

        return null;
    }

    #endregion
}