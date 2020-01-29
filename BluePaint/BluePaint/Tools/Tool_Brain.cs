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

using BluePaint.EventArgs;
using BlueControls;
using System.Drawing;
using BlueBrain;
using BlueBasics;
using static BlueBasics.modAllgemein;
using static BlueBasics.FileOperations;
using System.IO;
using System;

namespace BluePaint
{
    public partial class Tool_Brain : GenericTool //  System.Windows.Forms.UserControl //  
    {

        int Schwelle = 120;

        BlueBrain.FullyConnectedNetwork Br = null;


        string f = string.Empty;

        public Tool_Brain()
        {
            InitializeComponent();


            f = System.Windows.Forms.Application.StartupPath + "\\SingleLine.brn";

            if (FileExists(f))
            {
                Br = new FullyConnectedNetwork(f);
            }
            else
            {

                var inp = new ListExt<string> {  "-2,-2", "-1,-2", "0,-2", "1,-2", "2,-2",
                                        "-2,-1", "-1,-1", "0,-1", "1,-1", "2,-1",
                                        "-2,0", "-1,0",  "1,0", "2,0",
                                        "-2,1", "-1,1", "0,1", "1,1", "2,1",
                                        "-2,2", "-1,2", "0,2", "1,2", "2,2"};
                var outp = new ListExt<string> { "0", "1", "2" }; // Transparent, Black, Color

                Br = new BlueBrain.FullyConnectedNetwork(inp, outp, 25);
                Br.Save(f);
            }




        }

        private void Learn()
        {
            OnForceUndoSaving();

            var All = Directory.GetFiles(txtPath.Text, "*.png", SearchOption.TopDirectoryOnly);


            foreach (var thisf in All)
            {


                if (!thisf.ToLower().Contains("ready"))
                {


                    var P = (Bitmap)Image_FromFile(thisf);
                    var PR = (Bitmap)Image_FromFile(thisf.Trim(".png") + "-ready.png");
                    OnOverridePic(P);

                    for (var x = 0; x < P.Width; x++)
                    {
                        for (var y = 0; y < P.Height; y++)
                        {
                            FillNetwork(P, x, y);

                            var c = PR.GetPixel(x, y);


                            var z = "0";
                            if (c.R == 0 && c.A > 200) { z = "1"; }
                            if (c.R == 255) { z = "2"; }


                            Br.Compute();

                            //if (Br.OutputLayer.SoftMax() != z)
                            //{
                            Br.BackPropagationGradiant(z, 1, false);
                            //}


                        }
                    }

                    Br.Save(f);
                }

            }
        }

        private void btnLernen_Click(object sender, System.EventArgs e)
        {
            for (var z = 0; z < 100; z++)
            {

                Learn();
                btnAnwenden_Click(null, null);
                Develop.DoEvents();
            }
        }

        private void FillNetwork(Bitmap p, int x, int y)
        {
            for (var px = -2; px <= 2; px++)
            {
                for (var py = -2; py <= 2; py++)
                {
                    var C = GetPixel(p, x + px, y + py);

                    if (px != 0 || py != 0)
                    {
                        var s = px.ToString() + "," + py.ToString();
                        Br.InputLayer.SetValue(s, C);
                    }
                }
            }
        }

        private float GetPixel(Bitmap p, int x, int y)
        {

            if (x < 0 || y < 0 || x >= p.Width || y >= p.Height) { return 0; }

            var c = p.GetPixel(x, y);

            var br = c.GetBrightness() * 255;
            var st = c.GetSaturation() * 255;  // Weiße Flächen haben keine Sättigung....

            if (br < Schwelle && st < Schwelle) { return ((float)c.A / 255); }

            return -1;

        }

        private void btnAnwenden_Click(object sender, System.EventArgs e)
        {
            //OnForceUndoSaving();

            var P = OnNeedCurrentPic();

            var NP = new Bitmap(P.Width, P.Height);




            for (var x = 0; x < P.Width; x++)
            {
                for (var y = 0; y < P.Height; y++)
                {
                    FillNetwork(P, x, y);
                    Br.Compute();


                    switch (Br.OutputLayer.SoftMax())
                    {
                        case "0":
                            NP.SetPixel(x, y, Color.Transparent);
                            break;

                        case "1":
                            NP.SetPixel(x, y, Color.Black);
                            break;

                        case "2":
                            var c = GetBestColor(P, x, y, true);


                            NP.SetPixel(x, y, c);
                            break;

                    }





                }
            }

            OnOverridePic(NP);



        }


        private Color GetPixelColor(Bitmap p, int x, int y)
        {

            if (x < 0 || y < 0 || x >= p.Width || y >= p.Height) { return Color.Transparent; }

           return p.GetPixel(x, y);



        }


        private Color GetBestColor(Bitmap p, int x, int y, bool weiter)
        {

            //var c = p.GetPixel(x, y);


            //for (var px = -5; px <= 5; px++)
            //{
            //    for (var py = -5; py <= 5; py++)
            //    {
            //        var cn = GetPixelColor(p, x + px, y + py);
            //        if (cn.GetSaturation() > c.GetSaturation()) { c = cn; }
            //    }
            //}

            return Color.White;


            //var c = p.GetPixel(x, y);

            //var br = c.GetBrightness() * 255;
            ////var st = c.GetSaturation() * 255;  // Weiße Flächen haben keine Sättigung....


            //if (br > 50 && c.A > 230) { return c; }

            //if (weiter)
            //{

            //    var c2 = GetBestColor(p, x, y - 1, false);
            //    if (c2.ToArgb() != 0) { return c2; }

            //    var c3 = GetBestColor(p, x, y + 1, false);
            //    if (c3.ToArgb() != 0) { return c3; }


            //    var c4 = GetBestColor(p, x + 1, y, false);
            //    if (c4.ToArgb() != 0) { return c4; }


            //    var c5 = GetBestColor(p, x + 1, y, false);
            //    if (c5.ToArgb() != 0) { return c5; }


            //    return Color.Magenta;
            //}



            return Color.FromArgb(0, 0, 0, 0);
        }

        private void btnLernmaske_Click(object sender, System.EventArgs e)
        {



            //OnForceUndoSaving();

            var P = OnNeedCurrentPic();

            var NP = new Bitmap(P.Width, P.Height);




            for (var x = 0; x < P.Width; x++)
            {
                for (var y = 0; y < P.Height; y++)
                {
                    var p = GetPixel(P, x, y);

                    if (p < 0)
                    {
                        NP.SetPixel(x, y, Color.Red);
                    }
                    else
                    {
                        var b = 255 - (int)(p * 255f);
                        NP.SetPixel(x, y, Color.FromArgb(b, b, b));
                    }



                }
            }

            OnOverridePic(NP);

        }
    }
}
