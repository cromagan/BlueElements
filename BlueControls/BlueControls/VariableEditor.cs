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
using System.Windows.Forms;
using BlueBasics;
using BlueDatabase;
using BlueScript.Variables;

namespace BlueControls;

public partial class VariableEditor : UserControl {

    #region Fields

    private bool _inited;

    #endregion

    #region Constructors

    public VariableEditor() => InitializeComponent();

    #endregion

    #region Properties

    public bool Editabe { get; set; }

    #endregion

    #region Methods

    public VariableCollection GetVariables() {
        if (!Editabe) {
            Develop.DebugPrint_NichtImplementiert();
            // Bei Editable TRUE sind es nur string variablen
        }
        var list = new VariableCollection();

        if (filterVariablen?.Table?.Database is not DatabaseAbstract db) { return list; }

        foreach (var thisr in db.Row) {
            var v = new VariableString(thisr.CellGetString("Name"), thisr.CellGetString("Inhalt"), false, false, thisr.CellGetString("Kommentar"));
            list.Add(v);
        }

        return list;
    }

    public RowItem? RowOfVariable(string variable) {
        if (tableVariablen?.Database == null) { return null; }
        return tableVariablen.Database.Row[variable];
    }

    public RowItem? RowOfVariable(Variable variable) {
        if (tableVariablen?.Database == null) { return null; }
        return tableVariablen.Database.Row[variable.Name];
    }

    public void WriteVariablesToTable(IReadOnlyCollection<Variable> variables) {
        var l = new VariableCollection();

        foreach (var v in variables) {
            l.Add(v);
        }
        WriteVariablesToTable(l);
    }

    public void WriteVariablesToTable(VariableCollection? variables) {
        if (!_inited) {
            _inited = true;
            GenerateVariableTable();
        }

        if (tableVariablen?.Database == null) { return; }
        if (variables == null) { return; }

        foreach (var thisv in variables) {
            var ro = RowOfVariable(thisv) ?? tableVariablen.Database.Row.GenerateAndAdd(thisv.Name, "Neue Variable");

            if (ro != null) {
                ro.CellSet("typ", thisv.MyClassId);
                ro.CellSet("RO", thisv.ReadOnly);
                ro.CellSet("System", thisv.SystemVariable);

                var tmpi = thisv.ReadableText;
                if (!Editabe && tmpi.Length > 3990) {
                    tmpi = tmpi.Substring(0, 3990) + "...";
                }

                ro.CellSet("Inhalt", tmpi);
                ro.CellSet("Kommentar", thisv.Comment);
            }
        }
    }

    internal void Clear() => tableVariablen.Database?.Row.Clear("Variablen gelöscht");

    protected override void OnVisibleChanged(System.EventArgs e) {
        base.OnVisibleChanged(e);

        if (Visible && !_inited && !Disposing) {
            _inited = true;
            GenerateVariableTable();
        }
    }

    private void GenerateVariableTable() {
        Database x = new(false, DatabaseAbstract.UniqueKeyValue());
        x.DropMessages = false;
        var na = x.Column.GenerateAndAdd("Name", "N", ColumnFormatHolder.SystemName, "Variablenname");
        _ = x.Column.GenerateAndAdd("Typ", "T", ColumnFormatHolder.Text, "Variablentyp");
        _ = x.Column.GenerateAndAdd("RO", "R", ColumnFormatHolder.Bit, "Readonly, Schreibgeschützt");
        _ = x.Column.GenerateAndAdd("System", "S", ColumnFormatHolder.Bit, "Systemspalte\r\nIm Script nicht verfügbar");
        var inh = x.Column.GenerateAndAdd("Inhalt", "I", ColumnFormatHolder.Text, "Inhalt");
        var kom = x.Column.GenerateAndAdd("Kommentar", "K", ColumnFormatHolder.Text, "Komentar");

        foreach (var thisColumn in x.Column) {
            if (!thisColumn.IsSystemColumn()) {
                thisColumn.MultiLine = true;
                thisColumn.TextBearbeitungErlaubt = false;
                thisColumn.DropdownBearbeitungErlaubt = false;
            }
        }

        x.Column.GenerateAndAddSystem();

        if (Editabe) {
            var l = new List<ColumnItem?> { na, inh, kom };

            foreach (var thisColumn2 in l) {
                if (thisColumn2 != null) {
                    thisColumn2.TextBearbeitungErlaubt = true;
                    thisColumn2.MultiLine = false;
                    thisColumn2.PermissionGroupsChangeCell = new(new List<string> { Constants.Everybody });
                }
            }

            if (na != null) { na.Caption = "Variablen-\rName"; }
            if (inh != null) { inh.Caption = "Inhalt"; }
            if (kom != null) { kom.Caption = "Kommentar"; }

            x.PermissionGroupsNewRow = new(new List<string> { Constants.Everybody });
        }

        x.RepairAfterParse();
        var car = x.ColumnArrangements?.CloneWithClones();

        if (car != null) {
            if (Editabe) {
                car[1].ShowColumns("Name", "Inhalt", "Kommentar");
            } else {
                car[1].ShowColumns("Name", "Typ", "RO", "System", "Inhalt", "Kommentar");
            }
            //car[1].HideSystemColumns();
            x.ColumnArrangements = new(car);
        }

        x.SortDefinition = new RowSortDefinition(x, "Name", true);
        tableVariablen.DatabaseSet(x, string.Empty);
        tableVariablen.Arrangement = 1;
        filterVariablen.Table = tableVariablen;
    }

    #endregion
}