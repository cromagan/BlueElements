// Licensed under AGPL-3.0; see License.md for disclaimer and details.

using BlueControls.Controls;
using BlueControls.EventArgs;
using BlueTable.Interfaces;

namespace BlueControls.ControlStrategies;

public class ComboBoxControlStrategy : ControlStrategy, IHasColumn {

    #region Fields

    private ComboBox? _control;

    #endregion

    #region Properties

    public static string ClassId => "Combobox";

    /// <summary>
    /// Die Spalte, zu der das Control gehört.
    /// </summary>
    public ColumnItem? Column { get; set; }

    public override string Description => "Textfeld mit Dropdown-Knopf für feste Vorgaben und freie Eingabe.";
    public override string KeyName => ClassId;

    public override bool SupportsSuggestions => true;
    public override bool SupportsTextEdit => true;

    protected override System.Windows.Forms.Control? ControlCore => _control;

    #endregion

    #region Methods

    /// <summary>
    /// Vergrößert die Breite um den Dropdown-Knopf, damit der Zell-Text
    /// beim Edit nicht vom Knopf überdeckt wird. Einzeilig bleibt die
    /// Combobox auf Zeilenhöhe.
    /// </summary>
    public override Rectangle CalculateRequiredBounds(Rectangle bounds) {
        var required = base.CalculateRequiredBounds(bounds);
        return new Rectangle(required.Location,
            new Size(required.Width + (_control?.btnDropDown.Width ?? 0),
                MultiLine ? required.Height : Math.Min(required.Height, SingleLineHeight.CanvasToControl(Zoom))));
    }

    public override void HandleCaptionClick() {
        _control?.Focus();
        _control?.ShowMenu(this, new System.Windows.Forms.MouseEventArgs(System.Windows.Forms.MouseButtons.None, 0, 0, 0, 0));
    }

    /// <summary>
    /// Die Spalte braucht auswählbare Werte: Dropdown-Items, die Werte der
    /// anderen Zellen oder eine Beziehung mit Dropdown-Werten.
    /// </summary>
    public override string ErrorReason() {
        if (Column is not { IsDisposed: false } column) { return string.Empty; }
        if (column.MayHaveDropDown()) { return string.Empty; }
        return ColumnErrorConstants.NoDropdownItems;
    }

    public override string ReadableText() => "Textfeld mit Auswahlknopf";

    public override void SubscribeEvents() {
        _control?.TextChanged += ValueChanged_ComboBox;
        _control?.NavigateToNext += Control_NavigateToNext;
        _control?.ItemRemoved += ComboBox_ItemRemoved;
        _control?.DropDownShowing += ComboBox_DropDownShowing;
        _control?.EnterKey += Control_EnterKey;
        _control?.EscKey += Control_EscKey;
        _control?.TabKey += Control_TabKey;
        _control?.LostFocus += Control_LostFocus;
    }

    public override QuickImage? SymbolForReadableText() => QuickImage.Get(ImageCode.Textfeld);

    public override void UnsubscribeEvents() {
        _control?.TextChanged -= ValueChanged_ComboBox;
        _control?.NavigateToNext -= Control_NavigateToNext;
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
        _control?.Zoom = Zoom;
    }

    protected override void CreateControlCore() => _control = new ComboBox();

    protected override void ForceWriteBackValue() {
        if (_control is not { IsDisposed: false } c) { return; }
        Value = c.Text;
    }

    protected override void SetValueToControlInternal(string value) => _control?.Text = value;

    private void ComboBox_DropDownShowing(object? sender, System.EventArgs e) => OnDropDownShowing();

    private void ComboBox_ItemRemoved(object? sender, ListItemEventArgs e) => OnItemRemoved(e);

    private void Control_EnterKey(object? sender, System.EventArgs e) => OnEnterKey();

    private void Control_EscKey(object? sender, System.EventArgs e) => OnEscKey();

    private void Control_LostFocus(object? sender, System.EventArgs e) => OnLostFocus();

    private void Control_NavigateToNext(object? sender, NavigationDirectionEventArgs e) => OnNavigateToNext(e.Direction);

    private void Control_TabKey(object? sender, System.EventArgs e) => OnTabKey();

    private void ValueChanged_ComboBox(object? sender, System.EventArgs e) => ForceWriteBackValue();

    #endregion
}