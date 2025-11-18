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
using System.Drawing;
using static BlueBasics.Extensions;

namespace BlueScript.Methods;


internal class Method_LoadImage : Method {

    #region Properties

    public override List<List<string>> Args => [StringVal];
    public override string Command => "loadimage";
    public override List<string> Constants => [];
    public override string Description => "Lädt das angegebene Bild aus dem Dateisystem.\r\nDiese Routine wird keinen Fehler auslösen.\r\nFalls etwas schief läuft, enthält die Variable ein Bild des Wertes NULL.";
    public override bool GetCodeBlockAfter => false;
    public override int LastArgMinCount => -1;
    public override MethodType MethodLevel => MethodType.LongTime;
    public override bool MustUseReturnValue => true;
    public override string Returns => VariableBitmap.ShortName_Variable;
    public override string StartSequence => "(";
    public override string Syntax => "LoadImage(Filename)";

    #endregion

    #region Methods

    public override DoItFeedback DoIt(VariableCollection varCol, SplittedAttributesFeedback attvar, ScriptProperties scp, LogData ld) {
        // Da es keine Möglichkeit gibt, eine Bild Variable (außerhalb eines If) zu deklarieren,
        // darf diese Routine nicht fehlschlagen.

        //if (attvar.ValueString(0).FileType() != FileFormat.Image) {
        //    return new DoItFeedback(ld, "Datei ist kein Bildformat: " + attvar.ValueString(0));
        //}

        //if (!IO.FileExists(attvar.ValueString(0))) {
        //    return new DoItFeedback(ld, "Datei nicht gefunden: " + attvar.ValueString(0));
        //}

        try {
            Generic.CollectGarbage();
            var bmp = (Bitmap)Image_FromFile(attvar.ValueStringGet(0))!;
            return new DoItFeedback(bmp);
        } catch {
            return new DoItFeedback(null as Bitmap);
            //return new DoItFeedback(ld, "Datei konnte nicht geladen werden: " + attvar.ValueString(0));
        }
    }

    #endregion
}