// Licensed under AGPL-3.0; see License.md for disclaimer and details.

using BlueControls.Controls.ConnectedFormula;
using BlueControls.EventArgs;
using static BlueControls.Classes.ItemCollectionList.AbstractListItemExtension;

namespace BlueControls.Controls;

public partial class FlexiControlForRowSelector : GenericControlReciverSender, IHasSettings {

    #region Fields

    private readonly string _showformat;

    #endregion

    #region Constructors

    public FlexiControlForRowSelector(Table? table, string caption, string showFormat) : base(false, false, false) {
        InitializeComponent();
        f.CaptionPosition = CaptionPosition.Über_dem_Feld;
        f.EditType = EditTypeFormula.Textfeld_mit_Auswahlknopf;

        f.Caption = string.IsNullOrEmpty(caption) ? "Wählen:" : caption;
        _showformat = showFormat;

        if (string.IsNullOrEmpty(_showformat) && table is { Column.Count: > 0 } && table.Column.First is { IsDisposed: false } fc) {
            _showformat = "~" + fc.KeyName + "~";
        }
    }

    #endregion

    #region Properties

    public CaptionPosition CaptionPosition { get => f.CaptionPosition; internal set => f.CaptionPosition = value; }
    public EditTypeFormula EditType { get => f.EditType; internal set => f.EditType = value; }
    public List<string> Settings { get; } = [];
    public bool SettingsLoaded { get; set; }
    public string SettingsManualFilename { get; set; } = string.Empty;
    public bool UsesSettings => true;

    #endregion

    #region Methods

    protected override void Dispose(bool disposing) {
        if (disposing) {
            f.ItemRemoved -= Cb_ItemRemoved;
            Tag = null;
        }

        base.Dispose(disposing);
    }

    protected override void HandleChangesNow() {
        base.HandleChangesNow();
        if (IsDisposed) { return; }
        if (RowsInputChangedHandled && FilterInputChangedHandled) { return; }

        f.RemoveAllowed = true;

        DoInputFilter(FilterOutput.Table, true);
        RowsInputChangedHandled = true;

        f.ListItems ??= [];
        var ex = f.ListItems.ToList();

        #region Zeilen erzeugen

        if (FilterInput?.Rows is not { } rows) { return; }

        foreach (var thisR in rows) {
            var existing = f.ListItems.GetByKey(thisR.KeyName);
            if (existing == null) {
                var tmpQuickInfo = thisR.ReplaceVariables(_showformat, true, null);
                f.ListItems.Add(ItemOf(tmpQuickInfo, thisR.KeyName));
            } else {
                ex.Remove(existing);
            }
        }

        #endregion

        #region Veraltete Zeilen entfernen

        foreach (var thisit in ex) {
            f.ListItems.Remove(thisit);
        }

        #endregion

        #region Nur eine Zeile? auswählen!

        // nicht vorher auf null setzen, um Blinki zu vermeiden
        if (f.ListItems.Count == 1) {
            f.Value = f.ListItems[0]?.KeyName ?? string.Empty;
        } else {
            var fh = this.GetSettings(FilterHash());

            if (!string.IsNullOrEmpty(fh) && f.ListItems.GetByKey(fh) is { } ali) {
                f.Value = ali.KeyName;
            }
        }

        f.DisabledReason = f.ListItems.Count < 2 ? "Keine Auswahl möglich." : string.Empty;

        #endregion

        #region  Prüfen ob die aktuelle Auswahl passt

        // am Ende auf null setzen, um Blinki zu vermeiden

        if (f.ListItems.GetByKey(f.Value) == null) {
            f.Value = string.Empty;
        }

        #endregion
    }

    private void Cb_ItemRemoved(object? sender, AbstractListItemEventArgs e) {
        var fh = FilterHash();
        if (f.Value == e.Item.KeyName) {
            f.Value = string.Empty;
        }
        this.SettingsRemove(fh);
        //Invalidate_FilterOutput();
    }

    private void F_NavigateToNext(object? sender, NavigationDirectionEventArgs e) => NextControl(e.Direction);

    private void F_ValueChanged(object sender, System.EventArgs e) {
        var fh = FilterHash();
        var row = FilterInput?.Rows.GetByKey(f.Value);
        this.SetSetting(fh, row?.KeyName ?? string.Empty);

        if (row == null) {
            Invalidate_FilterOutput();
            return;
        }
        using var nfc = new FilterCollection(row, "FlexiControlRowSelector");

        FilterOutput.ChangeTo(nfc);
    }

    #endregion
}