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

using BlueScript.Classes;
using BlueScript.Methods;
using BlueScript.Variables;
using System.Collections.Generic;
using System.Drawing;
using static BlueScript.Variables.VariableBitmap;

namespace BlueControls.AdditionalScriptMethods;

public sealed class Method_DrawLine : Method {

    #region Properties

    public static List<List<string>> Args => [BmpVar, FloatVal, FloatVal, FloatVal, FloatVal];
    public static string Command => "drawline";
    public static List<string> Constants => [];
    public static string Description => "Zeichnet eine Linie auf dem angegebenen Bild.";

    public static int LastArgMinCount => -1;

    public static string Returns => string.Empty;
    public static string StartSequence => "(";
    public static string Syntax => "DrawLine(Bild, x1, y1, x2, y2);";

    #endregion

    #region Methods

    public static DoItFeedback DoItSplitted(VariableCollection varCol, SplittedAttributesFeedback attvar, ScriptProperties scp, LogData ld) {
        if (attvar.ValueBitmapGet(0) is not { } bmp) { return DoItFeedback.FalscherDatentyp(ld); }

        try {
            using var gr = Graphics.FromImage(bmp);
            gr.DrawLine(Pens.Black, attvar.ValueIntGet(1), attvar.ValueIntGet(2), attvar.ValueIntGet(3), attvar.ValueIntGet(4));
        } catch {
            return new DoItFeedback("Linie konnte nicht gezeichnet werden.", true, ld);
        }

        return DoItFeedback.Null();
    }

    #endregion
}