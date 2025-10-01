// Authors:
// Christian Peter
//
// Copyright (c) 2025 Christian Peter
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

using BlueControls;
using BlueControls.Forms;
using BlueControls.Interfaces;
using BluePaint;
using System;
using static BlueBasics.Develop;

namespace BeCreative {

    public partial class Start : Form, IIsStandalone {

        #region Constructors

        public Start() {
            InitializeComponent();
        }

        #endregion

        #region Methods

        private void btnBildEditor_Click(object sender, EventArgs e) => DoForm(new MainWindow(true));

        private void btnDatenbank_Click(object sender, EventArgs e) => DoForm(new TableViewForm(null, true, true, true));

        private void btnFormular_Click(object sender, EventArgs e) => DoForm(new ConnectedFormulaEditor());

        private void btnFormularAnsicht_Click(object sender, EventArgs e) => DoForm(new ConnectedFormulaForm());

        private void btnHierachie_Click(object sender, EventArgs e) => DoForm(new RelationDiagram(null));

        private void btnLayout_Click(object sender, EventArgs e) => DoForm(new PadEditorWithFileAccess());

        private void DoForm(System.Windows.Forms.Form frm) {
            if (frm.IsDisposed) {
                DebugPrint("Fenster verworfen!");
                return;
            }
            FormManager.RegisterForm(frm);
            frm.Show();
            Close();
            frm.BringToFront();
        }

        #endregion

        private void btnTextEditor_Click(object sender, EventArgs e) {

        }
    }
}