// Licensed under AGPL-3.0; see License.md for disclaimer and details.

using BlueControls.Classes;
using BlueControls.Classes.ItemCollectionPad;
using BlueControls.Controls.ConnectedFormula;
using BlueControls.EventArgs;
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

    public void SetRow(RowItem? row) => CFormula.SetToRow(row);

    protected void FormulaSet(string? filename) {
        if (filename is null || !FileExists(filename)) {
            FormulaSet(null as ItemCollectionPadItem);
            return;
        }

        btnLastFormulas.AddFileName(filename, string.Empty);
        LoadTab.FileName = filename;
        var tmpFormula = ConnectedFormula.Get(filename);
        if (tmpFormula is null) { return; }
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

        if (((IMultiUserCapable)cf).AcquireWriteAccess() is { Length: > 0 }) { return; }

        Opacity = 0f;
        using var x = new ConnectedFormulaEditor(cf.Filename, null);

        x.ShowDialog();
        ((IMultiUserCapable)cf).RevokeWriteAccess();
        CFormula.InvalidateView();
        Opacity = 1f;
    }

    private void btnLetzteDateien_ItemClicked(object sender, AbstractListItemEventArgs e) {
        FormManager.SaveAllFiles();

        FormulaSet(e.Item.KeyName);
    }

    private void btnMonitoring_Click(object sender, System.EventArgs e) => GlobalMonitor.Start();

    private void btnOeffnen_Click(object sender, System.EventArgs e) {
        FormManager.SaveAllFiles();
        LoadTab.ShowDialog();
    }

    private void btnSave_Click(object sender, System.EventArgs e) {
        btnSaveLoad.Enabled = false;

        FormManager.SaveAllFiles();

        btnSaveLoad.Enabled = true;
    }

    private void btnTopMost_CheckedChanged(object sender, System.EventArgs e) => TopMost = btnTopMost.Checked;

    private void CheckButtons() => btnFormular.Enabled = CFormula.Page is not null;

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
        //    if (oldf is not null) {
        //        RemoveRow();
        //        oldf.Loaded -= _cf_Loaded;
        //        oldf.Changed -= _page_PropertyChanged;
        //    }

        //    InvalidateView();
        //    ConnectedFormula = cf;

        //    if (cf is not null) {
        //        cf.Loaded += _cf_Loaded;
        //        cf.Changed += _page_PropertyChanged;
        //    }
        //}

        //if (Table != table) {
        //    if (Table is not null) {
                //        RemoveRow();
                //        Table.DisposingEvent -= _table_Disposing;
                //        Table.Row.RowRemoving -= Row_RowRemoving;
        //    }
        //    InvalidateView();
        //    Table = table;

        //    if (Table is not null) {
                //        Table.DisposingEvent += _table_Disposing;
                //        Table.Row.RowRemoving += Row_RowRemoving;
        //    }
        //}

        //if (rowKey != -1 && Table is not null && cf is not null) {
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