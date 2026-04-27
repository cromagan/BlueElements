// Licensed under AGPL-3.0; see License.md for disclaimer and details.

using BlueBasics.Enums;
using BlueControls.Forms;
using BlueTable.Classes;
using BlueTable.Interfaces;
using System.Collections.Generic;
using System.ComponentModel;
using static BlueControls.Classes.ItemCollectionList.AbstractListItemExtension;

namespace BlueControls.BlueTableDialogs;

public sealed partial class ImportBtb : FormWithStatusBar, IHasTable {

    #region Fields

    private List<string>? _files;

    #endregion

    #region Constructors

    public ImportBtb(Table? table) : base() {
        // Dieser Aufruf ist für den Designer erforderlich.
        InitializeComponent();
        // Fügen Sie Initialisierungen nach dem InitializeComponent()-Aufruf hinzu.
        //_originalImportText = importtext.Replace("\r\n", "\r").Trim("\r");
        //var ein = _originalImportText.SplitAndCutByCrToList();
        //Eintr.Text = ein.Count + " zum Importieren bereit.";
        Table = table;

        if (table != null) {
            //var lst =  List<AbstractListItem>();
            cbxColDateiname.ItemAddRange(ItemsOf(table.Column, false));
            //cbxColDateiname.Item = lst;
        }

        CheckButtons();
    }

    #endregion

    #region Properties

    public Table? Table {
        get;
        private set {
            if (IsDisposed || (value?.IsDisposed ?? true)) { value = null; }
            if (value == field) { return; }

            field?.DisposingEvent -= _table_Disposing;
            field = value;

            field?.DisposingEvent += _table_Disposing;
        }
    }

    #endregion

    #region Methods

    protected override void OnClosing(CancelEventArgs e) {
        Table = null;
        base.OnClosing(e);
    }

    private void _table_Disposing(object? sender, System.EventArgs e) {
        Table = null;
        Close();
    }

    private void btnChoseTable_Click(object sender, System.EventArgs e) => LoadTab.ShowDialog();

    private void Cancel_Click(object sender, System.EventArgs e) => Close();

    private void cbxColDateiname_TextChanged(object sender, System.EventArgs e) => CheckButtons();

    private void CheckButtons() {
        if (Table == null) {
            txtInfo.Text = "Keine Tabelle gewählt.";
            btnImport.Enabled = false;
            return;
        }

        if (_files is not { Count: not 0 }) {
            txtInfo.Text = "Keine Datei(en) zum Importieren gewählt.";
            btnImport.Enabled = false;
            return;
        }

        if (Table.Column[cbxColDateiname.Text] == null) {
            txtInfo.Text = "Keine Spalte für Dateinahmen gewählt.";
            btnImport.Enabled = false;
            return;
        }

        btnImport.Enabled = true;

        if (_files.Count == 1) {
            txtInfo.Text = _files[0];
            return;
        }

        txtInfo.Text = _files.Count + " Dateien.";
    }

    private void Fertig_Click(object sender, System.EventArgs e) {
        //var TR = string.Empty;
        //if (TabStopp.Checked) {
        //    TR = "\t";
        //} else if (Semikolon.Checked) {
        //    TR = ";";
        //} else if (Komma.Checked) {
        //    TR = ",";
        //} else if (Leerzeichen.Checked) {
        //    TR = " ";
        //} else if (Andere.Checked) {
        //    TR = aTXT.Text;
        //}
        //if (string.IsNullOrEmpty(TR)) {
        //    BlueControls.Forms.MessageBox.Show("Bitte Trennzeichen angeben.", ImageCode.Information, "OK");
        //    return;
        //}

        if (_files is not { Count: not 0 }) { return; }
        if (Table == null) { return; }

        var m = "Tabellen-Fehler";

        if (Table is TableFile { IsDisposed: false } tbf) {
            m = tbf.ImportBdb(_files, Table.Column[cbxColDateiname.Text], btnDateienlöschen.Checked);
        }

        if (!string.IsNullOrEmpty(m)) {
            MessageBox.Show(m, ImageCode.Information, "OK");
        }
        Close();
    }

    private void LoadTab_FileOk(object sender, CancelEventArgs e) {
        _files = [.. LoadTab.FileNames];
        CheckButtons();
    }

    #endregion
}