// Licensed under AGPL-3.0; see License.md for disclaimer and details.

using static BlueBasics.ClassesStatic.Converter;

namespace BlueControls.Controls.FlexiControlStrategies;

public class FlexiStrategyButtonColor : FlexiStrategyBase {

    #region Fields

    private Button? _control;

    #endregion

    #region Properties

    public override System.Windows.Forms.Control? Control => _control;

    #endregion

    #region Methods

    public override void CreateControl() {
        _control = new Button() {
            Name = "ColorButton",
            Checked = false,
            ButtonStyle = ButtonStyle.Button,
            Text = string.Empty
        };
    }

    public override void SubscribeEvents() { }

    public override void UnsubscribeEvents() { }

    protected override void ApplyStyle() {
    }

    protected override void SetValueToControl() {
        _control?.ImageCode = string.IsNullOrEmpty(Value) ? "Fragezeichen|24" : "Kreis|24|||" + Color.FromArgb(IntParse(Value)).ToHtmlCode();
    }

    #endregion
}