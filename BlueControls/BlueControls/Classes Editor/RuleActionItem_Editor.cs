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
using BlueControls.Forms;
using BlueControls.EventArgs;
using BlueControls.ItemCollection;
using BlueDatabase;
using BlueDatabase.Enums;
using BlueControls.Enums;

namespace BlueControls.Classes_Editor
{
    internal sealed partial class RuleActionItem_Editor
    {

        public RuleActionItem_Editor() : base()
        {
            InitializeComponent();
        }







        protected override void PrepaireFormula()
        {



            cbxRuleAktion.Item.Clear();


            cbxRuleAktion.Item.Add(new TextListItem("0+", "Wenn...", false));
            for (var z = 0; z <= 5000; z++)
            {

                if (z == 1000)
                {
                    cbxRuleAktion.Item.Add(new TextListItem("1000+", "Dann...", false));
                }


                var Ac = (enAction)z;

                if (Ac.ToString() != z.ToString())
                {
                    var t = string.Empty;
                    QuickImage s = null;
                    RuleActionItem.MaximalText(Item.Rule.Database, Ac, ref t, ref s);
                    cbxRuleAktion.Item.Add(new TextListItem(z.ToString(), t, s));
                }
            }


            lstRuleAktionColumns.Item.Clear();
            lstRuleAktionColumns.Item.AddRange(Item.Rule.Database.Column, false, false);
            lstRuleAktionColumns.Item.Sort();
        }

        private void Enable_Action_Controls(bool CanDeleteAllData)
        {


            if (Item != null)
            {

                btnHelp.Enabled = true;
                cbxRuleAktion.Enabled = true;

                switch (RuleActionItem.NeededColumns(cbxRuleAktion.Text))
                {
                    case enNeededColumns.None:
                        lstRuleAktionColumns.Enabled = false;
                        break;
                    case enNeededColumns.OnlyOne:
                        lstRuleAktionColumns.Enabled = true;
                        lstRuleAktionColumns.Item.CheckBehavior = enCheckBehavior.SingleSelection;
                        break;
                    default:
                        lstRuleAktionColumns.Enabled = true;
                        lstRuleAktionColumns.Item.CheckBehavior = enCheckBehavior.MultiSelection;
                        break;
                }



                switch (RuleActionItem.NeededText(cbxRuleAktion.Text))
                {
                    case enNeededText.None:
                        txbRuleActionText.Enabled = false;
                        break;
                    //case enNeededText.OnlyOneLine:
                    //    Rule_Action_Text.Enabled = true;
                    //    Rule_Action_Text.MultiLine = false;
                    //    break;
                    default:
                        txbRuleActionText.Enabled = true;
                        txbRuleActionText.MultiLine = true;

                        break;
                }

            }
            else
            {

                btnHelp.Enabled = false;
                cbxRuleAktion.Enabled = false;
                lstRuleAktionColumns.Enabled = false;
                txbRuleActionText.Enabled = false;

            }

            if (!cbxRuleAktion.Enabled)
            {
                if (CanDeleteAllData)
                {
                    cbxRuleAktion.Text = string.Empty;
                }
            }

            if (!txbRuleActionText.Enabled)
            {
                if (CanDeleteAllData)
                {
                    txbRuleActionText.Text = string.Empty;
                }
                else
                {
                    if (!string.IsNullOrEmpty(txbRuleActionText.Text))
                    {
                        txbRuleActionText.Enabled = true;
                    }
                }
            }

            if (!lstRuleAktionColumns.Enabled)
            {
                if (CanDeleteAllData)
                {
                    lstRuleAktionColumns.Item.UncheckAll();
                }
                else
                {
                    if (lstRuleAktionColumns.Item.Checked().Count > 0)
                    {
                        lstRuleAktionColumns.Enabled = true;
                    }
                }
            }




        }

        private void WriteBack()
        {

            if (IsFilling) { return; }

            Item.Action = (enAction)int.Parse(cbxRuleAktion.Text);

            var l = lstRuleAktionColumns.Item.Checked();
            Item.Columns.Clear(); // = New List(Of ColumnItem)

            foreach (var t1 in l)
            {
                Item.Columns.Add((ColumnItem)((TextListItem)t1).Tags);
            }

            Item.Text = txbRuleActionText.Text;



            Enable_Action_Controls(false); // Um nicht mehr erlaubte Felder aus und einzuschalten
            OnChanged(Item);
        }

        private void lstRuleAktionColumns_ItemClicked(object sender, BasicListItemEventArgs e)
        {
            WriteBack();
        }

        private void cbxRuleAktion_ItemClicked(object sender, BasicListItemEventArgs e)
        {
            WriteBack();
        }

        protected override void EnabledAndFillFormula()
        {
            Enabled = true;

            cbxRuleAktion.Text = ((int)Item.Action).ToString();

            if (cbxRuleAktion.Text == "0")
            {
                cbxRuleAktion.Text = "0+";
            }

            lstRuleAktionColumns.Item.UncheckAll();
            foreach (var t in Item.Columns)
            {
                if (t != null && lstRuleAktionColumns.Item[t.Name] != null) { lstRuleAktionColumns.Item[t.Name].Checked = true; }
            }
            txbRuleActionText.Text = Item.Text;



            Enable_Action_Controls(false);
        }

        private void cbxRuleAktion_TextChanged(object sender, System.EventArgs e)
        {
            WriteBack();
        }

        private void btnHelp_Click(object sender, System.EventArgs e)
        {
            if (string.IsNullOrEmpty(cbxRuleAktion.Text)) { return; }

            var x = (enAction)int.Parse(cbxRuleAktion.Text);
            if (x.ToString() == ((int)x).ToString()) { return; }

            MessageBox.Show(RuleActionItem.HelpTextinHTML(Item.Rule.Database, x), enImageCode.Information, "OK");

        }


        private void txbRuleActionText_TextChanged(object sender, System.EventArgs e)
        {
            WriteBack();
        }

        protected override void DisableAndClearFormula()
        {
            Enabled = false;
            Enable_Action_Controls(true);
        }


    }
}
