// Licensed under AGPL-3.0; see License.md for disclaimer and details.

using BlueControls.Controls;
using BlueTable.Interfaces;

namespace BlueControls.ControlStrategies;

/// <summary>
/// Zeigt nichts an: Ein einfacher Klick in die Zelle setzt den nächsten der
/// auswählbaren Werte der Spalte ein (Dropdown-Items und ggf. die Werte der
/// anderen Zellen). Nach dem letzten Wert folgt wieder der erste — bei
/// MinTextLength 0 dazwischen ein Leerwert.
/// </summary>
public class CycleValueControlStrategy : ControlStrategy, IHasColumn {

    #region Properties

    public static string ClassId => "CycleValue";

    public ColumnItem? Column { get; set; }

    public override string Description => "Zeigt nichts an: Ein Klick in die Zelle setzt den nächsten der auswählbaren Werte ein.";

    public override bool IsInstantAction => true;

    public override string KeyName => ClassId;

    protected override System.Windows.Forms.Control? ControlCore => null;

    #endregion

    #region Methods

    /// <summary>
    /// Die Spalte braucht auswählbare Werte und darf nicht mehrzeilig sein.
    /// </summary>
    public override string ErrorReason() {
        if (Column is not { IsDisposed: false } column) { return string.Empty; }
        if (!column.MayHaveDropDown()) { return ColumnErrorConstants.NoDropdownItems; }
        if (column.MultiLine) { return ColumnErrorConstants.NoMultilineAllowed; }
        return string.Empty;
    }

    public override string ReadableText() => "Zyklus-Knopf";

    public override void SubscribeEvents() { }

    public override QuickImage? SymbolForReadableText() => QuickImage.Get(ImageCode.Refresh);

    public override void UnsubscribeEvents() { }

    protected override void ApplyStyle() { }

    protected override void CreateControlCore() { }

    /// <summary>
    /// Setzt den nächsten der auswählbaren Werte in die Zelle: Aus einem
    /// Leerwert wird der erste Wert, danach jeweils der folgende. Nach dem
    /// letzten Wert folgt der erste — bei MinTextLength 0 vorher ein Leerwert.
    /// </summary>
    protected override void ExecuteInstantAction(ColumnItem column, RowItem row) {
        var items = CycleValuesOf(column);
        if (items.Count == 0) {
            TableView.NotEditableInfo("Die Spalte enthält keine auswählbaren Werte.");
            return;
        }

        var nextIndex = IndexOfItem(items, row.CellGetString(column)) + 1;

        if (nextIndex > 0 && nextIndex < items.Count) {
            row.CellSet(column, items[nextIndex], "Zyklus-Knopf");
            return;
        }

        if (nextIndex >= items.Count && column.MinTextLength == 0) {
            row.CellSet(column, string.Empty, "Zyklus-Knopf");
            return;
        }

        row.CellSet(column, items[0], "Zyklus-Knopf");
    }

    protected override void ForceWriteBackValue() { }

    protected override void SetValueToControlInternal(string value) { }

    /// <summary>
    /// Auswählbare Werte der Spalte: Dropdown-Items und — wenn erlaubt —
    /// die Werte der anderen Zellen derselben Spalte.
    /// </summary>
    private static List<string> CycleValuesOf(ColumnItem column) {
        List<string> items = [.. column.DropDownItems];
        if (column.ShowValuesOfOtherCellsInDropdown) { items.AddRange(column.Contents()); }
        return items.SortedDistinctList();
    }

    /// <summary>
    /// Index des Wertes in den auswählbaren Werten; -1, wenn er fehlt.
    /// </summary>
    private static int IndexOfItem(List<string> items, string value) {
        for (var i = 0; i < items.Count; i++) {
            if (string.Equals(items[i], value, StringComparison.OrdinalIgnoreCase)) { return i; }
        }
        return -1;
    }

    #endregion
}