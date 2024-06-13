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
using BlueDatabase;
using BlueDatabase.Enums;
using BlueDatabase.Interfaces;
using System.Collections.Generic;
using static BlueBasics.Converter;
using static BlueControls.ItemCollectionList.AbstractListItemExtension;

namespace BlueControls.Editoren;

public partial class FilterEditor : EditorEasy, IHasDatabase {

    #region Constructors

    public FilterEditor() {
        InitializeComponent();
    }

    #endregion

    #region Properties

    public Database? Database {
        get {
            if (ToEdit is not FilterItem f || f.IsDisposed) { return null; }
            return f.Database;
        }
    }

    #endregion

    #region Methods

    public override void Clear() {
        cbxFilterType.Text = string.Empty;
        cbxColumn.Text = string.Empty;
        txbFilterText.Text = string.Empty;
    }

    protected override bool Init(IEditable? toEdit) {
        if (toEdit is not FilterItem fi || fi.IsDisposed) { return false; }

        cbxFilterType.Text = ((int)fi.FilterType).ToString();

        cbxColumn.Text = fi.Column?.KeyName ?? string.Empty;

        txbFilterText.Text = fi.SearchValue.JoinWithCr();

        return true;
    }

    protected override void InitializeComponentDefaultValues() {
        cbxFilterType.ItemClear();
        cbxColumn.ItemClear();

        if (Database is not Database db) { return; }

        cbxFilterType.ItemAdd(ItemOf("Ohne Filter", "0"));
        cbxFilterType.ItemAdd(ItemOf("Ist (schreibungsneutral)", ((int)FilterType.Istgleich_GroßKleinEgal).ToString()));
        cbxFilterType.ItemAdd(ItemOf("Ist", ((int)FilterType.Istgleich).ToString()));
        cbxFilterType.ItemAdd(ItemOf("Trifft nie zu", ((int)FilterType.AlwaysFalse).ToString()));
        cbxFilterType.ItemAdd(ItemOf("Enthält (schreibungsneutral)", ((int)FilterType.Instr_GroßKleinEgal).ToString()));
        cbxFilterType.ItemAdd(ItemOf("Enthält", ((int)FilterType.Instr).ToString()));

        foreach (var thisColumn in db.Column) {
            if (thisColumn.Function.CanBeCheckedByRules()) {
                cbxColumn.ItemAdd(ItemOf(thisColumn));
            }
        }
    }

    private void cbxColumn_TextChanged(object sender, System.EventArgs e) {
        if (ToEdit is not FilterItem fi || fi.IsDisposed) { return; }

        fi.Column = fi.Database?.Column[cbxColumn.Text];
    }

    private void cbxFilterType_TextChanged(object sender, System.EventArgs e) {
        if (ToEdit is not FilterItem fi || fi.IsDisposed) { return; }

        fi.FilterType = (FilterType)(IntParse(cbxFilterType.Text));
    }

    private void txbFilterText_TextChanged(object sender, System.EventArgs e) {
        if (ToEdit is not FilterItem fi || fi.IsDisposed) { return; }

        fi.SearchValue = (new List<string>() { txbFilterText.Text }).AsReadOnly();
    }

    #endregion
}