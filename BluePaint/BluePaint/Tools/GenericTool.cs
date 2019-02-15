using BlueControls.Controls;
using BluePaint.EventArgs;
using System;
using System.Drawing;

namespace BluePaint
{
    

    public partial class GenericTool : GroupBox
    {

        public GenericTool()
        {
            InitializeComponent();
        }

        public event System.EventHandler HideMainWindow;

        public event System.EventHandler ShowMainWindow;

        public event System.EventHandler ForceUndoSaving;

        public event System.EventHandler PicChangedByTool;

        public event System.EventHandler<BitmapEventArgs> OverridePic;


        protected Bitmap _Pic;
        protected Bitmap _PicPreview;

        public void SetPics(Bitmap Pic, Bitmap PicPreview)
        {
            _Pic = Pic;
            _PicPreview = PicPreview;
            PicChangedFromMain();
        }




        public virtual void ToolFirstShown(){}


        /// <summary>
        /// 
        /// </summary>
        /// <param name="e">Pixel-Koordinaten auf dem Bitmap</param>
        public virtual new void MouseDown(System.Windows.Forms.MouseEventArgs e) { }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="e">Pixel-Koordinaten auf dem Bitmap</param>
        public virtual new void MouseMove(System.Windows.Forms.MouseEventArgs e) { }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="e">Pixel-Koordinaten auf dem Bitmap</param>
        public virtual new void MouseUp(System.Windows.Forms.MouseEventArgs e) { }


        public virtual void PicChangedFromMain() { }



        protected virtual void OnHideMainWindow()
        {

            HideMainWindow?.Invoke(this, System.EventArgs.Empty);
        }


        protected virtual void OnShowMainWindow()
        {

            ShowMainWindow?.Invoke(this, System.EventArgs.Empty);
        }


        protected virtual void OnOverridePic(BitmapEventArgs e)
        {

            OverridePic?.Invoke(this, e);
        }


        protected virtual void OnPicChangedByTool()
        {

            PicChangedByTool?.Invoke(this, System.EventArgs.Empty);
        }

        protected virtual void OnForceUndoSaving()
        {

            ForceUndoSaving?.Invoke(this, System.EventArgs.Empty);
        }


        public bool IsInsidePic(System.Windows.Forms.MouseEventArgs e)
        {

            if (e.X >= 0 && e.Y >= 0 && e.X < _Pic.Width && e.Y < _Pic.Height) { return true; }
            return false;

        }
        public Point PointInsidePic(System.Windows.Forms.MouseEventArgs e)
        {

            if (_Pic == null) { return Point.Empty; }


            var x1 = e.X;
            var y1 = e.Y;

            x1 = Math.Max(0, x1);
            y1 = Math.Max(0, y1);

            x1 = Math.Min(_Pic.Width - 1, x1);
            y1 = Math.Min(_Pic.Height - 1, y1);

            return new Point(x1, y1);

        }

        public void ClearPreviewPic()
        {

            if (_PicPreview == null) { return; }

            var gr = Graphics.FromImage(_PicPreview);
            gr.Clear(Color.FromArgb(0, 0, 0, 0));

        }




    }

}