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

using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using BlueScript.Enums;
using BlueScript.Structures;
using BlueScript.Variables;

namespace BlueScript.Methods;

// ReSharper disable once UnusedMember.Global
[SuppressMessage("Microsoft.Performance", "CA1812:AvoidUninstantiatedInternalClasses")]
internal class Method_Add : Method {

    #region Properties

    public override List<List<string>> Args => [ListStringVar, [VariableString.ShortName_Plain, VariableListString.ShortName_Plain]];
    public override string Command => "add";
    public override string Description => "Fügt einer Liste einen oder mehrere Werte hinzu.";
    public override bool EndlessArgs => true;

    public override bool GetCodeBlockAfter => false;
    public override MethodType MethodType => MethodType.Standard;
    public override bool MustUseReturnValue => false;
    public override string Returns => string.Empty;
    public override string StartSequence => "(";
    public override string Syntax => "Add(List-Variable, Value1, Value2, ...);";

    #endregion

    #region Methods

    public override DoItFeedback DoIt(VariableCollection varCol, CanDoFeedback infos, ScriptProperties scp) {
        var attvar = SplitAttributeToVars(varCol, infos.AttributText, Args, EndlessArgs, infos.Data, scp);
        if (!string.IsNullOrEmpty(attvar.ErrorMessage)) { return DoItFeedback.AttributFehler(infos.Data, this, attvar); }

        if (attvar.ReadOnly(0)) { return DoItFeedback.Schreibgschützt(infos.Data); }

        var tmpList = attvar.ValueListStringGet(0);
        for (var z = 1; z < attvar.Attributes.Count; z++) {
            if (attvar.Attributes[z] is VariableString vs) {
                tmpList.Add(vs.ValueString);
            }
            if (attvar.Attributes[z] is VariableListString vl) {
                tmpList.AddRange(vl.ValueList);
            }
        }

        if (attvar.ValueListStringSet(0, tmpList, infos.Data) is DoItFeedback dif) { return dif; }

        return DoItFeedback.Null();
    }

    #endregion
}