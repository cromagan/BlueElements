// Licensed under AGPL-3.0; see License.md for disclaimer and details.

using BlueControls.Classes.ItemCollectionList;
using BlueControls.EventArgs;

namespace BlueControls.Controls.FlexiControlStrategies;

/// <summary>
/// Strategy für das Dropdown-Auswahlmenü innerhalb einer <see cref="TableView" />.
/// Im Gegensatz zum früheren <c>FloatingInputBoxListBoxStyle</c> wird hier eine
/// <see cref="ListBox" /> mit <see cref="ListBoxAppearance.DropdownSelectbox" />
/// direkt als Inline-Control in der Tabelle verwendet. Das Control wird bewusst
/// größer dargestellt als das Zellfeld, sodass der optische Eindruck des alten
/// Dropdown-Menüs erhalten bleibt.
/// </summary>
public class FlexiStrategyDropDownListBox : FlexiStrategyBase {

    #region Fields

    private ListBox? _control;

    #endregion

    #region Properties

    public override System.Windows.Forms.Control? Control => _control;

    public override bool SupportsSuggestions => true;

    #endregion

    #region Methods

    /// <summary>
    /// Berechnet die für die ListBox benötigte Größe anhand der aktuell
    /// gesetzten <see cref="FlexiStrategyBase.ListItems" />. Die Methode
    /// wird von der TableView aufgerufen, um das Inline-Control passend
    /// zu dimensionieren (größer als das Zellfeld).
    /// </summary>
    public Size CalculateRequiredSize(int minWidth, int minHeight) {
        if (ListItems is null || ListItems.Count == 0) {
            return new Size(Math.Max(minWidth, 16), Math.Max(minHeight, 16));
        }

        var (biggestX, _, height, _) = ListItems.CanvasItemData(Design.Item_DropdownMenu);

        biggestX = Math.Max(biggestX, minWidth);
        height = Math.Max(height, minHeight);

        return new Size(biggestX, height);
    }

    public override void CreateControl() {
        _control = new ListBox();
    }

    public override void SubscribeEvents() {
        _control?.ItemClicked += ListBox_ItemClicked;
        _control?.LostFocus += Control_LostFocus;
    }

    public override void UnsubscribeEvents() {
        _control?.ItemClicked -= ListBox_ItemClicked;
        _control?.LostFocus -= Control_LostFocus;
    }

    protected override void ApplyStyle() {
        if (_control is null) { return; }

        _control.Appearance = ListBoxAppearance.DropdownSelectbox;
        _control.Translate = true;
        _control.AutoSort = AutoSort;
        _control.CheckBehavior = CheckBehavior;
        _control.RemoveAllowed = RemoveAllowed;
        _control.MoveAllowed = MoveAllowed;
        _control.AddAllowed = AddType.None;
        _control.ItemClear();
        if (ListItems is not null) { _control.ItemAddRange(ListItems); }
    }

    protected override void SetValueToControlInternal(string value) {
        if (_control is null) { return; }
        var values = value.SplitAndCutByCr();
        _control.Check(values, true);
    }

    private void Control_LostFocus(object? sender, System.EventArgs e) => OnLostFocus();

    private void ListBox_ItemClicked(object? sender, AbstractListItemEventArgs e) {
        OnValueChanged(e.Item.KeyName);
    }

    #endregion
}