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
        int r = 6;

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

                var inp = new ListExt<string>();
                var oup = new ListExt<string>();
                for (var px = -r; px <= r; px++)
                {
                    for (var py = -r; py <= r; py++)
                    {

                        if (InRange(px, py))
                        {
                            inp.Add(px.ToString() + ";" + py.ToString());
                            oup.Add(px.ToString() + ";" + py.ToString());
                        }

                    }
                }

                oup.Add("black");
                oup.Add("transparent");
                //oup.Add("original");

                Br = new BlueBrain.FullyConnectedNetwork(inp, oup, r * r * 4);
                Br.Save(f);
            }




        }


        private bool InRange(int x, int y)
        {
            var isr = Math.Sqrt(x * x + y * y);
            return (isr <= r);
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
                            var OnlyColored = FillNetwork(P, x, y);

                            var c = PR.GetPixel(x, y);


                            if (OnlyColored)
                            {
                                Br.BackPropagationGradiant("0;0", 0.1, true); // = Original-Farbe
                            }
                            else if (c.R == 0 && c.A > 200)
                            {
                                Br.BackPropagationGradiant("black", 0.1, true);
                            }
                            else if (c.A < 20)
                            {
                                Br.BackPropagationGradiant("transparent", 0.1, true);
                            }
                            else
                            {
                                for (var px = -r; px <= r; px++)
                                {
                                    for (var py = -r; py <= r; py++)
                                    {
                                        if (InRange(px, py))
                                        {
                                            var CV = GetPixelColor(P, x + px, y + py);
                                            var CO = GetPixelColor(PR, x + px, y + py);
                                            if (CV.ToArgb() == CO.ToArgb())
                                            {
                                                var s = px.ToString() + ";" + py.ToString();
                                                Br.BackPropagationGradiant(s, 0.1, true);
                                            }
                                        }
                                    }
                                }
                            }
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

        private bool FillNetwork(Bitmap p, int x, int y)
        {
            var OnlyColoredArea = true;

            for (var px = -r; px <= r; px++)
            {
                for (var py = -r; py <= r; py++)
                {
                    if (InRange(px, py))
                    {
                        var C = GetPixel(p, x + px, y + py);
                        var s = px.ToString() + ";" + py.ToString();
                        Br.InputLayer.SetValue(s, C);
                        if (C >= 0) { OnlyColoredArea = false; }
                    }
                }
            }

            return OnlyColoredArea;
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
                        case "transparent":
                            NP.SetPixel(x, y, Color.Transparent);
                            break;

                        case "black":
                            NP.SetPixel(x, y, Color.Black);
                            break;

                        default:
                            var l = Br.OutputLayer.SoftMax().SplitBy(";");

                            var c = GetPixelColor(P, x + int.Parse(l[0]), y + int.Parse(l[1]));


                            NP.SetPixel(x, y, Color.Red);
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
