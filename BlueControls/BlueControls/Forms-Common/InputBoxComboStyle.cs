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


using BlueControls.ItemCollection;
using System.Collections.Generic;

namespace BlueControls.Forms
{
    public partial class InputBoxComboStyle : Forms.DialogWithOkAndCancel
    {

        string GiveBack = string.Empty;


        #region Konstruktor


        private InputBoxComboStyle() : base()
        {
            InitializeComponent();
        }

        private InputBoxComboStyle(string TXT, string VorschlagsText, ItemCollectionList SuggestOriginal, bool TexteingabeErlaubt) : this()
        {
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


        #endregion

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

        protected override void SetValue(bool canceled)
        {
            if (canceled)
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
