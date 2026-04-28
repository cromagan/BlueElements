// Licensed under AGPL-3.0; see License.md for disclaimer and details.

using BlueControls.Classes;
using BlueControls.Forms;
using BlueScript.Classes;
using BlueScript.Enums;
using BlueScript.Methods;
using BlueScript.Variables;
using BlueTable.AdditionalScriptMethods;
using BlueTable.Classes;
using System.Collections.Generic;

namespace BlueControls.AdditionalScriptMethods;

public class Method_ShowFormulaForm : Method {

    #region Properties

    public override List<List<string>> Args => [StringVal, Method_TableGeneric.RowVar, BoolVal, BoolVal];
    public override string Command => "showformulaform";
    public override string Description => "Öffnet ein Formular-Fenster.\r\n" +
        "  1. Dateiname (String) - Pfad zur Formular-Datei\r\n" +
        "  2. Zeile (RowItem) - Darf Null sein\r\n" +
        "  3. IsModal (Bool) - Ob das Fenster modal angezeigt werden soll\r\n" +
        "  4. TopMost (Bool) - Ob das Fenster im Vordergrund bleiben soll";
    public override int LastArgMinCount => 0;
    public override MethodType MethodLevel => MethodType.GUI;
    public override string Syntax => "ShowFormulaForm(Dateiname, Zeile, IsModal, TopMost);";

    #endregion

    #region Methods

    public override DoItFeedback DoIt(VariableCollection varCol, SplittedAttributesFeedback attvar, ScriptProperties scp, LogData ld) {
        var filename = attvar.ValueStringGet(0);
        var row = attvar.ValueRowGet(1);
        var isModal = attvar.ValueBoolGet(2);
        var topMost = attvar.ValueBoolGet(3);

        var form = new ConnectedFormulaForm(filename, string.Empty);
        form.TopMost = topMost;

        if (row is { IsDisposed: false }) {
            form.SetRow(row);
        }

        if (isModal) {
            form.ShowDialog();
        } else {
            FormManager.RegisterForm(form);
            form.Show();
        }

        return DoItFeedback.Null();
    }

    #endregion
}
