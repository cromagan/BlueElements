// Licensed under AGPL-3.0; see License.md for disclaimer and details.

using BlueBasics.Enums;
using BlueControls.Forms;
using BlueTable.Classes;
using BlueTable.Interfaces;
using System.ComponentModel;
using System.Linq;
using static BlueBasics.Extensions;

namespace BlueControls.BlueTableDialogs;

public sealed partial class ImportCsv : FormWithStatusBar, IHasTable {

    #region Fields

    private readonly string _originalImportText;

    #endregion

    #region Constructors

    public ImportCsv(Table? table, string importtext) : base() {
        // Dieser Aufruf ist für den Designer erforderlich.
        InitializeComponent();
        // Fügen Sie Initialisierungen nach dem InitializeComponent()-Aufruf hinzu.
        _originalImportText = importtext.Replace("\r\n", "\r").Trim("\r");
        var ein = _originalImportText.SplitAndCutByCr().ToList();
        capEinträge.Text = $"{ein.Count-1} zum Importieren bereit.";
        Table = table;
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

    private void Cancel_Click(object sender, System.EventArgs e) => Close();

    private void Fertig_Click(object sender, System.EventArgs e) {
        var tr = string.Empty;
        if (optTabStopp.Checked) {
            tr = "\t";
        } else if (optSemikolon.Checked) {
            tr = ";";
        } else if (optKomma.Checked) {
            tr = ",";
        } else if (optLeerzeichen.Checked) {
            tr = " ";
        } else if (optAndere.Checked) {
            tr = txtAndere.Text;
        }
        if (string.IsNullOrEmpty(tr)) {
            MessageBox.Show("Bitte Trennzeichen angeben.", ImageCode.Information, "OK");
            return;
        }
        var m = "Tabellen-Fehler";

        if (Table is { IsDisposed: false }) {
            m = Table.ImportCsv(_originalImportText, optZeilenZuorden.Checked, tr, chkDoppelteTrennzeichen.Checked, chkTrennzeichenAmAnfang.Checked);
        }

        if (!string.IsNullOrEmpty(m)) {
            MessageBox.Show(m, ImageCode.Information, "OK");
        }
        Close();
    }

    #endregion
}