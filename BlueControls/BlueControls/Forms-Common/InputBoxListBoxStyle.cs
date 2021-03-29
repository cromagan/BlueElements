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
using BlueControls.Enums;
using BlueControls.ItemCollection;
using System.Collections.Generic;

namespace BlueControls.Forms {
    public partial class InputBoxListBoxStyle : Forms.DialogWithOkAndCancel {
        private List<string> GiveBack = null;

        #region Konstruktor


        private InputBoxListBoxStyle() : base() {
            InitializeComponent();
        }

        private InputBoxListBoxStyle(string TXT, ItemCollectionList ItemsOriginal, enAddType AddNewAllowed, bool CancelErl) : this() {
            if (ItemsOriginal.Appearance != enBlueListBoxAppearance.Listbox) {
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

        #endregion

        public static string Show(string TXT, List<string> Items) {

            if (Items == null || Items.Count == 0) {
                return InputBox.Show(TXT, "", enDataFormat.Text);
            }


            var x = new ItemCollectionList(enBlueListBoxAppearance.Listbox) {
                CheckBehavior = enCheckBehavior.AlwaysSingleSelection
            };
            x.AddRange(Items);
            x.Sort();

            var erg = Show(TXT, x, enAddType.None, true);


            if (erg is null || erg.Count != 1) { return string.Empty; }
            return erg[0];
        }


        public static List<string> Show(string TXT, ItemCollectionList ItemsOriginal, enAddType AddNewAllowed, bool CancelErl) {
            var MB = new InputBoxListBoxStyle(TXT, ItemsOriginal, AddNewAllowed, CancelErl);
            MB.ShowDialog();

            return MB.GiveBack;
        }





        private void InputBox_Shown(object sender, System.EventArgs e) {
            txbText.Focus();
        }

        protected override void SetValue(bool canceled) {
            if (canceled) {
                GiveBack = null;
            } else {
                GiveBack = txbText.Item.Checked().ToListOfString();
            }
        }
    }
}
