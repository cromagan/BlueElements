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
using BlueControls.Designer_Support;
using BlueControls.Forms;
using BlueControls.Enums;
using BlueControls.EventArgs;
using BlueControls.Interfaces;
using BlueControls.ItemCollection;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Design;
using System.Runtime.InteropServices;

namespace BlueControls.Controls
{   
    [ToolboxBitmap(typeof(System.Windows.Forms.TabControl))]
    [Designer(typeof(TabControlDesigner))]
    public sealed class TabControl : System.Windows.Forms.TabControl, IContextMenu, IBackgroundNone
    {

        public event System.EventHandler<TabControlEventArgs> SelectedIndexChanging;
        private TabPage _HotTab;
        public event EventHandler<ContextMenuInitEventArgs> ContextMenuInit;
        public event EventHandler<ContextMenuItemClickedEventArgs> ContextMenuItemClicked;



        public TabControl()
        {


            //This call is required by the Windows Form Designer.

            //Add any initialization after the InitializeComponent() call
            //   Me.SetStyle(System.Windows.Forms.ControlStyles.AllPaintingInWmPaint Or System.Windows.Forms.ControlStyles.OptimizedDoubleBuffer Or System.Windows.Forms.ControlStyles.ResizeRedraw Or System.Windows.Forms.ControlStyles.UserPaint, True)
            SetStyle(System.Windows.Forms.ControlStyles.ResizeRedraw, false);

            SetStyle(System.Windows.Forms.ControlStyles.SupportsTransparentBackColor, false);
            SetStyle(System.Windows.Forms.ControlStyles.Opaque, false);

            //The next 3 styles are allefor double buffering
            SetStyle(System.Windows.Forms.ControlStyles.DoubleBuffer, true);
            // Me.SetStyle(System.Windows.Forms.ControlStyles.OptimizedDoubleBuffer, True)
            SetStyle(System.Windows.Forms.ControlStyles.AllPaintingInWmPaint, true);
            SetStyle(System.Windows.Forms.ControlStyles.UserPaint, true);


            Skin.SkinChanged += SkinChanged;

        }

        //UserControl1 overrides dispose to clean up the component list.
        protected override void Dispose(bool NowDisposing)
        {

            Skin.SkinChanged -= SkinChanged;
            base.Dispose(NowDisposing);
        }



        #region  Interop for SelectedIndexChanging event 

        [StructLayout(LayoutKind.Sequential)]
        private struct NMHDR
        {
            private readonly int HWND;
            private readonly int idFrom;
            public readonly int code;
            public override string ToString()
            {
                return string.Format("Hwnd: {0}, ControlID: {1}, Code: {2}", HWND, idFrom, code);
            }
        }

        private const int TCN_FIRST = -550; //&HFFFFFFFFFFFFFDDA& unchecked((int)0xFFFFFFFFFFFFFDDA) & ;

        private const int TCN_SELCHANGING = TCN_FIRST - 2;

        // Private Const WM_USER As Int32 = &H400&
        //   Private Const WM_NOTIFY As Int32 = &H4E&
        //  Private Const WM_REFLECT As Int32 = WM_USER + &H1C00&

        #endregion


        #region  AutoScale deaktivieren 
        // https://msdn.microsoft.com/de-de/library/ms229605(v=vs.110).aspx

        private bool _IndexChanged = false;

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

        protected override Rectangle GetScaledBounds(Rectangle tbounds, SizeF factor, System.Windows.Forms.BoundsSpecified specified)
        {
            return tbounds; //MyBase.GetScaledBounds(bounds, factor, specified)
        }
        #endregion






        private bool _IsRibbonBar;

        #region  Properties 

        [Editor(typeof(TabPageCollectionEditor), typeof(UITypeEditor))]
        public new TabPageCollection TabPages
        {
            get
            {
                return base.TabPages;
            }
        }

        [DefaultValue(false)]
        public bool IsRibbonBar
        {
            get
            {
                return _IsRibbonBar;
            }
            set
            {
                if (_IsRibbonBar == value)
                {
                    return;
                }
                _IsRibbonBar = value;
                Height = 110;
                SendToBack();
                Dock = System.Windows.Forms.DockStyle.Top;

                Invalidate();
            }
        }


        [DefaultValue(0)]
        public new int SelectedIndex
        {
            get
            {
                Develop.DebugPrint_InvokeRequired(this.InvokeRequired, false);
                return base.SelectedIndex;
            }
            set
            {
                base.SelectedIndex = value;
            }
        }

