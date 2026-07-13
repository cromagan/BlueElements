// Licensed under AGPL-3.0; see License.md for disclaimer and details.

namespace BlueScript.Methods;


internal class Method_Round : Method {

    #region Properties

    public override List<List<string>> Args => [FloatVal, FloatVal];
    public override string Command => "round";
    public override string Description => "Rundet den Zahlenwert mathematisch korrekt.";
    public override bool MustUseReturnValue => true;
    public override string Returns => VariableDouble.ShortName_Plain;
    public override string Syntax => "Round(Value, Nachkommastellen)";

    #endregion

    #region Methods

    public override DoItFeedback DoIt(VariableCollection varCol, SplittedAttributesFeedback attvar, ScriptProperties scp) {
        var n = (int)attvar.ValueNumGet(1);
        if (n < 0) { n = 0; }
        if (n > 10) { n = 10; }
        var val = Math.Round(attvar.ValueNumGet(0), n, MidpointRounding.AwayFromZero);
        return new DoItFeedback(val);
    }

    #endregion
}