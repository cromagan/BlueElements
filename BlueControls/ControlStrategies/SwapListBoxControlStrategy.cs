// Licensed under AGPL-3.0; see License.md for disclaimer and details.

using BlueControls.Controls;

namespace BlueControls.ControlStrategies;

public class SwapListBoxControlStrategy : ControlStrategy {

    #region Fields

    private SwapListBox? _control;

    #endregion

    #region Properties

    public static string ClassId => "SwapListBox";

    public override System.Windows.Forms.Control? Control => _control;

    public override string KeyName => ClassId;

    #endregion

    #region Methods

    public override void CreateControl() => _control = new SwapListBox();

    public override void SubscribeEvents() {
        _control?.ItemCheckedChanged += SwapListBox_ItemCheckedChanged;
        _control?.LostFocus += Control_LostFocus;
    }

    public override void UnsubscribeEvents() {
        _control?.ItemCheckedChanged -= SwapListBox_ItemCheckedChanged;
        _control?.LostFocus -= Control_LostFocus;
    }

    protected override void ApplyStyle() {
        if (_control is not { } c) { return; }

        c.SuggestionsClear();
        if (ListItems is not null) {
            var itemsToAdd = new List<ListItem>(ListItems);
            if (AutoSort) { itemsToAdd.Sort(); }
            c.SuggestionsAdd(itemsToAdd);
        }

        if (AddAllowed != AddType.None) {
            c.AddAllowed = AddAllowed;
        } else {
            c.AddAllowed = TextInputAllowed ? AddType.Text : AddType.None;
        }

        c.QuickInfo = QuickInfo;
    }

    protected override void SetValueToControlInternal(string value) => _control?.Check(value.SplitAndCutByCr());

    private void Control_LostFocus(object? sender, System.EventArgs e) => OnLostFocus();

    private void SwapListBox_ItemCheckedChanged(object? sender, System.EventArgs e) {
        if (_control is { } c) { OnValueChanged(string.Join('\r', c.Checked)); }
    }

    #endregion
}