// Licensed under MIT; see License.md for disclaimer, details, and extended user conditions.

namespace BlueScript.ScriptCommands;


/// <summary>
/// Beendet das Skript oder Unterskript ohne Fehler und setzt den Rückgabewert für Call-Routinen.
/// </summary>
internal class ReturnScriptCommand : ScriptCommand {

    #region Properties

    public override List<List<string>> Args => [StringVal];
    public override string Command => "return";
    public override string StartSequence => string.Empty;

    public override string Syntax => "ReturnScriptCommand \"ReturnValue\";";

    #endregion

    #region Methods

    public override DoItFeedback DoIt(VariableCollection varCol, SplittedAttributesFeedback attvar, ScriptProperties scp) =>
        new(false, false, true, string.Empty, attvar.Attributes[0]);

    #endregion
}
