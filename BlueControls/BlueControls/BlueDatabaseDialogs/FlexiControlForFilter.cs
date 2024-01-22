// Authors:
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

using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using BlueBasics;
using BlueBasics.Enums;
using BlueControls.BlueDatabaseDialogs;
using BlueControls.Designer_Support;
using BlueControls.Enums;
using BlueControls.EventArgs;
using BlueControls.Interfaces;
using BlueDatabase;
using BlueDatabase.Enums;

#nullable enable

namespace BlueControls.Controls;

[Designer(typeof(BasicDesigner))]
public partial class FlexiControlForFilter : FlexiControl, IControlSendSomething, IControlAcceptSomething {
    //public FlexiControlForFilter() : this(null, null) { }

    #region Fields

    private bool _doFilterDeleteButton;
    private FlexiFilterDefaultFilter _filterart_bei_texteingabe = FlexiFilterDefaultFilter.Textteil;
    private string _origin;
    private FlexiFilterDefaultOutput _standard_bei_keiner_Eingabe = FlexiFilterDefaultOutput.Alles_Anzeigen;

    #endregion

    #region Constructors

    public FlexiControlForFilter(ColumnItem? column) {
        // Dieser Aufruf ist für den Designer erforderlich.
        InitializeComponent();

        // Fügen Sie Initialisierungen nach dem InitializeComponent()-Aufruf hinzu.
        Size = new Size(204, 24);
        AlwaysInstantChange = true;
        FilterSingleColumn = column;
        this.Invalidate_FilterInput(true);
        ShowInfoWhenDisabled = true;
        _origin = string.Empty;
        OnValueChanged();
    }

    #endregion

    #region Properties

    public List<IControlAcceptSomething> Childs { get; } = [];

    /// <summary>
    /// Da die CaptionPosition von dem Steuerelemnt bei bedarf geämndert wird,
    /// muss ein default angegeben werden - wie es normalerweise auszusehen hat.
    /// </summary>
    public CaptionPosition DefaultCaptionPosition { get; set; } = CaptionPosition.Links_neben_Dem_Feld;

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
    public FilterCollection? FilterInput { get; set; }

    public bool FilterManualSeted { get; set; } = false;

    public FilterCollection FilterOutput { get; } = new("FilterOutput");

    public ColumnItem? FilterSingleColumn { get; }

