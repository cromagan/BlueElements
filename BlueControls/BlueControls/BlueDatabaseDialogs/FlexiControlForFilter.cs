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

        List<string> AllItems = null;


        public FlexiControlForFilter() : this(null, enÜberschriftAnordnung.Links_neben_Dem_Feld)
        {
            // Dieser Aufruf ist für den Designer erforderlich.
            // InitializeComponent();
        }

        public FlexiControlForFilter(FilterItem filter, enÜberschriftAnordnung captionPosition)
        {
            // Dieser Aufruf ist für den Designer erforderlich.
            InitializeComponent();

            // Fügen Sie Initialisierungen nach dem InitializeComponent()-Aufruf hinzu.
            Size = new Size(300, 300);
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
                Enabled = false;
                Caption = string.Empty;
                EditType = enEditTypeFormula.None;
                QuickInfo = string.Empty;
                FileEncryptionKey = string.Empty;
                Value = string.Empty;
            }
            else
            {
                Enabled = true;
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
                ItemCollectionList.GetItemCollection(Item2, Filter.Column, null, enShortenStyle.Unreplaced, 10000);
                StyleComboBox(cbx, Item2, System.Windows.Forms.ComboBoxStyle.DropDown);
            }


            if (e.Control is Button btn)
            {
                btn.ImageCode = "Kreuz|16";
                btn.Text = Filter.ReadableText();
            }

        }



        public bool IsThisItemBetterForIS()
        {
            var cbx = GetComboBox();
            if (cbx == null) { return true; }

            return cbx.Item[Value] != null;
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




    }
}