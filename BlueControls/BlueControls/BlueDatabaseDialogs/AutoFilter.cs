#region BlueElements - a collection of useful tools, database and controls
// Authors: 
// Christian Peter
// 
// Copyright (c) 2020 Christian Peter
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
#endregion

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

namespace BlueControls.BlueDatabaseDialogs {

    public partial class AutoFilter : FloatingForm //System.Windows.Forms.UserControl //
    {



        public AutoFilter(ColumnItem vColumn, FilterCollection vFilter) : base(enDesign.Form_AutoFilter) {
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


            Column = vColumn;

            GenerateAll(vFilter);


        }


        public void GenerateAll(FilterCollection filter) {

            var nochOk = true;

            var List_FilterString = Column.Autofilter_ItemList(filter);


            var F = Skin.GetBlueFont(enDesign.Table_Cell, enStates.Standard);

            Width = Math.Max(txbEingabe.Width + Skin.Padding * 2, Table.tmpColumnContentWidth(null, Column, F, 16));

            lsbFilterItems.Item.Clear();
            lsbFilterItems.CheckBehavior = enCheckBehavior.MultiSelection;

            if (List_FilterString.Count < 400) {
                lsbFilterItems.Item.AddRange(List_FilterString, Column, enShortenStyle.Replaced, Column.BildTextVerhalten);
                lsbFilterItems.Item.Sort(); // Wichtig, dieser Sort kümmert sich, dass das Format (z. B.  Zahlen) berücksichtigt wird
            }
            else {
                lsbFilterItems.Item.Add("Zu viele Einträge", "x", enImageCode.Kreuz, false);
                nochOk = false;
            }


            var PrefSize = lsbFilterItems.Item.CalculateColumnAndSize();
            lsbFilterItems.Anchor = System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Top;
            lsbFilterItems.Width = Math.Min(Table.tmpColumnContentWidth(null, Column, F, 16), Width - Skin.PaddingSmal * 2);

            lsbFilterItems.Width = Math.Max(lsbFilterItems.Width, PrefSize.Width);
            lsbFilterItems.Height = Math.Max(lsbFilterItems.Height, PrefSize.Height);

            lsbFilterItems.Width = Math.Max(lsbFilterItems.Width, Width - Skin.PaddingSmal * 2);

            lsbFilterItems.Height = Math.Min(lsbFilterItems.Height, 560);

            if (Column.FilterOptions != enFilterOptions.Enabled_OnlyAndAllowed && Column.FilterOptions != enFilterOptions.Enabled_OnlyOrAllowed) {

                txbEingabe.Enabled = Column.FilterOptions.HasFlag(enFilterOptions.TextFilterEnabled);
                capWas.Enabled = Column.FilterOptions.HasFlag(enFilterOptions.TextFilterEnabled);

                if (Column.Format.IsZahl()) {
                    capWas.Text = "...oder von-bis:";
                }


                lsbStandardFilter.Item.Clear();
                if (filter != null) {
                    lsbStandardFilter.Item.Add("Filter löschen", "filterlöschen", QuickImage.Get("Trichter|16||1"), filter[Column] != null, Constants.FirstSortChar + "01");
                }
                else {
                    lsbStandardFilter.Item.Add("Filter löschen", "filterlöschen", QuickImage.Get("Trichter|16||1"), false, Constants.FirstSortChar + "01");
                }


                var tmp = CellItem.ValueReadable(Column, string.Empty, enShortenStyle.Replaced, enBildTextVerhalten.Nur_Text, true);


                if (string.IsNullOrEmpty(tmp)) {
                    lsbStandardFilter.Item.Add("leere", "filterleere", QuickImage.Get("TasteABC|20|16|1"), true, Constants.FirstSortChar + "02");
                    lsbStandardFilter.Item.Add("nicht leere", "filternichtleere", QuickImage.Get("TasteABC|20|16"), true, Constants.FirstSortChar + "03");
                }
                else {
                    lsbStandardFilter.Item.Add(tmp + " (= leere)", "filterleere", QuickImage.Get("TasteABC|20|16|1"), true, Constants.FirstSortChar + "02");
                    lsbStandardFilter.Item.Add("nicht leere", "filternichtleere", QuickImage.Get("TasteABC|20|16"), false, Constants.FirstSortChar + "03");
                }






                lsbStandardFilter.Item.Add("aus der Zwischenablage", "clipboard", QuickImage.Get(enImageCode.Clipboard, 17), Column.FilterOptions.HasFlag(enFilterOptions.ExtendedFilterEnabled), Constants.FirstSortChar + "05");
                lsbStandardFilter.Item.Add("NICHT in der Zwischenablage", "nichtclipboard", QuickImage.Get("Clipboard|17||1"), Column.FilterOptions.HasFlag(enFilterOptions.ExtendedFilterEnabled) && !Column.MultiLine && string.IsNullOrEmpty(tmp), Constants.FirstSortChar + "06");
                lsbStandardFilter.Item.Add("mehrfache UND-Auswahl aktivieren", "ModusMultiUnd", QuickImage.Get(enImageCode.PlusZeichen, 17, "0000FF", ""), Column.MultiLine && Column.FilterOptions.HasFlag(enFilterOptions.ExtendedFilterEnabled) && nochOk, Constants.FirstSortChar + "07");
                lsbStandardFilter.Item.Add("mehrfache ODER-Auswahl aktivieren", "ModusMultiOder", QuickImage.Get(enImageCode.PlusZeichen, 17), Column.FilterOptions.HasFlag(enFilterOptions.ExtendedFilterEnabled) && nochOk, Constants.FirstSortChar + "08");
                lsbStandardFilter.Item.Add("negativ Auswahl aktivieren", "ModusNegativ", QuickImage.Get(enImageCode.MinusZeichen, 17), !Column.MultiLine && Column.FilterOptions.HasFlag(enFilterOptions.ExtendedFilterEnabled) && nochOk, Constants.FirstSortChar + "09");
                lsbStandardFilter.Item.Add("Einzigartige Einträge", "Einzigartig", QuickImage.Get(enImageCode.Eins, 17), Column.FilterOptions.HasFlag(enFilterOptions.ExtendedFilterEnabled), Constants.FirstSortChar + "10");
                lsbStandardFilter.Item.Add("Nicht Einzigartige Einträge", "NichtEinzigartig", QuickImage.Get("Eins|17||1"), Column.FilterOptions.HasFlag(enFilterOptions.ExtendedFilterEnabled), Constants.FirstSortChar + "11");
                lsbStandardFilter.Item.Add("Vergleiche mit anderer Spalte", "Spaltenvergleich", QuickImage.Get(enImageCode.Spalte, 17), Column.FilterOptions.HasFlag(enFilterOptions.ExtendedFilterEnabled) && filter[Column.Database.Column[0]] == null, Constants.FirstSortChar + "12");


            }

            Width = Math.Max(lsbFilterItems.Right + Skin.PaddingSmal * 2, Width);
            Height = lsbFilterItems.Bottom + Skin.PaddingSmal;


            if (filter != null) {

                foreach (var Thisfilter in filter) {
                    if (Thisfilter != null && Thisfilter.FilterType != enFilterType.KeinFilter) {
                        if (Thisfilter.Column == Column) {


                            if (Thisfilter.FilterType.HasFlag(enFilterType.Istgleich)) {
                                foreach (var ThisValue in Thisfilter.SearchValue) {
                                    if (lsbFilterItems.Item[ThisValue] != null) {
                                        lsbFilterItems.Item[ThisValue].Checked = true;
                                    }
                                    else if (string.IsNullOrEmpty(ThisValue)) {
                                        lsbStandardFilter.Item["filterleere"].Checked = true;
                                    }

                                }

                            }
                            else if (Thisfilter.FilterType.HasFlag(enFilterType.Instr)) {
                                txbEingabe.Text = Thisfilter.SearchValue[0];


                            }
                            else if (Convert.ToBoolean((int)Thisfilter.FilterType & 2)) {
                                if (Thisfilter.SearchValue.Count == 1 && string.IsNullOrEmpty(Thisfilter.SearchValue[0])) {
                                    lsbStandardFilter.Item["filternichtleere"].Checked = true;
                                }
                            }

                        }
                    }
                }
            }


            if (nochOk) {
                if (Column.FilterOptions == enFilterOptions.Enabled_OnlyAndAllowed) { ChangeToMultiUnd(); }
                if (Column.FilterOptions == enFilterOptions.Enabled_OnlyOrAllowed) { ChangeToMultiOder(); }
            }

        }




