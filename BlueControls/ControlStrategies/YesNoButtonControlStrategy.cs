// Licensed under AGPL-3.0; see License.md for disclaimer and details.

using BlueControls.Controls;

namespace BlueControls.ControlStrategies;

public class YesNoButtonControlStrategy : ControlStrategy {

    #region Fields

    private Button? _control;

    #endregion

    #region Properties

    public static string ClassId => "YesNoButton";

    public override string Description => "Zeigt einen Ja/Nein-Knopf, der zwischen den Werten + und - umschaltet.";
    public override string KeyName => ClassId;
    protected override System.Windows.Forms.Control? ControlCore => _control;

    #endregion

    #region Methods

    public override void SubscribeEvents() {
        _control?.CheckedChanged += YesNoButton_CheckedChanged;
        _control?.LostFocus += Control_LostFocus;
    }

    public override QuickImage? SymbolForReadableText() => QuickImage.Get(BlueBasics.Enums.ImageCode.Häkchen);

    public override void UnsubscribeEvents() {
        _control?.CheckedChanged -= YesNoButton_CheckedChanged;
        _control?.LostFocus -= Control_LostFocus;
    }

    protected override void ApplyStyle() => _control?.QuickInfo = QuickInfo;

    protected override void CreateControlCore() => _control = new Button() {
        Name = "YesNoButton",
        ButtonStyle = ButtonStyle.Yes_or_No,
        Text = string.Empty,
        ImageCode = string.Empty
    };

    protected override void ForceWriteBackValue() {
        if (_control is not { IsDisposed: false } c) { return; }
        Value = c.Checked.ToPlusMinus();
    }

    protected override void SetValueToControlInternal(string value) => _control?.Checked = value.FromPlusMinus();

    private void Control_LostFocus(object? sender, System.EventArgs e) => OnLostFocus();

    private void YesNoButton_CheckedChanged(object? sender, System.EventArgs e) => ForceWriteBackValue();

    #endregion
}