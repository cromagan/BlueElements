// Licensed under MIT; see License.md for disclaimer, details, and extended user conditions.

using BlueScript.Classes;

namespace BlueScript.ScriptCommands;

/// <summary>
/// Gibt einen systemweit einzigartigen Zeilenschlüssel aller geladenen Tabellen aus.
/// </summary>
public class UniqueRowIdScriptCommand : ScriptCommand {

    #region Properties

    public override string Command => "uniquerowkey";
    public override bool MustUseReturnValue => true;
    public override string Returns => StringScriptVariable.ShortName_Plain;
    public override string Syntax => "UniqueRowKey()";

    #endregion

    #region Methods

    public override DoItFeedback DoIt(VariableCollection varCol, SplittedAttributesFeedback attvar, ScriptProperties scp) => new(RowCollection.UniqueKeyValue());

    #endregion
}
