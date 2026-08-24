// Licensed under AGPL-3.0; see License.md for disclaimer and details.

using BlueScript.Classes;
using BlueScript.Enums;
using BlueScript.ScriptVariables;

namespace BlueScript.ScriptCommands;

public class ShowFormulaFormScriptCommand : ScriptCommand {

    #region Properties

    public override List<List<string>> Args => [StringVal, RowVar, StringVal, BoolVal, BoolVal];
    public override string Command => "showformulaform";

    public override string Description => "Öffnet ein Formular-Fenster.\r\n" +
        "  1. Dateiname (StringScriptCommand) - Pfad zur Formular-Datei\r\n" +
        "  2. Zeile (RowItem) - Darf Null sein (evtl. RowEmpty-Variable benutzen)\r\n" +
        "  3. Modus\r\n" +
        "  4. IsModal (Bool) - Ob das Fenster modal angezeigt werden soll\r\n" +
        "  5. TopMost (Bool) - Ob das Fenster im Vordergrund bleiben soll";

    public override LastArgMinCountTypeScriptCommand LastArgMinCount => LastArgMinCountTypeScriptCommand.Optional;
    public override ScriptCommandType ScriptCommandLevel => ScriptCommandType.GUI;
    public override string Syntax => "ShowFormulaFormScriptCommand(Dateiname, Zeile, Modus,  IsModal, TopMost);";

    #endregion

    #region Methods

    public override DoItFeedback DoIt(VariableCollection varCol, SplittedAttributesFeedback attvar, ScriptProperties scp) {
        var filename = attvar.ValueStringGet(0);
        var row = attvar.ValueRowGet(1);
        var mode = attvar.ValueStringGet(2);
        var isModal = attvar.ValueBoolGet(3);
        var topMost = attvar.ValueBoolGet(4);

        if (!filename.IsValidFilepathAndName()) { return new DoItFeedback("Dateinamen-Fehler!", true); }
        if (!IO.FileExists(filename)) { return new DoItFeedback("Datei existiert nicht", true); }

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