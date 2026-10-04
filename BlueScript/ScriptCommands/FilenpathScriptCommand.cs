// Licensed under MIT; see License.md for disclaimer, details, and extended user conditions.

namespace BlueScript.ScriptCommands;

/// <summary>
/// Gibt den Dateipad eines Dateistrings zurück, mit abschließenden \.
/// </summary>
internal class FilenpathScriptCommand : ScriptCommand {

    #region Properties

    public override List<List<string>> Args => [[StringScriptVariable.ClassId]];
    public override string Command => "filepath";
    public override bool MustUseReturnValue => true;
    public override string Returns => StringScriptVariable.ClassId;
    public override string Syntax => "Filepath(FilePathAndName)";

    #endregion

    #region Methods

    public override DoItFeedback DoIt(VariableCollection varCol, SplittedAttributesFeedback attvar, ScriptProperties scp) => new(attvar.ValueStringGet(0).FilePath());

    #endregion
}
