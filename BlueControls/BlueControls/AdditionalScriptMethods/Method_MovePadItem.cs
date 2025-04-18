﻿// Authors:
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
internal class Method_MovePadItem : Method {

    #region Properties

    public override List<List<string>> Args => [[VariablePadItem.ShortName_Variable, VariableItemCollectionPad.ShortName_Variable], FloatVal, FloatVal];
    public override string Command => "movepaditem";
    public override List<string> Constants => [];
    public override string Description => "Verschiebt das vorhandene PadItem (oder alle Paditems in der Sammlung) um die angegebenen Pixel.";
    public override bool GetCodeBlockAfter => false;
    public override int LastArgMinCount => -1;
    public override MethodType MethodType => MethodType.Standard;
    public override bool MustUseReturnValue => false;
    public override string Returns => string.Empty;
    public override string StartSequence => "(";
    public override string Syntax => "MovePadItem(PadItem/Collection, X, Y);";

    #endregion

    #region Methods

    public override DoItFeedback DoIt(VariableCollection varCol, SplittedAttributesFeedback attvar, ScriptProperties scp, LogData ld) {
        if (attvar.Attributes[0] is VariableItemCollectionPad icp) {
            if (icp.ValueItemCollection is not { IsDisposed: false } icpv) { return DoItFeedback.InternerFehler(ld); }
            icpv.Items_Move(attvar.ValueIntGet(1), attvar.ValueIntGet(2));
        }

        if (attvar.Attributes[0] is VariablePadItem ici) {
            if (ici.ValuePadItem is not { IsDisposed: false } iciv) { return DoItFeedback.InternerFehler(ld); }
            if (iciv.Parent is not ItemCollectionPadItem { IsDisposed: false }) { return new DoItFeedback("Das Item gehört keiner Collection an", true, ld); }
            iciv.Move(attvar.ValueIntGet(1), attvar.ValueIntGet(2), false);
        }

        return DoItFeedback.Null();
    }

    #endregion
}