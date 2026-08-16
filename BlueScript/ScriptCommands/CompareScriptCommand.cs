// Licensed under AGPL-3.0; see License.md for disclaimer and details.

namespace BlueScript.ScriptCommands;

internal class CompareScriptCommand : ScriptCommand {

    #region Properties

    public override List<List<string>> Args => [BoolVal, BoolVal, [StringScriptVariable.ShortName_Plain, DoubleScriptVariable.ShortName_Plain, BoolScriptVariable.ShortName_Plain]];
    public override string Command => "compare";

    public override string Description => "Diese Routine vergleicht Werte mit einander und gibt true zurück, wenn diese gleich sind. Dabei müssen die Datentypen übereinstimmen.\r\n" +
                                           "Bei IgnoreNullOrEmpty wird bei Zahlen ebenfalls 0 ignoriert";

    public override LastArgMinCountTypeScriptCommand LastArgMinCount => LastArgMinCountTypeScriptCommand.MinTwice;
    public override bool MustUseReturnValue => true;
    public override string Returns => BoolScriptVariable.ShortName_Plain;
    public override string Syntax => "Compare(IgnoreNullOrEmpty, CaseSensitive, Value1, ...);";

    #endregion

    #region Methods

    public override DoItFeedback DoIt(VariableCollection varCol, SplittedAttributesFeedback attvar, ScriptProperties scp) {
        var ignorenull = attvar.ValueBoolGet(0);
        var cases = attvar.ValueBoolGet(1);

        string? firstval = null;

        for (var z = 2; z < attvar.Attributes.Count; z++) {
            if (attvar.MyClassId(z) != attvar.MyClassId(2)) { return new DoItFeedback("Variablentypen unterschiedlich.", true); }

            var hasval = !ignorenull;
            var val = string.Empty;

            switch (attvar.Attributes[z]) {
                case DoubleScriptVariable vf:
                    if (!hasval && vf.ValueNum != 0) { hasval = true; }
                    val = vf.ValueForReplace;
                    break;

                case StringScriptVariable vs:
                    if (!hasval && !string.IsNullOrEmpty(vs.ValueString)) { hasval = true; }
                    val = vs.ValueForReplace;
                    break;

                case BoolScriptVariable vb:
                    hasval = true;
                    val = vb.ValueForReplace;
                    break;
            }

            if (hasval) {
                if (!cases) { val = val.ToUpperInvariant(); }
                firstval ??= val;

                if (val != firstval) { return DoItFeedback.Falsch(); }
            }
        }

        return DoItFeedback.Wahr();
    }

    #endregion
}