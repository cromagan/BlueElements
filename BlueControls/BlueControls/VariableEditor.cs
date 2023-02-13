﻿// Authors:
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
using System.Linq;
using System.Windows.Forms;
using BlueBasics;
using BlueDatabase;
using BlueScript.Variables;
using System;

using System.Collections.Generic;

using System.ComponentModel;
using System.Drawing;

using System.Windows.Forms;
using BlueBasics;

using BlueBasics.Enums;
using BlueBasics.Interfaces;
using BlueControls.Designer_Support;
using BlueControls.Enums;
using BlueControls.EventArgs;
using BlueControls.Extended_Text;
using BlueControls.Forms;
using BlueControls.Interfaces;
using BlueControls.ItemCollection.ItemCollectionList;
using BlueDatabase.Enums;
using BlueDatabase.EventArgs;
using BlueDatabase.Interfaces;
using static BlueBasics.Converter;

namespace BlueControls;

public partial class VariableEditor : UserControl {

    #region Fields

    private bool _inited;

    #endregion

    #region Constructors

    public VariableEditor() {
        InitializeComponent();
    }

    #endregion

    #region Properties

    public bool Editabe { get; set; }

    #endregion

    #region Methods

    public List<Variable> GetVariables() {
        if (!Editabe) {
            Develop.DebugPrint_NichtImplementiert();
            // Bei Editable TRUE sind es nur string variablen
        }
        var list = new List<Variable>();

        foreach (var thisr in filterVariablen.Table.Database.Row) {
            var v = new VariableString(thisr.CellGetString("Name"), thisr.CellGetString("Inhalt"), false, false,
                thisr.CellGetString("Kommentar"));
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

    public void WriteVariablesToTable(IList<VariableString>? variables) {
        if (variables == null) { return; }

        var l = new List<Variable>();
        foreach (var thisv in variables) {
            l.Add(thisv);
        }
        WriteVariablesToTable(l);
    }

    public void WriteVariablesToTable(IList<Variable>? variables) {
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
                if (!Editabe && tmpi.Length > 500) {
                    tmpi = tmpi.Substring(0, 500) + "...";
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
        Database x = new(false, "Script_Variablen");
        var na = x.Column.GenerateAndAdd("Name", "N", ColumnFormatHolder.SystemName, "Variablenname");
        _ = x.Column.GenerateAndAdd("Typ", "T", ColumnFormatHolder.Text, "Variablentyp");
        _ = x.Column.GenerateAndAdd("RO", "R", ColumnFormatHolder.Bit, "Readonly, Schreibgeschützt");
        _ = x.Column.GenerateAndAdd("System", "S", ColumnFormatHolder.Bit, "Systemspalte\r\nIm Script nicht verfügbar");
        var inh = x.Column.GenerateAndAdd("Inhalt", "I", ColumnFormatHolder.Text, "Inhalt");
        var kom = x.Column.GenerateAndAdd("Kommentar", "K", ColumnFormatHolder.Text, "Komentar");

        foreach (var thisColumn in x.Column.Where(thisColumn => !thisColumn.IsSystemColumn())) {
            thisColumn.MultiLine = true;
            thisColumn.TextBearbeitungErlaubt = false;
            thisColumn.DropdownBearbeitungErlaubt = false;
        }

        if (Editabe) {
            var l = new List<ColumnItem?> { na, inh, kom };

            foreach (var thisColumn2 in l) {
                if (thisColumn2 != null) {
                    thisColumn2.TextBearbeitungErlaubt = true;
                    thisColumn2.MultiLine = false;
                    thisColumn2.PermissionGroupsChangeCell = new(new List<string> { "#Everybody" });
                }
            }

            if (na != null) { na.Caption = "Variablen-\rName"; }
            if (inh != null) { inh.Caption = "Inhalt"; }
            if (kom != null) { kom.Caption = "Kommentar"; }

            x.PermissionGroupsNewRow = new(new List<string> { "#Everybody" });
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