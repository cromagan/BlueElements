// Licensed under MIT; see License.md for disclaimer, details, and extended user conditions.

namespace BlueScript.ScriptCommands;

/// <summary>
/// Prüft, ob eine Datei existiert
/// </summary>
internal class FileExistsScriptCommand : ScriptCommand {

    #region Properties

    public override List<List<string>> Args => [StringVal];
    public override string Command => "fileexists";
    public override bool MustUseReturnValue => true;
    public override string Returns => BoolScriptVariable.ShortName_Plain;
    public override ScriptCommandType ScriptCommandLevel => ScriptCommandType.LongTime;
    public override string Syntax => "FileExists(FilePath)";

    #endregion

    #region Methods

    public override DoItFeedback DoIt(VariableCollection varCol, SplittedAttributesFeedback attvar, ScriptProperties scp) {
        var filn = attvar.ValueStringGet(0);

        return !filn.IsValidFilepathAndName()
            ? new DoItFeedback("Dateinamen-Fehler!", true)
            : new DoItFeedback(IO.FileExists(filn));
    }

    #endregion
}
