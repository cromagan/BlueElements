// Licensed under AGPL-3.0; see License.md for disclaimer and details.

namespace BlueScript.ScriptCommands;

internal class DirectoryExistsScriptCommand : ScriptCommand {

    #region Properties

    public override List<List<string>> Args => [StringVal];
    public override string Command => "directoryexists";
    public override string Description => "Prüft, ob ein Verzeichnis existiert";
    public override ScriptCommandType ScriptCommandLevel => ScriptCommandType.LongTime;
    public override bool MustUseReturnValue => true;
    public override string Returns => BoolScriptVariable.ShortName_Plain;
    public override string Syntax => "DirectoryExists(FilePath)";

    #endregion

    #region Methods

    public override DoItFeedback DoIt(VariableCollection varCol, SplittedAttributesFeedback attvar, ScriptProperties scp) {
        var pf = attvar.ValueStringGet(0);

        if (!pf.IsValidFilePath()) {
            return new DoItFeedback("Dateipfad ungültig: " + pf, true);
        }
        return new DoItFeedback(IO.DirectoryExists(pf));
    }

    #endregion
}