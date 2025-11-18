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
using BlueScript.Structures;
using BlueScript.Variables;
using System;
using System.Collections.Generic;

namespace BlueScript.Methods;


internal class Method_IndexOf : Method {

    #region Properties

    public override List<List<string>> Args => [[VariableString.ShortName_Variable, VariableListString.ShortName_Variable], BoolVal, [VariableString.ShortName_Plain, VariableListString.ShortName_Plain]];

    public override string Command => "indexof";

    public override List<string> Constants => [];
    public override string Description => "Bei String:\r\nSucht im ersten String nach dem zweiten String und gibt dessen Position zurück.\r\nBei Listen:\r\nSucht in der Liste den zweiten String.\r\nAllgemein:\r\nWird er nicht gefunden, wird -1 zurück gegeben. Wird er an erster Position gefunden, wird 0 zurück gegeben.";

    public override bool GetCodeBlockAfter => false;
    public override int LastArgMinCount => -1;
    public override MethodType MethodLevel => MethodType.Standard;
    public override bool MustUseReturnValue => true;
    public override string Returns => VariableString.ShortName_Plain;
    public override string StartSequence => "(";
    public override string Syntax => "IndexOf(ListVariable/StringVariable, CaseSensitive, Value)";

    #endregion

    #region Methods

    public override DoItFeedback DoIt(VariableCollection varCol, SplittedAttributesFeedback attvar, ScriptProperties scp, LogData ld) {
        var search = attvar.ValueStringGet(2);
        var sens = attvar.ValueBoolGet(1) ? StringComparison.Ordinal : StringComparison.OrdinalIgnoreCase;
        var pos = -1;

        if (attvar.Attributes[0] is VariableString v) {
            pos = v.ValueString.IndexOf(search, sens);
        }

        if (attvar.Attributes[0] is VariableListString vl) {
            pos = vl.ValueList.FindIndex(x => x.Equals(search, sens));
        }

        return new DoItFeedback(pos);
    }

    #endregion
}