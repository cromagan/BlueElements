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

using BlueControls.Controls;
using BlueScript;
using BlueScript.Enums;
using BlueScript.Structures;
using BlueScript.Variables;
using System.Collections.Generic;

namespace BlueDatabase.AdditionalScriptMethods;

// ReSharper disable once ClassNeverInstantiated.Global
public class Method_RowNext : Method_TableGeneric {

    #region Properties

    public override List<List<string>> Args => [[Variable.Any_Variable], BoolVal, FilterVar];
    public override string Command => "rownext";
    public override List<string> Constants => [];

    public override string Description => "Sucht Zeilen mittels dem gegebenen Filter und sortiert diese.\r\n" +
                                          "Sucht in diesen Zeilen die eigene Zeile und gibt die darauffolgende zurück.\r\n" +
                                          "Mit RowIsNull kann abgefragt werden, ob die Zeile gefunden wurde.\r\n" +
                                          "Wird kein Filter angegeben, werden alle Zeilen berücksichtigt.\r\n" +
                                          "Ist die eigene Zeile nicht in der Liste, wird null zurückgegeben.";

    public override bool GetCodeBlockAfter => false;
    public override int LastArgMinCount => 0;
    public override MethodType MethodType => MethodType.Database | MethodType.MyDatabaseRow;
    public override bool MustUseReturnValue => true;
    public override string Returns => VariableRowItem.ShortName_Variable;
    public override string StartSequence => "(";
    public override string Syntax => "RowNext(FilterColumn, Filter A->Z, Filter, ...)";

    #endregion

    #region Methods

    public override DoItFeedback DoIt(VariableCollection varCol, SplittedAttributesFeedback attvar, ScriptProperties scp, LogData ld) {
        var mr = MyRow(scp);
        if (mr is not { IsDisposed: false }) { return DoItFeedback.InternerFehler(ld); }

        var column = Column(scp, attvar, 0);
        if (column is not { IsDisposed: false }) { return new DoItFeedback("Spalte nicht gefunden: " + attvar.Name(0), true, ld); }

        if (mr.Database is not { IsDisposed: false } myDb) { return DoItFeedback.InternerFehler(ld); }

        if (myDb != column.Database) { return DoItFeedback.InternerFehler(ld); }

        List<RowItem> r = [];
        if (attvar.Attributes.Count > 2) {
            var (allFi, failedReason, needsScriptFix) = Method_Filter.ObjectToFilter(attvar.Attributes, 2, myDb, scp.ScriptName, true);
            if (allFi == null || !string.IsNullOrEmpty(failedReason)) { return new DoItFeedback($"Filter-Fehler: {failedReason}", needsScriptFix, ld); }
            r.AddRange(allFi.Rows);
            allFi.Dispose();
        } else {
            r.AddRange(myDb.Row);
        }

        if (r.Count < 2) { return Method_Row.RowToObjectFeedback(null); }

        if (mr.Database != r[0].Database) { return new DoItFeedback("Filterfehler, falsche Tabelle", true, ld); }

        if (!r.Contains(mr)) { return Method_Row.RowToObjectFeedback(null); }

        var rsd = new RowSortDefinition(myDb, column, attvar.ValueBoolGet(1));
        var (rows, _) = Table.CalculateSortedRows(myDb, r, null, rsd);

        var givebackrow = -1;

        for (var z = 0; z < rows.Count; z++) {
            if (rows[z].Row == mr) {
                givebackrow = z + 1;
                break;
            }
        }

        return givebackrow < 0 || givebackrow >= rows.Count
            ? Method_Row.RowToObjectFeedback(null)
            : Method_Row.RowToObjectFeedback(rows[givebackrow].Row);
    }

    #endregion
}