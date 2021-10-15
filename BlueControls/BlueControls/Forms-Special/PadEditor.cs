// Authors:
// Christian Peter
//
// Copyright (c) 2021 Christian Peter
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
using System.IO;
using static BlueBasics.FileOperations;

namespace BlueControls.Forms {

    public partial class PadEditor : Form {

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

        /// <summary>
        /// löscht den kompletten Inhalt des Pads auch die ID und setzt es auf Disabled
        /// </summary>
        public void DisablePad() {
            Pad.Item = new ItemCollectionPad();
            Pad.Enabled = false;
        }

        public virtual void ItemChanged() {
            Pad.ZoomFit();
            Ribbon.SelectedIndex = 1;
            PadDesign.Text = Pad.Item.SheetStyle.CellFirstString();
            cbxSchriftGröße.Text = ((int)(Pad.Item.SheetStyleScale * 100)).ToString(Constants.Format_Integer3);
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="useThisID">Wenn das Blatt bereits eine Id hat, muss die Id verwendet werden. Wird das Feld leer gelassen, wird die beinhaltete Id benutzt.</param>
        public void LoadFile(string fileName, string useThisID) {
            var t = File.ReadAllText(fileName, Constants.Win1252);
            LoadFromString(t, useThisID);
            btnLastFiles.AddFileName(fileName, fileName.FileNameWithSuffix());
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="useThisID">Wenn das Blatt bereits eine Id hat, muss die Id verwendet werden. Wird das Feld leer gelassen, wird die beinhaltete Id benutzt.</param>
        public void LoadFromString(string data, string useThisID) {
            Pad.Enabled = true;
            Pad.Item = new ItemCollectionPad(data, useThisID);
            ItemChanged();
        }

        private void btnAddDimension_Click(object sender, System.EventArgs e) {
            DimensionPadItem b = new(Pad.Item, new PointF(300, 300), new PointF(400, 300), 30);
            Pad.Item.Add(b);
        }

        private void btnAddImage_Click(object sender, System.EventArgs e) {
            BitmapPadItem b = new(Pad.Item, QuickImage.Get(enImageCode.Fragezeichen).BMP, new Size(1000, 1000));
            Pad.Item.Add(b);
        }

        private void btnAddLine_Click(object sender, System.EventArgs e) {
            var P = Pad.MiddleOfVisiblesScreen();
            var w = (int)(300 / Pad.ZoomCurrent());
            LinePadItem b = new(Pad.Item, PadStyles.Style_Standard, new Point(P.X - w, P.Y), new Point(P.X + w, P.Y));
            Pad.Item.Add(b);
        }

        private void btnAddPhsyik_Click(object sender, System.EventArgs e) {
            clsPhysicPadItem b = new(Pad.Item);
            //b.SetCoordinates(new RectangleM(100, 100, 300, 300));
            Pad.Item.Add(b);
        }

        private void btnAddSymbol_Click(object sender, System.EventArgs e) {
            SymbolPadItem b = new(Pad.Item);
            b.SetCoordinates(new RectangleM(100, 100, 300, 300), true);
            Pad.Item.Add(b);
        }

        private void btnAddText_Click(object sender, System.EventArgs e) {
            TextPadItem b = new(Pad.Item) {
                Text = string.Empty,
                Stil = PadStyles.Style_Standard
            };
            Pad.Item.Add(b);
            b.SetCoordinates(new RectangleM(10, 10, 200, 200), true);
        }

        private void btnAddUnterStufe_Click(object sender, System.EventArgs e) {
            ChildPadItem b = new(Pad.Item);
            b.SetCoordinates(new RectangleM(100, 100, 300, 300), true);
            Pad.Item.Add(b);
        }

        private void btnAlsBildSpeichern_Click(object sender, System.EventArgs e) => Pad.OpenSaveDialog(string.Empty);

        private void btnArbeitsbreichSetup_Click(object sender, System.EventArgs e) => Pad.ShowWorkingAreaSetup();

        private void btnDruckerDialog_Click(object sender, System.EventArgs e) => Pad.Print();

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

        private void btnLastFiles_ItemClicked(object sender, BasicListItemEventArgs e) => LoadFile(e.Item.Internal, string.Empty);

        private void btnNeu_Click(object sender, System.EventArgs e) {
            Pad.Item.Clear();
            Pad.ZoomFit();
            Ribbon.SelectedIndex = 1;
        }

        private void btnOeffnen_Click(object sender, System.EventArgs e) {
            LoadTab.Tag = sender;
            LoadTab.ShowDialog();
        }

        private void btnPageSetup_Click(object sender, System.EventArgs e) => Pad.ShowPrinterPageSetup();

        private void btnSpeichern_Click(object sender, System.EventArgs e) => SaveTab.ShowDialog();

        private void btnVorschau_Click(object sender, System.EventArgs e) => Pad.ShowPrintPreview();

        private void btnVorschauModus_CheckedChanged(object sender, System.EventArgs e) => Pad.ShowInPrintMode = btnVorschauModus.Checked;

        private void btnZoom11_Click(object sender, System.EventArgs e) => Pad.Zoom100();

        private void btnZoomFit_Click(object sender, System.EventArgs e) => Pad.ZoomFit();

        private void cbxSchriftGröße_ItemClicked(object sender, BasicListItemEventArgs e) => Pad.Item.SheetStyleScale = double.Parse(cbxSchriftGröße.Text) / 100d;

        private void ckbRaster_CheckedChanged(object sender, System.EventArgs e) => Pad.Item.SnapMode = ckbRaster.Checked ? enSnapMode.SnapToGrid : enSnapMode.Ohne;

        private void LoadTab_FileOk(object sender, System.ComponentModel.CancelEventArgs e) {
            if (sender == btnOeffnen) {
                LoadFile(LoadTab.FileName, LoadTab.FileName);
            } else {
                // Kann nur der Import-Knopf sein
                LoadFile(LoadTab.FileName, string.Empty);
            }
            Ribbon.SelectedIndex = 1;
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

        private void Pad_MouseUp(object sender, System.Windows.Forms.MouseEventArgs e) {
            if (btnZoomIn.Checked) { Pad.ZoomIn(e); }
            if (btnZoomOut.Checked) { Pad.ZoomOut(e); }
        }

        private void Pad_PreviewModChanged(object sender, System.EventArgs e) => btnVorschauModus.Checked = Pad.ShowInPrintMode;

        private void PadDesign_ItemClicked(object sender, BasicListItemEventArgs e) => Pad.Item.SheetStyle = Skin.StyleDB.Row[e.Item.Internal];

        private void SaveTab_FileOk(object sender, System.ComponentModel.CancelEventArgs e) {
            var t = Pad.Item.ToString();
            WriteAllText(SaveTab.FileName, t, Constants.Win1252, false);
            btnLastFiles.AddFileName(SaveTab.FileName, string.Empty);
        }

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