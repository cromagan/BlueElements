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

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using BlueBasics;
using BlueBasics.Enums;
using BlueControls.CellRenderer;
using BlueControls.Enums;
using BlueControls.EventArgs;
using BlueControls.Forms;
using BlueControls.ItemCollectionList;
using BlueDatabase;
using BlueDatabase.Enums;
using static BlueBasics.Converter;
using static BlueControls.ItemCollectionList.AbstractListItemExtension;

namespace BlueControls.BlueDatabaseDialogs;

public partial class AutoFilter : FloatingForm //System.Windows.Forms.UserControl //
{
    #region Fields

    private readonly ColumnItem _column;

    private bool _multiAuswahlOder;

    private bool _multiAuswahlUnd;

    private bool _negativAuswahl;

    #endregion

    #region Constructors

    public AutoFilter(ColumnItem column, FilterCollection? fc, List<RowItem>? pinned, int minWidth, Renderer_Abstract? renderer) : base(Design.Form_AutoFilter) {
        // Dieser Aufruf ist für den Windows Form-Designer erforderlich.
        InitializeComponent();

        _column = column;

        GenerateAll(fc, pinned, minWidth, renderer);
    }

    #endregion

    #region Events

    public event EventHandler<FilterCommandEventArgs>? FilterCommand;

    #endregion

    #region Methods

    public static List<string> Autofilter_ItemList(ColumnItem? column, FilterCollection? fc, List<RowItem>? pinned) {
        if (column is not { IsDisposed: false }) { return []; }

        if (fc is not { Count: >= 0 }) { return column.Contents(); }
        var fc2 = (FilterCollection)fc.Clone("autofilter");
        fc2.Remove(column);

        //foreach (var thisFilter in filter) {
        //    if (thisFilter != null && column != thisFilter.Column) {
        //        tfilter.Add(thisFilter);
        //    }
        //}

        return column.Contents(fc2, pinned);
    }

