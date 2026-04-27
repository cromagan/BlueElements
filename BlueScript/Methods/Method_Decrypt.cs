// Licensed under AGPL-3.0; see License.md for disclaimer and details.

namespace BlueScript.Methods;


internal class Method_Decrypt : Method {

    #region Properties

    public override List<List<string>> Args => [StringVal, StringVal];
    public override string Command => "decrypt";
    public override string Description => "Entschlüsselt einen Text.";
    public override bool MustUseReturnValue => true;
    public override string Returns => VariableString.ShortName_Plain;
    public override string Syntax => "Decrypt(OriginalString, Schlüssel)";

    #endregion

    #region Methods

    public override DoItFeedback DoIt(VariableCollection varCol, SplittedAttributesFeedback attvar, ScriptProperties scp, LogData ld) {
        var wert = attvar.ValueStringGet(0).Decrypt(attvar.ValueStringGet(1));

        return wert == null ? new DoItFeedback("Entschlüsselung fehlgeschlagen.", true, ld) : new DoItFeedback(wert);
    }

    #endregion
}