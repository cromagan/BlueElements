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

public class Method_Row : Method_Database {

    #region Properties

    public override List<List<string>> Args => new() { new List<string> { VariableFilterItem.ShortName_Variable } };

    public override string Description => "Sucht eine Zeile mittels dem gegebenen Filter.\r\n" +
                                          "Wird keine Zeile gefunden, wird ein leeres Zeilenobjekt erstellt. Es wird keine neue Zeile erstellt.\r\n" +
                                          "Mit RowIsNull kann abgefragt werden, ob die Zeile gefunden wurde.\r\n" +
                                          "Werden mehrere Zeilen gefunden, wird das Programm abgebrochen. Um das zu verhindern, kann RowCount benutzt werden.";

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

    public static DoItFeedback RowToObjectFeedback(Script s, CanDoFeedback? infos, RowItem? row) => new(s, infos, new VariableRowItem(row));

    public override List<string> Comand(Script? s) => new() { "row" };

    public override DoItFeedback DoIt(Script s, CanDoFeedback infos) {
        var attvar = SplitAttributeToVars(s, infos.AttributText, Args, EndlessArgs);
        if (!string.IsNullOrEmpty(attvar.ErrorMessage)) { return DoItFeedback.AttributFehler(s, infos, this, attvar); }

        var allFi = Method_Filter.ObjectToFilter(attvar.Attributes, 0);
        if (allFi is null) { return new DoItFeedback(s, infos, "Fehler im Filter"); }

        var r = RowCollection.MatchesTo(allFi);

        if (r.Count > 1) { return new DoItFeedback(s, infos, "Datenbankfehler, zu viele Einträge gefunden. Zuvor Prüfen mit RowCount."); }

        if (r == null || r.Count is 0 or > 1) {
            return RowToObjectFeedback(s, infos, null);
        }

        return RowToObjectFeedback(s, infos, r[0]);
    }

    #endregion
}