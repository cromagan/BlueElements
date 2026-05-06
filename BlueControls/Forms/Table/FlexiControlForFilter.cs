// Licensed under AGPL-3.0; see License.md for disclaimer and details.

using BlueControls.BlueTableDialogs;
using BlueControls.Classes.ItemCollectionList;
using BlueControls.Classes.ItemCollectionList.TableItems;
using BlueControls.Controls.ConnectedFormula;
using BlueControls.Designer_Support;
using BlueControls.Enums;
using BlueControls.EventArgs;
using BlueControls.Renderer;
using BlueScript.Variables;
using System.Diagnostics;
using static BlueControls.Classes.ItemCollectionList.AbstractListItemExtension;

namespace BlueControls.Controls;

[Designer(typeof(BasicDesigner))]
public partial class FlexiControlForFilter : GenericControlReciverSender, IHasSettings, IHasFieldVariable {

    #region Fields

    public bool Einschnappen = true;
    public FlexiFilterDefaultFilter Filterart_Bei_Texteingabe = FlexiFilterDefaultFilter.Textteil;
    public FlexiFilterDefaultOutput Standard_bei_keiner_Eingabe = FlexiFilterDefaultOutput.Alles_Anzeigen;
    private const int MaxRecentFilterEntries = 20;
    private readonly Renderer_Abstract _renderer;

    #endregion

    #region Constructors

    public FlexiControlForFilter(ColumnItem? filterColumn, CaptionPosition defaultCaptionPosition, FlexiFilterDefaultOutput emptyInputBehavior, FlexiFilterDefaultFilter defaultTextInputFilter, bool einschnappen, bool saveSettings) : base(false, false, false) {
        InitializeComponent();

        Size = new Size(204, 24);
        FilterSingleColumn = filterColumn;
        f.ShowInfoWhenDisabled = true;
        _renderer = TableView.RendererOf(filterColumn, Constants.Win11);
        Standard_bei_keiner_Eingabe = emptyInputBehavior;
        Filterart_Bei_Texteingabe = defaultTextInputFilter;
        DefaultCaptionPosition = defaultCaptionPosition;
        Einschnappen = einschnappen;
        SavesSettings = saveSettings;
    }

    #endregion

    #region Properties

    /// <summary>
    /// Da die CaptionPosition von dem Steuerelement bei Bedarf geändert wird,
    /// muss ein Defaultwert angegeben werden - wie es normalerweise auszusehen hat.
    /// </summary>
    public CaptionPosition DefaultCaptionPosition { get; }

    public string FieldName {
        get {
            if (GeneratedFrom is not IHasFieldVariable efpi) { return string.Empty; }
            return efpi.FieldName;
        }
    }

