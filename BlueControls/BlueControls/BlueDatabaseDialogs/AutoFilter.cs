// Authors:
// Christian Peter
//
// Copyright (c) 2022 Christian Peter
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
using BlueControls.Controls;
using BlueControls.Enums;
using BlueControls.EventArgs;
using BlueControls.Forms;
using BlueDatabase;
using BlueDatabase.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using BlueControls.ItemCollection.ItemCollectionList;
using static BlueBasics.Converter;

namespace BlueControls.BlueDatabaseDialogs {

    public partial class AutoFilter : FloatingForm //System.Windows.Forms.UserControl //
    {
        #region Fields

        private readonly ColumnItem? _column;

        private bool _multiAuswahlOder;

        private bool _multiAuswahlUnd;

        private bool _negativAuswahl;

        #endregion

        #region Constructors

        public AutoFilter(ColumnItem? column, FilterCollection? filter, List<RowItem>? pinned) : base(Design.Form_AutoFilter) {
            // Dieser Aufruf ist für den Windows Form-Designer erforderlich.
            InitializeComponent();
            //Me.SetNotFocusable()
            SetStyle(System.Windows.Forms.ControlStyles.Selectable, false);
            SetStyle(System.Windows.Forms.ControlStyles.StandardClick, false);
            SetStyle(System.Windows.Forms.ControlStyles.StandardDoubleClick, false);
            // Initialisierungen nach dem Aufruf InitializeComponent() hinzufügen
            SetStyle(System.Windows.Forms.ControlStyles.ResizeRedraw, false);
            SetStyle(System.Windows.Forms.ControlStyles.SupportsTransparentBackColor, false);
            SetStyle(System.Windows.Forms.ControlStyles.Opaque, false);
            //The next 3 styles are allefor double buffering
            SetStyle(System.Windows.Forms.ControlStyles.DoubleBuffer, true);
            SetStyle(System.Windows.Forms.ControlStyles.AllPaintingInWmPaint, true);
            SetStyle(System.Windows.Forms.ControlStyles.UserPaint, true);
            _column = column;
            GenerateAll(filter, pinned);
        }

        #endregion

        #region Events

        public event EventHandler<FilterComandEventArgs> FilterComand;

        #endregion

        #region Methods

