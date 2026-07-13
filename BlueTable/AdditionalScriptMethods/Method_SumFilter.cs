// Licensed under AGPL-3.0; see License.md for disclaimer and details.

namespace BlueTable.AdditionalScriptMethods;

public class Method_SumFilter : Method_TableGeneric {

    #region Properties

    public override List<List<string>> Args => [StringVal, FilterVar];
    public override string Command => "sumfilter";
    public override string Description => "Lädt eine andere Tabelle (die mit den Filtern definiert wurde)\rund gibt aus der angegebenen Spalte alle Einträge summiert zurück.\rDabei wird der Filter benutzt.\rEin Filter kann mit dem Befehl 'Filter' erstellt werden.";
    public override LastArgMinCountType LastArgMinCount => LastArgMinCountType.MinOnce;
    public override MethodType MethodLevel => MethodType.LongTime;
    public override bool MustUseReturnValue => true;
    public override string Returns => VariableDouble.ShortName_Plain;


    public override string Syntax => "SumFilter(Colum, Filter, ...)";

    #endregion

    #region Methods

    public override DoItFeedback DoIt(VariableCollection varCol, SplittedAttributesFeedback attvar, ScriptProperties scp) {
        var (allFi, errorreason, needsScriptFix) = Method_Filter.ObjectToFilter(attvar.Attributes, 1, MyTable(scp), scp.ScriptName, true);
        if (allFi is null || !string.IsNullOrEmpty(errorreason)) { return new DoItFeedback($"Filter-Fehler: {errorreason}", needsScriptFix); }

        if (allFi.Table is not { IsDisposed: false } tb) {
            allFi.Dispose();
            return new DoItFeedback("Tabellenfehler!", true);
        }

        var r = allFi.Rows;
        allFi.Dispose();

        var returncolumn = tb.Column[attvar.ReadableText(0)];
        if (returncolumn is null) { return new DoItFeedback("Spalte nicht gefunden: " + attvar.ReadableText(0), true); }

        returncolumn.AddSystemInfo("Value Used in Script", tb, scp.ScriptName);

        var x = returncolumn.Summe(r);

        return x is not { } xd ? new DoItFeedback("Summe konnte nicht berechnet werden.", true) : new DoItFeedback(xd);
    }

    #endregion
}