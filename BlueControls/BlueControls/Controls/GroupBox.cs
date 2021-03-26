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

using BlueBasics.Enums;
using BlueControls.Enums;
using BlueControls.Interfaces;
using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace BlueControls.Controls {
    //[Designer("System.Windows.Forms.Design.ParentControlDesigner,System.Design", typeof(IDesigner))]
    [ToolboxBitmap(typeof(System.Windows.Forms.GroupBox))]
    public partial class GroupBox : System.Windows.Forms.GroupBox, IUseMyBackColor {

        #region Constructor
        public GroupBox() : base() {


            //// Fügen Sie Initialisierungen nach dem InitializeComponent()-Aufruf hinzu.
            //SetNotFocusable();
            //_MouseHighlight = false;
            SetStandardValues();
        }

        #endregion

        #region  Variablen 
        //private string _Text = string.Empty;
        private enGroupBoxStyle _GroupBoxStyle = enGroupBoxStyle.Normal;
        #endregion


        #region  Events 
        //public new event EventHandler TextChanged;
        #endregion



        #region  Properties 

        [DefaultValue(enGroupBoxStyle.Normal)]
        public enGroupBoxStyle GroupBoxStyle {
            get => _GroupBoxStyle;
            set {
                if (_GroupBoxStyle == value) { return; }
                _GroupBoxStyle = value;
                SetStandardValues();
                Invalidate();
            }
        }


        //[DefaultValue(0)]
        //public new int TabIndex
        //{
        //    get
        //    {
        //        return 0;
        //    }

        //    set
        //    {

        //        base.TabIndex = 0;
        //    }
        //}

        //[DefaultValue(false)]
        //public new bool TabStop
        //{
        //    get
        //    {
        //        return false;
        //    }

        //    set
        //    {
        //        base.TabStop = false;

        //    }
        //}


        //[DefaultValue("")]
        //public new string Text
        //{
        //    get
        //    {
        //        return _Text;
        //    }

        //    set
        //    {
        //        if (_Text == value) { return; }
        //        _Text = value;
        //        Invalidate();
        //        //OnTextChanged();
        //    }
        //}


        #endregion


        protected override void OnParentChanged(System.EventArgs e) {
            base.OnParentChanged(e);
            SetStandardValues();
            ChildControls_RibbonBar();
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

        private void SetStandardValues() {
            var l = GenericControl.Typ(Parent);

            if (_GroupBoxStyle == enGroupBoxStyle.RibbonBar) { l = enPartentType.RibbonPage; }


            switch (l) {
                case enPartentType.RibbonPage:
                    GroupBoxStyle = enGroupBoxStyle.RibbonBar;
                    BackColor = Skin.Color_Back(enDesign.RibbonBar_Body, enStates.Standard);
                    break;
                case enPartentType.TabPage:
                    BackColor = Skin.Color_Back(enDesign.TabStrip_Body, enStates.Standard);
                    break;

                case enPartentType.Form:
                    BackColor = Skin.Color_Back(enDesign.Form_Standard, enStates.Standard);
                    break;

            }

        }

        protected override void OnPaintBackground(PaintEventArgs pevent) {
            // base.OnPaintBackground(pevent);
        }

        protected override void OnPaint(PaintEventArgs e) {
            //      base.OnPaint(e);

            var state = enStates.Standard;

            if (!GenericControl.AllEnabled(Parent)) { state = enStates.Standard_Disabled; }

            var r = new Rectangle(0, 0, Width, Height);

            e.Graphics.Clear(BackColor);

            switch (_GroupBoxStyle) {
                case enGroupBoxStyle.RibbonBar:
                    Skin.Draw_Border(e.Graphics, enDesign.RibbonBar_Frame, state, r);

                    if (!string.IsNullOrEmpty(Text)) {
                        var bottomTXT = new Rectangle(0, 0, Width, Height + 2);
                        Skin.Draw_FormatedText(e.Graphics, Text, enDesign.RibbonBar_Frame, state, null, enAlignment.Bottom_HorizontalCenter, bottomTXT, this, false, true);
                    }

                    break;

                case enGroupBoxStyle.Normal:
                    if (Height > 33) {
                        Skin.Draw_Border(e.Graphics, enDesign.Frame, state, r);
                        if (!string.IsNullOrEmpty(Text)) {
                            var topTXT = new Rectangle(Skin.Padding, 0, Width, Height);
                            Skin.Draw_FormatedText(e.Graphics, Text, enDesign.Frame, state, null, enAlignment.Top_Left, topTXT, this, true, true);
                        }
                    }
                    break;

                default:
                    Skin.Draw_Back_Transparent(e.Graphics, DisplayRectangle, this);
                    break;
            }


            if (DesignMode) { ChildControls_RibbonBar(); }

        }

        private void ChildControls_RibbonBar() {

            if (_GroupBoxStyle != enGroupBoxStyle.RibbonBar) { return; }

            //if (Parent == null) { return; }
            if (Width < 10 || Height < 10) { return; }
            if (Controls.Count == 0) { return; }

            foreach (System.Windows.Forms.Control thisControl in Controls) {


                switch (thisControl) {

                    case Caption Caption:
                        if (Caption.TextAnzeigeVerhalten == enSteuerelementVerhalten.Steuerelement_Anpassen) { Caption.TextAnzeigeVerhalten = enSteuerelementVerhalten.Text_Abschneiden; }
                        thisControl.Top = (int)(thisControl.Top / 22.0) * 22 + 2;
                        thisControl.Height = Math.Max((int)(thisControl.Height / 22.0) * 22, 22);
                        break;

                    case Line _:
                        thisControl.Top = (int)(thisControl.Top / 22.0) * 22 + 2;
                        thisControl.Height = Math.Max((int)(thisControl.Height / 22.0) * 22, 22);
                        break;

                    case Button _:
                    case ComboBox _:
                    case ListBox _:
                    case TextBox _:
                        thisControl.Top = (int)(thisControl.Top / 22.0) * 22 + 2;
                        thisControl.Height = Math.Max((int)(thisControl.Height / 22.0) * 22, 22);
                        break;
                }
            }
        }
    }
}