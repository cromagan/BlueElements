// Authors:
// Christian Peter
//
// Copyright © 2026 Christian Peter
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

using BlueBasics.Classes;
using BlueBasics.ClassesStatic;
using BlueBasics.Enums;
using BlueControls.Classes;
using BlueControls.Classes.ItemCollectionPad;
using BlueControls.EventArgs;
using BlueTable.Classes;
using System.ComponentModel;
using static BlueBasics.ClassesStatic.Develop;
using static BlueBasics.ClassesStatic.IO;

namespace BlueControls.Forms;

public partial class ConnectedFormulaForm : FormWithStatusBar {

    #region Constructors

    public ConnectedFormulaForm() => InitializeComponent();

    public ConnectedFormulaForm(string filename, string mode) : this() {
        CFormula.Mode = mode;

        FormulaSet(filename);
    }

    #endregion

    #region Methods

    [StandaloneInfo("Formular-Ansicht", ImageCode.Anwendung, "Allgemein", "Allgemeine Formularansicht", 801)]
    public static System.Windows.Forms.Form Start() => new ConnectedFormulaForm();

    protected void FormulaSet(string? filename) {
        if (filename == null || !FileExists(filename)) {
            FormulaSet(null as ItemCollectionPadItem);
            return;
        }

        btnLastFormulas.AddFileName(filename, string.Empty);
        LoadTab.FileName = filename;
        var tmpFormula = BlueControls.Controls.ConnectedFormula.ConnectedFormula.GetByFilename(filename);
        if (tmpFormula == null) { return; }
        FormulaSet(tmpFormula.GetPage("Head"));
    }

    protected override void OnLoad(System.EventArgs e) {
        base.OnLoad(e);
        CheckButtons();
    }

    private void btnFormular_Click(object sender, System.EventArgs e) {
        DebugPrint_InvokeRequired(InvokeRequired, true);
        if (CFormula.Page?.GetConnectedFormula() is not { IsDisposed: false } cf) { return; }
        if (!Generic.IsAdministrator()) { return; }

        if (!cf.LockEditing()) { return; }

        Opacity = 0f;
        using var x = new ConnectedFormulaEditor(cf.Filename, null);

        x.ShowDialog();
        cf.UnlockEditing();
        CFormula.InvalidateView();
        Opacity = 1f;
    }

    private void btnLetzteDateien_ItemClicked(object sender, AbstractListItemEventArgs e) {
        MultiUserFile.SaveAll(true);

        FormulaSet(e.Item.KeyName);
    }

    private void btnMonitoring_Click(object sender, System.EventArgs e) => GlobalMonitor.Start();

    private void btnOeffnen_Click(object sender, System.EventArgs e) {
        MultiUserFile.SaveAll(false);
        Table.SaveAll(false);
        LoadTab.ShowDialog();
    }

    private void btnTopMost_CheckedChanged(object sender, System.EventArgs e) => TopMost = btnTopMost.Checked;

    private void CheckButtons() => btnFormular.Enabled = CFormula.Page != null;

    private void FormulaSet(ItemCollectionPadItem? page) {
        if (IsDisposed) { return; }

        MessageBoxOnError = Generic.IsAdministrator();

        CFormula.Page = page;

        CheckButtons();

        //var oldf = ConnectedFormula; // Zwischenspeichern wegen möglichen NULL verweisen

        //if (oldf == cf &&
        //    Table == table &&
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

        //if (Table != table) {
        //    if (Table != null) {
        //        RemoveRow();
        //        Table.DisposingEvent -= _table_Disposing;
        //        Table.Row.RowRemoving -= Row_RowRemoving;
        //    }
        //    InvalidateView();
        //    Table = table;

        //    if (Table != null) {
        //        Table.DisposingEvent += _table_Disposing;
        //        Table.Row.RowRemoving += Row_RowRemoving;
        //    }
        //}

        //if (rowKey != -1 && Table != null && cf != null) {
        //    RowKey = rowKey;
        //    _tmpShowingRow = Table?.Row.GetByKey(RowKey);
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

    #endregion
}