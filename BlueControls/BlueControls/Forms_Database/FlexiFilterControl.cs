// Authors:
// Christian Peter
//
// Copyright (c) 2025 Christian Peter
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
using BlueBasics.Enums;
using BlueControls.BlueDatabaseDialogs;
using BlueControls.CellRenderer;
using BlueControls.Designer_Support;
using BlueControls.Enums;
using BlueControls.EventArgs;
using BlueControls.Interfaces;
using BlueControls.ItemCollectionList;
using BlueDatabase;
using BlueDatabase.Enums;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using static BlueControls.ItemCollectionList.AbstractListItemExtension;

namespace BlueControls.Controls;

[Designer(typeof(BasicDesigner))]
public partial class FlexiFilterControl : GenericControlReciverSender, IHasSettings {

    #region Fields

    private const int MaxRecentFilterEntries = 20;
    private readonly Renderer_Abstract _renderer;
    private FlexiFilterDefaultFilter _defaultTextInputFilter = FlexiFilterDefaultFilter.Textteil;
    private FlexiFilterDefaultOutput _emptyInputBehavior = FlexiFilterDefaultOutput.Alles_Anzeigen;
    private string _filterOrigin;
    private bool _isDeleteButtonVisible;
    private bool _isFilterFromInput;

    #endregion

    #region Constructors

    public FlexiFilterControl(ColumnItem? filterColumn, CaptionPosition defaultCaptionPosition) : base(false, false, false) {
        // Dieser Aufruf ist für den Designer erforderlich.
        InitializeComponent();

        Size = new Size(204, 24);
        FilterSingleColumn = filterColumn;
        f.ShowInfoWhenDisabled = true;
        _filterOrigin = string.Empty;
        _isFilterFromInput = false;
        _renderer = Table.RendererOf(filterColumn, Constants.Win11);
        DefaultCaptionPosition = defaultCaptionPosition;
    }

    #endregion

    #region Properties

    /// <summary>
    /// Da die CaptionPosition von dem Steuerelement bei Bedarf geändert wird,
    /// muss ein Defaultwert angegeben werden - wie es normalerweise auszusehen hat.
    /// </summary>
    public CaptionPosition DefaultCaptionPosition { get; }

    public FlexiFilterDefaultFilter Filterart_Bei_Texteingabe {
        get => _defaultTextInputFilter;
        set {
            if (IsDisposed) { return; }
            if (_defaultTextInputFilter == value) { return; }
            _defaultTextInputFilter = value;
            F_ValueChanged(null, System.EventArgs.Empty);
        }
    }

