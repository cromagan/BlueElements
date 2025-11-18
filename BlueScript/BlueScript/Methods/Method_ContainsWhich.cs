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

using BlueBasics;
using BlueScript.Enums;
using BlueScript.Structures;
using BlueScript.Variables;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace BlueScript.Methods;


internal class Method_ContainsWhitch : Method {

    #region Properties

    public override List<List<string>> Args => [[VariableString.ShortName_Plain, VariableListString.ShortName_Plain], BoolVal, [VariableString.ShortName_Plain, VariableListString.ShortName_Plain]];
    public override string Command => "containswhich";
    public override List<string> Constants => [];
    public override string Description => "Prüft ob eine der Zeichenketten als ganzes Wort vorkommt. Gibt dann alle gefundenen Strings als Liste a zurück.\r\nWort bedeutet, dass es als ganzes Wort vorkommen muss: 'Dach' gilt z.B. nicht als 'Hausdach'";
    public override bool GetCodeBlockAfter => false;
    public override int LastArgMinCount => 1;
    public override MethodType MethodLevel => MethodType.Standard;
    public override bool MustUseReturnValue => true;
    public override string Returns => VariableListString.ShortName_Plain;

    public override string StartSequence => "(";

    public override string Syntax => "ContainsWhich(String, CaseSensitive, Value1, Value2, ...)";

    #endregion

    #region Methods

    public override DoItFeedback DoIt(VariableCollection varCol, SplittedAttributesFeedback attvar, ScriptProperties scp, LogData ld) {
        var found = new List<string>();

        #region Wortliste erzeugen

        var wordlist = new List<string>();

        for (var z = 2; z < attvar.Attributes.Count; z++) {
            if (attvar.Attributes[z] is VariableString vs1) { wordlist.Add(vs1.ValueString); }
            if (attvar.Attributes[z] is VariableListString vl1) { wordlist.AddRange(vl1.ValueList); }
        }
        wordlist = wordlist.SortedDistinctList();

        #endregion

        var rx = RegexOptions.IgnoreCase;
        if (attvar.ValueBoolGet(1)) { rx = RegexOptions.None; }

        if (attvar.Attributes[0] is VariableString vs2) {
            foreach (var thisW in wordlist) {
                if (vs2.ValueString.ContainsWord(thisW, rx)) {
                    found.AddIfNotExists(thisW);
                }
            }
        }

        if (attvar.Attributes[0] is VariableListString vl2) {
            foreach (var thiss in vl2.ValueList) {
                foreach (var thisW in wordlist) {
                    if (thiss.ContainsWord(thisW, rx)) {
                        found.AddIfNotExists(thisW);
                    }
                }
            }
        }

        return new DoItFeedback(found);
    }

    #endregion
}