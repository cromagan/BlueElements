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

using System.Drawing;
using BlueControls.ItemCollection;
using BlueDatabase;
using BlueDatabase.Enums;
using BlueControls.Designer_Support;
using System.ComponentModel;
using System.Collections.Generic;
using BlueControls.Enums;
using BlueBasics.Enums;
using System;

namespace BlueControls.Controls
{
    [Designer(typeof(BasicDesigner))]
    public partial class FlexiControlForFilter : FlexiControl
    {


        /// <summary>
        /// ACHTUNG: Das Control wird niemals den Filter selbst ändern.
        /// Der Filter wird nur zur einfacheren Identifizierung der nachfolgenden Steuerelemente behalten.
        /// </summary>
        public readonly FilterItem Filter = null;

        public readonly Table TableView = null;


        public FlexiControlForFilter() : this(null, null, enÜberschriftAnordnung.Links_neben_Dem_Feld)
        {
            // Dieser Aufruf ist für den Designer erforderlich.
            // InitializeComponent();
        }

        public FlexiControlForFilter(Table tableView, FilterItem filter, enÜberschriftAnordnung captionPosition)
        {
            // Dieser Aufruf ist für den Designer erforderlich.
            InitializeComponent();

            // Fügen Sie Initialisierungen nach dem InitializeComponent()-Aufruf hinzu.
            Size = new Size(300, 300);
            TableView = tableView;
            Filter = filter;
            UpdateFilterData();

            InstantChangedEvent = true;

            Filter.Changed += Filter_Changed;
        }

        private void Filter_Changed(object sender, System.EventArgs e)
        {
            UpdateFilterData();
        }

        private void UpdateFilterData()
        {
            if (Filter == null || Filter.Column == null)
            {
                DisabledReason = "Bezug zum Filter verloren.";
                Caption = string.Empty;
                EditType = enEditTypeFormula.None;
                QuickInfo = string.Empty;
                FileEncryptionKey = string.Empty;
                Value = string.Empty;
            }
            else
            {
                DisabledReason = string.Empty;
                Caption = Filter.Column.ReadableText() + ":";

                var qi = Filter.Column.QickInfoText(string.Empty);

                if (string.IsNullOrEmpty(qi))
                {
                    QuickInfo = "<b>Filter:</b><br>" + Filter.ReadableText();
                }
                else
                {
                    QuickInfo = "<b>Filter:</b><br>" + Filter.ReadableText() + "<br><br><b>Info:</b><br>" + qi;
                }



                if (!Filter.Column.AutoFilterErlaubt)
                {
                    EditType = enEditTypeFormula.None;
                }
                else
                {

                    if (Filter.FilterType == enFilterType.Instr_GroßKleinEgal && Filter.SearchValue != null && Filter.SearchValue.Count == 1)
                    {
                        CaptionPosition = enÜberschriftAnordnung.Links_neben_Dem_Feld;
                        EditType = enEditTypeFormula.Textfeld_mit_Auswahlknopf;

                        Value = Filter.SearchValue[0];
                    }
                    else
                    {
                        CaptionPosition = enÜberschriftAnordnung.ohne;
                        EditType = enEditTypeFormula.Button;
                    }
                }

            }
        }



        protected override void OnControlAdded(System.Windows.Forms.ControlEventArgs e)
        {

            base.OnControlAdded(e);

            if (e.Control is ComboBox cbx)
            {
                var Item2 = new ItemCollectionList();
                Item2.Add(new TextListItem("|~", "Keine weiteren Einträge vorhanden"));

                //var c = Filter.Column.Contents(null);

                //foreach (var thiss in c)
                //{
                //    Item2.Add(new TextListItem("|" + thiss, thiss));
                //}

                StyleComboBox(cbx, Item2, System.Windows.Forms.ComboBoxStyle.DropDown);
                cbx.DropDownShowing += Cbx_DropDownShowing;
            }


            if (e.Control is Button btn)
            {
                btn.ImageCode = "Kreuz|16";
                btn.Text = Filter.ReadableText();
            }
        }


        private void Cbx_DropDownShowing(object sender, System.EventArgs e)
        {
            var cbx = (ComboBox)sender;








            //var List_FilterString = Column.Autofilter_ItemList(vFilter);


            //var F = Skin.GetBlueFont(enDesign.Table_Cell, enStates.Standard);

            //Width = Math.Max(TXTBox.Width + Skin.Padding * 2, Table.tmpColumnContentWidth(Column, F, 16));

            cbx.Item.Clear();
            cbx.Item.CheckBehavior = enCheckBehavior.MultiSelection;


            if (TableView == null)
            {
                cbx.Item.Add(new TextListItem("|~", "Anzeigefehler", enImageCode.Kreuz, false));
                return;
            }

            var List_FilterString = Filter.Column.Autofilter_ItemList(TableView.Filter);

            if (List_FilterString.Count == 0)
            {

                cbx.Item.Add(new TextListItem("|~", "Keine weiteren Einträge vorhanden", enImageCode.Kreuz, false));
            }

            else if (List_FilterString.Count < 400)
            {
                cbx.Item.AddRange(List_FilterString, Filter.Column, enShortenStyle.Replaced);
                cbx.Item.Sort(); // Wichtig, dieser Sort kümmert sich, dass das Format (z. B.  Zahlen) berücksichtigt wird
            }
            else
            {
                cbx.Item.Add(new TextListItem("|~", "Zu viele Einträge", enImageCode.Kreuz, false));
            }

        }

        private ComboBox GetComboBox()
        {

            foreach (var thisc in Controls)
            {
                if (thisc is ComboBox cbx)
                {

                    return cbx;
                }
            }
            return null;

        }

        internal bool WasThisValueClicked()
        {
            var cb = GetComboBox();
            if (cb == null) { return false; }
            return cb.WasThisValueClicked();
        }
    }
}