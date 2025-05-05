﻿// Authors:
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
using System.Windows.Forms;
using static BlueControls.ItemCollectionList.AbstractListItemExtension;

namespace BlueControls.Controls;

[Designer(typeof(BasicDesigner))]
public partial class FlexiFilterControl : GenericControlReciverSender, IHasSettings {

    #region Fields

    public FlexiFilterDefaultFilter Filterart_Bei_Texteingabe = FlexiFilterDefaultFilter.Textteil;
    public FlexiFilterDefaultOutput Standard_bei_keiner_Eingabe = FlexiFilterDefaultOutput.Alles_Anzeigen;
    private const int MaxRecentFilterEntries = 20;
    private readonly Renderer_Abstract _renderer;

    #endregion

    #region Constructors

    public FlexiFilterControl(ColumnItem? filterColumn, CaptionPosition defaultCaptionPosition, FlexiFilterDefaultOutput emptyInputBehavior, FlexiFilterDefaultFilter defaultTextInputFilter) : base(false, false, false) {
        // Dieser Aufruf ist für den Designer erforderlich.
        InitializeComponent();

        Size = new Size(204, 24);
        FilterSingleColumn = filterColumn;
        f.ShowInfoWhenDisabled = true;
        _renderer = Table.RendererOf(filterColumn, Constants.Win11);
        Standard_bei_keiner_Eingabe = emptyInputBehavior;
        Filterart_Bei_Texteingabe = defaultTextInputFilter;
        DefaultCaptionPosition = defaultCaptionPosition;
        //Invalidate_FilterInput();
    }

    #endregion

    #region Properties

    /// <summary>
    /// Da die CaptionPosition von dem Steuerelement bei Bedarf geändert wird,
    /// muss ein Defaultwert angegeben werden - wie es normalerweise auszusehen hat.
    /// </summary>
    public CaptionPosition DefaultCaptionPosition { get; }

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

