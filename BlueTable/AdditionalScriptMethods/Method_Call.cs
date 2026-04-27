// Licensed under AGPL-3.0; see License.md for disclaimer and details.

using System.Diagnostics;

namespace BlueTable.AdditionalScriptMethods;

internal class Method_Call : Method_TableGeneric {

    #region Properties

    public override List<List<string>> Args => [StringVal, StringVal];

    public override string Command => "call";

    public override string Description => "Ruft eine Subroutine auf.\r\n" +
        "Variablen aus der Hauptroutine können in der Subroutine geändert werden und werden zurück gegeben.";


    public override int LastArgMinCount => 0;



    public override string Returns => VariableString.ShortName_Plain;


    public override string Syntax => "Call(SubName, Attribut0, ...);";

    #endregion

    #region Methods

    public override DoItFeedback DoIt(VariableCollection varCol, SplittedAttributesFeedback attvar, ScriptProperties scp, LogData ld) {
        if (MyTable(scp) is not { IsDisposed: false } myTb) { return DoItFeedback.InternerFehler(ld); }

        var vs = attvar.ValueStringGet(0);

        var script = myTb.EventScript.GetByKey(vs);
        if (script == null) { return new DoItFeedback("Skript nicht vorhanden: " + vs, true, ld); }

        var newat = script.Attributes();
        foreach (var thisAt in scp.ScriptAttributes) {
            if (!newat.Contains(thisAt)) {
                return new DoItFeedback("Aufzurufendes Skript hat andere Bedingungen, " + thisAt + " fehlt.", true, ld);
            }
        }

        var (f, error) = Script.NormalizedText(script.Script);

        if (!string.IsNullOrEmpty(error)) {
            return new DoItFeedback("Fehler in Unter-Skript " + vs + ": " + error, true, ld);
        }

        #region Attributliste erzeugen

        var a = new List<string>();
        for (var z = 1; z < attvar.Attributes.Count; z++) {
            if (attvar.Attributes[z] is VariableString vs1) { a.Add(vs1.ValueString); }
        }

        #endregion

        //Diese Routine kann nicht benutzt werden, weil sie die Zeilenvariableen neu erstellt
        //        var scx = myDb.ExecuteScript(null, vs, scp.ProduktivPhase, null, a, true, true);

        var sw = Stopwatch.StartNew();

        var scx = Method_CallByFilename.CallSub(varCol, scp, f, 0, vs, null, a, vs, ld);
        myTb.UpdateScript(script, scx, sw, null, scx.Variables?.GetBoolean("Extended") ?? false, scp.ProduktivPhase, !scp.ProduktivPhase);
        scx.ConsumeBreakAndReturn();// Aus der Subroutine heraus dürden keine Breaks/Return erhalten bleiben
        if (scx.NeedsScriptFix) {
            return new DoItFeedback($"Unterskript '{script.KeyName}' hat Fehler verursacht.", false, ld);
        }
        return scx;
    }

    #endregion
}