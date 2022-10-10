// Authors:
// Christian Peter
//
// Copyright (c) 2022 Christian Peter
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
using BlueControls.ItemCollection.ItemCollectionList;
using BlueDatabase;
using System;
using System.Collections.Generic;
using System.Linq;

namespace BlueControls.Forms;

public sealed partial class ItemSelect : DialogWithOkAndCancel {

    #region Fields

    private BasicListItem? _giveBack;

    #endregion

    #region Constructors

    private ItemSelect(List<BasicListItem?> items) : base(true, true) {
        InitializeComponent();

        List.Item.Clear();
        foreach (var thisItem in items) {
            List.Item.Add(thisItem);
        }

        List.Item.Sort();

        Setup(400, List.Bottom);
    }

    #endregion

    #region Methods

    public static RowItem? Show(List<RowItem?> rows, string layoutId) {
        try {
            var items = rows.Select(thisRow => new RowFormulaListItem(thisRow, layoutId, string.Empty)).Cast<BasicListItem?>().ToList();

            var x = Show(items);
            return (x as RowFormulaListItem)?.Row;
        } catch (Exception ex) {
            Develop.DebugPrint(ex);
            return null;
        }
    }

    public static string Show(List<string> files) {
        var items = new List<BasicListItem?>();

        foreach (var thisString in files) {
            if (thisString.FileType() == FileFormat.Image) {
                items.Add(new BitmapListItem(thisString, thisString, thisString.FileNameWithoutSuffix()));
            }
        }
        var x = Show(items);
        return x != null ? x.Internal : string.Empty;
    }

    public static BasicListItem? Show(List<BasicListItem?>? items) {
        if (items == null || items.Count == 0) { return null; }

        var x = new ItemSelect(items);
        x.ShowDialog();

        return x._giveBack;
    }

    protected override void SetValue(bool canceled) {
        _giveBack = null;
        if (canceled) { return; }

        var l = List.Item.Checked();
        if (l == null || l.Count != 1) { return; }

        _giveBack = l[0];
    }

    #endregion
}