// Authors:
// Christian Peter
//
// Copyright (c) 2025 Christian Peter
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

#nullable enable

using BlueBasics;
using BlueBasics.Enums;
using BlueControls.Enums;
using BlueControls.Forms;
using BlueControls.Interfaces;
using BlueScript.Enums;
using BlueScript.Methods;
using BlueScript.Structures;
using BlueScript.Variables;
using System.Collections.Generic;
using System.Drawing.Imaging;
using static BlueScript.Variables.VariableBitmap;

namespace BlueControls.AdditionalScriptMethods;

// ReSharper disable once UnusedMember.Global
internal class Method_CheckBitmap : Method, IComandBuilder {

    #region Properties

    public override List<List<string>> Args => [BmpVar, FloatVal, FloatVal, StringVal];

    public override string Command => "checkbitmap";

    public override List<string> Constants => [];
    public override string Description => "Prüft auf den XY-Koordinaten, ob dort ein bestimmtes Bild abgebildet ist. Zum Erstellen des Befehls den Assistenten benutzen.";

    public override bool GetCodeBlockAfter => false;
    public override int LastArgMinCount => -1;
    public override MethodType MethodType => MethodType.Math;
    public override bool MustUseReturnValue => true;
    public override string Returns => VariableBool.ShortName_Plain;
    public override string StartSequence => "(";
    public override string Syntax => "CheckBitmap(BMP, X,Y, HasCode)";

    #endregion

    #region Methods

    public string ComandDescription() => "Prüfe, ob auf dem Bildchirm etwas Bestimmtes zu sehen ist.";

    public QuickImage ComandImage() => QuickImage.Get(ImageCode.Bild, 16);

    public override DoItFeedback DoIt(VariableCollection varCol, SplittedAttributesFeedback attvar, ScriptProperties scp, CanDoFeedback ld){
        if (attvar.ValueBitmapGet(0) is not { } bmp) { return DoItFeedback.FalscherDatentyp(ld); }

        var x = attvar.ValueIntGet(1);
        var y = attvar.ValueIntGet(2);
        using var bmps = new BitmapExt(bmp);
        using var bmpa = bmps.Crop(x - 10, y - 5, 20, 10);
        return new DoItFeedback(Converter.BitmapToBase64(bmpa, ImageFormat.Bmp).GetHashString() == attvar.ValueStringGet(3), ld.EndPosition());
    }

    public string GetCode(Form? form) {
        var c = ScreenShot.GrabAndClick("Wählen sie den Punkt, der geprüft werden soll.", form, Helpers.Draw20x10);

        if (c.Screen is not { } bmp) { return string.Empty; }

        var n = InputBox.Show("Variablenname:", "result", FormatHolder.SystemName);

        if (string.IsNullOrEmpty(n)) {
            n = "result";
        }

        using var bmps = new BitmapExt(bmp);
        using var bmpa = bmps.Crop(c.Point1.X - 10, c.Point1.Y - 5, 20, 10);
        return $"var sc = Screenshot();\r\nvar {n} = CheckBitmap(sc, {c.Point1.X}, {c.Point1.Y}, \"{Converter.BitmapToBase64(bmpa, ImageFormat.Bmp).GetHashString()}\");";
    }

    #endregion
}