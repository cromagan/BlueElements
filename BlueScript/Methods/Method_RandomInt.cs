// Licensed under AGPL-3.0; see License.md for disclaimer and details.

namespace BlueScript.Methods;

internal class Method_RandomInt : Method {

    #region Properties

    public override List<List<string>> Args => [FloatVal];
    public override string Command => "randomint";
    public override string Description => "Gibt eine nicht negative Zufalls-Ganzzahl zurück,\rdie kleiner als das angegebene Maximum ist.";
    public override bool MustUseReturnValue => true;
    public override string Returns => VariableDouble.ShortName_Plain;
    public override string Syntax => "RandomInt(maxValue)";

    #endregion

    #region Methods

    public override DoItFeedback DoIt(VariableCollection varCol, SplittedAttributesFeedback attvar, ScriptProperties scp) => new(BlueBasics.ClassesStatic.Constants.GlobalRnd.Next(0, attvar.ValueIntGet(0)));

    #endregion
}