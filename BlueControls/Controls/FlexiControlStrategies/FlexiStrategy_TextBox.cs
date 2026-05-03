// Licensed under AGPL-3.0; see License.md for disclaimer and details.

using BlueControls.EventArgs;

namespace BlueControls.Controls.FlexiControlStrategies;

public class FlexiStrategyTextBox : FlexiStrategyBase {

    #region Fields

    private TextBox? _control;
    private EventHandler<NavigationDirectionEventArgs>? _navigateHandler;

    #endregion

    #region Properties

    public override System.Windows.Forms.Control? Control => _control;

    #endregion

    #region Methods

    public override void CreateControl() {
        _control = new TextBox();
        SubscribeEvents();
    }

    public override void SetValue(string value) {
        if (_control is not null) { _control.Text = value; }
    }

    public override void StyleControl(FlexiStyleContext context, ColumnItem? column, string caption) {
        base.StyleControl(context, column, caption);
        if (_control is null) { return; }
        _control.RaiseChangeDelay = context.Delay;
        _control.Verhalten = _control.MultiLine || _control.Height > 20
            ? SteuerelementVerhalten.Scrollen_mit_Textumbruch
            : SteuerelementVerhalten.Scrollen_ohne_Textumbruch;
    }

    public override void SubscribeEvents() {
        if (_control is null) { return; }
        _control.TextChanged += ValueChanged_TextBox;
        _navigateHandler = (_, e) => OnNavigateToNext(e.Direction);
        _control.NavigateToNext += _navigateHandler;
    }

    public override void UnsubscribeEvents() {
        if (_control is null) { return; }
        _control.TextChanged -= ValueChanged_TextBox;
        if (_navigateHandler is not null) { _control.NavigateToNext -= _navigateHandler; }
    }

    private void ValueChanged_TextBox(object? sender, System.EventArgs e) => OnValueChanged(_control!.Text);

    #endregion
}