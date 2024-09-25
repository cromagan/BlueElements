﻿// Authors:
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
internal class Method_AddPadItem : Method {

    #region Properties

    public override List<List<string>> Args => [[VariableItemCollectionPad.ShortName_Variable], [VariablePadItem.ShortName_Variable]];
    public override string Command => "addpaditem";
    public override List<string> Constants => [];
    public override string Description => "Fügt einer ItemCollectionPad ein PadItem hinzu.\r\nAnschließend wird es mit JointPoints ausgerichtet.";
    public override bool GetCodeBlockAfter => false;
    public override int LastArgMinCount => -1;
    public override MethodType MethodType => MethodType.Standard;
    public override bool MustUseReturnValue => false;
    public override string Returns => string.Empty;
    public override string StartSequence => "(";
    public override string Syntax => "AddPadItem(Collection, PadItem);";

    #endregion

    #region Methods

    public override DoItFeedback DoIt(VariableCollection varCol, SplittedAttributesFeedback attvar, ScriptProperties scp, LogData ld) {
        if (attvar.ReadOnly(0)) { return DoItFeedback.Schreibgschützt(ld); }

        if (attvar.Attributes[0] is not VariableItemCollectionPad icp) { return DoItFeedback.InternerFehler(ld); }
        if (icp.ValueItemCollection is not { IsDisposed: false } icpv) { return DoItFeedback.InternerFehler(ld); }

        if (attvar.Attributes[1] is not VariablePadItem ici) { return DoItFeedback.InternerFehler(ld); }
        if (ici.ValuePadItem is not { IsDisposed: false } iciv) { return DoItFeedback.InternerFehler(ld); }

        if (iciv.Parent != null) { return new DoItFeedback(ld, "Das Item gehört breits einer Collection an"); }

        icpv.Add(iciv);

        if (iciv.JointPoints.Count == 0) { return new DoItFeedback(ld, "Das Item hat keine Jointpoints."); }


        foreach (var pt in iciv.JointPoints) {
            var p = icpv.GetJointPoint(pt.KeyName, iciv);
            if (p != null) {
                iciv.ConnectJointPoint(pt, p);
                return DoItFeedback.Null();
            }
        }


        return DoItFeedback.Null();
    }

    #endregion
}