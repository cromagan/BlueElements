// Licensed under AGPL-3.0; see License.md for disclaimer and details.

using BlueScript.Classes;
using BlueScript.Enums;
using BlueScript.Variables;
using System.Collections.Generic;

namespace BlueTable.AdditionalScriptMethods;

public class Method_SumFilter : Method_TableGeneric {

    #region Properties

    public override List<List<string>> Args => [StringVal, FilterVar];
    public override string Command => "sumfilter";
    public override List<string> Constants => [];
    public override string Description => "Lädt eine andere Tabelle (die mit den Filtern definiert wurde)\rund gibt aus der angegebenen Spalte alle Einträge summiert zurück.\rDabei wird der Filter benutzt.\rEin Filter kann mit dem Befehl 'Filter' erstellt werden.";
    public override bool GetCodeBlockAfter => false;
    public override int LastArgMinCount => 1;
    public override MethodType MethodLevel => MethodType.LongTime;
    public override bool MustUseReturnValue => true;
    public override string Returns => VariableDouble.ShortName_Plain;

    public override string StartSequence => "(";

    public override string Syntax => "SumFilter(Colum, Filter, ...)";

    #endregion

    #region Methods

    public override DoItFeedback DoIt(VariableCollection varCol, SplittedAttributesFeedback attvar, ScriptProperties scp, LogData ld) {
        var (allFi, errorreason, needsScriptFix) = Method_Filter.ObjectToFilter(attvar.Attributes, 1, MyTable(scp), scp.ScriptName, true);
        if (allFi == null || !string.IsNullOrEmpty(errorreason)) { return new DoItFeedback($"Filter-Fehler: {errorreason}", needsScriptFix, ld); }

        if (allFi.Table is not { IsDisposed: false } tb) {
            allFi.Dispose();
            return new DoItFeedback("Tabellefehler!", true, ld);
        }

        var r = allFi.Rows;
        allFi.Dispose();

        var returncolumn = tb.Column[attvar.ReadableText(0)];
        if (returncolumn == null) { return new DoItFeedback("Spalte nicht gefunden: " + attvar.ReadableText(0), true, ld); }

        returncolumn.AddSystemInfo("Value Used in Script", tb, scp.ScriptName);

        var x = returncolumn.Summe(r);

        return x is not { } xd ? new DoItFeedback("Summe konnte nicht berechnet werden.", true, ld) : new DoItFeedback(xd);
    }

    #endregion
}