// Licensed under AGPL-3.0; see License.md for disclaimer and details.

using BlueControls.Editoren;
using BlueScript.Classes;
using BlueScript.Enums;
using BlueScript.Variables;
using BlueTable.AdditionalScriptMethods;

namespace BlueControls.AdditionalScriptMethods;

public class Method_EditRow : Method_TableGeneric {

    #region Properties

    public override List<List<string>> Args => [RowVar];
    public override string Command => "editrow";

    public override string Description => "Öffnet den Bearbeiten-Dialog der Zeile.\r\n" +
            "Die eigene Zeile kann nur bearbeitet werden, wenn das Skript ReadOnly ist.";

    public override LastArgMinCountType LastArgMinCount => LastArgMinCountType.MinOnce;
    public override MethodType MethodLevel => MethodType.ManipulatesUser;

    public override string Syntax => "EditRow(Row);";

    #endregion

    #region Methods

    public override DoItFeedback DoIt(VariableCollection varCol, SplittedAttributesFeedback attvar, ScriptProperties scp, LogData ld) {
        if (attvar.ValueRowGet(0) is not { IsDisposed: false } row) { return new DoItFeedback("Zeile nicht gefunden", true, ld); }
        if (row.Table is not { IsDisposed: false } tb) { return new DoItFeedback("Fehler in der Zeile", true, ld); }

        var f = tb.IsGenericEditable(false);
        if (!string.IsNullOrEmpty(f)) {
            return new DoItFeedback($"Tabellensperre: {f}", true, ld);
        }

        if (row == BlockedRow(scp)) {
            MessageBox.Show("Bearbeitung aktuell nicht möglich.", ImageCode.Warnung, "OK");
            return new DoItFeedback("Die Zeile kann aktuell nicht bearbeitet werden.", false, ld);
        }

        if (!scp.ProduktivPhase) { return DoItFeedback.TestModusInaktiv(ld); }

        row.Edit(typeof(RowEditor), true);

        return DoItFeedback.Null();
    }

    #endregion
}