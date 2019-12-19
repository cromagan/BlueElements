﻿#region BlueElements - a collection of useful tools, database and controls
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

        public event System.EventHandler DoInvalidate;
        public event System.EventHandler ZoomFit;
        public event System.EventHandler HideMainWindow;
        public event System.EventHandler ShowMainWindow;
        public event System.EventHandler ForceUndoSaving;
        public event System.EventHandler<BitmapEventArgs> OverridePic;
        public event System.EventHandler<BitmapEventArgs> NeedCurrentPic;


        public virtual void ToolFirstShown() { }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="e">Pixel-Koordinaten auf dem Bitmap</param>
        public virtual new void MouseDown(BlueControls.EventArgs.MouseEventArgs1_1 e, Bitmap OriginalPic) { }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="e">Pixel-Koordinaten auf dem Bitmap</param>
        public virtual new void MouseMove(BlueControls.EventArgs.MouseEventArgs1_1DownAndCurrent e, Bitmap OriginalPic) { }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="e">Pixel-Koordinaten auf dem Bitmap</param>
        public virtual new void MouseUp(BlueControls.EventArgs.MouseEventArgs1_1DownAndCurrent e, Bitmap OriginalPic) { }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="e">Pixel-Koordinaten auf dem Bitmap</param>
        public virtual void DoAdditionalDrawing(BlueControls.EventArgs.AdditionalDrawing e, Bitmap OriginalPic) { }




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


        protected virtual Bitmap OnNeedCurrentPic()
        {
            var e = new BitmapEventArgs(null);
            NeedCurrentPic?.Invoke(this, e);
            return e.BMP;
        }

        /// <summary>
        /// OnForceUndoSaving wird automatisch in der MainForm ausgelöst.
        /// Wird benutzt, wenn ein neues Bild erstellt wurde und dieses in den Speicher soll.
        /// </summary>
        /// <param name="BMP"></param>
        protected virtual void OnOverridePic(Bitmap BMP)
        {

            OverridePic?.Invoke(this, new BitmapEventArgs(BMP));
        }

        ///// <summary>
        ///// Wird benutzt, wenn in das vorhandene Bild etwas gemalt wurde.
        ///// Wird das ganze Bild geändert, muss OnOverridePic benutzt werden.
        ///// </summary>
        //protected virtual void OnPicChangedByTool()
        //{

        //    PicChangedByTool?.Invoke(this, System.EventArgs.Empty);
        //}

        protected virtual void OnForceUndoSaving()
        {

            ForceUndoSaving?.Invoke(this, System.EventArgs.Empty);
        }

        protected virtual void OnDoInvalidate()
        {

            DoInvalidate?.Invoke(this, System.EventArgs.Empty);
        }

        //public void ClearPreviewPic()
        //{

        //    if (_PicPreview == null) { return; }

        //    var gr = Graphics.FromImage(_PicPreview);
        //    gr.Clear(Color.Transparent);
        //    gr.Dispose();

        //}




    }

}