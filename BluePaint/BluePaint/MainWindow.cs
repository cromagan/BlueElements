#region BlueElements - a collection of useful tools, database and controls
// Authors: 
// Christian Peter
// 
// Copyright (c) 2019 Christian Peter
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
using BluePaint.EventArgs;
using System;
using System.Drawing;
using static BlueBasics.Extensions;
using static BlueBasics.FileOperations;
using BlueControls.Forms;

namespace BluePaint
{
    public partial class MainWindow
    {
        private string _filename = string.Empty;
        private GenericTool CurrentTool;
        private Bitmap _PicUndo = null;

        private bool _isSaved = true;



        public MainWindow() : this(false)
        {
            //InitializeComponent();
        }

        public MainWindow(bool LoadSaveEnabled) : base()
        {
            InitializeComponent();

            Tab_Start.Enabled = LoadSaveEnabled;
            btnOK.Visible = !LoadSaveEnabled;
        }


        public MainWindow(string filename, bool LoadSaveEnabled) : this(LoadSaveEnabled)
        {
            //InitializeComponent();

            LoadFromDisk(filename);

        }

        private void LoadFromDisk(string filename)
        {

            if (!IsSaved()) { return; }

            if (FileExists(filename))
            {
                SetPic((Bitmap)modAllgemein.Image_FromFile(filename));
                _filename = filename;
                _isSaved = true;

                btnLetzteDateien.AddFileName(filename, string.Empty);
            }


        }

        private void Screenshot_Click(object sender, System.EventArgs e)
        {
            SetTool(new Tool_Screenshot(), true);
        }

        private void Clipping_Click(object sender, System.EventArgs e)
        {
            SetTool(new Tool_Clipping(), true);
        }

        /// <summary>
        /// Filename wird entfernt!
        /// </summary>
        /// <param name="bmp"></param>
        public void SetPic(Bitmap bmp)
        {
            CurrentTool_OverridePic(this, new BitmapEventArgs(bmp));
            _filename = string.Empty;
        }

        public void SetTool(GenericTool NewTool, bool DoInitalizingAction)
        {



            if (P.OverlayBMP != null)
            {
                var gr = Graphics.FromImage(P.OverlayBMP);
                gr.Clear(Color.Transparent);
                P.Invalidate();
            }


            if (CurrentTool != null)
            {
                CurrentTool.Dispose();
                Split.Panel1.Controls.Remove(CurrentTool);

                CurrentTool.ZoomFit -= CurrentTool_ZoomFit;
                CurrentTool.HideMainWindow -= CurrentTool_HideMainWindow;
                CurrentTool.ShowMainWindow -= CurrentTool_ShowMainWindow;
                CurrentTool.PicChangedByTool -= CurrentTool_PicChangedByTool;
                CurrentTool.OverridePic -= CurrentTool_OverridePic;
                CurrentTool.ForceUndoSaving -= CurrentTool_ForceUndoSaving;
                CurrentTool = null;
            }




            if (NewTool != null)
            {
                CurrentTool = NewTool;
                Split.Panel1.Controls.Add(NewTool);
                NewTool.Dock = System.Windows.Forms.DockStyle.Fill;


                CurrentTool.SetPics(P.BMP, P.OverlayBMP);
                CurrentTool.ZoomFit += CurrentTool_ZoomFit;
                CurrentTool.HideMainWindow += CurrentTool_HideMainWindow;
                CurrentTool.ShowMainWindow += CurrentTool_ShowMainWindow;
                CurrentTool.PicChangedByTool += CurrentTool_PicChangedByTool;
                CurrentTool.OverridePic += CurrentTool_OverridePic;
                CurrentTool.ForceUndoSaving += CurrentTool_ForceUndoSaving;


                if (DoInitalizingAction)
                {
                    NewTool.ToolFirstShown();
                }

            }


        }

        private void CurrentTool_ZoomFit(object sender, System.EventArgs e)
        {
            P.ZoomFit();
        }

        private void CurrentTool_PicChangedByTool(object sender, System.EventArgs e)
        {
            P.Invalidate();
        }

        private void CurrentTool_HideMainWindow(object sender, System.EventArgs e)
        {
            this.Hide();
        }

        private void CurrentTool_ShowMainWindow(object sender, System.EventArgs e)
        {
            this.Show();
        }

