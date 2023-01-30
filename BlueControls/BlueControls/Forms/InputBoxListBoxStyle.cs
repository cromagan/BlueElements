// Authors:
// Christian Peter
//
// Copyright (c) 2023 Christian Peter
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

#nullable enable

using BlueBasics;
using BlueBasics.Enums;
using BlueControls.Enums;
using BlueControls.ItemCollection.ItemCollectionList;
using System.Collections.Generic;
using BlueDatabase;

namespace BlueControls.Forms;

public partial class InputBoxListBoxStyle : DialogWithOkAndCancel {

    #region Fields

    private List<string>? _giveBack;

    #endregion

    #region Constructors

    private InputBoxListBoxStyle() : this(string.Empty, new ItemCollectionList(), AddType.None, true) { }

    private InputBoxListBoxStyle(string txt, ItemCollectionList itemsOriginal, AddType addNewAllowed, bool cancelErl) : base(cancelErl, true) {
        InitializeComponent();
        if (itemsOriginal.Appearance != BlueListBoxAppearance.Listbox) {
            Develop.DebugPrint("Design nicht Listbox");
        }
        //var itemsClone = (ItemCollectionList)itemsOriginal.Clone();
        txbText.Item.CheckBehavior = itemsOriginal.CheckBehavior;
        txbText.Item.AddClonesFrom(itemsOriginal);
        txbText.MoveAllowed = false;
        txbText.RemoveAllowed = false;
        txbText.AddAllowed = addNewAllowed;
        txbText.AddAllowed = addNewAllowed;
        Setup(txt, txbText, 250);
    }

    #endregion

    #region Methods

    public static string Show(string txt, List<string>? items) {
        if (items == null || items.Count == 0) {
            return InputBox.Show(txt, string.Empty, FormatHolder.Text);
        }
        ItemCollectionList x = new(BlueListBoxAppearance.Listbox) {
            CheckBehavior = CheckBehavior.AlwaysSingleSelection
        };
        x.AddRange(items);
        x.Sort();
        var erg = Show(txt, x, AddType.None, true);
        return erg is null || erg.Count != 1 ? string.Empty : erg[0];
    }

    public static List<string>? Show(string txt, ItemCollectionList? items, AddType addNewAllowed, bool cancelErl) {
        InputBoxListBoxStyle mb = new(txt, items, addNewAllowed, cancelErl);
        mb.ShowDialog();
        return mb._giveBack;
    }

    protected override void SetValue(bool canceled) => _giveBack = canceled ? null : txbText.Item.Checked().ToListOfString();

    private void InputBox_Shown(object sender, System.EventArgs e) => txbText.Focus();

    #endregion
}