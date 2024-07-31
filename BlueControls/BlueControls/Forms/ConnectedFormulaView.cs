// Authors:
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
using BlueBasics.MultiUserFile;
using BlueControls.Controls;
using BlueControls.EventArgs;
using BlueControls.Interfaces;
using BlueControls.ItemCollectionPad.Abstract;
using BlueControls.ItemCollectionPad.FunktionsItems_Formular.Abstract;
using BlueDatabase;
using System.ComponentModel;
using System.Windows.Forms;
using static BlueBasics.Develop;
using static BlueBasics.IO;

namespace BlueControls.Forms;

public partial class ConnectedFormulaView : FormWithStatusBar {

    #region Fields

    private AbstractPadItem? _lastItem = null;

    #endregion

    #region Constructors

    public ConnectedFormulaView() => InitializeComponent();

    public ConnectedFormulaView(string filename, string mode) : this() {
        btnEingehendeDatenbank.Enabled = false;
        btnAusgehendeDatenbank.Enabled |= false;

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
        if (_lastItem is IItemSendFilter sif && sif.DatabaseOutput is Database db && !db.IsDisposed) {
            var c = new TableView(db, false, true);
            c.ShowDialog();
        }
    }

    private void btnEingehendeDatenbank_Click(object sender, System.EventArgs e) {
        if (_lastItem is IItemAcceptFilter iaf && iaf.DatabaseInput is Database db && !db.IsDisposed) {
            var c = new TableView(db, false, true);
            c.ShowDialog();
        }
    }

    private void btnFormular_Click(object sender, System.EventArgs e) {
        DebugPrint_InvokeRequired(InvokeRequired, true);
        if (CFormula.ConnectedFormula == null) { return; }

        var x = new ConnectedFormulaEditor(CFormula.ConnectedFormula.Filename, null);
        x.Show();
        CFormula.InvalidateView();
    }

    private void btnLetzteDateien_ItemClicked(object sender, AbstractListItemEventArgs e) {
        MultiUserFile.ForceLoadSaveAll();

        if (e.Item == null) { return; }
        FormulaSet(e.Item.KeyName);
    }

    private void btnOeffnen_Click(object sender, System.EventArgs e) {
        MultiUserFile.SaveAll(false);
        Database.ForceSaveAll();
        _ = LoadTab.ShowDialog();
    }

    private void CFormula_ChildGotFocus(object sender, ControlEventArgs e) => SetItem(e.Control);

    private void CheckButtons() => btnFormular.Enabled = CFormula.ConnectedFormula != null;

    private void FormulaSet(string? filename) {
        FormulaSet(null as ConnectedFormula.ConnectedFormula);

        if (filename == null || !FileExists(filename)) {
            //CheckButtons();
            return;
        }

        btnLastFormulas.AddFileName(filename, string.Empty);
        LoadTab.FileName = filename;
        var tmpFormula = ConnectedFormula.ConnectedFormula.GetByFilename(filename);
        if (tmpFormula == null) { return; }
        FormulaSet(tmpFormula);
    }

    private void FormulaSet(ConnectedFormula.ConnectedFormula? cf) {
        if (IsDisposed) { return; }

        if (cf != null && !cf.IsDisposed) {
            DropMessages = cf.IsAdministrator();
        }

        CFormula.InitFormula(cf, null);

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
        //        oldf.Changed -= _cf_PropertyChanged;
        //    }

        //    InvalidateView();
        //    ConnectedFormula = cf;

        //    if (cf != null) {
        //        cf.Loaded += _cf_Loaded;
        //        cf.Changed += _cf_PropertyChanged;
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
            _lastItem = grc.Item;
        } else if (control is Control c) {
            SetItem(c.Parent);
            return;
        } else {
            _lastItem = null;
        }

        btnEingehendeDatenbank.Enabled = Generic.IsAdministrator() && _lastItem is IItemAcceptFilter;
        btnAusgehendeDatenbank.Enabled = Generic.IsAdministrator() && _lastItem is IItemSendFilter;

        if (_lastItem is FakeControlPadItem fcpi) {
            capClicked.Text = "<imagecode=Information|16> " +    fcpi.MyClassId;
            capClicked.QuickInfo = fcpi.Description;
        } else {
            capClicked.Text = "-";
        }
    }

    #endregion
}