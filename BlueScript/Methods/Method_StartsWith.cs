// Licensed under AGPL-3.0; see License.md for disclaimer and details.

using BlueScript.Classes;
using BlueScript.Enums;
using BlueScript.Variables;
using System.Collections.Generic;

namespace BlueScript.Methods;

internal class Method_StartsWith : Method {

    #region Properties

    public override List<List<string>> Args => [StringVal, BoolVal, StringVal];
    public override string Command => "startswith";
    public override string Description => "Prüft, ob der String mit einem der angegebenen Strings startet.";
    public override int LastArgMinCount => 1;
    public override bool MustUseReturnValue => true;
    public override string Returns => VariableBool.ShortName_Plain;
    public override string Syntax => "StartsWith(String, CaseSensitive, Value1, Value2, ...)";

    #endregion

    #region Methods

    public override DoItFeedback DoIt(VariableCollection varCol, SplittedAttributesFeedback attvar, ScriptProperties scp, LogData ld) {
        for (var z = 2; z < attvar.Attributes.Count; z++) {
            if (attvar.ValueBoolGet(1)) {
                if (attvar.ValueStringGet(0).StartsWith(attvar.ValueStringGet(z))) {
                    return DoItFeedback.Wahr();
                }
            } else {
                if (attvar.ValueStringGet(0).StartsWith(attvar.ValueStringGet(z), System.StringComparison.OrdinalIgnoreCase)) {
                    return DoItFeedback.Wahr();
                }
            }
        }
        return DoItFeedback.Falsch();
    }

    #endregion
}