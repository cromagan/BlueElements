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

using System;
using System.Drawing;
using System.Windows.Forms;
using BlueBasics;
using BlueBasics.Enums;
using BlueControls.Enums;
using BlueControls.EventArgs;
using BlueControls.ItemCollection;
using static BlueBasics.Converter;

namespace BlueControls.Forms;

public partial class PadEditor : PadEditorReadOnly {

    #region Constructors

    public PadEditor() : base() {
        // Dieser Aufruf ist f�r den Designer erforderlich.
        InitializeComponent();
        // F�gen Sie Initialisierungen nach dem InitializeComponent()-Aufruf hinzu.
        //InitWindow(false, string.Empty,-1, string.Empty);

        //if (FitWindowToBest) {
        //    if (System.Windows.Forms.Screen.AllScreens.Length == 1 || OpenOnScreen < 0) {
        //        var OpScNr = Generic.PointOnScreenNr(System.Windows.Forms.Cursor.Position);
        //        Width = (int)(System.Windows.Forms.Screen.AllScreens[OpScNr].WorkingArea.Width / 1.5);
        //        Height = (int)(System.Windows.Forms.Screen.AllScreens[OpScNr].WorkingArea.Height / 1.5);
        //        Left = (int)(System.Windows.Forms.Screen.AllScreens[OpScNr].WorkingArea.Left + ((System.Windows.Forms.Screen.AllScreens[OpScNr].WorkingArea.Width - Width) / 2.0));
        //        Top = (int)(System.Windows.Forms.Screen.AllScreens[OpScNr].WorkingArea.Top + ((System.Windows.Forms.Screen.AllScreens[OpScNr].WorkingArea.Height - Height) / 2.0));
        //    } else {
        //        Width = System.Windows.Forms.Screen.AllScreens[OpenOnScreen].WorkingArea.Width;
        //        Height = System.Windows.Forms.Screen.AllScreens[OpenOnScreen].WorkingArea.Height;
        //        Left = System.Windows.Forms.Screen.AllScreens[OpenOnScreen].WorkingArea.Left;
        //        Top = System.Windows.Forms.Screen.AllScreens[OpenOnScreen].WorkingArea.Top;
        //    }
        //}
        //if (!string.IsNullOrEmpty(WindowCaption)) {
        //    Text = WindowCaption;
        //}
        PadDesign.Item.Clear();
        PadDesign.Item.AddRange(Skin.AllStyles());
        PadDesign.Text = PadDesign.Item[0].KeyName;
        Pad.Item.SheetStyle = Skin.StyleDb.Row[PadDesign.Text];
        _ = cbxSchriftGr��e.Item.Add("30%", "030");
        _ = cbxSchriftGr��e.Item.Add("40%", "040");
        _ = cbxSchriftGr��e.Item.Add("50%", "050");
        _ = cbxSchriftGr��e.Item.Add("60%", "060");
        _ = cbxSchriftGr��e.Item.Add("70%", "070");
        _ = cbxSchriftGr��e.Item.Add("80%", "080");
        _ = cbxSchriftGr��e.Item.Add("90%", "090");
        _ = cbxSchriftGr��e.Item.Add("100%", "100");
        _ = cbxSchriftGr��e.Item.Add("110%", "110");
        _ = cbxSchriftGr��e.Item.Add("120%", "120");
        _ = cbxSchriftGr��e.Item.Add("130%", "130");
        _ = cbxSchriftGr��e.Item.Add("140%", "140");
        _ = cbxSchriftGr��e.Item.Add("150%", "150");
        //cbxSchriftGr��e.Item.Sort();
        cbxSchriftGr��e.Text = "100";
    }

    #endregion

    #region Methods

    public virtual void ItemChanged() {
        Pad.ZoomFit();
        //Ribbon.SelectedIndex = 1;
        PadDesign.Text = Pad.Item.SheetStyle.CellFirstString();
        cbxSchriftGr��e.Text = ((int)(Pad.Item.SheetStyleScale * 100)).ToString(Constants.Format_Integer3);
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
        ColorDia.Color = Pad.Item.BackColor;
        _ = ColorDia.ShowDialog();
        Pad.Item.BackColor = ColorDia.Color;
        Pad.Invalidate();
    }

    private void btnKeinHintergrund_Click(object sender, System.EventArgs e) {
        Pad.Item.BackColor = Color.Transparent;
        Pad.Invalidate();
    }

    private void cbxSchriftGr��e_ItemClicked(object sender, BasicListItemEventArgs e) => Pad.Item.SheetStyleScale = FloatParse(cbxSchriftGr��e.Text) / 100f;

    private void ckbRaster_CheckedChanged(object sender, System.EventArgs e) => Pad.Item.SnapMode = ckbRaster.Checked ? SnapMode.SnapToGrid : SnapMode.Ohne;

    private void Pad_ClickedItemChanged(object sender, System.EventArgs e) {
        // FALLS ein PadEditor doppelt offen ist, kann ein Control Element aber nur
        // einem Parent zugeordnet werden.
        // Deswegen m�ssen die Element einzigartig sein (also extra f�r das Men� generiert werden)
        // Und deswegen k�nnen sie auch disposed werden.

        foreach (var thisControl in tabElementEigenschaften.Controls) {
            if (thisControl is IDisposable d) { d?.Dispose(); }
        }
        tabElementEigenschaften.Controls.Clear();

        if (Pad.LastClickedItem == null) { return; }

        var flexis = Pad.LastClickedItem.GetStyleOptions();
        if (flexis.Count == 0) { return; }

        var top = Skin.Padding;
        foreach (var thisFlexi in flexis) {
            if (thisFlexi != null && !thisFlexi.IsDisposed) {
                tabElementEigenschaften.Controls.Add(thisFlexi);
                thisFlexi.Left = Skin.Padding;
                thisFlexi.Top = top;
                thisFlexi.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
                top = top + Skin.Padding + thisFlexi.Height;
                thisFlexi.Width = tabElementEigenschaften.Width - (Skin.Padding * 4);
            }
        }
    }

    private void Pad_GotNewItemCollection(object sender, System.EventArgs e) {
        ckbRaster.Enabled = Pad.Item != null;
        txbRasterAnzeige.Enabled = Pad.Item != null;
        txbRasterFangen.Enabled = Pad.Item != null;

        if (Pad.Item != null) {
            ckbRaster.Checked = Pad.Item.SnapMode == SnapMode.SnapToGrid;
            txbRasterAnzeige.Text = Pad.Item.GridShow.ToString(Constants.Format_Float2);
            txbRasterFangen.Text = Pad.Item.GridSnap.ToString(Constants.Format_Float2);
        }
    }

    private void PadDesign_ItemClicked(object sender, BasicListItemEventArgs e) => Pad.Item.SheetStyle = Skin.StyleDb.Row[e.Item.KeyName];

    private void txbRasterAnzeige_TextChanged(object sender, System.EventArgs e) {
        if (!txbRasterAnzeige.Text.IsNumeral()) { return; }
        Pad.Item.GridShow = FloatParse(txbRasterAnzeige.Text);
    }

    private void txbRasterFangen_TextChanged(object sender, System.EventArgs e) {
        if (!txbRasterFangen.Text.IsNumeral()) { return; }
        Pad.Item.GridSnap = FloatParse(txbRasterFangen.Text);
    }

    #endregion
}