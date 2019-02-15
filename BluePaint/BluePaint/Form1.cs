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
        private Bitmap _Pic = null;
        private Bitmap _PicUndo = null;
        private Bitmap _PicPreview = null;
        private float _Zoom;
        private Rectangle _DrawArea;
        private Point _MouseCurrent;


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



            if (_PicPreview != null)
            {
                var gr = Graphics.FromImage(_PicPreview);
                gr.Clear(Color.FromArgb(0, 0, 0, 0));
                Redraw();
            }


            if (CurrentTool != null)
            {
                CurrentTool.Dispose();
                Split.Panel1.Controls.Remove(CurrentTool);


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


                CurrentTool.SetPics(_Pic, _PicPreview);
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

        private void CurrentTool_PicChangedByTool(object sender, System.EventArgs e)
        {
            Redraw();
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
            _Pic = e.BMP;
            _PicPreview = new Bitmap(_Pic.Width, _Pic.Height);
            Redraw();


            if (CurrentTool != null) { CurrentTool.SetPics(_Pic, _PicPreview); }
        }

        private void CurrentTool_ForceUndoSaving(object sender, System.EventArgs e)
        {


            if (_PicUndo != null)
            {
                _PicUndo.Dispose();
                _PicUndo = null;
                GC.Collect();
            }


            if (_Pic == null)
            {
                Rückg.Enabled = false;
                return;
            }

            _PicUndo = _Pic.Image_Clone();
            Rückg.Enabled = true;

        }


        public void Redraw()
        {

            if (P.Image == null || P.Image.Width != P.Width || P.Image.Height != P.Height)
            {
                P.Image = new Bitmap(P.Width, P.Height);
            }

            var gr = Graphics.FromImage(P.Image);

            gr.Clear(Color.FromArgb(201, 211, 226));


            if (_Pic != null)
            {

                _Zoom = (float)Math.Min(P.Width / (double)(_Pic.Width + 20), P.Height / (double)(_Pic.Height + 20));

                var DW = Convert.ToInt32(Convert.ToSingle(Math.Floor(_Pic.Width * _Zoom)));
                var DH = Convert.ToInt32(Convert.ToSingle(Math.Floor(_Pic.Height * _Zoom)));
                var RückX = Convert.ToInt32(Math.Floor((P.Width - DW) / 2.0));
                var RückY = Convert.ToInt32(Math.Floor((P.Height - DH) / 2.0));

                _DrawArea = new Rectangle(RückX, RückY, DW, DH);
            }
            else
            {
                _DrawArea = new Rectangle();
                _Zoom = 1.0F;
            }


            if (_Zoom >= 1)
            {
                gr.InterpolationMode = InterpolationMode.NearestNeighbor;
            }
            else
            {
                gr.InterpolationMode = InterpolationMode.Bicubic;
            }


            gr.PixelOffsetMode = PixelOffsetMode.Half;

            if (_Pic != null)
            {
                gr.DrawImage(_Pic, _DrawArea);
            }

            if (_PicPreview != null)
            {
                gr.DrawImage(_PicPreview, _DrawArea);
            }

            P.Refresh();
        }

        private void Bruchlinie_Click(object sender, System.EventArgs e)
        {
            SetTool(new Tool_Bruchlinie(), true);
        }


        private void P_SizeChanged(object sender, System.EventArgs e)
        {
            Redraw();
        }

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

            if (_PicUndo == null)
            {
                return;
            }
            Rückg.Enabled = false;


            BlueBasics.modAllgemein.Swap(ref _Pic, ref _PicUndo);
            _PicPreview = new Bitmap(_Pic.Width, _Pic.Height);

            Redraw();

            if (CurrentTool != null)
            {
                CurrentTool.SetPics(_Pic, _PicPreview);
            }

        }



        private void P_MouseDown(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            MouseToPixelKoordsOnImage(e, false);

            if (CurrentTool != null)
            {
                CurrentTool.MouseDown(new System.Windows.Forms.MouseEventArgs(e.Button, e.Clicks, _MouseCurrent.X, _MouseCurrent.Y, e.Delta));
            }
        }


        private void P_MouseMove(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            MouseToPixelKoordsOnImage(e, false);

            if (CurrentTool != null)
            {
                CurrentTool.MouseMove(new System.Windows.Forms.MouseEventArgs(e.Button, e.Clicks, _MouseCurrent.X, _MouseCurrent.Y, e.Delta));
            }


            if (_Pic == null)
            {
                return;
            }
            if (_MouseCurrent.X >= 0 && _MouseCurrent.Y >= 0 && _MouseCurrent.X < _Pic.Width && _MouseCurrent.Y < _Pic.Height)
            {
                var c = _Pic.GetPixel(_MouseCurrent.X, _MouseCurrent.Y);

                InfoText.Text = "X: " + _MouseCurrent.X +
                               "<br>Y: " + _MouseCurrent.Y +
                               "<br>Farbe: " + c.ToHTMLCode().ToUpper();

            }
            else
            {
                InfoText.Text = "";

            }
            ShowLupe(e);
        }

        private void P_MouseUp(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            MouseToPixelKoordsOnImage(e, false);
            if (CurrentTool != null)
            {
                CurrentTool.MouseUp(new System.Windows.Forms.MouseEventArgs(e.Button, e.Clicks, _MouseCurrent.X, _MouseCurrent.Y, e.Delta));
            }
        }

        /// <summary>
        /// Berechnet Maus Koordinaten auf dem Großen Bild
        /// in in Koordinaten um, als ob auf dem Bild direkt gewählt werden würde.
        /// </summary>
        /// <remarks>
        /// </remarks>
        private void MouseToPixelKoordsOnImage(System.Windows.Forms.MouseEventArgs e, bool TrimIntoPic)
        {

            if (_Zoom < 0.01F) { return; }

            var x1 = Convert.ToInt32(Convert.ToSingle(Math.Floor((e.X - _DrawArea.Left) / _Zoom)));
            var y1 = Convert.ToInt32(Convert.ToSingle(Math.Floor((e.Y - _DrawArea.Top) / _Zoom)));

            if (TrimIntoPic)
            {
                x1 = Math.Max(0, x1);
                y1 = Math.Max(0, y1);

                x1 = Math.Min(_Pic.Width, x1);
                y1 = Math.Min(_Pic.Height, y1);

            }

            _MouseCurrent = new Point(x1, y1);

        }

        public new Bitmap ShowDialog()
        {
            if (this.Visible) { this.Visible = false; }

            base.ShowDialog();
            return _Pic;

        }

        private void GroupBox2_Click(object sender, System.EventArgs e)
        {

        }

        private void OK_Click(object sender, System.EventArgs e)
        {
            this.Close();
        }


        private void P_MouseLeave(object sender, System.EventArgs e)
        {
            ShowLupe(null);

            InfoText.Text = "";

        }

        public void ShowLupe(System.Windows.Forms.MouseEventArgs e)
        {

            if (Lupe.Image == null || Lupe.Image.Width != Lupe.Width || Lupe.Image.Height != Lupe.Height)
            {
                Lupe.Image = new Bitmap(Lupe.Width, Lupe.Height);
            }


            var gr = Graphics.FromImage(Lupe.Image);
            gr.Clear(Color.White);
            if (e == null)
            {
                return;
            }

            var zoomf = Lupe.Width / 15.0;

            var r = new Rectangle(0, 0, Lupe.Width, Lupe.Height);


            gr.InterpolationMode = InterpolationMode.NearestNeighbor;
            gr.PixelOffsetMode = PixelOffsetMode.Half;

            gr.DrawImage(_Pic, r, new Rectangle(_MouseCurrent.X - 7, _MouseCurrent.Y - 7, 15, 15), GraphicsUnit.Pixel);

            if (_PicPreview != null)
            {
                gr.DrawImage(_PicPreview, r, new Rectangle(_MouseCurrent.X - 7, _MouseCurrent.Y - 7, 15, 15), GraphicsUnit.Pixel);
            }


            var Mitte = r.PointOf(enAlignment.Horizontal_Vertical_Center);
            gr.DrawLine(new Pen(Color.FromArgb(128, 255, 255, 255), 3), Mitte.X, Mitte.Y - 7, Mitte.X, Mitte.Y + 6);
            gr.DrawLine(new Pen(Color.FromArgb(128, 255, 255, 255), 3), Mitte.X - 7, Mitte.Y, Mitte.X + 6, Mitte.Y);

            gr.DrawLine(new Pen(Color.FromArgb(20, 255, 0, 0)), Mitte.X, r.Top, Mitte.X, r.Bottom);
            gr.DrawLine(new Pen(Color.FromArgb(20, 255, 0, 0)), r.Left, Mitte.Y, r.Right, Mitte.Y);

            gr.DrawLine(Pens.Red, Mitte.X, Mitte.Y - 6, Mitte.X, Mitte.Y + 5);
            gr.DrawLine(Pens.Red, Mitte.X - 6, Mitte.Y, Mitte.X + 5, Mitte.Y);


            Lupe.Refresh();

        }

        private void Dummy_Click(object sender, System.EventArgs e)
        {
            SetTool(new Tool_DummyGenerator(), true);
        }
    }

}