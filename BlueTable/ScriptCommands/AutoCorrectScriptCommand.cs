// Licensed under AGPL-3.0; see License.md for disclaimer and details.
using BlueScript.Classes;

namespace BlueScript.ScriptCommands;

internal class AutoCorrectScriptCommand : TableGenericScriptCommand {

    #region Properties

    public override List<List<string>> Args => [[ScriptVariable.Any_Variable]];
    public override string Command => "autocorrect";

    public override string Description => "Ändert den Wert der angegebenen Variablen so ab, wie es in die Zelle geschrieben werden würde.\r\n" +
        "Z.B: Autosort und Ersetzungen\r\n" +
        "Es können nur Variablen benutzt werden, die auch zu einer Spalte gehören.";

    public override LastArgMinCountTypeScriptCommand LastArgMinCount => LastArgMinCountTypeScriptCommand.MinOnce;

    public override string Syntax => "AutoCorrectScriptCommand(Column1, ...)";

    #endregion

    #region Methods

    public override DoItFeedback DoIt(VariableCollection varCol, SplittedAttributesFeedback attvar, ScriptProperties scp) {
        for (var n = 0; n < attvar.Attributes.Count; n++) {
            var column = Column(scp, attvar, n);
            if (column is not { IsDisposed: false }) { return new DoItFeedback("Spalte in Tabelle nicht gefunden.", true); }
            var columnVar = attvar.Attributes[n];

            if (columnVar is not { ReadOnly: false }) { return new DoItFeedback("Variable ist schreibgeschützt.", true); }
            if (!column.CanBeChangedByRules()) { return new DoItFeedback("Spalte nicht veränderbar.", true); }

            var s = string.Empty;
            switch (columnVar) {
                case DoubleScriptVariable vf:
                    s = vf.ValueNum.ToString1_5();
                    break;

                case ListOfStringsScriptVariable vl:
                    s = string.Join('\r', vl.ValueList);
                    break;

                case BoolScriptVariable vb:
                    s = vb.ValueBool.ToPlusMinus();
                    break;

                case StringScriptVariable vs:
                    s = vs.ValueString;
                    break;

                default:
                    Develop.DebugPrint("Typ nicht erkannt: " + columnVar.MyClassId);
                    break;
            }

            s = column.AutoCorrect(s, false);

            switch (columnVar) {
                case DoubleScriptVariable vf:
                    vf.ValueNum = DoubleParse(s);
                    break;

                case ListOfStringsScriptVariable vl:
                    vl.ValueList = [.. s.SplitByCr()];
                    break;

                case BoolScriptVariable vb:
                    vb.ValueBool = s.FromPlusMinus();
                    break;

                case StringScriptVariable vs:
                    vs.ValueString = s;
                    break;

                default:
                    Develop.DebugPrint("Typ nicht erkannt: " + columnVar.MyClassId);
                    break;
            }
        }

        return DoItFeedback.Null();
    }

    #endregion
}