    public void GenerateAll(FilterCollection? fc, List<RowItem>? pinned, int minWidth, Renderer_Abstract renderer) {
        var nochOk = true;
        var listFilterString = Autofilter_ItemList(_column, fc, pinned);
        //var f = Skin.GetBlueFont(Design.Table_Cell, States.Standard);

        //ACHUNG:
        // Column ist für die Filter in dieser Datenbank zuständig
        // lColumn für das Aussehen und Verhalten des FilterDialogs

        //if (_column.Function == ColumnFunction.Verknüpfung_zu_anderer_Datenbank) {
        //    _ = CellCollection.LinkedCellData(_column, null, false, false);
        //}

        Width = Math.Max(txbEingabe.Width + (Skin.Padding * 2), minWidth);
        lsbFilterItems.ItemClear();
        lsbFilterItems.CheckBehavior = CheckBehavior.MultiSelection;

        if (listFilterString.Count < 400) {
            lsbFilterItems.ItemAddRange(ItemsOf(listFilterString, _column, renderer));
            //lsbFilterItems.Item.Sort(); // Wichtig, dieser Sort kümmert sich, dass das Format (z. B.  Zahlen) berücksichtigt wird
        } else {
            lsbFilterItems.ItemAdd(ItemOf("Zu viele Einträge", "x", ImageCode.Kreuz, false));
            nochOk = false;
        }

        var prefSize = lsbFilterItems.CalculateColumnAndSize(renderer);
        lsbFilterItems.Anchor = AnchorStyles.Left | AnchorStyles.Top;
        lsbFilterItems.Width = Math.Min(minWidth, Width - (Skin.PaddingSmal * 2));
        lsbFilterItems.Width = Math.Max(lsbFilterItems.Width, prefSize.Width);
        lsbFilterItems.Height = Math.Max(lsbFilterItems.Height, prefSize.Height);
        lsbFilterItems.Width = Math.Max(lsbFilterItems.Width, Width - (Skin.PaddingSmal * 2));
        lsbFilterItems.Height = Math.Min(lsbFilterItems.Height, 560);

        AbstractListItem? leere = null;
        AbstractListItem? nichtleere = null;

        #region die Besonderen Filter generieren

        if (_column.FilterOptions is not FilterOptions.Enabled_OnlyAndAllowed and not FilterOptions.Enabled_OnlyOrAllowed) {
            txbEingabe.Enabled = _column.FilterOptions.HasFlag(FilterOptions.TextFilterEnabled);
            capWas.Enabled = _column.FilterOptions.HasFlag(FilterOptions.TextFilterEnabled);
            if (_column.SortType is SortierTyp.ZahlenwertFloat or SortierTyp.ZahlenwertInt) {
                capWas.Text = "...oder von-bis:";
            }
            lsbStandardFilter.ItemClear();
            if (fc != null) {
                lsbStandardFilter.ItemAdd(ItemOf("Filter löschen", "filterlöschen", QuickImage.Get("Trichter|16||1"), fc[_column] != null, Constants.FirstSortChar + "01"));
            } else {
                lsbStandardFilter.ItemAdd(ItemOf("Filter löschen", "filterlöschen", QuickImage.Get("Trichter|16||1"), false, Constants.FirstSortChar + "01"));
            }

            var tmp = renderer.ValueReadable(string.Empty, ShortenStyle.Replaced, _column.DoOpticalTranslation);
            bool nichtleereallowed;
            if (string.IsNullOrEmpty(tmp)) {
                leere = ItemOf("leere", "filterleere", QuickImage.Get("TasteABC|20|16|1"), true, Constants.FirstSortChar + "02");
                nichtleereallowed = true;
            } else {
                leere = ItemOf(tmp + " (= leere)", "filterleere", QuickImage.Get("TasteABC|20|16|1"), true, Constants.FirstSortChar + "02");
                nichtleereallowed = false;
            }

            if (_column is { } && _column == _column.Database?.Column.SplitColumn) {
                nichtleereallowed = false;
            }

            nichtleere = ItemOf("nicht leere", "filternichtleere", QuickImage.Get("TasteABC|20|16"), nichtleereallowed, Constants.FirstSortChar + "03");

            lsbStandardFilter.ItemAdd(leere);
            lsbStandardFilter.ItemAdd(nichtleere);

            lsbStandardFilter.ItemAdd(ItemOf("aus der Zwischenablage", "clipboard", QuickImage.Get(ImageCode.Clipboard, 17), _column.FilterOptions.HasFlag(FilterOptions.ExtendedFilterEnabled), Constants.FirstSortChar + "05"));
            lsbStandardFilter.ItemAdd(ItemOf("NICHT in der Zwischenablage", "nichtclipboard", QuickImage.Get("Clipboard|17||1"), _column.FilterOptions.HasFlag(FilterOptions.ExtendedFilterEnabled) && !_column.MultiLine && string.IsNullOrEmpty(tmp), Constants.FirstSortChar + "06"));
            lsbStandardFilter.ItemAdd(ItemOf("mehrfache UND-Auswahl aktivieren", "ModusMultiUnd", QuickImage.Get(ImageCode.PlusZeichen, 17, Color.Blue, Color.Transparent), _column.MultiLine && _column.FilterOptions.HasFlag(FilterOptions.ExtendedFilterEnabled) && nochOk, Constants.FirstSortChar + "07"));
            lsbStandardFilter.ItemAdd(ItemOf("mehrfache ODER-Auswahl aktivieren", "ModusMultiOder", QuickImage.Get(ImageCode.PlusZeichen, 17), _column.FilterOptions.HasFlag(FilterOptions.ExtendedFilterEnabled) && nochOk, Constants.FirstSortChar + "08"));
            lsbStandardFilter.ItemAdd(ItemOf("negativ Auswahl aktivieren", "ModusNegativ", QuickImage.Get(ImageCode.MinusZeichen, 17), !_column.MultiLine && _column.FilterOptions.HasFlag(FilterOptions.ExtendedFilterEnabled) && nochOk, Constants.FirstSortChar + "09"));
            lsbStandardFilter.ItemAdd(ItemOf("Einzigartige Einträge", "Einzigartig", QuickImage.Get(ImageCode.Eins, 17), _column.FilterOptions.HasFlag(FilterOptions.ExtendedFilterEnabled) && listFilterString.Count < 1000, Constants.FirstSortChar + "10"));
            lsbStandardFilter.ItemAdd(ItemOf("Nicht Einzigartige Einträge", "NichtEinzigartig", QuickImage.Get("Eins|17||1"), _column.FilterOptions.HasFlag(FilterOptions.ExtendedFilterEnabled), Constants.FirstSortChar + "11"));
            //lsbStandardFilter.ItemAdd(Add("Vergleiche mit anderer Spalte", "Spaltenvergleich", QuickImage.Get(ImageCode.Spalte, 17), lColumn.FilterOptions.HasFlag(FilterOptions.ExtendedFilterEnabled) && filter[_column.Database.Column.First] == null, Constants.FirstSortChar + "12"));
        }

        #endregion

        Width = Math.Max(lsbFilterItems.Right + (Skin.PaddingSmal * 2), Width);
        Height = lsbFilterItems.Bottom + Skin.PaddingSmal;

        #region Wenn ein Filter übergeben wurde, die Einträge markieren

        if (fc?[_column] is { IsDisposed: false } myFilter) {
            if (myFilter.FilterType.HasFlag(FilterType.Istgleich)) {
                if (myFilter.SearchValue.Count == 0 || (myFilter.SearchValue.Count == 1 && string.IsNullOrEmpty(myFilter.SearchValue[0]))) {
                    // Filter Leere anzeigen
                    if (leere != null) { lsbStandardFilter.Check(leere); }
                } else {
                    // Items des Istgleich-Filters anzeigen
                    lsbFilterItems.Check(myFilter.SearchValue);
                }
            } else if (myFilter.FilterType.HasFlag(FilterType.Instr)) {
                // Textfiler anzeigen
                txbEingabe.Text = myFilter.SearchValue[0];
            } else if (Convert.ToBoolean((int)myFilter.FilterType & 2)) {
                // Filter Nichtleere
                if (myFilter.SearchValue.Count == 1 && string.IsNullOrEmpty(myFilter.SearchValue[0]) && nichtleere != null) {
                    lsbStandardFilter.Check(nichtleere);
                }
            }
        }

        #endregion

        if (nochOk) {
            if (_column.FilterOptions == FilterOptions.Enabled_OnlyAndAllowed) { ChangeToMultiUnd(); }
            if (_column.FilterOptions == FilterOptions.Enabled_OnlyOrAllowed) { ChangeToMultiOder(); }
        }
    }

