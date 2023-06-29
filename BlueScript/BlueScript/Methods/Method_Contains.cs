// Authors:
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
using System.Linq;
using BlueBasics;
using BlueScript.Enums;
using BlueScript.Structures;
using BlueScript.Variables;

namespace BlueScript.Methods;

internal class Method_Contains : Method {

    #region Properties

    public override List<List<string>> Args => new() { new List<string> { VariableString.ShortName_Variable, VariableListString.ShortName_Variable }, BoolVal, new List<string> { VariableString.ShortName_Plain, VariableListString.ShortName_Plain } };
    public override string Description => "Bei Listen: Prüft, ob einer der Werte in der Liste steht. Bei String: Prüft ob eine der Zeichenketten vorkommt.";
    public override bool EndlessArgs => true;
    public override string EndSequence => ")";
    public override bool GetCodeBlockAfter => false;
    public override MethodType MethodType => MethodType.Standard;
    public override string Returns => VariableBool.ShortName_Plain;
    public override string StartSequence => "(";

    public override string Syntax => "Contains(ListVariable/StringVariable, CaseSensitive, Value1, Value2, ...)";

    #endregion

    #region Methods

    public override List<string> Comand(VariableCollection? currentvariables) => new() { "contains" };

    public override DoItFeedback DoIt(VariableCollection vs, CanDoFeedback infos, ScriptProperties scp) {
        var attvar = SplitAttributeToVars(vs, infos.AttributText, Args, EndlessArgs, infos.Data, scp);
        if (!string.IsNullOrEmpty(attvar.ErrorMessage)) { return DoItFeedback.AttributFehler(infos.Data, this, attvar); }

        #region Wortliste erzeugen

        var wordlist = new List<string>();

        for (var z = 2; z < attvar.Attributes.Count; z++) {
            if (attvar.Attributes[z] is VariableString vsx) { wordlist.Add(vsx.ValueString); }
            if (attvar.Attributes[z] is VariableListString vlx) { wordlist.AddRange(vlx.ValueList); }
        }
        wordlist = wordlist.SortedDistinctList();

        #endregion

        if (attvar.Attributes[0] is VariableListString vl) {
            var x = vl.ValueList;
            if (wordlist.Any(thisW => x.Contains(thisW, attvar.ValueBoolGet(1)))) {
                return DoItFeedback.Wahr();
            }
            return DoItFeedback.Falsch();
        }

        if (attvar.Attributes[0] is VariableString vsxy) {
            foreach (var thisW in wordlist) {
                if (attvar.ValueBoolGet(1)) {
                    if (vsxy.ValueString.Contains(thisW)) {
                        return DoItFeedback.Wahr();
                    }
                } else {
                    if (vsxy.ValueString.ToLower().Contains(thisW.ToLower())) {
                        return DoItFeedback.Wahr();
                    }
                }
            }
            return DoItFeedback.Falsch();
        }

        return DoItFeedback.FalscherDatentyp(infos.Data);
    }

    #endregion
}