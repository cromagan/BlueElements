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
using BlueDatabase;
using Skript.Enums;
using System.Collections.Generic;

namespace BlueScript {

    public class Method_Row : MethodDatabase {

        #region Properties

        public override List<enVariableDataType> Args => new() { enVariableDataType.Object };

        public override string Description => "Sucht eine Zeile mittels dem gegebenen Filter.";

        public override bool EndlessArgs => true;

        public override string EndSequence => ")";

        public override bool GetCodeBlockAfter => false;

        public override enVariableDataType Returns => enVariableDataType.Object;

        public override string StartSequence => "(";

        public override string Syntax => "Row(Filter, ...)";

        #endregion

        #region Methods

        public static RowItem ObjectToRow(Variable attribute) {
            if (!attribute.ObjectType("row")) { return null; }

            var d = attribute.ObjectData();
            if (d.ToUpper() == "NULL") { return null; }

            var d2 = d.SplitAndCutBy("|");

            var db = Database.GetByFilename(d2[0], true, false);

            return db?.Row.SearchByKey(long.Parse(d2[1]));
        }

        public static strDoItFeedback RowToObject(RowItem row) => row == null
                        ? new strDoItFeedback("NULL", "row")
                : new strDoItFeedback(row.Database.Filename + "|" + row.Key.ToString(), "row");

        public override List<string> Comand(Script s) => new() { "row" };

        public override strDoItFeedback DoIt(strCanDoFeedback infos, Script s) {
            var attvar = SplitAttributeToVars(infos.AttributText, s, Args, EndlessArgs);
            if (!string.IsNullOrEmpty(attvar.ErrorMessage)) { return strDoItFeedback.AttributFehler(this, attvar); }

            var allFi = Method_Filter.ObjectToFilter(attvar.Attributes, 0);
            if (allFi is null) { return new strDoItFeedback("Fehler im Filter"); }

            var r = RowCollection.MatchesTo(allFi);

            return r == null || r.Count == 0 || r.Count > 1 ? RowToObject(null) : RowToObject(r[0]);
        }

        #endregion
    }
}