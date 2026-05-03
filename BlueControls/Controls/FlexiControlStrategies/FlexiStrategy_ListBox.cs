// Licensed under AGPL-3.0; see License.md for disclaimer and details.

using static BlueControls.Classes.ItemCollectionList.AbstractListItemExtension;

namespace BlueControls.Controls.FlexiControlStrategies;

public class FlexiStrategyListBox : IFlexiStrategy {
    private ListBox? _control;

    public System.Windows.Forms.Control? Control => _control;

    public void CreateControl(FlexiControl owner) {
        _control = new ListBox() {
            Enabled = owner.Enabled,
            CheckBehavior = CheckBehavior.MultiSelection
        };
        _control.ItemClear();
        SubscribeEvents(owner);
    }

    public void SetValue(FlexiControl owner, string value) {
        if (_control is not null) { _control.Check(value.SplitAndCutByCr(), true); }
    }

    public void SubscribeEvents(FlexiControl owner) {
        if (_control is null) { return; }
        _control.ItemCheckedChanged += ListBox_ItemCheckedChanged;
    }

    public void UnsubscribeEvents() {
        if (_control is null) { return; }
        _control.ItemCheckedChanged -= ListBox_ItemCheckedChanged;
    }

    private void ListBox_ItemCheckedChanged(object? sender, System.EventArgs e) => ((FlexiControl)((ListBox)sender).Parent).ValueSet(string.Join('\r', ((ListBox)sender).Checked), false);
}