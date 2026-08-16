// Licensed under AGPL-3.0; see License.md for disclaimer and details.

namespace BlueScript.ScriptCommands;

internal class SetIfHasValueScriptCommand : ScriptCommand {

    #region Properties

    public override List<List<string>> Args => [[StringScriptVariable.ShortName_Variable, ListOfStringsScriptVariable.ShortName_Variable, DoubleScriptVariable.ShortName_Variable, BoolScriptVariable.ShortName_Variable], [ScriptVariable.Any_Plain]];
    public override string Command => "setifhasvalue";
    public override string Description => "Diese Routine setzt den ersten Wert, der keinen Fehler verursacht und einen Wert enthält in die erste Variable.\r\nDabei müssen die Datentypen übereinstimmen.\r\nFalls einer der Werte ein Variable ist, die nicht existiert, wird diese einfach übergangen.\r\nAls 'kein Wert' wird bei Zahlen ebenfalls 0 gewertet.\r\nListen, die einen Eintrag haben (auch wenn dessen Wert leer ist), zählt nicht als kein Eintrag.";
    public override LastArgMinCountTypeScriptCommand LastArgMinCount => LastArgMinCountTypeScriptCommand.MinOnce;
    public override string Syntax => "SetIfHasValueScriptCommand(Variable, Werte, ...);";

    #endregion

    #region Methods

    public override DoItFeedback DoIt(VariableCollection varCol, SplittedAttributesFeedback attvar, ScriptProperties scp) {
        if (attvar.ReadOnly(0)) { return DoItFeedback.Schreibgschützt(); }

        for (var z = 1; z < attvar.Attributes.Count; z++) {
            if (attvar.Attributes[z] is UnknownScriptVariable) { continue; }
            if (attvar.MyClassId(z) != attvar.MyClassId(0)) { return new DoItFeedback("Variablentyp zur Ausgangsvariable unterschiedlich.", true); }

            switch (attvar.Attributes[z]) {
                case DoubleScriptVariable vf:
                    if (vf.ValueNum != 0) {
                        return attvar.ValueNumSet(0, vf.ValueNum) ?? DoItFeedback.Null();
                    }
                    break;

                case StringScriptVariable vs:
                    if (!string.IsNullOrEmpty(vs.ValueString)) {
                        return attvar.ValueStringSet(0, vs.ValueString) ?? DoItFeedback.Null();
                    }
                    break;

                case BoolScriptVariable vb:
                    if (attvar.ValueBoolSet(0, vb.ValueBool) is { } dif3) { return dif3; }
                    return DoItFeedback.Null();

                case ListOfStringsScriptVariable vl:
                    if (attvar.ValueListStringSet(0, vl.ValueList) is { } dif4) { return dif4; }
                    return DoItFeedback.Null();
            }
        }

        return DoItFeedback.Null();
    }

    #endregion
}