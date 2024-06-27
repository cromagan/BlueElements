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
using BlueScript.Structures;
using BlueScript.Variables;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace BlueScript.Methods;

// ReSharper disable once UnusedMember.Global
[SuppressMessage("Microsoft.Performance", "CA1812:AvoidUninstantiatedInternalClasses")]
internal class Method_Compare : Method {

    #region Properties

    public override List<List<string>> Args => [BoolVal, BoolVal, [VariableString.ShortName_Plain, VariableFloat.ShortName_Plain, VariableBool.ShortName_Plain]];
    public override string Command => "compare";

    public override string Description => "Diese Routine vergleicht Werte mit einander und gibt true zurück, wenn diese gleich sind. Dabei müssen die Datentypen übereinstimmen.\r\n" +
                                           "Bei IgnoreNullOrEmpty wird bei Zahlen ebenfalls 0 ignoriert";

    public override bool GetCodeBlockAfter => false;
    public override int LastArgMinCount => 2;
    public override MethodType MethodType => MethodType.Standard;
    public override bool MustUseReturnValue => true;
    public override string Returns => VariableBool.ShortName_Plain;
    public override string StartSequence => "(";
    public override string Syntax => "Compare(IgnoreNullOrEmpty, CaseSensitive, Value1, ...);";

    #endregion

    #region Methods

    public override DoItFeedback DoIt(VariableCollection varCol, CanDoFeedback infos, ScriptProperties scp) {
        var attvar = SplitAttributeToVars(varCol, infos.AttributText, Args, LastArgMinCount, infos.Data, scp);
        if (!string.IsNullOrEmpty(attvar.ErrorMessage)) { return DoItFeedback.AttributFehler(infos.Data, this, attvar); }

        var ignorenull = attvar.ValueBoolGet(0);
        var cases = attvar.ValueBoolGet(1);

        string? firstval = null;

        for (var z = 2; z < attvar.Attributes.Count; z++) {
            if (attvar.MyClassId(z) != attvar.MyClassId(2)) { return new DoItFeedback(infos.Data, "Variablentypen unterschiedlich."); }

            var hasval = !ignorenull;
            var val = string.Empty;

            switch (attvar.Attributes[z]) {
                case VariableFloat vf:
                    if (!hasval && vf.ValueNum != 0) { hasval = true; }
                    val = vf.ValueForReplace;
                    break;

                case VariableString vs:
                    if (!hasval && !string.IsNullOrEmpty(vs.ValueString)) { hasval = true; }
                    val = vs.ValueForReplace;
                    break;

                case VariableBool vb:
                    hasval = true;
                    val = vb.ValueForReplace;
                    break;
            }

            if (hasval) {
                if (!cases) { val = val.ToUpperInvariant(); }
                firstval ??= val;

                if (val != firstval) { return DoItFeedback.Falsch(); }
            }
        }

        return DoItFeedback.Wahr();
    }

    #endregion
}