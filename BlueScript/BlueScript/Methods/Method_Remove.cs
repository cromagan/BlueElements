﻿// Authors:
// Christian Peter
//
// Copyright (c) 2023 Christian Peter
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
using BlueBasics;
using BlueScript.Enums;
using BlueScript.Structures;
using BlueScript.Variables;

namespace BlueScript.Methods;

// ReSharper disable once UnusedMember.Global
[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1812:AvoidUninstantiatedInternalClasses")]
internal class Method_Remove : Method {

    #region Properties

    public override List<List<string>> Args => new() { ListStringVar, BoolVal, new List<string> { VariableString.ShortName_Plain, VariableListString.ShortName_Plain } };
    public override string Description => "Entfernt aus der Liste die angegebenen Werte.";
    public override bool EndlessArgs => true;
    public override string EndSequence => ");";
    public override bool GetCodeBlockAfter => false;
    public override MethodType MethodType => MethodType.Standard;
    public override string Returns => string.Empty;
    public override string StartSequence => "(";
    public override string Syntax => "Remove(ListVariable, CaseSensitive, Value1, Value2, ...);";

    #endregion

    #region Methods

    public override List<string> Comand(List<Variable>? currentvariables) => new() { "remove" };

    public override DoItFeedback DoIt(Script s, CanDoFeedback infos) {
        var attvar = SplitAttributeToVars(s, infos.AttributText, Args, EndlessArgs, infos.Data);
        if (!string.IsNullOrEmpty(attvar.ErrorMessage)) { return DoItFeedback.AttributFehler(infos.Data, this, attvar); }

        if (attvar.Attributes[0].ReadOnly) { return DoItFeedback.Schreibgschützt(infos.Data); }

        var tmpList = ((VariableListString)attvar.Attributes[0]).ValueList;
        for (var z = 2; z < attvar.Attributes.Count; z++) {
            if (attvar.Attributes[z] is VariableString vs) {
                tmpList!.RemoveString(vs.ValueString, ((VariableBool)attvar.Attributes[1]).ValueBool);
            }
            if (attvar.Attributes[z] is VariableListString vl) {
                tmpList.RemoveString(vl.ValueList, ((VariableBool)attvar.Attributes[1]).ValueBool);
            }
        }
        ((VariableListString)attvar.Attributes[0]).ValueList = tmpList;
        return DoItFeedback.Null();
    }

    #endregion
}