// Licensed under AGPL-3.0; see License.md for disclaimer and details.

namespace BlueScript.Methods;


internal class Method_Encrypt : Method {

    #region Properties

    public override List<List<string>> Args => [StringVal, StringVal];
    public override string Command => "encrypt";
    public override string Description => "Verschlüsselt einen Text.";
    public override bool MustUseReturnValue => true;
    public override string Returns => VariableString.ShortName_Plain;
    public override string Syntax => "Encrypt(OriginalString, Schlüssel)";

    #endregion

    #region Methods

    public override DoItFeedback DoIt(VariableCollection varCol, SplittedAttributesFeedback attvar, ScriptProperties scp, LogData ld) {
        var wert = attvar.ValueStringGet(0).Encrypt(attvar.ValueStringGet(1));

        return wert == null ? new DoItFeedback("Verschlüsselung fehlgeschlagen.", true, ld) : new DoItFeedback(wert);
    }

    #endregion
}