    protected override void OnLostFocus(System.EventArgs e) {
        base.OnLostFocus(e);
        Something_LostFocus(this, e);
    }

    private void butFertig_Click(object sender, System.EventArgs e) {
        var searchValue = lsbFilterItems.Checked;

        if (searchValue.Count == 0) {
            CloseAndDispose("FilterDelete", null);
            return;
        }
        if (_multiAuswahlOder) {
            CloseAndDispose("Filter", new FilterItem(_column, FilterType.Istgleich_ODER_GroßKleinEgal, searchValue));
            return;
        }
        if (_multiAuswahlUnd) {
            CloseAndDispose("Filter", new FilterItem(_column, FilterType.Istgleich_UND_GroßKleinEgal, searchValue));
            return;
        }
        Develop.DebugPrint("Filter Fehler!");
        CloseAndDispose("FilterDelete", null);
    }

    private void ChangeDesign() {
        lsbStandardFilter.Visible = false;
        capWas.Visible = false;
        txbEingabe.Visible = false;
        txbEingabe.Text = string.Empty;
        capInfo.Visible = true;
        butFertig.Visible = true;
    }

    private void ChangeToMultiOder() {
        var f = Skin.GetBlueFont(Design.Caption, States.Standard);
        _multiAuswahlOder = true;
        capInfo.Text = LanguageTool.DoTranslate("<fontsize=15><b><u>ODER-Filterung:</u></b><fontsize=" + f.Size + "><br><br>Wählen sie Einträge, von denen <b>einer</b> zutreffen muss:");
        ChangeDesign();
    }

    private void ChangeToMultiUnd() {
        _multiAuswahlUnd = true;
        var f = Skin.GetBlueFont(Design.Caption, States.Standard);
        capInfo.Text = LanguageTool.DoTranslate("<fontsize=15><b><u>UND-Filterung:</u></b><fontsize=" + f.Size + "><br><br>Wählen sie welche Einträge <b>alle</b> zutreffen müssen:");
        ChangeDesign();
    }

    private void CloseAndDispose(string command, FilterItem? newFilter) {
        if (IsClosed || IsDisposed) { return; }
        Close();
        OnFilterCommand(new FilterCommandEventArgs(command, _column, newFilter));
    }

    private void FiltItems_ItemClicked(object sender, AbstractListItemEventArgs e) {
        if (_multiAuswahlUnd || _multiAuswahlOder) { return; }

        var doJoker = !string.IsNullOrEmpty(_column.AutoFilterJoker);
        if (_negativAuswahl) { doJoker = false; }
        List<string> l = [e.Item.KeyName];
        if (doJoker) { l.AddIfNotExists(_column.AutoFilterJoker); }
        if (_negativAuswahl) {
            // Nur ohne Multirow
            CloseAndDispose("Filter", new FilterItem(_column, FilterType.Ungleich_MultiRowIgnorieren_GroßKleinEgal, l));
        } else {
            CloseAndDispose("Filter", new FilterItem(_column, FilterType.Istgleich_ODER_GroßKleinEgal, l));
        }
    }

