// Licensed under AGPL-3.0; see License.md for disclaimer and details.

namespace BlueControls.Controls.FlexiControlStrategies;

public class FlexiStrategyTextBoxSuggestions : FlexiStrategyBase {

    #region Fields

    private TextBoxSuggestions? _control;

    #endregion

    #region Properties

    public override System.Windows.Forms.Control? Control => _control;

    #endregion

    #region Methods

    public override void CreateControl() {
        _control = new TextBoxSuggestions();
    }

    public override void SubscribeEvents() {
        _control?.TextChanged += ValueChanged_TextBoxSuggestions;
    }

    public override void UnsubscribeEvents() {
        _control?.TextChanged -= ValueChanged_TextBoxSuggestions;
    }

    protected override void ApplyStyle() {
        if (_control is not { IsDisposed: false }) { return; }

        _control.CustomContextMenuItems = CustomContextMenuItems;
        _control.RaiseChangeDelay = RaiseChangeDelay;
        _control.GetStyleFrom(this);
        _control.CustomVocabulary = CustomVocabulary;
        _control.Suffix = Suffix;
        _control.Verhalten = ParentHeight > 20
            ? SteuerelementVerhalten.Scrollen_mit_Textumbruch
            : SteuerelementVerhalten.Scrollen_ohne_Textumbruch;
        _control.QuickInfo = QuickInfo;

        if (ListItems is { Count: > 0 } items) {
            _control.Suggestions = new System.Collections.ObjectModel.ReadOnlyCollection<string>([.. items.Select(i => i.KeyName)]);
        } else {
            _control.Suggestions = System.Collections.ObjectModel.ReadOnlyCollection<string>.Empty;
        }
    }

    protected override void SetValueToControlInternal(string value) => _control.Text = value;

    private void ValueChanged_TextBoxSuggestions(object? sender, System.EventArgs e) => OnValueChanged(_control?.Text ?? string.Empty);

    #endregion
}