    [Browsable(false)]
    [EditorBrowsable(EditorBrowsableState.Never)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public ColumnItem? FilterSingleColumn { get; }

    /// <summary>
    /// Bei True werden die zuletzt eingegebenen Werte auf Festplatte gespeichert und geladen.
    /// </summary>
    public bool SavesSettings { get; internal set; }

    public List<string> Settings { get; } = [];

    public bool SettingsLoaded { get; set; }

    public string SettingsManualFilename { get; set; } = string.Empty;

    public bool UsesSettings => true;

    public string Value {
        get => f.Value;
        set => f.Value = value;
    }

    #endregion

    #region Methods

    public Variable? GetFieldVariable() {
        if (FilterSingleColumn is { } c) {
            var fn = FieldName;

            if (!string.IsNullOrEmpty(fn)) {
                return RowItem.CellToVariable(fn, c.ScriptType, Value, false, "Feld im Formular");
            }
        }

        return null;
    }

    public override void Invalidate_FilterInput() {
        base.Invalidate_FilterInput();
        HandleChangesNow();
    }

    public void SetValueFromVariable(Variable v) => Value = v.ValueForCell;

    protected override void Dispose(bool disposing) {
        base.Dispose(disposing);

        if (disposing) {
            f?.Dispose();
            f = null;
        }
    }

    protected override void HandleChangesNow() {
        if (IsDisposed || f is null) { return; }

        base.HandleChangesNow();

        if (FilterInputChangedHandled) { return; }

        DoInputFilter(null, false);

        var filterSingle = FilterInput?[FilterSingleColumn];
        UpdateFilterData(filterSingle);
    }

    protected override void OnCreateControl() {
        base.OnCreateControl();
        F_ValueChanged(this, System.EventArgs.Empty);
    }

    protected override void OnQuickInfoChanged() {
        if (IsDisposed || f is null) { return; }
        base.OnQuickInfoChanged();
        f.QuickInfo = QuickInfo;
    }

    private void AutoFilter_FilterCommand(object? sender, FilterCommandEventArgs e) {
        if (e.Command != "Filter") {
            UpdateFilterData(null);
        } else {
            UpdateFilterData(e.Filter);
        }
    }

    private void Cbx_DropDownShowing(object? sender, System.EventArgs e) {
        if (SavesSettings) {
            this.LoadSettingsFromDisk(false);
            var nr = 0;
            var f2 = FilterHash();
            var recentItems = new List<AbstractListItem>();

            for (var z = Settings.Count - 1; z >= 0 && nr < MaxRecentFilterEntries; z--) {
                var x = Settings[z].SplitAndCutBy("|");
                if (x.GetUpperBound(0) > 0 && !string.IsNullOrEmpty(x[1]) && f2 == x[0]) {
                    var show = (nr + 1).ToString3() + ": " + x[1];
                    var it = new TextListItem(show, x[1], null, false, true, string.Empty, nr.ToString3());
                    recentItems.Add(it);
                    nr++;
                }
            }
            if (nr > 0) {
                f.ListItems = recentItems;
                f.RemoveAllowed = true;
                return;
            }
        }

        f.RemoveAllowed = false;

        if (FilterSingleColumn == null) {
            f.ListItems = [ItemOf("Keine Spalte angegeben.", "|~", ImageCode.Kreuz, false)];
            return;
        }

        var listFilterString = AutoFilter.Autofilter_ItemList(FilterSingleColumn, FilterInput, null, true);
        if (listFilterString.Count == 0) {
            f.ListItems = [ItemOf("Keine (weiteren) Einträge vorhanden", "|~", ImageCode.Kreuz, false)];
        } else if (listFilterString.Count < 200) {
            f.ListItems = ItemsOf(listFilterString, FilterSingleColumn, _renderer).ToList();
        } else {
            f.ListItems = [ItemOf("Zu viele Einträge", "|~", ImageCode.Kreuz, false)];
        }
    }

    private void Cbx_ItemRemoved(object? sender, AbstractListItemEventArgs e) => this.SettingsRemoveValue($"{FilterHash()}|{e.Item.KeyName}");

    private void F_ExecuteComand(object? sender, System.EventArgs e) {
        var filterSingle = FilterInput?[FilterSingleColumn];

        if (filterSingle == null) {
            Invalidate_FilterOutput();
            f.Value = string.Empty;
            UpdateFilterData(null);
            return;
        }

        if (f.CaptionPosition == CaptionPosition.ohne) {
            Invalidate_FilterOutput();
            return;
        }

        if (FilterSingleColumn is not { IsDisposed: false } c) { return; }

        var autofilter = new AutoFilter(c, FilterInput, null, Width, _renderer);
        var p = PointToScreen(Point.Empty);
        autofilter.Position_LocateToPosition(p with { Y = p.Y + Height });
        autofilter.Show();
        autofilter.FilterCommand += AutoFilter_FilterCommand;
        Develop.Debugprint_BackgroundThread();
    }

    private void F_NavigateToNext(object? sender, BlueControls.EventArgs.NavigationDirectionEventArgs e) => NextControl(e.Direction);

    private void F_ValueChanged(object sender, System.EventArgs e) {
        if (IsDisposed || f is null) { return; }

        if (FilterSingleColumn?.Table is not { IsDisposed: false }) {
            UpdateFilterData(null);
            return;
        }

        var filterSingleo = FilterOutput?[FilterSingleColumn];

        var currentValue = filterSingleo != null ? string.Join('\r', filterSingleo.SearchValue) : string.Empty;

        if (currentValue == f.Value) {
            return;
        }

        var _filterOrigin = filterSingleo?.Origin ?? string.Empty;

        FilterItem? filterSingle;
        if (string.IsNullOrEmpty(f.Value)) {
            filterSingle = null;
        } else {
            if (f.WasValueClicked()) {
                filterSingle = new FilterItem(FilterSingleColumn, FilterType.Istgleich_ODER_GroßKleinEgal, f.Value, _filterOrigin);
            } else {
                filterSingle = Filterart_Bei_Texteingabe == FlexiFilterDefaultFilter.Textteil
                    ? new FilterItem(FilterSingleColumn, FilterType.Instr_GroßKleinEgal, f.Value, _filterOrigin)
                    : new FilterItem(FilterSingleColumn, FilterType.Istgleich_ODER_GroßKleinEgal, f.Value, _filterOrigin);
            }
        }

        if (filterSingle != null && filterSingleo != null && filterSingle.Equals(filterSingleo)) { return; }

        var editTypeBefore = f.EditType;
        UpdateFilterData(filterSingle);
        if (Einschnappen && editTypeBefore != EditTypeFormula.Button && f.EditType == EditTypeFormula.Button) {
            NextControl(NavigationDirection.Next);
        }
    }

    private void GenerateQickInfoText(FilterItem? filterSingle) {
        if (FilterSingleColumn is not { IsDisposed: false }) {
            QuickInfo = string.Empty;
            return;
        }

        #region QuickInfo erstellen

        var qi = RowListItem.QuickInfoText(FilterSingleColumn, string.Empty);

        if (filterSingle != null) {
            if (string.IsNullOrEmpty(qi)) {
                QuickInfo = "<b><u>Filter:</u></b><br>" + filterSingle.ReadableText().CreateHtmlCodes();
            } else {
                QuickInfo = "<b><u>Filter:</u></b><br>" + filterSingle.ReadableText().CreateHtmlCodes() +
                            "<br><br><b>Info:</b><br>" + qi;
            }
        } else {
            if (!string.IsNullOrEmpty(qi)) {
                QuickInfo = "<b>Info:</b><br>" + qi;
            } else {
                QuickInfo = string.Empty;
            }
        }

        #endregion
    }

    private bool MustMenu() {
        if (FilterSingleColumn == null) { return false; }
        return FilterSingleColumn.FilterOptions.HasFlag(FilterOptions.OnlyAndAllowed) ||
                         FilterSingleColumn.FilterOptions.HasFlag(FilterOptions.OnlyOrAllowed);
    }

    private void SetupButton(CaptionPosition captionPos) {
        f.CaptionPosition = captionPos;
        f.EditType = EditTypeFormula.Button;
        f.CreateSubControls();
        var filterSingle = FilterInput?[FilterSingleColumn];

        f.Translate = false;

        if (f.CaptionPosition == CaptionPosition.ohne && filterSingle != null) {
            f.ImageCode = "Trichter|16||1";
            f.Caption = filterSingle.ReadableText();
        } else {
            if (filterSingle is { SearchValue.Count: > 0 } && !string.IsNullOrEmpty(filterSingle.SearchValue[0])) {
                f.ImageCode = "Trichter|16";
                f.Caption = LanguageTool.DoTranslate("wählen ({0})", true, filterSingle.SearchValue.Count.ToString1());
            } else {
                f.ImageCode = "Trichter|16";
                f.Caption = LanguageTool.DoTranslate("Gewählt: " + f.Value);
                GenerateQickInfoText(null);
            }
        }
    }

    private bool TextEntryAllowed() {
        if (FilterSingleColumn == null) { return false; }
        return FilterSingleColumn.FilterOptions.HasFlag(FilterOptions.TextFilterEnabled);
    }

    private void UpdateFilterData(FilterItem? filterSingle) {
        if (IsDisposed || f is null) { return; }

        #region Endlosschleifen abfangen

        var stackTrace = new StackTrace();
        if (stackTrace.FrameCount > 100) {
            f.DisabledReason = "Interner Fehler\r\nEndlosschleife abgefangen.";
            f.Caption = "Interner Fehler";
            f.EditType = EditTypeFormula.nur_als_Text_anzeigen;
            FilterOutput.ChangeTo(new FilterItem(FilterSingleColumn?.Table, string.Empty));
            Invalidate_FilterOutput();
            return;
        }

        #endregion

        #region Wenn keine Spalte vorhanden, Fehler machen

        if (FilterSingleColumn?.Table is not { IsDisposed: false } tb) {
            f.DisabledReason = "Bezug zum Filter verloren.";
            f.Caption = "?";
            f.EditType = EditTypeFormula.nur_als_Text_anzeigen;
            QuickInfo = string.Empty;
            f.Value = string.Empty;
            Invalidate_FilterOutput();
            return;
        }

        #endregion

        #region Wenn Ausgangsfilter Disposed ist, Fehler machen

        if (FilterOutput.IsDisposed) {
            f.DisabledReason = "Ausgangsfilter verworfen.";
            f.Caption = "?";
            f.EditType = EditTypeFormula.nur_als_Text_anzeigen;
            return;
        }

        #endregion

        DoInputFilter(null, false);

        using var fic = FilterInput?.Clone("UpdateFilterData") as FilterCollection ??
                       new FilterCollection(tb, "UpdateFilterData");

        if (filterSingle == null || filterSingle.SearchValue.Count == 0) {
            if (Standard_bei_keiner_Eingabe == FlexiFilterDefaultOutput.Nichts_Anzeigen) {
                fic.RemoveOtherAndAdd(new FilterItem(FilterSingleColumn, FilterType.AlwaysFalse, string.Empty, string.Empty));
            } else {
                fic.Remove(FilterSingleColumn);
            }
        } else {
            fic.RemoveOtherAndAdd(filterSingle);
        }

        FilterOutput.ChangeTo(fic);

        if (FilterOutput.Table is Table tbo && SavesSettings && tbo.Row.Count > 30) {
            this.LoadSettingsFromDisk(false);

            if (Filterart_Bei_Texteingabe == FlexiFilterDefaultFilter.Istgleich && FilterOutput?.Rows.Count > 0) {
                var toAdd = $"{FilterHash()}|{f.Value}";
                this.SettingsAdd(toAdd);
            }
        }

        var nf = FilterOutput?[FilterSingleColumn];

        var nvalue = nf != null ? string.Join('\r', nf.SearchValue) : string.Empty;
        var _filterOrigin = nf?.Origin ?? string.Empty;

        if (IsDisposed || f is null) { return; }

        f.Value = nvalue;

        GenerateQickInfoText(filterSingle);
        if (IsDisposed || f is null) { return; }
        f.DisabledReason = !string.IsNullOrEmpty(_filterOrigin) ? $"<b>Dieser Filter wurde automatisch gesetzt:</b><br>{_filterOrigin}" : string.Empty;

        if (MustMenu()) {
            SetupButton(DefaultCaptionPosition);
            f.Value = nvalue;
            return;
        }

        var showDelFilterButton = false;

        if (filterSingle != null) {
            if (filterSingle.SearchValue.Count > 1) { showDelFilterButton = true; }
            if (filterSingle.FilterType == FilterType.Istgleich_MultiRowIgnorieren) { showDelFilterButton = true; }
            if (filterSingle.FilterType == FilterType.Ungleich_MultiRowIgnorieren) { showDelFilterButton = true; }

            if (Einschnappen && !showDelFilterButton && filterSingle.FilterType != FilterType.Instr_GroßKleinEgal && filterSingle.FilterType != FilterType.BeginntMit && filterSingle.SearchValue.Count == 1 && filterSingle.Column is { IsDisposed: false }) {
                using var fc = new FilterCollection(filterSingle, "Contents Ermittlung");

                if (filterSingle.Table?.Column.ChunkValueColumn is { IsDisposed: false } spc &&
                    spc != FilterSingleColumn &&
                    fic[spc] is { } fis) {
                    fc.Add(fis);
                }

                showDelFilterButton = fc.Rows.Count > 0;
            }

            if (showDelFilterButton) {
                if (IsDisposed || f is null) { return; }
                SetupButton(CaptionPosition.ohne);
                f.Value = nvalue;
                return;
            }
        }

        if (filterSingle != null) {
            if (filterSingle is { FilterType: FilterType.Instr_GroßKleinEgal, SearchValue.Count: 1 }) {
                nvalue = filterSingle.SearchValue[0];
            } else if (Filterart_Bei_Texteingabe == FlexiFilterDefaultFilter.Istgleich) {
                nvalue = filterSingle.SearchValue[0];
            }
        }

        if (!FilterSingleColumn.AutoFilterSymbolPossible()) {
            f.EditType = EditTypeFormula.nur_als_Text_anzeigen;
            f.DisabledReason = "Kein Filter erlaubt";
            return;
        }

        if (IsDisposed || f is null) { return; }

        f.CaptionPosition = DefaultCaptionPosition;
        f.Caption = FilterSingleColumn.ReadableText() + ":";
        f.EditType = EditTypeFormula.Textfeld_mit_Auswahlknopf;
        f.TextInputAllowed = TextEntryAllowed();
        f.RaiseChangeDelay = 1;
        f.ListItems = [ItemOf("Keine weiteren Einträge vorhanden", "|~")];

        if (FilterInput?.HasAlwaysFalse() ?? false) {
            f.DisabledReason = "Bitte vorherhige Felder richtig befüllen,\r\ndann gehts auch hier weiter.";
            return;
        }

        f.Value = nvalue;
    }

    #endregion
}