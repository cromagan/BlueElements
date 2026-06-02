// Licensed under AGPL-3.0; see License.md for disclaimer and details.

using BlueScript.Classes;
using BlueScript.Enums;
using BlueScript.Variables;

namespace BlueControls.AdditionalScriptMethods;


internal class Method_MouseDownUp : Method, IComandBuilder {

    #region Properties

    public override List<List<string>> Args => [FloatVal, FloatVal, FloatVal, FloatVal, FloatVal];

    public override string Command => "mousedownup";

    public override string Description => "Simuliert einen Maus-Klick. Sind die Koordiataten unterschiedlich, wird die Maus gedrpckt dort hin gezogen.";

    public override MethodType MethodLevel => MethodType.ManipulatesUser;
    public override string Syntax => "MouseDownUp(DownX, DownY, TimeInSeconds, UpX, UpY)";

    #endregion

    #region Methods

    public string ComandDescription() => "Klicke mit der Maus.";

    public QuickImage ComandImage() => QuickImage.Get(ImageCode.Mauspfeil, 16);

    public override DoItFeedback DoIt(VariableCollection varCol, SplittedAttributesFeedback attvar, ScriptProperties scp, LogData ld) {
        var xdown = attvar.ValueIntGet(0);
        var ydown = attvar.ValueIntGet(1);

        var time = attvar.ValueNumGet(2);

        if (time is < 0 or > 5) {
            return new DoItFeedback("Zeitintervall nur von 0 bis 5 Sekunden erlaubt", true, ld);
        }

        var xup = attvar.ValueIntGet(3);
        var yup = attvar.ValueIntGet(4);

        WindowsRemoteControl.MoveMouse(xdown, ydown);
        Generic.Pause(0.01, false);
        WindowsRemoteControl.MouseAction(MouseEventFlags.MOUSEEVENTF_LEFTDOWN, xdown, ydown);
        Generic.Pause(time, false);
        WindowsRemoteControl.MoveMouse(xup, yup);
        Generic.Pause(0.01, false);
        WindowsRemoteControl.MouseAction(MouseEventFlags.MOUSEEVENTF_LEFTUP, xup, yup);
        return DoItFeedback.Null();
    }

    public string GetCode(Form? form) {
        var c = ScreenShot.GrabAndClick("Wählen sie den Punkt, der geklickt werden soll.", form, Helpers.None);

        if (c.Screen is null) { return string.Empty; }

        return $"MouseDownUp({c.Point1.X}, {c.Point1.Y}, 0.2, {c.Point1.X}, {c.Point1.Y});";
    }

    #endregion
}