// Licensed under AGPL-3.0; see License.md for disclaimer and details.

using BlueScript.Classes;
using BlueScript.Enums;
using BlueScript.Variables;

namespace BlueControls.AdditionalScriptMethods;

internal class Method_SetClipboard : Method {

    #region Properties

    public override List<List<string>> Args => [StringVal];

    public override string Command => "setclipboard";
    public override string Description => "Speichert den Text im Clipboard.";

    public override MethodType MethodLevel => MethodType.ManipulatesUser;

    public override string Syntax => "SetClipboard(Text);";

    #endregion

    #region Methods

    public override DoItFeedback DoIt(VariableCollection varCol, SplittedAttributesFeedback attvar, ScriptProperties scp, LogData ld) {
        var vs = attvar.ValueStringGet(0);
        if (!CopytoClipboard(vs)) {
            return new DoItFeedback("Fehler beim Kopieren in die Zwischenablage.", false, ld);
        }

        return DoItFeedback.Null();
    }

    #endregion
}