        public void GenerateAll(FilterCollection? filter, List<RowItem?> pinned) {
            var nochOk = true;
            var listFilterString = _column.Autofilter_ItemList(filter, pinned);
            var f = Skin.GetBlueFont(Design.Table_Cell, States.Standard);

            //ACHUNG:
            // Column ist für die Filter in dieser Datenbank zuständig
            // lColumn für das Aussehen und Verhalten des FilterDialogs

            ColumnItem? lColumn = null;
            if (_column.Format == enDataFormat.Verknüpfung_zu_anderer_Datenbank) {
                (lColumn, _, _) = CellCollection.LinkedCellData(_column, null, false, false);
            }
            if (lColumn == null) { lColumn = _column; }

            Width = Math.Max(txbEingabe.Width + (Skin.Padding * 2), Table.TmpColumnContentWidth(null, lColumn, f, 16));
            lsbFilterItems.Item.Clear();
            lsbFilterItems.CheckBehavior = CheckBehavior.MultiSelection;

            if (listFilterString.Count < 400) {
                lsbFilterItems.Item.AddRange(listFilterString, lColumn, ShortenStyle.Replaced, lColumn.BildTextVerhalten);
                lsbFilterItems.Item.Sort(); // Wichtig, dieser Sort kümmert sich, dass das Format (z. B.  Zahlen) berücksichtigt wird
            } else {
                lsbFilterItems.Item.Add("Zu viele Einträge", "x", enImageCode.Kreuz, false);
                nochOk = false;
            }

            var prefSize = lsbFilterItems.Item.CalculateColumnAndSize();
            lsbFilterItems.Anchor = System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Top;
            lsbFilterItems.Width = Math.Min(Table.TmpColumnContentWidth(null, lColumn, f, 16), Width - (Skin.PaddingSmal * 2));
            lsbFilterItems.Width = Math.Max(lsbFilterItems.Width, prefSize.Width);
            lsbFilterItems.Height = Math.Max(lsbFilterItems.Height, prefSize.Height);
            lsbFilterItems.Width = Math.Max(lsbFilterItems.Width, Width - (Skin.PaddingSmal * 2));
            lsbFilterItems.Height = Math.Min(lsbFilterItems.Height, 560);

            if (lColumn.FilterOptions is not FilterOptions.Enabled_OnlyAndAllowed and not FilterOptions.Enabled_OnlyOrAllowed) {
                txbEingabe.Enabled = lColumn.FilterOptions.HasFlag(FilterOptions.TextFilterEnabled);
                capWas.Enabled = lColumn.FilterOptions.HasFlag(FilterOptions.TextFilterEnabled);
                if (lColumn.SortType is enSortierTyp.ZahlenwertFloat or enSortierTyp.ZahlenwertInt) {
                    capWas.Text = "...oder von-bis:";
                }
                lsbStandardFilter.Item.Clear();
                if (filter != null) {
                    lsbStandardFilter.Item.Add("Filter löschen", "filterlöschen", QuickImage.Get("Trichter|16||1"), filter[_column] != null, Constants.FirstSortChar + "01");
                } else {
                    lsbStandardFilter.Item.Add("Filter löschen", "filterlöschen", QuickImage.Get("Trichter|16||1"), false, Constants.FirstSortChar + "01");
                }
                var tmp = CellItem.ValueReadable(lColumn, string.Empty, ShortenStyle.Replaced, BildTextVerhalten.Nur_Text, true);
                if (string.IsNullOrEmpty(tmp)) {
                    lsbStandardFilter.Item.Add("leere", "filterleere", QuickImage.Get("TasteABC|20|16|1"), true, Constants.FirstSortChar + "02");
                    lsbStandardFilter.Item.Add("nicht leere", "filternichtleere", QuickImage.Get("TasteABC|20|16"), true, Constants.FirstSortChar + "03");
                } else {
                    lsbStandardFilter.Item.Add(tmp + " (= leere)", "filterleere", QuickImage.Get("TasteABC|20|16|1"), true, Constants.FirstSortChar + "02");
                    lsbStandardFilter.Item.Add("nicht leere", "filternichtleere", QuickImage.Get("TasteABC|20|16"), false, Constants.FirstSortChar + "03");
                }
                lsbStandardFilter.Item.Add("aus der Zwischenablage", "clipboard", QuickImage.Get(enImageCode.Clipboard, 17), lColumn.FilterOptions.HasFlag(FilterOptions.ExtendedFilterEnabled), Constants.FirstSortChar + "05");
                lsbStandardFilter.Item.Add("NICHT in der Zwischenablage", "nichtclipboard", QuickImage.Get("Clipboard|17||1"), lColumn.FilterOptions.HasFlag(FilterOptions.ExtendedFilterEnabled) && !_column.MultiLine && string.IsNullOrEmpty(tmp), Constants.FirstSortChar + "06");
                lsbStandardFilter.Item.Add("mehrfache UND-Auswahl aktivieren", "ModusMultiUnd", QuickImage.Get(enImageCode.PlusZeichen, 17, "0000FF", ""), lColumn.MultiLine && lColumn.FilterOptions.HasFlag(FilterOptions.ExtendedFilterEnabled) && nochOk, Constants.FirstSortChar + "07");
                lsbStandardFilter.Item.Add("mehrfache ODER-Auswahl aktivieren", "ModusMultiOder", QuickImage.Get(enImageCode.PlusZeichen, 17), lColumn.FilterOptions.HasFlag(FilterOptions.ExtendedFilterEnabled) && nochOk, Constants.FirstSortChar + "08");
                lsbStandardFilter.Item.Add("negativ Auswahl aktivieren", "ModusNegativ", QuickImage.Get(enImageCode.MinusZeichen, 17), !lColumn.MultiLine && lColumn.FilterOptions.HasFlag(FilterOptions.ExtendedFilterEnabled) && nochOk, Constants.FirstSortChar + "09");
                lsbStandardFilter.Item.Add("Einzigartige Einträge", "Einzigartig", QuickImage.Get(enImageCode.Eins, 17), lColumn.FilterOptions.HasFlag(FilterOptions.ExtendedFilterEnabled), Constants.FirstSortChar + "10");
                lsbStandardFilter.Item.Add("Nicht Einzigartige Einträge", "NichtEinzigartig", QuickImage.Get("Eins|17||1"), lColumn.FilterOptions.HasFlag(FilterOptions.ExtendedFilterEnabled), Constants.FirstSortChar + "11");
                lsbStandardFilter.Item.Add("Vergleiche mit anderer Spalte", "Spaltenvergleich", QuickImage.Get(enImageCode.Spalte, 17), lColumn.FilterOptions.HasFlag(FilterOptions.ExtendedFilterEnabled) && filter[_column.Database.Column[0]] == null, Constants.FirstSortChar + "12");
            }
            Width = Math.Max(lsbFilterItems.Right + (Skin.PaddingSmal * 2), Width);
            Height = lsbFilterItems.Bottom + Skin.PaddingSmal;
            if (filter != null) {
                foreach (var thisfilter in filter.Where(thisfilter => thisfilter != null && thisfilter.FilterType != FilterType.KeinFilter).Where(thisfilter => thisfilter.Column == _column)) {
                    if (thisfilter.FilterType.HasFlag(FilterType.Istgleich)) {
                        foreach (var thisValue in thisfilter.SearchValue) {
                            if (lsbFilterItems.Item[thisValue] != null) {
                                lsbFilterItems.Item[thisValue].Checked = true;
                            } else if (string.IsNullOrEmpty(thisValue)) {
                                lsbStandardFilter.Item["filterleere"].Checked = true;
                            }
                        }
                    } else if (thisfilter.FilterType.HasFlag(FilterType.Instr)) {
                        txbEingabe.Text = thisfilter.SearchValue[0];
                    } else if (Convert.ToBoolean((int)thisfilter.FilterType & 2)) {
                        if (thisfilter.SearchValue.Count == 1 && string.IsNullOrEmpty(thisfilter.SearchValue[0])) {
                            lsbStandardFilter.Item["filternichtleere"].Checked = true;
                        }
                    }
                }
            }
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
            //Line.Visible = false;
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
                e.Item.Internal
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
            switch (e.Item.Internal.ToLower()) {
                case "filterleere": {
                        CloseAndDispose("Filter", new FilterItem(_column, FilterType.Istgleich | FilterType.MultiRowIgnorieren, ""));
                        break;
                    }

                case "filternichtleere": {
                        CloseAndDispose("Filter", new FilterItem(_column, FilterType.Ungleich_MultiRowIgnorieren, ""));
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
                        lsbStandardFilter.Item["FilterLeere"].Enabled = false;
                        lsbStandardFilter.Item["FilterNichtLeere"].Enabled = false;
                        lsbStandardFilter.Item["Clipboard"].Enabled = false;
                        lsbStandardFilter.Item["ModusMultiUnd"].Enabled = false;
                        lsbStandardFilter.Item["ModusMultiOder"].Enabled = false;
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

                case "spaltenvergleich": {
                        CloseAndDispose("DoSpaltenvergleich", null);
                        break;
                    }
                default: {
                        Develop.DebugPrint("Unbekannter Filter: " + e.Item.Internal);
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
                    if (!txbEingabe.Focused) { txbEingabe.Focus(); }
                }
            }
        }

        //private void OnMouseEnter(object sender, System.EventArgs e)
        //{
        //    IsMouseInControl();
        //    MouseMoved?.Invoke(this, System.EventArgs.Empty);
        //}
        //private void OnMouseLeave(object sender, System.EventArgs e)
        //{
        //    IsMouseInControl();
        //    MouseMoved?.Invoke(this, System.EventArgs.Empty);
        //}
        //private void OnMouseMove(object sender, System.Windows.Forms.MouseEventArgs e)
        //{
        //    IsMouseInControl();
        //    MouseMoved?.Invoke(this, System.EventArgs.Empty);
        //}
        private void TXTBox_Enter(object sender, System.EventArgs e) {
            if (string.IsNullOrEmpty(txbEingabe.Text)) {
                CloseAndDispose("FilterDelete", null);
                return;
            }
            if (_column.SortType is enSortierTyp.ZahlenwertFloat or enSortierTyp.ZahlenwertInt) {
                if (txbEingabe.Text.Contains("-")) {
                    var tmp = txbEingabe.Text.Replace(" ", "");
                    var l = Berechnung.LastMinusIndex(tmp);
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
}