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
using BlueBasics.Enums;
using BluePaint.EventArgs;
using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using static BlueBasics.Extensions;



namespace BluePaint
{
    public partial class Form1
    {

        public Form1()
        {
            InitializeComponent();
        }

        private GenericTool CurrentTool;
        private Bitmap _PicUndo = null;

        private void Screenshot_Click(object sender, System.EventArgs e)
        {
            SetTool(new Tool_Screenshot(), true);
        }

        private void Clipping_Click(object sender, System.EventArgs e)
        {
            SetTool(new Tool_Clipping(), true);
        }


        public void SetPic(Bitmap bmp)
        {
            CurrentTool_OverridePic(this, new BitmapEventArgs(bmp));
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


        //public void Redraw()
        //{

        //    if (P.Image == null || P.Image.Width != P.Width || P.Image.Height != P.Height)
        //    {
        //        P.Image = new Bitmap(P.Width, P.Height);
        //    }

        //    var gr = Graphics.FromImage(P.Image);

        //    gr.Clear(Color.FromArgb(201, 211, 226));


        //    if (P.BMP != null)
        //    {

        //        _Zoom = (float)Math.Min(P.Width / (double)(P.BMP.Width + 20), P.Height / (double)(P.BMP.Height + 20));

        //        var DW = Convert.ToInt32(Convert.ToSingle(Math.Floor(P.BMP.Width * _Zoom)));
        //        var DH = Convert.ToInt32(Convert.ToSingle(Math.Floor(P.BMP.Height * _Zoom)));
        //        var RückX = Convert.ToInt32(Math.Floor((P.Width - DW) / 2.0));
        //        var RückY = Convert.ToInt32(Math.Floor((P.Height - DH) / 2.0));

        //        _DrawArea = new Rectangle(RückX, RückY, DW, DH);
        //    }
        //    else
        //    {
        //        _DrawArea = new Rectangle();
        //        _Zoom = 1.0F;
        //    }


        //    if (_Zoom >= 1)
        //    {
        //        gr.InterpolationMode = InterpolationMode.NearestNeighbor;
        //    }
        //    else
        //    {
        //        gr.InterpolationMode = InterpolationMode.Bicubic;
        //    }


        //    gr.PixelOffsetMode = PixelOffsetMode.Half;

        //    if (P.BMP != null)
        //    {
        //        gr.DrawImage(P.BMP, _DrawArea);
        //    }

        //    if (P.OverlayBMP != null)
        //    {
        //        gr.DrawImage(P.OverlayBMP, _DrawArea);
        //    }

        //    P.Refresh();
        //}

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


    }

}