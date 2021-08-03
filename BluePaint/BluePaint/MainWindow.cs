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
using BlueControls.Forms;
using BluePaint.EventArgs;
using System;
using System.Collections.Generic;
using System.Drawing;
using static BlueBasics.BitmapExt;
using static BlueBasics.Extensions;
using static BlueBasics.FileOperations;

namespace BluePaint {

    public partial class MainWindow {

        #region Fields

        private static List<string> _macro = null;

        // Merkt sich im Falle einer Aufnahme die benutzen Tools. So können sie ganz einfach wieder aufgerufen werden
        private static List<GenericTool> _merker = null;

        private bool _aufnahme = false;
        private string _filename = string.Empty;
        private bool _isSaved = true;
        private Bitmap _PicUndo = null;
        private GenericTool CurrentTool;

        #endregion

        #region Constructors

        public MainWindow(bool loadSaveEnabled) : base() {
            InitializeComponent();
            Tab_Start.Enabled = loadSaveEnabled;
            btnOK.Visible = !loadSaveEnabled;
            tabRibbonbar.SelectedIndex = 1;
        }

        public MainWindow(string filename, bool LoadSaveEnabled) : this(LoadSaveEnabled) => LoadFromDisk(filename);

        private MainWindow() : this(true) { }

        #endregion

        #region Methods

        /// <summary>
        /// Filename wird entfernt!
        /// </summary>
        /// <param name="bmp"></param>
        public void SetPic(Bitmap bmp) {
            CurrentTool_OverridePic(this, new BitmapEventArgs(bmp));
            _filename = string.Empty;
        }

        public void SetTool(GenericTool NewTool, bool DoInitalizingAction) {
            if (AreSame(NewTool, CurrentTool)) {
                MessageBox.Show("Das Werkzeug ist aktuell schon gewählt.", enImageCode.Information, "OK");
                return;
            }
            if (CurrentTool != null) {
                CurrentTool.OnToolChanging();
                CurrentTool.Dispose();
                Split.Panel1.Controls.Remove(CurrentTool);
                CurrentTool.ZoomFit -= CurrentTool_ZoomFit;
                CurrentTool.HideMainWindow -= CurrentTool_HideMainWindow;
                CurrentTool.ShowMainWindow -= CurrentTool_ShowMainWindow;
                CurrentTool.ForceUndoSaving -= CurrentTool_ForceUndoSaving;
                //CurrentTool.PicChangedByTool -= CurrentTool_PicChangedByTool;
                CurrentTool.OverridePic -= CurrentTool_OverridePic;
                CurrentTool.DoInvalidate -= CurrentTool_DoInvalidate;
                CurrentTool.NeedCurrentPic -= CurrentTool_NeedCurrentPic;
                CurrentTool.CommandForMacro -= CurrentTool_CommandForMacro;
                CurrentTool = null;
            }
            P.Invalidate();
            if (NewTool != null) {
                CurrentTool = NewTool;
                if (_aufnahme) {
                    if (string.IsNullOrEmpty(NewTool.MacroKennung())) {
                        MessageBox.Show("Während einer Aufnahme<br>nicht möglich.", enImageCode.Information, "OK");
                        return;
                    }
                    _merker.Add(NewTool);
                }
                Split.Panel1.Controls.Add(NewTool);
                NewTool.Dock = System.Windows.Forms.DockStyle.Fill;
                //CurrentTool.SetPics(P.BMP, P.OverlayBMP);
                CurrentTool.ZoomFit += CurrentTool_ZoomFit;
                CurrentTool.HideMainWindow += CurrentTool_HideMainWindow;
                CurrentTool.ShowMainWindow += CurrentTool_ShowMainWindow;
                //CurrentTool.PicChangedByTool += CurrentTool_PicChangedByTool;
                CurrentTool.OverridePic += CurrentTool_OverridePic;
                CurrentTool.ForceUndoSaving += CurrentTool_ForceUndoSaving;
                CurrentTool.DoInvalidate += CurrentTool_DoInvalidate;
                CurrentTool.NeedCurrentPic += CurrentTool_NeedCurrentPic;
                CurrentTool.CommandForMacro += CurrentTool_CommandForMacro;
                if (DoInitalizingAction) {
                    NewTool.ToolFirstShown();
                }
                if (_aufnahme && _merker.Contains(NewTool)) {
                    _merker.Add(NewTool);
                }
            }
        }

