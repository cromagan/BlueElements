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

using BlueBasics.Enums;
using BlueControls.Enums;
using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace BlueControls.Controls {

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
            Rectangle r = new(0, 0, Width, Height);
            e.Graphics.Clear(BackColor);

            switch (_groupBoxStyle) {
                case GroupBoxStyle.RibbonBar:
                    Skin.Draw_Border(e.Graphics, Design.RibbonBar_Frame, state, r);
                    if (!string.IsNullOrEmpty(Text)) {
                        Rectangle bottomTxt = new(0, 0, Width, Height + 2);
                        Skin.Draw_FormatedText(e.Graphics, Text, Design.RibbonBar_Frame, state, null, Alignment.Bottom_HorizontalCenter, bottomTxt, this, false, true);
                    }
                    break;

                case GroupBoxStyle.Normal:
                    if (Height > 33) {
                        Skin.Draw_Border(e.Graphics, Design.GroupBox, state, r);
                        if (!string.IsNullOrEmpty(Text)) {
                            Rectangle topTxt = new(Skin.Padding, 0, Width, Height);
                            Skin.Draw_FormatedText(e.Graphics, Text, Design.GroupBox, state, null, Alignment.Top_Left, topTxt, this, true, true);
                        }
                    }
                    break;

                case GroupBoxStyle.NormalBold:
                    if (Height > 33) {
                        Skin.Draw_Border(e.Graphics, Design.GroupBoxBold, state, r);
                        if (!string.IsNullOrEmpty(Text)) {
                            Rectangle topTxt = new(Skin.Padding, Skin.PaddingSmal, Width, Height);
                            Skin.Draw_FormatedText(e.Graphics, Text, Design.GroupBoxBold, state, null, Alignment.Top_Left, topTxt, this, false, true);
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
                    Skin.Draw_Back_Transparent(e.Graphics, DisplayRectangle, this);
                    break;
            }
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
                        if (caption.TextAnzeigeVerhalten == enSteuerelementVerhalten.Steuerelement_Anpassen) { caption.TextAnzeigeVerhalten = enSteuerelementVerhalten.Text_Abschneiden; }
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
            if (_groupBoxStyle == GroupBoxStyle.RibbonBar) { l = PartentType.RibbonPage; }
            switch (l) {
                case PartentType.RibbonPage:
                    GroupBoxStyle = GroupBoxStyle.RibbonBar;
                    BackColor = Skin.Color_Back(Design.RibbonBar_Body, States.Standard);
                    break;

                case PartentType.TabPage:
                    BackColor = Skin.Color_Back(Design.TabStrip_Body, States.Standard);
                    break;

                case PartentType.Form:
                    BackColor = Skin.Color_Back(Design.Form_Standard, States.Standard);
                    break;
            }
        }

        #endregion
    }
}