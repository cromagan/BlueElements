﻿#region BlueElements - a collection of useful tools, database and controls
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

using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using BlueBasics;
using BlueControls.Controls;
using BlueControls.Enums;

namespace BlueControls.Forms
{
    public partial class Form : System.Windows.Forms.Form, BlueControls.Interfaces.IDesignAble
    {
        public Form(): base()
        {

            if (Skin.SkinDB == null) { Skin.LoadSkin(); }
            InitializeComponent();
        }

        public bool IsClosed { get; private set; }

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
            if (!IsClosed && !IsDisposed) { base.OnPaint(e); }
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

                if (Skin.SkinDB == null) { Skin.LoadSkin(); }

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


        protected override Rectangle GetScaledBounds(Rectangle bounds, SizeF factor, System.Windows.Forms.BoundsSpecified specified)
        {
            return bounds; //MyBase.GetScaledBounds(bounds, factor, specified)
        }
        #endregion



        protected override void OnFormClosing(System.Windows.Forms.FormClosingEventArgs e)
        {
            //https://docs.microsoft.com/en-us/dotnet/api/system.windows.forms.form.closed?view=netframework-4.8
            if (IsClosed) { return; }
            base.OnFormClosing(e);
            if (!e.Cancel)
            {
                IsClosed = true;
                Skin.SkinChanged -= SkinChanged;
            }
        }



        protected override void OnCreateControl()
        {
            Develop.StartService();
            Table.StartDatabaseService();
            base.OnCreateControl();
        }



        protected override void OnResize(System.EventArgs e)
        {
            if (!IsClosed) { base.OnResize(e); }
        }

        protected override void OnSizeChanged(System.EventArgs e)
        {
            if (!IsClosed) { base.OnSizeChanged(e); }
        }


        protected override void OnLoad(System.EventArgs e)
        {
            BackColor = Skin.Color_Back(_design, enStates.Standard);
            base.OnLoad(e);
            Skin.SkinChanged += SkinChanged;
        }


        protected override void OnResizeBegin(System.EventArgs e)
        {
            if (!IsClosed) { base.OnResizeBegin(e); }
        }

        protected override void OnResizeEnd(System.EventArgs e)
        {
            if (!IsClosed) { base.OnResizeEnd(e); }
        }

        protected override void OnInvalidated(System.Windows.Forms.InvalidateEventArgs e)
        {
            if (!IsClosed) { base.OnInvalidated(e); }
        }

        private void SkinChanged(object sender, System.EventArgs e)
        {
            BackColor = Skin.Color_Back(_design, enStates.Standard);
            SuspendLayout();
            Invalidate();
            ResumeLayout();
        }









        public List<Button> Generate_Buttons(string[] Names)
        {
            var MyX = this.Width - Skin.Padding - BorderWidth;
            var erT = new ExtText(enDesign.Button, enStates.Standard);
            var Buts = new List<Button>();

            for (var Z = Names.GetUpperBound(0); Z > -1; Z--)
            {
                if (!string.IsNullOrEmpty(Names[Z]))
                {
                    var B = new Button();

                    erT.TextDimensions = Size.Empty;
                    erT.PlainText = Names[Z];
                    B = new Button();
                    B.Name = Z.ToString();
                    B.Text = Names[Z];
                    var W = 2;

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

                        case "verwerfen":
                        case "löschen":
                            B.ImageCode = "Papierkorb|16";
                            W = 4;
                            break;

                        case "speichern":
                        case "sichern":
                            B.ImageCode = "Diskette|16";
                            W = 4;
                            break;

                        case "laden":
                            B.ImageCode = "Ordner|16";
                            W = 4;
                            break;


                        default:
                            B.ImageCode = string.Empty;
                            break;
                    }

                    B.Size = new Size(erT.Width() + Skin.Padding * W, erT.Height() + Skin.Padding * 2);
                    B.Location = new Point(MyX - B.Width, this.Height - BorderHeight - Skin.Padding - B.Height);
                    MyX = B.Location.X - Skin.Padding;

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