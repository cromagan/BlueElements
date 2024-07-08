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
public partial class FlexiControlForFilter : GenericControlReciver, IControlSendFilter, IDisposableExtended, IHasSettings {

    #region Fields

    private const int MaxCount = 20;
    private bool _doFilterDeleteButton;

    private FlexiFilterDefaultFilter _filterart_bei_texteingabe = FlexiFilterDefaultFilter.Textteil;

    private bool _fromInputFilter;

    private string _origin;

    private FlexiFilterDefaultOutput _standard_bei_keiner_Eingabe = FlexiFilterDefaultOutput.Alles_Anzeigen;

    #endregion

    #region Constructors

    public FlexiControlForFilter(ColumnItem? column, CaptionPosition defaultCaptionPosition) : base(false, false) {
        // Dieser Aufruf ist für den Designer erforderlich.
        InitializeComponent();

        // Fügen Sie Initialisierungen nach dem InitializeComponent()-Aufruf hinzu.
        Size = new Size(204, 24);
        //AlwaysInstantChange = true;
        FilterSingleColumn = column;
        this.Invalidate_FilterInput();
        f.ShowInfoWhenDisabled = true;
        _origin = string.Empty;
        _fromInputFilter = false;
        ((IControlSendFilter)this).RegisterEvents();
        base.RegisterEvents();
        DefaultCaptionPosition = defaultCaptionPosition;
        F_ValueChanged(null, System.EventArgs.Empty);
    }

    #endregion

    #region Properties

