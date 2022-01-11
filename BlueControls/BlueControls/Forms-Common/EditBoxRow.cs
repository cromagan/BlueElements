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

using BlueDatabase;

namespace BlueControls.Forms {

    public partial class EditBoxRow : DialogWithOkAndCancel {

        #region Constructors

        private EditBoxRow() : this(string.Empty, null) { }

        private EditBoxRow(string txt, RowItem row) : base(false, true) {
            InitializeComponent();

            formToEdit.Database = row.Database;
            formToEdit.ShowingRowKey = row.Key;

            Setup(txt, formToEdit, formToEdit.MinimumSize.Width + 50);
        }

        #endregion

        #region Methods

        public static void Show(string txt, RowItem row, bool isDialog) {
            EditBoxRow MB = new(txt, row);
            if (isDialog) {
                MB.ShowDialog();
            } else {
                MB.Show();
            }
        }

        protected override void SetValue(bool canceled) { }

        #endregion

        // Nix zu tun
    }
}