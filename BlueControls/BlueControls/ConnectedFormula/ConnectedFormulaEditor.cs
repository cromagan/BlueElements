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

using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using static BlueBasics.FileOperations;
using BlueControls.ItemCollection;
using BlueDatabase;
using static BlueBasics.Converter;
using BlueControls.Interfaces;
using BlueControls.ItemCollection.ItemCollectionList;
using BlueDatabase.Enums;
using System;

namespace BlueControls.Forms;

public partial class ConnectedFormulaEditor : PadEditor {

    #region Fields

    private ConnectedFormula.ConnectedFormula? _cf;

    #endregion

    #region Constructors

    public ConnectedFormulaEditor(string? filename, List<string>? notAllowedchilds) {
        InitializeComponent();

        FormulaSet(filename, notAllowedchilds);
    }

    public ConnectedFormulaEditor() : this(string.Empty, null) { }

    #endregion

    #region Methods

    public static List<BasicListItem> GetAllowedEditTypes(ColumnItem? column) {
        var l = new List<BasicListItem>();
        if (column == null) { return l; }
        var t = typeof(EditTypeFormula);

        foreach (int z1 in Enum.GetValues(t)) {
            if (column.UserEditDialogTypeInFormula((EditTypeFormula)z1)) {
                l.Add(new TextListItem(Enum.GetName(t, z1).Replace("_", " "), z1.ToString(), null, false, true, string.Empty));
            }
        }
        return l;
    }

    private void btnEingangsZeile_Click(object sender, System.EventArgs e) {
        var it = new RowInputPadItem(string.Empty);
        //it.Bei_Export_sichtbar = false;
        Pad.AddCentered(it);
    }

    private void btnFeldHinzu_Click(object sender, System.EventArgs e) {
        var l = Pad.LastClickedItem;

        var x = new EditFieldPadItem(string.Empty);

        if (l is ICalculateOneRowItemLevel ri) {
            x.GetRowFrom = ri;
        }
        if (l is CustomizableShowPadItem efi && efi.GetRowFrom != null) {
            x.GetRowFrom = efi.GetRowFrom;
        }

        Pad.AddCentered(x);

        if (x.GetRowFrom != null && x.GetRowFrom.Database != null) {
            x.Spalte_wählen = string.Empty; // Dummy setzen
        }
    }

    private void btnKonstante_Click(object sender, System.EventArgs e) {
        var x = new ConstantTextPaditem();
        //x.Bei_Export_Sichtbar = false;
        Pad.AddCentered(x);
    }

    private void btnLetzteDateien_ItemClicked(object sender, EventArgs.BasicListItemEventArgs e) {
        BlueBasics.MultiUserFile.MultiUserFile.ForceLoadSaveAll();

        if (e?.Item == null) { return; }
        FormulaSet(e.Item.Internal, null);
    }

    private void btnNeuDB_SaveAs_Click(object sender, System.EventArgs e) {
        BlueBasics.MultiUserFile.MultiUserFile.SaveAll(true);

        if (sender == btnSaveAs) {
            if (_cf == null) { return; }
        }

        if (sender == btnNeuDB) {
            if (_cf != null) { FormulaSet(null as ConnectedFormula.ConnectedFormula, null); }
        }

        SaveTab.ShowDialog();
        if (!PathExists(SaveTab.FileName.FilePath())) { return; }
        if (string.IsNullOrEmpty(SaveTab.FileName)) { return; }

        if (sender == btnNeuDB) {
            FormulaSet(new ConnectedFormula.ConnectedFormula(), null); // Ab jetzt in der Variable _Database zu finden
        }
        if (FileExists(SaveTab.FileName)) { DeleteFile(SaveTab.FileName, true); }

        _cf?.SaveAsAndChangeTo(SaveTab.FileName);

        FormulaSet(SaveTab.FileName, null);
    }

    private void btnOeffnen_Click(object sender, System.EventArgs e) {
        BlueBasics.MultiUserFile.MultiUserFile.ForceLoadSaveAll();
        LoadTab.ShowDialog();
    }

    private void btnPfeileAusblenden_CheckedChanged(object sender, System.EventArgs e) => btnVorschauModus.Checked = btnPfeileAusblenden.Checked;

    private void btnRegisterKarte_Click(object sender, System.EventArgs e) {
        var n = InputBox.Show("Formular-Name:");
        if (string.IsNullOrEmpty(n)) { return; }

        var it = new RowInputPadItem(string.Empty);
        //it.Bei_Export_sichtbar = false;
        Pad.AddCentered(it);
        it.Page = n;
    }

