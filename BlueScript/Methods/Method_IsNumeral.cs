// Licensed under AGPL-3.0; see License.md for disclaimer and details.

using BlueBasics;
using BlueScript.Classes;
using BlueScript.Enums;
using BlueScript.Variables;
using System.Collections.Generic;

namespace BlueScript.Methods;


internal class Method_IsNumeral : Method {

    #region Properties

    public override List<List<string>> Args => [[VariableString.ShortName_Plain, VariableDouble.ShortName_Plain]];
    public override string Command => "isnumeral";
    public override string Description => "Prüft, ob der Inhalt der Variable eine gültige Zahl ist. ";
    public override bool MustUseReturnValue => true;
    public override string Returns => VariableBool.ShortName_Plain;
    public override string Syntax => "isNumeral(Value)";

    #endregion

    #region Methods

    public override DoItFeedback DoIt(VariableCollection varCol, SplittedAttributesFeedback attvar, ScriptProperties scp, LogData ld) {
        if (attvar.Attributes[0] is VariableDouble) { return DoItFeedback.Wahr(); }
        if (attvar.Attributes[0] is VariableString vs) {
            if (vs.ValueString.IsNumeral()) { return DoItFeedback.Wahr(); }
        }
        return DoItFeedback.Falsch();
    }

    #endregion
}