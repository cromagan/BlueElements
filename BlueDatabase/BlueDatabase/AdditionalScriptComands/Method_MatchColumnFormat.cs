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

using Skript.Enums;
using System.Collections.Generic;
using static BlueBasics.Converter;
using static BlueBasics.Extensions;
using static BlueBasics.DataFormat;
using BlueDatabase.Interfaces;

namespace BlueScript {

    internal class Method_MatchColumnFormat : MethodDatabase {

        #region Properties

        public override List<enVariableDataType> Args => new() { enVariableDataType.String_or_List, enVariableDataType.Variable_Any };
        public override string Description => "Prüft, ob der Inhalt der Variable mit dem Format der angegebenen Spalte übereinstimmt. Leere Inhalte sind dabei TRUE.";
        public override bool EndlessArgs => false;
        public override string EndSequence => ")";
        public override bool GetCodeBlockAfter => false;
        public override enVariableDataType Returns => enVariableDataType.Bool;
        public override string StartSequence => "(";
        public override string Syntax => "MatchColumnFormat(Value, Column)";

        #endregion

        #region Methods

        public override List<string> Comand(Script s) => new() { "matchcolumnformat" };

        public override strDoItFeedback DoIt(strCanDoFeedback infos, Script s) {
            var attvar = SplitAttributeToVars(infos.AttributText, s, Args, EndlessArgs);
            if (!string.IsNullOrEmpty(attvar.ErrorMessage)) { return strDoItFeedback.AttributFehler(this, attvar); }

            var column = Column(s, attvar.Attributes[1].Name);
            if (column == null) { new strDoItFeedback("Spalte in Datenbank nicht gefunden"); }

            var tocheck = new List<string>();
            if (attvar.Attributes[0].Type == enVariableDataType.List) {
                tocheck.AddRange(attvar.Attributes[0].ValueListString);
            } else {
                tocheck.Add(attvar.Attributes[0].ValueString);
            }

            tocheck = tocheck.SortedDistinctList();

            foreach (var thisstring in tocheck) {
                if (!thisstring.IsFormat(column)) { return strDoItFeedback.Falsch(); }
            }

            return strDoItFeedback.Wahr();
        }

        #endregion
    }
}