        #endregion


        #region  SelectedIndexChanging event Implementation 
        protected override void WndProc(ref System.Windows.Forms.Message m)
        {
            if (m.Msg == (int)enWndProc.WM_REFLECT + (int)enWndProc.WM_NOTIFY)
            {
                var hdr = (NMHDR)Marshal.PtrToStructure(m.LParam, typeof(NMHDR));
                if (hdr.code == TCN_SELCHANGING)
                {
                    if (_HotTab != null)
                    {
                        var e = new TabControlEventArgs(_HotTab, Controls.IndexOf(_HotTab));
                        OnSelectedIndexChanging(e);

                        if (e.Cancel || _HotTab.Enabled == false)
                        {
                            m.Result = new IntPtr(1);
                            return;
                        }
                    }
                }
            }

            if (m.Msg == (int)enWndProc.WM_ERASEBKGND) { return; }

            base.WndProc(ref m);
        }

        private void OnSelectedIndexChanging(TabControlEventArgs e)
        {
            SelectedIndexChanging?.Invoke(this, e);
        }

        #endregion


        protected override void OnMouseLeave(System.EventArgs e)
        {
            base.OnMouseLeave(e);
            _HotTab = null;
        }

        protected override void OnMouseMove(System.Windows.Forms.MouseEventArgs e)
        {
            if (!HotTrack) { HotTrack = true; }

            base.OnMouseMove(e);
            _HotTab = (TabPage)TestTab(new Point(e.X, e.Y));
        }

        protected override void OnMouseUp(System.Windows.Forms.MouseEventArgs e)
        {
            base.OnMouseUp(e);
            if (e.Button == System.Windows.Forms.MouseButtons.Right) { ContextMenu_Show(this, e); }
        }









        #region  Custom Methods 

        public void InsertTabPage(System.Windows.Forms.TabPage tabpage, int index)
        {

            if (index < 0 || index > TabCount)
            {
                throw new ArgumentException("Index out of Range.");
            }

            TabPages.Add(tabpage);
            if (index < TabCount - 1)
            {
                do
                {
                    SwapTabPages(tabpage, TabPages[TabPages.IndexOf(tabpage) - 1]);
                } while (TabPages.IndexOf(tabpage) != index);
            }

            SelectedTab = tabpage;

        }

        public void SwapTabPages(System.Windows.Forms.TabPage tp1, System.Windows.Forms.TabPage tp2)
        {
            if (TabPages.Contains(tp1) == false || TabPages.Contains(tp2) == false)
            {
                throw new ArgumentException("TabPages must be in the TabCotrols TabPageCollection.");
            }
            var Index1 = TabPages.IndexOf(tp1);
            var Index2 = TabPages.IndexOf(tp2);
            TabPages[Index1] = tp2;
            TabPages[Index2] = tp1;
        }

        private System.Windows.Forms.TabPage TestTab(Point pt)
        {
            for (var index = 0 ; index < TabCount ; index++)
            {
                if (GetTabRect(index).Contains(pt.X, pt.Y))
                {
                    return TabPages[index];
                }
            }
            return null;
        }

        #endregion



        protected override void OnPaintBackground(System.Windows.Forms.PaintEventArgs pevent)
        {
            // do not allow the background to be painted
            // Um flimmern zu vermeiden!
        }



        protected override void OnPaint(System.Windows.Forms.PaintEventArgs e)
        {

            if (Skin.SkinDB == null) { return; }


            if (IsRibbonBar)
            {
                Height = 110;
                SendToBack();
                Dock = System.Windows.Forms.DockStyle.Top;

                Skin.Draw_Back(e.Graphics, enDesign.RibbonBar_Back, enStates.Standard, new Rectangle(0, 0, Width, Height), this, true);
            }
            else
            {
                Skin.Draw_Back(e.Graphics, enDesign.TabStrip_Back, enStates.Standard, new Rectangle(0, 0, Width, Height), this, true);
            }



            if (TabCount == 0 && DesignMode)
            {
                e.Graphics.DrawRectangle(Pens.Magenta, new Rectangle(0, 0, Width - 1, Height - 1));
                return;
            }



            for (var id = 0 ; id < TabCount ; id++)
            {
                if (id != SelectedIndex) { DrawTabHead(e.Graphics, id); }
            }


            for (var id = 0 ; id < TabCount ; id++)
            {
                if (id == SelectedIndex)
                {
                    DrawTabBody(e.Graphics, id);
                    DrawTabHead(e.Graphics, id);
                }
            }
        }




