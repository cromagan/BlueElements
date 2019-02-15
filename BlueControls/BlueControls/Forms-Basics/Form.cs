using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using BlueBasics;
using BlueBasics.Enums;
using BlueControls.Controls;
using BlueControls.Enums;

namespace BlueControls.Forms
{
    public partial class Form : BlueControls.Interfaces.IDesignAble
    {
        public Form()
        {
            InitializeComponent();
        }

        private bool _Closed;

        /// <summary>
        /// Die Dicke des linken und rechen Randes einer Form in Pixel
        /// </summary>
        public static readonly int BorderWidth = 16;

        /// <summary>
        /// Die Dicke des oberen Balkens und unteren Randes einer Form in Pixel
        /// </summary>
        public static readonly int BorderHeight = 39;

        /// <summary>
        /// Die Dicke des unteren Rahmens einer Form in Pixel
        /// </summary>
        public static readonly int BorderBottom = 8;

        /// <summary>
        /// Die Dicke des oberen Balken einer Form in Pixel
        /// </summary>
        public static readonly int BorderTop = 31;

        protected override void OnPaint(System.Windows.Forms.PaintEventArgs e)
        {
            if (!_Closed) { base.OnPaint(e); }
        }


        #region  AutoScale deaktivieren 
        // https://msdn.microsoft.com/de-de/library/ms229605(v=vs.110).aspx



        public new void PerformAutoScale()
        {
            // NIX TUN!!!!
        }

        public void Scale()
        {
            // NIX TUN!!!!
        }

        private enDesign _design = enDesign.Form_Standard;

        private bool _CloseButtonEnabled = true;

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

        [DefaultValue(enDesign.Form_Standard)]
        public enDesign Design
        {
            get
            {
                return _design;
            }
            set
            {
                if (value == _design) { return; }
                _design = value;
                SkinChanged(this, System.EventArgs.Empty);
            }
        }

        [DefaultValue(true)]
        public bool CloseButtonEnabled
        {
            get
            {
                return _CloseButtonEnabled;
            }
            set
            {

                _CloseButtonEnabled = value;
            }
        }


        protected override Rectangle GetScaledBounds(Rectangle tbounds, SizeF factor, System.Windows.Forms.BoundsSpecified specified)
        {
            return tbounds; //MyBase.GetScaledBounds(bounds, factor, specified)
        }
        #endregion


        protected override void OnClosed(System.EventArgs e)
        {
            GenericControl.Skin.SkinChanged -= SkinChanged;
            _Closed = true;
            base.OnClosed(e);
        }

        protected override void OnFormClosing(System.Windows.Forms.FormClosingEventArgs e)
        {
            if (_Closed) { return; }
            base.OnFormClosing(e);
            if (!e.Cancel) { _Closed = true; }
        }

        protected override void OnCreateControl()
        {
            Develop.StartService();
            base.OnCreateControl();
        }



        protected override void OnResize(System.EventArgs e)
        {
            if (!_Closed) { base.OnResize(e); }
        }

        protected override void OnSizeChanged(System.EventArgs e)
        {
            if (!_Closed) { base.OnSizeChanged(e); }
        }


        protected override void OnLoad(System.EventArgs e)
        {
            BackColor = GenericControl.Skin.Color_Back(_design, enStates.Standard);
            base.OnLoad(e);
            GenericControl.Skin.SkinChanged += SkinChanged;
            //Develop.Debugprint_BackgroundThread();
        }


        protected override void OnResizeBegin(System.EventArgs e)
        {
            if (!_Closed) { base.OnResizeBegin(e); }
        }

        protected override void OnResizeEnd(System.EventArgs e)
        {
            if (!_Closed) { base.OnResizeEnd(e); }
        }

        protected override void OnInvalidated(System.Windows.Forms.InvalidateEventArgs e)
        {
            if (!_Closed) { base.OnInvalidated(e); }
        }

        private void SkinChanged(object sender, System.EventArgs e)
        {
            BackColor = GenericControl.Skin.Color_Back(_design, enStates.Standard);
            SuspendLayout();
            Invalidate();
            ResumeLayout();
        }










        public List<Button> Generate_Buttons(string[] Names)
        {
            var MyX = this.Width - GenericControl.Skin.Padding - BorderWidth;
            var erT = new ExtText(enDesign.Button, enStates.Standard);
            var Buts = new List<Button>();

            for (var Z = Names.GetUpperBound(0) ; Z > -1 ; Z--)
            {
                if (!string.IsNullOrEmpty(Names[Z]))
                {
                    var B = new Button();

                    erT.Autoumbruch = false;
                    erT.PlainText = Names[Z];
                    B = new Button();
                    B.Name = Z.ToString();
                    B.Text = Names[Z];
                    int W = 2;

                    switch (B.Text.ToLower())
                    {
                        case "ja":
                        case "ok":
                            B.ImageCode = "Häkchen|16";
                            W = 4;
                            break;

                        case "nein":
                        case "abbrechen":
                        case "abbruch":
                            B.ImageCode = "Kreuz|16";
                            W = 4;
                            break;

                        case "löschen":
                            B.ImageCode = "Papierkorb|16";
                            W = 4;
                            break;

                        default:
                            B.ImageCode = string.Empty;
                            break;
                    }

                    B.Size = new Size(erT.Width() + GenericControl.Skin.Padding * W, erT.Height() + GenericControl.Skin.Padding * 2);
                    B.Location = new Point(MyX - B.Width, this.Height - BorderHeight - GenericControl.Skin.Padding - B.Height);
                    MyX = B.Location.X - GenericControl.Skin.Padding;

                    B.ButtonStyle = enButtonStyle.Button;
                    B.Visible = true;
                    B.Anchor = System.Windows.Forms.AnchorStyles.Right | System.Windows.Forms.AnchorStyles.Bottom;
                    Controls.Add(B);
                    Buts.Add(B);

                }
            }
            return Buts;
        }

        public bool IsMouseInForm()
        {
            return new Rectangle(Location, Size).Contains(System.Windows.Forms.Cursor.Position);
        }


        protected override System.Windows.Forms.CreateParams CreateParams
        {
            get
            {
                var oParam = base.CreateParams;
                if (!_CloseButtonEnabled)
                {
                    oParam.ClassStyle |= (int)enCS.NOCLOSE;
                }
                return oParam;
            }
        }
    }
}