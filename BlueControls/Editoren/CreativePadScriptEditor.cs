// Licensed under AGPL-3.0; see License.md for disclaimer and details.

using BlueBasics.Interfaces;
using BlueControls.Editoren;
using BlueControls.Forms;
using BlueControls.ItemCollectionPad;
using BlueControls.ItemCollectionPad.FunktionsItems_Formular;
using BlueScript.Structures;
using BlueTable;
using BlueTable.Interfaces;
using System.Windows.Forms;

namespace BlueControls.BlueTableDialogs;

public sealed partial class CreativePadScriptEditor : ScriptEditorGeneric, IHasTable {

    #region Fields

    private CreativePadItem? _item;

    #endregion

    #region Constructors

    public CreativePadScriptEditor() : base() {
        // Dieser Aufruf ist für den Windows Form-Designer erforderlich.
        InitializeComponent();
    }

    #endregion

    #region Properties

    public override object? Object {
        get => IsDisposed ? null : (object?)_item;
        set {
            if (value is not CreativePadItem) { value = null; }
            if (_item == value) { return; }

            WriteInfosBack();

            _item = null; // Um keine Werte zurück zu schreiben während des Anzeigens

            if (value is CreativePadItem cpi) {
                tbcScriptEigenschaften.Enabled = true;
                Script = cpi.Script;
                LastFailedReason = string.Empty;
                _item = cpi;
            } else {
                tbcScriptEigenschaften.Enabled = false;
                Script = string.Empty;
                LastFailedReason = string.Empty;
            }
        }
    }

    /// <summary>
    /// Nur zum setzen der Zeile zum Testen.
    /// </summary>
    public RowItem? Row {
        set {
            txbTestZeile.Text = value?.CellFirstString() ?? string.Empty;
            Table = value?.Table;
        }
    }

    public Table? Table {
        get;
        set {
            if (IsDisposed || (value?.IsDisposed ?? true)) { value = null; }
            if (value == field) { return; }

            if (field is not null) {
                field.DisposingEvent -= _table_Disposing;
            }
            field = value;

            if (field is not null) {
                field.DisposingEvent += _table_Disposing;
            }
        }
    }

   #endregion

    #region Methods

    public override ScriptEndedFeedback ExecuteScript(bool testmode) {
        if (IsDisposed || Table is not { IsDisposed: false }) {
            return new ScriptEndedFeedback("Keine Tabelle geladen.", false, false, "Allgemein");
        }

        if (_item is null) {
            return new ScriptEndedFeedback("Kein Skript gewählt.", false, false, "Allgemein");
        }

        WriteInfosBack();

        if (!_item.IsOk()) {
            return new ScriptEndedFeedback("Bitte zuerst den Fehler korrigieren: " + _item.ErrorReason(), false, false, "Allgemein");
        }

        if (Table.Row.Count == 0) {
            return new ScriptEndedFeedback("Zum Test wird zumindest eine Zeile benötigt.", false, false, "Allgemein");
        }
        if (string.IsNullOrEmpty(txbTestZeile.Text)) {
            txbTestZeile.Text = Table?.Row.First()?.CellFirstString() ?? string.Empty;
        }

        var r = Table?.Row[txbTestZeile.Text] ?? Table?.Row.GetByKey(txbTestZeile.Text);
        if (r is not { IsDisposed: false }) {
            return new ScriptEndedFeedback("Zeile nicht gefunden.", false, false, "Allgemein");
        }

        var p = new ItemCollectionPadItem {
            Endless = true
        };
        var f = p.ExecuteScript(_item.Script, "Testmodus", r, !testmode);

        cpad.Items = p;
        cpad.ZoomFit();
        return f;
    }

    public override void WriteInfosBack() => _item?.Script = Script;

    protected override void OnFormClosing(FormClosingEventArgs e) {
        WriteInfosBack();

        base.OnFormClosing(e);

        Object = null; // erst das Item!
    }

    private void _table_Disposing(object sender, System.EventArgs e) {
        Table = null;
        Close();
    }

    private void btnTabelleKopf_Click(object sender, System.EventArgs e) => InputBoxEditor.EditItem(Table, typeof(TableHeadEditor), false);

    #endregion
}