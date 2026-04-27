// Licensed under AGPL-3.0; see License.md for disclaimer and details.

namespace BlueScript.Methods;


internal class Method_Length : Method {

    #region Properties

    public override List<List<string>> Args => [StringVal];

    public override string Command => "length";

    public override string Description => "Gibt die Anzahl der Zeichen des Strings zurück";

    public override bool MustUseReturnValue => true;
    public override string Returns => VariableDouble.ShortName_Plain;
    public override string Syntax => "Length(String)";

    #endregion

    #region Methods

    public override DoItFeedback DoIt(VariableCollection varCol, SplittedAttributesFeedback attvar, ScriptProperties scp, LogData ld) => new(attvar.ValueStringGet(0).Length);

    #endregion
}