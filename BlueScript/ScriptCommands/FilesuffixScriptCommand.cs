// Licensed under MIT; see License.md for disclaimer, details, and extended user conditions.

namespace BlueScript.ScriptCommands;

/// <summary>
/// Gibt den Dateisuffix zurück
/// </summary>
internal class FilesuffixScriptCommand : ScriptCommand {

    #region Properties

    public override List<List<string>> Args => [[StringScriptVariable.ClassId]];
    public override string Command => "filesuffix";
    public override bool MustUseReturnValue => true;
    public override string Returns => StringScriptVariable.ClassId;
    public override string Syntax => "FileSuffix(FilePathAndName)";

    #endregion

    #region Methods

    public override DoItFeedback DoIt(VariableCollection varCol, SplittedAttributesFeedback attvar, ScriptProperties scp) => new(attvar.ValueStringGet(0).FileSuffix());

    #endregion
}
