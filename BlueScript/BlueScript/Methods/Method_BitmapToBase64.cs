// Authors:
// Christian Peter
//
// Copyright (c) 2022 Christian Peter
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
using System.Drawing.Imaging;
using BlueBasics;
using BlueScript.Structures;
using BlueScript.Enums;
using BlueScript.Variables;

namespace BlueScript.Methods {

    internal class Method_BitmapToBase64 : Method {

        #region Properties

        public override List<VariableDataType> Args => new() { VariableDataType.Variable_Object, VariableDataType.String };
        public override string Description => "Konvertiert das Bild in das Base64 Format und gibt dessen String zurück.";
        public override bool EndlessArgs => false;
        public override string EndSequence => ")";
        public override bool GetCodeBlockAfter => false;
        public override VariableDataType Returns => VariableDataType.String;
        public override string StartSequence => "(";
        public override string Syntax => "BitmapToBase64(Bitmap, JPG / PNG)";

        #endregion

        #region Methods

        public override List<string> Comand(Script? s) => new() { "bitmaptobase64" };

        public override DoItFeedback DoIt(CanDoFeedback infos, Script s) {
            var attvar = SplitAttributeToVars(infos.AttributText, s, Args, EndlessArgs);
            if (!string.IsNullOrEmpty(attvar.ErrorMessage)) { return DoItFeedback.AttributFehler(this, attvar); }

            string x;

            switch (((VariableString)attvar.Attributes[1]).ValueString.ToUpper()) {
                case "JPG":
                    x = Converter.BitmapToBase64(((VariableBitmap)attvar.Attributes[0]).ValueBitmap, ImageFormat.Jpeg);
                    break;

                case "PNG":
                    x = Converter.BitmapToBase64(((VariableBitmap)attvar.Attributes[0]).ValueBitmap, ImageFormat.Png);
                    break;

                default:
                    return new DoItFeedback("Es wir als zweites Attribut ein String mit dem Inhalt jpg oder png erwartet.");
            }

            return new DoItFeedback(x, string.Empty);
        }

        #endregion
    }
}