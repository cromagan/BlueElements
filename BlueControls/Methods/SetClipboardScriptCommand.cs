// Licensed under AGPL-3.0; see License.md for disclaimer and details.

using BlueScript.Classes;
using BlueScript.Enums;
using BlueScript.ScriptVariables;

namespace BlueScript.ScriptCommands;

internal class SetClipboardScriptCommand : ScriptCommand {

    #region Properties

    public override List<List<string>> Args => [StringVal];

    public override string Command => "setclipboard";
    public override string Description => "Speichert den Text im Clipboard.";

    public override ScriptCommandType ScriptCommandLevel => ScriptCommandType.ManipulatesUser;

    public override string Syntax => "SetClipboardScriptCommand(Text);";

    #endregion

    #region Methods

    public override DoItFeedback DoIt(VariableCollection varCol, SplittedAttributesFeedback attvar, ScriptProperties scp) {
        var vs = attvar.ValueStringGet(0);
        if (!CopytoClipboard(vs)) {
            return new DoItFeedback("Fehler beim Kopieren in die Zwischenablage.", false);
        }

        return DoItFeedback.Null();
    }

    #endregion
}