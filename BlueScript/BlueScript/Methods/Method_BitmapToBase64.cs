// Authors:
// Christian Peter
//
// Copyright (c) 2021 Christian Peter
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

using Skript.Enums;
using System.Collections.Generic;

namespace BlueScript {

    internal class Method_BitmapToBase64 : Method {

        #region Properties

        public override List<enVariableDataType> Args => new() { enVariableDataType.Bitmap, enVariableDataType.String };
        public override string Description => "Konvertiert das Bild in das Base64 Format und gibt dessen String zurück.";
        public override bool EndlessArgs => false;
        public override string EndSequence => ")";
        public override bool GetCodeBlockAfter => false;
        public override enVariableDataType Returns => enVariableDataType.String;
        public override string StartSequence => "(";
        public override string Syntax => "BitmapToBase64(Bitmap, JPG / PNG)";
        #endregion

        #region Methods

        public override List<string> Comand(Script s) => new() { "bitmaptobase64" };

        public override strDoItFeedback DoIt(strCanDoFeedback infos, Script s) {
            var attvar = SplitAttributeToVars(infos.AttributText, s, Args, EndlessArgs);
            if (!string.IsNullOrEmpty(attvar.ErrorMessage)) { return strDoItFeedback.AttributFehler(this, attvar); }


            string x;

            switch (attvar.Attributes[1].ValueString.ToUpper()) {
                case "JPG":
                    x = BlueBasics.Converter.BitmapToBase64(attvar.Attributes[0].ValueBitmap(s), System.Drawing.Imaging.ImageFormat.Jpeg);
                    break;
                case "PNG":
                    x = BlueBasics.Converter.BitmapToBase64(attvar.Attributes[0].ValueBitmap(s), System.Drawing.Imaging.ImageFormat.Png);
                    break;
                default:
                    return new strDoItFeedback("Es wir als zweites Attribut ein String mit dem Inhalt jpg oder png erwartet.");
            }


            return new strDoItFeedback(x, enVariableDataType.String);
        }

        #endregion
    }
}