// Licensed under AGPL-3.0; see License.md for disclaimer and details.

using BlueControls.Classes;
using BlueScript.Classes;
using BlueScript.Enums;
using BlueScript.Variables;
using BlueTable.AdditionalScriptVariables;
using static BlueTable.AdditionalScriptMethods.Method_TableGeneric;

namespace BlueControls.AdditionalScriptMethods;

internal class Method_OpenTab : Method {

    #region Properties

    public override List<List<string>> Args => [TableVar];
    public override string Command => "opentab";
    public override string Description => "Öffent einen neuen Tab in allen TableViews.";
    public override MethodType MethodLevel => MethodType.GUI;
    public override string Syntax => "OpenTab(Table);";

    #endregion

    #region Methods

    public override DoItFeedback DoIt(VariableCollection varCol, SplittedAttributesFeedback attvar, ScriptProperties scp, LogData ld) {
        if (attvar.Attributes[0] is not VariableTable vtb || vtb.Table is not { IsDisposed: false } tb) {
            return new DoItFeedback("Tabelle nicht vorhanden", true, ld);
        }

        if (string.IsNullOrWhiteSpace(tb.Caption)) {
            if (tb is TableFile tbf) {
                return new DoItFeedback($"Die Benennung der Tabelle '{tbf.Filename.FileNameWithSuffix()}' fehlt.", true, ld);
            }

            return new DoItFeedback("Die Benennung der Tabelle fehlt.", true, ld);
        }

        foreach (var thisForm in FormManager.Forms) {
            if (thisForm is TableViewForm tbf && tbf.TabExists(tb.Caption) is null) {
                if (!scp.ProduktivPhase) { return DoItFeedback.TestModusInaktiv(ld); }
                tbf.AddTabPage(tb.Caption);
            }
        }

        return DoItFeedback.Null();
    }

    #endregion
}