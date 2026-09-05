// Licensed under AGPL-3.0; see License.md for disclaimer and details.

namespace BlueScript.ScriptCommands;


/// <summary>
/// Entfernt aus dem Text &lt; &gt; Tags.
/// #Hasttag: Klammern, HTML, XML
/// </summary>
internal class RemoveXmlTagsScriptCommand : ScriptCommand {

    #region Properties

    public override List<List<string>> Args => [StringVal];
    public override string Command => "removexmltags";
    public override bool MustUseReturnValue => true;
    public override string Returns => StringScriptVariable.ShortName_Plain;
    public override string Syntax => "RemoveXMLTags(text)";

    #endregion

    #region Methods

    public override DoItFeedback DoIt(VariableCollection varCol, SplittedAttributesFeedback attvar, ScriptProperties scp) => new(attvar.ValueStringGet(0).RemoveXmlTags());

    #endregion
}
