// Licensed under AGPL-3.0; see License.md for disclaimer and details.

using BlueScript.Classes;
using BlueScript.Enums;
using BlueScript.Methods;
using BlueScript.Variables;
using BlueTable.Classes;
using System.Collections.Generic;

namespace BlueTable.AdditionalScriptMethods;


public class Method_UniqueRowId : Method {

    #region Properties

    public override List<List<string>> Args => [];
    public override string Command => "uniquerowkey";
    public override List<string> Constants => [];
    public override string Description => "Gibt einen systemweit einzigartigen Zeilenschlüssel aller geladenen Tabellen aus.";
    public override bool GetCodeBlockAfter => false;
    public override int LastArgMinCount => -1;
    public override MethodType MethodLevel => MethodType.Standard;
    public override bool MustUseReturnValue => true;
    public override string Returns => VariableString.ShortName_Plain;
    public override string StartSequence => "(";
    public override string Syntax => "UniqueRowKey()";

    #endregion

    #region Methods

    public override DoItFeedback DoIt(VariableCollection varCol, SplittedAttributesFeedback attvar, ScriptProperties scp, LogData ld) => new(RowCollection.UniqueKeyValue());

    #endregion
}