        private void DrawTabHead(Graphics graphics, int id)
        {
            var tmpState = enStates.Standard;
            if (!TabPages[id].Enabled) { tmpState = enStates.Standard_Disabled; }


            if (id == SelectedIndex) { tmpState |= enStates.Checked; }


            if (TabPages[id].Enabled && _HotTab == TabPages[id]) { tmpState |= enStates.Standard_MouseOver; }




            var r = GetTabRect(id);

            r.Y -= 2;
            r.X += 1;

            if (_IsRibbonBar)
            {
                Skin.Draw_Back(graphics, enDesign.RibbonBar_Head, tmpState, r, this, true);
                Skin.Draw_FormatedText(graphics, TabPages[id].Text, enDesign.RibbonBar_Head, tmpState, null, enAlignment.Horizontal_Vertical_Center, r, this, false, true);
                Skin.Draw_Border(graphics, enDesign.RibbonBar_Head, tmpState, r);
            }
            else
            {
                Skin.Draw_Back(graphics, enDesign.TabStrip_Head, tmpState, r, this, true);
                Skin.Draw_FormatedText(graphics, TabPages[id].Text, enDesign.TabStrip_Head, tmpState, null, enAlignment.Horizontal_Vertical_Center, r, this, false, true);
                Skin.Draw_Border(graphics, enDesign.TabStrip_Head, tmpState, r);
            }




        }

        private void DrawTabBody(Graphics graphics, int id)
        {
            var w = enStates.Standard;
            if (!TabPages[id].Enabled) { w = enStates.Standard_Disabled; }

            var tabRect = GetTabRect(id);
            var r = new Rectangle(0, tabRect.Bottom, Width, Height - tabRect.Bottom);

            if (r.Width < 2 || r.Height < 2) { return; }

            if (_IsRibbonBar)
            {
                Skin.Draw_Back(graphics, enDesign.RibbonBar_Body, w, r, this, true);
                Skin.Draw_Border(graphics, enDesign.RibbonBar_Body, w, r);
            }
            else
            {
                Skin.Draw_Back(graphics, enDesign.TabStrip_Body, w, r, this, true);
                Skin.Draw_Border(graphics, enDesign.TabStrip_Body, w, r);
            }


        }




        private void ContextMenuItemClickedInternalProcessig(object sender, ContextMenuItemClickedEventArgs e)
        {
            FloatingInputBoxListBoxStyle.Close(this);
            if (e.ClickedComand.Internal().ToLower() == "abbruch") { return; }

            OnContextMenuItemClicked(e);
        }

        private void OnContextMenuItemClicked(ContextMenuItemClickedEventArgs e)
        {
            ContextMenuItemClicked?.Invoke(this, e);
        }

        protected override void OnSelectedIndexChanged(System.EventArgs e)
        {
            if (_IndexChanged) { return; }
            _IndexChanged = true;
            base.OnSelectedIndexChanged(e);
            _IndexChanged = false;
        }


        public void ContextMenu_Show(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            FloatingInputBoxListBoxStyle.Close(this);

            var ta = new List<string>();
            _HotTab = (TabPage)TestTab(new Point(e.X, e.Y));
            if (_HotTab != null)
            {
                ta.TagSet("Page", TabPages.IndexOf(_HotTab).ToString());
            }


            var UserMenu = new ItemCollectionList(enBlueListBoxAppearance.KontextMenu);

            OnContextMenuInit(new ContextMenuInitEventArgs(_HotTab, UserMenu));

            if (UserMenu.Count > 0)
            {
                UserMenu.Add(new LineListItem());
                UserMenu.Add(enContextMenuComands.Abbruch);

                var _ContextMenu = FloatingInputBoxListBoxStyle.Show(UserMenu, _HotTab, this, true);
                _ContextMenu.ItemClicked += ContextMenuItemClickedInternalProcessig;
            }

        }

        private void OnContextMenuInit(ContextMenuInitEventArgs e)
        {
            ContextMenuInit?.Invoke(this, e);
        }

        private void SkinChanged(object sender, System.EventArgs e)
        {
            SuspendLayout();
            Invalidate();
            ResumeLayout();

        }
    }


    #region  SelectedIndexChanging EventArgs Class/Delegate 

    #endregion
}