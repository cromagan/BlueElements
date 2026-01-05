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

using BlueBasics;
using BlueBasics.Interfaces;
using BlueControls.Editoren;
using BlueScript.Variables;
using BlueTable;
using BlueTable.EventArgs;
using System.Collections.Generic;

namespace BlueControls;

public partial class VariableEditor : EditorEasy {

    #region Constructors

    public VariableEditor() : base() => InitializeComponent();

    #endregion

    #region Methods

    public override void Clear() => tableVariablen.Table?.Row.Clear("Variablen gelöscht");

    public VariableCollection? GetCloneOfCurrent() {
        if (ToEdit is null) { return null; }
        if (!string.IsNullOrEmpty(Error)) { return null; }

        if (!Editable || IsDisposed) {
            Develop.DebugPrint_NichtImplementiert(true);
            // Bei Editable TRUE sind es nur string variablen
        }
        var list = new VariableCollection();

        if (tableVariablen.Table is not { IsDisposed: false } tb) { return list; }

        foreach (var thisr in tb.Row) {
            var v = new VariableString(thisr.CellGetString("Name"), thisr.CellGetString("Inhalt"), false, thisr.CellGetString("Kommentar"));
            list.Add(v);
        }

        return list;
    }

    public RowItem? RowOfVariable(string variable) => tableVariablen?.Table is not { IsDisposed: false } tb ? null : tb.Row[variable];

    public RowItem? RowOfVariable(Variable variable) => IsDisposed || tableVariablen?.Table is not { IsDisposed: false } tb ? null : tb.Row[variable.KeyName];

    protected override void InitializeComponentDefaultValues() {
        var tb = Table.Get();
        var na = tb.Column.GenerateAndAdd("Name", "N", ColumnFormatHolder.SystemName, "Variablenname");
        na.IsFirst = true;
        tb.Column.GenerateAndAdd("Typ", "T", ColumnFormatHolder.Text, "Variablentyp");
        tb.Column.GenerateAndAdd("RO", "R", ColumnFormatHolder.Bit, "Readonly, Schreibgeschützt");
        var inh = tb.Column.GenerateAndAdd("Inhalt", "I", ColumnFormatHolder.TextMitFormatierung, "Inhalt");
        var kom = tb.Column.GenerateAndAdd("Kommentar", "K", ColumnFormatHolder.Text, "Kommentar");

        foreach (var thisColumn in tb.Column) {
            if (!thisColumn.IsSystemColumn()) {
                thisColumn.MultiLine = true;
                thisColumn.EditableWithTextInput = false;
                thisColumn.EditableWithDropdown = false;
            }
        }

        tb.Column.GenerateAndAddSystem();

        if (Editable) {
            List<ColumnItem?> l = [na, inh, kom];

            foreach (var thisColumn2 in l) {
                if (thisColumn2 != null) {
                    thisColumn2.EditableWithTextInput = true;
                    thisColumn2.MultiLine = false;
                    thisColumn2.PermissionGroupsChangeCell = new([Constants.Everybody]);
                }
            }

            na?.Caption = "Variablen-\rName";
            inh?.Caption = "Inhalt";
            kom?.Caption = "Kommentar";

            tb.PermissionGroupsNewRow = new([Constants.Everybody]);
        }

        var tcvc = ColumnViewCollection.ParseAll(tb);

        //if (car != null) {
        if (Editable) {
            tcvc[0].ShowColumns("Name", "Inhalt", "Kommentar");
            tcvc[1].ShowColumns("Name", "Inhalt", "Kommentar");
        } else {
            tcvc[0].ShowColumns("Name", "Typ", "RO", "System", "Inhalt", "Kommentar");
            tcvc[1].ShowColumns("Name", "Typ", "RO", "System", "Inhalt", "Kommentar");
        }

        tb.RepairAfterParse();
        tb.ColumnArrangements = tcvc.ToString(false);

        tb.SortDefinition = new RowSortDefinition(tb, na, true);

        tableVariablen.TableSet(tb, string.Empty);

        tb.Cell.CellValueChanged += TableVariablen_CellValueChanged;
    }

    protected override bool SetValuesToFormula(IEditable? toEdit) {
        if (IsDisposed || tableVariablen?.Table is not { IsDisposed: false } tb) { return false; }
        if (toEdit is not VariableCollection vc) { return false; }

        foreach (var thisv in vc) {
            var ro = RowOfVariable(thisv) ?? tb.Row.GenerateAndAdd(thisv.KeyName, "Neue Variable");

            if (ro != null) {
                ro.CellSet("typ", thisv.MyClassId, string.Empty);
                ro.CellSet("RO", thisv.ReadOnly, string.Empty);

                var tmpi = thisv.ReadableText;
                if (!Editable && tmpi.Length > 3990) {
                    tmpi = tmpi.Substring(0, 3990) + "...";
                }

                ro.CellSet("Inhalt", tmpi, string.Empty);
                ro.CellSet("Kommentar", thisv.Comment, string.Empty);
            }
        }

        return true;
    }

    private void TableVariablen_CellValueChanged(object sender, CellEventArgs e) {
        var c = tableVariablen.Table?.Column.First;
        if (e.Column == c && string.IsNullOrEmpty(e.Row.CellGetString(c))) {
            RowCollection.Remove(e.Row, "Variable gelöscht");
        }
    }

    #endregion
}