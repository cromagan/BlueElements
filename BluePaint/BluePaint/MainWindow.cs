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

#nullable enable

using BlueBasics;
using BlueBasics.Enums;
using BlueControls.Forms;
using BluePaint.EventArgs;
using System;
using System.Collections.Generic;
using System.Drawing;
using static BlueBasics.BitmapExt;
using static BlueBasics.FileOperations;

namespace BluePaint {

    public partial class MainWindow {

        #region Fields

        private static List<string> _macro;

        // Merkt sich im Falle einer Aufnahme die benutzen Tools. So können sie ganz einfach wieder aufgerufen werden
        private static List<GenericTool> _merker;

        private bool _aufnahme;
        private GenericTool? _currentTool;
        private string _filename = string.Empty;
        private bool _isSaved = true;
        private Bitmap? _picUndo;

        #endregion

        #region Constructors

        public MainWindow(bool loadSaveEnabled) : base() {
            InitializeComponent();
            Tab_Start.Enabled = loadSaveEnabled;
            btnOK.Visible = !loadSaveEnabled;
            tabRibbonbar.SelectedIndex = 1;
        }

        public MainWindow(string filename, bool loadSaveEnabled) : this(loadSaveEnabled) => LoadFromDisk(filename);

        private MainWindow() : this(true) { }

        #endregion

        #region Methods

        /// <summary>
        /// Filename wird entfernt!
        /// </summary>
        /// <param name="bmp"></param>
        public void SetPic(Bitmap? bmp) {
            CurrentTool_OverridePic(this, new BitmapEventArgs(bmp));
            _filename = string.Empty;
        }

        public void SetTool(GenericTool? newTool, bool doInitalizingAction) {
            if (AreSame(newTool, _currentTool)) {
                MessageBox.Show("Das Werkzeug ist aktuell schon gewählt.", enImageCode.Information, "OK");
                return;
            }
            if (_currentTool != null) {
                _currentTool.OnToolChanging();
                _currentTool.Dispose();
                Split.Panel1.Controls.Remove(_currentTool);
                _currentTool.ZoomFit -= CurrentTool_ZoomFit;
                _currentTool.HideMainWindow -= CurrentTool_HideMainWindow;
                _currentTool.ShowMainWindow -= CurrentTool_ShowMainWindow;
                _currentTool.ForceUndoSaving -= CurrentTool_ForceUndoSaving;
                //CurrentTool.PicChangedByTool -= CurrentTool_PicChangedByTool;
                _currentTool.OverridePic -= CurrentTool_OverridePic;
                _currentTool.DoInvalidate -= CurrentTool_DoInvalidate;
                _currentTool.NeedCurrentPic -= CurrentTool_NeedCurrentPic;
                _currentTool.CommandForMacro -= CurrentTool_CommandForMacro;
                _currentTool = null;
            }
            P.Invalidate();
            if (newTool != null) {
                _currentTool = newTool;
                if (_aufnahme) {
                    if (string.IsNullOrEmpty(newTool.MacroKennung())) {
                        MessageBox.Show("Während einer Aufnahme<br>nicht möglich.", enImageCode.Information, "OK");
                        return;
                    }
                    _merker.Add(newTool);
                }
                Split.Panel1.Controls.Add(newTool);
                newTool.Dock = System.Windows.Forms.DockStyle.Fill;
                //CurrentTool.SetPics(P.Bmp, P.OverlayBmp);
                _currentTool.ZoomFit += CurrentTool_ZoomFit;
                _currentTool.HideMainWindow += CurrentTool_HideMainWindow;
                _currentTool.ShowMainWindow += CurrentTool_ShowMainWindow;
                //CurrentTool.PicChangedByTool += CurrentTool_PicChangedByTool;
                _currentTool.OverridePic += CurrentTool_OverridePic;
                _currentTool.ForceUndoSaving += CurrentTool_ForceUndoSaving;
                _currentTool.DoInvalidate += CurrentTool_DoInvalidate;
                _currentTool.NeedCurrentPic += CurrentTool_NeedCurrentPic;
                _currentTool.CommandForMacro += CurrentTool_CommandForMacro;
                if (doInitalizingAction) {
                    newTool.ToolFirstShown();
                }
                if (_aufnahme && _merker.Contains(newTool)) {
                    _merker.Add(newTool);
                }
            }
        }

        public new Bitmap? ShowDialog() {
            if (Visible) { Visible = false; }
            base.ShowDialog();
            return P.Bmp;
        }

        protected override void OnFormClosing(System.Windows.Forms.FormClosingEventArgs e) {
            if (!IsSaved()) { e.Cancel = true; }
            base.OnFormClosing(e);
        }