    [Browsable(false)]
    [EditorBrowsable(EditorBrowsableState.Never)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public List<IControlSendSomething> Parents { get; } = [];

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

    //public bool ContextMenuItemClickedInternalProcessig(object sender, ContextMenuItemClickedEventArgs e) {
    //    switch (e.ClickedCommand.ToLower()) {
    //        case "#columnedit":
    //            if (e.HotItem is ColumnItem col) {
    //                Forms.TableView.OpenColumnEditor(col, null);
    //            }
    //            return true;
    //    }
    //    return false;
    //}

    #region Methods

    public void FilterInput_Changed(object? sender, System.EventArgs e) {
        if (FilterManualSeted) { Develop.DebugPrint("Steuerelement unterstützt keine manuellen Filter"); }
        this.DoInputFilter();

        var filterSingle = FilterInput?[FilterSingleColumn];

        _origin = filterSingle?.Origin ?? string.Empty;

        if (filterSingle != null) {
            _doFilterDeleteButton = filterSingle.FilterType is FilterType.Istgleich_GroßKleinEgal or FilterType.Istgleich_ODER_GroßKleinEgal;

            if (filterSingle.SearchValue.Count > 1) { _doFilterDeleteButton = true; }

            ValueSet(filterSingle.SearchValue.JoinWithCr(), true, true);
        } else {
            _doFilterDeleteButton = false;
            ValueSet(string.Empty, true, true);
        }

        Invalidate();
    }

    public void FilterInput_Changing(object sender, System.EventArgs e) { }

    public void Parents_Added(bool hasFilter) {
        if (IsDisposed) { return; }
        if (!hasFilter) { return; }

        FilterInput_Changed(null, System.EventArgs.Empty);
    }

    //public void GetContextMenuItems(MouseEventArgs? e, ItemCollectionList.ItemCollectionList items, out object? hotItem) {
    //    hotItem = null;
    //    if (FilterSingleColumn?.Database is not Database db || db.IsDisposed || !db.IsAdministrator()) { return; }

    //    hotItem = FilterSingleColumn;
    //    _ = items.Add("Spalte bearbeiten", "#ColumnEdit", QuickImage.Get(ImageCode.Spalte));
    //}

    protected override void CommandButton_Click(object sender, System.EventArgs e) {
        //base.CommandButton_Click(); // Nope, keine Ereignisse und auch nicht auf + setzen
        _doFilterDeleteButton = false;

        if (CaptionPosition == CaptionPosition.ohne) {
            FilterOutput.Clear();
            return;
        }

        if (FilterSingleColumn is not ColumnItem c) { return; }

        //if( FilterInput?.Clone("ButtonClicked") is not FilterCollection f) {  return; }

        //if (f.CaptionPosition == ÜberschriftAnordnung.ohne) {
        //    // ein Großer Knopf ohne Überschrift, da wird der evl. Filter gelöscht
        //    _table.Filter.Remove(((FlexiControlForFilter)sender).FilterSingle);
        //    return;
        //}

        //if (f.FilterSingleColumn == null) { return; }

        ////f.Enabled = false;
        AutoFilter autofilter = new(c, FilterInput, null, Width);
        var p = PointToScreen(Point.Empty);
        autofilter.Position_LocateToPosition(p with { Y = p.Y + Height });
        autofilter.Show();
        autofilter.FilterCommand += AutoFilter_FilterCommand;
        Develop.Debugprint_BackgroundThread();
    }

    protected override void Dispose(bool disposing) {
        base.Dispose(disposing);

        if (disposing) {
            FilterOutput.Dispose();
            this.Invalidate_FilterInput(false);
            Tag = null;
            Childs.Clear();
        }
    }

    protected override void OnControlAdded(ControlEventArgs e) {
        base.OnControlAdded(e);

        if (e.Control is ComboBox cbx) {
            ItemCollectionList.ItemCollectionList item2 = new(true)
            {
                { "Keine weiteren Einträge vorhanden", "|~" }
            };
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

        if (MustMenu() && GetButton() is Button b) {
            DoButtonStyle(b);
            return;
        }

        if (GetComboBox() is ComboBox cb && cb.WasThisValueClicked()) { _doFilterDeleteButton = true; }

        FilterItem? filterSingle = null;

        if (FilterSingleColumn != null) {
            if (string.IsNullOrEmpty(Value)) {
                if (_standard_bei_keiner_Eingabe == FlexiFilterDefaultOutput.Nichts_Anzeigen) {
                    filterSingle = new FilterItem(FilterSingleColumn, FilterType.AlwaysFalse, string.Empty, _origin);
                } else {
                    filterSingle = null;
                }
            } else {
                if (_doFilterDeleteButton) {
                    filterSingle = new FilterItem(FilterSingleColumn, FilterType.Istgleich_ODER_GroßKleinEgal, Value.SplitByCr(), _origin);
                } else {
                    if (_filterart_bei_texteingabe == FlexiFilterDefaultFilter.Textteil) {
                        filterSingle = new FilterItem(FilterSingleColumn, FilterType.Instr_GroßKleinEgal, Value, _origin);
                    } else {
                        filterSingle = new FilterItem(FilterSingleColumn, FilterType.Istgleich_ODER_GroßKleinEgal, Value, _origin);
                    }
                }
            }
        }

        if (FilterSingleColumn == null) {
            FilterOutput.Clear();
        } else {
            FilterOutput.ChangeTo(filterSingle);
        }

        UpdateFilterData(filterSingle, _doFilterDeleteButton);

        //if (filterSingle != null && filterSingle.IsOk()) {
        //    FilterOutput.ChangeTo(filterSingle);
        //    return;
        //}

        //var isFilter = WasThisValueClicked(); //  flx.Value.StartsWith("|");
        //                                          //flx.Filter.Herkunft = "Filterleiste";
        //var v = flx.Value; //.Trim("|");
        //if (_table.Filter.Count == 0 || !_table.Filter.Contains(flx.FilterSingle)) {
        //    if (isFilter) { flx.FilterSingle.FilterType = FilterType.Istgleich_ODER_GroßKleinEgal; } // Filter noch nicht in der Collection, kann ganz einfach geändert werden
        //    flx.FilterSingle.Changeto(flx.FilterSingle.FilterType, v);
        //    _table.Filter.Add(flx.FilterSingle);
        //    return;
        //}
        //if (flx.FilterSingle.SearchValue.Count != 1) {
        //    Develop.DebugPrint_NichtImplementiert();
        //    return;
        //}
        //if (isFilter) {
        //    flx.FilterSingle.Changeto(FilterType.Istgleich_ODER_GroßKleinEgal, v);
        //} else {
        //    if (string.IsNullOrEmpty(v)) {
        //        _table.Filter.Remove(flx.FilterSingle);
        //    } else {
        //        flx.FilterSingle.Changeto(FilterType.Instr_GroßKleinEgal, v);
        //        // flx.Filter.SearchValue[0] =v;
        //    }
        //}
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
            FilterOutput.Clear();
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
        cbx.Item.Clear();
        cbx.Item.CheckBehavior = CheckBehavior.MultiSelection;
        //if (TableView == null) {
        //    _ = cbx.Item.Add("Anzeigefehler", "|~", ImageCode.Kreuz, false);
        //    return;
        //}
        var listFilterString = AutoFilter.Autofilter_ItemList(FilterSingleColumn, FilterInput, null);
        if (listFilterString.Count == 0) {
            _ = cbx.Item.Add("Keine weiteren Einträge vorhanden", "|~", ImageCode.Kreuz, false);
        } else if (listFilterString.Count < 400) {
            if (FilterSingleColumn != null) { cbx.Item.AddRange(listFilterString, FilterSingleColumn, ShortenStyle.Replaced, FilterSingleColumn.BehaviorOfImageAndText); }
            //cbx.Item.Sort(); // Wichtig, dieser Sort kümmert sich, dass das Format (z. B.  Zahlen) berücksichtigt wird
        } else {
            _ = cbx.Item.Add("Zu viele Einträge", "|~", ImageCode.Kreuz, false);
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
                btn.Text = LanguageTool.DoTranslate("wählen");
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

        GenerateQickInfoText(filterSingle);

        #region Wenn keine Spalte vorhanden, Fehler machen

        if (FilterSingleColumn == null || FilterSingleColumn.IsDisposed) {
            DisabledReason = "Bezug zum Filter verloren.";
            Caption = "?";
            EditType = EditTypeFormula.nur_als_Text_anzeigen;
            QuickInfo = string.Empty;
            ValueSet(string.Empty, true, true);
            return;
        }

        #endregion

        DisabledReason = !string.IsNullOrEmpty(_origin) ? "Dieser Filter wurde<br>automatisch gesetzt." : string.Empty;

        //var showWählen = MustMenu();

        //var texteingabe = TextEntryAllowed();

        var nvalue = string.Empty;

        #region Wählen-Button - und keine weitere Berechnungen

        if (MustMenu()) {
            CaptionPosition = DefaultCaptionPosition;
            Caption = FilterSingleColumn.ReadableText() + ":";
            EditType = EditTypeFormula.Button;
            return;
        }

        #endregion

        #region Löschen-Button - und keine weiteren Berechnungen

        if (showDelFilterButton) {
            CaptionPosition = CaptionPosition.ohne;
            EditType = EditTypeFormula.Button;
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
        ValueSet(nvalue, true, true);
        return;
        //}

        #endregion
    }

    #endregion
}