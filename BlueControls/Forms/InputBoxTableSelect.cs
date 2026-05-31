// Licensed under AGPL-3.0; see License.md for disclaimer and details.

using BlueControls.Renderer;
using BlueTable.EventArgs;

namespace BlueControls.Forms;

public sealed partial class InputBoxTableSelect : DialogWithOkAndCancel {

    #region Fields

    private ColumnItem? _choose;
    private RowItem? _selectedRow;

    #endregion

    #region Constructors

    private InputBoxTableSelect(string txt, Table? tb) : base(true, true) {
        InitializeComponent();
        Text = txt;

        if (tb is null) { return; }

        _choose = tb.Column.GenerateAndAdd("btn_waehlen", "Wählen", ColumnFormatHolder_TextOneLine.Instance);

        if (_choose is { IsDisposed: false }) {
            var btn = new Renderer_Button {
                Text_anzeigen = false,
                Bild_anzeigen = true,
                CheckStatus_anzeigen = false
            };
            _choose.FixedColumnWidth = 32;
            _choose.DefaultRenderer = Renderer_Button.ClassId;
            _choose.RendererSettings = btn.ParseableItems().FinishParseable();
        }

        foreach (var thisRow in tb.Row) {
            thisRow.CellSet(_choose, "Pfeil_Rechts_Scrollbar|16;+;Wählen", string.Empty);
        }

        tb.RepairAfterParse();
        tb.Column.DisableAllEditing();

        var tcvc = ColumnViewCollection.ParseAll(tb);
        foreach (var cv in tcvc) {
            cv.ShowColumns(_choose?.KeyName ?? string.Empty);
        }
        tb.ColumnArrangements = tcvc.AsReadOnly();

        _tableView.Table = tb;
        _tableView.ContextMenuDefault = false;
        _tableView.EditButton = false;
        Setup(txt, _tableView, 700);
    }

    #endregion

    #region Methods

    public static RowItem? Show(string txt, Table table) {
        if (table is null or { IsDisposed: true }) { return null; }

        var x = new InputBoxTableSelect(txt, table);
        x.ShowDialog();
        var result = x._selectedRow;
        x.Dispose();
        return result;
    }

    protected override void OnShown(System.EventArgs e) {
        base.OnShown(e);

        if (_tableView.Table == null) {
            MessageBox.Show("Ungültige Tabelle!");
            Cancel();
        }
    }

    protected override bool SetValue() {
        _selectedRow = Canceled ? null : _tableView.CursorPosRow?.Row;
        return true;
    }

    private void _tableView_CellClicked(object sender, CellEventArgs e) {
        if (e.Column == _choose) {
            Ok();
        }
    }

    #endregion
}