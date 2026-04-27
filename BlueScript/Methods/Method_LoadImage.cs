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

using BlueBasics.ClassesStatic;
using BlueScript.Classes;
using BlueScript.Enums;
using BlueScript.Variables;
using System.Collections.Generic;
using System.Drawing;
using static BlueBasics.Extensions;

namespace BlueScript.Methods;


internal sealed class Method_LoadImage : Method {

    #region Properties

    public static List<List<string>> Args => [StringVal];
    public static string Command => "loadimage";
    public static List<string> Constants => [];
    public static string Description => "Lädt das angegebene Bild aus dem Dateisystem.\r\nDiese Routine wird keinen Fehler auslösen.\r\nFalls etwas schief läuft, enthält die Variable ein Bild des Wertes NULL.";

    public static int LastArgMinCount => -1;
    public static MethodType MethodLevel => MethodType.LongTime;
    public static bool MustUseReturnValue => true;
    public static string Returns => VariableBitmap.ShortName_Variable;
    public static string StartSequence => "(";
    public static string Syntax => "LoadImage(Filename)";

    #endregion

    #region Methods

    public static DoItFeedback DoItSplitted(VariableCollection varCol, SplittedAttributesFeedback attvar, ScriptProperties scp, LogData ld) {
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
            var img = Image_FromFile(attvar.ValueStringGet(0));
            if (img is null) { return new DoItFeedback(null as Bitmap); }
            var bmp = (Bitmap)img;
            return new DoItFeedback(bmp);
        } catch {
            return new DoItFeedback(null as Bitmap);
            //return new DoItFeedback(ld, "Datei konnte nicht geladen werden: " + attvar.ValueString(0));
        }
    }

    #endregion
}