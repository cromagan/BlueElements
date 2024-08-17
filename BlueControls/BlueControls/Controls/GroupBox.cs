// Authors:
// Christian Peter
//
// Copyright (c) 2024 Christian Peter
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

using BlueBasics.Enums;
using BlueControls.Enums;
using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace BlueControls.Controls;

[ToolboxBitmap(typeof(System.Windows.Forms.GroupBox))]
public partial class GroupBox : System.Windows.Forms.GroupBox {

    #region Fields

    private GroupBoxStyle _groupBoxStyle = GroupBoxStyle.Normal;

    #endregion

    #region Constructors

    public GroupBox() : base() => SetStandardValues();

    #endregion

    #region Properties

    [DefaultValue(GroupBoxStyle.Normal)]
    public GroupBoxStyle GroupBoxStyle {
        get => _groupBoxStyle;
        set {
            if (_groupBoxStyle == value) { return; }
            _groupBoxStyle = value;
            SetStandardValues();
            Invalidate();
        }
    }

    #endregion

    #region Methods

    public static void DrawGroupBox(Control c, Graphics gr, States state, GroupBoxStyle _groupBoxStyle, string caption) {
        Rectangle r = new(0, 0, c.Width, c.Height);
        gr.Clear(c.BackColor);

        switch (_groupBoxStyle) {
            case GroupBoxStyle.RibbonBar:
                Skin.Draw_Border(gr, Design.RibbonBar_Frame, state, r);
                if (!string.IsNullOrEmpty(caption)) {
                    Rectangle bottomTxt = new(0, 0, c.Width, c.Height + 2);
                    Skin.Draw_FormatedText(gr, caption, Design.RibbonBar_Frame, state, null, Alignment.Bottom_HorizontalCenter, bottomTxt, c, false, true);
                }
                break;

            case GroupBoxStyle.Normal:
                if (c.Height > 33) {
                    Skin.Draw_Border(gr, Design.GroupBox, state, r);
                    if (!string.IsNullOrEmpty(caption)) {
                        Rectangle topTxt = new(Skin.Padding, 0, c.Width, c.Height);
                        Skin.Draw_FormatedText(gr, caption, Design.GroupBox, state, null, Alignment.Top_Left, topTxt, c, true, true);
                    }
                }
                break;

            case GroupBoxStyle.NormalBold:
                if (c.Height > 33) {
                    Skin.Draw_Border(gr, Design.GroupBoxBold, state, r);
                    if (!string.IsNullOrEmpty(caption)) {
                        Rectangle topTxt = new(Skin.Padding, Skin.PaddingSmal, c.Width, c.Height);
                        Skin.Draw_FormatedText(gr, caption, Design.GroupBoxBold, state, null, Alignment.Top_Left, topTxt, c, false, true);
                    }
                } else {
                    var d = Skin.DesignOf(Design.GroupBoxBold, state);
                    gr.Clear(d.BorderColor1);
                    if (!string.IsNullOrEmpty(caption)) {
                        Rectangle topTxt = new(Skin.Padding, 0, c.Width, c.Height);
                        Skin.Draw_FormatedText(gr, caption, Design.GroupBoxBold, state, null, Alignment.VerticalCenter_Left, topTxt, c, false, true);
                    }
                }
                break;
            //if (Height > 33) {
            //    r.Inflate(-2, -2);
            //    for (var z = 0; z < 8; z++) {
            //        Skin.Draw_Border(e.Graphics, enDesign.Frame, state, r);
            //        r.Inflate(1, 1);
            //    }

            //    if (!string.IsNullOrEmpty(Text)) {
            //        Rectangle topTXT = new(Skin.Padding, 0, Width, Height);
            //        Skin.Draw_FormatedText(e.Graphics, Text, enDesign.Frame, state, null, enAlignment.Top_Left, topTXT, this, true, true);
            //    }
            //}
            //break;

            default:
                Skin.Draw_Back_Transparent(gr, c.DisplayRectangle, c);
                break;
        }
    }

    protected override void OnControlAdded(ControlEventArgs e) {
        base.OnControlAdded(e);
        SetStandardValues();
        ChildControls_RibbonBar();
    }

    protected override void OnControlRemoved(ControlEventArgs e) {
        base.OnControlRemoved(e);
        SetStandardValues();
        ChildControls_RibbonBar();
    }

    protected override void OnDockChanged(System.EventArgs e) {
        base.OnDockChanged(e);
        SetStandardValues();
        ChildControls_RibbonBar();
    }

    protected override void OnPaint(PaintEventArgs e) {
        var state = States.Standard;
        if (!GenericControl.AllEnabled(Parent)) { state = States.Standard_Disabled; }
        DrawGroupBox(this, e.Graphics, state, GroupBoxStyle, Text);
        if (DesignMode) { ChildControls_RibbonBar(); }
    }

    protected override void OnPaintBackground(PaintEventArgs pevent) { }

    protected override void OnParentChanged(System.EventArgs e) {
        base.OnParentChanged(e);
        SetStandardValues();
        ChildControls_RibbonBar();
    }

    private void ChildControls_RibbonBar() {
        if (_groupBoxStyle != GroupBoxStyle.RibbonBar) { return; }
        if (Width < 10 || Height < 10) { return; }
        if (Controls.Count == 0) { return; }

        foreach (Control thisControl in Controls) {
            switch (thisControl) {
                case Caption caption:
                    thisControl.Top = ((int)(thisControl.Top / 22.0) * 22) + 2;
                    thisControl.Height = Math.Max((int)(thisControl.Height / 22.0) * 22, 22);
                    break;

                case Line:
                    thisControl.Top = ((int)(thisControl.Top / 22.0) * 22) + 2;
                    thisControl.Height = Math.Max((int)(thisControl.Height / 22.0) * 22, 22);
                    break;

                case Button:

                case ComboBox:

                case ListBox:

                case TextBox:
                    thisControl.Top = ((int)(thisControl.Top / 22.0) * 22) + 2;
                    thisControl.Height = Math.Max((int)(thisControl.Height / 22.0) * 22, 22);
                    break;
            }
        }
    }

    private void SetStandardValues() {
        var l = GenericControl.Typ(Parent);
        if (_groupBoxStyle == GroupBoxStyle.RibbonBar) { l = ParentType.RibbonPage; }
        switch (l) {
            case ParentType.RibbonPage:
                GroupBoxStyle = GroupBoxStyle.RibbonBar;
                base.BackColor = Skin.Color_Back(Design.RibbonBar_Body, States.Standard);
                break;

            case ParentType.TabPage:
                base.BackColor = Skin.Color_Back(Design.TabStrip_Body, States.Standard);
                break;

            case ParentType.Form:
                base.BackColor = Skin.Color_Back(Design.Form_Standard, States.Standard);
                break;
        }
    }

    #endregion
}