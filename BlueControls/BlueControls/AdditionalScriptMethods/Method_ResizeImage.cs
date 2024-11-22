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
using System.Drawing;
using System.Drawing.Drawing2D;
using BlueBasics;
using BlueBasics.Enums;
using BlueScript.Enums;
using BlueScript.Methods;
using BlueScript.Structures;
using BlueScript.Variables;
using static BlueScript.Variables.VariableBitmap;

namespace BlueControls.AdditionalScriptMethods;

// ReSharper disable once UnusedMember.Global
public class Method_ResizeImage : Method {

    #region Properties

    public override List<List<string>> Args => [BmpVar, FloatVal, FloatVal];
    public override string Command => "resizeimage";
    public override List<string> Constants => [];
    public override string Description => "Verändert die Größe des Bildes";
    public override bool GetCodeBlockAfter => false;
    public override int LastArgMinCount => -1;
    public override MethodType MethodType => MethodType.DrawOnBitmap;
    public override bool MustUseReturnValue => true;
    public override string Returns => VariableBitmap.ShortName_Variable;
    public override string StartSequence => "(";
    public override string Syntax => "ResizeImage(Bild, MaxWidth, MaxHeight);";

    #endregion

    #region Methods

    public override DoItFeedback DoIt(VariableCollection varCol, SplittedAttributesFeedback attvar, ScriptProperties scp, LogData ld) {
        if (attvar.ValueBitmapGet(0) is not { } bmp) { return DoItFeedback.FalscherDatentyp(ld); }

        try {
            var bmp2 = bmp.Resize(attvar.ValueIntGet(1), attvar.ValueIntGet(2),
                SizeModes.Breite_oder_Höhe_Anpassen_MitVergrößern, InterpolationMode.HighQualityBicubic, true);

            return new DoItFeedback(bmp2);
        } catch {
            return new DoItFeedback(ld, "Bildgröße konnte nicht verändert werden.");
        }

        return DoItFeedback.Null();
    }

    #endregion
}