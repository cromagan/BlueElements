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

using BlueControls.ItemCollectionPad;
using BlueScript.Enums;
using BlueScript.Methods;
using BlueScript.Structures;
using BlueScript.Variables;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace BlueControls.AdditionalScriptMethods;

// ReSharper disable once UnusedMember.Global
[SuppressMessage("Microsoft.Performance", "CA1812:AvoidUninstantiatedInternalClasses")]
internal class Method_ScaledView : Method {

    #region Properties

    public override List<List<string>> Args => [[VariableItemCollectionPad.ShortName_Variable], StringVal, StringVal, StringVal, FloatVal, StringVal];
    public override string Command => "scaledview";
    public override List<string> Constants => [];
    public override string Description => "Erstellt eine Skalierte Ansicht - mit den angegebenen JointPoings.";
    public override bool GetCodeBlockAfter => false;
    public override int LastArgMinCount => 2;
    public override MethodType MethodType => MethodType.Standard;
    public override bool MustUseReturnValue => false;
    public override string Returns => VariablePadItem.ShortName_Variable;
    public override string StartSequence => "(";
    public override string Syntax => "ScaledView(Collection, LinkeObereEckePunkt, Überschrift, Skalierung, Einzuschließende Punkte, ...);";

    #endregion

    #region Methods

    public override DoItFeedback DoIt(VariableCollection varCol, SplittedAttributesFeedback attvar, ScriptProperties scp, LogData ld) {
        //if (attvar.ReadOnly(0)) { return DoItFeedback.Schreibgschützt(ld); }

        if (attvar.Attributes[0] is not VariableItemCollectionPad icp) { return DoItFeedback.InternerFehler(ld); }
        if (icp.ValueItemCollection is not { IsDisposed: false } icpv) { return DoItFeedback.InternerFehler(ld); }

        var p1 = icpv.GetJointPoint(attvar.ValueStringGet(1), null);
        var d = new ScaledViewPadItem();


        if (p1 != null) {
            d.SetLeftTopPoint(p1.X, p1.Y);
        }

        d.Caption = attvar.ValueStringGet(2);
        d.Scale = attvar.ValueNumGet(3);

        //var p2 = icpv.GetJointPoint(attvar.ValueStringGet(4), null);
        //var abmm = attvar.ValueNumGet(5);

        //if (p1 != null && p2 != null) {
        //    if (attvar.ValueBoolGet(6)) { p2 = new PointM(p1.X, p2.Y); }
        //    if (attvar.ValueBoolGet(7)) { p2 = new PointM(p2.X, p1.Y); }
        //}


        //d.Text_Oben = attvar.ValueStringGet(1);
        //d.Text_Unten = attvar.ValueStringGet(2);

        return new DoItFeedback(new VariablePadItem(d));
    }

    #endregion
}