    [Browsable(false)]
    [EditorBrowsable(EditorBrowsableState.Never)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public List<GenericControlReciver> Childs { get; } = [];

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
            F_ValueChanged(null, System.EventArgs.Empty);
        }
    }

    [DefaultValue(null)]
    [Browsable(false)]
    [EditorBrowsable(EditorBrowsableState.Never)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public FilterCollection FilterOutput { get; } = new("FilterOutput 02");

    [Browsable(false)]
    [EditorBrowsable(EditorBrowsableState.Never)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public ColumnItem? FilterSingleColumn { get; }

    public bool SavesSettings { get; internal set; } = false;

    public List<string> Settings { get; } = new();

    public bool SettingsLoaded { get; set; }

    public string SettingsManualFilename { get; set; }

    public FlexiFilterDefaultOutput Standard_bei_keiner_Eingabe {
        get => _standard_bei_keiner_Eingabe;
        set {
            if (IsDisposed) { return; }
            if (_standard_bei_keiner_Eingabe == value) { return; }
            _standard_bei_keiner_Eingabe = value;
            F_ValueChanged(null, System.EventArgs.Empty);
        }
    }

    #endregion

    #region Methods

    public void FilterOutput_DispodingEvent(object sender, System.EventArgs e) => this.FilterOutput_DispodingEvent();

    public void FilterOutput_PropertyChanged(object sender, System.EventArgs e) {
        if (SavesSettings) {
            this.LoadSettingsFromDisk(false);
            if (FilterOutput.Count == 1) {
                if (FilterOutput.Rows.Count == 1) {
                    var toAdd = this.FilterHash() + "|" + FilterOutput[0].SearchValue.JoinWithCr();
                    this.SettingsAdd(toAdd);
                }
            }
        }

        this.FilterOutput_PropertyChanged();
    }

    public override void HandleChangesNow() {
        base.HandleChangesNow();
        if (IsDisposed) { return; }
        if (FilterInputChangedHandled) { return; }
        FilterInputChangedHandled = true;

        this.DoInputFilter(null, false);

        var filterSingle = FilterInput?[FilterSingleColumn];

        if (filterSingle != null) {
            _doFilterDeleteButton = filterSingle.FilterType != FilterType.Instr_GroßKleinEgal;
            if (filterSingle.SearchValue.Count > 1) { _doFilterDeleteButton = true; }
            if (f.GetComboBox() is ComboBox cb && cb.WasThisValueClicked()) { _doFilterDeleteButton = true; }
            _fromInputFilter = true;
            _origin = filterSingle.Origin;
        } else {
            _fromInputFilter = false;
            _doFilterDeleteButton = false;
            _origin = string.Empty;
        }

        UpdateFilterData(filterSingle, _doFilterDeleteButton);
    }

    public override void ParentFilterOutput_Changed() {
        base.ParentFilterOutput_Changed();
        HandleChangesNow();
    }

    protected override void Dispose(bool disposing) {
        if (disposing) {
            Tag = null;
            Childs.Clear();
        }

        base.Dispose(disposing);
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

    private void AutoFilter_FilterCommand(object sender, FilterCommandEventArgs e) {
        if (e.Command != "Filter") {
            this.Invalidate_FilterOutput();
            UpdateFilterData(null, false);
        } else {
            FilterOutput.ChangeTo(e.Filter);
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
            if (FilterSingleColumn != null) { cbx.ItemAddRange(ItemsOf(listFilterString, FilterSingleColumn, ShortenStyle.Replaced, FilterSingleColumn.BehaviorOfImageAndText)); }
            //cbx.Item.Sort(); // Wichtig, dieser Sort kümmert sich, dass das Format (z. B.  Zahlen) berücksichtigt wird
        } else {
            if (SavesSettings) {
                this.LoadSettingsFromDisk(false);
                var nr = -1;
                var f = this.FilterHash();

                for (var z = Settings.Count - 1; z >= 0; z--) {
                    var x = Settings[z].SplitAndCutBy("|");
                    if (x.GetUpperBound(0) > 0 && !string.IsNullOrEmpty(x[1]) && f == x[0]) {
                        nr++;
                        if (nr < MaxCount) {
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
            if (filterSingle != null && filterSingle.SearchValue.Count > 0 && !string.IsNullOrEmpty(filterSingle.SearchValue[0])) {
                btn.ImageCode = "Trichter|16";
                btn.Text = LanguageTool.DoTranslate("wählen ({0})", true, filterSingle.SearchValue.Count.ToString());
            } else {
                btn.ImageCode = "Trichter|16";
                btn.Text = LanguageTool.DoTranslate("Gewählt:" + f.Value);
                GenerateQickInfoText(null);
            }
        }
    }

    private void F_ButtonClick(object sender, System.EventArgs e) {
        //base.CommandButton_Click(); // Nope, keine Ereignisse und auch nicht auf + setzen
        _doFilterDeleteButton = false;

        var filterSingle = FilterInput?[FilterSingleColumn];

        if (filterSingle == null) {
            this.Invalidate_FilterOutput();
            f.ValueSet(string.Empty, true);
            return;
        }

        if (f.CaptionPosition == CaptionPosition.ohne) {
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

    private void F_ValueChanged(object sender, System.EventArgs e) {
        var filterSingle = FilterInput?[FilterSingleColumn];

        if (!_doFilterDeleteButton) {
            if (filterSingle != null && f.Value != filterSingle.SearchValue.JoinWith("|")) { _fromInputFilter = false; }
            if (!_fromInputFilter) { filterSingle = null; }

            if (FilterSingleColumn != null && filterSingle == null) {
                if (string.IsNullOrEmpty(f.Value)) {
                    if (_standard_bei_keiner_Eingabe == FlexiFilterDefaultOutput.Nichts_Anzeigen) {
                        filterSingle = new FilterItem(FilterSingleColumn, FilterType.AlwaysFalse, string.Empty, _origin);
                    }
                } else {
                    if (f.GetComboBox() is ComboBox cmb && cmb.WasThisValueClicked()) {
                        _doFilterDeleteButton = true;
                        filterSingle = new FilterItem(FilterSingleColumn, FilterType.Istgleich_ODER_GroßKleinEgal, f.Value, _origin);
                    } else if (_filterart_bei_texteingabe == FlexiFilterDefaultFilter.Textteil) {
                        filterSingle = new FilterItem(FilterSingleColumn, FilterType.Instr_GroßKleinEgal, f.Value, _origin);
                    } else {
                        filterSingle = new FilterItem(FilterSingleColumn, FilterType.Istgleich_ODER_GroßKleinEgal, f.Value, _origin);
                    }
                }
            }
        }
        UpdateFilterData(filterSingle, _doFilterDeleteButton);
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

        f.ValueSet(nvalue, true);

        GenerateQickInfoText(filterSingle);

        #region Wenn keine Spalte vorhanden, Fehler machen

        if (FilterSingleColumn == null || FilterSingleColumn.IsDisposed) {
            f.DisabledReason = "Bezug zum Filter verloren.";
            f.Caption = "?";
            f.EditType = EditTypeFormula.nur_als_Text_anzeigen;
            QuickInfo = string.Empty;
            f.ValueSet(string.Empty, true);
            return;
        }

        #endregion

        f.DisabledReason = !string.IsNullOrEmpty(_origin) ? "Dieser Filter wurde<br>automatisch gesetzt." : string.Empty;

        #region Wählen-Button - und keine weitere Berechnungen

        if (MustMenu()) {
            f.CaptionPosition = DefaultCaptionPosition;
            f.Caption = FilterSingleColumn.ReadableText() + ":";
            f.EditType = EditTypeFormula.Button;
            if (f.GetButton() is Button b) { DoButtonStyle(b); }
            return;
        }

        #endregion

        #region Löschen-Button - und keine weiteren Berechnungen

        if (showDelFilterButton) {
            f.CaptionPosition = CaptionPosition.ohne;
            f.EditType = EditTypeFormula.Button;
            if (f.GetButton() is Button b) { DoButtonStyle(b); }
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