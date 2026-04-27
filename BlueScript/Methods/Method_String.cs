// Licensed under AGPL-3.0; see License.md for disclaimer and details.

using BlueScript.Classes;
using BlueScript.Enums;
using BlueScript.Variables;
using System.Collections.Generic;

namespace BlueScript.Methods;


internal class Method_String : Method {

    #region Properties

    public override List<List<string>> Args => [[VariableDouble.ShortName_Plain, VariableString.ShortName_Plain]];
    public override string Command => "string";
    public override List<string> Constants => [];
    public override string Description => "Wandelt die Zahl in einen Text um. Kulanterweise werden Strings einfach als String weitergegeben.";
    public override bool GetCodeBlockAfter => false;
    public override int LastArgMinCount => -1;
    public override MethodType MethodLevel => MethodType.Standard;
    public override bool MustUseReturnValue => true;
    public override string Returns => VariableString.ShortName_Plain;
    public override string StartSequence => "(";
    public override string Syntax => "String(numeral)";

    #endregion

    #region Methods

    public override DoItFeedback DoIt(VariableCollection varCol, SplittedAttributesFeedback attvar, ScriptProperties scp, LogData ld) => new(attvar.ReadableText(0));

    #endregion
}