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

using BlueScript.Enums;
using BlueScript.Methods;
using BlueScript.Structures;
using BlueScript.Variables;
using System.Collections.Generic;
using System.Drawing;
using static BlueScript.Variables.VariableBitmap;

namespace BlueControls.AdditionalScriptMethods;


public class Method_DrawLine : Method {

    #region Properties

    public override List<List<string>> Args => [BmpVar, FloatVal, FloatVal, FloatVal, FloatVal];
    public override string Command => "drawline";
    public override List<string> Constants => [];
    public override string Description => "Zeichnet eine Linie auf dem angegebenen Bild.";
    public override bool GetCodeBlockAfter => false;
    public override int LastArgMinCount => -1;
    public override MethodType MethodLevel => MethodType.Standard;
    public override bool MustUseReturnValue => false;
    public override string Returns => string.Empty;
    public override string StartSequence => "(";
    public override string Syntax => "DrawLine(Bild, x1, y1, x2, y2);";

    #endregion

    #region Methods

    public override DoItFeedback DoIt(VariableCollection varCol, SplittedAttributesFeedback attvar, ScriptProperties scp, LogData ld) {
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