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

using System;
using System.ComponentModel;
using System.ComponentModel.Design;
using System.Drawing;
using System.Drawing.Imaging;
using BlueBasics.Enums;
using BlueControls.Enums;
using BlueControls.Interfaces;

namespace BlueControls.Controls
{
    [Designer("System.Windows.Forms.Design.ParentControlDesigner,System.Design", typeof(IDesigner))]
    public partial class GroupBox : GenericControl
    {

        #region Constructor
        public GroupBox() : base(false, true)
        {


            // Fügen Sie Initialisierungen nach dem InitializeComponent()-Aufruf hinzu.
            SetNotFocusable();
            _MouseHighlight = false;
        }

        #endregion

        #region  Variablen 
        private string _Text = string.Empty;
        private enGroupBoxStyle _GroupBoxStyle = enGroupBoxStyle.Normal;
        #endregion


        #region  Events 
        //public new event EventHandler TextChanged;
        #endregion



        #region  Properties 

        [DefaultValue(enGroupBoxStyle.Normal)]
        public enGroupBoxStyle GroupBoxStyle
        {
            get
            {
                return _GroupBoxStyle;
            }
            set
            {
                if (_GroupBoxStyle == value) { return; }
                _GroupBoxStyle = value;
                Invalidate();
            }
        }


        [DefaultValue(0)]
        public new int TabIndex
        {
            get
            {
                return 0;
            }

            set
            {

                base.TabIndex = 0;
            }
        }

        [DefaultValue(false)]
        public new bool TabStop
        {
            get
            {
                return false;
            }

            set
            {
                base.TabStop = false;

            }
        }


        [DefaultValue("")]
        public new string Text
        {
            get
            {
                return _Text;
            }

            set
            {
                if (_Text == value) { return; }
                _Text = value;
                Invalidate();
                //OnTextChanged();
            }
        }


        #endregion



        protected override void DrawControl(Graphics gr, enStates state)
        {
            //if (_BitmapOfControl == null) { _BitmapOfControl = new Bitmap(ClientSize.Width, ClientSize.Height, PixelFormat.Format32bppPArgb); }

            //var TMPGR = Graphics.FromImage(_BitmapOfControl);

            if (ParentType() == enPartentType.RibbonPage) { _GroupBoxStyle = enGroupBoxStyle.RibbonBar; }


            switch (_GroupBoxStyle)
            {
                case enGroupBoxStyle.RibbonBar:

                    if (!Parent.Enabled) { state = enStates.Standard_Disabled; } // TabPage
                    if (!Parent.Parent.Enabled) { state = enStates.Standard_Disabled; }// RibbonBar
                                                                                       //  vState = enStates.Standard_Disabled
                    Skin.Draw_Back(gr, enDesign.RibbonBar_Frame, state, DisplayRectangle, this, true);
                    Skin.Draw_Border(gr, enDesign.RibbonBar_Frame, state, DisplayRectangle);
                    Skin.Draw_FormatedText(gr, _Text, enDesign.RibbonBar_Frame, state, null, enAlignment.Bottom_HorizontalCenter, new Rectangle(DisplayRectangle.Left, DisplayRectangle.Top, DisplayRectangle.Width, DisplayRectangle.Height + 2), this, false, Translate);

                    if (Dock != System.Windows.Forms.DockStyle.Left)
                    {
                        Dock = System.Windows.Forms.DockStyle.Left;
                        return;
                    }

                    ChildControls_RibbonBar();
                    break;
                case enGroupBoxStyle.Normal:
                    var r = new Rectangle(DisplayRectangle.Left + Skin.Padding, DisplayRectangle.Top, DisplayRectangle.Width, DisplayRectangle.Height);
                    Skin.Draw_Back(gr, enDesign.Frame, state, DisplayRectangle, this, true);

                    if (Height > 33)
                    {
                        Skin.Draw_Border(gr, enDesign.Frame, state, DisplayRectangle);
                        Skin.Draw_FormatedText(gr, _Text, enDesign.Frame, state, null, enAlignment.Top_Left, r, this, true, Translate);
                    }
                    break;

                default:
                    Skin.Draw_Back_Transparent(gr, DisplayRectangle, this);
                    break;
            }

            //gr.DrawImage(_BitmapOfControl, 0, 0);
            //TMPGR.Dispose();
        }

        private void ChildControls_RibbonBar()
        {

            if (Parent == null) { return; }
            if (Width < 10 || Height < 10) { return; }
            if (Controls.Count == 0) { return; }

            foreach (System.Windows.Forms.Control thisControl in Controls)
            {


                switch (thisControl)
                {

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