        #region  Variablen 

        private readonly ColumnItem Column;

        private bool MultiAuswahlUND;
        private bool MultiAuswahlODER;
        private bool NegativAuswahl;

        #endregion


        #region  Events 
        public event System.EventHandler<FilterComandEventArgs> FilterComand;

        #endregion




        private void CloseAndDispose(string Comand, FilterItem NewFilter) {
            if (IsClosed) { return; }
            Close();
            OnFilterComand(new FilterComandEventArgs(Comand, Column, NewFilter));

        }

        private void OnFilterComand(FilterComandEventArgs e) {
            FilterComand?.Invoke(this, e);
        }

        private void Timer1_Tick(object sender, System.EventArgs e) {
            BringToFront();

            if (Timer1x.Interval < 5000) {
                Timer1x.Interval = 5000;
                if (txbEingabe.Enabled && txbEingabe.Visible) {
                    if (!txbEingabe.Focused()) { txbEingabe.Focus(); }
                }
            }

        }

        private void FiltItems_ItemClicked(object sender, BasicListItemEventArgs e) {

            if (MultiAuswahlUND || MultiAuswahlODER) { return; }

            var DoJoker = true;

            if (string.IsNullOrEmpty(Column.AutoFilterJoker)) { DoJoker = false; }
            if (NegativAuswahl) { DoJoker = false; }


            var l = new List<string>
                {
                    e.Item.Internal
                };


            if (DoJoker) { l.Add(Column.AutoFilterJoker); }


            if (NegativAuswahl) {
                // Nur Ohne Multirow
                CloseAndDispose("Filter", new FilterItem(Column, enFilterType.Ungleich_MultiRowIgnorieren_GroßKleinEgal, l));
            }
            else {
                CloseAndDispose("Filter", new FilterItem(Column, enFilterType.Istgleich_ODER_GroßKleinEgal, l));
            }

        }

