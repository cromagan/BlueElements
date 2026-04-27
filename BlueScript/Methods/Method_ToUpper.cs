// Licensed under AGPL-3.0; see License.md for disclaimer and details.

namespace BlueScript.Methods;


internal class Method_ToUpper : Method {

    #region Properties

    public override List<List<string>> Args => [StringVal];
    public override string Command => "toupper";
    public override string Description => "Gibt den Text in Großbuchstaben zurück";
    public override bool MustUseReturnValue => true;
    public override string Returns => VariableString.ShortName_Plain;
    public override string Syntax => "ToUpper(OriginalString)";

    #endregion

    #region Methods

    public override DoItFeedback DoIt(VariableCollection varCol, SplittedAttributesFeedback attvar, ScriptProperties scp, LogData ld) => new(attvar.ValueStringGet(0).ToUpperInvariant());

    #endregion
}