        private static bool AreSame(object? a, object? b) {
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
            if (P.Bmp == null) {
                MessageBox.Show("Kein Bild vorhanden.");
                return;
            }
            SetTool(null, false);
            _macro = new List<string>();
            _merker = new List<GenericTool>();
            _aufnahme = true;
            _picUndo = null;
            btnRückgänig.Enabled = false;
            btnAufnahme.Enabled = false;
            btnStop.Enabled = true;
            grpDatei.Enabled = false;
            MessageBox.Show("Aufnahme gestartet.", enImageCode.Aufnahme, "Ok");
        }

        private void btnCopy_Click(object sender, System.EventArgs e) {
            SetTool(null, false); // um OnToolChangeAuszulösen
            if (P.Bmp == null) {
                MessageBox.Show("Kein Bild vorhanden.");
                return;
            }
            System.Windows.Forms.Clipboard.SetImage(P.Bmp);
            //System.Windows.Forms.Clipboard.SetDataObject(P.Bmp, false);
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
            _macro.Add(_currentTool.MacroKennung().ToNonCritical() + ";" + e.Command.ToNonCritical());
        }

        private void CurrentTool_DoInvalidate(object sender, System.EventArgs e) => P.Invalidate();

        private void CurrentTool_ForceUndoSaving(object sender, System.EventArgs e) {
            _isSaved = false;
            if (_picUndo != null) {
                _picUndo.Dispose();
                _picUndo = null;
                GC.Collect();
            }
            if (P.Bmp == null) {
                btnRückgänig.Enabled = false;
                return;
            }
            _picUndo = Image_Clone(P.Bmp);
            btnRückgänig.Enabled = true;
        }

        //private void CurrentTool_PicChangedByTool(object sender, System.EventArgs e)
        //{
        //    P.Invalidate();
        //}
        private void CurrentTool_HideMainWindow(object sender, System.EventArgs e) => Hide();

        private void CurrentTool_NeedCurrentPic(object sender, BitmapEventArgs e) => e.Bmp = P.Bmp;

        private void CurrentTool_OverridePic(object sender, BitmapEventArgs e) {
            CurrentTool_ForceUndoSaving(this, System.EventArgs.Empty);
            P.Bmp = e.Bmp;
            //if (P.Bmp != null)
            //{
            //    P.OverlayBmp = new Bitmap(P.Bmp.Width, P.Bmp.Height);
            //}
            P.Invalidate();
            //if (CurrentTool != null) { CurrentTool.SetPics(P.Bmp, P.OverlayBmp); }
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
                SetPic((Bitmap)Image_FromFile(filename));
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

        private void P_DoAdditionalDrawing(object sender, BlueControls.EventArgs.AdditionalDrawing e) => _currentTool?.DoAdditionalDrawing(e, P.Bmp);

        private void P_ImageMouseDown(object sender, BlueControls.EventArgs.MouseEventArgs1_1 e) => _currentTool?.MouseDown(e, P.Bmp);

        private void P_ImageMouseMove(object sender, BlueControls.EventArgs.MouseEventArgs1_1DownAndCurrent e) {
            _currentTool?.MouseMove(e, P.Bmp);
            if (e.Current.IsInPic) {
                var c = P.Bmp.GetPixel(e.Current.TrimmedX, e.Current.TrimmedY);
                InfoText.Text = "X: " + e.Current.TrimmedX +
                               "<br>Y: " + e.Current.TrimmedY +
                               "<br>Farbe: " + c.ToHtmlCode().ToUpper();
            } else {
                InfoText.Text = "";
            }
        }

        private void P_ImageMouseUp(object sender, BlueControls.EventArgs.MouseEventArgs1_1DownAndCurrent e) => _currentTool?.MouseUp(e, P.Bmp);

        private void P_MouseLeave(object sender, System.EventArgs e) => InfoText.Text = "";

        private void Radiergummi_Click(object sender, System.EventArgs e) => SetTool(new Tool_Eraser(_aufnahme), !_aufnahme);

        private void Rückg_Click(object sender, System.EventArgs e) {
            if (_picUndo == null) { return; }
            btnRückgänig.Enabled = false;
            _isSaved = false;
            var bmp = P.Bmp;
            Generic.Swap(ref bmp, ref _picUndo);
            P.Bmp = bmp;
            if (P.Bmp.Width != _picUndo.Width || P.Bmp.Height != _picUndo.Height) {
                P.ZoomFit();
            } else {
                P.Invalidate();
            }
            if (_aufnahme) {
                _macro.RemoveAt(_macro.Count - 1);
            }

            _currentTool?.PictureChangedByMainWindow();
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
                        P.Bmp.Save(_filename, System.Drawing.Imaging.ImageFormat.Jpeg);
                        _isSaved = true;
                        break;

                    case "Bmp":
                        P.Bmp.Save(_filename, System.Drawing.Imaging.ImageFormat.Bmp);
                        _isSaved = true;
                        break;

                    case "PNG":
                        P.Bmp.Save(_filename, System.Drawing.Imaging.ImageFormat.Png);
                        _isSaved = true;
                        break;
                }
                P.Bmp.Save(_filename);
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