// Licensed under AGPL-3.0; see License.md for disclaimer and details.

using BlueScript.Classes;
using BlueScript.Enums;
using BlueScript.Methods;
using BlueScript.Variables;
using BlueTable.AdditionalScriptVariables;
using BlueTable.Classes;
using System.Collections.Generic;

namespace BlueTable.AdditionalScriptMethods;

internal class Method_Table : Method {

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

    public override DoItFeedback DoIt(VariableCollection varCol, SplittedAttributesFeedback attvar, ScriptProperties scp, LogData ld) {
        var filn = attvar.ValueStringGet(0);

        if (Table.Get(filn, null) is { IsDisposed: false } tb) {
            return new DoItFeedback(new VariableTable(tb));
        }

        return new DoItFeedback($"Tabelle '{filn}' nicht gefunden", true, ld);
    }

    #endregion
}