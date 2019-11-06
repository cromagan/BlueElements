#region BlueElements - a collection of useful tools, database and controls
// Authors: 
// Christian Peter
// 
// Copyright (c) 2019 Christian Peter
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

using System;
using System.Collections.Generic;
using System.Drawing;
using BlueBasics;
using BlueBasics.Enums;
using BlueControls.Controls;
using BlueControls.EventArgs;
using BlueControls.ItemCollection;
using BlueDatabase;
using BlueControls.Enums;
using BlueControls.Forms;
using BlueDatabase.Enums;

namespace BlueControls.BlueDatabaseDialogs
{

    public partial class AutoFilter : FloatingForm
    {



        public AutoFilter(ColumnItem vColumn, FilterCollection vFilter) : base(null)
        {
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


        public void GenerateAll(FilterCollection vFilter)
        {

            var nochOk = true;

            var List_FilterString = Column.Autofilter_ItemList(vFilter);


            var F = Skin.GetBlueFont(enDesign.Table_Cell, enStates.Standard);

            Width = Math.Max(TXTBox.Width + Skin.Padding * 2, Table.tmpColumnContentWidth(Column, F, 16));

            FiltItems.Item.Clear();
            FiltItems.CheckBehavior = enCheckBehavior.MultiSelection;

            if (List_FilterString.Count < 400)
            {
                FiltItems.Item.AddRange(List_FilterString, Column, enShortenStyle.Both);
                FiltItems.Item.Sort(); // Wichtig, dieser Sort kümmert sich, dass das Format (z. B.  Zahlen) berücksichtigt wird
            }
            else
            {
                FiltItems.Item.Add(new TextListItem("x", "Zu viele Einträge", enImageCode.Kreuz, false));
                nochOk = false;
            }

            FiltItems.Item.ComputeAllItemPositions(new Size(10, 10), true, false, enBlueListBoxAppearance.Autofilter, null, null);
            var PrefSize = FiltItems.Item.MaximumBounds();
            FiltItems.Anchor = System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Top;
            FiltItems.Width = Math.Min(Table.tmpColumnContentWidth(Column, F, 16), Width - Skin.PaddingSmal * 2);

            FiltItems.Width = Math.Max(FiltItems.Width, PrefSize.Width);
            FiltItems.Height = Math.Max(FiltItems.Height, PrefSize.Height);

            FiltItems.Width = Math.Max(FiltItems.Width, Width - Skin.PaddingSmal * 2);

            FiltItems.Height = Math.Min(FiltItems.Height, 560);


            TXTBox.Enabled = Column.AutofilterTextFilterErlaubt;
            Was.Enabled = Column.AutofilterTextFilterErlaubt;

            if (Column.Format.IsZahl())
            {
                Was.Text = "...oder von-bis:";
            }


            sFilter.Item.Clear();
            if (vFilter != null)
            {
                sFilter.Item.Add(new TextListItem("filterlöschen", "Filter löschen", QuickImage.Get("Trichter|16||1"), vFilter.Uses(Column), Constants.FirstSortChar + "01"));
            }
            else
            {
                sFilter.Item.Add(new TextListItem("filterlöschen", "Filter löschen", QuickImage.Get("Trichter|16||1"), false, Constants.FirstSortChar + "01"));
            }


            var tmp = CellItem.ValueReadable(Column, string.Empty, enShortenStyle.Replaced);


            if (string.IsNullOrEmpty(tmp))
            {
                sFilter.Item.Add(new TextListItem("filterleere", "leere", QuickImage.Get("TasteABC|20|16|1"), true, Constants.FirstSortChar + "02"));
                sFilter.Item.Add(new TextListItem("filternichtleere", "nicht leere", QuickImage.Get("TasteABC|20|16"), true, Constants.FirstSortChar + "03"));
            }
            else
            {
                sFilter.Item.Add(new TextListItem("filterleere", tmp + " (= leere)", QuickImage.Get("TasteABC|20|16|1"), true, Constants.FirstSortChar + "02"));
                sFilter.Item.Add(new TextListItem("filternichtleere", "nicht leere", QuickImage.Get("TasteABC|20|16"), false, Constants.FirstSortChar + "03"));
            }






            sFilter.Item.Add(new TextListItem("clipboard", "aus der Zwischenablage", QuickImage.Get(enImageCode.Clipboard, 17), Column.AutoFilterErweitertErlaubt, Constants.FirstSortChar + "05"));
            sFilter.Item.Add(new TextListItem("ModusMultiUnd", "mehrfache UND-Auswahl aktivieren", QuickImage.Get(enImageCode.PlusZeichen, 17, "0000FF", ""), Column.MultiLine && Column.AutoFilterErweitertErlaubt && nochOk, Constants.FirstSortChar + "06"));
            sFilter.Item.Add(new TextListItem("ModusMultiOder", "mehrfache ODER-Auswahl aktivieren", QuickImage.Get(enImageCode.PlusZeichen, 17), Column.AutoFilterErweitertErlaubt && nochOk, Constants.FirstSortChar + "07"));
            sFilter.Item.Add(new TextListItem("ModusNegativ", "negativ Auswahl aktivieren", QuickImage.Get(enImageCode.MinusZeichen, 17), !Column.MultiLine && Column.AutoFilterErweitertErlaubt && nochOk, Constants.FirstSortChar + "08"));
            sFilter.Item.Add(new TextListItem("Einzigartig", "Einzigartige Einträge", QuickImage.Get(enImageCode.Eins, 17), Column.AutoFilterErweitertErlaubt, Constants.FirstSortChar + "09"));
            sFilter.Item.Add(new TextListItem("NichtEinzigartig", "Nicht Einzigartige Einträge", QuickImage.Get("Eins|17||1"), Column.AutoFilterErweitertErlaubt, Constants.FirstSortChar + "10"));




            Width = Math.Max(FiltItems.Right + Skin.PaddingSmal * 2, Width);
            Height = FiltItems.Bottom + Skin.PaddingSmal;


            if (vFilter != null)
            {

                foreach (var Thisfilter in vFilter)
                {
                    if (Thisfilter != null && Thisfilter.FilterType != enFilterType.KeinFilter)
                    {
                        if (Thisfilter.Column == Column)
                        {


                            if (Convert.ToBoolean(Thisfilter.FilterType & enFilterType.Istgleich))
                            {
                                foreach (var ThisValue in Thisfilter.SearchValue)
                                {
                                    if (FiltItems.Item[ThisValue] != null)
                                    {
                                        FiltItems.Item[ThisValue].Checked = true;
                                    }
                                    else if (string.IsNullOrEmpty(ThisValue))
                                    {
                                        sFilter.Item["filterleere"].Checked = true;
                                    }

                                }

                            }
                            else if (Convert.ToBoolean(Thisfilter.FilterType & enFilterType.Instr))
                            {
                                TXTBox.Text = Thisfilter.SearchValue[0];


                            }
                            else if (Convert.ToBoolean((int)Thisfilter.FilterType & 2))
                            {
                                if (Thisfilter.SearchValue.Count == 1 && string.IsNullOrEmpty(Thisfilter.SearchValue[0]))
                                {
                                    sFilter.Item["filternichtleere"].Checked = true;
                                }
                            }

                        }
                    }
                }
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




        private void CloseAndDispose(string Comand, FilterItem NewFilter)
        {
            if (IsClosed) { return; }
            Close();
            OnFilterComand(new FilterComandEventArgs(Comand, Column, NewFilter));

        }

        private void OnFilterComand(FilterComandEventArgs e)
        {
            FilterComand?.Invoke(this, e);
        }

        private void Timer1_Tick(object sender, System.EventArgs e)
        {
            BringToFront();

            if (Timer1x.Interval < 5000)
            {
                Timer1x.Interval = 5000;
                if (TXTBox.Enabled)
                {
                    if (!TXTBox.Focused()) { TXTBox.Focus(); }
                }
            }

        }

        private void FiltItems_ItemClicked(object sender, BasicListItemEventArgs e)
        {

            if (MultiAuswahlUND || MultiAuswahlODER) { return; }

            var DoJoker = true;

            if (string.IsNullOrEmpty(Column.AutoFilterJoker)) { DoJoker = false; }
            if (NegativAuswahl) { DoJoker = false; }


            var l = new List<string>
                {
                    e.Item.Internal()
                };


            if (DoJoker) { l.Add(Column.AutoFilterJoker); }


            if (NegativAuswahl)
            {
                // Nur Ohne Multirow
                CloseAndDispose("Filter", new FilterItem(Column, enFilterType.Ungleich_MultiRowIgnorieren_GroßKleinEgal, l));
            }
            else
            {
                CloseAndDispose("Filter", new FilterItem(Column, enFilterType.Istgleich_ODER_GroßKleinEgal, l));
            }

        }

        protected override void OnLostFocus(System.EventArgs e)
        {
            base.OnLostFocus(e);
            Something_LostFocus(this, e);
        }

        private void Something_LostFocus(object sender, System.EventArgs e)
        {

            if (IsClosed) { return; }

            if (TXTBox.Focused()) { return; }
            if (Focused) { return; }
            if (FiltItems.Focused()) { return; }
            if (sFilter.Focused()) { return; }

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

        private void TXTBox_Enter(object sender, System.EventArgs e)
        {
            if (string.IsNullOrEmpty(TXTBox.Text))
            {
                CloseAndDispose("FilterDelete", null);
                return;
            }


            if (Column.Format.IsZahl())
            {
                if (TXTBox.Text.Contains("-"))
                {
                    var tmp = TXTBox.Text.Replace(" ", "");
                    var l = modErgebnis.LastMinusIndex(tmp);
                    if (l > 0 && l < tmp.Length - 1)
                    {

                        var Z1 = tmp.Substring(0, l);
                        var Z2 = tmp.Substring(l + 1);

                        if (Z1.IsDouble() && Z2.IsDouble())
                        {

                            var Zd1 = double.Parse(Z1);
                            var Zd2 = double.Parse(Z2);
                            if (Zd2 < Zd1)
                            {
                                modAllgemein.Swap(ref Zd1, ref Zd2);
                            }
                            CloseAndDispose("Filter", new FilterItem(Column, enFilterType.Berechne | enFilterType.UND, "BTW(VALUE, " + Zd1 + "," + Zd2 + ")"));
                            return;
                        }
                    }
                }
            }

            CloseAndDispose("Filter", new FilterItem(Column, enFilterType.Instr | enFilterType.GroßKleinEgal, TXTBox.Text));
        }

        private void sFilter_ItemClicked(object sender, BasicListItemEventArgs e)
        {


            switch (e.Item.Internal().ToLower())
            {
                case "filterleere":
                    {
                        CloseAndDispose("Filter", new FilterItem(Column, enFilterType.Istgleich | enFilterType.MultiRowIgnorieren, ""));
                        break;
                    }
                case "filternichtleere":
                    {
                        CloseAndDispose("Filter", new FilterItem(Column, enFilterType.Ungleich_MultiRowIgnorieren, ""));
                        break;
                    }
                case "clipboard":
                    {

                        var nt = Convert.ToString(System.Windows.Forms.Clipboard.GetDataObject().GetData(System.Windows.Forms.DataFormats.Text));
                        nt = nt.RemoveChars(Constants.Char_NotFromClip);
                        nt = nt.TrimEnd("\r\n");
                        var _SearchValue = new List<string>(nt.SplitByCR()).SortedDistinctList();

                        if (_SearchValue.Count == 0)
                        {
                            CloseAndDispose("FilterDelete", null);
                        }
                        else
                        {
                            CloseAndDispose("Filter", new FilterItem(Column, enFilterType.IstGleich_ODER | enFilterType.GroßKleinEgal, _SearchValue));
                        }

                        break;
                    }
                case "filterlöschen":
                    {
                        CloseAndDispose("FilterDelete", null);
                        break;
                    }
                case "modusmultiund":
                    {
                        MultiAuswahlUND = true;
                        TXTBox.Text = "";
                        TXTBox.Enabled = false;

                        sFilter.Item.Remove("ModusMultiUnd");
                        sFilter.Item.Add(new TextListItem("ModusMultiUndFertig", "Und-Mehrfachauswahl fertig!", QuickImage.Get(enImageCode.Häkchen, 17), Column.AutoFilterErweitertErlaubt, Constants.FirstSortChar + "6"));

                        sFilter.Item["Clipboard"].Enabled = false;
                        sFilter.Item["ModusNegativ"].Enabled = false;
                        sFilter.Item["ModusMultiOder"].Enabled = false;
                        sFilter.Item["Einzigartig"].Enabled = false;
                        sFilter.Item["NichtEinzigartig"].Enabled = false;

                        break;
                    }
                case "modusmultioder":
                    {
                        MultiAuswahlUND = true;
                        TXTBox.Text = "";
                        TXTBox.Enabled = false;
                        sFilter.Item.Remove("ModusMultiOder");
                        sFilter.Item.Add(new TextListItem("ModusMultiOderFertig", "Oder-Mehrfachauswahl fertig!", QuickImage.Get(enImageCode.Häkchen, 17), Column.AutoFilterErweitertErlaubt, Constants.FirstSortChar + "7"));

                        sFilter.Item["Clipboard"].Enabled = false;
                        sFilter.Item["ModusNegativ"].Enabled = false;
                        sFilter.Item["ModusMultiUnd"].Enabled = false;
                        sFilter.Item["Einzigartig"].Enabled = false;
                        sFilter.Item["NichtEinzigartig"].Enabled = false;
                        // FiltItems.CheckBehavior = enCheckBehavior.MultiSelection ' eh schon
                        //MitJoker = False
                        //eMailInfo("Joker wurde automatisch entfernt", enImageCode.Stern)

                        break;
                    }
                case "modusmultioderfertig":
                    {
                        var _SearchValue = FiltItems.Item.Checked().ToListOfString();

                        if (_SearchValue.Count == 0)
                        {
                            CloseAndDispose("FilterDelete", null);
                        }
                        else
                        {
                            CloseAndDispose("Filter", new FilterItem(Column, enFilterType.Istgleich_ODER_GroßKleinEgal, _SearchValue));
                        }

                        break;
                    }
                case "modusmultiundfertig":
                    {
                        var _SearchValue = FiltItems.Item.Checked().ToListOfString();
                        if (_SearchValue.Count == 0)
                        {
                            CloseAndDispose("FilterDelete", null);
                        }
                        else
                        {
                            CloseAndDispose("Filter", new FilterItem(Column, enFilterType.Istgleich_UND_GroßKleinEgal, _SearchValue));
                        }



                        break;
                    }
                case "modusnegativ":
                    {
                        NegativAuswahl = true;
                        MultiAuswahlUND = false;
                        MultiAuswahlODER = false;
                        sFilter.Item["FilterLeere"].Enabled = false;
                        sFilter.Item["FilterNichtLeere"].Enabled = false;
                        sFilter.Item["Clipboard"].Enabled = false;
                        sFilter.Item["ModusMultiUnd"].Enabled = false;
                        sFilter.Item["ModusMultiOder"].Enabled = false;


                        break;
                    }
                case "einzigartig":
                    {
                        CloseAndDispose("DoEinzigartig", null);
                        break;
                    }
                case "nichteinzigartig":
                    {
                        CloseAndDispose("DoNichtEinzigartig", null);


                        break;
                    }
                default:
                    {
                        Develop.DebugPrint("Unbekannter Filter: " + e.Item.Internal());

                        break;
                    }
            }


        }

    }
}
