// Licensed under AGPL-3.0; see License.md for disclaimer and details.

namespace BlueScript.Methods;

internal class Method_Number : Method {

    #region Properties

    public override List<List<string>> Args => [StringVal, FloatVal];
    public override string Command => "number";
    public override string Description => "Gibt den Text als Zahl zurück. Fall dies keine gültige Zahl ist, wird NaN-Value zurückgegeben.";
    public override bool MustUseReturnValue => true;
    public override string Returns => VariableDouble.ShortName_Plain;
    public override string Syntax => "Number(string, NaNValue)";

    #endregion

    #region Methods

    public override DoItFeedback DoIt(VariableCollection varCol, SplittedAttributesFeedback attvar, ScriptProperties scp, LogData ld) {
        if (Converter.DoubleTryParse(attvar.ValueStringGet(0), out var dbl)) {
            return new DoItFeedback(dbl);
        }

        return new DoItFeedback(attvar.ValueNumGet(1));
    }

    #endregion
}