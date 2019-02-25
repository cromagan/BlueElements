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

using BlueBrain;
using BlueControls.Enums;
using BlueControls.Interfaces;
using System;
using System.Drawing;
using System.Drawing.Imaging;

namespace BlueControls.Controls
{
    public partial class BrainDrawer : GenericControl, IBackgroundBitmap
    {
        public BrainDrawer()
        {
            SetDoubleBuffering();
            InitializeComponent();
        }


        public FullyConnectedNetwork Brain = null;
        protected Bitmap _BitmapOfControl;






        private float ColorWidth(double V)
        {

            var W = Math.Abs((float)(V * 10));

            if (W < 1) { W = 1; }
            if (W > 15) { W = 15; }

            return W;


        }



        private Color ColorValue(double V)
        {
            if (V == 0) { return Color.FromArgb(155, 155, 0); }


            var W = Math.Abs((int)(V * 100 + 155));



            if (W > 255) { W = 255; }
            if (W < 155) { W = 155; }


            if (V > 0) { return Color.FromArgb(0, W, 0); }

            return Color.FromArgb(W, 0, 0);
        }

        protected override void DrawControl(Graphics gr, enStates state)
        {

            if (_BitmapOfControl == null) { SnapShot(false, false); }


            gr.DrawImage(_BitmapOfControl, 0, 0);

        }


        public void SnapShot(bool ComputeBRain, bool DoRefreh)

        {
            if (_BitmapOfControl == null)
            {
                _BitmapOfControl = new Bitmap(ClientSize.Width, ClientSize.Height, PixelFormat.Format32bppPArgb);
            }
            var TMPGR = Graphics.FromImage(_BitmapOfControl);



            TMPGR.Clear(Color.White);
            if (Brain != null)
            {

                if (ComputeBRain) { Brain.Compute(); }


                var SM = Brain.OutputLayer.SoftMax();

                const int Y = 40;
                const int X = 180;
                const int he = 24;
                const int wi = 80;


                var Black = new Pen(Color.FromArgb(0, 0, 0));
                var Black2 = new Pen(Color.FromArgb(255, 255, 0), 2);

                var BlackB = new SolidBrush(Color.FromArgb(0, 0, 0));
                var F = new Font("Arial", 6);


                for (var l = 0 ; l < Brain.Layers.Length ; l++)
                {

                    for (var n = 0 ; n < Brain.Layers[l].Neurones.Length ; n++)
                    {
                        var NV = Brain.Layers[l].Neurones[n].Value;
                        var da = new Rectangle(l * X, n * Y, wi, he);




                        for (var w = 0 ; w < Brain.Layers[l].Neurones[n].Weight.Length ; w++)
                        {
                            var WV = Brain.Layers[l].Neurones[n].Weight[w];
                            TMPGR.DrawLine(new Pen(ColorValue(WV), ColorWidth(WV)), l * X + wi - 2, n * Y + he / 2, (l + 1) * X + 2, w * Y + he / 2);
                        }

                        TMPGR.FillRectangle(new SolidBrush(ColorValue(NV)), da);


                        if (!string.IsNullOrEmpty(Brain.Layers[l].Neurones[n].Name)) { TMPGR.DrawString(Brain.Layers[l].Neurones[n].Name, F, BlackB, da.X + 1, da.Y + 1); }

                        TMPGR.DrawString(NV.ToString(), F, BlackB, da.X + 1, da.Y + 10);

                        //if (l > 0)
                        //{
                        //    TMPGR.DrawString(Brain.Layers[l].Neurones[n].Error.ToString(), F, new SolidBrush(ColorValue(Brain.Layers[l].Neurones[n].Error)), da.X + 1, da.Y + 24);
                        //}

                        if (l == Brain.Layers.Length - 1)
                        {
                            if (Brain.Layers[l].Neurones[n].Name == SM)
                            {
                                da.Inflate(-1, -1);
                                TMPGR.DrawRectangle(Black2, da);
                            }
                            else
                            {
                                TMPGR.DrawRectangle(Black, da);
                            }
                            TMPGR.DrawString(Math.Truncate(Brain.Layers[l].SoftMaxValue(Brain.Layers[l].Neurones[n].Name)*100).ToString(), F, BlackB, da.Right-20, da.Y + 1);

                        }
                        else
                        {
                            TMPGR.DrawRectangle(Black, da);
                        }




                    }
                }


            }
            TMPGR.Dispose();
            if (DoRefreh) { Refresh(); }

        }





        protected override void InitializeSkin()
        {

        }

        public Bitmap BitmapOfControl()
        {
            return _BitmapOfControl;
        }
    }
}
