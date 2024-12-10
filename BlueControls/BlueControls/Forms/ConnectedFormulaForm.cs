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

using System.ComponentModel;
using System.Windows.Forms;
using BlueBasics;
using BlueBasics.MultiUserFile;
using BlueControls.Controls;
using BlueControls.EventArgs;
using BlueControls.Interfaces;
using BlueControls.ItemCollectionPad;
using BlueControls.ItemCollectionPad.Abstract;
using BlueControls.ItemCollectionPad.FunktionsItems_Formular.Abstract;
using BlueDatabase;
using static BlueBasics.Develop;
using static BlueBasics.IO;

namespace BlueControls.Forms;

public partial class ConnectedFormulaForm : FormWithStatusBar {

    #region Fields

    private AbstractPadItem? _lastItem;
    private IOpenScriptEditor? _lastObject;

    #endregion

    #region Constructors

    public ConnectedFormulaForm() => InitializeComponent();

    public ConnectedFormulaForm(string filename, string mode) : this() {
        btnEingehendeDatenbank.Enabled = false;
        btnAusgehendeDatenbank.Enabled = false;

        CFormula.Mode = mode;

        FormulaSet(filename);
    }

    #endregion

    #region Methods

    protected override void OnLoad(System.EventArgs e) {
        base.OnLoad(e);
        CheckButtons();
    }

    private void btnAusgehendeDatenbank_Click(object sender, System.EventArgs e) {
        if (_lastItem is ReciverSenderControlPadItem { DatabaseOutput: { IsDisposed: false } db }) {
            var c = new TableView(db, false, true);
            c.ShowDialog();
        }
    }

    private void btnEingehendeDatenbank_Click(object sender, System.EventArgs e) {
        if (_lastItem is ReciverControlPadItem { DatabaseInput: { IsDisposed: false } db }) {
            var c = new TableView(db, false, true);
            c.ShowDialog();
        }
    }

    private void btnElementBearbeiten_Click(object sender, System.EventArgs e) {
        DebugPrint_InvokeRequired(InvokeRequired, true);
        if (CFormula.Page?.GetConnectedFormula() is not { IsDisposed: false } cf) { return; }
        if (!Generic.IsAdministrator()) { return; }

        if (_lastItem is not { IsDisposed: false } api) { return; }

        Database.ForceSaveAll();
        MultiUserFile.SaveAll(false);

        if (!cf.LockEditing()) { return; }
        InputBoxEditor.Show(api, true);

        MultiUserFile.SaveAll(true);
        cf.UnlockEditing();
        Database.ForceSaveAll();
        CFormula.InvalidateView();
    }

    private void btnFormular_Click(object sender, System.EventArgs e) {
        DebugPrint_InvokeRequired(InvokeRequired, true);
        if (CFormula.Page?.GetConnectedFormula() is not { IsDisposed: false } cf) { return; }
        if (!Generic.IsAdministrator()) { return; }

        if (!cf.LockEditing()) { return; }

        this.Opacity = 0f;
        using var x = new ConnectedFormulaEditor(cf.Filename, null);

        x.ShowDialog();
        cf.UnlockEditing();
        CFormula.InvalidateView();
        this.Opacity = 1f;
    }

    private void btnLetzteDateien_ItemClicked(object sender, AbstractListItemEventArgs e) {
        MultiUserFile.SaveAll(true);

        FormulaSet(e.Item.KeyName);
    }

    private void btnOeffnen_Click(object sender, System.EventArgs e) {
        MultiUserFile.SaveAll(false);
        Database.ForceSaveAll();
        _ = LoadTab.ShowDialog();
    }

    private void btnScript_Click(object sender, System.EventArgs e) {
        DebugPrint_InvokeRequired(InvokeRequired, true);
        if (CFormula.Page == null) { return; }
        if (!Generic.IsAdministrator()) { return; }

        if (_lastObject is not { } api) { return; }
        api.OpenScriptEditor();
    }

    private void CFormula_ChildGotFocus(object sender, ControlEventArgs e) => SetItem(e.Control);

    private void CheckButtons() => btnFormular.Enabled = CFormula.Page != null;

    private void FormulaSet(string? filename) {
        if (filename == null || !FileExists(filename)) {
            FormulaSet(null as ItemCollectionPadItem);
            return;
        }

        btnLastFormulas.AddFileName(filename, string.Empty);
        LoadTab.FileName = filename;
        var tmpFormula = ConnectedFormula.ConnectedFormula.GetByFilename(filename);
        if (tmpFormula == null) { return; }
        FormulaSet(tmpFormula.GetPage("Head"));
    }

    private void FormulaSet(ItemCollectionPadItem? page) {
        if (IsDisposed) { return; }

        DropMessages = Generic.IsAdministrator();

        CFormula.Page = page;

        CheckButtons();

        //var oldf = ConnectedFormula; // Zwischenspeichern wegen möglichen NULL verweisen

        //if (oldf == cf &&
        //    Database == database &&
        //    RowKey == rowKey) { return; }

        //SuspendLayout();

        //if (oldf != cf) {
        //    if (oldf != null) {
        //        RemoveRow();
        //        oldf.Loaded -= _cf_Loaded;
        //        oldf.Changed -= _page_PropertyChanged;
        //    }

        //    InvalidateView();
        //    ConnectedFormula = cf;

        //    if (cf != null) {
        //        cf.Loaded += _cf_Loaded;
        //        cf.Changed += _page_PropertyChanged;
        //    }
        //}

        //if (Database != database) {
        //    if (Database != null) {
        //        RemoveRow();
        //        Database.DisposingEvent -= _database_Disposing;
        //        Database.Row.RowRemoving -= Row_RowRemoving;
        //    }
        //    InvalidateView();
        //    Database = database;

        //    if (Database != null) {
        //        Database.DisposingEvent += _database_Disposing;
        //        Database.Row.RowRemoving += Row_RowRemoving;
        //    }
        //}

        //if (rowKey != -1 && Database != null && cf != null) {
        //    RowKey = rowKey;
        //    _tmpShowingRow = Database?.Row.SearchByKey(RowKey);
        //    SetInputRow();
        //} else {
        //    RemoveRow();
        //}

        //ResumeLayout();
        //Invalidate();
    }

    private void LoadTab_FileOk(object sender, CancelEventArgs e) {
        if (!FileExists(LoadTab.FileName)) { return; }

        FormulaSet(LoadTab.FileName);
    }

    private void SetItem(object? control) {
        if (control is GenericControlReciver grc) {
            _lastItem = grc.GeneratedFrom;
            if (control is IOpenScriptEditor ose) {
                _lastObject = ose;
            } else {
                _lastObject = null;
            }
        } else if (control is Control c) {
            SetItem(c.Parent);
            return;
        } else {
            _lastObject = null;
            _lastItem = null;
        }

        btnEingehendeDatenbank.Enabled = Generic.IsAdministrator() && _lastItem is ReciverControlPadItem;
        btnAusgehendeDatenbank.Enabled = Generic.IsAdministrator() && _lastItem is ReciverSenderControlPadItem;

        if (_lastItem is ReciverControlPadItem fcpi) {
            capClicked.Text = "<imagecode=Information|16> " + fcpi.MyClassId;
            capClicked.QuickInfo = fcpi.Description;
            btnElementBearbeiten.Enabled = Generic.IsAdministrator();
        } else {
            capClicked.Text = "-";
            btnElementBearbeiten.Enabled = false;
        }

        btnScript.Enabled = _lastObject is { };
    }

    #endregion
}