    [Browsable(false)]
    [EditorBrowsable(EditorBrowsableState.Never)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public ColumnItem? FilterSingleColumn { get; }

    /// <summary>
    /// Bei True werden die zuletzt eingegebenen Werte auf Festplatte gespeichert und geladen.
    /// </summary>
    public bool SavesSettings { get; internal set; } = false;

    public List<string> Settings { get; } = [];
    public bool SettingsLoaded { get; set; }
    public string SettingsManualFilename { get; set; } = string.Empty;

    public FlexiFilterDefaultOutput Standard_bei_keiner_Eingabe {
        get => _emptyInputBehavior;
        set {
            if (IsDisposed) { return; }
            if (_emptyInputBehavior == value) { return; }
            _emptyInputBehavior = value;
            F_ValueChanged(null, System.EventArgs.Empty);
        }
    }

    public bool UsesSettings => true;

    #endregion

    #region Methods

    public override void Invalidate_FilterInput() {
        base.Invalidate_FilterInput();
        HandleChangesNow();
    }

    protected override void Dispose(bool disposing) {
        base.Dispose(disposing);

        if (disposing) {
            f.Dispose();
            f = null;
        }
    }

    protected override void FilterOutput_PropertyChanged(object sender, System.EventArgs e) {
        if (SavesSettings) {
            this.LoadSettingsFromDisk(false);

            if (FilterOutput is { Rows.Count: 1 } fio && FilterSingleColumn is { IsDisposed: false } c) {
                if (FilterCollection.InitValue(c, true, fio.ToArray()) is { } vv) {
                    this.SettingsAdd($"{FilterHash()}|{vv}");
                }
            }
        }

        base.FilterOutput_PropertyChanged(this, System.EventArgs.Empty);
    }

    protected override void HandleChangesNow() {
        base.HandleChangesNow();
        if (IsDisposed) { return; }
        if (FilterInputChangedHandled) { return; }

        DoInputFilter(null, false);

        var filterSingle = FilterInput?[FilterSingleColumn];

        if (filterSingle != null) {
            _isDeleteButtonVisible = filterSingle.FilterType != FilterType.Instr_GroßKleinEgal ||
                                    filterSingle.SearchValue.Count > 1 ||
                                    (f.GetControl<ComboBox>() is { IsDisposed: false } cb && cb.WasThisValueClicked());
            _isFilterFromInput = true;
            _filterOrigin = filterSingle.Origin;
        } else {
            _isFilterFromInput = false;
            _isDeleteButtonVisible = false;
            _filterOrigin = string.Empty;
        }

        UpdateFilterData(filterSingle, _isDeleteButtonVisible);
    }

    protected override void OnCreateControl() {
        base.OnCreateControl();
        F_ValueChanged(this, System.EventArgs.Empty);
    }

    protected override void OnQuickInfoChanged() {
        base.OnQuickInfoChanged();
        f.QuickInfo = QuickInfo;
    }

    private void AutoFilter_FilterCommand(object sender, FilterCommandEventArgs e) {
        if (e.Command != "Filter") {
            UpdateFilterData(null, false);
        } else {
            UpdateFilterData(e.Filter, false);
        }
    }

    private void Cbx_DropDownShowing(object sender, System.EventArgs e) {
        var cbx = (ComboBox)sender;
        cbx.ItemClear();
        var listFilterString = AutoFilter.Autofilter_ItemList(FilterSingleColumn, FilterInput, null);
        if (listFilterString.Count == 0) {
            cbx.ItemAdd(ItemOf("Keine weiteren Einträge vorhanden", "|~", ImageCode.Kreuz, false));
        } else if (listFilterString.Count < 400) {
            if (FilterSingleColumn != null) { cbx.ItemAddRange(ItemsOf(listFilterString, FilterSingleColumn, _renderer)); }
            //cbx.Item.Sort(); // Wichtig, dieser Sort kümmert sich, dass das Format (z. B.  Zahlen) berücksichtigt wird
        } else {
            if (SavesSettings) {
                this.LoadSettingsFromDisk(false);
                var nr = -1;
                var f2 = FilterHash();

                for (var z = Settings.Count - 1; z >= 0; z--) {
                    var x = Settings[z].SplitAndCutBy("|");
                    if (x.GetUpperBound(0) > 0 && !string.IsNullOrEmpty(x[1]) && f2 == x[0]) {
                        nr++;
                        if (nr < MaxRecentFilterEntries) {
                            var show = (nr + 1).ToStringInt3() + ": " + x[1];
                            TextListItem it = new(show, x[1], null, false, true, nr.ToStringInt3());
                            cbx.ItemAdd(it);
                        }
                    }
                }
            }

            cbx.ItemAdd(ItemOf("Zu viele Einträge", "|~", ImageCode.Kreuz, false));
        }
    }

    private void DoButtonStyle(Button btn) {
        var filterSingle = FilterInput?[FilterSingleColumn];

        //if (filterSingle == null) { return; }
        btn.Translate = false;

        if (f.CaptionPosition == CaptionPosition.ohne && filterSingle != null) {
            btn.ImageCode = "Trichter|16||1";
            btn.Text = filterSingle.ReadableText();
        } else {
            if (filterSingle is { SearchValue.Count: > 0 } && !string.IsNullOrEmpty(filterSingle.SearchValue[0])) {
                btn.ImageCode = "Trichter|16";
                btn.Text = LanguageTool.DoTranslate("wählen ({0})", true, filterSingle.SearchValue.Count.ToString());
            } else {
                btn.ImageCode = "Trichter|16";
                btn.Text = LanguageTool.DoTranslate("Gewählt: " + f.Value);
                GenerateQickInfoText(null);
            }
        }
    }

    private void F_ButtonClick(object sender, System.EventArgs e) {
        //base.CommandButton_Click(); // Nope, keine Ereignisse und auch nicht auf + setzen
        _isDeleteButtonVisible = false;

        var filterSingle = FilterInput?[FilterSingleColumn];

        if (filterSingle == null) {
            Invalidate_FilterOutput();
            f.ValueSet(string.Empty, true);
            return;
        }

        if (f.CaptionPosition == CaptionPosition.ohne) {
            Invalidate_FilterOutput();
            return;
        }

        if (FilterSingleColumn is not { IsDisposed: false } c) { return; }

        AutoFilter autofilter = new(c, FilterInput, null, Width, _renderer);
        var p = PointToScreen(Point.Empty);
        autofilter.Position_LocateToPosition(p with { Y = p.Y + Height });
        autofilter.Show();
        autofilter.FilterCommand += AutoFilter_FilterCommand;
        Develop.Debugprint_BackgroundThread();
    }

    private void F_ControlAdded(object sender, ControlEventArgs e) {
        if (e.Control is ComboBox cbx) {
            List<AbstractListItem> item2 = [ItemOf("Keine weiteren Einträge vorhanden", "|~")];

            //var c = Filter.Column.Contents(null);
            //foreach (var thiss in c)
            //{
            //    Item2.GenerateAndAdd("|" + thiss, thiss));
            //}

            if (TextEntryAllowed()) {
                f.StyleComboBox(cbx, item2, ComboBoxStyle.DropDown, false);
            } else {
                f.StyleComboBox(cbx, item2, ComboBoxStyle.DropDownList, false);
            }

            cbx.DropDownShowing += Cbx_DropDownShowing;
        }

        if (e.Control is Button btn) {
            DoButtonStyle(btn);
        }
    }

    private void F_ControlRemoved(object sender, ControlEventArgs e) {
        if (e.Control is ComboBox cbx) {
            cbx.DropDownShowing -= Cbx_DropDownShowing;
        }
    }

    private void F_ValueChanged(object? sender, System.EventArgs e) {
        var filterSingle = FilterInput?[FilterSingleColumn];

        if (!_isDeleteButtonVisible) {
            if (filterSingle != null && f.Value != filterSingle.SearchValue.JoinWith("|")) { _isFilterFromInput = false; }
            if (!_isFilterFromInput) { filterSingle = null; }

            if (FilterSingleColumn != null && filterSingle == null) {
                if (string.IsNullOrEmpty(f.Value)) {
                    if (_emptyInputBehavior == FlexiFilterDefaultOutput.Nichts_Anzeigen) {
                        filterSingle = new FilterItem(FilterSingleColumn, FilterType.AlwaysFalse, string.Empty, _filterOrigin);
                    }
                } else {
                    if (f.GetControl<ComboBox>() is { IsDisposed: false } cmb && cmb.WasThisValueClicked()) {
                        _isDeleteButtonVisible = true;
                        filterSingle = new FilterItem(FilterSingleColumn, FilterType.Istgleich_ODER_GroßKleinEgal, f.Value, _filterOrigin);
                    } else {
                        filterSingle = _defaultTextInputFilter == FlexiFilterDefaultFilter.Textteil
                            ? new FilterItem(FilterSingleColumn, FilterType.Instr_GroßKleinEgal, f.Value, _filterOrigin)
                            : new FilterItem(FilterSingleColumn, FilterType.Istgleich_ODER_GroßKleinEgal, f.Value, _filterOrigin);
                    }
                }
            }
        }
        UpdateFilterData(filterSingle, _isDeleteButtonVisible);
    }

    private void GenerateQickInfoText(FilterItem? filterSingle) {
        if (FilterSingleColumn is not { IsDisposed: false }) {
            QuickInfo = string.Empty;
            return;
        }

        #region QuickInfo erstellen

        var qi = Table.QuickInfoText(FilterSingleColumn, string.Empty);

        if (filterSingle != null) {
            if (string.IsNullOrEmpty(qi)) {
                QuickInfo = "<b><u>Filter:</u></b><br>" + filterSingle.ReadableText().CreateHtmlCodes(false);
            } else {
                QuickInfo = "<b><u>Filter:</u></b><br>" + filterSingle.ReadableText().CreateHtmlCodes(false) +
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

    private bool TextEntryAllowed() {
        if (FilterSingleColumn == null) { return false; }
        return FilterSingleColumn.FilterOptions.HasFlag(FilterOptions.TextFilterEnabled);
    }

    private void UpdateFilterData(FilterItem? filterSingle, bool showDelFilterButton) {
        if (IsDisposed || f is null) { return; }

        if (FilterSingleColumn?.Database is { IsDisposed: false } db && filterSingle is { }) {
            DoInputFilter(null, false);
            using var fic = FilterInput?.Clone("UpdateFilterData") as FilterCollection ?? new FilterCollection(db, "UpadteFilterData");

            fic?.RemoveOtherAndAdd(filterSingle);
            FilterOutput.ChangeTo(fic);
        } else {
            Invalidate_FilterOutput();
        }
        var nvalue = string.Empty;
        if (filterSingle != null) {
            nvalue = filterSingle.SearchValue.JoinWithCr();
        }

        if (IsDisposed || f is null) { return; } // Kommt vor!

        f.ValueSet(nvalue, true);

        GenerateQickInfoText(filterSingle);

        #region Wenn keine Spalte vorhanden, Fehler machen

        if (FilterSingleColumn is not { IsDisposed: false }) {
            f.DisabledReason = "Bezug zum Filter verloren.";
            f.Caption = "?";
            f.EditType = EditTypeFormula.nur_als_Text_anzeigen;
            QuickInfo = string.Empty;
            f.ValueSet(string.Empty, true);
            return;
        }

        #endregion

        f.DisabledReason = !string.IsNullOrEmpty(_filterOrigin) ? "Dieser Filter wurde<br>automatisch gesetzt." : string.Empty;

        #region Wählen-Button - und keine weitere Berechnungen

        if (MustMenu()) {
            f.CaptionPosition = DefaultCaptionPosition;
            f.Caption = FilterSingleColumn.ReadableText() + ":";
            f.EditType = EditTypeFormula.Button;
            if (f.GetControl<Button>() is { IsDisposed: false } b) { DoButtonStyle(b); }
            return;
        }

        #endregion

        #region Löschen-Button - und keine weiteren Berechnungen

        if (showDelFilterButton) {
            f.CaptionPosition = CaptionPosition.ohne;
            f.EditType = EditTypeFormula.Button;
            if (f.GetControl<Button>() is { IsDisposed: false } b) { DoButtonStyle(b); }
            return;
        }

        #endregion

        if (filterSingle != null) {
            if (filterSingle is { FilterType: FilterType.Instr_GroßKleinEgal, SearchValue.Count: 1 }) {
                //texteingabe = true;
                //showWählen = false;
                nvalue = filterSingle.SearchValue[0];
            } else if (_defaultTextInputFilter == FlexiFilterDefaultFilter.Ist) {
                //texteingabe = true;
                //showWählen = false;
                nvalue = filterSingle.SearchValue[0];
            } else if (filterSingle.FilterType is FilterType.Istgleich or FilterType.Istgleich_ODER_GroßKleinEgal) {
                //texteingabe = false;
                //showWählen = false;
            }
        }

        #region Filter verbieten - und keine weiteren Berechnungen

        if (!FilterSingleColumn.AutoFilterSymbolPossible()) {
            f.EditType = EditTypeFormula.nur_als_Text_anzeigen;
            f.DisabledReason = "Kein Filter erlaubt";
            return;
        }

        #endregion

        #region Text-Eingabefeld - und keine weitere Berechnungen

        //if (texteingabe) {
        f.CaptionPosition = DefaultCaptionPosition;
        f.Caption = FilterSingleColumn.ReadableText() + ":";
        f.EditType = EditTypeFormula.Textfeld_mit_Auswahlknopf;
        f.ValueSet(nvalue, true);
        //return;
        //}

        #endregion
    }

    #endregion
}