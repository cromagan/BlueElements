// Licensed under MIT; see License.md for disclaimer, details, and extended user conditions.

namespace BlueScript.ScriptCommands;


/// <summary>
/// Ersetzt einen ASCII-StringScriptCommand nach UTF8.
/// </summary>
internal class StringToUTF8ScriptCommand : ScriptCommand {

    #region Properties

    public override List<List<string>> Args => [StringVal];
    public override string Command => "stringtoutf8";
    public override bool MustUseReturnValue => true;
    public override string Returns => StringScriptVariable.ShortName_Plain;
    public override string Syntax => "StringToUTF8(StringScriptCommand, IgnoreBRbool)";

    #endregion

    #region Methods

    public override DoItFeedback DoIt(VariableCollection varCol, SplittedAttributesFeedback attvar, ScriptProperties scp) => new(attvar.ValueStringGet(0).StringtoUtf8());

    #endregion
}
