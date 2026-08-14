// Licensed under AGPL-3.0; see License.md for disclaimer and details.

namespace BlueScript.ScriptCommands;

internal class DecryptScriptCommand : ScriptCommand {

    #region Properties

    public override List<List<string>> Args => [StringVal, StringVal];
    public override string Command => "decrypt";
    public override string Description => "Entschlüsselt einen Text.";
    public override bool MustUseReturnValue => true;
    public override string Returns => StringScriptVariable.ShortName_Plain;
    public override string Syntax => "ScriptCommand(OriginalString, Schlüssel)";

    #endregion

    #region Methods

    public override DoItFeedback DoIt(VariableCollection varCol, SplittedAttributesFeedback attvar, ScriptProperties scp) {
        var wert = attvar.ValueStringGet(0).Decrypt(attvar.ValueStringGet(1));

        return wert is null ? new DoItFeedback("Entschlüsselung fehlgeschlagen.", true) : new DoItFeedback(wert);
    }

    #endregion
}