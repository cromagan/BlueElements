// Authors:
// Christian Peter
//
// Copyright (c) 2025 Christian Peter
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

using BlueBasics;
using BlueControls.Enums;
using BlueControls.ItemCollectionList;
using System.Collections.Generic;
using static BlueControls.ItemCollectionList.AbstractListItemExtension;

namespace BlueControls.Forms;

public partial class InputBoxListBoxStyle : DialogWithOkAndCancel {

    #region Fields

    private List<string>? _giveBack;

    #endregion

    #region Constructors

    private InputBoxListBoxStyle() : this(string.Empty, null, CheckBehavior.SingleSelection, null, AddType.None, true) { }

    private InputBoxListBoxStyle(string txt, List<AbstractListItem>? itemsOriginal, CheckBehavior checkBehavior, List<string>? check, AddType addNewAllowed, bool autosort) : base(true, true) {
        InitializeComponent();

        txbText.CheckBehavior = checkBehavior;
        txbText.ItemAddRange(itemsOriginal);
        if (check != null) { txbText.Check(check); }
        txbText.MoveAllowed = false;
        txbText.RemoveAllowed = false;
        txbText.AddAllowed = addNewAllowed;
        txbText.AddAllowed = addNewAllowed;
        txbText.AutoSort = autosort;
        Setup(txt, txbText, 250);
    }

    #endregion

    #region Methods

    public static string Show(string txt, List<string>? items) {
        if (items is not { Count: not 0 }) {
            return InputBox.Show(txt, string.Empty, FormatHolder.Text);
        }

        List<AbstractListItem> x = [];
        x.AddRange(ItemsOf(items));
        //x.Sort();
        var erg = Show(txt, x, CheckBehavior.SingleSelection, null, AddType.None);
        return erg?.Count != 1 ? string.Empty : erg[0];
    }

    public static List<string>? Show(string txt, List<AbstractListItem> items, CheckBehavior checkBehavior, List<string>? check, AddType addNewAllowed) {
        InputBoxListBoxStyle mb = new(txt, items, checkBehavior, check, addNewAllowed, true);
        mb.ShowDialog();
        return mb._giveBack;
    }

    protected override bool SetValue() {
        _giveBack = Canceled ? null : [.. txbText.Checked];
        return true;
    }

    private void InputBox_Shown(object sender, System.EventArgs e) => txbText.Focus();

    #endregion
}