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
    public partial class GroupBox : IBackgroundBitmap
    {
        public GroupBox()
        {

            // Dieser Aufruf ist für den Designer erforderlich.
            InitializeComponent();

            // Fügen Sie Initialisierungen nach dem InitializeComponent()-Aufruf hinzu.
            SetNotFocusable();
            _MouseHighlight = false;
        }


        #region  Variablen 
        private Bitmap _BitmapOfControl;
        private bool _GeneratingBitmapOfControl;
        private string _Text = "";
        #endregion





        #region  Properties 

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
                if (_Text == value)
                {
                    return;
                }
                _Text = value;
                Invalidate();
            }
        }


        #endregion
        protected override void InitializeSkin()
        {
            SuspendLayout();
            Invalidate();
            Refresh();
            ResumeLayout();
        }


        protected override void DrawControl(Graphics GR, enStates vState)
        {
            if (_BitmapOfControl == null) { _BitmapOfControl = new Bitmap(ClientSize.Width, ClientSize.Height, PixelFormat.Format32bppPArgb); }

            var TMPGR = Graphics.FromImage(_BitmapOfControl);


            if (ParentType() == enPartentType.RibbonPage)
            {

                if (!Parent.Enabled) { vState = enStates.Standard_Disabled; } // TabPage
                if (!Parent.Parent.Enabled) { vState = enStates.Standard_Disabled; }// RibbonBar
                //  vState = enStates.Standard_Disabled
                Skin.Draw_Back(TMPGR, enDesign.RibbonBar_Frame, vState, DisplayRectangle, this, true);
                Skin.Draw_Border(TMPGR, enDesign.RibbonBar_Frame, vState, DisplayRectangle);
                Skin.Draw_FormatedText(TMPGR, _Text, enDesign.RibbonBar_Frame, vState, null, enAlignment.Bottom_HorizontalCenter, new Rectangle(DisplayRectangle.Left, DisplayRectangle.Top, DisplayRectangle.Width, DisplayRectangle.Height + 2), this, false);

                if (Dock != System.Windows.Forms.DockStyle.Left)
                {
                    Dock = System.Windows.Forms.DockStyle.Left;
                    return;
                }

                ChildControls_RibbonBar();

            }
            else
            {

                var r = new Rectangle(DisplayRectangle.Left + Skin.Padding, DisplayRectangle.Top, DisplayRectangle.Width, DisplayRectangle.Height);
                Skin.Draw_Back(TMPGR, enDesign.Frame, vState, DisplayRectangle, this, true);

                if (Height > 33)
                {
                    Skin.Draw_Border(TMPGR, enDesign.Frame, vState, DisplayRectangle);
                    Skin.Draw_FormatedText(TMPGR, _Text, enDesign.Frame, vState, null, enAlignment.Top_Left, r, this, true);
                }

            }

            GR.DrawImage(_BitmapOfControl, 0, 0);
            TMPGR.Dispose();

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
                        thisControl.Top = Convert.ToInt32(thisControl.Top / 22.0) * 22 + 2;
                        thisControl.Height = Math.Max(Convert.ToInt32(thisControl.Height / 22.0) * 22, 22);
                        break;

                    case Line _:
                        thisControl.Top = Convert.ToInt32(thisControl.Top / 22.0) * 22 + 2;
                        thisControl.Height = Math.Max(Convert.ToInt32(thisControl.Height / 22.0) * 22, 22);
                        break;

                    case Button _:
                    case ComboBox _:
                    case ListBox _:
                    case TextBox _:
                        thisControl.Top = Convert.ToInt32(thisControl.Top / 22.0) * 22 + 2;
                        thisControl.Height = Math.Max(Convert.ToInt32(thisControl.Height / 22.0) * 22, 22);
                        break;
                }
            }
        }



        public Bitmap BitmapOfControl()
        {
            if (_GeneratingBitmapOfControl) { return null; }
            _GeneratingBitmapOfControl = true;
            if (_BitmapOfControl == null) { Refresh(); }
            _GeneratingBitmapOfControl = false;
            return _BitmapOfControl;
        }






        protected override void OnSizeChanged(System.EventArgs e)
        {
            if (_BitmapOfControl != null)
            {
                if (_BitmapOfControl.Width < Width || _BitmapOfControl.Height < Height)
                {
                    _BitmapOfControl.Dispose();
                    _BitmapOfControl = null;
                }
            }

            Invalidate();
            base.OnSizeChanged(e);
        }


    }
}