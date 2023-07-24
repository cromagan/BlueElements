// Authors:
// Christian Peter
//
// Copyright (c) 2023 Christian Peter
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
using BlueControls.Controls;
using BlueControls.Enums;
using BlueControls.EventArgs;
using BlueControls.Forms;
using BlueControls.ItemCollection.ItemCollectionList;
using BlueDatabase;
using BlueDatabase.Enums;
using static BlueBasics.Converter;

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

    public AutoFilter(ColumnItem column, FilterCollection? filter, List<RowItem>? pinned) : base(Design.Form_AutoFilter) {
        // Dieser Aufruf ist für den Windows Form-Designer erforderlich.
        InitializeComponent();

        _column = column;
        GenerateAll(filter, pinned);
    }

    #endregion

    #region Events

    public event EventHandler<FilterComandEventArgs>? FilterComand;

    #endregion

    #region Methods

    public static List<string> Autofilter_ItemList(ColumnItem? column, FilterCollection? filter, List<RowItem>? pinned) {
        if (column == null || column.IsDisposed) { return new List<string>(); }

        if (filter == null || filter.Count < 0) { return column.Contents(); }
        FilterCollection tfilter = new(column.Database);
        foreach (var thisFilter in filter) {
            if (thisFilter != null && column != thisFilter.Column) {
                tfilter.Add(thisFilter);
            }
        }

        return column.Contents(tfilter, pinned);
    }

    public void GenerateAll(FilterCollection? filter, List<RowItem>? pinned) {
        var nochOk = true;
        var listFilterString = Autofilter_ItemList(_column, filter, pinned);
        var f = Skin.GetBlueFont(Design.Table_Cell, States.Standard);

        //ACHUNG:
        // Column ist für die Filter in dieser Datenbank zuständig
        // lColumn für das Aussehen und Verhalten des FilterDialogs

        ColumnItem? lColumn = null;
        if (_column.Format == DataFormat.Verknüpfung_zu_anderer_Datenbank) {
            (lColumn, _, _, _) = CellCollection.LinkedCellData(_column, null, false, false);
        }
        lColumn ??= _column;

        Width = Math.Max(txbEingabe.Width + (Skin.Padding * 2), lColumn.ContentWidth);
        lsbFilterItems.Item.Clear();
        lsbFilterItems.CheckBehavior = CheckBehavior.MultiSelection;

        if (listFilterString.Count < 400) {
            lsbFilterItems.Item.AddRange(listFilterString, lColumn, ShortenStyle.Replaced, lColumn.BehaviorOfImageAndText);
            //lsbFilterItems.Item.Sort(); // Wichtig, dieser Sort kümmert sich, dass das Format (z. B.  Zahlen) berücksichtigt wird
        } else {
            _ = lsbFilterItems.Item.Add("Zu viele Einträge", "x", ImageCode.Kreuz, false);
            nochOk = false;
        }

        var prefSize = lsbFilterItems.Item.CalculateColumnAndSize();
        lsbFilterItems.Anchor = AnchorStyles.Left | AnchorStyles.Top;
        lsbFilterItems.Width = Math.Min(lColumn.ContentWidth, Width - (Skin.PaddingSmal * 2));
        lsbFilterItems.Width = Math.Max(lsbFilterItems.Width, prefSize.Width);
        lsbFilterItems.Height = Math.Max(lsbFilterItems.Height, prefSize.Height);
        lsbFilterItems.Width = Math.Max(lsbFilterItems.Width, Width - (Skin.PaddingSmal * 2));
        lsbFilterItems.Height = Math.Min(lsbFilterItems.Height, 560);

        BasicListItem? leere = null;
        BasicListItem? nichtleere = null;

        #region die Besonderen Filter generieren

        if (lColumn.FilterOptions is not FilterOptions.Enabled_OnlyAndAllowed and not FilterOptions.Enabled_OnlyOrAllowed) {
            txbEingabe.Enabled = lColumn.FilterOptions.HasFlag(FilterOptions.TextFilterEnabled);
            capWas.Enabled = lColumn.FilterOptions.HasFlag(FilterOptions.TextFilterEnabled);
            if (lColumn.SortType is SortierTyp.ZahlenwertFloat or SortierTyp.ZahlenwertInt) {
                capWas.Text = "...oder von-bis:";
            }
            lsbStandardFilter.Item.Clear();
            if (filter != null) {
                _ = lsbStandardFilter.Item.Add("Filter löschen", "filterlöschen", QuickImage.Get("Trichter|16||1"), filter[_column] != null, Constants.FirstSortChar + "01");
            } else {
                _ = lsbStandardFilter.Item.Add("Filter löschen", "filterlöschen", QuickImage.Get("Trichter|16||1"), false, Constants.FirstSortChar + "01");
            }

            var tmp = CellItem.ValueReadable(lColumn, string.Empty, ShortenStyle.Replaced, BildTextVerhalten.Nur_Text, true);
            if (string.IsNullOrEmpty(tmp)) {
                leere = lsbStandardFilter.Item.Add("leere", "filterleere", QuickImage.Get("TasteABC|20|16|1"), true, Constants.FirstSortChar + "02");
                nichtleere = lsbStandardFilter.Item.Add("nicht leere", "filternichtleere", QuickImage.Get("TasteABC|20|16"), true, Constants.FirstSortChar + "03");
            } else {
                leere = lsbStandardFilter.Item.Add(tmp + " (= leere)", "filterleere", QuickImage.Get("TasteABC|20|16|1"), true, Constants.FirstSortChar + "02");
                nichtleere = lsbStandardFilter.Item.Add("nicht leere", "filternichtleere", QuickImage.Get("TasteABC|20|16"), false, Constants.FirstSortChar + "03");
            }
            _ = lsbStandardFilter.Item.Add("aus der Zwischenablage", "clipboard", QuickImage.Get(ImageCode.Clipboard, 17), lColumn.FilterOptions.HasFlag(FilterOptions.ExtendedFilterEnabled), Constants.FirstSortChar + "05");
            _ = lsbStandardFilter.Item.Add("NICHT in der Zwischenablage", "nichtclipboard", QuickImage.Get("Clipboard|17||1"), lColumn.FilterOptions.HasFlag(FilterOptions.ExtendedFilterEnabled) && !_column.MultiLine && string.IsNullOrEmpty(tmp), Constants.FirstSortChar + "06");
            _ = lsbStandardFilter.Item.Add("mehrfache UND-Auswahl aktivieren", "ModusMultiUnd", QuickImage.Get(ImageCode.PlusZeichen, 17, Color.Blue, Color.Transparent), lColumn.MultiLine && lColumn.FilterOptions.HasFlag(FilterOptions.ExtendedFilterEnabled) && nochOk, Constants.FirstSortChar + "07");
            _ = lsbStandardFilter.Item.Add("mehrfache ODER-Auswahl aktivieren", "ModusMultiOder", QuickImage.Get(ImageCode.PlusZeichen, 17), lColumn.FilterOptions.HasFlag(FilterOptions.ExtendedFilterEnabled) && nochOk, Constants.FirstSortChar + "08");
            _ = lsbStandardFilter.Item.Add("negativ Auswahl aktivieren", "ModusNegativ", QuickImage.Get(ImageCode.MinusZeichen, 17), !lColumn.MultiLine && lColumn.FilterOptions.HasFlag(FilterOptions.ExtendedFilterEnabled) && nochOk, Constants.FirstSortChar + "09");
            _ = lsbStandardFilter.Item.Add("Einzigartige Einträge", "Einzigartig", QuickImage.Get(ImageCode.Eins, 17), lColumn.FilterOptions.HasFlag(FilterOptions.ExtendedFilterEnabled), Constants.FirstSortChar + "10");
            _ = lsbStandardFilter.Item.Add("Nicht Einzigartige Einträge", "NichtEinzigartig", QuickImage.Get("Eins|17||1"), lColumn.FilterOptions.HasFlag(FilterOptions.ExtendedFilterEnabled), Constants.FirstSortChar + "11");
            //lsbStandardFilter.Item.Add("Vergleiche mit anderer Spalte", "Spaltenvergleich", QuickImage.Get(ImageCode.Spalte, 17), lColumn.FilterOptions.HasFlag(FilterOptions.ExtendedFilterEnabled) && filter[_column.Database.Column.First] == null, Constants.FirstSortChar + "12");
        }

        #endregion

        Width = Math.Max(lsbFilterItems.Right + (Skin.PaddingSmal * 2), Width);
        Height = lsbFilterItems.Bottom + Skin.PaddingSmal;

        #region Wenn ein Filter übergeben wurde, die Einträge markieren

        if (filter != null) {
            foreach (var thisfilter in filter) {
                if (thisfilter != null && thisfilter.FilterType != FilterType.KeinFilter) {
                    if (thisfilter.Column == _column) {
                        if (thisfilter.FilterType.HasFlag(FilterType.Istgleich)) {
                            foreach (var thisValue in thisfilter.SearchValue) {
                                if (lsbFilterItems.Item[thisValue] is BasicListItem bli) {
                                    bli.Checked = true;
                                } else if (string.IsNullOrEmpty(thisValue) && leere != null) {
                                    leere.Checked = true;
                                }
                            }
                        } else if (thisfilter.FilterType.HasFlag(FilterType.Instr)) {
                            txbEingabe.Text = thisfilter.SearchValue[0];
                        } else if (Convert.ToBoolean((int)thisfilter.FilterType & 2)) {
                            if (thisfilter.SearchValue.Count == 1 && string.IsNullOrEmpty(thisfilter.SearchValue[0]) && nichtleere != null) {
                                nichtleere.Checked = true;
                            }
                        }
                    }
                }
            }
        }

        #endregion

        if (nochOk) {
            if (lColumn.FilterOptions == FilterOptions.Enabled_OnlyAndAllowed) { ChangeToMultiUnd(); }
            if (lColumn.FilterOptions == FilterOptions.Enabled_OnlyOrAllowed) { ChangeToMultiOder(); }
        }
    }

    protected override void OnLostFocus(System.EventArgs e) {
        base.OnLostFocus(e);
        Something_LostFocus(this, e);
    }

    private void butFertig_Click(object sender, System.EventArgs e) {
        var searchValue = lsbFilterItems.Item.Checked().ToListOfString();
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
        capInfo.Text = LanguageTool.DoTranslate("<fontsize=15><b><u>ODER-Filterung:</u></b><fontsize=" + f.FontSize + "><br><br>Wählen sie Einträge, von denen <b>einer</b> zutreffen muss:");
        ChangeDesign();
    }

    private void ChangeToMultiUnd() {
        _multiAuswahlUnd = true;
        var f = Skin.GetBlueFont(Design.Caption, States.Standard);
        capInfo.Text = LanguageTool.DoTranslate("<fontsize=15><b><u>UND-Filterung:</u></b><fontsize=" + f.FontSize + "><br><br>Wählen sie welche Einträge <b>alle</b> zutreffen müssen:");
        ChangeDesign();
    }

    private void CloseAndDispose(string comand, FilterItem? newFilter) {
        if (IsClosed) { return; }
        Close();
        OnFilterComand(new FilterComandEventArgs(comand, _column, newFilter));
    }

    private void FiltItems_ItemClicked(object sender, BasicListItemEventArgs e) {
        if (_multiAuswahlUnd || _multiAuswahlOder) { return; }

        var doJoker = !string.IsNullOrEmpty(_column.AutoFilterJoker);
        if (_negativAuswahl) { doJoker = false; }
        List<string> l = new()
        {
            e.Item.KeyName
        };
        if (doJoker) { l.Add(_column.AutoFilterJoker); }
        if (_negativAuswahl) {
            // Nur Ohne Multirow
            CloseAndDispose("Filter", new FilterItem(_column, FilterType.Ungleich_MultiRowIgnorieren_GroßKleinEgal, l));
        } else {
            CloseAndDispose("Filter", new FilterItem(_column, FilterType.Istgleich_ODER_GroßKleinEgal, l));
        }
    }

    private void OnFilterComand(FilterComandEventArgs e) => FilterComand?.Invoke(this, e);

    private void sFilter_ItemClicked(object sender, BasicListItemEventArgs e) {
        switch (e.Item.KeyName.ToLower()) {
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
                    lsbStandardFilter.Item["FilterLeere"]?.Disable();
                    lsbStandardFilter.Item["FilterNichtLeere"]?.Disable();
                    lsbStandardFilter.Item["Clipboard"]?.Disable();
                    lsbStandardFilter.Item["ModusMultiUnd"]?.Disable();
                    lsbStandardFilter.Item["ModusMultiOder"]?.Disable();
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
            if (txbEingabe.Enabled && txbEingabe.Visible) {
                if (!txbEingabe.Focused) { _ = txbEingabe.Focus(); }
            }
        }
    }

    private void TXTBox_Enter(object sender, System.EventArgs e) {
        if (string.IsNullOrEmpty(txbEingabe.Text)) {
            CloseAndDispose("FilterDelete", null);
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
                        CloseAndDispose("Filter", new FilterItem(_column, FilterType.Between | FilterType.UND, zd1.ToString(Constants.Format_Float9) + "|" + zd2.ToString(Constants.Format_Float9)));
                        return;
                    }
                }
            }
        }
        CloseAndDispose("Filter", new FilterItem(_column, FilterType.Instr | FilterType.GroßKleinEgal, txbEingabe.Text));
    }

    #endregion
}