        private void CurrentTool_OverridePic(object sender, BitmapEventArgs e)
        {
            CurrentTool_ForceUndoSaving(this, System.EventArgs.Empty);
            P.BMP = e.BMP;

            if (P.BMP != null)
            {
                P.OverlayBMP = new Bitmap(P.BMP.Width, P.BMP.Height);
            }

            P.Refresh();


            if (CurrentTool != null) { CurrentTool.SetPics(P.BMP, P.OverlayBMP); }
        }

        private void CurrentTool_ForceUndoSaving(object sender, System.EventArgs e)
        {

            _isSaved = false;

            if (_PicUndo != null)
            {
                _PicUndo.Dispose();
                _PicUndo = null;
                GC.Collect();
            }


            if (P.BMP == null)
            {
                btnRückgänig.Enabled = false;
                return;
            }

            _PicUndo = P.BMP.Image_Clone();
            btnRückgänig.Enabled = true;

        }

        private void Bruchlinie_Click(object sender, System.EventArgs e)
        {
            SetTool(new Tool_Bruchlinie(), true);
        }


        //private void P_SizeChanged(object sender, System.EventArgs e)
        //{
        //    P.Refresh();
        //}

        private void Spiegeln_Click(object sender, System.EventArgs e)
        {
            SetTool(new Tool_Spiegeln(), true);
        }

        private void Zeichnen_Click(object sender, System.EventArgs e)
        {
            SetTool(new Tool_Paint(), true);
        }

        private void Radiergummi_Click(object sender, System.EventArgs e)
        {
            SetTool(new Tool_Eraser(), true);
        }

        private void Kontrast_Click(object sender, System.EventArgs e)
        {
            SetTool(new Tool_Kontrast(), true);
        }

        private void Rückg_Click(object sender, System.EventArgs e)
        {

            if (_PicUndo == null) { return; }
            btnRückgänig.Enabled = false;
            _isSaved = false;


            BlueBasics.modAllgemein.Swap(ref P.BMP, ref _PicUndo);
            P.OverlayBMP = new Bitmap(P.BMP.Width, P.BMP.Height);

            if (P.BMP.Width != _PicUndo.Width || P.BMP.Height != _PicUndo.Height)
            {
                P.ZoomFit();
            }
            else
            {

                P.Refresh();
            }

            if (CurrentTool != null) { CurrentTool.SetPics(P.BMP, P.OverlayBMP); }





        }



        private void P_ImageMouseDown(object sender, BlueControls.EventArgs.MouseEventArgs1_1 e)
        {


            if (CurrentTool != null)
            {
                CurrentTool.MouseDown(e);
            }
        }


        private void P_ImageMouseMove(object sender, BlueControls.EventArgs.MouseEventArgs1_1 e)
        {


            if (CurrentTool != null)
            {
                CurrentTool.MouseMove(e);
            }

            if (e.IsInPic)
            {
                var c = P.BMP.GetPixel(e.TrimmedX, e.TrimmedY);

                InfoText.Text = "X: " + e.TrimmedX +
                               "<br>Y: " + e.TrimmedY +
                               "<br>Farbe: " + c.ToHTMLCode().ToUpper();

            }
            else
            {
                InfoText.Text = "";

            }
            // ShowLupe(e);
        }

        private void P_ImageMouseUp(object sender, BlueControls.EventArgs.MouseEventArgs1_1 e)
        {
            if (CurrentTool != null)
            {
                CurrentTool.MouseUp(e);
            }
        }



        public new Bitmap ShowDialog()
        {
            if (this.Visible) { this.Visible = false; }

            base.ShowDialog();
            return P.BMP;

        }

        private void OK_Click(object sender, System.EventArgs e)
        {
            this.Close();
        }


        private void P_MouseLeave(object sender, System.EventArgs e)
        {
            //ShowLupe(null);

            InfoText.Text = "";

        }

        //public void ShowLupe(System.Windows.Forms.MouseEventArgs e)
        //{

        //    if (Lupe.Image == null || Lupe.Image.Width != Lupe.Width || Lupe.Image.Height != Lupe.Height)
        //    {
        //        Lupe.Image = new Bitmap(Lupe.Width, Lupe.Height);
        //    }


        //    var gr = Graphics.FromImage(Lupe.Image);
        //    gr.Clear(Color.White);
        //    if (e == null) { return; }


        //    var r = new Rectangle(0, 0, Lupe.Width, Lupe.Height);


