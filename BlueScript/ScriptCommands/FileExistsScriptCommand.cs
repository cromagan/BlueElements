// Licensed under AGPL-3.0; see License.md for disclaimer and details.

namespace BlueScript.ScriptCommands;

internal class FileExistsScriptCommand : ScriptCommand {

    #region Properties

    public override List<List<string>> Args => [StringVal];
    public override string Command => "fileexists";
    public override string Description => "Prüft, ob eine Datei existiert";
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