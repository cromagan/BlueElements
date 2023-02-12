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

using System;
using System.Collections.Generic;
using System.Windows.Forms;
using BlueBasics;

namespace BlueControls.Forms;

public partial class PadEditorReadOnly : FormWithStatusBar {

    #region Constructors

    public PadEditorReadOnly() : base() => InitializeComponent();

    #endregion

    #region Methods

    private void btnAlsBildSpeichern_Click(object sender, System.EventArgs e) => Pad.OpenSaveDialog(string.Empty);

    private void btnDruckerDialog_Click(object sender, System.EventArgs e) => Pad.Print();

    private void btnPageSetup_Click(object sender, System.EventArgs e) => Pad.ShowPrinterPageSetup();

    private void btnVorschau_Click(object sender, System.EventArgs e) => Pad.ShowPrintPreview();

    private void btnVorschauModus_CheckedChanged(object sender, System.EventArgs e) => Pad.ShowInPrintMode = btnVorschauModus.Checked;

    private void btnZoom11_Click(object sender, System.EventArgs e) => Pad.Zoom = 1f;

    private void btnZoomFit_Click(object sender, System.EventArgs e) => Pad.ZoomFit();

    private void DoPages() {
        if (InvokeRequired) {
            _ = Invoke(new Action(DoPages));
            return;
        }

        var x = new List<string>();

        if (Pad?.Item != null) { x.AddRange(Pad.Item.AllPages()); }

        if (Pad != null) { _ = x.AddIfNotExists(Pad.CurrentPage); }

        TabPage? later = null;

        if (x.Count == 1 && string.IsNullOrEmpty(x[0])) { x.Clear(); }

        if (x.Count > 0) {
            tabSeiten.Visible = true;

            foreach (var thisTab in tabSeiten.TabPages) {
                var tb = (TabPage)thisTab;

                if (!x.Contains(tb.Text)) {
                    tabSeiten.TabPages.Remove(tb);
                    DoPages();
                    return;
                }

                _ = x.Remove(tb.Text);
                if (Pad != null && tb.Text == Pad.CurrentPage) { later = tb; }
            }

            foreach (var thisn in x) {
                var t = new TabPage(thisn) {
                    Name = "Seite_" + thisn
                };
                tabSeiten.TabPages.Add(t);

                if (Pad != null && t.Text == Pad.CurrentPage) { later = t; }
            }
        } else {
            tabSeiten.Visible = false;
            tabSeiten.TabPages.Clear();
        }

        tabSeiten.SelectedTab = later;
    }

    private void Pad_Changed(object sender, System.EventArgs e) => DoPages();

    private void Pad_DrawModChanged(object sender, System.EventArgs e) {
        btnVorschauModus.Checked = Pad.ShowInPrintMode;

        DoPages();
    }

    private void Pad_GotNewItemCollection(object sender, System.EventArgs e) {
        btnVorschauModus.Checked = Pad.ShowInPrintMode;

        DoPages();
    }

    private void Pad_MouseUp(object sender, MouseEventArgs e) {
        if (btnZoomIn.Checked) { Pad.ZoomIn(e); }
        if (btnZoomOut.Checked) { Pad.ZoomOut(e); }
    }

    private void tabSeiten_Selected(object sender, TabControlEventArgs e) {
        var s = string.Empty;

        if (tabSeiten.SelectedTab != null) {
            s = tabSeiten.SelectedTab.Text;
        }

        Pad.CurrentPage = s;
    }

    #endregion
}