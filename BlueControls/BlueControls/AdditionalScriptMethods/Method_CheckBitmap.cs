// Authors:
// Christian Peter
//
// Copyright (c) 2024 Christian Peter
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

using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Drawing.Imaging;
using BlueBasics;
using BlueControls.Interfaces;
using BlueScript.Enums;
using BlueScript.Methods;
using BlueScript.Structures;
using BlueScript.Variables;
using static BlueScript.Variables.VariableBitmap;

namespace BlueControls.AdditionalScriptMethods;

// ReSharper disable once UnusedMember.Global
[SuppressMessage("Microsoft.Performance", "CA1812:AvoidUninstantiatedInternalClasses")]
internal class Method_CheckBitmap : Method, IComandBuilder {

    #region Properties

    public override List<List<string>> Args => [BmpVar, FloatVal, FloatVal, StringVal];

    public override string Command => "checkscreen";

    public override List<string> Constants => [];
    public override string Description => "Prüft auf den XY-Koordinaten, ob dort ein bestimmtes Bild abgebildet ist";

    public override bool GetCodeBlockAfter => false;
    public override int LastArgMinCount => -1;
    public override MethodType MethodType => MethodType.Math;
    public override bool MustUseReturnValue => true;
    public override string Returns => VariableBool.ShortName_Plain;
    public override string StartSequence => "(";
    public override string Syntax => "CheckScreen(BMP, X,Y,Base64_BMP_ImageCode)";

    #endregion

    #region Methods

    public override DoItFeedback DoIt(VariableCollection varCol, SplittedAttributesFeedback attvar, ScriptProperties scp, LogData ld) {
        if (attvar.ValueBitmapGet(0) is not { } bmp) { return DoItFeedback.FalscherDatentyp(ld); }

        var x = attvar.ValueIntGet(1);
        var y = attvar.ValueIntGet(2);
        using var bmps = new BitmapExt(bmp);
        using var bmpa = bmps.Crop(x - 10, y - 5, 20, 10);
        return new DoItFeedback(Converter.BitmapToBase64(bmpa, ImageFormat.Bmp) == attvar.ValueStringGet(3));
    }

    public string GetCode() {
        var c = ScreenShot.GrabAndClick("Wählen sie den Punkt, der geprüft werden soll.", null);

        //if (c.GrabedArea())

        return string.Empty;
    }

    #endregion
}