        protected override void OnLostFocus(System.EventArgs e) {
            base.OnLostFocus(e);
            Something_LostFocus(this, e);
        }

        private void Something_LostFocus(object sender, System.EventArgs e) {

            if (IsClosed) { return; }

            if (txbEingabe.Focused()) { return; }
            if (Focused) { return; }
            if (lsbFilterItems.Focused()) { return; }
            if (lsbStandardFilter.Focused()) { return; }
            if (butFertig.Focused) { return; }

            CloseAndDispose(string.Empty, null);
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


            if (Column.Format.IsZahl()) {
                if (txbEingabe.Text.Contains("-")) {
                    var tmp = txbEingabe.Text.Replace(" ", "");
                    var l = modErgebnis.LastMinusIndex(tmp);
                    if (l > 0 && l < tmp.Length - 1) {

                        var Z1 = tmp.Substring(0, l);
                        var Z2 = tmp.Substring(l + 1);

                        if (Z1.IsDouble() && Z2.IsDouble()) {

                            var Zd1 = double.Parse(Z1);
                            var Zd2 = double.Parse(Z2);
                            if (Zd2 < Zd1) {
                                modAllgemein.Swap(ref Zd1, ref Zd2);
                            }
                            CloseAndDispose("Filter", new FilterItem(Column, enFilterType.Berechne | enFilterType.UND, "BTW(VALUE, " + Zd1.ToString().Replace(",", ".") + "," + Zd2.ToString().Replace(",", ".") + ")"));
                            return;
                        }
                    }
                }
            }

            CloseAndDispose("Filter", new FilterItem(Column, enFilterType.Instr | enFilterType.GroßKleinEgal, txbEingabe.Text));
        }

        private void sFilter_ItemClicked(object sender, BasicListItemEventArgs e) {


            switch (e.Item.Internal.ToLower()) {
                case "filterleere": {
                        CloseAndDispose("Filter", new FilterItem(Column, enFilterType.Istgleich | enFilterType.MultiRowIgnorieren, ""));
                        break;
                    }
                case "filternichtleere": {
                        CloseAndDispose("Filter", new FilterItem(Column, enFilterType.Ungleich_MultiRowIgnorieren, ""));
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
                        NegativAuswahl = true;
                        MultiAuswahlUND = false;
                        MultiAuswahlODER = false;
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

        private void ChangeToMultiOder() {
            var F = Skin.GetBlueFont(enDesign.Caption, enStates.Standard);
            MultiAuswahlODER = true;
            capInfo.Text = LanguageTool.DoTranslate("<fontsize=15><b><u>ODER-Filterung:</u></b><fontsize=" + F.FontSize.ToString() + "><br><br>Wählen sie Einträge, von denen <b>einer</b> zutreffen muss:");

            ChangeDesign();



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

        private void ChangeToMultiUnd() {
            MultiAuswahlUND = true;

            var F = Skin.GetBlueFont(enDesign.Caption, enStates.Standard);

            capInfo.Text = LanguageTool.DoTranslate("<fontsize=15><b><u>UND-Filterung:</u></b><fontsize=" + F.FontSize.ToString() + "><br><br>Wählen sie welche Einträge <b>alle</b> zutreffen müssen:");
            ChangeDesign();


        }

        private void butFertig_Click(object sender, System.EventArgs e) {


            var _SearchValue = lsbFilterItems.Item.Checked().ToListOfString();

            if (_SearchValue.Count == 0) {
                CloseAndDispose("FilterDelete", null);
                return;
            }

            if (MultiAuswahlODER) {
                CloseAndDispose("Filter", new FilterItem(Column, enFilterType.Istgleich_ODER_GroßKleinEgal, _SearchValue));
                return;
            }



            if (MultiAuswahlUND) {
                CloseAndDispose("Filter", new FilterItem(Column, enFilterType.Istgleich_UND_GroßKleinEgal, _SearchValue));
                return;
            }

            Develop.DebugPrint("Filter Fehler!");
            CloseAndDispose("FilterDelete", null);

        }
    }
}
