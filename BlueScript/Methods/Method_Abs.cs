// Licensed under AGPL-3.0; see License.md for disclaimer and details.

namespace BlueScript.Methods;


internal class Method_Abs : Method {

    #region Properties

    public override List<List<string>> Args => [FloatVal];

    public override string Command => "abs";

    public override string Description => "Gibt den absoluten Wert der Zahk zurück. Beispiel: abs(-20) ergibt 20. abs(20) ergibt ebenfalls 20.";

    public override bool MustUseReturnValue => true;
    public override string Returns => VariableDouble.ShortName_Plain;
    public override string Syntax => "Abs(Number)";

    #endregion

    #region Methods

    public override DoItFeedback DoIt(VariableCollection varCol, SplittedAttributesFeedback attvar, ScriptProperties scp) => new(Math.Abs(attvar.ValueNumGet(0)));

    #endregion
}