        public new Bitmap ShowDialog() {
            if (Visible) { Visible = false; }
            base.ShowDialog();
            return P.BMP;
        }

        protected override void OnFormClosing(System.Windows.Forms.FormClosingEventArgs e) {
            if (!IsSaved()) { e.Cancel = true; }
            base.OnFormClosing(e);
        }

        private bool AreSame(object a, object b) {
            if (a == null || b == null) { return false; }
            var t = a.GetType();
            var u = b.GetType();
            if (t.IsAssignableFrom(u) || u.IsAssignableFrom(t)) {
                // x.IsAssignableFrom(y) returns true if:
                //   (1) x and y are the same type
                //   (2) x and y are in the same inheritance hierarchy
                //   (3) y is implemented by x
                //   (4) y is a generic type parameter and one of its constraints is x
                return true;
            }
            return false;
        }

        private void Bruchlinie_Click(object sender, System.EventArgs e) => SetTool(new Tool_Bruchlinie(), !_aufnahme);

        private void btn100_Click(object sender, System.EventArgs e) => P.Zoom100();

        private void btnAufnahme_Click(object sender, System.EventArgs e) {
            if (P.BMP == null) {
                MessageBox.Show("Kein Bild vorhanden.");
                return;
            }
            SetTool(null, false);
            _macro = new List<string>();
            _merker = new List<GenericTool>();
            _aufnahme = true;
            _PicUndo = null;
            btnRückgänig.Enabled = false;
            btnAufnahme.Enabled = false;
            btnStop.Enabled = true;
            grpDatei.Enabled = false;
            MessageBox.Show("Aufnahme gestartet.", enImageCode.Aufnahme, "Ok");
        }

        private void btnCopy_Click(object sender, System.EventArgs e) {
            SetTool(null, false); // um OnToolChangeAuszulösen
            if (P.BMP == null) {
                MessageBox.Show("Kein Bild vorhanden.");
                return;
            }
            System.Windows.Forms.Clipboard.SetImage(P.BMP);
            //System.Windows.Forms.Clipboard.SetDataObject(P.BMP, false);
            Notification.Show("Das Bild ist nun<br>in der Zwischenablage.", enImageCode.Clipboard);
        }

        private void btnEinfügen_Click(object sender, System.EventArgs e) {
            if (!IsSaved()) { return; }
            if (!System.Windows.Forms.Clipboard.ContainsImage()) {
                Notification.Show("Abbruch,<br>kein Bild im Zwischenspeicher!", enImageCode.Information);
                return;
            }
            SetPic((Bitmap)System.Windows.Forms.Clipboard.GetImage());
            _isSaved = false;
            _filename = "*";
            P.ZoomFit();
        }

        private void btnGrößeÄndern_Click(object sender, System.EventArgs e) => SetTool(new Tool_Resize(), !_aufnahme);

        private void btnLetzteDateien_ItemClicked(object sender, BlueControls.EventArgs.BasicListItemEventArgs e) {
            if (!IsSaved()) { return; }
            LoadFromDisk(e.Item.Internal);
        }

        private void btnNeu_Click(object sender, System.EventArgs e) {
            if (!IsSaved()) { return; }
            SetPic(new Bitmap(100, 100));
            _filename = "*";
        }

        private void btnOeffnen_Click(object sender, System.EventArgs e) {
            if (!IsSaved()) { return; }
            LoadTab.ShowDialog();
        }

        private void btnSave_Click(object sender, System.EventArgs e) => Speichern();

