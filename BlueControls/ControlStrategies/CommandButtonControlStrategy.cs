// Licensed under AGPL-3.0; see License.md for disclaimer and details.

using BlueControls.Controls;

namespace BlueControls.ControlStrategies;

public class CommandButtonControlStrategy : ControlStrategy {

    #region Fields

    private const string _buttonCaptionKey = "buttoncaption";
    private const string _buttonImageCodeKey = "buttonimage";
    private Button? _control;

    #endregion

    #region Properties

    public static string ClassId => "CommandButton";

    /// <summary>
    /// Die Aufschrift des Knopfes.
    /// </summary>
    public string ButtonCaption {
        get;
        set {
            if (IsDisposed || field == value) { return; }
            field = value;

            ControlStrategyParameter.Set(_buttonCaptionKey, value);

            if (IsEventsSuppressed) { return; }
            OnDoUpdateSideOptionMenu();
            ApplyStyle();
        }
    } = string.Empty;

    /// <summary>
    /// Bildcode des Symbols auf dem Knopf.
    /// </summary>
    public string ButtonImageCode {
        get;
        set {
            if (IsDisposed || field == value) { return; }
            field = value;
            ControlStrategyParameter.Set(_buttonImageCodeKey, value);

            if (IsEventsSuppressed) { return; }
            OnDoUpdateSideOptionMenu();
            ApplyStyle();
        }
    } = string.Empty;

    public override string Description => "Zeigt einen Knopf, der anstelle einer Wert-Eingabe ein Kommando auslöst.";
    public override bool IsCommandButton => true;
    public override string KeyName => ClassId;
    protected override System.Windows.Forms.Control? ControlCore => _control;

    #endregion

    #region Methods

    public override List<GenericControl> GetProperties(int widthOfControl)
        => [.. base.GetProperties(widthOfControl),
            new FlexiControlForProperty<string>(() => ButtonCaption, "Beschriftung"),
            new FlexiControlForProperty<string>(() => ButtonImageCode, "Bildcode")];

    public override string ReadableText() => "Drückbarer Knopf";

    public override void SubscribeEvents() {
        _control?.Click += CommandButton_Click;
        _control?.LostFocus += Control_LostFocus;
    }

    public override QuickImage? SymbolForReadableText() => QuickImage.Get(BlueBasics.Enums.ImageCode.Schaltfläche);

    public override void UnsubscribeEvents() {
        _control?.Click -= CommandButton_Click;
        _control?.LostFocus -= Control_LostFocus;
    }

    protected override void ApplyStyle() {
        if (_control is not null) {
            _control.Text = ButtonCaption;
            _control.CustomContextMenuItems = CustomContextMenuItems;
            _control.ImageCode = ButtonImageCode;
            _control.QuickInfo = QuickInfo;
        }
    }

    protected override void CreateControlCore() => _control = new Button() {
        Name = "CommandButton",
        Checked = false,
        ButtonStyle = ButtonStyle.Button,
        Text = string.Empty
    };

    protected override void ForceWriteBackValue() { }

    protected override void ReadParameters(JsonObject json) {
        base.ReadParameters(json);
        ButtonCaption = json.GetString(_buttonCaptionKey, ButtonCaption);
        ButtonImageCode = json.GetString(_buttonImageCodeKey, ButtonImageCode);
    }

    protected override void SetValueToControlInternal(string value) { }

    private void CommandButton_Click(object? sender, System.EventArgs e) => OnExecuteCommand();

    private void Control_LostFocus(object? sender, System.EventArgs e) => OnLostFocus();

    #endregion
}