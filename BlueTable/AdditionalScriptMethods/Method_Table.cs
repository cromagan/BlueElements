// Licensed under AGPL-3.0; see License.md for disclaimer and details.

using BlueTable.AdditionalScriptVariables;

namespace BlueTable.AdditionalScriptMethods;

internal class MethodTable : Method {

    #region Properties

    public override List<List<string>> Args => [StringVal];
    public override string Command => "table";
    public override string Description => "Versucht die Tabelle in den Speicher zu holen.";
    public override MethodType MethodLevel => MethodType.LongTime;
    public override bool MustUseReturnValue => true;
    public override string Returns => VariableTable.ShortName_Variable;
    public override string Syntax => "Table(Filename/Tablename)";

    #endregion

    #region Methods

    public override DoItFeedback DoIt(VariableCollection varCol, SplittedAttributesFeedback attvar, ScriptProperties scp) {
        var filn = attvar.ValueStringGet(0);

        if (Table.Get(filn) is not { IsDisposed: false } tb) {
            return new DoItFeedback($"Tabelle '{filn}' nicht gefunden", true);
        }

        if (!tb.Unlocked) {
            return new DoItFeedback($"Tabelle '{filn}' ist passwortgeschützt und kann im Skript nicht verwendet werden.", true);
        }

        return new DoItFeedback(new VariableTable(tb));
    }

    #endregion
}