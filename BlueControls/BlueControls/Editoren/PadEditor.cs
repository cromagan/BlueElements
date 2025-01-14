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

#nullable enable

using System.Drawing;
using System.Windows.Forms;
using BlueBasics;
using BlueBasics.Interfaces;
using BlueControls.Enums;
using BlueControls.EventArgs;
using static BlueBasics.Converter;
using static BlueControls.ItemCollectionList.AbstractListItemExtension;

namespace BlueControls.Forms;

public partial class PadEditor : FormWithStatusBar {

    #region Constructors

    public PadEditor() : base() {
        // Dieser Aufruf ist für den Designer erforderlich.
        InitializeComponent();
        // Fügen Sie Initialisierungen nach dem InitializeComponent()-Aufruf hinzu.

        PadDesign.ItemClear();
        PadDesign.ItemAddRange(ItemsOf(Skin.AllStyles()));
        PadDesign.Text = PadDesign[0]?.KeyName ?? string.Empty;

        if (Pad?.Items != null && Skin.StyleDb != null) {
            Pad.Items.SheetStyle = PadDesign.Text;
        }
    }

    #endregion

    #region Methods

    protected virtual void Pad_GotNewItemCollection(object sender, System.EventArgs e) {
        btnVorschauModus.Checked = Pad.ShowInPrintMode;

        ckbRaster.Enabled = Pad.Items != null;
        txbRasterAnzeige.Enabled = Pad.Items != null;
        txbRasterFangen.Enabled = Pad.Items != null;

        if (Pad.Items != null) {
            ckbRaster.Checked = Pad.Items.SnapMode == SnapMode.SnapToGrid;
            txbRasterAnzeige.Text = Pad.Items.GridShow.ToStringFloat2();
            txbRasterFangen.Text = Pad.Items.GridSnap.ToStringFloat2();
            PadDesign.Text = Pad.Items.SheetStyle;
        }
    }

    private void btnAlsBildSpeichern_Click(object sender, System.EventArgs e) => Pad.OpenSaveDialog(string.Empty);

    private void btnArbeitsbreichSetup_Click(object sender, System.EventArgs e) => Pad.ShowWorkingAreaSetup();

    private void btnDruckerDialog_Click(object sender, System.EventArgs e) => Pad.Print();

    private void btnHintergrundFarbe_Click(object sender, System.EventArgs e) {
        if (Pad?.Items == null) { return; }

        ColorDia.Color = Pad.Items.BackColor;
        _ = ColorDia.ShowDialog();
        Pad.Items.BackColor = ColorDia.Color;
        Pad.Invalidate();
    }

    private void btnKeinHintergrund_Click(object sender, System.EventArgs e) {
        if (Pad?.Items == null) { return; }

        Pad.Items.BackColor = Color.Transparent;
        Pad.Invalidate();
    }

    private void btnNoArea_Click(object sender, System.EventArgs e) {
        if (Pad?.Items == null) { return; }

        Pad.Items.Endless = true;
        Pad.Invalidate();
    }

    private void btnPageSetup_Click(object sender, System.EventArgs e) => Pad.ShowPrinterPageSetup();

    private void btnVorschau_Click(object sender, System.EventArgs e) => Pad.ShowPrintPreview();

    private void btnVorschauModus_CheckedChanged(object sender, System.EventArgs e) => Pad.ShowInPrintMode = btnVorschauModus.Checked;

    private void btnZoom11_Click(object sender, System.EventArgs e) => Pad.Zoom = 1f;

    private void btnZoomFit_Click(object sender, System.EventArgs e) => Pad.ZoomFit();

    private void ckbRaster_CheckedChanged(object sender, System.EventArgs e) {
        if (Pad?.Items == null) { return; }
        Pad.Items.SnapMode = ckbRaster.Checked ? SnapMode.SnapToGrid : SnapMode.Ohne;
    }

    private void LastClickedItem_DoUpdateSideOptionMenu(object sender, System.EventArgs e) => Pad.LastClickedItem.DoForm(tabElementEigenschaften);

    private void Pad_ClickedItemChanged(object sender, System.EventArgs e) {
        // FALLS ein PadEditor doppelt offen ist, kann ein Control Element aber nur
        // einem Parent zugeordnet werden.
        // Deswegen müssen die Element einzigartig sein (also extra für das Menü generiert werden)
        // Und deswegen können sie auch disposed werden.

        if (Pad.LastClickedItem != null) {
            Pad.LastClickedItem.DoUpdateSideOptionMenu += LastClickedItem_DoUpdateSideOptionMenu;
        }

        LastClickedItem_DoUpdateSideOptionMenu(this, System.EventArgs.Empty);
    }

    private void Pad_ClickedItemChanging(object sender, System.EventArgs e) {
        if (Pad.LastClickedItem != null) {
            Pad.LastClickedItem.DoUpdateSideOptionMenu -= LastClickedItem_DoUpdateSideOptionMenu;
        }
    }

    private void Pad_DrawModChanged(object sender, System.EventArgs e) => btnVorschauModus.Checked = Pad.ShowInPrintMode;

    private void Pad_MouseUp(object sender, MouseEventArgs e) {
        if (btnZoomIn.Checked) { Pad.ZoomIn(e); }
        if (btnZoomOut.Checked) { Pad.ZoomOut(e); }
    }

    private void PadDesign_ItemClicked(object sender, AbstractListItemEventArgs e) {
        if (Pad?.Items != null && Skin.StyleDb?.Row != null) {
            Pad.Items.SheetStyle = e.Item.KeyName;
        }
    }

    private void txbRasterAnzeige_TextChanged(object sender, System.EventArgs e) {
        if (!txbRasterAnzeige.Text.IsNumeral()) { return; }
        if (!txbRasterAnzeige.Visible) { return; }
        if (Pad?.Items == null) { return; }
        Pad.Items.GridShow = FloatParse(txbRasterAnzeige.Text);
    }

    private void txbRasterFangen_TextChanged(object sender, System.EventArgs e) {
        if (!txbRasterFangen.Text.IsNumeral()) { return; }
        if (!txbRasterFangen.Visible) { return; }
        if (Pad?.Items == null) { return; }
        Pad.Items.GridSnap = FloatParse(txbRasterFangen.Text);
    }

    #endregion
}