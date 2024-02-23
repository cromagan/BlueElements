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

using BlueBasics;
using BlueBasics.Interfaces;

namespace BlueControls.ItemCollectionList;

public class ReadableListItem : TextListItem {

    #region Constructors

    public ReadableListItem(IReadableTextWithKey item, bool isCaption, bool enabled, string userDefCompareKey) : base(item.ReadableText(), item.KeyName, item.SymbolForReadableText(), isCaption, enabled, userDefCompareKey) {
        Item = item;

        if (item is IChangedFeedback it2) {
            it2.Changed += Item_Changed;
        }
    }

    #endregion

    #region Properties

    public IReadableTextWithKey Item { get; }
    public override string QuickInfo => Item.QuickInfo;

    #endregion

    #region Methods

    public override object Clone() {
        var l = new ReadableListItem(Item, IsCaption, Enabled, UserDefCompareKey);
        l.CloneBasicStatesFrom(this);
        return l;
    }

    private void Item_Changed(object sender, System.EventArgs e) {
        Text = Item.ReadableText();
        Symbol = Item.SymbolForReadableText();
        Internal = Item.KeyName;
        OnChanged(); // Um den Refresh zu bewirken
    }

    #endregion
}