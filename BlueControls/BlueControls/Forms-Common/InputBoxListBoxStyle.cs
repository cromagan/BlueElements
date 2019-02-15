using BlueBasics;
using BlueBasics.Enums;
using BlueControls.Controls;
using BlueControls.Enums;
using BlueControls.ItemCollection.ItemCollectionList;
using System.Collections.Generic;

namespace BlueControls.DialogBoxes
{
    public partial class InputBoxListBoxStyle : DialogBoxes.DialogWithOkAndCancel
    {

        List<string> GiveBack = null;

        public InputBoxListBoxStyle()
        {
            InitializeComponent();
        }

        public InputBoxListBoxStyle(string TXT, ItemCollectionList ItemsOriginal, enAddType AddNewAllowed, bool CancelErl)
        {
            InitializeComponent();


            if (ItemsOriginal.Appearance != enBlueListBoxAppearance.Listbox)
            {
                Develop.DebugPrint("Design nicht Listbox");
            }



            var itemsClone = (ItemCollectionList)ItemsOriginal.Clone();
            txbText.Item.CheckBehavior = itemsClone.CheckBehavior;
            txbText.Item.AddRange(itemsClone);

            txbText.MoveAllowed = false;
            txbText.RemoveAllowed = false;
            txbText.AddAllowed = AddNewAllowed;

            txbText.AddAllowed = AddNewAllowed;

            Setup(TXT, txbText, 250, CancelErl, true);


        }

        public static string Show(string TXT, List<string> Items)
        {

            if (Items == null || Items.Count == 0)
            {
                return InputBox.Show(TXT, "", enDataFormat.Text_Ohne_Kritische_Zeichen);
            }


            var x = new ItemCollectionList(enBlueListBoxAppearance.Listbox);
            x.CheckBehavior = enCheckBehavior.AlwaysSingleSelection;
            x.AddRange(Items);
            x.Sort();

            var erg = Show(TXT, x , enAddType.None, true);


            if (erg is null || erg.Count != 1) { return string.Empty; }
            return erg[0];
        }


        public static List<string> Show(string TXT, ItemCollectionList ItemsOriginal, enAddType AddNewAllowed, bool CancelErl)
        {

           var MB = new InputBoxListBoxStyle(TXT, ItemsOriginal, AddNewAllowed, CancelErl);
            MB.ShowDialog();

            return MB.GiveBack;
        }





        private void InputBox_Shown(object sender, System.EventArgs e)
        {
            txbText.Focus();
        }

        protected override void SetValue()
        {
            if (CancelPressed)
            {
                GiveBack = null;
            }
            else
            {
                GiveBack = txbText.Item.Checked().ToListOfString(); 
            }
        }
    }
}
