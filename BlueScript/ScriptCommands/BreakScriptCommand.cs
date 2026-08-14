// Licensed under AGPL-3.0; see License.md for disclaimer and details.

namespace BlueScript.ScriptCommands;

public class BreakScriptCommand : ScriptCommand {

    #region Fields

    public static readonly ScriptCommand Method = new BreakScriptCommand();

    #endregion

    #region Properties

    public override string Command => "break";
    public override string Description => "Beendet eine Schleife oder Subroutine sofort.\r\nKann auch nur innerhalb von diesen verwendet werden.";
    public override ScriptCommandType ScriptCommandLevel => ScriptCommandType.Special;
    public override string StartSequence => string.Empty;

    public override string Syntax => "Break;";

    #endregion

    #region Methods

    public override DoItFeedback DoIt(VariableCollection varCol, SplittedAttributesFeedback attvar, ScriptProperties scp) =>
        new(false, true, false, string.Empty, null);

    #endregion
}