// Authors:
// Christian Peter
//
// Copyright (c) 2021 Christian Peter
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

using BlueBasics.Enums;
using BlueControls.Enums;
using BlueControls.EventArgs;
using BlueControls.Forms;
using BlueControls.Interfaces;
using BlueControls.ItemCollection;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace BlueControls.Controls {

    [ToolboxBitmap(typeof(System.Windows.Forms.TabControl))]
    public abstract partial class AbstractTabControl : System.Windows.Forms.TabControl, IContextMenu {

        #region Fields

        private System.Windows.Forms.TabPage _HotTab;

        private bool _IndexChanged = false;

        #endregion

        #region Constructors

        public AbstractTabControl() : base() {
            //This call is required by the Windows Form Designer.
            //Add any initialization after the InitializeComponent() call
            //   Me.SetStyle(System.Windows.Forms.ControlStyles.AllPaintingInWmPaint Or System.Windows.Forms.ControlStyles.OptimizedDoubleBuffer Or System.Windows.Forms.ControlStyles.ResizeRedraw Or System.Windows.Forms.ControlStyles.UserPaint, True)
            //SetStyle(System.Windows.Forms.ControlStyles.ResizeRedraw, false);
            SetStyle(System.Windows.Forms.ControlStyles.SupportsTransparentBackColor, false);
            SetStyle(System.Windows.Forms.ControlStyles.Opaque, true);

            //The next 3 styles are allefor double buffering
            SetStyle(System.Windows.Forms.ControlStyles.DoubleBuffer, true);
            SetStyle(System.Windows.Forms.ControlStyles.AllPaintingInWmPaint, true);
            SetStyle(System.Windows.Forms.ControlStyles.UserPaint, true);
        }

        #endregion

        #region Events

        public event EventHandler<ContextMenuInitEventArgs> ContextMenuInit;

        public event EventHandler<ContextMenuItemClickedEventArgs> ContextMenuItemClicked;

        #endregion

        #region Properties

        [DefaultValue(false)]
        public override bool AutoSize {
            get => false; //MyBase.AutoSize
            set => base.AutoSize = false;
        }

        protected override bool ScaleChildren => false;

        #endregion

        #region Methods

        public bool ContextMenuItemClickedInternalProcessig(object sender, ContextMenuItemClickedEventArgs e) => false;

        public void GetContextMenuItems(System.Windows.Forms.MouseEventArgs e, ItemCollectionList Items, out object HotItem, List<string> Tags, ref bool Cancel, ref bool Translate) => HotItem = e != null ? TestTab(new Point(e.X, e.Y)) : null;

        public void OnContextMenuInit(ContextMenuInitEventArgs e) => ContextMenuInit?.Invoke(this, e);

        public void OnContextMenuItemClicked(ContextMenuItemClickedEventArgs e) => ContextMenuItemClicked?.Invoke(this, e);

        public void PerformAutoScale() { }

        public void Scale() { }

        protected override Rectangle GetScaledBounds(Rectangle tbounds, SizeF factor, System.Windows.Forms.BoundsSpecified specified) => tbounds;

        protected override void OnControlAdded(ControlEventArgs e) {
            base.OnControlAdded(e);
            if (e.Control is TabPage tp) {
                tp.BackColor = this is RibbonBar ? Skin.Color_Back(enDesign.RibbonBar_Body, enStates.Standard)
                             : this is TabControl ? Skin.Color_Back(enDesign.TabStrip_Body, enStates.Standard)
                             : Color.Red;
                Invalidate();
            }
        }

        // NIX TUN!!!!

        // NIX TUN!!!!
        protected override void OnMouseLeave(System.EventArgs e) {
            _HotTab = null;
            base.OnMouseLeave(e);
        }

        protected override void OnMouseMove(System.Windows.Forms.MouseEventArgs e) {
            if (!HotTrack) { HotTrack = true; }
            _HotTab = TestTab(new Point(e.X, e.Y));
            base.OnMouseMove(e);
        }

        protected override void OnMouseUp(System.Windows.Forms.MouseEventArgs e) {
            base.OnMouseUp(e);
            if (e.Button == System.Windows.Forms.MouseButtons.Right) { FloatingInputBoxListBoxStyle.ContextMenuShow(this, e); }
        }

        protected override void OnPaint(System.Windows.Forms.PaintEventArgs e) {
            if (this is RibbonBar) {
                Skin.Draw_Back(e.Graphics, enDesign.RibbonBar_Back, enStates.Standard, new Rectangle(0, 0, Width, Height), this, true);
            } else {
                Skin.Draw_Back(e.Graphics, enDesign.TabStrip_Back, enStates.Standard, new Rectangle(0, 0, Width, Height), this, true);
            }
            if (TabCount == 0 && DesignMode) {
                e.Graphics.DrawRectangle(Pens.Magenta, new Rectangle(0, 0, Width - 1, Height - 1));
                return;
            }
            for (var id = 0; id < TabCount; id++) {
                if (id != SelectedIndex) { DrawTabHead(e.Graphics, id); }
            }
            for (var id = 0; id < TabCount; id++) {
                if (id == SelectedIndex) {
                    DrawTabBody(e.Graphics, id);
                    DrawTabHead(e.Graphics, id);
                }
            }
        }

        protected override void OnPaintBackground(System.Windows.Forms.PaintEventArgs pevent) {
            // do not allow the background to be painted
            // Um flimmern zu vermeiden!
        }

        protected override void OnSelectedIndexChanged(System.EventArgs e) {
            if (_IndexChanged) { return; }
            _IndexChanged = true;
            base.OnSelectedIndexChanged(e);
            _IndexChanged = false;
        }

        protected override void ScaleControl(SizeF factor, System.Windows.Forms.BoundsSpecified specified) {
            factor = new SizeF(1, 1);
            base.ScaleControl(factor, specified);
        }

        protected override void WndProc(ref System.Windows.Forms.Message m) {
            if (m.Msg == (int)enWndProc.WM_ERASEBKGND) { return; }
            base.WndProc(ref m);
        }

        private void DrawTabBody(Graphics graphics, int id) {
            var w = enStates.Standard;
            if (!TabPages[id].Enabled) { w = enStates.Standard_Disabled; }
            var tabRect = GetTabRect(id);
            Rectangle r = new(0, tabRect.Bottom, Width, Height - tabRect.Bottom);
            if (r.Width < 2 || r.Height < 2) { return; }
            if (this is RibbonBar) {
                Skin.Draw_Back(graphics, enDesign.RibbonBar_Body, w, r, this, true);
                Skin.Draw_Border(graphics, enDesign.RibbonBar_Body, w, r);
            } else {
                Skin.Draw_Back(graphics, enDesign.TabStrip_Body, w, r, this, true);
                Skin.Draw_Border(graphics, enDesign.TabStrip_Body, w, r);
            }
        }

        private void DrawTabHead(Graphics graphics, int id) {
            try {
                var tmpState = enStates.Standard;
                if (!TabPages[id].Enabled) { tmpState = enStates.Standard_Disabled; }
                if (id == SelectedIndex) { tmpState |= enStates.Checked; }
                if (TabPages[id].Enabled && _HotTab == TabPages[id]) { tmpState |= enStates.Standard_MouseOver; }
                var r = GetTabRect(id);
                r.Y -= 2;
                r.X++;
                if (this is RibbonBar) {
                    Skin.Draw_Back(graphics, enDesign.RibbonBar_Head, tmpState, r, this, true);
                    Skin.Draw_FormatedText(graphics, TabPages[id].Text, enDesign.RibbonBar_Head, tmpState, null, enAlignment.Horizontal_Vertical_Center, r, this, false, true);
                    Skin.Draw_Border(graphics, enDesign.RibbonBar_Head, tmpState, r);
                } else {
                    Skin.Draw_Back(graphics, enDesign.TabStrip_Head, tmpState, r, this, true);
                    Skin.Draw_FormatedText(graphics, TabPages[id].Text, enDesign.TabStrip_Head, tmpState, null, enAlignment.Horizontal_Vertical_Center, r, this, false, true);
                    Skin.Draw_Border(graphics, enDesign.TabStrip_Head, tmpState, r);
                }
            } catch { }
        }

        private System.Windows.Forms.TabPage TestTab(Point pt) {
            for (var index = 0; index < TabCount; index++) {
                if (GetTabRect(index).Contains(pt.X, pt.Y)) {
                    return TabPages[index];
                }
            }
            return null;
        }

        #endregion
    }
}