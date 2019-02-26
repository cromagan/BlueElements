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

using BlueBasics;
using BlueBasics.Interfaces;
using BlueControls.EventArgs;
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


            ActionSelector.Enabled = true;
            RuleActionEditor.Enabled = true;


            ActionSelector.Item.Clear();


            foreach (var ThisAction in tmp.Actions)
            {
                if (ThisAction != null)
                {
                    ActionSelector.Item.Add(new ObjectListItem(ThisAction));
                }
            }
            ActionSelector.Item.Sort();

            if (ActionSelector.Item.Count > 1)
            {
                ActionSelector.Item[0].Checked = true;
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
                DialogBoxes.Notification.Show("Bitte vorher eine Regel auswählen.");
                return;
            }


            var NewAction = new RuleActionItem(tmp, 0, string.Empty, null);
            tmp.Actions.Add(NewAction);

            var NewActionItem = new ObjectListItem(NewAction);

            ActionSelector.Item.Add(NewActionItem);

            NewActionItem.Checked = true;
            OnChanged(tmp);
        }

        private void ActionSelector_Item_CheckedChanged(object sender, System.EventArgs e)
        {
            if (ActionSelector.Item.Checked().Count != 1)
            {
                RuleActionEditor.ObjectWithDialog = null;
                return;
            }

            var SelectedAction = (RuleActionItem)((ObjectListItem)ActionSelector.Item.Checked()[0]).Obj;
            RuleActionEditor.ObjectWithDialog = SelectedAction;
        }

        private void RuleActionEditor_Changed(object sender, System.EventArgs e)
        {
            if (IsFilling()) { return; }
            ActionSelector.Invalidate();

            OnChanged(tmp);
        }

        private void ActionSelector_RemoveClicked(object sender, ListOfBasicListItemEventArgs e)
        {
            OnChanged(tmp);
        }



        protected override void DisableAndClearFormula()
        {
            ActionSelector.Enabled = true;
            RuleActionEditor.Enabled = false;

            ActionSelector.Item.Clear();
            RuleActionEditor.ObjectWithDialog = null;
        }


    }
}
