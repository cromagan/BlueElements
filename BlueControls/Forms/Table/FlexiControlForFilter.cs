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

using BlueBasics;
using BlueBasics.Enums;
using BlueControls.BlueTableDialogs;
using BlueControls.CellRenderer;
using BlueControls.Designer_Support;
using BlueControls.Enums;
using BlueControls.EventArgs;
using BlueControls.Interfaces;
using BlueControls.ItemCollectionList;
using BlueScript.Variables;
using BlueTable;
using BlueTable.Enums;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;
using static BlueControls.ItemCollectionList.AbstractListItemExtension;

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
        // Dieser Aufruf ist für den Designer erforderlich.
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
        //Invalidate_FilterInput();
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
        set => f.ValueSet(value, true);
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

    private void AutoFilter_FilterCommand(object sender, FilterCommandEventArgs e) {
        if (e.Command != "Filter") {
            UpdateFilterData(null);
        } else {
            UpdateFilterData(e.Filter);
        }
    }

    private void Cbx_DropDownShowing(object sender, System.EventArgs e) {
        var cbx = (ComboBox)sender;
        cbx.ItemClear();

        if (SavesSettings) {
            this.LoadSettingsFromDisk(false);
            var nr = -1;
            var f2 = FilterHash();

            for (var z = Settings.Count - 1; z >= 0; z--) {
                var x = Settings[z].SplitAndCutBy("|");
                if (x.GetUpperBound(0) > 0 && !string.IsNullOrEmpty(x[1]) && f2 == x[0]) {
                    nr++;
                    if (nr < MaxRecentFilterEntries) {
                        var show = (nr + 1).ToString3() + ": " + x[1];
                        var it = new TextListItem(show, x[1], null, false, true, string.Empty, nr.ToString3());
                        cbx.ItemAdd(it);
                    }
                }
            }
            if (nr >= 0) { return; }
        }

        if (FilterSingleColumn == null) {
            cbx.ItemAdd(ItemOf("Keine Spalte angegeben.", "|~", ImageCode.Kreuz, false));
            return;
        }

        var listFilterString = AutoFilter.Autofilter_ItemList(FilterSingleColumn, FilterInput, null, true);
        if (listFilterString.Count == 0) {
            cbx.ItemAdd(ItemOf("Keine (weiteren) Einträge vorhanden", "|~", ImageCode.Kreuz, false));
        } else if (listFilterString.Count < 200) {
            cbx.ItemAddRange(ItemsOf(listFilterString, FilterSingleColumn, _renderer, null));
        } else {
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
                btn.Text = LanguageTool.DoTranslate("wählen ({0})", true, filterSingle.SearchValue.Count.ToString1());
            } else {
                btn.ImageCode = "Trichter|16";
                btn.Text = LanguageTool.DoTranslate("Gewählt: " + f.Value);
                GenerateQickInfoText(null);
            }
        }
    }

    private void F_ButtonClick(object sender, System.EventArgs e) {
        var filterSingle = FilterInput?[FilterSingleColumn];

        if (filterSingle == null) {
            Invalidate_FilterOutput();
            f.ValueSet(string.Empty, true);
            UpdateFilterData(null);
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

            if (TextEntryAllowed()) {
                f.StyleComboBox(cbx, item2, ComboBoxStyle.DropDown, false, 1);
            } else {
                f.StyleComboBox(cbx, item2, ComboBoxStyle.DropDownList, false, 1);
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
        if (IsDisposed || f is null) { return; }

        if (FilterSingleColumn?.Table is not { IsDisposed: false }) {
            UpdateFilterData(null);
            return;
        }

        var filterSingleo = FilterOutput?[FilterSingleColumn];

        var currentValue = filterSingleo?.SearchValue.JoinWithCr() ?? string.Empty;

        // Wenn der aktuelle Wert bereits mit dem UI-Wert übereinstimmt, nichts tun
        if (currentValue == f.Value) {
            return;
        }

        var _filterOrigin = filterSingleo?.Origin ?? string.Empty;

        FilterItem? filterSingle;
        if (string.IsNullOrEmpty(f.Value)) {
            filterSingle = null;
        } else {
            if (f.GetControl<ComboBox>() is { IsDisposed: false } cmb && cmb.WasThisValueClicked()) {
                filterSingle = new FilterItem(FilterSingleColumn, FilterType.Istgleich_ODER_GroßKleinEgal, f.Value, _filterOrigin);
            } else {
                filterSingle = Filterart_Bei_Texteingabe == FlexiFilterDefaultFilter.Textteil
                    ? new FilterItem(FilterSingleColumn, FilterType.Instr_GroßKleinEgal, f.Value, _filterOrigin)
                    : new FilterItem(FilterSingleColumn, FilterType.Istgleich_ODER_GroßKleinEgal, f.Value, _filterOrigin);
            }
        }

        if (filterSingle != null && filterSingleo != null && filterSingle.Equals(filterSingleo)) { return; }

        UpdateFilterData(filterSingle);
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
            f.ValueSet(string.Empty, true);
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

        if (SavesSettings && FilterOutput.Rows.Count > 30) {
            this.LoadSettingsFromDisk(false);

            if (Filterart_Bei_Texteingabe == FlexiFilterDefaultFilter.Istgleich && FilterOutput?.Rows.Count > 0) {
                var toAdd = $"{FilterHash()}|{f.Value}";
                this.SettingsAdd(toAdd);
            }
        }

        var nf = FilterOutput?[FilterSingleColumn];

        var nvalue = nf?.SearchValue.JoinWithCr() ?? string.Empty;
        var _filterOrigin = nf?.Origin ?? string.Empty;

        if (IsDisposed || f is null) { return; } // Kommt vor!

        f.ValueSet(nvalue, true);

        GenerateQickInfoText(filterSingle);
        if (IsDisposed || f is null) { return; } // Kommt vor!
        f.DisabledReason = !string.IsNullOrEmpty(_filterOrigin) ? $"<b>Dieser Filter wurde automatisch gesetzt:</b><br>{_filterOrigin}" : string.Empty;

        if (MustMenu()) {
            f.CaptionPosition = DefaultCaptionPosition;
            f.Caption = FilterSingleColumn.ReadableText() + ":";
            f.EditType = EditTypeFormula.Button;
            if (f.GetControl<Button>() is { IsDisposed: false } b) { DoButtonStyle(b); }
            return;
        }

        // Komplett neue Berechnung für showDelFilterButton
        var showDelFilterButton = false;

        if (filterSingle != null) {
            // Fall 1: Eine ComboBox wurde angeklickt (nicht durch Texteingabe)
            if (showDelFilterButton && f.GetControl<ComboBox>() is { IsDisposed: false } cmb && cmb.WasThisValueClicked()) { showDelFilterButton = true; }

            // Fall 2: Es existiert ein Filter, der mehr als einen Wert hat
            if (filterSingle.SearchValue.Count > 1) { showDelFilterButton = true; }

            // Fall 3: Leere
            if (filterSingle.FilterType == FilterType.Istgleich_MultiRowIgnorieren) { showDelFilterButton = true; }

            // Fall 4: Nicht-Leere
            if (filterSingle.FilterType == FilterType.Ungleich_MultiRowIgnorieren) { showDelFilterButton = true; }

            // Fall 5: Aufwendige Berechnung, wenn der Filter ein Ergebnis zurückliefert
            if (Einschnappen && !showDelFilterButton && filterSingle.FilterType != FilterType.Instr_GroßKleinEgal && filterSingle.FilterType != FilterType.BeginntMit && filterSingle.SearchValue.Count == 1 && filterSingle.Column is { IsDisposed: false } c) {
                //if (!filterSingle.FilterType.HasFlag(FilterType.GroßKleinEgal)) { Develop.DebugPrint("Falscher Filtertyp"); }
                using var fc = new FilterCollection(filterSingle, "Contents Ermittlung");

                if (filterSingle.Table?.Column.ChunkValueColumn is { IsDisposed: false } spc &&
                    spc != FilterSingleColumn &&
                    fic[spc] is { } fis) {
                    fc.Add(fis);
                }

                showDelFilterButton = fc.Rows.Count > 0;
            }

            if (showDelFilterButton) {
                if (IsDisposed || f is null) { return; } // Kommt vor!
                f.CaptionPosition = CaptionPosition.ohne;
                f.EditType = EditTypeFormula.Button;
                if (f.GetControl<Button>() is { IsDisposed: false } b) { DoButtonStyle(b); }
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

        if (IsDisposed || f is null) { return; } // Kommt vor!

        f.CaptionPosition = DefaultCaptionPosition;
        f.Caption = FilterSingleColumn.ReadableText() + ":";
        f.EditType = EditTypeFormula.Textfeld_mit_Auswahlknopf;

        if (FilterSingleColumn.Value_for_Chunk != ChunkType.None) {
            f.DisabledReason = "Chunk-Spalte.";
            f.EditType = EditTypeFormula.nur_als_Text_anzeigen;
            return;
        }

        if (FilterInput?.HasAlwaysFalse() ?? false) {
            f.DisabledReason = "Bitte vorherhige Felder richtig befüllen,\r\ndann gehts auch hier weiter.";
            return;
        }

        f.ValueSet(nvalue, true);
    }

    #endregion
}