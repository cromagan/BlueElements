// Licensed under AGPL-3.0; see License.md for disclaimer and details.

using BlueControls.Controls;
using BlueScript.Classes;
using BlueScript.Enums;
using BlueScript.ScriptVariables;
using System.Windows.Forms;

namespace BlueScript.ScriptCommands;

public class BlinkScriptCommand : TableGenericScriptCommand {

    #region Properties

    public override List<List<string>> Args => [StringVal, RowVar, StringVal];

    public override string Command => "blink";

    public override string Description => "Lässt die Zelle (Spalte in Zeile) in allen sichtbaren Tabellen- und Formular-Ansichten dreimal in der Farbe aufblinken.\r\nDient dazu, Änderungen eines Skriptes sichtbar zu machen.\r\nDie Tabelle wird aus der Zeile ermittelt.\r\nFarbe als Hex (#RRGGBB).";

    public override ScriptCommandType ScriptCommandLevel => ScriptCommandType.Standard;

    public override string Syntax => "Blink(Column, Row, Color);";

    #endregion

    #region Methods

    public override DoItFeedback DoIt(VariableCollection varCol, SplittedAttributesFeedback attvar, ScriptProperties scp) {
        if (attvar.ValueRowGet(1) is not { IsDisposed: false } row) { return new DoItFeedback("Zeile nicht gefunden", true); }
        if (row.Table is not { IsDisposed: false } tb) { return new DoItFeedback("Fehler in der Zeile", true); }

        var column = tb.Column[attvar.ValueStringGet(0)];
        if (column is not { IsDisposed: false }) { return new DoItFeedback("Spalte nicht gefunden: " + attvar.ValueStringGet(0), true); }

        if (!ColorTryParse(attvar.ValueStringGet(2), out var color)) { return new DoItFeedback("Farbe ungültig: " + attvar.ValueStringGet(2), true); }

        var openForms = OpenInvokerOrForms();
        if (openForms.Count == 0) { return DoItFeedback.Null(); }

        var invoker = openForms[0];
        if (invoker.InvokeRequired) {
            invoker.BeginInvoke(new Action<ColumnItem, RowItem, Color>(ShowForViews), column, row, color);
            return DoItFeedback.Null();
        }

        ShowForViews(column, row, color);

        return DoItFeedback.Null();
    }

    private static IEnumerable<Control> AllControls() {
        foreach (var form in OpenInvokerOrForms()) {
            if (form is not { IsHandleCreated: true, IsDisposed: false, Visible: true } f) { continue; }
            foreach (var c in ChildrenOf(f)) { yield return c; }
        }
    }

    private static IEnumerable<Control> ChildrenOf(Control parent) {
        foreach (Control c in parent.Controls) {
            yield return c;
            foreach (var sub in ChildrenOf(c)) { yield return sub; }
        }
    }

    private static List<System.Windows.Forms.Form> OpenInvokerOrForms() {
        List<System.Windows.Forms.Form> result = [];
        foreach (System.Windows.Forms.Form form in Application.OpenForms) {
            if (form is { IsHandleCreated: true, IsDisposed: false, Visible: true }) { result.Add(form); }
        }
        return result;
    }

    /// <summary>
    /// Sucht alle sichtbaren Ansichten der Zelle und legt darüber je ein Blink-Overlay. Muss auf dem UI-Thread laufen.
    /// </summary>
    private static void ShowForViews(ColumnItem column, RowItem row, Color color) {
        List<Control> all = [.. AllControls()];

        foreach (var tv in all.OfType<TableView>()) {
            if (tv.CellScreenRectangle(column, row) is { } rect) { BlinkOverlay.Blink(rect, color); }
        }

        foreach (var fc in all.OfType<FlexiControlForCell>()) {
            if (!fc.Visible || fc.IsDisposed) { continue; }
            if (fc.Column != column || fc.RowSingleOrNull() != row) { continue; }
            BlinkOverlay.Blink(fc.RectangleToScreen(fc.ClientRectangle), color);
        }
    }

    #endregion
}