// Authors:
// Christian Peter
//
// Copyright © 2026 Christian Peter
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

using BlueBasics;
using BlueScript.Classes;
using BlueScript.Enums;
using BlueScript.Variables;
using System;
using System.Collections.Generic;
using System.Linq;

namespace BlueScript.Methods;

internal class Method_Contains : Method {

    #region Properties

    public override List<List<string>> Args => [[VariableString.ShortName_Variable, VariableListString.ShortName_Variable], BoolVal, [VariableString.ShortName_Plain, VariableListString.ShortName_Plain]];
    public override string Command => "contains";
    public override List<string> Constants => [];
    public override string Description => "Bei Listen: Prüft, ob einer der Werte in der Liste steht. Bei String: Prüft ob eine der Zeichenketten vorkommt.";
    public override bool GetCodeBlockAfter => false;
    public override int LastArgMinCount => 1;
    public override MethodType MethodLevel => MethodType.Standard;
    public override bool MustUseReturnValue => true;
    public override string Returns => VariableBool.ShortName_Plain;
    public override string StartSequence => "(";

    public override string Syntax => "Contains(ListVariable/StringVariable, CaseSensitive, Value1, Value2, ...)";

    #endregion

    #region Methods

    public override DoItFeedback DoIt(VariableCollection varCol, SplittedAttributesFeedback attvar, ScriptProperties scp, LogData ld) {

        #region Wortliste erzeugen

        var wordlist = new List<string>();

        for (var z = 2; z < attvar.Attributes.Count; z++) {
            if (attvar.Attributes[z] is VariableString vs1) { wordlist.Add(vs1.ValueString); }
            if (attvar.Attributes[z] is VariableListString vl1) { wordlist.AddRange(vl1.ValueList); }
        }
        wordlist = wordlist.SortedDistinctList();

        #endregion

        // Der Comparer muss hier definiert werden, damit er für beide Blöcke gültig ist.
        var comparer = attvar.ValueBoolGet(1) ? StringComparer.Ordinal : StringComparer.OrdinalIgnoreCase;
        var comparison = attvar.ValueBoolGet(1) ? StringComparison.Ordinal : StringComparison.OrdinalIgnoreCase;

        if (attvar.Attributes[0] is VariableListString vl2) {
            var x = vl2.ValueList;
            return wordlist.Exists(thisW => x.Contains(thisW, comparer)) ? DoItFeedback.Wahr() : DoItFeedback.Falsch();
        }

        if (attvar.Attributes[0] is VariableString vs2) {
            foreach (var thisW in wordlist) {
                // String.Contains benötigt StringComparison, nicht StringComparer.
                if (vs2.ValueString.Contains(thisW, comparison)) {
                    return DoItFeedback.Wahr();
                }
            }
            return DoItFeedback.Falsch();
        }

        return DoItFeedback.FalscherDatentyp(ld);
    }

    #endregion
}