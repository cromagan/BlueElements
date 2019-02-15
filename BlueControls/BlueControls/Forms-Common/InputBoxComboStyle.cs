using BlueBasics;
using BlueControls.ItemCollection.ItemCollectionList;
using System.Collections.Generic;

namespace BlueControls.DialogBoxes
{
    public partial class InputBoxComboStyle : DialogBoxes.DialogWithOkAndCancel
    {

        string GiveBack = string.Empty;

        public InputBoxComboStyle()
        {
            InitializeComponent();
        }

        public InputBoxComboStyle(string TXT, string VorschlagsText, ItemCollectionList SuggestOriginal, bool TexteingabeErlaubt)
        {
            InitializeComponent();

            cbxText.Text = VorschlagsText;

            var SuggestClone = (ItemCollectionList)SuggestOriginal.Clone();

            cbxText.Item.CheckBehavior = SuggestClone.CheckBehavior;
            cbxText.Item.AddRange(SuggestClone);


            if (TexteingabeErlaubt)
            {
                cbxText.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDown;
            }
            else
            {
                cbxText.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            }


            Setup(TXT, cbxText, 250, true, true);

            GiveBack = VorschlagsText;
        }


        public static string Show(string TXT, ItemCollectionList Suggest,bool TexteingabeErlaubt)
        {
            return Show(TXT, string.Empty, Suggest, TexteingabeErlaubt);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="TXT"></param>
        /// <param name="VorschlagsText"></param>
        /// <param name="SuggestOriginal">Wird geklont, es kann auch aus einer Listbox kommen, und dann stimmen die Events nicht mehr. Es muss auch einbe ItemCollection bleiben, damit aus der Datenbank auch Bilder etc. angezeigt werden können.</param>
        /// <returns></returns>
        private static string Show(string TXT, string VorschlagsText, ItemCollectionList SuggestOriginal, bool TexteingabeErlaubt)
        {


            var MB = new InputBoxComboStyle(TXT, VorschlagsText, SuggestOriginal, TexteingabeErlaubt);
            MB.ShowDialog();

            return MB.GiveBack;
        }

        public static string Show(string TXT, List<string> Suggest, bool TexteingabeErlaubt)
        {
            var cSuggest = new ItemCollectionList();
            cSuggest.AddRange(Suggest);
            cSuggest.Sort();
            return Show(TXT, string.Empty, cSuggest, TexteingabeErlaubt);
        }

        private void cbxText_ESC(object sender, System.EventArgs e)
        {
            Cancel();
        }

        private void cbxText_Enter(object sender, System.EventArgs e)
        {
            Ok();
        }

        private void InputComboBox_Shown(object sender, System.EventArgs e)
        {
            cbxText.Focus();
        }

        protected override void SetValue()
        {
            if (CancelPressed)
            {
                GiveBack = string.Empty;
            }
            else
            {
                GiveBack = cbxText.Text;
            }
        }
    }
}
