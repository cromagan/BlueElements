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
using BlueBasics.Enums;
using BlueControls.Enums;
using BlueControls.EventArgs;
using BlueControls.ItemCollection;
using System.Drawing;

namespace BlueControls.Forms {

    public partial class PadEditor : PadEditorReadOnly {

        #region Constructors

        public PadEditor() : base() {
            // Dieser Aufruf ist für den Designer erforderlich.
            InitializeComponent();
            // Fügen Sie Initialisierungen nach dem InitializeComponent()-Aufruf hinzu.
            //InitWindow(false, "", -1, "");

            //if (FitWindowToBest) {
            //    if (System.Windows.Forms.Screen.AllScreens.Length == 1 || OpenOnScreen < 0) {
            //        var OpScNr = modAllgemein.PointOnScreenNr(System.Windows.Forms.Cursor.Position);
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
            PadDesign.Text = PadDesign.Item[0].Internal;
            Pad.Item.SheetStyle = Skin.StyleDB.Row[PadDesign.Text];
            cbxSchriftGröße.Item.Add("30%", "030");
            cbxSchriftGröße.Item.Add("40%", "040");
            cbxSchriftGröße.Item.Add("50%", "050");
            cbxSchriftGröße.Item.Add("60%", "060");
            cbxSchriftGröße.Item.Add("70%", "070");
            cbxSchriftGröße.Item.Add("80%", "080");
            cbxSchriftGröße.Item.Add("90%", "090");
            cbxSchriftGröße.Item.Add("100%", "100");
            cbxSchriftGröße.Item.Add("110%", "110");
            cbxSchriftGröße.Item.Add("120%", "120");
            cbxSchriftGröße.Item.Add("130%", "130");
            cbxSchriftGröße.Item.Add("140%", "140");
            cbxSchriftGröße.Item.Add("150%", "150");
            cbxSchriftGröße.Item.Sort();
            cbxSchriftGröße.Text = "100";
            if (Develop.IsHostRunning()) { TopMost = false; }
        }

        #endregion

        #region Methods

        public virtual void ItemChanged() {
            Pad.ZoomFit();
            Ribbon.SelectedIndex = 1;
            PadDesign.Text = Pad.Item.SheetStyle.CellFirstString();
            cbxSchriftGröße.Text = ((int)(Pad.Item.SheetStyleScale * 100)).ToString(Constants.Format_Integer3);
        }

        private void btnAddDimension_Click(object sender, System.EventArgs e) {
            DimensionPadItem b = new(new PointF(300, 300), new PointF(400, 300), 30);
            Pad.Item.Add(b);
        }

        private void btnAddImage_Click(object sender, System.EventArgs e) {
            BitmapPadItem b = new(QuickImage.Get(enImageCode.Fragezeichen), new Size(1000, 1000));
            Pad.Item.Add(b);
        }

        private void btnAddLine_Click(object sender, System.EventArgs e) {
            var P = Pad.MiddleOfVisiblesScreen();
            var w = (int)(300 / Pad.ZoomCurrent());
            LinePadItem b = new(PadStyles.Style_Standard, new Point(P.X - w, P.Y), new Point(P.X + w, P.Y));
            Pad.Item.Add(b);
        }

        private void btnAddPhsyik_Click(object sender, System.EventArgs e) {
            clsPhysicPadItem b = new(Pad.Item);
            //b.SetCoordinates(new RectangleF(100, 100, 300, 300));
            Pad.Item.Add(b);
        }

        private void btnAddSymbol_Click(object sender, System.EventArgs e) {
            SymbolPadItem b = new();
            b.SetCoordinates(new RectangleF(100, 100, 300, 300), true);
            Pad.Item.Add(b);
        }

        private void btnAddText_Click(object sender, System.EventArgs e) {
            TextPadItem b = new() {
                Text = string.Empty,
                Stil = PadStyles.Style_Standard
            };
            Pad.Item.Add(b);
            b.SetCoordinates(new RectangleF(10, 10, 200, 200), true);
        }

        private void btnAddUnterStufe_Click(object sender, System.EventArgs e) {
            ChildPadItem b = new();
            b.SetCoordinates(new RectangleF(100, 100, 300, 300), true);
            Pad.Item.Add(b);
        }

        private void btnArbeitsbreichSetup_Click(object sender, System.EventArgs e) => Pad.ShowWorkingAreaSetup();

        private void btnHintergrundFarbe_Click(object sender, System.EventArgs e) {
            ColorDia.Color = Pad.Item.BackColor;
            ColorDia.ShowDialog();
            Pad.Item.BackColor = ColorDia.Color;
            Pad.Invalidate();
        }

        private void btnKeinHintergrund_Click(object sender, System.EventArgs e) {
            Pad.Item.BackColor = Color.Transparent;
            Pad.Invalidate();
        }

        private void cbxSchriftGröße_ItemClicked(object sender, BasicListItemEventArgs e) => Pad.Item.SheetStyleScale = float.Parse(cbxSchriftGröße.Text) / 100f;

        private void ckbRaster_CheckedChanged(object sender, System.EventArgs e) => Pad.Item.SnapMode = ckbRaster.Checked ? enSnapMode.SnapToGrid : enSnapMode.Ohne;

        private void Pad_ClickedItemChanged(object sender, System.EventArgs e) {
            tabElementEigenschaften.Controls.Clear();
            if (Pad.LastClickedItem == null) { return; }
            var Flexis = Pad.LastClickedItem.GetStyleOptions();
            if (Flexis.Count == 0) { return; }
            var top = Skin.Padding;
            foreach (var ThisFlexi in Flexis) {
                tabElementEigenschaften.Controls.Add(ThisFlexi);
                ThisFlexi.DisabledReason = string.Empty;
                ThisFlexi.Left = Skin.Padding;
                ThisFlexi.Top = top;
                ThisFlexi.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
                top = top + Skin.Padding + ThisFlexi.Height;
                ThisFlexi.Width = tabElementEigenschaften.Width - (Skin.Padding * 4);
                //ThisFlexi.ButtonClicked += FlexiButtonClick;
            }
        }

        private void Pad_GotNewItemCollection(object sender, System.EventArgs e) {
            ckbRaster.Enabled = Pad.Item != null;
            txbRasterAnzeige.Enabled = Pad.Item != null;
            txbRasterFangen.Enabled = Pad.Item != null;

            if (Pad.Item != null) {
                ckbRaster.Checked = Pad.Item.SnapMode == enSnapMode.SnapToGrid;
                txbRasterAnzeige.Text = Pad.Item.GridShow.ToString(Constants.Format_Float2);
                txbRasterFangen.Text = Pad.Item.GridSnap.ToString(Constants.Format_Float2);
            }
        }

        private void PadDesign_ItemClicked(object sender, BasicListItemEventArgs e) => Pad.Item.SheetStyle = Skin.StyleDB.Row[e.Item.Internal];

        private void txbRasterAnzeige_TextChanged(object sender, System.EventArgs e) {
            if (!txbRasterAnzeige.Text.IsNumeral()) { return; }
            Pad.Item.GridShow = float.Parse(txbRasterAnzeige.Text);
        }

        private void txbRasterFangen_TextChanged(object sender, System.EventArgs e) {
            if (!txbRasterFangen.Text.IsNumeral()) { return; }
            Pad.Item.GridSnap = float.Parse(txbRasterFangen.Text);
        }

        #endregion
    }
}