// Authors:
// Christian Peter
//
// Copyright © 2026 Christian Peter
// https://github.com/cromagan/BlueElements
//
// License: GNU Affero General Public License v3.0
// https://github.com/cromagan/BlueElements/blob/master/LICENSE
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL
// THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING
// FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER
// DEALINGS IN THE SOFTWARE.

using BlueBasics.Classes;
using BlueBasics.ClassesStatic;
using BlueBasics.Enums;
using BlueControls.Enums;
using BlueControls.Forms;
using BlueControls.Interfaces;
using BlueScript.Classes;
using BlueScript.Enums;
using BlueScript.Methods;
using BlueScript.Variables;
using System.Collections.Generic;

namespace BlueControls.AdditionalScriptMethods;


internal class Method_MouseDownUp : Method, IComandBuilder {

    #region Properties

    public override List<List<string>> Args => [FloatVal, FloatVal, FloatVal, FloatVal, FloatVal];

    public override string Command => "mousedownup";

    public override List<string> Constants => [];
    public override string Description => "Simuliert einen Maus-Klick. Sind die Koordiataten unterschiedlich, wird die Maus gedrpckt dort hin gezogen.";

    public override bool GetCodeBlockAfter => false;
    public override int LastArgMinCount => -1;
    public override MethodType MethodLevel => MethodType.ManipulatesUser;
    public override bool MustUseReturnValue => false;
    public override string Returns => string.Empty;
    public override string StartSequence => "(";
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

        if (c.Screen is not { }) { return string.Empty; }

        return $"MouseDownUp({c.Point1.X}, {c.Point1.Y}, 0.2, {c.Point1.X}, {c.Point1.Y});";
    }

    #endregion
}