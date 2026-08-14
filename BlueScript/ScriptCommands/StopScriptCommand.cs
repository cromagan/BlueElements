// Licensed under AGPL-3.0; see License.md for disclaimer and details.

namespace BlueScript.ScriptCommands;

public class StopScriptCommand : ScriptCommand {

    #region Properties

    public override string Command => "stop";
    public override string Description => "Beendet die Ausführung im Testmodus.";
    public override string StartSequence => string.Empty;

    public override string Syntax => "StopScriptCommand;";

    #endregion

    #region Methods

    public override DoItFeedback DoIt(VariableCollection varCol, SplittedAttributesFeedback attvar, ScriptProperties scp) {

        if (!scp.ProduktivPhase) { return new DoItFeedback("=== STOP ===", true); }
        return DoItFeedback.Null();
    }

    #endregion
}