﻿// Authors:
// Christian Peter
//
// Copyright (c) 2024 Christian Peter
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

using BlueBasics;
using BlueBasics.Interfaces;
using BlueControls.Editoren;
using BlueControls.Forms;
using BlueDatabase;
using BlueDatabase.EventArgs;
using BlueScript.Variables;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Forms;
using static System.Net.Mime.MediaTypeNames;

namespace BlueControls;

public partial class VariableEditor : EditorAbstract {

    #region Constructors

    public VariableEditor() : base() => InitializeComponent();

    #endregion

    #region Methods

    public override void Clear() => tableVariablen.Database?.Row.Clear("Variablen gelöscht");

    public VariableCollection GetVariables() {
        if (!Editabe || IsDisposed) {
            Develop.DebugPrint_NichtImplementiert(true);
            // Bei Editable TRUE sind es nur string variablen
        }
        var list = new VariableCollection();

        if (filterVariablen?.Table?.Database is not Database db || db.IsDisposed) { return list; }

        foreach (var thisr in db.Row) {
            var v = new VariableString(thisr.CellGetString("Name"), thisr.CellGetString("Inhalt"), false, thisr.CellGetString("Kommentar"));
            list.Add(v);
        }

        return list;
    }

    public RowItem? RowOfVariable(string variable) {
        if (tableVariablen?.Database is not Database db || db.IsDisposed) { return null; }
        return db.Row[variable];
    }

    public RowItem? RowOfVariable(Variable variable) {
        if (IsDisposed || tableVariablen?.Database is not Database db || db.IsDisposed) { return null; }
        return db.Row[variable.KeyName];
    }

    protected override bool Init(IEditable? variables) {
        if (IsDisposed || tableVariablen?.Database is not Database db || db.IsDisposed) { return false; }
        if (variables is not VariableCollection vc) { return false; }

        foreach (var thisv in vc) {
            var ro = RowOfVariable(thisv) ?? db.Row.GenerateAndAdd(thisv.KeyName, null, "Neue Variable");

            if (ro != null) {
                ro.CellSet("typ", thisv.MyClassId, string.Empty);
                ro.CellSet("RO", thisv.ReadOnly, string.Empty);

                var tmpi = thisv.ReadableText;
                if (!Editabe && tmpi.Length > 3990) {
                    tmpi = tmpi.Substring(0, 3990) + "...";
                }

                ro.CellSet("Inhalt", tmpi, string.Empty);
                ro.CellSet("Kommentar", thisv.Comment, string.Empty);
            }
        }

        return true;
    }

    protected override void InitializeComponentDefaultValues() {
        Database db = new(Database.UniqueKeyValue());
        db.LogUndo = false;
        db.DropMessages = false;
        var na = db.Column.GenerateAndAdd("Name", "N", ColumnFormatHolder.SystemName, "Variablenname");
        _ = db.Column.GenerateAndAdd("Typ", "T", ColumnFormatHolder.Text, "Variablentyp");
        _ = db.Column.GenerateAndAdd("RO", "R", ColumnFormatHolder.Bit, "Readonly, Schreibgeschützt");
        var inh = db.Column.GenerateAndAdd("Inhalt", "I", ColumnFormatHolder.Text, "Inhalt");
        var kom = db.Column.GenerateAndAdd("Kommentar", "K", ColumnFormatHolder.Text, "Komentar");

        foreach (var thisColumn in db.Column) {
            if (!thisColumn.IsSystemColumn()) {
                thisColumn.MultiLine = true;
                thisColumn.TextBearbeitungErlaubt = false;
                thisColumn.DropdownBearbeitungErlaubt = false;
            }
        }

        db.Column.GenerateAndAddSystem();

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

            db.PermissionGroupsNewRow = new(new List<string> { Constants.Everybody });
        }

        db.RepairAfterParse();
        var car = db.ColumnArrangements.CloneWithClones();

        //if (car != null) {
        if (Editabe) {
            car[1].ShowColumns("Name", "Inhalt", "Kommentar");
        } else {
            car[1].ShowColumns("Name", "Typ", "RO", "System", "Inhalt", "Kommentar");
        }

        db.ColumnArrangements = new(car);

        db.SortDefinition = new RowSortDefinition(db, na, true);

        tableVariablen.DatabaseSet(db, string.Empty);
        filterVariablen.Table = tableVariablen;

        db.Cell.CellValueChanged += TableVariablen_CellValueChanged;
    }

    private void TableVariablen_CellValueChanged(object sender, CellEventArgs e) {
        var c = tableVariablen.Database?.Column.First();
        if (e.Column == c) {
            if (e.Row.CellIsNullOrEmpty(c))
                tableVariablen.Database?.Row.Remove(e.Row, "Variable gelöscht");
        }
    }

    #endregion
}