    private void btnTabControlAdd_Click(object sender, System.EventArgs e) {
        if (_cf == null) { return; }

        var x = new TabFormulaPaditem(string.Empty, _cf);
        x.Bei_Export_sichtbar = true;
        Pad.AddCentered(x);
    }

    private void btnVariable_Click(object sender, System.EventArgs e) {
        var l = Pad.LastClickedItem;

        var x = new VariableFieldPadItem(string.Empty);

        if (l is ICalculateOneRowItemLevel ri) {
            x.GetRowFrom = ri;
        }

        if (l is CustomizableShowPadItem efi && efi.GetRowFrom != null) {
            x.GetRowFrom = efi.GetRowFrom;
        }

        Pad.AddCentered(x);
    }

    private void btnVorschauModus_CheckedChanged(object sender, System.EventArgs e) => btnPfeileAusblenden.Checked = btnVorschauModus.Checked;

    private void btnVorschauÖffnen_Click(object sender, System.EventArgs e) {
        BlueBasics.MultiUserFile.MultiUserFile.SaveAll(false);
        EditBoxRow_NEW.Show("Achtung:\r\nVoll funktionsfähige Test-Ansicht", _cf, true);
    }

    private void btnZeileHinzu_Click(object sender, System.EventArgs e) {
        var it = new RowWithFilterPaditem(string.Empty);
        Pad.AddCentered(it);
        ChooseDatabaseAndId(it);
    }

    private void CheckButtons() {
    }

    private void ChooseDatabaseAndId(ICalculateOneRowItemLevel? it) {
        if (_cf == null || it == null) { return; }

        if (string.IsNullOrEmpty(LoadTabDatabase.InitialDirectory)) {
            LoadTabDatabase.InitialDirectory = _cf.Filename.FilePath();
        }

        LoadTabDatabase.ShowDialog();

        if (!FileExists(LoadTabDatabase.FileName)) { return; }
        LoadTabDatabase.InitialDirectory = LoadTabDatabase.FileName.FilePath();

        _cf.DatabaseFiles.AddIfNotExists(LoadTabDatabase.FileName);

        var db = Database.GetByFilename(LoadTabDatabase.FileName, false, false);
        if (db == null) { return; }

        it.Database = db;
        it.Id = _cf.NextId();
    }

    private void FormulaSet(string? filename, List<string>? notAllowedchilds) {
        FormulaSet(null as ConnectedFormula.ConnectedFormula, notAllowedchilds);

        if (filename == null || !FileExists(filename)) {
            CheckButtons();
            return;
        }

        btnLetzteFormulare.AddFileName(filename, string.Empty);
        LoadTab.FileName = filename;
        var tmpDatabase = ConnectedFormula.ConnectedFormula.GetByFilename(filename);
        if (tmpDatabase == null) { return; }
        FormulaSet(tmpDatabase, notAllowedchilds);
    }

    private void FormulaSet(ConnectedFormula.ConnectedFormula? formular, List<string>? notAllowedchilds) {
        _cf = formular;

        if (notAllowedchilds != null && _cf != null) {
            _cf.NotAllowedChilds.AddRange(notAllowedchilds);
        }

        if (_cf == null) {
            Pad.Item = null;
        } else {
            Pad.Item = _cf.PadData;
            //Pad.Item.SheetSizeInMm = new SizeF(PixelToMm(500, 300), PixelToMm(850, 300));
            //Pad.Item.GridShow = 0.5f;
            //Pad.Item.GridSnap = 0.5f;
        }
    }

    private void grpFileExplorer_Click(object sender, System.EventArgs e) {
        var l = Pad.LastClickedItem;

        var x = new FileExplorerPadItem(string.Empty);

        if (l is ICalculateOneRowItemLevel ri) {
            x.GetRowFrom = ri;
        }
        if (l is CustomizableShowPadItem efi && efi.GetRowFrom != null) {
            x.GetRowFrom = efi.GetRowFrom;
        }

        Pad.AddCentered(x);
    }

    private void LoadTab_FileOk(object sender, CancelEventArgs e) => FormulaSet(LoadTab.FileName, null);

    private void Pad_GotNewItemCollection(object sender, System.EventArgs e) {
        Pad.CurrentPage = "Head";
    }

    #endregion
}