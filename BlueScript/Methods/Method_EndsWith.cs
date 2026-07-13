// Licensed under AGPL-3.0; see License.md for disclaimer and details.

namespace BlueScript.Methods;

internal class Method_EndsWith : Method {

    #region Properties

    public override List<List<string>> Args => [StringVal, BoolVal, StringVal];
    public override string Command => "endswith";
    public override string Description => "Prüft, ob der String mit einem der angegeben Strings endet.";
    public override LastArgMinCountType LastArgMinCount => LastArgMinCountType.MinOnce;
    public override bool MustUseReturnValue => true;
    public override string Returns => VariableBool.ShortName_Plain;
    public override string Syntax => "EndsWith(String, CaseSensitive, Value1, Value2, ...)";

    #endregion

    #region Methods

    public override DoItFeedback DoIt(VariableCollection varCol, SplittedAttributesFeedback attvar, ScriptProperties scp) {
        for (var z = 2; z < attvar.Attributes.Count; z++) {
            if (attvar.ValueBoolGet(1)) {
                if (attvar.ValueStringGet(0).EndsWith(attvar.ValueStringGet(z), StringComparison.Ordinal)) {
                    return DoItFeedback.Wahr();
                }
            } else {
                if (attvar.ValueStringGet(0).EndsWith(attvar.ValueStringGet(z), StringComparison.OrdinalIgnoreCase)) {
                    return DoItFeedback.Wahr();
                }
            }
        }
        return DoItFeedback.Falsch();
    }

    #endregion
}