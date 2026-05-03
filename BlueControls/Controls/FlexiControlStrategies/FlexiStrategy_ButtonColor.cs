// Licensed under AGPL-3.0; see License.md for disclaimer and details.

using static BlueBasics.ClassesStatic.Converter;

namespace BlueControls.Controls.FlexiControlStrategies;

public class FlexiStrategyButtonColor : IFlexiStrategy {
    private Button? _control;

    public System.Windows.Forms.Control? Control => _control;

    public void CreateControl(FlexiControl owner) {
        _control = new Button() {
            Enabled = owner.Enabled,
            Name = "ColorButton",
            Checked = false,
            ButtonStyle = ButtonStyle.Button,
            Text = string.Empty
        };
    }

    public void SetValue(FlexiControl owner, string value) {
        if (_control is null) { return; }
        _control.ImageCode = string.IsNullOrEmpty(value) ? "Fragezeichen|24" : "Kreis|24|||" + Color.FromArgb(IntParse(value)).ToHtmlCode();
    }

    public void SubscribeEvents(FlexiControl owner) {
    }

    public void UnsubscribeEvents() {
    }
}