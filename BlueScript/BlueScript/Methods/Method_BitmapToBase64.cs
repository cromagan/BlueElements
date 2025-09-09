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
using BlueScript.Enums;
using BlueScript.Structures;
using BlueScript.Variables;
using System.Collections.Generic;
using System.Drawing.Imaging;
using static BlueScript.Variables.VariableBitmap;

namespace BlueScript.Methods;

// ReSharper disable once UnusedMember.Global
internal class Method_BitmapToBase64 : Method {

    #region Properties

    public override List<List<string>> Args => [BmpVar, StringVal];
    public override string Command => "bitmaptobase64";
    public override List<string> Constants => ["PNG", "JPG", "BMP"];
    public override string Description => "Konvertiert das Bild in das Base64 Format und gibt dessen String zurück.";
    public override bool GetCodeBlockAfter => false;
    public override int LastArgMinCount => -1;
    public override MethodType MethodLevel => MethodType.LongTime;
    public override bool MustUseReturnValue => true;
    public override string Returns => VariableString.ShortName_Plain;
    public override string StartSequence => "(";
    public override string Syntax => "BitmapToBase64(Bitmap, JPG/PNG/BMP)";

    #endregion

    #region Methods

    public override DoItFeedback DoIt(VariableCollection varCol, SplittedAttributesFeedback attvar, ScriptProperties scp, LogData ld) {
        string x;

        switch (attvar.ValueStringGet(1).ToUpperInvariant()) {
            case "JPG":
                x = Converter.BitmapToBase64(attvar.ValueBitmapGet(0), ImageFormat.Jpeg);
                break;

            case "PNG":
                x = Converter.BitmapToBase64(attvar.ValueBitmapGet(0), ImageFormat.Png);
                break;

            case "BMP":
                x = Converter.BitmapToBase64(attvar.ValueBitmapGet(0), ImageFormat.Bmp);
                break;

            default:
                return new DoItFeedback("Es wir als zweites Attribut ein String mit dem Inhalt bmp, jpg oder png erwartet.", true, ld);
        }

        return new DoItFeedback(x);
    }

    #endregion
}