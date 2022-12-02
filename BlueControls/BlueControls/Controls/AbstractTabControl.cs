// Authors:
// Christian Peter
//
// Copyright (c) 2022 Christian Peter
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

#nullable enable

using BlueBasics;
using BlueControls.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace BlueControls.Controls;

[ToolboxBitmap(typeof(System.Windows.Forms.TabControl))]
public abstract class AbstractTabControl : System.Windows.Forms.TabControl {

    #region Fields

    private bool _indexChanged;

    #endregion

    #region Constructors

    protected AbstractTabControl() : base() {
        //This call is required by the Windows Form Designer.
        //GenerateAndAdd any initialization after the InitializeComponent() call
        //   Me.SetStyle(System.Windows.Forms.ControlStyles.AllPaintingInWmPaint Or System.Windows.Forms.ControlStyles.OptimizedDoubleBuffer Or System.Windows.Forms.ControlStyles.ResizeRedraw Or System.Windows.Forms.ControlStyles.UserPaint, True)
        //SetStyle(System.Windows.Forms.ControlStyles.ResizeRedraw, false);
        SetStyle(ControlStyles.SupportsTransparentBackColor, false);
        SetStyle(ControlStyles.Opaque, true);

        //The next 3 styles are all for double buffering
        SetStyle(ControlStyles.DoubleBuffer, true);
        SetStyle(ControlStyles.AllPaintingInWmPaint, true);
        SetStyle(ControlStyles.UserPaint, true);
    }

    #endregion

    #region Properties

    [DefaultValue(false)]
    public override bool AutoSize {
        get => false; //MyBase.AutoSize
        set => base.AutoSize = false;
    }

    public TabPage? HotTab { get; private set; }

    [Category("Verhalten")]
    public TabPage TabDefault { get; set; }

    [Category("Verhalten")]
    public string[] TabDefaultOrder { get; set; }

    protected override bool ScaleChildren => false;

    #endregion

    #region Methods

    public void PerformAutoScale() { }

    //public void OnContextMenuItemClicked(ContextMenuItemClickedEventArgs e) => ContextMenuItemClicked?.Invoke(this, e);
    public void Scale() { }

