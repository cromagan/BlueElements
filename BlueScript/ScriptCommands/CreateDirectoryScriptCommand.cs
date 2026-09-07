// Licensed under MIT; see License.md for disclaimer, details, and extended user conditions.

using static BlueBasics.ClassesStatic.IO;

namespace BlueScript.ScriptCommands;

/// <summary>
/// Erstellt ein Verzeichnis, falls dieses nicht existert. Gibt TRUE zurück, erstellt wurde oder bereits existierte.
/// </summary>
internal class CreateDirectoryScriptCommand : ScriptCommand {

    #region Properties

    public override List<List<string>> Args => [StringVal];
    public override string Command => "directorycreate";

    public override string Returns => BoolScriptVariable.ShortName_Plain;
    public override ScriptCommandType ScriptCommandLevel => ScriptCommandType.LongTime;
    public override string Syntax => "CreateDirectory(Path)";

    #endregion

    #region Methods

    public override DoItFeedback DoIt(VariableCollection varCol, SplittedAttributesFeedback attvar, ScriptProperties scp) {
        var p = attvar.ValueStringGet(0).TrimEnd('\\');
        return CreateDirectory(p).IsSuccessful ? DoItFeedback.Wahr() : DoItFeedback.Falsch();
    }

    #endregion
}
