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

using BlueBasics.EventArgs;
using BlueBasics.Interfaces;
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

        private RuleItem tmp;

        protected override void ConvertObject(IObjectWithDialog ThisObject)
        {
            tmp = (RuleItem)ThisObject;
        }

        protected override void PrepaireFormula()
        {

        }



        protected override void EnabledAndFillFormula()
        {


            lstActionSelector.Enabled = true;
            RuleActionEditor.Enabled = true;


            lstActionSelector.Item.Clear();


            foreach (var ThisAction in tmp.Actions)
            {
                if (ThisAction != null)
                {
                    lstActionSelector.Item.Add(new TextListItem(ThisAction));
                }
            }
            lstActionSelector.Item.Sort();

            if (lstActionSelector.Item.Count > 1)
            {
                lstActionSelector.Item[0].Checked = true;
            }
            else
            {
                RuleActionEditor.ObjectWithDialog = null;
            }
        }



        private void ActionSelector_AddClicked(object sender, System.EventArgs e)
        {
            if (tmp== null)
            {
                Forms.Notification.Show("Bitte vorher eine Regel auswählen.");
                return;
            }


            var NewAction = new RuleActionItem(tmp, 0, string.Empty, null);
            tmp.Actions.Add(NewAction);

            var NewActionItem = new TextListItem(NewAction);

            lstActionSelector.Item.Add(NewActionItem);

            NewActionItem.Checked = true;
            OnChanged(tmp);
        }

        private void lstActionSelector_ItemCheckedChanged(object sender, System.EventArgs e)
        {
            if (lstActionSelector.Item.Checked().Count != 1)
            {
                RuleActionEditor.ObjectWithDialog = null;
                return;
            }

            var SelectedAction = (RuleActionItem)((TextListItem)lstActionSelector.Item.Checked()[0]).Tags;
            RuleActionEditor.ObjectWithDialog = SelectedAction;
        }

        private void RuleActionEditor_Changed(object sender, System.EventArgs e)
        {

            if (IsFilling()) { return; }


            foreach (var thisitem in lstActionSelector.Item)
            {
                if (thisitem is TextListItem tli)
                {
                    if (tli.Tags == RuleActionEditor.ObjectWithDialog)
                    {
                        tli.Text = RuleActionEditor.ObjectWithDialog.ReadableText();
                        tli.Symbol = RuleActionEditor.ObjectWithDialog.SymbolForReadableText();
                    }
                }
            }


            OnChanged(tmp);
        }


        private void lstActionSelector_ItemRemoving(object sender, ListEventArgs e)
        {
            if (tmp == null) { return; }
            tmp.Actions.Remove((RuleActionItem)((TextListItem)e.Item).Tags);
            OnChanged(tmp);
        }



        protected override void DisableAndClearFormula()
        {
            lstActionSelector.Enabled = true;
            RuleActionEditor.Enabled = false;

            lstActionSelector.Item.Clear();
            RuleActionEditor.ObjectWithDialog = null;
        }
    }
}
