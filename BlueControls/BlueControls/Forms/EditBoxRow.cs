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

using BlueDatabase;

namespace BlueControls.Forms;

public partial class EditBoxRow : DialogWithOkAndCancel {

    #region Constructors

    private EditBoxRow() : this(string.Empty, null, null) { }

    private EditBoxRow(string txt, ConnectedFormula.ConnectedFormula? cf, RowItem? row) : base(false, true) {
        InitializeComponent();

        formToEdit.SetData(cf, row?.Database, string.Empty);

        if (row != null && !row.IsDisposed) {
            if (cf == null) {
                formToEdit.GetConnectedFormulaFromDatabase(row?.Database);
            }

            formToEdit.SetData(row?.Database, row?.KeyName ?? string.Empty);
        }

        Setup(txt, formToEdit, formToEdit.MinimumSize.Width + 50);
    }

    #endregion

    #region Methods

    public static void Show(string txt, RowItem? row, bool isDialog) {
        EditBoxRow mb = new(txt, null, row);
        if (isDialog) {
            _ = mb.ShowDialog();
            mb.Dispose();
        } else {
            mb.Show();
        }
    }

    public static void Show(string txt, ConnectedFormula.ConnectedFormula? cf, RowItem? row, bool isDialog) {
        EditBoxRow mb = new(txt, cf, row);
        if (isDialog) {
            _ = mb.ShowDialog();
            mb.Dispose();
        } else {
            mb.Show();
        }
    }

    public static void Show(string txt, ConnectedFormula.ConnectedFormula? cf, bool isDialog) {
        EditBoxRow mb = new(txt, cf, null);
        if (isDialog) {
            _ = mb.ShowDialog();
            mb.Dispose();
        } else {
            mb.Show();
        }
    }

    protected override void SetValue(bool canceled) { }

    #endregion

    // Nix zu tun
}