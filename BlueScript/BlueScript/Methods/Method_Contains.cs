// Authors:
// Christian Peter
//
// Copyright (c) 2022 Christian Peter
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
using Skript.Enums;
using System.Collections.Generic;
using System.Linq;
using BlueScript.Structuren;

namespace BlueScript {

    internal class Method_Contains : Method {

        #region Properties

        public override List<enVariableDataType> Args => new() { enVariableDataType.Variable_List_Or_String, enVariableDataType.Bool, enVariableDataType.String_or_List };

        public override string Description => "Bei Listen: Prüft, ob einer der Werte in der Liste steht. Bei String: Prüft ob eine der Zeichenketten vorkommt.";

        public override bool EndlessArgs => true;

        public override string EndSequence => ")";

        public override bool GetCodeBlockAfter => false;

        public override enVariableDataType Returns => enVariableDataType.Bool;

        public override string StartSequence => "(";

        public override string Syntax => "Contains(ListVariable/StringVariable, CaseSensitive, Value1, Value2, ...)";

        #endregion

        #region Methods

        public override List<string> Comand(Script s) => new() { "contains" };

        public override strDoItFeedback DoIt(strCanDoFeedback infos, Script s) {
            var attvar = SplitAttributeToVars(infos.AttributText, s, Args, EndlessArgs);
            if (!string.IsNullOrEmpty(attvar.ErrorMessage)) { return strDoItFeedback.AttributFehler(this, attvar); }

            #region Wortliste erzeugen

            var wordlist = new List<string>();

            for (var z = 2; z < attvar.Attributes.Count; z++) {
                if (attvar.Attributes[z].Type == enVariableDataType.String) { wordlist.Add(attvar.Attributes[z].ValueString); }
                if (attvar.Attributes[z].Type == enVariableDataType.List) { wordlist.AddRange(attvar.Attributes[z].ValueListString); }
            }
            wordlist = wordlist.SortedDistinctList();

            #endregion

            if (attvar.Attributes[0].Type == enVariableDataType.List) {
                var x = attvar.Attributes[0].ValueListString;
                if (wordlist.Any(thisW => x.Contains(thisW, attvar.Attributes[1].ValueBool))) {
                    return strDoItFeedback.Wahr();
                }
                return strDoItFeedback.Falsch();
            }

            if (attvar.Attributes[0].Type == enVariableDataType.String) {
                foreach (var thisW in wordlist) {
                    if (attvar.Attributes[1].ValueBool) {
                        if (attvar.Attributes[0].ValueString.Contains(thisW)) {
                            return strDoItFeedback.Wahr();
                        }
                    } else {
                        if (attvar.Attributes[0].ValueString.ToLower().Contains(thisW.ToLower())) {
                            return strDoItFeedback.Wahr();
                        }
                    }
                }
                return strDoItFeedback.Falsch();
            }

            return strDoItFeedback.FalscherDatentyp();
        }

        #endregion
    }
}