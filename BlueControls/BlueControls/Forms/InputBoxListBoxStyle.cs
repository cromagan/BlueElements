// Authors:
// Christian Peter
//
// Copyright (c) 2024 Christian Peter
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

using System.Collections.Generic;
using BlueBasics;
using BlueControls.Enums;

namespace BlueControls.Forms;

public partial class InputBoxListBoxStyle : DialogWithOkAndCancel {

    #region Fields

    private List<string>? _giveBack;

    #endregion

    #region Constructors

    private InputBoxListBoxStyle() : this(string.Empty, new ItemCollectionList.ItemCollectionList(true), CheckBehavior.SingleSelection, null, AddType.None) { }

    private InputBoxListBoxStyle(string txt, ItemCollectionList.ItemCollectionList itemsOriginal, CheckBehavior checkBehavior, List<string>? check, AddType addNewAllowed) : base(checkBehavior != CheckBehavior.AlwaysSingleSelection, true) {
        InitializeComponent();
        if (itemsOriginal.Appearance is not ListBoxAppearance.Listbox and not ListBoxAppearance.Listbox_Boxes) {
            Develop.DebugPrint("Design nicht Listbox");
        }
        //var itemsClone = (ItemCollectionList)itemsOriginal.Clone();
        txbText.CheckBehavior = checkBehavior;
        txbText.Item.AddClonesFrom(itemsOriginal);
        if (check != null) { txbText.Check(check); }
        txbText.MoveAllowed = false;
        txbText.RemoveAllowed = false;
        txbText.AddAllowed = addNewAllowed;
        txbText.AddAllowed = addNewAllowed;
        txbText.AutoSort = itemsOriginal.AutoSort;
        Setup(txt, txbText, 250);
    }

    #endregion

    #region Methods

    public static string Show(string txt, List<string>? items) {
        if (items == null || items.Count == 0) {
            return InputBox.Show(txt, string.Empty, FormatHolder.Text);
        }

        ItemCollectionList.ItemCollectionList x = new(ListBoxAppearance.Listbox, true);
        x.AddRange(items);
        //x.Sort();
        var erg = Show(txt, x, CheckBehavior.AlwaysSingleSelection, null, AddType.None);
        return erg is null || erg.Count != 1 ? string.Empty : erg[0];
    }

    public static List<string>? Show(string txt, ItemCollectionList.ItemCollectionList items, CheckBehavior checkBehavior, List<string>? check, AddType addNewAllowed) {
        InputBoxListBoxStyle mb = new(txt, items, checkBehavior, check, addNewAllowed);
        _ = mb.ShowDialog();
        return mb._giveBack;
    }

    protected override void SetValue(bool canceled) => _giveBack = canceled ? null : [.. txbText.Checked];

    private void InputBox_Shown(object sender, System.EventArgs e) => txbText.Focus();

    #endregion
}