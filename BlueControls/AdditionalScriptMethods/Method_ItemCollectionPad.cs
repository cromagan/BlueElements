// Licensed under AGPL-3.0; see License.md for disclaimer and details.

using BlueScript.Enums;
using BlueScript.Methods;
using BlueScript.Structures;
using BlueScript.Variables;
using System.Collections.Generic;

namespace BlueControls.AdditionalScriptMethods;


public class Method_ItemCollectionPad : Method {

    #region Properties

    public override string Command => "itemcollectionpad";
    public override string Description => "Erstellt eine neue Item-Collection. In diese können PadItems geladen werden.";
    public override bool MustUseReturnValue => true;
    public override string Returns => VariableItemCollectionPad.ShortName_Variable;
    public override string Syntax => "ItemCollectionPadItem()";

    #endregion

    #region Methods

    public override DoItFeedback DoIt(VariableCollection varCol, SplittedAttributesFeedback attvar, ScriptProperties scp, LogData ld) => new(new VariableItemCollectionPad([]));

    #endregion
}