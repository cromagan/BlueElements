// Licensed under MIT; see License.md for disclaimer, details, and extended user conditions.

using BlueScript.EventArgs;

namespace BlueScript.ScriptCommands;

/// <summary>
/// Gibt eine Nachricht aus, z. B. im Skript-Editor. Nützlich zur Fehlersuche in Skripten.
/// </summary>
public class DebugPrintScriptCommand : ScriptCommand {

    #region Events

    public static event EventHandler<TextEventArgs>? LineAdded;

    #endregion

    #region Properties

    public override List<List<string>> Args => [StringVal];

    public override string Command => "debugprint";

    public override string Syntax => "DebugPrint(Text);";

    #endregion

    #region Methods

    public override DoItFeedback DoIt(VariableCollection varCol, SplittedAttributesFeedback attvar, ScriptProperties scp) {
        var txt = attvar.ValueStringGet(0);

        scp.DebugOutput.Add(txt);

        OnLineAdded(txt);

        return DoItFeedback.Null();
    }

    private static void OnLineAdded(string text) => LineAdded?.Invoke(null, new TextEventArgs(text));

    #endregion
}