    //public void OnContextMenuInit(ContextMenuInitEventArgs e) => ContextMenuInit?.Invoke(this, e);
    protected void DrawControl(PaintEventArgs e, Design design) {
        Skin.Draw_Back(e.Graphics, design, States.Standard, new Rectangle(0, 0, Width, Height), this, true);

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

    //public void GetContextMenuItems(System.Windows.Forms.MouseEventArgs e, ItemCollectionList Items, out object HotItem, List<string> Tags, ref bool Cancel, ref bool Translate) => HotItem = e != null ? TestTab(new Point(e.X, e.Y)) : null;
    protected override Rectangle GetScaledBounds(Rectangle tbounds, SizeF factor, BoundsSpecified specified) => tbounds;

    // NIX TUN!!!!
    protected override void OnMouseLeave(System.EventArgs e) {
        HotTab = null;
        base.OnMouseLeave(e);
    }

    //public bool ContextMenuItemClickedInternalProcessig(object sender, ContextMenuItemClickedEventArgs e) => false;
    //protected override void OnControlAdded(ControlEventArgs e) {
    //    base.OnControlAdded(e);
    //    if (e.Control is TabPage tp) {
    //        tp.BackColor = this is RibbonBar ? Skin.Color_Back(enDesign.RibbonBar_Body, enStates.Standard)
    //                     : this is TabControl ? Skin.Color_Back(enDesign.TabStrip_Body, enStates.Standard)
    //                     : Color.Red;
    //        Invalidate();
    //    }
    //}
    protected override void OnMouseMove(MouseEventArgs e) {
        if (!HotTrack) { HotTrack = true; }
        HotTab = TestTab(new Point(e.X, e.Y));
        base.OnMouseMove(e);
    }

    //protected override void OnMouseUp(System.Windows.Forms.MouseEventArgs e) {
    //    base.OnMouseUp(e);
    //    if (e.Button == MouseButtons.Right) { FloatingInputBoxListBoxStyle.ContextMenuShow(this, e); }
    //}
    protected override void OnPaintBackground(PaintEventArgs pevent) {
        // do not allow the background to be painted
        // Um flimmern zu vermeiden!
    }

    //protected override void OnParentChanged(System.EventArgs e) {
    //    base.OnParentChanged(e);

    //}

    protected override void OnSelectedIndexChanged(System.EventArgs e) {
        if (_indexChanged) { return; }
        _indexChanged = true;
        base.OnSelectedIndexChanged(e);
        _indexChanged = false;
    }

    protected override void OnVisibleChanged(System.EventArgs e) {
        try {
            base.OnVisibleChanged(e);
            if (DesignMode) { return; }
            if (!Visible) { return; }

            var tmp = TabDefaultOrder;
            if (tmp != null && tmp.GetUpperBound(0) == -1) { tmp = null; }
            if (tmp != null && tmp.GetUpperBound(0) == 0 && string.IsNullOrEmpty(tmp[0])) { tmp = null; }

            if (tmp != null) {
                var neworder = new List<TabPage>();

                foreach (var thisTabName in tmp) {
                    foreach (var thisTab in TabPages) {
                        if (thisTab is TabPage tb) {
                            if (string.Equals(tb.Text, thisTabName, StringComparison.OrdinalIgnoreCase)) {
                                neworder.AddIfNotExists(tb);
                            }
                        }
                    }
                }

                TabPages.Clear();
                foreach (var thisTp in neworder) {
                    TabPages.Add(thisTp);
                }
            }

            if (TabDefault != null && TabPages.Contains(TabDefault)) { SelectedTab = TabDefault; }
        } catch { }
    }

    protected override void ScaleControl(SizeF factor, BoundsSpecified specified) => base.ScaleControl(new SizeF(1, 1), specified);

    protected override void WndProc(ref Message m) {
        if (m.Msg == (int)Enums.WndProc.WM_ERASEBKGND) { return; }
        base.WndProc(ref m);
    }

    private void DrawTabBody(Graphics graphics, int id) {
        var w = States.Standard;
        if (!TabPages[id].Enabled) { w = States.Standard_Disabled; }
        var tabRect = GetTabRect(id);
        Rectangle r = new(0, tabRect.Bottom, Width, Height - tabRect.Bottom);
        if (r.Width < 2 || r.Height < 2) { return; }
        if (this is RibbonBar) {
            Skin.Draw_Back(graphics, Design.RibbonBar_Body, w, r, this, true);
            Skin.Draw_Border(graphics, Design.RibbonBar_Body, w, r);
        } else {
            Skin.Draw_Back(graphics, Design.TabStrip_Body, w, r, this, true);
            Skin.Draw_Border(graphics, Design.TabStrip_Body, w, r);
        }
    }

    private void DrawTabHead(Graphics graphics, int id) {
        try {
            var tmpState = States.Standard;
            if (!TabPages[id].Enabled) { tmpState = States.Standard_Disabled; }
            if (id == SelectedIndex) { tmpState |= States.Checked; }
            if (TabPages[id].Enabled && HotTab == TabPages[id]) { tmpState |= States.Standard_MouseOver; }
            var r = GetTabRect(id);
            r.Y -= 2;
            r.X++;
            if (this is RibbonBar) {
                Skin.Draw_Back(graphics, Design.RibbonBar_Head, tmpState, r, this, true);
                Skin.Draw_FormatedText(graphics, TabPages[id].Text, Design.RibbonBar_Head, tmpState, null, BlueBasics.Enums.Alignment.Horizontal_Vertical_Center, r, this, false, true);
                Skin.Draw_Border(graphics, Design.RibbonBar_Head, tmpState, r);
            } else {
                Skin.Draw_Back(graphics, Design.TabStrip_Head, tmpState, r, this, true);
                Skin.Draw_FormatedText(graphics, TabPages[id].Text, Design.TabStrip_Head, tmpState, null, BlueBasics.Enums.Alignment.Horizontal_Vertical_Center, r, this, false, true);
                Skin.Draw_Border(graphics, Design.TabStrip_Head, tmpState, r);
            }
        } catch { }
    }

    private TabPage? TestTab(Point pt) {
        for (var index = 0; index < TabCount; index++) {
            if (GetTabRect(index).Contains(pt.X, pt.Y)) {
                return TabPages[index];
            }
        }
        return null;
    }

    #endregion
}