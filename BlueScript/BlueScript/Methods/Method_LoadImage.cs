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

using BlueBasics;
using Skript.Enums;
using System.Collections.Generic;
using System.Drawing;

namespace BlueScript {

    internal class Method_LoadImage : Method {

        #region Properties

        public override List<enVariableDataType> Args => new() { enVariableDataType.String };
        public override string Description => "Lädt das angegebene Bild aus dem Dateisystem.";
        public override bool EndlessArgs => false;
        public override string EndSequence => ")";
        public override bool GetCodeBlockAfter => false;
        public override enVariableDataType Returns => enVariableDataType.Bitmap;
        public override string StartSequence => "(";
        public override string Syntax => "LoadImage(Filename)";

        #endregion

        #region Methods

        public override List<string> Comand(Script s) => new() { "loadimage" };

        public override strDoItFeedback DoIt(strCanDoFeedback infos, Script s) {
            var attvar = SplitAttributeToVars(infos.AttributText, s, Args, EndlessArgs);
            if (!string.IsNullOrEmpty(attvar.ErrorMessage)) { return strDoItFeedback.AttributFehler(this, attvar); }

            if (attvar.Attributes[0].ValueString.FileType() != BlueBasics.Enums.enFileFormat.Image) {
                return new strDoItFeedback("Datei ist kein Bildformat: " + attvar.Attributes[0].ValueString);
            }

            if (!FileOperations.FileExists(attvar.Attributes[0].ValueString)) {
                return new strDoItFeedback("Datei nicht gefunden: " + attvar.Attributes[0].ValueString);
            }

            try {
                Generic.CollectGarbage();
                var bmp = (Bitmap)BitmapExt.Image_FromFile(attvar.Attributes[0].ValueString);
                var nr = s.AddBitmapToCache(bmp);

                return new strDoItFeedback(nr.ToString(), enVariableDataType.Bitmap);
            } catch {
                return new strDoItFeedback("Datei konnte nicht geladen werden: " + attvar.Attributes[0].ValueString);
            }
        }

        #endregion
    }
}