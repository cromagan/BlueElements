// Licensed under AGPL-3.0; see License.md for disclaimer and details.

using BlueControls.EventArgs;

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
        _control = new ComboBox();
    }

    public override void HandleCaptionClick() {
        _control?.Focus();
        _control?.ShowMenu(this, new System.Windows.Forms.MouseEventArgs(System.Windows.Forms.MouseButtons.None, 0, 0, 0, 0));
    }

    public override void SubscribeEvents() {
        _control?.TextChanged += ValueChanged_ComboBox;
        _navigateHandler = (_, e) => OnNavigateToNext(e.Direction);
        _control?.NavigateToNext += _navigateHandler;
        _control?.ItemRemoved += ComboBox_ItemRemoved;
        _control?.DropDownShowing += ComboBox_DropDownShowing;
    }

    public override void UnsubscribeEvents() {
        _control?.TextChanged -= ValueChanged_ComboBox;
        if (_navigateHandler is not null && _control is not null) { _control.NavigateToNext -= _navigateHandler; }
        _control?.ItemRemoved -= ComboBox_ItemRemoved;
        _control?.DropDownShowing -= ComboBox_DropDownShowing;
    }

    public override bool WasValueClicked() => _control?.WasThisValueClicked() ?? false;

    protected override void ApplyStyle() {
        _control?.DropDownStyle = TextInputAllowed ? System.Windows.Forms.ComboBoxStyle.DropDown : System.Windows.Forms.ComboBoxStyle.DropDownList;

        _control?.GetStyleFrom(this);

        _control?.ItemClear();
        if (ListItems is not null) { _control?.ItemAddRange(ListItems); }
        _control?.ItemEditAllowed = string.Equals(Generic.UserGroup, Constants.Administrator, StringComparison.OrdinalIgnoreCase);
        _control?.CustomContextMenuItems = CustomContextMenuItems;
        _control?.RaiseChangeDelay = RaiseChangeDelay;
        _control?.RemoveAllowed = RemoveAllowed;
        _control?.QuickInfo = QuickInfo;
    }

    protected override void SetValueToControlInternal(string value) {
        _control?.Text = value;
    }

    private void ComboBox_DropDownShowing(object? sender, System.EventArgs e) => OnDropDownShowing();

    private void ComboBox_ItemRemoved(object? sender, AbstractListItemEventArgs e) => OnItemRemoved(e);

    private void ValueChanged_ComboBox(object? sender, System.EventArgs e) => OnValueChanged(_control?.Text ?? string.Empty);

    #endregion
}