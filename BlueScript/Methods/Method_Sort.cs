// Licensed under AGPL-3.0; see License.md for disclaimer and details.

using BlueBasics;
using BlueScript.Classes;
using BlueScript.Enums;
using BlueScript.Variables;
using System.Collections.Generic;

namespace BlueScript.Methods;


internal class Method_Sort : Method {

    #region Properties

    public override List<List<string>> Args => [ListStringVar, BoolVal];
    public override string Command => "sort";
    public override List<string> Constants => [];
    public override string Description => "Sortiert die Liste. Falls das zweite Attribut TRUE ist, werden Doubletten und leere Einträge entfernt.";
    public override bool GetCodeBlockAfter => false;
    public override int LastArgMinCount => -1;
    public override MethodType MethodLevel => MethodType.Standard;
    public override bool MustUseReturnValue => false;
    public override string Returns => string.Empty;
    public override string StartSequence => "(";
    public override string Syntax => "Sort(ListVariable, EliminateDupes);";

    #endregion

    #region Methods

    public override DoItFeedback DoIt(VariableCollection varCol, SplittedAttributesFeedback attvar, ScriptProperties scp, LogData ld) {
        if (attvar.ReadOnly(0)) { return DoItFeedback.Schreibgschützt(ld); }

        var x = attvar.ValueListStringGet(0);
        if (attvar.ValueBoolGet(1)) {
            x = x.SortedDistinctList();
        } else {
            x.Sort();
        }

        if (attvar.Attributes[0] is not VariableListString vli) {
            return DoItFeedback.AttributFehler(ld, attvar);
        }

        vli.ValueList = x;
        return DoItFeedback.Null();
    }

    #endregion
}