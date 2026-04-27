// Licensed under AGPL-3.0; see License.md for disclaimer and details.

namespace BlueScript.Methods;


internal class Method_RemoveXmlTags : Method {

    #region Properties

    public override List<List<string>> Args => [StringVal];
    public override string Command => "removexmltags";
    public override string Description => "Entfernt aus dem Text < > Tags.\r\n#Hasttag: Klammern, HTML, XML";
    public override bool MustUseReturnValue => true;
    public override string Returns => VariableString.ShortName_Plain;
    public override string Syntax => "RemoveXMLTags(text)";

    #endregion

    #region Methods

    public override DoItFeedback DoIt(VariableCollection varCol, SplittedAttributesFeedback attvar, ScriptProperties scp, LogData ld) => new(attvar.ValueStringGet(0).RemoveXmlTags());

    #endregion
}