#region BlueElements - a collection of useful tools, database and controls

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

#endregion BlueElements - a collection of useful tools, database and controls

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
        private readonly string _Title = string.Empty;

        public PadEditor() : base() {
            // Dieser Aufruf ist für den Designer erforderlich.
            InitializeComponent();
            // Fügen Sie Initialisierungen nach dem InitializeComponent()-Aufruf hinzu.
            InitWindow(false, "", -1, "");
        }

        private void InitWindow(bool FitWindowToBest, string WindowCaption, int OpenOnScreen, string DesignName) {
            if (FitWindowToBest) {
                if (System.Windows.Forms.Screen.AllScreens.Length == 1 || OpenOnScreen < 0) {
                    var OpScNr = modAllgemein.PointOnScreenNr(System.Windows.Forms.Cursor.Position);
                    Width = (int)(System.Windows.Forms.Screen.AllScreens[OpScNr].WorkingArea.Width / 1.5);
                    Height = (int)(System.Windows.Forms.Screen.AllScreens[OpScNr].WorkingArea.Height / 1.5);
                    Left = (int)(System.Windows.Forms.Screen.AllScreens[OpScNr].WorkingArea.Left + ((System.Windows.Forms.Screen.AllScreens[OpScNr].WorkingArea.Width - Width) / 2.0));
                    Top = (int)(System.Windows.Forms.Screen.AllScreens[OpScNr].WorkingArea.Top + ((System.Windows.Forms.Screen.AllScreens[OpScNr].WorkingArea.Height - Height) / 2.0));
                } else {
                    Width = System.Windows.Forms.Screen.AllScreens[OpenOnScreen].WorkingArea.Width;
                    Height = System.Windows.Forms.Screen.AllScreens[OpenOnScreen].WorkingArea.Height;
                    Left = System.Windows.Forms.Screen.AllScreens[OpenOnScreen].WorkingArea.Left;
                    Top = System.Windows.Forms.Screen.AllScreens[OpenOnScreen].WorkingArea.Top;
                }
            }
            if (!string.IsNullOrEmpty(WindowCaption)) {
                Text = WindowCaption;
            }
            PadDesign.Item.Clear();
            PadDesign.Item.AddRange(Skin.AllStyles());
            PadDesign.Text = string.IsNullOrEmpty(DesignName) ? PadDesign.Item[0].Internal : DesignName;
            Pad.Item.SheetStyle = Skin.StyleDB.Row[PadDesign.Text];
            SchriftGröße.Item.Add("30%", "030");
            SchriftGröße.Item.Add("40%", "040");
            SchriftGröße.Item.Add("50%", "050");
            SchriftGröße.Item.Add("60%", "060");
            SchriftGröße.Item.Add("70%", "070");
            SchriftGröße.Item.Add("80%", "080");
            SchriftGröße.Item.Add("90%", "090");
            SchriftGröße.Item.Add("100%", "100");
            SchriftGröße.Item.Add("110%", "110");
            SchriftGröße.Item.Add("120%", "120");
            SchriftGröße.Item.Add("130%", "130");
            SchriftGröße.Item.Add("140%", "140");
            SchriftGröße.Item.Add("150%", "150");
            SchriftGröße.Item.Sort();
            SchriftGröße.Text = "100";
            if (Develop.IsHostRunning()) { TopMost = false; }
        }

        private void btnZoomFit_Click(object sender, System.EventArgs e) => Pad.ZoomFit();

        private void Pad_MouseUp(object sender, System.Windows.Forms.MouseEventArgs e) {
            if (btnZoomIn.Checked) { Pad.ZoomIn(e); }
            if (btnZoomOut.Checked) { Pad.ZoomOut(e); }
        }

        private void btnAddLine_Click(object sender, System.EventArgs e) {
            var P = Pad.MiddleOfVisiblesScreen();
            var w = (int)(300 / Pad.ZoomCurrent());
            LinePadItem b = new(Pad.Item, PadStyles.Style_Standard, new Point(P.X - w, P.Y), new Point(P.X + w, P.Y));
            Pad.Item.Add(b);
        }

        private void btnAddImage_Click(object sender, System.EventArgs e) {
            BitmapPadItem b = new(Pad.Item, QuickImage.Get(enImageCode.Fragezeichen).BMP, new Size(1000, 1000));
            Pad.Item.Add(b);
        }

        private void AddText_Click(object sender, System.EventArgs e) {
            TextPadItem b = new(Pad.Item) {
                Interner_Text = string.Empty,
                Stil = PadStyles.Style_Standard
            };
            Pad.Item.Add(b);
            b.SetCoordinates(new RectangleM(10, 10, 200, 200), true);
        }

        private void btnAddDimension_Click(object sender, System.EventArgs e) {
            DimensionPadItem b = new(Pad.Item, new PointF(300, 300), new PointF(400, 300), 30);
            Pad.Item.Add(b);
        }

        private void Bild_Click(object sender, System.EventArgs e) => Pad.OpenSaveDialog(_Title);

        private void ButtonPageSetup_Click(object sender, System.EventArgs e) => Pad.ShowPrinterPageSetup();

        private void Drucken_Click(object sender, System.EventArgs e) => Pad.Print();

        private void Vorschau_Click(object sender, System.EventArgs e) => Pad.ShowPrintPreview();

        private void PadDesign_ItemClicked(object sender, BasicListItemEventArgs e) => Pad.Item.SheetStyle = Skin.StyleDB.Row[e.Item.Internal];

        private void ckbRaster_CheckedChanged(object sender, System.EventArgs e) => Pad.Item.SnapMode = ckbRaster.Checked ? enSnapMode.SnapToGrid : enSnapMode.Ohne;

        private void txbRasterAnzeige_TextChanged(object sender, System.EventArgs e) {
            if (!txbRasterAnzeige.Text.IsNumeral()) { return; }
            Pad.Item.GridShow = float.Parse(txbRasterAnzeige.Text);
        }

        private void RasterFangen_TextChanged(object sender, System.EventArgs e) {
            if (!txbRasterFangen.Text.IsNumeral()) { return; }
            Pad.Item.GridSnap = float.Parse(txbRasterFangen.Text);
        }

        private void SchriftGröße_ItemClicked(object sender, BasicListItemEventArgs e) => Pad.Item.SheetStyleScale = decimal.Parse(SchriftGröße.Text) / 100m;

        private void ArbeitsbreichSetup_Click(object sender, System.EventArgs e) => Pad.ShowWorkingAreaSetup();

        private void btnAddUnterStufe_Click(object sender, System.EventArgs e) {
            ChildPadItem b = new(Pad.Item);
            b.SetCoordinates(new RectangleM(100, 100, 300, 300), true);
            Pad.Item.Add(b);
        }

        private void btnAddSymbol_Click(object sender, System.EventArgs e) {
            SymbolPadItem b = new(Pad.Item);
            b.SetCoordinates(new RectangleM(100, 100, 300, 300), true);
            Pad.Item.Add(b);
        }

        public virtual void ItemChanged() {
            Pad.ZoomFit();
            Ribbon.SelectedIndex = 1;
            PadDesign.Text = Pad.Item.SheetStyle.CellFirstString();
            SchriftGröße.Text = ((int)(Pad.Item.SheetStyleScale * 100)).ToString(Constants.Format_Integer3);
        }

        private void btnPhsyik_Click(object sender, System.EventArgs e) {
            clsPhysicPadItem b = new(Pad.Item);
            //b.SetCoordinates(new RectangleM(100, 100, 300, 300));
            Pad.Item.Add(b);
        }

        private void btnVorschauModus_CheckedChanged(object sender, System.EventArgs e) => Pad.ShowInPrintMode = btnVorschauModus.Checked;

        private void Pad_PreviewModChanged(object sender, System.EventArgs e) => btnVorschauModus.Checked = Pad.ShowInPrintMode;

        #region Load / Save

        private void LoadTab_FileOk(object sender, System.ComponentModel.CancelEventArgs e) {
            if (sender == btnOeffnen) {
                LoadFile(LoadTab.FileName, LoadTab.FileName);
            } else {
                // Kann nur der Import-Knopf sein
                LoadFile(LoadTab.FileName, string.Empty);
            }
            Ribbon.SelectedIndex = 1;
        }

        private void SaveTab_FileOk(object sender, System.ComponentModel.CancelEventArgs e) {
            var t = Pad.Item.ToString();
            SaveToDisk(SaveTab.FileName, t, false, System.Text.Encoding.GetEncoding(1252));
            btnLastFiles.AddFileName(SaveTab.FileName, string.Empty);
        }

        private void btnOeffnen_Click(object sender, System.EventArgs e) {
            LoadTab.Tag = sender;
            LoadTab.ShowDialog();
        }

        private void btnSpeichern_Click(object sender, System.EventArgs e) => SaveTab.ShowDialog();

        private void btnNeu_Click(object sender, System.EventArgs e) {
            Pad.Item.Clear();
            Pad.ZoomFit();
            Ribbon.SelectedIndex = 1;
        }

        /// <summary>
        /// löscht den kompletten Inhalt des Pads auch die ID und setzt es auf Disabled
        /// </summary>
        public void DisablePad() {
            Pad.Item = new ItemCollectionPad();
            Pad.Enabled = false;
        }

        private void btnLastFiles_ItemClicked(object sender, BasicListItemEventArgs e) => LoadFile(e.Item.Internal, string.Empty);

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
            //Pad.Item.Clear();
            Pad.Item = new ItemCollectionPad(data, useThisID);
            ItemChanged();
        }

        #endregion Load / Save

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
    }
}