// Licensed under AGPL-3.0; see License.md for disclaimer and details.

namespace BlueScript.Methods;

internal class Method_Count : Method {

    #region Properties

    public override List<List<string>> Args => [ListStringVar];
    public override string Command => "count";
    public override string Description => "Gibt die Anzahl der Elemente der Liste zurück.";
    public override bool MustUseReturnValue => true;
    public override string Returns => VariableDouble.ShortName_Plain;
    public override string Syntax => "Count(ListVariable)";

    #endregion

    #region Methods

    public override DoItFeedback DoIt(VariableCollection varCol, SplittedAttributesFeedback attvar, ScriptProperties scp) => new(attvar.ValueListStringGet(0).Count);

    #endregion
}