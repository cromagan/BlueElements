// Licensed under AGPL-3.0; see License.md for disclaimer and details.

namespace BlueScript.ScriptCommands;


internal class SetIfExistsScriptCommand : ScriptCommand {

    #region Properties

    public override List<List<string>> Args => [[StringScriptVariable.ShortName_Variable, ListOfStringsScriptVariable.ShortName_Variable, DoubleScriptVariable.ShortName_Variable, BoolScriptVariable.ShortName_Variable], [ScriptVariable.Any_Plain]];
    public override string Command => "setifexists";
    public override string Description => "Diese Routine setzt den ersten Wert, der keinen Fehler verursacht in die erste Variable.\r\nDabei müssen die Datentypen übereinstimmen.\r\nFalls einer der Werte eine Variable ist, die nicht existiert, wird diese einfach übergangen.";
    public override LastArgMinCountTypeScriptCommand LastArgMinCount => LastArgMinCountTypeScriptCommand.MinOnce;
    public override string Syntax => "SetIfExistsScriptCommand(Variable, Werte, ...);";

    #endregion

    #region Methods

    public override DoItFeedback DoIt(VariableCollection varCol, SplittedAttributesFeedback attvar, ScriptProperties scp) {
        if (attvar.ReadOnly(0)) { return DoItFeedback.Schreibgschützt(); }

        for (var z = 1; z < attvar.Attributes.Count; z++) {
            if (attvar.Attributes[z] is UnknownScriptVariable) { continue; }

            if (attvar.MyClassId(z) != attvar.MyClassId(0)) { return new DoItFeedback("Variablentyp zur Ausgangsvariable unterschiedlich.", true); }

            switch (attvar.Attributes[z]) {
                case StringScriptVariable vs:
                    if (attvar.ValueStringSet(0, vs.ValueString) is { } dif) { return dif; }
                    return DoItFeedback.Null();

                case BoolScriptVariable vb:
                    if (attvar.ValueBoolSet(0, vb.ValueBool) is { } dif2) { return dif2; }
                    return DoItFeedback.Null();

                case DoubleScriptVariable vf:
                    if (attvar.ValueNumSet(0, vf.ValueNum) is { } dif3) { return dif3; }
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