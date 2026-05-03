// Licensed under AGPL-3.0; see License.md for disclaimer and details.

using BlueControls.Classes.ItemCollectionList;
using BlueControls.Renderer;
using static BlueControls.Classes.ItemCollectionList.AbstractListItemExtension;

namespace BlueControls.Controls.FlexiControlStrategies;

public class FlexiStrategySwapListBox : FlexiStrategyBase {

    #region Fields

    private SwapListBox? _control;

    #endregion

    #region Properties

    public override System.Windows.Forms.Control? Control => _control;

    #endregion

    #region Methods

    public override void CreateControl() {
        _control = new SwapListBox();
        _control.UnCheck();
        SubscribeEvents();
    }

    public override void SetValue(string value) {
        if (_control is not null) { _control.Check(value.SplitAndCutByCr()); }
    }

    public override void StyleControl(FlexiStyleContext context, ColumnItem? column, string caption) {
        base.StyleControl(context, column, caption);
        if (_control is null) { return; }

        _control.SuggestionsClear();
        if (column is not { IsDisposed: false }) { return; }

        var r = TableView.RendererOf(column, Constants.Win11);
        var items = new List<AbstractListItem>();
        items.AddRange(ItemsOf(column, null, 10000, r));
        _control.SuggestionsAdd(items);

        switch (ColumnItem.UserEditDialogTypeInTable(column, false)) {
            case EditTypeTable.Textfeld:
                _control.AddAllowed = AddType.Text;
                break;

            case EditTypeTable.Listbox:
                _control.AddAllowed = AddType.OnlySuggests;
                break;

            default:
                _control.AddAllowed = AddType.None;
                break;
        }
    }

    public override void SubscribeEvents() {
        if (_control is null) { return; }
        _control.ItemCheckedChanged += SwapListBox_ItemCheckedChanged;
    }

    public override void UnsubscribeEvents() {
        if (_control is null) { return; }
        _control.ItemCheckedChanged -= SwapListBox_ItemCheckedChanged;
    }

    private void SwapListBox_ItemCheckedChanged(object? sender, System.EventArgs e) => OnValueChanged(string.Join('\r', _control!.Checked));

    #endregion
}