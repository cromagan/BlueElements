﻿// Authors:
// Christian Peter
//
// Copyright (c) 2024 Christian Peter
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
using BlueBasics.Interfaces;
using BlueControls.BlueDatabaseDialogs;
using BlueControls.Designer_Support;
using BlueControls.Enums;
using BlueControls.EventArgs;
using BlueControls.Interfaces;
using BlueDatabase;
using BlueDatabase.Enums;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using static BlueControls.ItemCollectionList.AbstractListItemExtension;
using BlueControls.ItemCollectionList;

#nullable enable

namespace BlueControls.Controls;

[Designer(typeof(BasicDesigner))]
public partial class FlexiControlForFilter : FlexiControl, IControlSendFilter, IControlAcceptFilter, IDisposableExtended {

    #region Fields

    private bool _doFilterDeleteButton;

    private FlexiFilterDefaultFilter _filterart_bei_texteingabe = FlexiFilterDefaultFilter.Textteil;

    private FilterCollection? _filterInput;
    private bool _fromInputFilter;

    private string _origin;

    private FlexiFilterDefaultOutput _standard_bei_keiner_Eingabe = FlexiFilterDefaultOutput.Alles_Anzeigen;

    #endregion

    #region Constructors

    public FlexiControlForFilter(ColumnItem? column, CaptionPosition defaultCaptionPosition) {
        // Dieser Aufruf ist für den Designer erforderlich.
        InitializeComponent();

        // Fügen Sie Initialisierungen nach dem InitializeComponent()-Aufruf hinzu.
        Size = new Size(204, 24);
        //AlwaysInstantChange = true;
        FilterSingleColumn = column;
        this.Invalidate_FilterInput();
        ShowInfoWhenDisabled = true;
        _origin = string.Empty;
        _fromInputFilter = false;
        ((IControlSendFilter)this).RegisterEvents();
        ((IControlAcceptFilter)this).RegisterEvents();
        DefaultCaptionPosition = defaultCaptionPosition;
        OnValueChanged();
    }

    #endregion

    #region Properties

