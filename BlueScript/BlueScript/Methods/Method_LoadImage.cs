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

using System.Collections.Generic;
using System.Drawing;
using BlueBasics;
using BlueBasics.Enums;
using BlueScript.Structures;
using BlueScript.Enums;
using BlueScript.Variables;

namespace BlueScript.Methods {

    internal class Method_LoadImage : Method {

        #region Properties

        public override List<VariableDataType> Args => new() { VariableDataType.String };
        public override string Description => "Lädt das angegebene Bild aus dem Dateisystem.";
        public override bool EndlessArgs => false;
        public override string EndSequence => ")";
        public override bool GetCodeBlockAfter => false;
        public override VariableDataType Returns => VariableDataType.Bitmap;
        public override string StartSequence => "(";
        public override string Syntax => "LoadImage(Filename)";

        #endregion

        #region Methods

        public override List<string> Comand(Script? s) => new() { "loadimage" };

        public override DoItFeedback DoIt(CanDoFeedback infos, Script s) {
            var attvar = SplitAttributeToVars(infos.AttributText, s, Args, EndlessArgs);
            if (!string.IsNullOrEmpty(attvar.ErrorMessage)) { return DoItFeedback.AttributFehler(this, attvar); }

            if (((VariableString)attvar.Attributes[0]).ValueString.FileType() != enFileFormat.Image) {
                return new DoItFeedback("Datei ist kein Bildformat: " + ((VariableString)attvar.Attributes[0]).ValueString);
            }

            if (!FileOperations.FileExists(((VariableString)attvar.Attributes[0]).ValueString)) {
                return new DoItFeedback("Datei nicht gefunden: " + ((VariableString)attvar.Attributes[0]).ValueString);
            }

            try {
                Generic.CollectGarbage();
                var bmp = (Bitmap)BitmapExt.Image_FromFile(((VariableString)attvar.Attributes[0]).ValueString);
                //var nr = s.AddBitmapToCache(bmp);

                return new DoItFeedback(bmp);
            } catch {
                return new DoItFeedback("Datei konnte nicht geladen werden: " + ((VariableString)attvar.Attributes[0]).ValueString);
            }
        }

        #endregion
    }
}