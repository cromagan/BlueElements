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

using System.Drawing;
using BlueBasics;
using BlueControls.BlueDatabaseDialogs;
using BlueControls.EventArgs;
using BlueControls.Forms;
using BlueDatabase;
using BlueDatabase.Enums;

namespace BlueControls.Classes_Editor;

internal sealed partial class FilterItem_Editor : AbstractClassEditor<FilterItem> //System.Windows.Forms.UserControl // :
{
    #region Fields

    private AutoFilter? _autofilter;

    #endregion

    #region Constructors

    public FilterItem_Editor() : base() => InitializeComponent();

    #endregion

    #region Methods

    protected override void DisableAndClearFormula() {
        Enabled = false;
        cbxColumns.Text = string.Empty;
    }

    protected override void EnabledAndFillFormula(FilterItem data) {
        if (data == null) { return; }

        Enabled = true;
        if (data?.Column == null) {
            cbxColumns.Text = string.Empty;
            return;
        }
        cbxColumns.Text = data.Column.Name;
    }

    protected override void PrepaireFormula(FilterItem data) {
        if (data?.Database?.Column != null) { cbxColumns.Item?.AddRange(data.Database.Column, true); }
    }

    private void AutoFilter_FilterComand(object sender, FilterComandEventArgs e) {
        if (e.Comand != "Filter") {
            Notification.Show("Diese Funktion wird nicht unterstützt,<br>abbruch.");
            return;
        }

        if (Item == null || e.Filter == null) { return; }

        Item.FilterType = e.Filter.FilterType;
        Item.SearchValue.Clear();
        Item.SearchValue.AddRange(e.Filter.SearchValue);
    }

    private void btnFilterWahl_Click(object sender, System.EventArgs e) {
        if (Item?.Database == null || Item.Database.IsDisposed) { return; }

        var c = Item.Database.Column[cbxColumns.Text];
        if (c == null || !c.AutoFilterSymbolPossible()) { return; }
        FilterCollection tmpfc = new(Item.Database);
        if (Item.FilterType != FilterType.KeinFilter) { tmpfc.Add(Item); }
        _autofilter = new AutoFilter(c, tmpfc, null);
        var p = btnFilterWahl.PointToScreen(Point.Empty);
        _autofilter.Position_LocateToPosition(p with { Y = p.Y + btnFilterWahl.Height });
        _autofilter.Show();
        _autofilter.FilterComand += AutoFilter_FilterComand;
        Develop.Debugprint_BackgroundThread();
    }

    private void cbxColumns_TextChanged(object sender, System.EventArgs e) {
        var c = Item.Database.Column[cbxColumns.Text];
        btnFilterWahl.Enabled = c == null || c.AutoFilterSymbolPossible() || true;
        Item.Column = c;
        Item.FilterType = FilterType.KeinFilter;
        Item.SearchValue.Clear();
    }

    #endregion
}