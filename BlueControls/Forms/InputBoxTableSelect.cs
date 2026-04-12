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

using BlueTable.Classes;
using BlueTable.EventArgs;

namespace BlueControls.Forms;

public sealed partial class InputBoxTableSelect : DialogWithOkAndCancel {

    #region Fields

    private RowItem? _selectedRow;

    #endregion

    #region Constructors

    private InputBoxTableSelect(string txt, Table table, string buttonColumnName) : base(true, true) {
        InitializeComponent();
        Text = txt;
        _tableView.TableSet(table, string.Empty);
        _tableView.ContextMenuDefault = false;
        _tableView.EditButton = false;
        _buttonColumnName = buttonColumnName;
        Setup(txt, _tableView, 700);
    }

    #endregion

    #region Methods

    public static RowItem? Show(string txt, Table table, string buttonColumnName) {
        if (table is null or { IsDisposed: true }) { return null; }

        var x = new InputBoxTableSelect(txt, table, buttonColumnName);
        x.ShowDialog();
        var result = x._selectedRow;
        x.Dispose();
        return result;
    }

    protected override bool SetValue() {
        if (Canceled) { return true; }
        return true;
    }

    private void _tableView_CellClicked(object sender, CellEventArgs e) {
        if (e.Column?.KeyName == _buttonColumnName) {
            _selectedRow = e.Row;
            Ok();
        }
    }

    #endregion
}