// Licensed under AGPL-3.0; see License.md for disclaimer and details.

using BlueControls.Controls;
using BlueControls.EventArgs;

namespace BlueControls.ControlStrategies;

public class ComboBoxControlStrategy : ControlStrategy {

    #region Fields

    private ComboBox? _control;
    private EventHandler<NavigationDirectionEventArgs>? _navigateHandler;

    #endregion

    #region Properties

    public static string ClassId => "Combobox";

    protected override System.Windows.Forms.Control? ControlCore => _control;

    public override string KeyName => ClassId;

    public override bool SupportsSuggestions => true;
    public override bool SupportsTextEdit => true;

    #endregion

    #region Methods

    /// <summary>
    /// Vergrößert die Breite um den Dropdown-Knopf, damit der Zell-Text
    /// beim Edit nicht vom Knopf überdeckt wird.
    /// </summary>
    public override Size CalculateRequiredSize(int minWidth, int minHeight) =>
        new(minWidth + (_control?.btnDropDown.Width ?? 0), minHeight);

    public override void CreateControl() => _control = new ComboBox();

    public override void HandleCaptionClick() {
        _control?.Focus();
        _control?.ShowMenu(this, new System.Windows.Forms.MouseEventArgs(System.Windows.Forms.MouseButtons.None, 0, 0, 0, 0));
    }

    public override string ReadableText() => "Textfeld mit Auswahlknopf";

    public override void SubscribeEvents() {
        _control?.TextChanged += ValueChanged_ComboBox;
        _navigateHandler = (_, e) => OnNavigateToNext(e.Direction);
        _control?.NavigateToNext += _navigateHandler;
        _control?.ItemRemoved += ComboBox_ItemRemoved;
        _control?.DropDownShowing += ComboBox_DropDownShowing;
        _control?.EnterKey += Control_EnterKey;
        _control?.EscKey += Control_EscKey;
        _control?.TabKey += Control_TabKey;
        _control?.LostFocus += Control_LostFocus;
    }

    public override QuickImage? SymbolForReadableText() => QuickImage.Get("Textfeld");

    public override void UnsubscribeEvents() {
        _control?.TextChanged -= ValueChanged_ComboBox;
        if (_navigateHandler is not null && _control is not null) { _control.NavigateToNext -= _navigateHandler; }
        _control?.ItemRemoved -= ComboBox_ItemRemoved;
        _control?.DropDownShowing -= ComboBox_DropDownShowing;
        _control?.EnterKey -= Control_EnterKey;
        _control?.EscKey -= Control_EscKey;
        _control?.TabKey -= Control_TabKey;
        _control?.LostFocus -= Control_LostFocus;
    }

    public override bool WasValueClicked() => _control?.WasThisValueClicked() ?? false;

    protected override void ApplyStyle() {
        _control?.DropDownStyle = TextInputAllowed ? DropDownMode.DropDown : DropDownMode.DropDownList;

        _control?.GetStyleFrom(this);

        _control?.ItemClear();
        if (ListItems is not null) { _control?.ItemAddRange(ListItems); }
        _control?.ItemEditAllowed = string.Equals(UserGroup, Administrator, StringComparison.OrdinalIgnoreCase);
        _control?.CustomContextMenuItems = CustomContextMenuItems;
        _control?.RaiseChangeDelay = RaiseChangeDelay;
        _control?.RemoveAllowed = RemoveAllowed;
        _control?.QuickInfo = QuickInfo;
    }

    protected override void SetValueToControlInternal(string value) => _control?.Text = value;

    private void ComboBox_DropDownShowing(object? sender, System.EventArgs e) => OnDropDownShowing();

    private void ComboBox_ItemRemoved(object? sender, ListItemEventArgs e) => OnItemRemoved(e);

    private void Control_EnterKey(object? sender, System.EventArgs e) => OnEnterKey();

    private void Control_EscKey(object? sender, System.EventArgs e) => OnEscKey();

    private void Control_LostFocus(object? sender, System.EventArgs e) => OnLostFocus();

    private void Control_TabKey(object? sender, System.EventArgs e) => OnTabKey();

    private void ValueChanged_ComboBox(object? sender, System.EventArgs e) => OnValueChanged(_control?.Text ?? string.Empty);

    #endregion
}