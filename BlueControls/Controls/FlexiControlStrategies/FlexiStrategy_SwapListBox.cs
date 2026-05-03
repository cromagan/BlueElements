// Licensed under AGPL-3.0; see License.md for disclaimer and details.

using static BlueControls.Classes.ItemCollectionList.AbstractListItemExtension;

namespace BlueControls.Controls.FlexiControlStrategies;

public class FlexiStrategySwapListBox : IFlexiStrategy {
    private SwapListBox? _control;

    public System.Windows.Forms.Control? Control => _control;

    public void CreateControl(FlexiControl owner) {
        _control = new SwapListBox() {
            Enabled = owner.Enabled
        };
        _control.UnCheck();
        SubscribeEvents(owner);
    }

    public void SetValue(FlexiControl owner, string value) {
        if (_control is not null) { _control.Check(value.SplitAndCutByCr()); }
    }

    public void SubscribeEvents(FlexiControl owner) {
        if (_control is null) { return; }
        _control.ItemCheckedChanged += SwapListBox_ItemCheckedChanged;
    }

    public void UnsubscribeEvents() {
        if (_control is null) { return; }
        _control.ItemCheckedChanged -= SwapListBox_ItemCheckedChanged;
    }

    private void SwapListBox_ItemCheckedChanged(object? sender, System.EventArgs e) => ((FlexiControl)((SwapListBox)sender).Parent).ValueSet(string.Join('\r', ((SwapListBox)sender).Checked), false);
}