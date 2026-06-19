// Licensed under AGPL-3.0; see License.md for disclaimer and details.

using BlueTable.AdditionalScriptVariables;
using System.Diagnostics;

namespace BlueTable.AdditionalScriptMethods;

public class Method_CallTable : Method_TableGeneric {

    #region Properties

    public override List<List<string>> Args => [TableVar, StringVal, StringVal];
    public override string Command => "calltable";

    public override string Description => "Führt das Skript in der angegebenen Tabelle aus.\r\n" +
            "Die Attribute werden in eine List-Varible Attributes eingefügt und stehen im auszuführenden Skript zur Verfügung.\r\n" +
        "Es werden keine Variablen aus dem Haupt-Skript übernommen oder zurückgegeben.";

    public override LastArgMinCountType LastArgMinCount => LastArgMinCountType.Optional;
    public override MethodType MethodLevel => MethodType.Sub;

    public override string Returns => VariableString.ShortName_Plain;
    public override string Syntax => "CallTable(Table, Scriptname, Attribut0, ...);";

    #endregion

    #region Methods

    public override DoItFeedback DoIt(VariableCollection varCol, SplittedAttributesFeedback attvar, ScriptProperties scp, LogData ld) {
        if (attvar.Attributes[0] is not VariableTable vtb || vtb.Table is not { IsDisposed: false } tb) { return new DoItFeedback("Tabelle nicht vorhanden", true, ld); }
        if (tb == MyTable(scp)) { return new DoItFeedback("Befehl Call benutzen!", true, ld); }

        var f = tb.IsGenericEditable(false);
        if (!string.IsNullOrEmpty(f)) { return new DoItFeedback($"Tabellensperre: {f}", false, ld); }

        var stackTrace = new StackTrace();
        if (stackTrace.FrameCount > 400) { return new DoItFeedback("Stapelspeicherüberlauf", true, ld); }

        if (scp.SyntaxCheck) { return DoItFeedback.Null(); }
        if (!scp.ProduktivPhase) { return DoItFeedback.TestModusInaktiv(ld); }

        #region Attributliste erzeugen

        var a = new List<string>();
        for (var z = 2; z < attvar.Attributes.Count; z++) {
            if (attvar.Attributes[z] is VariableString vs1) { a.Add(vs1.ValueString); }
        }

        #endregion

        var scx = tb.ExecuteScript(null, attvar.ValueStringGet(1), scp.ProduktivPhase, null, a, true, true, 0, scp.SyntaxCheck);
        scx.ConsumeBreakAndReturn();
        if (scx.NeedsScriptFix) {
            return new DoItFeedback($"Unterskript '{attvar.ValueStringGet(1)}' in '{tb.Caption}':\r\n{scx.ProtocolText}", true, ld);
        }
        return scx;
    }

    #endregion
}