    protected override void HandleChangesNow() {
        base.HandleChangesNow();
        if (IsDisposed) { return; }
        if (FilterInputChangedHandled) { return; }

        DoInputFilter(null, false);

        var filterSingle = FilterInput?[FilterSingleColumn];

        // Entferne die Berechnung von _isDeleteButtonVisible
        UpdateFilterData(filterSingle);
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
            UpdateFilterData(null);
        } else {
            UpdateFilterData(e.Filter);
        }
    }

    private void Cbx_DropDownShowing(object sender, System.EventArgs e) {
        var cbx = (ComboBox)sender;
        cbx.ItemClear();
        var listFilterString = AutoFilter.Autofilter_ItemList(FilterSingleColumn, FilterInput, null, true);
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
        if (FilterSingleColumn?.Database is not { IsDisposed: false }) {
            UpdateFilterData(null);
            return;
        }

        var filterSingleo = FilterInput?[FilterSingleColumn];

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

    private void UpdateFilterData(FilterItem? filterSingle) {
        if (IsDisposed || f is null) { return; }

        #region Wenn keine Spalte vorhanden, Fehler machen

        if (FilterSingleColumn?.Database is not { IsDisposed: false } db) {
            f.DisabledReason = "Bezug zum Filter verloren.";
            f.Caption = "?";
            f.EditType = EditTypeFormula.nur_als_Text_anzeigen;
            QuickInfo = string.Empty;
            f.ValueSet(string.Empty, true);
            Invalidate_FilterOutput();
            return;
        }

        #endregion

        DoInputFilter(null, false);

        #region Den FilterOutput erstellen

        using var fic = FilterInput?.Clone("UpdateFilterData") as FilterCollection ?? new FilterCollection(db, "UpdateFilterData");

        if (filterSingle == null) {
            if (Standard_bei_keiner_Eingabe == FlexiFilterDefaultOutput.Nichts_Anzeigen) {
                fic.RemoveOtherAndAdd(new FilterItem(FilterSingleColumn, FilterType.AlwaysFalse, string.Empty, string.Empty));
            } else {
                fic.Remove(FilterSingleColumn);
            }
        } else {
            fic.RemoveOtherAndAdd(filterSingle);
        }

        FilterOutput.ChangeTo(fic);

        #endregion

        #region Auf Festplatte speichern

        if (SavesSettings) {
            this.LoadSettingsFromDisk(false);
            if (FilterOutput is { Count: 1, Rows.Count: 1 } fio && fio[0] is { } fi) {
                var toAdd = $"{FilterHash()}|{fi.SearchValue.JoinWithCr()}";
                this.SettingsAdd(toAdd);
            }
        }

        #endregion

        var nf = FilterOutput[FilterSingleColumn];

        var nvalue = nf?.SearchValue.JoinWithCr() ?? string.Empty;
        var _filterOrigin = nf?.Origin ?? string.Empty;

        if (IsDisposed || f is null) { return; } // Kommt vor!

        f.ValueSet(nvalue, true);

        GenerateQickInfoText(filterSingle);

        f.DisabledReason = !string.IsNullOrEmpty(_filterOrigin) ? $"<b>Dieser Filter wurde automatisch gesetzt:</b><br>{_filterOrigin}" : string.Empty;

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

        // Komplett neue Berechnung für showDelFilterButton
        var showDelFilterButton = false;

        if (filterSingle != null) {
            // Fall 1: Eine ComboBox wurde angeklickt (nicht durch Texteingabe)
            if (f.GetControl<ComboBox>() is { IsDisposed: false } cmb && cmb.WasThisValueClicked()) { showDelFilterButton = true; }

            // Fall 2: Es existiert ein Filter, der mehr als einen Wert hat
            if (filterSingle.SearchValue.Count > 1) { showDelFilterButton = true; }

            // Fall 3: Leere
            if (filterSingle.FilterType == FilterType.Istgleich_MultiRowIgnorieren) { showDelFilterButton = true; }

            // Fall 4: Nicht-Leere
            if (filterSingle.FilterType == FilterType.Ungleich_MultiRowIgnorieren) { showDelFilterButton = true; }

            // Fall 5: Aufwendige Berechnung, wenn der Filter ein Ergebnis zurückliefert
            if (!showDelFilterButton && filterSingle.FilterType != FilterType.Instr_GroßKleinEgal && filterSingle.FilterType != FilterType.BeginntMit && filterSingle.SearchValue.Count == 1 && filterSingle.Column is { } c) {
                //if (!filterSingle.FilterType.HasFlag(FilterType.GroßKleinEgal)) { Develop.DebugPrint("Falscher Filtertyp"); }
                using var fc = new FilterCollection(filterSingle, "Contents Ermittlung");

                if (filterSingle.Database?.Column.SplitColumn is { } spc &&
                    spc != FilterSingleColumn &&
                    fic[spc] is { } fis) {
                    fc.Add(fis);
                }

                showDelFilterButton = fc.Rows.Count > 0;
            }

            if (showDelFilterButton) {
                f.CaptionPosition = CaptionPosition.ohne;
                f.EditType = EditTypeFormula.Button;
                if (f.GetControl<Button>() is { IsDisposed: false } b) { DoButtonStyle(b); }
                return;
            }
        }

        #endregion

        if (filterSingle != null) {
            if (filterSingle is { FilterType: FilterType.Instr_GroßKleinEgal, SearchValue.Count: 1 }) {
                nvalue = filterSingle.SearchValue[0];
            } else if (Filterart_Bei_Texteingabe == FlexiFilterDefaultFilter.Ist) {
                nvalue = filterSingle.SearchValue[0];
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

        if (IsDisposed || f is null) { return; } // Kommt vor!

        f.CaptionPosition = DefaultCaptionPosition;
        f.Caption = FilterSingleColumn.ReadableText() + ":";
        f.EditType = EditTypeFormula.Textfeld_mit_Auswahlknopf;
        f.ValueSet(nvalue, true);

        #endregion
    }

    #endregion
}