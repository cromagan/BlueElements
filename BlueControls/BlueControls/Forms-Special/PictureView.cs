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
using System;
using System.Collections.Generic;
using System.Drawing;

namespace BlueControls.Forms {

    public partial class PictureView {

        #region Fields

        protected List<string>? FileList;
        private int _nr = -1;

        #endregion

        #region Constructors

        public PictureView() {
            // Dieser Aufruf ist für den Designer erforderlich.
            InitializeComponent();
            // Fügen Sie Initialisierungen nach dem InitializeComponent()-Aufruf hinzu.
            InitWindow(false, "", -1);
        }

        public PictureView(List<string>? fileList, bool mitScreenResize, string windowCaption) {
            // Dieser Aufruf ist für den Windows Form-Designer erforderlich.
            InitializeComponent();
            FileList = fileList;
            LoadPic(0);
            InitWindow(mitScreenResize, windowCaption, -1);
            btnZoomIn.Checked = true;
            btnChoose.Enabled = false;
        }

        public PictureView(Bitmap? bmp) {
            // Dieser Aufruf ist für den Windows Form-Designer erforderlich.
            InitializeComponent();
            FileList = new List<string>();
            InitWindow(false, "", -1);
            Pad.Bmp = bmp;
            Pad.ZoomFit();
            btnZoomIn.Checked = true;
            btnChoose.Enabled = false;
        }

        public PictureView(List<string>? fileList, bool mitScreenResize, string windowCaption, int openOnScreen) {
            // Dieser Aufruf ist für den Windows Form-Designer erforderlich.
            InitializeComponent();
            FileList = fileList;
            LoadPic(0);
            InitWindow(mitScreenResize, windowCaption, openOnScreen);
            grpSeiten.Visible = fileList != null && fileList.Count > 1;
            btnZoomIn.Checked = true;
            btnChoose.Enabled = false;
        }

        #endregion

        #region Methods

        //public PictureView(string CodeToParse, string Title)
        //{
        //    // Dieser Aufruf ist für den Windows Form-Designer erforderlich.
        //    InitializeComponent();
        //    _FileList = null;
        //    _Title = Title;
        //    Pad.Item.Clear();
        //    Pad.ParseData(CodeToParse, true, true);
        //    var Count = 0;
        //    do
        //    {
        //        Count++;
        //        if (Pad.RepairAll(0, true)) { break; }
        //        if (Count > 10) { break; }
        //    } while (true);
        //    Pad.RepairAll(1, true);
        //    InitWindow(false, Title, Title, -1, Pad.SheetStyle);
        //}
        protected void LoadPic(int nr) {
            _nr = nr;
            if (FileList != null && nr < FileList.Count) {
                try {
                    Pad.Bmp = (Bitmap)BitmapExt.Image_FromFile(FileList[nr]);
                } catch (Exception ex) {
                    Pad.Bmp = null;
                    Develop.DebugPrint(ex);
                }
            }
            Ribbon.SelectedIndex = 1;
            grpSeiten.Visible = FileList != null && FileList.Count > 1;
            if (FileList == null || FileList.Count == 0) {
                btnZurueck.Enabled = false;
                btnVor.Enabled = false;
            } else {
                grpSeiten.Enabled = true;
                btnZurueck.Enabled = Convert.ToBoolean(_nr > 0);
                btnVor.Enabled = Convert.ToBoolean(_nr < FileList.Count - 1);
            }
            Pad.ZoomFit();
        }

        private void btnVor_Click(object sender, System.EventArgs e) {
            _nr++;
            if (_nr >= FileList.Count - 1) { _nr = FileList.Count - 1; }
            LoadPic(_nr);
        }

        private void btnZoomFit_Click(object sender, System.EventArgs e) => Pad.ZoomFit();

        private void btnZurueck_Click(object sender, System.EventArgs e) {
            _nr--;
            if (_nr <= 0) { _nr = 0; }
            LoadPic(_nr);
        }

        private void InitWindow(bool fitWindowToBest, string windowCaption, int openOnScreen) {
            //    Me.ShowInTaskbar = False
            if (FileList == null || FileList.Count < 2) { grpSeiten.Enabled = false; }
            if (fitWindowToBest) {
                if (System.Windows.Forms.Screen.AllScreens.Length == 1 || openOnScreen < 0) {
                    var opScNr = Generic.PointOnScreenNr(System.Windows.Forms.Cursor.Position);
                    Width = (int)(System.Windows.Forms.Screen.AllScreens[opScNr].WorkingArea.Width / 1.5);
                    Height = (int)(System.Windows.Forms.Screen.AllScreens[opScNr].WorkingArea.Height / 1.5);
                    Left = (int)(System.Windows.Forms.Screen.AllScreens[opScNr].WorkingArea.Left + ((System.Windows.Forms.Screen.AllScreens[opScNr].WorkingArea.Width - Width) / 2.0));
                    Top = (int)(System.Windows.Forms.Screen.AllScreens[opScNr].WorkingArea.Top + ((System.Windows.Forms.Screen.AllScreens[opScNr].WorkingArea.Height - Height) / 2.0));
                } else {
                    Width = System.Windows.Forms.Screen.AllScreens[openOnScreen].WorkingArea.Width;
                    Height = System.Windows.Forms.Screen.AllScreens[openOnScreen].WorkingArea.Height;
                    Left = System.Windows.Forms.Screen.AllScreens[openOnScreen].WorkingArea.Left;
                    Top = System.Windows.Forms.Screen.AllScreens[openOnScreen].WorkingArea.Top;
                }
            }
            if (!string.IsNullOrEmpty(windowCaption)) {
                Text = windowCaption;
            }
            if (Develop.IsHostRunning()) { TopMost = false; }
        }

        private void Pad_MouseUp(object sender, System.Windows.Forms.MouseEventArgs e) {
            if (btnZoomIn.Checked) { Pad.ZoomIn(e); }
            if (btnZoomOut.Checked) { Pad.ZoomOut(e); }
        }

        #endregion
    }
}