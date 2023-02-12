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
using BlueScript;
using BlueScript.Structures;
using BlueScript.Variables;

namespace BlueDatabase.AdditionalScriptComands;

public class Method_Row : MethodDatabase {

    #region Properties

    public override List<List<string>> Args => new() { new List<string> { VariableFilterItem.ShortName_Variable } };
    public override string Description => "Sucht eine Zeile mittels dem gegebenen Filter.";

    public override bool EndlessArgs => true;

    public override string EndSequence => ")";

    public override bool GetCodeBlockAfter => false;

    public override string Returns => VariableRowItem.ShortName_Variable;

    public override string StartSequence => "(";

    public override string Syntax => "Row(Filter, ...)";

    #endregion

    #region Methods

    public static RowItem? ObjectToRow(Variable? attribute) {
        if (attribute is not VariableRowItem vro) { return null; }

        //var d = attribute.ObjectData();
        //if (d.ToUpper() == "NULL") { return null; }

        //var d2 = d.SplitAndCutBy("|");

        //var db = Database.GetByFilename(d2[0], true, false);

        //return db?.Row.SearchByKey(LongParse(d2[1]));

        return vro.RowItem;
    }

    public static DoItFeedback RowToObjectFeedback(RowItem? row, int line) => new(new VariableRowItem(row), line);

    public override List<string> Comand(Script? s) => new() { "row" };

    public override DoItFeedback DoIt(CanDoFeedback infos, Script s, int line) {
        var attvar = SplitAttributeToVars(infos.AttributText, s, Args, EndlessArgs, line);
        if (!string.IsNullOrEmpty(attvar.ErrorMessage)) { return DoItFeedback.AttributFehler(this, attvar, line); }

        var allFi = Method_Filter.ObjectToFilter(attvar.Attributes, 0);
        if (allFi is null) { return new DoItFeedback("Fehler im Filter", line); }

        var r = RowCollection.MatchesTo(allFi);

        if (r.Count > 1) { return new DoItFeedback("Datenbankfehler, zu viele Einträge gefunden. Zuvor Prüfen mit RowCount.", line); }

        if (r == null || r.Count is 0 or > 1) {
            return RowToObjectFeedback(null, line);
        }

        return RowToObjectFeedback(r[0], line);
    }

    #endregion
}