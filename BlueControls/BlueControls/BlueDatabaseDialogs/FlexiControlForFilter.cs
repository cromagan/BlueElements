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

    #region Constructors

    public FlexiControlForFilter(ColumnItem? column) {
        // Dieser Aufruf ist für den Designer erforderlich.
        InitializeComponent();

        // Fügen Sie Initialisierungen nach dem InitializeComponent()-Aufruf hinzu.
        Size = new Size(204, 24);
        AlwaysInstantChange = true;
        FilterSingleColumn = column;
        FilterInput = null;
        //UpdateFilterData();
        //FilterSingle.Changed += FilterSingle_Changed;
    }

    #endregion

    #region Properties

    public List<IControlAcceptSomething> Childs { get; } = [];

    [DefaultValue(null)]
    [Browsable(false)]
    [EditorBrowsable(EditorBrowsableState.Never)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public FilterCollection? FilterInput { get; set; }

    public bool FilterManualSeted { get; set; } = false;
    public FilterCollection FilterOutput { get; } = new("FilterOutput");

    public ColumnItem? FilterSingleColumn { get; private set; }

    [Browsable(false)]
    [EditorBrowsable(EditorBrowsableState.Never)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public List<IControlSendSomething> Parents { get; } = [];

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

    public void FilterInput_Changed(object sender, System.EventArgs e) {
        if (FilterManualSeted) { Develop.DebugPrint("Steuerelement unterstützt keine manuellen Filter"); }
        FilterInput?.Dispose();
        FilterInput = null;
        Invalidate();
    }

    public void FilterInput_Changing(object sender, System.EventArgs e) { }

    //public void GetContextMenuItems(MouseEventArgs? e, ItemCollectionList.ItemCollectionList items, out object? hotItem) {
    //    hotItem = null;
    //    if (FilterSingleColumn?.Database is not Database db || db.IsDisposed || !db.IsAdministrator()) { return; }

    //    hotItem = FilterSingleColumn;
    //    _ = items.Add("Spalte bearbeiten", "#ColumnEdit", QuickImage.Get(ImageCode.Spalte));
    //}

    //internal bool WasThisValueClicked() {
    //    var cb = GetComboBox();
    //    return cb != null && cb.WasThisValueClicked();
    //}

    protected override void Dispose(bool disposing) {
        base.Dispose(disposing);

        if (disposing) {
            FilterInput?.Dispose();
            FilterOutput.Dispose();
            FilterInput = null;
            Tag = null;
            Childs.Clear();
        }
    }

    protected override void DrawControl(Graphics gr, States state) {
        if (FilterInput == null) {
            //UpdateMyCollection();
            UpdateFilterData();
        }
        base.DrawControl(gr, state);
    }

    protected override void OnButtonClicked() {
        base.OnButtonClicked();

        if (CaptionPosition == ÜberschriftAnordnung.ohne) {
            FilterOutput.Clear();
            return;
        }

        //if(FilterSingleColumn is not ColumnItem c) { return; }

        //if( FilterInput?.Clone("ButtonClicked") is not FilterCollection f) {  return; }

        //var f = (FlexiControlForFilter)sender;
        //if (f.CaptionPosition == ÜberschriftAnordnung.ohne) {
        //    // ein Großer Knopf ohne Überschrift, da wird der evl. Filter gelöscht
        //    _table.Filter.Remove(((FlexiControlForFilter)sender).FilterSingle);
        //    return;
        //}

        //if (f.FilterSingleColumn == null) { return; }

        ////f.Enabled = false;
        //AutoFilter autofilter = new(f.FilterSingleColumn, _table.Filter, _table.PinnedRows, f.Width);
        //var p = f.PointToScreen(Point.Empty);
        //autofilter.Position_LocateToPosition(p with { Y = p.Y + f.Height });
        //autofilter.Show();
        //autofilter.FilterCommand += AutoFilter_FilterCommand;
        //Develop.Debugprint_BackgroundThread();
    }

    protected override void OnControlAdded(ControlEventArgs e) {
        base.OnControlAdded(e);

        var filterSingle = FilterInput?[FilterSingleColumn];

        if (filterSingle == null) { return; }

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
            StyleComboBox(cbx, item2, ComboBoxStyle.DropDown, false);
            cbx.DropDownShowing += Cbx_DropDownShowing;
        }

        if (e.Control is Button btn) {
            btn.Translate = false;

            if (CaptionPosition == ÜberschriftAnordnung.ohne) {
                btn.ImageCode = "Trichter|16||1";
                btn.Text = filterSingle.ReadableText();
            } else {
                if (filterSingle.SearchValue.Count > 0 && !string.IsNullOrEmpty(filterSingle.SearchValue[0])) {
                    btn.ImageCode = "Trichter|16";
                    btn.Text = LanguageTool.DoTranslate("wählen ({0})", true, filterSingle.SearchValue.Count.ToString());
                } else {
                    btn.ImageCode = "Trichter|16";
                    btn.Text = LanguageTool.DoTranslate("wählen");
                }
            }
        }
    }

    //protected override void OnMouseUp(MouseEventArgs e) {
    //    base.OnMouseUp(e);
    //    if (e.Button == MouseButtons.Right) {
    //        FloatingInputBoxListBoxStyle.ContextMenuShow(this, e);
    //    }
    //}

    protected override void OnValueChanged() {
        base.OnValueChanged();

        if (EditType == EditTypeFormula.Button) { return; }

        if (FilterOutput.Count != 1) { return; }

        if (FilterOutput[0] is FilterItem f) {
            f.Changeto(f.FilterType, Value);
        }

        //if (_table?.Filter == null) { return; }
        //var isFilter = flx.WasThisValueClicked(); //  flx.Value.StartsWith("|");
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

    //    //#endregion
    //}
    private void AutoFilter_FilterCommand(object sender, FilterCommandEventArgs e) {
        //if (_table?.Filter == null) { return; }

        //_table.Filter.Remove(e.Column);
        //if (e.Command != "Filter") { return; }

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

    private void UpdateFilterData() {
        //RemoveAll();

        if (IsDisposed) { return; }
        this.DoInputFilter();

        if (!Allinitialized) { _ = CreateSubControls(); }

        #region Wenn keine Spalte vorhanden, Fehler machen

        if (FilterSingleColumn == null || FilterSingleColumn.IsDisposed) {
            DisabledReason = "Bezug zum Filter verloren.";
            Caption = string.Empty;
            EditType = EditTypeFormula.None;
            QuickInfo = string.Empty;
            ValueSet(string.Empty, true, true);
            return;
        }

        #endregion

        var filterSingle = FilterInput?[FilterSingleColumn];

        FilterOutput.ChangeTo(filterSingle);

        DisabledReason = filterSingle != null && !string.IsNullOrEmpty(filterSingle.Herkunft) ?
            "Dieser Filter ist automatisch<br>gesetzt worden." : string.Empty;

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
            if (string.IsNullOrEmpty(qi)) {
                QuickInfo = "<b>Info:</b><br>" + qi;
            }
        }

        #endregion

        if (!FilterSingleColumn.AutoFilterSymbolPossible()) {
            EditType = EditTypeFormula.None;
            return;
        }

        var showDelFilterButton = true;
        var showWählen = FilterSingleColumn.FilterOptions.HasFlag(FilterOptions.OnlyAndAllowed) ||
             FilterSingleColumn.FilterOptions.HasFlag(FilterOptions.OnlyOrAllowed);

        var texteingabe = FilterSingleColumn.FilterOptions.HasFlag(FilterOptions.TextFilterEnabled);

        if (filterSingle != null) {
            if (filterSingle.FilterType == FilterType.Instr_GroßKleinEgal && filterSingle.SearchValue.Count == 1) {
                CaptionPosition = ÜberschriftAnordnung.Links_neben_Dem_Feld;
                Caption = FilterSingleColumn.ReadableText() + ":";
                EditType = EditTypeFormula.Textfeld_mit_Auswahlknopf;
                ValueSet(filterSingle.SearchValue[0], true, true);
                showDelFilterButton = false;
                showWählen = false;
            }

            if (showWählen && filterSingle.FilterType != FilterType.Instr_GroßKleinEgal) {
                showDelFilterButton = false;
            }
        }

        if ((showWählen || !texteingabe) && !showDelFilterButton) {
            showDelFilterButton = false;
            CaptionPosition = ÜberschriftAnordnung.Links_neben_Dem_Feld;
            Caption = FilterSingleColumn.ReadableText() + ":";
            EditType = EditTypeFormula.Button;
        }

        if (showDelFilterButton) {
            CaptionPosition = ÜberschriftAnordnung.ohne;
            EditType = EditTypeFormula.Button;
        }
    }

    #endregion

    //private void UpdateMyCollection() {
    //    //if (IsDisposed) { return; }
    //    //if (!Allinitialized) { _ = CreateSubControls(); }

    //    //this.DoInputFilter();

    //    //#region Combobox suchen

    //    //ComboBox? cb = null;
    //    //foreach (var thiscb in Controls) {
    //    //    if (thiscb is ComboBox cbx) { cb = cbx; break; }
    //    //}

    //    //#endregion

    //    //if (cb == null) { return; }

    //    //List<AbstractListItem> ex = [.. cb.Item];

    //    //#region Zeilen erzeugen

    //    ////FilterInput = this.FilterOfSender();
    //    //if (FilterInput == null) { return; }

    //    //var f = FilterInput.Rows;
    //    //foreach (var thisR in f) {
    //    //    if (cb?.Item?[thisR.KeyName] == null) {
    //    //        var tmpQuickInfo = thisR.ReplaceVariables(_showformat, true, true);
    //    //        _ = cb?.Item?.Add(tmpQuickInfo, thisR.KeyName);
    //    //        //cb.Item.Add(thisR, string.Empty);
    //    //    } else {
    //    //        foreach (var thisIt in ex) {
    //    //            if (thisIt.KeyName == thisR.KeyName) {
    //    //                _ = ex.Remove(thisIt);
    //    //                break;
    //    //            }
    //    //        }
    //    //    }
    //    //}

    //    //#endregion

    //    //#region Veraltete Zeilen entfernen

    //    //foreach (var thisit in ex) {
    //    //    cb?.Item?.Remove(thisit);
    //    //}

    //    //#endregion

    //    //#region Nur eine Zeile? auswählen!

    //    //// nicht vorher auf null setzen, um Blinki zu vermeiden
    //    //if (cb?.Item != null && cb.Item.Count == 1) {
    //    //    ValueSet(cb.Item[0].KeyName, true, true);
    //    //}

    //    //if (cb?.Item == null || cb.Item.Count < 2) {
    //    //    DisabledReason = "Keine Auswahl möglich.";
    //    //} else {
    //    //    DisabledReason = string.Empty;
    //    //}

    //    //#endregion

    //    //#region  Prüfen ob die aktuelle Auswahl passt

    //    //// am Ende auf null setzen, um Blinki zu vermeiden

    //    //if (cb?.Item?[Value] == null) {
    //    //    ValueSet(string.Empty, true, true);
    //    //}
}