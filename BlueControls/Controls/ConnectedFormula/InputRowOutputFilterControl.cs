// Licensed under AGPL-3.0; see License.md for disclaimer and details.

using BlueControls.Classes;
using BlueControls.Classes.ItemCollectionList;
using BlueControls.EventArgs;
using BlueTable.EventArgs;
using System.Collections.ObjectModel;
using System.Windows.Forms;
using static BlueControls.Classes.ItemCollectionList.AbstractListItemExtension;

namespace BlueControls.Controls.ConnectedFormula;

internal class InputRowOutputFilterControl : GenericControlReciverSender, IContextMenu {

    #region Fields

    private readonly string _filterwert;
    private readonly ColumnItem? _outputcolumn;
    private readonly FilterTypeRowInputItem _type;

    #endregion

    #region Constructors

    public InputRowOutputFilterControl(string filterwert, ColumnItem? outputcolumn, FilterTypeRowInputItem type) : base(false, false, false) {
        _filterwert = filterwert;
        _outputcolumn = outputcolumn;
        _type = type;
    }

    #endregion

    #region Properties

    public bool ContextMenuDefault { get; set; } = true;
    public ReadOnlyCollection<AbstractListItem> CustomContextMenuItems { get; set; } = new List<AbstractListItem>().AsReadOnly();

    public string ErrorText { get; set; } = string.Empty;

    #endregion

    #region Methods

    public List<AbstractListItem>? GetContextMenuItems(object? hotItem) {
        var items = new List<AbstractListItem>();

        var filterText = hotItem as string ?? string.Empty;

        if (_outputcolumn is not null) {
            items.Add(ItemOf("Filterwert kopieren", "Filterwert kopieren", ContextMenu_Copy, !string.IsNullOrEmpty(filterText)));
        }
        return items;
    }

    public override void Invalidate_FilterInput() {
        base.Invalidate_FilterInput();
        HandleChangesNow();
    }

    protected override void DrawControl(Graphics gr, States state) {
        if (IsDisposed) { return; }
        base.DrawControl(gr, state);

        string txt;

        var qi = QuickImage.Get(ImageCode.Trichter);

        if (FilterOutput.Count == 0) {
            txt = string.IsNullOrEmpty(ErrorText) ? "Kein Filter" : ErrorText;

            qi = null;
        } else if (!FilterOutput.IsOk()) {
            txt = FilterOutput.ErrorReason();

            qi = QuickImage.Get(ImageCode.Warnung);
        } else {
            if (!string.IsNullOrEmpty(ErrorText) && FilterOutput.HasAlwaysFalse()) {
                txt = ErrorText;
                qi = null;
            } else {
                txt = FilterOutput.ReadableText();
            }
        }

        Skin.Draw_Back_Transparent(gr, DisplayRectangle, this);
        Skin.Draw_FormatedText(gr, txt, qi, Alignment.Top_Left, DisplayRectangle, Design.Caption, States.Standard, null, false, false);
    }

    protected override void HandleChangesNow() {
        base.HandleChangesNow();
        if (IsDisposed) { return; }
        if (RowsInputChangedHandled && FilterInputChangedHandled) { return; }

        DoInputFilter(null, false);
        RowsInputChangedHandled = true;

        //if (FilterInput is null) { return; }

        var lastInputRow = FilterInput?.RowSingleOrNull;

        if (_outputcolumn is null) {
            //if (_standard_bei_keiner_Eingabe == FlexiFilterDefaultOutput.Nichts_Anzeigen) {
            FilterOutput.ChangeTo(new FilterItem(FilterInput?.Table, "IO"));
            //} else {
            //    this.Invalidate_FilterOutput();
            //}
            return;
        }

        string? va;
        if (lastInputRow is not null) {
            va = lastInputRow.ReplaceVariables(_filterwert, true, lastInputRow.CheckRow()?.PrepareFormulaFeedback.Variables);
        } else {
            if (FilterInput is not null) {
                FilterOutput.ChangeTo(new FilterItem(_outputcolumn.Table, "IO"));
                return;
            }
            va = _filterwert;
        }

        if (va.Contains('~')) {
            FilterOutput.ChangeTo(new FilterItem(_outputcolumn.Table, "IO2"));
            return;
        }

        //if (string.IsNullOrEmpty(va) && _standard_bei_keiner_Eingabe == FlexiFilterDefaultOutput.Nichts_Anzeigen) {
        //    FilterOutput.ChangeTo(new FilterItem(_outputcolumn?.Table, "IO2"));
        //    return;
        //}

        FilterItem? f;

        switch (_type) {
            case FilterTypeRowInputItem.Ist_schreibungsneutral:
                f = new FilterItem(_outputcolumn, FilterType.Istgleich_GroßKleinEgal, va);
                break;

            case FilterTypeRowInputItem.Ist_genau:
                f = new FilterItem(_outputcolumn, FilterType.Istgleich, va);
                break;

            case FilterTypeRowInputItem.Ist_eines_der_Wörter_schreibungsneutral:
                var list = va.AllWords().SortedDistinctList();
                f = new FilterItem(_outputcolumn, FilterType.Istgleich_ODER_GroßKleinEgal, list);
                break;

            case FilterTypeRowInputItem.Ist_nicht:
                f = new FilterItem(_outputcolumn, FilterType.Ungleich_MultiRowIgnorieren, va);
                break;

            default:
                f = new FilterItem(_outputcolumn.Table, "IO3");
                break;
        }

        FilterOutput.ChangeTo(f);
    }

    protected override void OnCreateControl() {
        base.OnCreateControl();
        HandleChangesNow(); // Wenn keine Input-Rows da sind
    }

    protected override void OnMouseDown(MouseEventArgs e) {
        base.OnMouseDown(e);
        Text = FilterOutput.ReadableText();
    }

    protected override void OnMouseUp(MouseEventArgs e) {
        base.OnMouseUp(e);
        if (e.Button == MouseButtons.Right) {
            var filterText = FilterCollection.InitValue(_outputcolumn, true, true, [.. FilterOutput]);

            ((IContextMenu)this).ContextMenuShow(filterText);
        }
    }

    protected override void TableInput_CellValueChanged(object sender, CellEventArgs e) {
        if (FilterInput?.RowSingleOrNull == e.Row) {
            Invalidate_FilterInput();
        }
    }

    protected override void TableInput_RowChecked(object sender, RowPrepareFormulaEventArgs e) {
        if (FilterInput?.RowSingleOrNull == e.Row) {
            Invalidate_FilterInput();
        }
    }

    private void ContextMenu_Copy(object? sender, ContextMenuEventArgs e) {
        var filterText = e.HotItem as string ?? string.Empty;

        if (string.IsNullOrEmpty(filterText)) {
            QuickNote.Show(NoteSymbols.Critical, "Fehlgeschlagen");
        } else if (Generic.CopytoClipboard(filterText)) {
            QuickNote.Show(NoteSymbols.Ok, "Kopiert");
        } else {
            QuickNote.Show(NoteSymbols.Critical, "Fehlgeschlagen");
        }
    }

    #endregion
}