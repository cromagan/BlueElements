// Licensed under MIT; see License.md for disclaimer, details, and extended user conditions.

namespace BlueScript.ScriptCommands;

/// <summary>
/// Beendet eine Schleife oder Subroutine sofort.
/// Kann auch nur innerhalb von diesen verwendet werden.
/// </summary>
public class BreakScriptCommand : ScriptCommand {

    #region Fields

    public static readonly ScriptCommand Method = new BreakScriptCommand();

    #endregion

    #region Properties

    public override string Command => "break";
    public override ScriptCommandType ScriptCommandLevel => ScriptCommandType.Special;
    public override string StartSequence => string.Empty;

    public override string Syntax => "Break;";

    #endregion

    #region Methods

    public override DoItFeedback DoIt(VariableCollection varCol, SplittedAttributesFeedback attvar, ScriptProperties scp) =>
        new(false, true, false, string.Empty, null);

    #endregion
}
