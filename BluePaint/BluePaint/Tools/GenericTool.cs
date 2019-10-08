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

        public event System.EventHandler ZoomFit;

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




        public virtual void ToolFirstShown() { }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="e">Pixel-Koordinaten auf dem Bitmap</param>
        public virtual new void MouseDown(BlueControls.EventArgs.MouseEventArgs1_1 e) { }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="e">Pixel-Koordinaten auf dem Bitmap</param>
        public virtual new void MouseMove(BlueControls.EventArgs.MouseEventArgs1_1 e) { }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="e">Pixel-Koordinaten auf dem Bitmap</param>
        public virtual new void MouseUp(BlueControls.EventArgs.MouseEventArgs1_1 e) { }


        public virtual void PicChangedFromMain() { }



        protected virtual void OnHideMainWindow()
        {

            HideMainWindow?.Invoke(this, System.EventArgs.Empty);
        }


        protected virtual void OnZoomFit()
        {

            ZoomFit?.Invoke(this, System.EventArgs.Empty);
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


        //public bool IsInsidePic(System.Windows.Forms.MouseEventArgs e)
        //{

        //    if (e.X >= 0 && e.Y >= 0 && e.X < _Pic.Width && e.Y < _Pic.Height) { return true; }
        //    return false;

        //}

        public void ClearPreviewPic()
        {

            if (_PicPreview == null) { return; }

            var gr = Graphics.FromImage(_PicPreview);
            gr.Clear(Color.Transparent);
            gr.Dispose();

        }




    }

}