    [Browsable(false)]
    [EditorBrowsable(EditorBrowsableState.Never)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public List<IControlAcceptFilter> Childs { get; } = [];

    /// <summary>
    /// Da die CaptionPosition von dem Steuerelement bei bedarf geämndert wird,
    /// muss ein default angegeben werden - wie es normalerweise auszusehen hat.
    /// </summary>
    public CaptionPosition DefaultCaptionPosition { get; private set; }

    public FlexiFilterDefaultFilter Filterart_bei_Texteingabe {
        get => _filterart_bei_texteingabe;
        set {
            if (IsDisposed) { return; }
            if (_filterart_bei_texteingabe == value) { return; }
            _filterart_bei_texteingabe = value;
            OnValueChanged();
        }
    }

    [DefaultValue(null)]
    [Browsable(false)]
    [EditorBrowsable(EditorBrowsableState.Never)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public FilterCollection? FilterInput {
        get => _filterInput;
        set {
            if (_filterInput == value) { return; }
            this.UnRegisterEventsAndDispose();
            _filterInput = value;
            ((IControlAcceptFilter)this).RegisterEvents();
        }
    }

    [Browsable(false)]
    [EditorBrowsable(EditorBrowsableState.Never)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public bool FilterInputChangedHandled { get; set; }

    [DefaultValue(null)]
    [Browsable(false)]
    [EditorBrowsable(EditorBrowsableState.Never)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public FilterCollection FilterOutput { get; } = new("FilterOutput 02");

    [Browsable(false)]
    [EditorBrowsable(EditorBrowsableState.Never)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public ColumnItem? FilterSingleColumn { get; }

    [Browsable(false)]
    [EditorBrowsable(EditorBrowsableState.Never)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public List<IControlSendFilter> Parents { get; } = [];

    public FlexiFilterDefaultOutput Standard_bei_keiner_Eingabe {
        get => _standard_bei_keiner_Eingabe;
        set {
            if (IsDisposed) { return; }
            if (_standard_bei_keiner_Eingabe == value) { return; }
            _standard_bei_keiner_Eingabe = value;
            OnValueChanged();
        }
    }

    #endregion

    #region Methods

    public void FilterInput_DispodingEvent(object sender, System.EventArgs e) => this.FilterInput_DispodingEvent();

    public void FilterInput_RowsChanged(object sender, System.EventArgs e) { }

    public void FilterOutput_DispodingEvent(object sender, System.EventArgs e) => this.FilterOutput_DispodingEvent();

    public void FilterOutput_PropertyChanged(object sender, System.EventArgs e) => this.FilterOutput_PropertyChanged();

    public void HandleChangesNow() {
        if (IsDisposed) { return; }
        if (FilterInputChangedHandled) { return; }
        FilterInputChangedHandled = true;

        this.DoInputFilter(null, false);

        var filterSingle = FilterInput?[FilterSingleColumn];

        if (filterSingle != null) {
            _doFilterDeleteButton = filterSingle.FilterType != FilterType.Instr_GroßKleinEgal;
            if (filterSingle.SearchValue.Count > 1) { _doFilterDeleteButton = true; }
            if (GetComboBox() is ComboBox cb && cb.WasThisValueClicked()) { _doFilterDeleteButton = true; }
            _fromInputFilter = true;
            _origin = filterSingle.Origin;
        } else {
            _fromInputFilter = false;
            _doFilterDeleteButton = false;
            _origin = string.Empty;
        }

        UpdateFilterData(filterSingle, _doFilterDeleteButton);
    }

    public void ParentFilterOutput_Changed() => HandleChangesNow();

    protected override void CommandButton_Click(object sender, System.EventArgs e) {
        //base.CommandButton_Click(); // Nope, keine Ereignisse und auch nicht auf + setzen
        _doFilterDeleteButton = false;

        var filterSingle = FilterInput?[FilterSingleColumn];

        if (filterSingle == null) {
            this.Invalidate_FilterOutput();
            ValueSet(string.Empty, true);
            return;
        }

        if (CaptionPosition == CaptionPosition.ohne) {
            this.Invalidate_FilterOutput();
            return;
        }

        if (FilterSingleColumn is not ColumnItem c) { return; }

        AutoFilter autofilter = new(c, FilterInput, null, Width);
        var p = PointToScreen(Point.Empty);
        autofilter.Position_LocateToPosition(p with { Y = p.Y + Height });
        autofilter.Show();
        autofilter.FilterCommand += AutoFilter_FilterCommand;
        Develop.Debugprint_BackgroundThread();
    }

    protected override void Dispose(bool disposing) {
        if (disposing) {
            ((IControlSendFilter)this).DoDispose();
            ((IControlAcceptFilter)this).DoDispose();
            Tag = null;
            Childs.Clear();
        }

        base.Dispose(disposing);
    }

    protected override void DrawControl(Graphics gr, States state) {
        HandleChangesNow();
        base.DrawControl(gr, state);
    }

    protected override void OnControlAdded(ControlEventArgs e) {
        base.OnControlAdded(e);

        if (e.Control is ComboBox cbx) {
            var item2 = new List<AbstractListItem>();
            item2.Add(ItemOf("Keine weiteren Einträge vorhanden", "|~"));

            //var c = Filter.Column.Contents(null);
            //foreach (var thiss in c)
            //{
            //    Item2.GenerateAndAdd("|" + thiss, thiss));
            //}

            if (TextEntryAllowed()) {
                StyleComboBox(cbx, item2, ComboBoxStyle.DropDown, false);
            } else {
                StyleComboBox(cbx, item2, ComboBoxStyle.DropDownList, false);
            }

            cbx.DropDownShowing += Cbx_DropDownShowing;
        }

        if (e.Control is Button btn) {
            DoButtonStyle(btn);
        }
    }

    protected override void OnValueChanged() {
        base.OnValueChanged();

        var filterSingle = FilterInput?[FilterSingleColumn];

        if (!_doFilterDeleteButton) {
            if (filterSingle != null && Value != filterSingle.SearchValue.JoinWith("|")) { _fromInputFilter = false; }
            if (!_fromInputFilter) { filterSingle = null; }

            if (FilterSingleColumn != null && filterSingle == null) {
                if (string.IsNullOrEmpty(Value)) {
                    if (_standard_bei_keiner_Eingabe == FlexiFilterDefaultOutput.Nichts_Anzeigen) {
                        filterSingle = new FilterItem(FilterSingleColumn, FilterType.AlwaysFalse, string.Empty, _origin);
                    }
                } else {
                    if (GetComboBox() is ComboBox cmb && cmb.WasThisValueClicked()) {
                        _doFilterDeleteButton = true;
                        filterSingle = new FilterItem(FilterSingleColumn, FilterType.Istgleich_ODER_GroßKleinEgal, Value, _origin);
                    } else if (_filterart_bei_texteingabe == FlexiFilterDefaultFilter.Textteil) {
                        filterSingle = new FilterItem(FilterSingleColumn, FilterType.Instr_GroßKleinEgal, Value, _origin);
                    } else {
                        filterSingle = new FilterItem(FilterSingleColumn, FilterType.Istgleich_ODER_GroßKleinEgal, Value, _origin);
                    }
                }
            }
        }
        UpdateFilterData(filterSingle, _doFilterDeleteButton);
    }

    //protected override void OnMouseUp(MouseEventArgs e) {
    //    base.OnMouseUp(e);
    //    if (e.Button == MouseButtons.Right) {
    //        FloatingInputBoxListBoxStyle.ContextMenuShow(this, e);
    //    }
    //}
    //    //#endregion
    //}
    private void AutoFilter_FilterCommand(object sender, FilterCommandEventArgs e) {
        //if (_table?.Filter == null) { return; }

        //_table.Filter.Remove(e.Column);
        //if (e.Command != "Filter") {
        //    return; }

        if (e.Command != "Filter") {
            this.Invalidate_FilterOutput();
            UpdateFilterData(null, false);
        } else {
            FilterOutput.ChangeTo(e.Filter);
            UpdateFilterData(e.Filter, false);

            //if (GetButton() is Button b) {
            //    DoButtonStyle(b);
            //}
        }

        //UpdateFilterData(filterSingle, _wasvalueclicked);

        //if (e.Filter == null) { return; }
        //_table.Filter.Add(e.Filter);
    }

    private void Cbx_DropDownShowing(object sender, System.EventArgs e) {
        var cbx = (ComboBox)sender;
        cbx.ItemClear();
        var listFilterString = AutoFilter.Autofilter_ItemList(FilterSingleColumn, FilterInput, null);
        if (listFilterString.Count == 0) {
            cbx.ItemAdd(ItemOf("Keine weiteren Einträge vorhanden", "|~", ImageCode.Kreuz, false));
        } else if (listFilterString.Count < 400) {
            if (FilterSingleColumn != null) { cbx.ItemAddRange(ItemsOf(listFilterString, FilterSingleColumn, ShortenStyle.Replaced, FilterSingleColumn.BehaviorOfImageAndText)); }
            //cbx.Item.Sort(); // Wichtig, dieser Sort kümmert sich, dass das Format (z. B.  Zahlen) berücksichtigt wird
        } else {
            cbx.ItemAdd(ItemOf("Zu viele Einträge", "|~", ImageCode.Kreuz, false));
        }
    }

    private void DoButtonStyle(Button btn) {
        var filterSingle = FilterInput?[FilterSingleColumn];

        //if (filterSingle == null) { return; }
        btn.Translate = false;

        if (CaptionPosition == CaptionPosition.ohne && filterSingle != null) {
            btn.ImageCode = "Trichter|16||1";
            btn.Text = filterSingle.ReadableText();
        } else {
            if (filterSingle != null && filterSingle.SearchValue.Count > 0 && !string.IsNullOrEmpty(filterSingle.SearchValue[0])) {
                btn.ImageCode = "Trichter|16";
                btn.Text = LanguageTool.DoTranslate("wählen ({0})", true, filterSingle.SearchValue.Count.ToString());
            } else {
                btn.ImageCode = "Trichter|16";
                btn.Text = LanguageTool.DoTranslate("Gewählt:" + Value);
                GenerateQickInfoText(null);
            }
        }
    }

    private void GenerateQickInfoText(FilterItem? filterSingle) {
        if (FilterSingleColumn == null || FilterSingleColumn.IsDisposed) {
            QuickInfo = string.Empty;
            return;
        }

        #region QuickInfo erstellen

        var qi = FilterSingleColumn.QuickInfoText(string.Empty);

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
        if (IsDisposed) { return; }

        if (FilterSingleColumn == null) {
            this.Invalidate_FilterOutput();
        } else {
            FilterOutput.ChangeTo((FilterItem?)filterSingle?.Clone());
        }

        var nvalue = string.Empty;
        if (filterSingle != null) {
            nvalue = filterSingle.SearchValue.JoinWithCr();
        }

        ValueSet(nvalue, true);

        GenerateQickInfoText(filterSingle);

        #region Wenn keine Spalte vorhanden, Fehler machen

        if (FilterSingleColumn == null || FilterSingleColumn.IsDisposed) {
            DisabledReason = "Bezug zum Filter verloren.";
            Caption = "?";
            EditType = EditTypeFormula.nur_als_Text_anzeigen;
            QuickInfo = string.Empty;
            ValueSet(string.Empty, true);
            return;
        }

        #endregion

        DisabledReason = !string.IsNullOrEmpty(_origin) ? "Dieser Filter wurde<br>automatisch gesetzt." : string.Empty;

        #region Wählen-Button - und keine weitere Berechnungen

        if (MustMenu()) {
            CaptionPosition = DefaultCaptionPosition;
            Caption = FilterSingleColumn.ReadableText() + ":";
            EditType = EditTypeFormula.Button;
            if (GetButton() is Button b) { DoButtonStyle(b); }
            return;
        }

        #endregion

        #region Löschen-Button - und keine weiteren Berechnungen

        if (showDelFilterButton) {
            CaptionPosition = CaptionPosition.ohne;
            EditType = EditTypeFormula.Button;
            if (GetButton() is Button b) { DoButtonStyle(b); }
            return;
        }

        #endregion

        if (filterSingle != null) {
            if (filterSingle.FilterType == FilterType.Instr_GroßKleinEgal && filterSingle.SearchValue.Count == 1) {
                //texteingabe = true;
                //showWählen = false;
                nvalue = filterSingle.SearchValue[0];
            } else if (_filterart_bei_texteingabe == FlexiFilterDefaultFilter.Ist) {
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
            EditType = EditTypeFormula.nur_als_Text_anzeigen;
            DisabledReason = "Kein Filter erlaubt";
            return;
        }

        #endregion

        #region Text-Eingabefeld - und keine weitere Berechnungen

        //if (texteingabe) {
        CaptionPosition = DefaultCaptionPosition;
        Caption = FilterSingleColumn.ReadableText() + ":";
        EditType = EditTypeFormula.Textfeld_mit_Auswahlknopf;
        ValueSet(nvalue, true);
        //return;
        //}

        #endregion
    }

    #endregion
}