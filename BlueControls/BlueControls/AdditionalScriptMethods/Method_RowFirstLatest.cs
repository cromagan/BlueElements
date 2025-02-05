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

using System.Collections.Generic;
using BlueControls.Controls;
using BlueScript;
using BlueScript.Enums;
using BlueScript.Structures;
using BlueScript.Variables;

namespace BlueDatabase.AdditionalScriptMethods;

// ReSharper disable once ClassNeverInstantiated.Global
public class Method_RowFirstLatest : Method_Database {

    #region Properties

    public override List<List<string>> Args => [StringVal, BoolVal, FilterVar];
    public override string Command => "rowfirstlatest";
    public override List<string> Constants => [];

    public override string Description => "Sucht eine Zeilen mittels dem gegebenen Filter und sortiert diese.\r\n" +
                                          "Mit RowIsNull kann abgefragt werden, ob die Zeile gefunden wurde.\r\n" +
                                          "Gibt dann die erste bwz. letzte Zeile zurück.";

    public override bool GetCodeBlockAfter => false;
    public override int LastArgMinCount => 1;
    public override MethodType MethodType => MethodType.Database;
    public override bool MustUseReturnValue => true;
    public override string Returns => VariableRowItem.ShortName_Variable;
    public override string StartSequence => "(";
    public override string Syntax => "RowFirstLatest(SortColumn, first, Filter, ...)";

    #endregion

    #region Methods

    public override DoItFeedback DoIt(VariableCollection varCol, SplittedAttributesFeedback attvar, ScriptProperties scp, LogData ld) {
        var (allFi, errorreason) = Method_Filter.ObjectToFilter(attvar.Attributes, 2, MyDatabase(scp), scp.ScriptName, true);
        if (allFi == null || !string.IsNullOrEmpty(errorreason)) { return new DoItFeedback(ld, $"Filter-Fehler: {errorreason}"); }

        if (allFi.Database is not { } db) { return DoItFeedback.InternerFehler(ld); }

        var r = allFi.Rows;
        allFi.Dispose();

        var column = db.Column[attvar.ValueStringGet(0)];
        if (column is not { IsDisposed: false }) { return new DoItFeedback(ld, "Spalte nicht gefunden: " + attvar.Name(0)); }

        if (r.Count < 1) { return Method_Row.RowToObjectFeedback(null); }

        var rsd = new RowSortDefinition(db, column, attvar.ValueBoolGet(1));
        var (rows, _) = Table.CalculateSortedRows(db, r, null, rsd);
        return Method_Row.RowToObjectFeedback(rows[0].Row);
    }

    #endregion
}