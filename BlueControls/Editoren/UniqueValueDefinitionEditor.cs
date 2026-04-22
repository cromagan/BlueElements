// Authors:
// Christian Peter
//
// Copyright © 2026 Christian Peter
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

using BlueBasics.Interfaces;
using BlueControls.Classes.ItemCollectionList;
using BlueControls.Editoren;
using BlueTable.Classes;
using BlueTable.Interfaces;
using System;
using System.Linq;
using static BlueControls.Classes.ItemCollectionList.AbstractListItemExtension;

namespace BlueControls.Forms;

public partial class UniqueValueDefinitionEditor : EditorEasy, IHasTable {

    #region Constructors

    public UniqueValueDefinitionEditor() : base() => InitializeComponent();

    #endregion

    #region Properties

    public override Type? EditorFor => typeof(UniqueValueDefinition);

    public Table? Table => ToEdit is UniqueValueDefinition u ? u.Table : null;

    #endregion

    #region Methods

    public override void Clear() => lbxKeyColumns.ItemClear();

    protected override void InitializeComponentDefaultValues() { }

    protected override bool SetValuesToFormula(IEditable? toEdit) {
        if (toEdit is not UniqueValueDefinition { } uvd) { return false; }

        lbxKeyColumns.ItemAddRange(ItemsOf(uvd.Table.Column, true));

        foreach (var thisColumn in uvd.KeyColumns) {
            if (thisColumn is { IsDisposed: false }) {
                lbxKeyColumns.Check(thisColumn.KeyName);
            }
        }

        return true;
    }

    private void DoNewDefinition() {
        if (Table is not { } tb) { return; }

        var colnam = lbxKeyColumns.CheckedItems.Select(thisk => (ColumnItem)((ReadableListItem)thisk).Item).ToList();
        var nr = new UniqueValueDefinition(tb, colnam);

        if (ToEdit?.Equals(nr) ?? false) { return; }

        ToEdit = nr;
    }

    private void lbxKeyColumns_ItemCheckedChanged(object sender, System.EventArgs e) => DoNewDefinition();

    #endregion
}