        //    gr.InterpolationMode = InterpolationMode.NearestNeighbor;
        //    gr.PixelOffsetMode = PixelOffsetMode.Half;

        //    gr.DrawImage(P.BMP, r, new Rectangle(e.X - 7, e.Y - 7, 15, 15), GraphicsUnit.Pixel);

        //    if (P.OverlayBMP != null)
        //    {
        //        gr.DrawImage(P.OverlayBMP, r, new Rectangle(e.X - 7, e.Y - 7, 15, 15), GraphicsUnit.Pixel);
        //    }


        //    var Mitte = r.PointOf(enAlignment.Horizontal_Vertical_Center);
        //    gr.DrawLine(new Pen(Color.FromArgb(128, 255, 255, 255), 3), Mitte.X, Mitte.Y - 7, Mitte.X, Mitte.Y + 6);
        //    gr.DrawLine(new Pen(Color.FromArgb(128, 255, 255, 255), 3), Mitte.X - 7, Mitte.Y, Mitte.X + 6, Mitte.Y);

        //    gr.DrawLine(new Pen(Color.FromArgb(20, 255, 0, 0)), Mitte.X, r.Top, Mitte.X, r.Bottom);
        //    gr.DrawLine(new Pen(Color.FromArgb(20, 255, 0, 0)), r.Left, Mitte.Y, r.Right, Mitte.Y);

        //    gr.DrawLine(Pens.Red, Mitte.X, Mitte.Y - 6, Mitte.X, Mitte.Y + 5);
        //    gr.DrawLine(Pens.Red, Mitte.X - 6, Mitte.Y, Mitte.X + 5, Mitte.Y);


        //    Lupe.Refresh();

        //}

        private void Dummy_Click(object sender, System.EventArgs e)
        {
            SetTool(new Tool_DummyGenerator(), true);
        }

        private void btnZoomFit_Click(object sender, System.EventArgs e)
        {
            P.ZoomFit();
        }

        private void btnNeu_Click(object sender, System.EventArgs e)
        {
            if (!IsSaved()) { return; }

            SetPic(new Bitmap(100, 100));
            _filename = "*";

        }

        private void btnOeffnen_Click(object sender, System.EventArgs e)
        {
            if (!IsSaved()) { return; }
            LoadTab.ShowDialog();
        }

        private void btnSaveAs_Click(object sender, System.EventArgs e)
        {

            SaveTab.ShowDialog();

            if (!PathExists(SaveTab.FileName.FilePath())) { return; }
            if (string.IsNullOrEmpty(SaveTab.FileName)) { return; }

          if (FileExists(SaveTab.FileName))
            {
                if (BlueControls.Forms.MessageBox.Show("Datei bereits vorhanden.<br>Überschreiben?", BlueBasics.Enums.enImageCode.Frage,"Ja","Nein" ) !=0) { return; }
            }

            _filename = SaveTab.FileName;
            _isSaved = false;

            Speichern();
        }

        private void btnLetzteDateien_ItemClicked(object sender, BlueControls.EventArgs.BasicListItemEventArgs e)
        {
            if (!IsSaved()) { return; }
            LoadFromDisk(e.Item.Internal);

        }

        private bool IsSaved()
        {
            if (_isSaved) { return true; }

            if (string.IsNullOrEmpty(_filename)) { return true; }


            switch (MessageBox.Show("Es sind ungespeicherte Änderungen vorhanden.<br>Was möchten sie tun?", BlueBasics.Enums.enImageCode.Diskette, "Speichern", "Verwerfen", "Abbrechen"))
            {


                case 0:
                    Speichern();
                    break;

                case 1:
                    return true;

                case 2:
                    return false;
            }

            return IsSaved();
        }

        private void btnSave_Click(object sender, System.EventArgs e)
        {
            Speichern();

        }

        private void Speichern()
        {
            if (_filename == "*")
            {
                btnSaveAs_Click(null, System.EventArgs.Empty);
                return;
            }


            try
            {

                switch (_filename.FileSuffix().ToUpper())
                {
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
            }
            catch
            {
                _isSaved = false;
            }

        }

        private void LoadTab_FileOk(object sender, System.ComponentModel.CancelEventArgs e)
        {
            LoadFromDisk(LoadTab.FileName);
        }


        protected override void OnFormClosing(System.Windows.Forms.FormClosingEventArgs e)
        {
            if (!IsSaved()) { e.Cancel = true; }

            base.OnFormClosing(e);
        }


    }

}