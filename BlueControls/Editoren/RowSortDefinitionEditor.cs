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
using BlueControls.Editoren;
using BlueControls.ItemCollectionList;
using BlueTable;
using BlueTable.Interfaces;
using System;
using System.Linq;
using static BlueControls.ItemCollectionList.AbstractListItemExtension;

namespace BlueControls.Forms;

public partial class RowSortDefinitionEditor : EditorEasy, IHasTable {

    #region Constructors

    public RowSortDefinitionEditor() : base() => InitializeComponent();

    #endregion

    #region Properties

    public override Type? EditorFor => typeof(RowSortDefinition);

    public Table? Table => ToEdit is RowSortDefinition r ? r.Table : null;

    #endregion

    #region Methods

    public override void Clear() {
        lbxSortierSpalten.ItemClear();
        lbxSortierSpalten.Suggestions.Clear();
    }

    protected override void InitializeComponentDefaultValues() {
    }

    protected override bool SetValuesToFormula(IEditable? toEdit) {
        if (toEdit is not RowSortDefinition { } rsd) { return false; }

        btnSortRichtung.Checked = rsd.Reverse;

        foreach (var thisColumn in rsd.UsedColumns) {
            if (thisColumn is { IsDisposed: false }) {
                lbxSortierSpalten.AddAndCheck(ItemOf(thisColumn));
            }
        }

        lbxSortierSpalten.Suggestions.AddRange(ItemsOf(rsd.Table.Column, true));

        return true;
    }

    private void btnSortRichtung_MouseClick(object sender, System.Windows.Forms.MouseEventArgs e) => DoNewSort();

    private void DoNewSort() {
        if (Table is not { } tb) { return; }

        var colnam = lbxSortierSpalten.Items.Select(thisk => (ColumnItem)((ReadableListItem)thisk).Item).ToList();
        var nr = new RowSortDefinition(tb, colnam, btnSortRichtung.Checked);

        if (ToEdit?.Equals(nr) ?? false) { return; }

        ToEdit = nr;
    }

    private void lbxSortierSpalten_ItemAddedByClick(object sender, EventArgs.AbstractListItemEventArgs e) => DoNewSort();

    private void lbxSortierSpalten_RemoveClicked(object sender, EventArgs.AbstractListItemEventArgs e) => DoNewSort();

    private void lbxSortierSpalten_UpDownClicked(object sender, System.EventArgs e) => DoNewSort();

    #endregion
}