#region BlueElements - a collection of useful tools, database and controls
// Authors: 
// Christian Peter
// 
// Copyright (c) 2020 Christian Peter
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
#endregion


using BlueBasics;
using System;
using System.Collections.Generic;
using System.Drawing;



namespace BlueControls.Forms
{
    public partial class PictureView
    {
        protected List<string> _FileList;
        private int _NR = -1;
        private readonly string _Title = string.Empty;


        public PictureView()
        {

            // Dieser Aufruf ist für den Designer erforderlich.
            InitializeComponent();

            // Fügen Sie Initialisierungen nach dem InitializeComponent()-Aufruf hinzu.

            InitWindow(false, "", -1, "");
        }


        public PictureView(List<string> FileList, bool MitScreenResize, string WindowCaption)
        {

            // Dieser Aufruf ist für den Windows Form-Designer erforderlich.
            InitializeComponent();

            _FileList = FileList;
            LoadPic(0);

            InitWindow(MitScreenResize, WindowCaption, -1, "");


            ZoomIn.Checked = true;
            Auswahl.Enabled = false;
        }

        public PictureView(Bitmap BMP)
        {

            // Dieser Aufruf ist für den Windows Form-Designer erforderlich.
            InitializeComponent();

            _FileList = new List<string>();


            InitWindow(false, "", -1, "");

            Pad.BMP = BMP;

            Pad.ZoomFit();

            ZoomIn.Checked = true;
            Auswahl.Enabled = false;
        }


        public PictureView(List<string> FileList, bool MitScreenResize, string WindowCaption, int OpenOnScreen)
        {

            // Dieser Aufruf ist für den Windows Form-Designer erforderlich.
            InitializeComponent();

            _FileList = FileList;
            LoadPic(0);

            InitWindow(MitScreenResize, WindowCaption, OpenOnScreen, "");

            grpSeiten.Visible = FileList != null && FileList.Count > 1;

            ZoomIn.Checked = true;
            Auswahl.Enabled = false;
        }

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
        //        Count += 1;
        //        if (Pad.RepairAll(0, true)) { break; }
        //        if (Count > 10) { break; }
        //    } while (true);

        //    Pad.RepairAll(1, true);


        //    InitWindow(false, Title, Title, -1, Pad.SheetStyle);
        //}


        protected void LoadPic(int Nr)
        {
            _NR = Nr;


            if (_FileList != null && Nr < _FileList.Count)
            {
                try
                {
                    Pad.BMP = (Bitmap)BitmapExt.Image_FromFile(_FileList[Nr]);
                }
                catch (Exception ex)
                {
                    Pad.BMP = null;
                    Develop.DebugPrint(ex);
                }


            }
            Ribbon.SelectedIndex = 1;


            grpSeiten.Visible = _FileList != null && _FileList.Count > 1;

            if (_FileList == null || _FileList.Count == 0)
            {
                Links.Enabled = false;
                Rechts.Enabled = false;
            }
            else
            {
                grpSeiten.Enabled = true;
                Links.Enabled = Convert.ToBoolean(_NR > 0);
                Rechts.Enabled = Convert.ToBoolean(_NR < _FileList.Count - 1);

            }



            Pad.ZoomFit();

        }


        private void InitWindow(bool FitWindowToBest, string WindowCaption, int OpenOnScreen, string DesignName)
        {
            //    Me.ShowInTaskbar = False

            if (_FileList == null || _FileList.Count < 2) { grpSeiten.Enabled = false; }


            if (FitWindowToBest)
            {
                if (System.Windows.Forms.Screen.AllScreens.Length == 1 || OpenOnScreen < 0)
                {
                    var OpScNr = modAllgemein.PointOnScreenNr(System.Windows.Forms.Cursor.Position);

                    Width = (int)(System.Windows.Forms.Screen.AllScreens[OpScNr].WorkingArea.Width / 1.5);
                    Height = (int)(System.Windows.Forms.Screen.AllScreens[OpScNr].WorkingArea.Height / 1.5);
                    Left = (int)(System.Windows.Forms.Screen.AllScreens[OpScNr].WorkingArea.Left + (System.Windows.Forms.Screen.AllScreens[OpScNr].WorkingArea.Width - Width) / 2.0);
                    Top = (int)(System.Windows.Forms.Screen.AllScreens[OpScNr].WorkingArea.Top + (System.Windows.Forms.Screen.AllScreens[OpScNr].WorkingArea.Height - Height) / 2.0);

                }
                else
                {
                    Width = System.Windows.Forms.Screen.AllScreens[OpenOnScreen].WorkingArea.Width;
                    Height = System.Windows.Forms.Screen.AllScreens[OpenOnScreen].WorkingArea.Height;
                    Left = System.Windows.Forms.Screen.AllScreens[OpenOnScreen].WorkingArea.Left;
                    Top = System.Windows.Forms.Screen.AllScreens[OpenOnScreen].WorkingArea.Top;
                }
            }


            if (!string.IsNullOrEmpty(WindowCaption))
            {
                Text = WindowCaption;
            }


            if (Develop.IsHostRunning()) { TopMost = false; }
        }


        private void Links_Click(object sender, System.EventArgs e)
        {

            _NR -= 1;
            if (_NR <= 0) { _NR = 0; }

            LoadPic(_NR);
        }

        private void Rechts_Click(object sender, System.EventArgs e)
        {
            _NR += 1;
            if (_NR >= _FileList.Count - 1) { _NR = _FileList.Count - 1; }

            LoadPic(_NR);
        }


        private void ZoomFitBut_Click(object sender, System.EventArgs e)
        {
            Pad.ZoomFit();
        }



        private void Pad_MouseUp(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            if (ZoomIn.Checked) { Pad.ZoomIn(e); }
            if (ZoomOut.Checked) { Pad.ZoomOut(e); }
        }

    }
}