    private void OnFilterCommand(FilterCommandEventArgs e) => FilterCommand?.Invoke(this, e);

    private void sFilter_ItemClicked(object sender, AbstractListItemEventArgs e) {
        switch (e.Item.KeyName.ToLowerInvariant()) {
            case "filterleere": {
                    CloseAndDispose("Filter", new FilterItem(_column, FilterType.Istgleich | FilterType.MultiRowIgnorieren, string.Empty));
                    break;
                }

            case "filternichtleere": {
                    CloseAndDispose("Filter", new FilterItem(_column, FilterType.Ungleich_MultiRowIgnorieren, string.Empty));
                    break;
                }

            case "clipboard": {
                    CloseAndDispose("DoClipboard", null);
                    break;
                }

            case "nichtclipboard": {
                    CloseAndDispose("DoNotClipboard", null);
                    break;
                }

            case "filterlöschen": {
                    CloseAndDispose("FilterDelete", null);
                    break;
                }

            case "modusmultiund": {
                    ChangeToMultiUnd();
                    break;
                }

            case "modusmultioder": {
                    ChangeToMultiOder();
                    break;
                }

            case "modusnegativ": {
                    _negativAuswahl = true;
                    _multiAuswahlUnd = false;
                    _multiAuswahlOder = false;
                    lsbStandardFilter["FilterLeere"]?.Disable();
                    lsbStandardFilter["FilterNichtLeere"]?.Disable();
                    lsbStandardFilter["Clipboard"]?.Disable();
                    lsbStandardFilter["ModusMultiUnd"]?.Disable();
                    lsbStandardFilter["ModusMultiOder"]?.Disable();
                    break;
                }

            case "einzigartig": {
                    CloseAndDispose("DoEinzigartig", null);
                    break;
                }

            case "nichteinzigartig": {
                    CloseAndDispose("DoNichtEinzigartig", null);
                    break;
                }

            //case "spaltenvergleich": {
            //        CloseAndDispose("DoSpaltenvergleich", null);
            //        break;
            //    }
            default: {
                    Develop.DebugPrint("Unbekannter Filter: " + e.Item.KeyName);
                    break;
                }
        }
    }

    private void Something_LostFocus(object sender, System.EventArgs e) {
        if (IsClosed) { return; }
        if (txbEingabe.Focused) { return; }
        if (Focused) { return; }
        if (lsbFilterItems.Focused()) { return; }
        if (lsbStandardFilter.Focused()) { return; }
        if (butFertig.Focused) { return; }
        CloseAndDispose(string.Empty, null);
    }

    private void Timer1_Tick(object sender, System.EventArgs e) {
        BringToFront();
        if (Timer1x.Interval < 5000) {
            Timer1x.Interval = 5000;
            if (txbEingabe.Enabled && txbEingabe is { Visible: true, Focused: false }) { _ = txbEingabe.Focus(); }
        }
    }

    private void TXTBox_Enter(object sender, System.EventArgs e) {
        if (string.IsNullOrEmpty(txbEingabe.Text)) {
            CloseAndDispose("FilterDelete", null);
            return;
        }

        if (_column.Function == ColumnFunction.Split_Medium) {
            CloseAndDispose("Filter", new FilterItem(_column, FilterType.Istgleich_GroßKleinEgal, txbEingabe.Text));
            return;
        }

        if (_column.SortType is SortierTyp.ZahlenwertFloat or SortierTyp.ZahlenwertInt) {
            if (txbEingabe.Text.Contains("-")) {
                var tmp = txbEingabe.Text.Replace(" ", string.Empty);
                var l = MathFormulaParser.LastMinusIndex(tmp);
                if (l > 0 && l < tmp.Length - 1) {
                    var z1 = tmp.Substring(0, l);
                    var z2 = tmp.Substring(l + 1);
                    if (z1.IsDouble() && z2.IsDouble()) {
                        var zd1 = DoubleParse(z1);
                        var zd2 = DoubleParse(z2);
                        if (zd2 < zd1) {
                            Generic.Swap(ref zd1, ref zd2);
                        }
                        CloseAndDispose("Filter", new FilterItem(_column, zd1, zd2));
                        return;
                    }
                }
            }
        }
        CloseAndDispose("Filter", new FilterItem(_column, FilterType.Instr | FilterType.GroßKleinEgal, txbEingabe.Text));
    }

    #endregion
}