// Licensed under MIT; see License.md for disclaimer, details, and extended user conditions.

namespace BlueScript.ScriptCommands;

/// <summary>
/// Gibt die Anzahl der Elemente der Liste zurück.
/// </summary>
internal class CountScriptCommand : ScriptCommand {

    #region Properties

    public override List<List<string>> Args => [ListStringVar];
    public override string Command => "count";
    public override bool MustUseReturnValue => true;
    public override string Returns => DoubleScriptVariable.ShortName_Plain;
    public override string Syntax => "Count(ListVariable)";

    #endregion

    #region Methods

    public override DoItFeedback DoIt(VariableCollection varCol, SplittedAttributesFeedback attvar, ScriptProperties scp) => new(attvar.ValueListStringGet(0).Count);

    #endregion
}