        private void btnSaveAs_Click(object sender, System.EventArgs e) {
            SaveTab.ShowDialog();
            if (!PathExists(SaveTab.FileName.FilePath())) { return; }
            if (string.IsNullOrEmpty(SaveTab.FileName)) { return; }
            if (FileExists(SaveTab.FileName)) {
                if (MessageBox.Show("Datei bereits vorhanden.<br>Überschreiben?", enImageCode.Frage, "Ja", "Nein") != 0) { return; }
            }
            _filename = SaveTab.FileName;
            _isSaved = false;
            Speichern();
        }

        private void btnStop_Click(object sender, System.EventArgs e) {
            SetTool(null, false);
            _aufnahme = false;
            btnAufnahme.Enabled = true;
            btnStop.Enabled = false;
            grpDatei.Enabled = true;
            MessageBox.Show("Aufnahme beendet.", enImageCode.Stop, "Ok");
            if (_macro.Count > 0) {
                _isSaved = true;
                SetTool(new Tool_Abspielen(_macro, _merker), false);
            }
        }

        private void btnZoomFit_Click(object sender, System.EventArgs e) => P.ZoomFit();

        private void Clipping_Click(object sender, System.EventArgs e) => SetTool(new Tool_Clipping(_aufnahme), !_aufnahme);

        private void CurrentTool_CommandForMacro(object sender, CommandForMacroArgs e) {
            if (!_aufnahme) { return; }
            _macro.Add(CurrentTool.MacroKennung().ToNonCritical() + ";" + e.Command.ToNonCritical());
        }

        private void CurrentTool_DoInvalidate(object sender, System.EventArgs e) => P.Invalidate();

        private void CurrentTool_ForceUndoSaving(object sender, System.EventArgs e) {
            _isSaved = false;
            if (_PicUndo != null) {
                _PicUndo.Dispose();
                _PicUndo = null;
                GC.Collect();
            }
            if (P.BMP == null) {
                btnRückgänig.Enabled = false;
                return;
            }
            _PicUndo = Image_Clone(P.BMP);
            btnRückgänig.Enabled = true;
        }

        //private void CurrentTool_PicChangedByTool(object sender, System.EventArgs e)
        //{
        //    P.Invalidate();
        //}
        private void CurrentTool_HideMainWindow(object sender, System.EventArgs e) => Hide();

        private void CurrentTool_NeedCurrentPic(object sender, BitmapEventArgs e) => e.BMP = P.BMP;

        private void CurrentTool_OverridePic(object sender, BitmapEventArgs e) {
            CurrentTool_ForceUndoSaving(this, System.EventArgs.Empty);
            P.BMP = e.BMP;
            //if (P.BMP != null)
            //{
            //    P.OverlayBMP = new Bitmap(P.BMP.Width, P.BMP.Height);
            //}
            P.Invalidate();
            //if (CurrentTool != null) { CurrentTool.SetPics(P.BMP, P.OverlayBMP); }
        }

        private void CurrentTool_ShowMainWindow(object sender, System.EventArgs e) => Show();

        private void CurrentTool_ZoomFit(object sender, System.EventArgs e) => P.ZoomFit();

        private void Dummy_Click(object sender, System.EventArgs e) => SetTool(new Tool_DummyGenerator(), !_aufnahme);

        private bool IsSaved() {
            if (_isSaved) { return true; }
            if (string.IsNullOrEmpty(_filename)) { return true; }
            switch (MessageBox.Show("Es sind ungespeicherte Änderungen vorhanden.<br>Was möchten sie tun?", enImageCode.Diskette, "Speichern", "Verwerfen", "Abbrechen")) {
                case 0:
                    Speichern();
                    break;

                case 1:
                    _isSaved = true;
                    return true;

                case 2:
                    return false;
            }
            return IsSaved();
        }

        private void Kontrast_Click(object sender, System.EventArgs e) => SetTool(new Tool_Kontrast(), !_aufnahme);

