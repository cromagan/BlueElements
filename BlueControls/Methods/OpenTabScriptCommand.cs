// Licensed under AGPL-3.0; see License.md for disclaimer and details.

using BlueScript.Classes;
using BlueScript.Enums;
using BlueScript.ScriptVariables;

namespace BlueScript.ScriptCommands;

internal class OpenTabScriptCommand : ScriptCommand {

    #region Properties

    public override List<List<string>> Args => [TableVar];
    public override string Command => "opentab";
    public override string Description => "Öffent einen neuen Tab in allen TableViews.";
    public override ScriptCommandType ScriptCommandLevel => ScriptCommandType.GUI;
    public override string Syntax => "OpenTabScriptCommand(Table);";

    #endregion

    #region Methods

    public override DoItFeedback DoIt(VariableCollection varCol, SplittedAttributesFeedback attvar, ScriptProperties scp) {
        if (attvar.Attributes[0] is not ScriptVariables.TableScriptVariable vtb || vtb.ValueTable is not { IsDisposed: false } tb) {
            return new DoItFeedback("Tabelle nicht vorhanden", true);
        }

        if (string.IsNullOrWhiteSpace(tb.Caption)) {
            if (tb is TableFile tbf) {
                return new DoItFeedback($"Die Benennung der Tabelle '{tbf.Filename.FileNameWithSuffix()}' fehlt.", true);
            }

            return new DoItFeedback("Die Benennung der Tabelle fehlt.", true);
        }

        foreach (var thisForm in FormManager.Forms) {
            if (thisForm is TableViewForm tbf && tbf.TabExists(tb.Caption) is null) {

                if (!scp.ProduktivPhase) { return DoItFeedback.TestModusInaktiv(); }
                tbf.AddTabPage(tb.Caption);
            }
        }

        return DoItFeedback.Null();
    }

    #endregion
}