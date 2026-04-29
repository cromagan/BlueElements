// Licensed under AGPL-3.0; see License.md for disclaimer and details.

using BlueControls.Classes;
using BlueScript.Classes;
using BlueScript.Enums;
using BlueScript.Variables;
using BlueTable.AdditionalScriptMethods;

namespace BlueControls.AdditionalScriptMethods;

public class Method_ShowFormulaForm : Method {

    #region Properties

    public override List<List<string>> Args => [StringVal, Method_TableGeneric.RowVar, StringVal, BoolVal, BoolVal];
    public override string Command => "showformulaform";

    public override string Description => "Öffnet ein Formular-Fenster.\r\n" +
        "  1. Dateiname (String) - Pfad zur Formular-Datei\r\n" +
        "  2. Zeile (RowItem) - Darf Null sein (evtl. RowEmpty-Variable benutzen)\r\n" +
        "  3. Modus\r\n" +
        "  4. IsModal (Bool) - Ob das Fenster modal angezeigt werden soll\r\n" +
        "  5. TopMost (Bool) - Ob das Fenster im Vordergrund bleiben soll";

    public override int LastArgMinCount => 0;
    public override MethodType MethodLevel => MethodType.GUI;
    public override string Syntax => "ShowFormulaForm(Dateiname, Zeile, Modus,  IsModal, TopMost);";

    #endregion

    #region Methods

    public override DoItFeedback DoIt(VariableCollection varCol, SplittedAttributesFeedback attvar, ScriptProperties scp, LogData ld) {
        var filename = attvar.ValueStringGet(0);
        var row = attvar.ValueRowGet(1);
        var mode = attvar.ValueStringGet(2);
        var isModal = attvar.ValueBoolGet(3);
        var topMost = attvar.ValueBoolGet(4);

        if (!filename.IsFormat(FormatHolder_FilepathAndName.Instance)) { return new DoItFeedback("Dateinamen-Fehler!", true, ld); }
        if (!IO.FileExists(filename)) { return new DoItFeedback("Datei existiert nicht", true, ld); }

        var form = new ConnectedFormulaForm(filename, mode);
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