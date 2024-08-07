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
using BlueBasics.Enums;
using BlueBasics.Interfaces;
using BlueControls.Enums;
using BlueControls.EventArgs;
using BlueControls.ItemCollectionPad;
using BlueControls.ItemCollectionPad.Tempor�r;
using System.Drawing;
using static BlueBasics.Converter;
using static BlueControls.ItemCollectionList.AbstractListItemExtension;

namespace BlueControls.Forms;

public partial class PadEditor : PadEditorReadOnly {

    #region Constructors

    protected PadEditor() : base() {
        // Dieser Aufruf ist f�r den Designer erforderlich.
        InitializeComponent();
        // F�gen Sie Initialisierungen nach dem InitializeComponent()-Aufruf hinzu.

        PadDesign.ItemClear();
        PadDesign.ItemAddRange(ItemsOf(Skin.AllStyles()));
        PadDesign.Text = PadDesign[0]?.KeyName ?? string.Empty;

        if (Pad?.Item != null && Skin.StyleDb != null) {
            Pad.Item.SheetStyle = Skin.StyleDb.Row[PadDesign.Text];
        }

        cbxSchriftGr��e.ItemAdd(ItemOf("30%", "030"));
        cbxSchriftGr��e.ItemAdd(ItemOf("40%", "040"));
        cbxSchriftGr��e.ItemAdd(ItemOf("50%", "050"));
        cbxSchriftGr��e.ItemAdd(ItemOf("60%", "060"));
        cbxSchriftGr��e.ItemAdd(ItemOf("70%", "070"));
        cbxSchriftGr��e.ItemAdd(ItemOf("80%", "080"));
        cbxSchriftGr��e.ItemAdd(ItemOf("90%", "090"));
        cbxSchriftGr��e.ItemAdd(ItemOf("100%", "100"));
        cbxSchriftGr��e.ItemAdd(ItemOf("110%", "110"));
        cbxSchriftGr��e.ItemAdd(ItemOf("120%", "120"));
        cbxSchriftGr��e.ItemAdd(ItemOf("130%", "130"));
        cbxSchriftGr��e.ItemAdd(ItemOf("140%", "140"));
        cbxSchriftGr��e.ItemAdd(ItemOf("150%", "150"));
        cbxSchriftGr��e.Text = "100";
    }

    #endregion

    #region Methods

    public void ItemChanged() {
        Pad.ZoomFit();

        if (Pad?.Item?.SheetStyle != null) {
            PadDesign.Text = Pad.Item.SheetStyle.CellFirstString();
            cbxSchriftGr��e.Text = ((int)(Pad.Item.SheetStyleScale * 100)).ToStringInt3();
        }
    }

    private void btnAddDimension_Click(object sender, System.EventArgs e) {
        DimensionPadItem b = new(new PointF(300, 300), new PointF(400, 300), 30);
        Pad.AddCentered(b);
    }

    private void btnAddImage_Click(object sender, System.EventArgs e) {
        BitmapPadItem b = new(QuickImage.Get(ImageCode.Fragezeichen), new Size(1000, 1000));
        Pad.AddCentered(b);
    }

    private void btnAddLine_Click(object sender, System.EventArgs e) {
        var p = Pad.MiddleOfVisiblesScreen();
        var w = (int)(300 / Pad.Zoom);
        LinePadItem b = new(PadStyles.Style_Standard, p with { X = p.X - w }, p with { X = p.X + w });
        Pad.AddCentered(b);
    }

    private void btnAddPhsyik_Click(object sender, System.EventArgs e) {
        PhysicPadItem b = new();
        //b.SetCoordinates(new RectangleF(100, 100, 300, 300));
        Pad.AddCentered(b);
    }

    private void btnAddSymbol_Click(object sender, System.EventArgs e) {
        SymbolPadItem b = new();
        b.SetCoordinates(new RectangleF(100, 100, 300, 300), true);
        Pad.AddCentered(b);
    }