        private void LoadFromDisk(string filename) {
            if (!IsSaved()) { return; }
            if (FileExists(filename)) {
                SetPic((Bitmap)BitmapExt.Image_FromFile(filename));
                _filename = filename;
                _isSaved = true;
                btnLetzteDateien.AddFileName(filename, string.Empty);
            }
            P.ZoomFit();
        }

        private void LoadTab_FileOk(object sender, System.ComponentModel.CancelEventArgs e) => LoadFromDisk(LoadTab.FileName);

        private void OK_Click(object sender, System.EventArgs e) {
            SetTool(null, false); // um OnToolChangeAuszulösen
            Close();
        }

        private void P_DoAdditionalDrawing(object sender, BlueControls.EventArgs.AdditionalDrawing e) => CurrentTool?.DoAdditionalDrawing(e, P.BMP);

        private void P_ImageMouseDown(object sender, BlueControls.EventArgs.MouseEventArgs1_1 e) => CurrentTool?.MouseDown(e, P.BMP);

        private void P_ImageMouseMove(object sender, BlueControls.EventArgs.MouseEventArgs1_1DownAndCurrent e) {
            CurrentTool?.MouseMove(e, P.BMP);
            if (e.Current.IsInPic) {
                var c = P.BMP.GetPixel(e.Current.TrimmedX, e.Current.TrimmedY);
                InfoText.Text = "X: " + e.Current.TrimmedX +
                               "<br>Y: " + e.Current.TrimmedY +
                               "<br>Farbe: " + c.ToHTMLCode().ToUpper();
            } else {
                InfoText.Text = "";
            }
        }

        private void P_ImageMouseUp(object sender, BlueControls.EventArgs.MouseEventArgs1_1DownAndCurrent e) => CurrentTool?.MouseUp(e, P.BMP);

        private void P_MouseLeave(object sender, System.EventArgs e) => InfoText.Text = "";

        private void Radiergummi_Click(object sender, System.EventArgs e) => SetTool(new Tool_Eraser(_aufnahme), !_aufnahme);

        private void Rückg_Click(object sender, System.EventArgs e) {
            if (_PicUndo == null) { return; }
            btnRückgänig.Enabled = false;
            _isSaved = false;
            var _bmp = P.BMP;
            Generic.Swap(ref _bmp, ref _PicUndo);
            P.BMP = _bmp;
            if (P.BMP.Width != _PicUndo.Width || P.BMP.Height != _PicUndo.Height) {
                P.ZoomFit();
            } else {
                P.Invalidate();
            }
            if (_aufnahme) {
                _macro.RemoveAt(_macro.Count - 1);
            }
            if (CurrentTool != null) {
                CurrentTool.PictureChangedByMainWindow();
            }
        }

        private void Screenshot_Click(object sender, System.EventArgs e) => SetTool(new Tool_Screenshot(), !_aufnahme);

        private void Speichern() {
            SetTool(null, false); // um OnToolChangeAuszulösen
            if (_filename == "*") {
                btnSaveAs_Click(null, System.EventArgs.Empty);
                return;
            }
            try {
                switch (_filename.FileSuffix().ToUpper()) {
                    case "JPG":
                        P.BMP.Save(_filename, System.Drawing.Imaging.ImageFormat.Jpeg);
                        _isSaved = true;
                        break;

                    case "BMP":
                        P.BMP.Save(_filename, System.Drawing.Imaging.ImageFormat.Bmp);
                        _isSaved = true;
                        break;

                    case "PNG":
                        P.BMP.Save(_filename, System.Drawing.Imaging.ImageFormat.Png);
                        _isSaved = true;
                        break;
                }
                P.BMP.Save(_filename);
                _isSaved = true;
            } catch {
                _isSaved = false;
            }
        }

        private void Spiegeln_Click(object sender, System.EventArgs e) => SetTool(new Tool_Spiegeln(), !_aufnahme);

        private void Zeichnen_Click(object sender, System.EventArgs e) => SetTool(new Tool_Paint(), !_aufnahme);

        #endregion
    }
}