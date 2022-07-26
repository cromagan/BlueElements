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

#nullable enable

using BlueBasics;
using BlueBasics.Enums;
using BlueControls.Controls;
using BlueControls.EventArgs;
using BlueControls.Forms;
using BlueControls.Interfaces;
using BlueDatabase;
using BlueScript;
using FastColoredTextBoxNS;
using System;
using System.Collections.Generic;
using System.Linq;
using BlueControls.ItemCollection.ItemCollectionList;
using BlueScript.Variables;
using static BlueBasics.Extensions;

namespace BlueControls;

public partial class VariableEditor : System.Windows.Forms.UserControl {

    #region Constructors

    public VariableEditor() {
        InitializeComponent();
        GenerateVariableTable();
    }

    #endregion

    #region Methods

    public RowItem? RowOfVariable(string variable) {
        if (tableVariablen?.Database == null) { return null; }
        return tableVariablen.Database.Row[variable];
    }

    public RowItem? RowOfVariable(Variable variable) {
        if (tableVariablen?.Database == null) { return null; }
        return tableVariablen.Database.Row[variable.Name];
    }

    public void WriteVariablesToTable(List<Variable>? variables) {
        if (tableVariablen?.Database == null) { return; }
        if (variables == null) { return; }

        foreach (var thisv in variables) {
            var ro = RowOfVariable(thisv);
            if (ro == null) {
                ro = tableVariablen.Database.Row.Add(thisv.Name);
            }

            ro.CellSet("typ", thisv.ShortName);
            ro.CellSet("RO", thisv.Readonly);
            ro.CellSet("System", thisv.SystemVariable);

            var tmpi = thisv.ReadableText;
            if (tmpi.Length > 500) { tmpi = tmpi.Substring(0, 500) + "..."; }

            ro.CellSet("Inhalt", tmpi);
            ro.CellSet("Kommentar", thisv.Coment);
        }
    }

    internal void Clear() {
        tableVariablen.Database?.Row.Clear();
    }

    private void GenerateVariableTable() {
        Database x = new(true);
        x.Column.Add("Name", "N", VarType.Text, "Variablenname");
        x.Column.Add("Typ", "T", VarType.Text, "Variablentyp");
        x.Column.Add("RO", "R", VarType.Bit, "Readonly, Schreibgeschützt");
        x.Column.Add("System", "S", VarType.Bit, "Systemspalte\r\nIm Script nicht verfügbar");
        x.Column.Add("Inhalt", "I", VarType.Text, "Inhalt (gekürzte Ansicht)");
        x.Column.Add("Kommentar", "K", VarType.Text, "Komentar");

        foreach (var thisColumn in x.Column.Where(thisColumn => string.IsNullOrEmpty(thisColumn.Identifier))) {
            thisColumn.MultiLine = true;
            thisColumn.TextBearbeitungErlaubt = false;
            thisColumn.DropdownBearbeitungErlaubt = false;
        }

        x.RepairAfterParse(null, null);
        x.ColumnArrangements[1].ShowAllColumns();
        x.ColumnArrangements[1].HideSystemColumns();
        x.SortDefinition = new RowSortDefinition(x, "Name", true);
        tableVariablen.Database = x;
        tableVariablen.Arrangement = 1;
        filterVariablen.Table = tableVariablen;
    }

    #endregion
}