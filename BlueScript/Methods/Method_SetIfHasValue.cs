// Licensed under AGPL-3.0; see License.md for disclaimer and details.

namespace BlueScript.Methods;


internal class Method_SetIfHasValue : Method {

    #region Properties

    public override List<List<string>> Args => [[VariableString.ShortName_Variable, VariableListString.ShortName_Variable, VariableDouble.ShortName_Variable, VariableBool.ShortName_Variable], [Variable.Any_Plain]];
    public override string Command => "setifhasvalue";
    public override string Description => "Diese Routine setzt den ersten Wert, der keinen Fehler verursacht und einen Wert enthält in die erste Variable.\r\nDabei müssen die Datentypen übereinstimmen.\r\nFalls einer der Werte ein Variable ist, die nicht existiert, wird diese einfach übergangen.\r\nAls 'kein Wert' wird bei Zahlen ebenfalls 0 gewertet.\r\nListen, die einen Eintrag haben (auch wenn dessen Wert leer ist), zählt nicht als kein Eintrag.";
    public override LastArgMinCountType LastArgMinCount => LastArgMinCountType.MinOnce;
    public override string Syntax => "SetIfHasValue(Variable, Werte, ...);";

    #endregion

    #region Methods

    public override DoItFeedback DoIt(VariableCollection varCol, SplittedAttributesFeedback attvar, ScriptProperties scp, LogData ld) {
        if (attvar.ReadOnly(0)) { return DoItFeedback.Schreibgschützt(ld); }

        for (var z = 1; z < attvar.Attributes.Count; z++) {
            if (attvar.Attributes[z] is VariableUnknown) { continue; }
            if (attvar.MyClassId(z) != attvar.MyClassId(0)) { return new DoItFeedback("Variablentyp zur Ausgangsvariable unterschiedlich.", true, ld); }

            switch (attvar.Attributes[z]) {
                case VariableDouble vf:
                    if (vf.ValueNum != 0) {
                        return attvar.ValueNumSet(0, vf.ValueNum, ld) ?? DoItFeedback.Null();
                    }
                    break;

                case VariableString vs:
                    if (!string.IsNullOrEmpty(vs.ValueString)) {
                        return attvar.ValueStringSet(0, vs.ValueString, ld) ?? DoItFeedback.Null();
                    }
                    break;

                case VariableBool vb:
                    if (attvar.ValueBoolSet(0, vb.ValueBool, ld) is { } dif3) { return dif3; }
                    return DoItFeedback.Null();

                case VariableListString vl:
                    if (attvar.ValueListStringSet(0, vl.ValueList, ld) is { } dif4) { return dif4; }
                    return DoItFeedback.Null();
            }
        }

        return DoItFeedback.Null();
    }

    #endregion
}