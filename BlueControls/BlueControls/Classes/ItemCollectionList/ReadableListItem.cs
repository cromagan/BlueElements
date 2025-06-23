﻿// Authors:
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

#nullable enable

using BlueBasics.Interfaces;
using System.ComponentModel;

namespace BlueControls.ItemCollectionList;

public class ReadableListItem : TextListItem {

    #region Constructors

    public ReadableListItem(IReadableTextWithKey item, bool isCaption, bool enabled, string userDefCompareKey) : base(item.ReadableText(), item.KeyName, item.SymbolForReadableText(), isCaption, enabled, userDefCompareKey) {
        Item = item;

        if (item is INotifyPropertyChanged it2) {
            it2.PropertyChanged += Item_PropertyChanged;
        }
    }

    #endregion

    #region Properties

    public IReadableTextWithKey Item { get; }
    public override string QuickInfo => Item.ColumnQuickInfo;

    #endregion

    #region Methods

    private void Item_PropertyChanged(object sender, PropertyChangedEventArgs e) {
        Text = Item.ReadableText();
        Symbol = Item.SymbolForReadableText();
        KeyName = Item.KeyName;
        OnPropertyChanged(e.PropertyName); // Um den Refresh zu bewirken
    }

    #endregion
}