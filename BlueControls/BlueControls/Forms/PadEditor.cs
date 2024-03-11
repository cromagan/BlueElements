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
using BlueControls.Controls;
using BlueControls.Enums;
using BlueControls.EventArgs;
using BlueControls.ItemCollectionPad;
using BlueControls.ItemCollectionPad.Abstract;
using BlueControls.ItemCollectionPad.Temporär;
using System;
using System.Drawing;
using System.Globalization;
using System.Windows.Forms;
using static BlueBasics.Converter;

namespace BlueControls.Forms;

public partial class PadEditor : PadEditorReadOnly {

    #region Constructors

    public PadEditor() : base() {
        // Dieser Aufruf ist für den Designer erforderlich.
        InitializeComponent();
        // Fügen Sie Initialisierungen nach dem InitializeComponent()-Aufruf hinzu.

        PadDesign.Item.Clear();
        PadDesign.Item.AddRange(Skin.AllStyles());
        PadDesign.Text = PadDesign.Item[0].KeyName;

        if (Pad?.Item != null && Skin.StyleDb != null) {
            Pad.Item.SheetStyle = Skin.StyleDb.Row[PadDesign.Text];
        }

        _ = cbxSchriftGröße.Item.Add("30%", "030");
        _ = cbxSchriftGröße.Item.Add("40%", "040");
        _ = cbxSchriftGröße.Item.Add("50%", "050");
        _ = cbxSchriftGröße.Item.Add("60%", "060");
        _ = cbxSchriftGröße.Item.Add("70%", "070");
        _ = cbxSchriftGröße.Item.Add("80%", "080");
        _ = cbxSchriftGröße.Item.Add("90%", "090");
        _ = cbxSchriftGröße.Item.Add("100%", "100");
        _ = cbxSchriftGröße.Item.Add("110%", "110");
        _ = cbxSchriftGröße.Item.Add("120%", "120");
        _ = cbxSchriftGröße.Item.Add("130%", "130");
        _ = cbxSchriftGröße.Item.Add("140%", "140");
        _ = cbxSchriftGröße.Item.Add("150%", "150");
        cbxSchriftGröße.Text = "100";
    }

    #endregion

    #region Methods

    public virtual void ItemChanged() {
        Pad.ZoomFit();

        if (Pad?.Item?.SheetStyle != null) {
            PadDesign.Text = Pad.Item.SheetStyle.CellFirstString();
            cbxSchriftGröße.Text = ((int)(Pad.Item.SheetStyleScale * 100)).ToString(Constants.Format_Integer3);
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

    private void cbxSchriftGröße_ItemClicked(object sender, AbstractListItemEventArgs e) {
        if (Pad?.Item == null) { return; }
        Pad.Item.SheetStyleScale = FloatParse(cbxSchriftGröße.Text) / 100f;
    }

    private void ckbRaster_CheckedChanged(object sender, System.EventArgs e) {
        if (Pad?.Item == null) { return; }
        Pad.Item.SnapMode = ckbRaster.Checked ? SnapMode.SnapToGrid : SnapMode.Ohne;
    }

    private void LastClickedItem_DoUpdateSideOptionMenu(object sender, System.EventArgs e) {

        #region  SideMenu leeren

        foreach (var thisControl in tabElementEigenschaften.Controls) {
            if (thisControl is IDisposable d) { d.Dispose(); }
        }
        tabElementEigenschaften.Controls.Clear();

        #endregion

        if (Pad.LastClickedItem is not AbstractPadItem bpi) { return; }

        var stdWidth = tabElementEigenschaften.Width - (Skin.Padding * 4);

        var flexis = bpi.GetStyleOptions(stdWidth);
        if (flexis.Count == 0) { return; }

        //Rückwärts inserten

        //flexis.Insert(0, new FlexiControl()); // Trennlinie
        if (bpi is IErrorCheckable iec && !iec.IsOk()) {
            flexis.Insert(0, new FlexiControl("<Imagecode=Warnung|16> " + iec.ErrorReason(), stdWidth, false)); // Fehlergrund
            flexis.Insert(0, new FlexiControl("Achtung!", stdWidth, true));


        }

        flexis.Insert(0, new FlexiControl(bpi.Description, stdWidth, false)); // Beschreibung
        flexis.Insert(0, new FlexiControl("Beschreibung:", stdWidth, true));

        #region  SideMenu erstellen

        var top = Skin.Padding;
        foreach (var thisFlexi in flexis) {
            if (thisFlexi != null && !thisFlexi.IsDisposed) {
                tabElementEigenschaften.Controls.Add(thisFlexi);
                thisFlexi.Left = Skin.Padding;
                thisFlexi.Top = top;
                thisFlexi.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
                top = top + Skin.Padding + thisFlexi.Height;
                thisFlexi.Width = stdWidth;
            }
        }

        #endregion
    }

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

    private void Pad_GotNewItemCollection(object sender, System.EventArgs e) {
        ckbRaster.Enabled = Pad.Item != null;
        txbRasterAnzeige.Enabled = Pad.Item != null;
        txbRasterFangen.Enabled = Pad.Item != null;

        if (Pad.Item != null) {
            ckbRaster.Checked = Pad.Item.SnapMode == SnapMode.SnapToGrid;
            txbRasterAnzeige.Text = Pad.Item.GridShow.ToString(Constants.Format_Float2, CultureInfo.InvariantCulture);
            txbRasterFangen.Text = Pad.Item.GridSnap.ToString(Constants.Format_Float2, CultureInfo.InvariantCulture);
        }
    }

    private void PadDesign_ItemClicked(object sender, AbstractListItemEventArgs e) => Pad.Item.SheetStyle = Skin.StyleDb.Row[e.Item.KeyName];

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