// Licensed under AGPL-3.0; see License.md for disclaimer and details.

using BlueControls.Controls;

namespace BlueControls.ControlStrategies;

public class LineControlStrategy : ControlStrategy {

    #region Fields

    private Line? _control;

    #endregion

    #region Properties

    public static string ClassId => "Line";

    protected override System.Windows.Forms.Control? ControlCore => _control;

    public override string KeyName => ClassId;

    #endregion

    #region Methods

    public override void CreateControl() => _control = new Line() { Orientation = Orientation.Waagerecht };

    public override void SubscribeEvents() => _control?.LostFocus += Control_LostFocus;

    public override void UnsubscribeEvents() => _control?.LostFocus -= Control_LostFocus;

    protected override void ApplyStyle() => _control?.QuickInfo = QuickInfo;

    protected override void SetValueToControlInternal(string value) { }

    private void Control_LostFocus(object? sender, System.EventArgs e) => OnLostFocus();

    #endregion
}