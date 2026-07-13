// Licensed under AGPL-3.0; see License.md for disclaimer and details.

namespace BlueScript.Methods;


internal class Method_StringToUTF8 : Method {

    #region Properties

    public override List<List<string>> Args => [StringVal];
    public override string Command => "stringtoutf8";
    public override string Description => "Ersetzt einen ASCII-String nach UTF8.";
    public override bool MustUseReturnValue => true;
    public override string Returns => VariableString.ShortName_Plain;
    public override string Syntax => "StringToUTF8(String, IgnoreBRbool)";

    #endregion

    #region Methods

    public override DoItFeedback DoIt(VariableCollection varCol, SplittedAttributesFeedback attvar, ScriptProperties scp) => new(attvar.ValueStringGet(0).StringtoUtf8());

    #endregion
}