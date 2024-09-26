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

using BlueScript.Enums;
using BlueScript.Methods;
using BlueScript.Structures;
using BlueScript.Variables;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace BlueControls.AdditionalScriptMethods;

// ReSharper disable once UnusedMember.Global
[SuppressMessage("Microsoft.Performance", "CA1812:AvoidUninstantiatedInternalClasses")]
internal class Method_ConnectPoints : Method {

    #region Properties

    public override List<List<string>> Args => [[VariablePadItem.ShortName_Variable], StringVal, StringVal, BoolVal, BoolVal];
    public override string Command => "connectpoints";
    public override List<string> Constants => [];
    public override string Description => "Verschiebt das vorhandene PadItem indem es versucht, die angegebenen Punkte zu verbinden.\r\nWird keinen Fehler auslösen.";
    public override bool GetCodeBlockAfter => false;
    public override int LastArgMinCount => -1;
    public override MethodType MethodType => MethodType.Standard;
    public override bool MustUseReturnValue => false;
    public override string Returns => string.Empty;
    public override string StartSequence => "(";
    public override string Syntax => "ConnectPoints(PadItem, PointInItem, OtherPoint, ConnectX, ConnectY);";

    #endregion

    #region Methods

    public override DoItFeedback DoIt(VariableCollection varCol, SplittedAttributesFeedback attvar, ScriptProperties scp, LogData ld) {
        //if (attvar.ReadOnly(0)) { return DoItFeedback.Schreibgschützt(ld); }

        //if (attvar.Attributes[0] is not VariableItemCollectionPad icp) { return DoItFeedback.InternerFehler(ld); }
        //if (icp.ValueItemCollection is not { IsDisposed: false } icpv) { return DoItFeedback.InternerFehler(ld); }

        if (attvar.Attributes[0] is not VariablePadItem ici) { return DoItFeedback.InternerFehler(ld); }
        if (ici.ValuePadItem is not { IsDisposed: false } iciv) { return DoItFeedback.InternerFehler(ld); }

        if (iciv.Parent is not {IsDisposed: false }) { return new DoItFeedback(ld, "Das Item gehört keiner Collection an"); }


        if(iciv.JointPoints.Count == 0) {            return DoItFeedback.Null();}

        iciv.ConnectJointPoint(iciv, attvar.ValueStringGet(1), attvar.ValueStringGet(2), attvar.ValueBoolGet(3), attvar.ValueBoolGet(4));



        return DoItFeedback.Null();

        //return new DoItFeedback(ld, "Keine übereinstimmenden JointPoints gefunden.");


    }

    #endregion
}