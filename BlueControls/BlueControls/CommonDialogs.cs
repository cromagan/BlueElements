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

using BlueControls.Controls;
using BlueControls.Enums;
using static BlueControls.ItemCollectionList.AbstractListItemExtension;
using BlueDatabase;

using System.Collections.Generic;
using BlueControls.ItemCollectionList;

#nullable enable

namespace BlueControls.Forms;

internal static class CommonDialogs {

    #region Methods

    public static Database? ChooseKnownDatabase(string caption, string mustbefreezed) {
        var l = Database.AllAvailableTables(mustbefreezed);

        var l2 = new List<AbstractListItem>();

        foreach (var thisd in l) {
            l2.Add(ItemOf(thisd));
        }

        var x = InputBoxListBoxStyle.Show(caption, l2, CheckBehavior.SingleSelection, null, AddType.None);

        if (x == null || x.Count != 1) { return null; }

        return Database.GetById(new ConnectionInfo(x[0], null, mustbefreezed), false, Table.Database_NeedPassword, true);
    }

    #endregion
}