using BlueBasics.EventArgs;
using static BlueBasics.Extensions;
using BlueControls.EventArgs;
using BlueControls.ItemCollection.ItemCollectionList;
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

        protected override void ConvertObject(object ThisObject)
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



        private void ActionSelector_Add_Clicked(object sender, AllreadyHandledEventArgs e)
        {
            e.AlreadyHandled = true;


            if (tmp== null)
            {
                BlueControls.DialogBoxes.Notification.Show("Bitte vorher eine Regel auswählen.");
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

            var SelectedAction = (RuleActionItem)((ObjectListItem)ActionSelector.Item.Checked()[0]).ObjectReadable;
            RuleActionEditor.ObjectWithDialog = SelectedAction;
        }

        private void RuleActionEditor_Changed(object sender, System.EventArgs e)
        {
            if (IsFilling()) { return; }
            ActionSelector.Invalidate();

            OnChanged(tmp);
        }

        private void ActionSelector_Remove_Clicked(object sender, ListOfBasicListItemEventArgs e)
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
