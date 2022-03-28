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

namespace BlueControls.Forms {

    public partial class EditBoxRow_NEW : DialogWithOkAndCancel {

        #region Constructors

        private EditBoxRow_NEW() : this(string.Empty, null) { }

        private EditBoxRow_NEW(string txt, ConnectedFormula.ConnectedFormula? cf) : base(false, true) {
            InitializeComponent();

            formToEdit.ConnectedFormula = cf;

            Setup(txt, formToEdit, formToEdit.MinimumSize.Width + 50);
        }

        #endregion

        #region Methods

        public static void Show(string txt, ConnectedFormula.ConnectedFormula? cf, bool isDialog) {
            EditBoxRow_NEW MB = new(txt, cf);
            if (isDialog) {
                MB.ShowDialog();
                MB.Dispose();
            } else {
                MB.Show();
            }
        }

        protected override void SetValue(bool canceled) { }

        #endregion

        // Nix zu tun
    }
}