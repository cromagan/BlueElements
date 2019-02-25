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
using BlueBasics;
using BlueBasics.Enums;
using BlueControls.DialogBoxes;
using BlueControls.EventArgs;
using BlueControls.ItemCollection;
using BlueDatabase;
using BlueDatabase.Enums;
using BlueControls.Enums;

namespace BlueControls.Classes_Editor
{
    internal sealed partial class RuleActionItem_Editor
    {

        public RuleActionItem_Editor()
        {
            InitializeComponent();
        }

        private RuleActionItem tmp;


        protected override void ConvertObject(object ThisObject)
        {
            tmp = (RuleActionItem)ThisObject;
        }






        protected override void PrepaireFormula()
        {



            Rule_Aktion.Item.Clear();


            Rule_Aktion.Item.Add(new TextListItem("0+", "Wenn...", false));
            for (var z = 0 ; z <= 5000 ; z++)
            {

                if (z == 1000)
                {
                    Rule_Aktion.Item.Add(new TextListItem("1000+", "Dann...", false));
                }


                var Ac = (enAction)z;

                if (Ac.ToString() != z.ToString())
                {


                    if (Ac != enAction.LinkedCell)
                    {

                        var t = string.Empty;
                        QuickImage s = null;
                        RuleActionItem.MaximalText(tmp.Rule.Database, Ac, ref t, ref s);

                        Rule_Aktion.Item.Add(new TextListItem(z.ToString(), t, s));
                    }
                }
            }


            Rule_Aktion_Columns.Item.Clear();
            Rule_Aktion_Columns.Item.AddRange(tmp.Rule.Database.Column, false, false);
            Rule_Aktion_Columns.Item.Sort();
        }

        private void Enable_Action_Controls(bool CanDeleteAllData)
        {


            if (tmp != null)
            {

                Help.Enabled = true;
                Rule_Aktion.Enabled = true;

                switch (RuleActionItem.NeededColumns(Rule_Aktion.Text))
                {
                    case enNeededColumns.None:
                        Rule_Aktion_Columns.Enabled = false;
                        break;
                    case enNeededColumns.OnlyOne:
                        Rule_Aktion_Columns.Enabled = true;
                        Rule_Aktion_Columns.Item.CheckBehavior = enCheckBehavior.SingleSelection;
                        break;
                    default:
                        Rule_Aktion_Columns.Enabled = true;
                        Rule_Aktion_Columns.Item.CheckBehavior = enCheckBehavior.MultiSelection;
                        break;
                }



                switch (RuleActionItem.NeededText(Rule_Aktion.Text))
                {
                    case enNeededText.None:
                        Rule_Action_Text.Enabled = false;
                        break;
                    //case enNeededText.OnlyOneLine:
                    //    Rule_Action_Text.Enabled = true;
                    //    Rule_Action_Text.MultiLine = false;
                    //    break;
                    default:
                        Rule_Action_Text.Enabled = true;
                        Rule_Action_Text.MultiLine = true;

                        break;
                }

            }
            else
            {

                Help.Enabled = false;
                Rule_Aktion.Enabled = false;
                Rule_Aktion_Columns.Enabled = false;
                Rule_Action_Text.Enabled = false;

            }

            if (!Rule_Aktion.Enabled)
            {
                if (CanDeleteAllData)
                {
                    Rule_Aktion.Text = string.Empty;
                }
            }

            if (!Rule_Action_Text.Enabled)
            {
                if (CanDeleteAllData)
                {
                    Rule_Action_Text.Text = string.Empty;
                }
                else
                {
                    if (!string.IsNullOrEmpty(Rule_Action_Text.Text))
                    {
                        Rule_Action_Text.Enabled = true;
                    }
                }
            }

            if (!Rule_Aktion_Columns.Enabled)
            {
                if (CanDeleteAllData)
                {
                    Rule_Aktion_Columns.Item.UncheckAll();
                }
                else
                {
                    if (Rule_Aktion_Columns.Item.Checked().Count > 0)
                    {
                        Rule_Aktion_Columns.Enabled = true;
                    }
                }
            }




        }

        private void WriteBack()
        {

            if (IsFilling())
            {
                return;
            }

            tmp.Action = (enAction)Convert.ToInt32(Rule_Aktion.Text);

            var l = Rule_Aktion_Columns.Item.Checked().ToListOfString();
            tmp.Columns.Clear(); // = New List(Of ColumnItem)

            foreach (var t1 in l)
            {
                tmp.Columns.Add(tmp.Rule.Database.Column[t1]);
            }

            tmp.Text = Rule_Action_Text.Text;



            Enable_Action_Controls(false); // Um nicht mehr erlaubte Felder aus und einzuschalten
            OnChanged(tmp);
        }

        private void Rule_Aktion_Columns_Item_Click(object sender, BasicListItemEventArgs e)
        {
            WriteBack();
        }

        private void Rule_Aktion_Item_Click(object sender, BasicListItemEventArgs e)
        {
            WriteBack();
        }

        protected override void EnabledAndFillFormula()
        {
            Enabled = true;

            Rule_Aktion.Text = ((int)tmp.Action).ToString();

            if (Rule_Aktion.Text == "0")
            {
                Rule_Aktion.Text = "0+";
            }

            Rule_Aktion_Columns.Item.UncheckAll();
            foreach (var t in tmp.Columns)
            {
                if (t != null && Rule_Aktion_Columns.Item[t.Name] != null) { Rule_Aktion_Columns.Item[t.Name].Checked = true; }
            }
            Rule_Action_Text.Text = tmp.Text;



            Enable_Action_Controls(false);
        }

        private void Rule_Aktion_TextChanged(object sender, System.EventArgs e)
        {
            WriteBack();
        }

        private void Help_Click(object sender, System.EventArgs e)
        {
            if (string.IsNullOrEmpty(Rule_Aktion.Text)) { return; }

            var x = (enAction)int.Parse(Rule_Aktion.Text);
            if (x.ToString() == ((int)x).ToString()) { return; }

            MessageBox.Show(RuleActionItem.HelpTextinHTML(tmp.Rule.Database, x), enImageCode.Information, "OK");

        }


        private void Rule_Action_Text_TextChanged(object sender, System.EventArgs e)
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
