// Licensed under AGPL-3.0; see License.md for disclaimer and details.

namespace BlueScript.Methods;


internal class Method_Sqrt : Method {

    #region Properties

    public override List<List<string>> Args => [FloatVal];

    public override string Command => "sqrt";

    public override string Description => "Berechnet die Quadartwurzel.";

    public override bool MustUseReturnValue => true;
    public override string Returns => VariableDouble.ShortName_Plain;
    public override string Syntax => "Sqrt(Number)";

    #endregion

    #region Methods

    public override DoItFeedback DoIt(VariableCollection varCol, SplittedAttributesFeedback attvar, ScriptProperties scp, LogData ld) => new(Math.Sqrt(attvar.ValueNumGet(0)));

    #endregion
}