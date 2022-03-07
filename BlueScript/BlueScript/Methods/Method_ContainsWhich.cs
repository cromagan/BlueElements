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

#nullable enable

using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using BlueBasics;
using BlueScript.Structuren;
using Skript.Enums;

namespace BlueScript.Methods {

    internal class Method_ContainsWhitch : Method {

        #region Properties

        public override List<enVariableDataType> Args => new() { enVariableDataType.String_or_List, enVariableDataType.String_or_List };

        public override string Description => "Prüft ob eine der Zeichenketten als ganzes Wort vorkommt. Gibt dann als Liste alle gefundenen Strings zurück.";

        public override bool EndlessArgs => true;

        public override string EndSequence => ")";

        public override bool GetCodeBlockAfter => false;

        public override enVariableDataType Returns => enVariableDataType.List;

        public override string StartSequence => "(";

        public override string Syntax => "ContainsWhich(String, Value1, Value2, ...)";

        #endregion

        #region Methods

        public override List<string> Comand(Script? s) => new() { "containswhich" };

        public override DoItFeedback DoIt(CanDoFeedback infos, Script s) {
            var attvar = SplitAttributeToVars(infos.AttributText, s, Args, EndlessArgs);
            if (!string.IsNullOrEmpty(attvar.ErrorMessage)) { return DoItFeedback.AttributFehler(this, attvar); }

            var found = new List<string>();

            #region Wortliste erzeugen

            var wordlist = new List<string>();

            for (var z = 1; z < attvar.Attributes.Count; z++) {
                if (attvar.Attributes[z].Type == enVariableDataType.String) { wordlist.Add(attvar.Attributes[z].ValueString); }
                if (attvar.Attributes[z].Type == enVariableDataType.List) { wordlist.AddRange(attvar.Attributes[z].ValueListString); }
            }
            wordlist = wordlist.SortedDistinctList();

            #endregion

            const RegexOptions rx = RegexOptions.IgnoreCase;
            //if (attvar.Attributes[1].ValueBool) { rx = RegexOptions.IgnoreCase; }

            foreach (var thisW in wordlist.Where(thisW => attvar.Attributes[0].ValueString.ContainsWord(thisW, rx))) {
                found.AddIfNotExists(thisW);
            }

            return new DoItFeedback(found.JoinWithCr() + "\r", enVariableDataType.List);
        }

        #endregion
    }
}