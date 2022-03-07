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

using BlueBasics;

namespace BlueControls.Forms {

    public partial class PadEditorReadOnly : Form {

        #region Constructors

        public PadEditorReadOnly() : base() {
            // Dieser Aufruf ist für den Designer erforderlich.
            InitializeComponent();
            // Fügen Sie Initialisierungen nach dem InitializeComponent()-Aufruf hinzu.

            if (Develop.IsHostRunning()) { TopMost = false; }
        }

        #endregion

        #region Methods

        private void btnAlsBildSpeichern_Click(object sender, System.EventArgs e) => Pad.OpenSaveDialog(string.Empty);

        private void btnDruckerDialog_Click(object sender, System.EventArgs e) => Pad.Print();

        private void btnPageSetup_Click(object sender, System.EventArgs e) => Pad.ShowPrinterPageSetup();

        private void btnVorschau_Click(object sender, System.EventArgs e) => Pad.ShowPrintPreview();

        private void btnVorschauModus_CheckedChanged(object sender, System.EventArgs e) => Pad.ShowInPrintMode = btnVorschauModus.Checked;

        private void btnZoom11_Click(object sender, System.EventArgs e) => Pad.Zoom100();

        private void btnZoomFit_Click(object sender, System.EventArgs e) => Pad.ZoomFit();

        private void Pad_MouseUp(object sender, System.Windows.Forms.MouseEventArgs e) {
            if (btnZoomIn.Checked) { Pad.ZoomIn(e); }
            if (btnZoomOut.Checked) { Pad.ZoomOut(e); }
        }

        private void Pad_PreviewModChanged(object sender, System.EventArgs e) => btnVorschauModus.Checked = Pad.ShowInPrintMode;

        #endregion
    }
}