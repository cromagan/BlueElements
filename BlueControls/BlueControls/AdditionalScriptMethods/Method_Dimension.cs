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

using BlueControls.ItemCollectionPad;
using BlueScript.Enums;
using BlueScript.Methods;
using BlueScript.Structures;
using BlueScript.Variables;
using System.Collections.Generic;

namespace BlueControls.AdditionalScriptMethods;

// ReSharper disable once UnusedMember.Global
internal class Method_Dimension : Method {

    #region Properties

    public override List<List<string>> Args => [[VariableItemCollectionPad.ShortName_Variable], StringVal, StringVal, StringVal, StringVal, FloatVal, BoolVal, BoolVal];
    public override string Command => "dimension";
    public override List<string> Constants => [];
    public override string Description => "Erstellt eine Bemaßung - mit den angegebenen JointPoints.";
    public override bool GetCodeBlockAfter => false;
    public override int LastArgMinCount => -1;
    public override MethodType MethodLevel => MethodType.Standard;
    public override bool MustUseReturnValue => false;
    public override string Returns => VariablePadItem.ShortName_Variable;
    public override string StartSequence => "(";
    public override string Syntax => "Dimension(Collection, TextOben, TextUnten, Punkt1, Punkt2, AbstandinMM, UseXofPoint1(=Vertikal), UseYOfPoint1(=Horizontal));";

    #endregion

    #region Methods

    public override DoItFeedback DoIt(VariableCollection varCol, SplittedAttributesFeedback attvar, ScriptProperties scp, LogData ld) {
        //if (attvar.ReadOnly(0)) { return DoItFeedback.Schreibgschützt(ld); }

        if (attvar.Attributes[0] is not VariableItemCollectionPad icp) { return DoItFeedback.InternerFehler(ld); }
        if (icp.ValueItemCollection is not { IsDisposed: false } icpv) { return DoItFeedback.InternerFehler(ld); }

        var p1 = icpv.GetJointPoint(attvar.ValueStringGet(3), null);
        var p2 = icpv.GetJointPoint(attvar.ValueStringGet(4), null);
        var abmm = attvar.ValueNumGet(5);

        if (p1 != null && p2 != null) {
            if (attvar.ValueBoolGet(6)) { p2 = new PointM(p1.X, p2.Y); }
            if (attvar.ValueBoolGet(7)) { p2 = new PointM(p2.X, p1.Y); }
        }
        var d = new DimensionPadItem(p1, p2, (float)abmm) {
            Text_Oben = attvar.ValueStringGet(1),
            Text_Unten = attvar.ValueStringGet(2)
        };

        return new DoItFeedback(new VariablePadItem(d));
    }

    #endregion
}