    private void btnAddText_Click(object sender, System.EventArgs e) {
        TextPadItem b = new() {
            Text = string.Empty,
            Stil = PadStyles.Style_Standard
        };
        Pad.AddCentered(b);
        b.SetCoordinates(new RectangleF(10, 10, 200, 200), true);
    }

    private void btnAddUnterStufe_Click(object sender, System.EventArgs e) {
        ChildPadItem b = new();
        b.SetCoordinates(new RectangleF(100, 100, 300, 300), true);
        Pad.AddCentered(b);
    }

    private void btnArbeitsbreichSetup_Click(object sender, System.EventArgs e) => Pad.ShowWorkingAreaSetup();

    private void btnHintergrundFarbe_Click(object sender, System.EventArgs e) {
        if (Pad?.Item == null) { return; }

        ColorDia.Color = Pad.Item.BackColor;
        _ = ColorDia.ShowDialog();
        Pad.Item.BackColor = ColorDia.Color;
        Pad.Invalidate();
    }

    private void btnKeinHintergrund_Click(object sender, System.EventArgs e) {
        if (Pad?.Item == null) { return; }

        Pad.Item.BackColor = Color.Transparent;
        Pad.Invalidate();
    }

    private void cbxSchriftGr��e_ItemClicked(object sender, AbstractListItemEventArgs e) {
        if (Pad?.Item == null) { return; }
        Pad.Item.SheetStyleScale = FloatParse(cbxSchriftGr��e.Text) / 100f;
    }

    private void ckbRaster_CheckedChanged(object sender, System.EventArgs e) {
        if (Pad?.Item == null) { return; }
        Pad.Item.SnapMode = ckbRaster.Checked ? SnapMode.SnapToGrid : SnapMode.Ohne;
    }

    private void LastClickedItem_DoUpdateSideOptionMenu(object sender, System.EventArgs e) => Pad.LastClickedItem.DoForm(tabElementEigenschaften.Controls, tabElementEigenschaften.Width);

    private void Pad_ClickedItemChanged(object sender, System.EventArgs e) {
        // FALLS ein PadEditor doppelt offen ist, kann ein Control Element aber nur
        // einem Parent zugeordnet werden.
        // Deswegen m�ssen die Element einzigartig sein (also extra f�r das Men� generiert werden)
        // Und deswegen k�nnen sie auch disposed werden.

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

    private void Pad_GotNewItemCollection(object sender, System.EventArgs e) {
        ckbRaster.Enabled = Pad.Item != null;
        txbRasterAnzeige.Enabled = Pad.Item != null;
        txbRasterFangen.Enabled = Pad.Item != null;

        if (Pad.Item != null) {
            ckbRaster.Checked = Pad.Item.SnapMode == SnapMode.SnapToGrid;
            txbRasterAnzeige.Text = Pad.Item.GridShow.ToStringFloat2();
            txbRasterFangen.Text = Pad.Item.GridSnap.ToStringFloat2();
        }
    }

    private void PadDesign_ItemClicked(object sender, AbstractListItemEventArgs e) {
        if (Pad?.Item != null && Skin.StyleDb?.Row != null) {
            Pad.Item.SheetStyle = Skin.StyleDb.Row[e.Item.KeyName];
        }
    }

    private void txbRasterAnzeige_TextChanged(object sender, System.EventArgs e) {
        if (!txbRasterAnzeige.Text.IsNumeral()) { return; }
        if (!txbRasterAnzeige.Visible) { return; }
        if (Pad?.Item == null) { return; }
        Pad.Item.GridShow = FloatParse(txbRasterAnzeige.Text);
    }

    private void txbRasterFangen_TextChanged(object sender, System.EventArgs e) {
        if (!txbRasterFangen.Text.IsNumeral()) { return; }
        if (!txbRasterFangen.Visible) { return; }
        if (Pad?.Item == null) { return; }
        Pad.Item.GridSnap = FloatParse(txbRasterFangen.Text);
    }

    #endregion
}