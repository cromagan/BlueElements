// Licensed under AGPL-3.0; see License.md for disclaimer and details.

using BlueControls.Classes.ItemCollectionList;
using BlueControls.EventArgs;
using BlueControls.Renderer;
using System.Windows.Forms;
using static BlueControls.Classes.ItemCollectionList.AbstractListItemExtension;

namespace BlueControls.Controls.FlexiControlStrategies;

public class FlexiStrategyComboBox : FlexiStrategyBase {

    #region Fields

    private ComboBox? _control;
    private EventHandler<NavigationDirectionEventArgs>? _navigateHandler;

    #endregion

    #region Properties

    public override System.Windows.Forms.Control? Control => _control;

    #endregion

    #region Methods

    public override void CreateControl() {
        _control = new ComboBox() { DropDownStyle = ComboBoxStyle.DropDownList };
        SubscribeEvents();
    }

    public override void SetValue(string value) {
        if (_control is not null) { _control.Text = value; }
    }

    public override void StyleControl(FlexiStyleContext context, ColumnItem? column, string caption) {
        base.StyleControl(context, column, caption);
        if (_control is null) { return; }

        var items = new List<AbstractListItem>();
        if (column is not null) {
            var r = TableView.RendererOf(context.OriginalColumn, Constants.Win11);
            items.AddRange(ItemsOf(column, null, 10000, r));
        }

        _control.RaiseChangeDelay = context.Delay;
        _control.ItemClear();
        _control.ItemEditAllowed = string.Equals(Generic.UserGroup, Constants.Administrator, StringComparison.OrdinalIgnoreCase);
        _control.ItemAddRange(items);

        if (column is { IsDisposed: false, EditableWithTextInput: true }) {
            _control.DropDownStyle = ComboBoxStyle.DropDown;
        } else {
            _control.DropDownStyle = ComboBoxStyle.DropDownList;
            if (_control[context.CurrentValue] == null) {
                OnValueChanged(string.Empty, true);
            }
        }
    }

    public override void SubscribeEvents() {
        if (_control is null) { return; }
        _control.TextChanged += ValueChanged_ComboBox;
        _navigateHandler = (_, e) => OnNavigateToNext(e.Direction);
        _control.NavigateToNext += _navigateHandler;
    }

    public override void UnsubscribeEvents() {
        if (_control is null) { return; }
        _control.TextChanged -= ValueChanged_ComboBox;
        if (_navigateHandler is not null) { _control.NavigateToNext -= _navigateHandler; }
    }

    private void ValueChanged_ComboBox(object? sender, System.EventArgs e) => OnValueChanged(_control!.Text);

    #endregion
}