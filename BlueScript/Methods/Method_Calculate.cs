// Licensed under AGPL-3.0; see License.md for disclaimer and details.

using BlueBasics.ClassesStatic;
using BlueScript.Classes;
using BlueScript.Enums;
using BlueScript.Variables;
using System.Collections.Generic;

namespace BlueScript.Methods;

internal class Method_Calculate : Method {

    #region Properties

    public override List<List<string>> Args => [StringVal, FloatVal];
    public override string Command => "calculate";
    public override string Description => "Berechet die Formel im String. Falls die Berechung fehlschlägt, wird NaN-Value zurückgegeben.";
    public override bool MustUseReturnValue => true;
    public override string Returns => VariableDouble.ShortName_Plain;
    public override string Syntax => "Calculate(string, NaNValue)";

    #endregion

    #region Methods

    public override DoItFeedback DoIt(VariableCollection varCol, SplittedAttributesFeedback attvar, ScriptProperties scp, LogData ld) {
        if (MathFormulaParser.Ergebnis(attvar.ValueStringGet(0)) is { } dbl) {
            return new DoItFeedback(dbl);
        }

        return new DoItFeedback(attvar.ValueNumGet(1));
    }

    #endregion
}