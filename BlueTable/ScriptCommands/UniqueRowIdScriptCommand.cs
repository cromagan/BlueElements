// Licensed under AGPL-3.0; see License.md for disclaimer and details.

using BlueScript.Classes;

namespace BlueScript.ScriptCommands;

public class UniqueRowIdScriptCommand : ScriptCommand {

    #region Properties

    public override string Command => "uniquerowkey";
    public override string Description => "Gibt einen systemweit einzigartigen Zeilenschlüssel aller geladenen Tabellen aus.";
    public override bool MustUseReturnValue => true;
    public override string Returns => StringScriptVariable.ShortName_Plain;
    public override string Syntax => "UniqueRowKey()";

    #endregion

    #region Methods

    public override DoItFeedback DoIt(VariableCollection varCol, SplittedAttributesFeedback attvar, ScriptProperties scp) => new(RowCollection.UniqueKeyValue());

    #endregion
}