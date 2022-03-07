﻿// Authors:
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

using System.Collections.Generic;
using BlueBasics;
using BlueScript;
using BlueScript.Structuren;
using Skript.Enums;

namespace BlueDatabase.AdditionalScriptComands {

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

        public static RowItem? ObjectToRow(Variable? attribute) {
            if (attribute == null || !attribute.ObjectType("row")) { return null; }

            var d = attribute.ObjectData();
            if (d.ToUpper() == "NULL") { return null; }

            var d2 = d.SplitAndCutBy("|");

            var db = Database.GetByFilename(d2[0], true, false);

            return db?.Row.SearchByKey(long.Parse(d2[1]));
        }

        public static DoItFeedback RowToObjectFeedback(RowItem? row) => row == null
? new DoItFeedback("NULL", "row")
: new DoItFeedback(row.Database.Filename + "|" + row.Key, "row");

        public override List<string> Comand(Script? s) => new() { "row" };

        public override DoItFeedback DoIt(CanDoFeedback infos, Script s) {
            var attvar = SplitAttributeToVars(infos.AttributText, s, Args, EndlessArgs);
            if (!string.IsNullOrEmpty(attvar.ErrorMessage)) { return DoItFeedback.AttributFehler(this, attvar); }

            var allFi = Method_Filter.ObjectToFilter(attvar.Attributes, 0);
            if (allFi is null) { return new DoItFeedback("Fehler im Filter"); }

            var r = RowCollection.MatchesTo(allFi);

            return r == null || r.Count is 0 or > 1 ? RowToObjectFeedback(null) : RowToObjectFeedback(r[0]);
        }

        #endregion
    }
}