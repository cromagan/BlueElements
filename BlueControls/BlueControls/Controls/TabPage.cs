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

using BlueControls.Enums;
using BlueControls.Interfaces;
using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms.Design;

namespace BlueControls.Controls
{
    [Designer(typeof(ScrollableControlDesigner))]
    public class TabPage : System.Windows.Forms.TabPage
    {

        #region Constructor



        public TabPage() : base()
        {

            //This call is required by the Windows Form Designer.
            //InitializeComponent()

            //Add any initialization after the InitializeComponent() call
            //SetStyle(System.Windows.Forms.ControlStyles.ResizeRedraw, false);

            //SetStyle(System.Windows.Forms.ControlStyles.SupportsTransparentBackColor, false);
            //SetStyle(System.Windows.Forms.ControlStyles.Opaque, true);

            ////The next 3 styles are allefor double buffering
            //SetStyle(System.Windows.Forms.ControlStyles.DoubleBuffer, true);
            //// Me.SetStyle(System.Windows.Forms.ControlStyles.OptimizedDoubleBuffer, True)
            //SetStyle(System.Windows.Forms.ControlStyles.AllPaintingInWmPaint, true);
            //SetStyle(System.Windows.Forms.ControlStyles.UserPaint, true);

            SetBackColor();


        }

        #endregion







        #region  AutoScale deaktivieren 
        // https://msdn.microsoft.com/de-de/library/ms229605(v=vs.110).aspx



        public void PerformAutoScale()
        {
            // NIX TUN!!!!
        }

        public void Scale()
        {
            // NIX TUN!!!!
        }



        protected override void ScaleControl(SizeF factor, System.Windows.Forms.BoundsSpecified specified)
        {
            factor = new SizeF(1, 1);
            base.ScaleControl(factor, specified);


        }

        protected override bool ScaleChildren
        {
            get
            {
                return false; //MyBase.ScaleChildren
            }
        }

        [DefaultValue(false)]
        public override bool AutoSize
        {
            get
            {
                return false; //MyBase.AutoSize
            }
            set
            {
                base.AutoSize = false;
            }
        }

        protected override Rectangle GetScaledBounds(Rectangle bounds, SizeF factor, System.Windows.Forms.BoundsSpecified specified)
        {
            return bounds; //MyBase.GetScaledBounds(bounds, factor, specified)
        }
        #endregion


        protected override void OnParentChanged(System.EventArgs e)
        {
            base.OnParentChanged(e);
            SetBackColor();
        }

        private void SetBackColor()
        {
            if (Parent != null)
            {
                if (((TabControl)Parent).IsRibbonBar)
                {
                    BackColor = Skin.Color_Back(enDesign.RibbonBar_Body, enStates.Standard);
                }
                else
                {
                    BackColor = Skin.Color_Back(enDesign.TabStrip_Body, enStates.Standard);
                }
            }
            else
            {
                BackColor = Color.Red;
            }
        }


        //protected override void OnPaintBackground(System.Windows.Forms.PaintEventArgs pevent)
        //{

        //}

        //private void DoDraw(Graphics GR)
        //{
        //    if (Skin.SkinDB == null) { return; }
        //    if (IsDisposed) { return; }

        //    if (Width < 1 || Height < 1) { return; }

        //    //if (_BitmapOfControl == null)
        //    //{
        //    //    _BitmapOfControl = new Bitmap(ClientSize.Width, ClientSize.Height, PixelFormat.Format32bppPArgb);
        //    //}

        //    //var TMPGR = Graphics.FromImage(_BitmapOfControl);

        //    if (Parent != null)
        //    {
        //        if (((TabControl)Parent).IsRibbonBar)
        //        {
        //            GR.Clear(Skin.Color_Back(enDesign.RibbonBar_Body, enStates.Standard));
        //            Skin.Draw_Back(GR, enDesign.RibbonBar_Body, enStates.Standard, ClientRectangle, this, true);
        //        }
        //        else
        //        {
        //            GR.Clear(Skin.Color_Back(enDesign.TabStrip_Body, enStates.Standard));
        //            Skin.Draw_Back(GR, enDesign.TabStrip_Body, enStates.Standard, ClientRectangle, this, true);
        //        }
        //    }

        //    //GR.DrawImage(_BitmapOfControl, 0, 0);
        //    //TMPGR.Dispose();


        //}


        //protected override void OnEnabledChanged(System.EventArgs e)
        //{

        //    if (InvokeRequired)
        //    {
        //        Invoke(new System.Action(() => OnEnabledChanged(e)));
        //        return;
        //    }


        //    base.OnEnabledChanged(e);
        //    Invalidate();
        //    Parent?.Invalidate();
        //}

        //protected override void OnPaint(System.Windows.Forms.PaintEventArgs e)
        //{
        //    DoDraw(e.Graphics);
        //}



        ///// <summary>
        ///// Veranlaßt, das das Control neu gezeichnet wird.
        ///// </summary>
        ///// <remarks></remarks>
        //public override void Refresh()
        //{
        //    if (IsDisposed) { return; }
        //    DoDraw(CreateGraphics());
        //}


        //protected override void WndProc(ref System.Windows.Forms.Message m)
        //{
        //    if (m.Msg == (int)enWndProc.WM_ERASEBKGND) { return; }
        //    base.WndProc(ref m);
        //}

        //protected override void OnEnter(System.EventArgs e)
        //{
        //    //   MyBase.OnEnter(e)
        //}

        //protected override void OnLeave(System.EventArgs e)
        //{
        //    //MyBase.OnLeave(e)
        //}



    }
}
