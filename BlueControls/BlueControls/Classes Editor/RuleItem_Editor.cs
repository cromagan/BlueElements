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

using BlueBasics.EventArgs;
using BlueControls.ItemCollection;
using BlueDatabase;

namespace BlueControls.Classes_Editor
{
    internal sealed partial class RuleItem_Editor
    {

        public RuleItem_Editor()
        {
            InitializeComponent();
        }

        protected override void PrepaireFormula()
        {

        }



        protected override void EnabledAndFillFormula()
        {


            lstActionSelector.Enabled = true;
            RuleActionEditor.Enabled = true;


            lstActionSelector.Item.Clear();


            foreach (var ThisAction in Item.Actions)
            {
                if (ThisAction != null)
                {
                    lstActionSelector.Item.Add(ThisAction);
                }
            }
            lstActionSelector.Item.Sort();

            if (lstActionSelector.Item.Count > 1)
            {
                lstActionSelector.Item[0].Checked = true;
            }
            else
            {
                RuleActionEditor.Item = null;
            }
        }



        private void ActionSelector_AddClicked(object sender, System.EventArgs e)
        {
            if (Item == null)
            {
                Forms.Notification.Show("Bitte vorher eine Regel auswählen.");
                return;
            }


            var NewAction = new RuleActionItem(Item, 0, string.Empty, null);
            Item.Actions.Add(NewAction);

            var NewActionItem = lstActionSelector.Item.Add(NewAction);
            NewActionItem.Checked = true;
            OnChanged(Item);
        }

        private void lstActionSelector_ItemCheckedChanged(object sender, System.EventArgs e)
        {
            if (lstActionSelector.Item.Checked().Count != 1)
            {
                RuleActionEditor.Item = null;
                return;
            }

            var SelectedAction = (RuleActionItem)((TextListItem)lstActionSelector.Item.Checked()[0]).Tags;
            RuleActionEditor.Item = SelectedAction;
        }

        private void RuleActionEditor_Changed(object sender, System.EventArgs e)
        {

            if (IsFilling) { return; }


            foreach (var thisitem in lstActionSelector.Item)
            {
                if (thisitem is TextListItem tli)
                {
                    if (tli.Tags == RuleActionEditor.Item)
                    {
                        tli.Text = RuleActionEditor.Item.ReadableText();
                        tli.Symbol = RuleActionEditor.Item.SymbolForReadableText();
                    }
                }
            }


            OnChanged(Item);
        }


        private void lstActionSelector_ItemRemoving(object sender, ListEventArgs e)
        {
            if (Item == null) { return; }
            Item.Actions.Remove((RuleActionItem)((TextListItem)e.Item).Tags);
            OnChanged(Item);
        }



        protected override void DisableAndClearFormula()
        {
            lstActionSelector.Enabled = true;
            RuleActionEditor.Enabled = false;

            lstActionSelector.Item.Clear();
            RuleActionEditor.Item = null;
        }
    }
}
