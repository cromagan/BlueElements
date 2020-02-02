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
using BlueBasics;
using static BlueBasics.modAllgemein;
using static BlueBasics.FileOperations;
using System.IO;
using System;
using System.Windows.Forms;
using Encog.Engine.Network.Activation;
using Encog.Neural.Networks;
using Encog.Neural.Networks.Layers;
using Encog.Neural.Networks.Training;
using Encog.Neural.Networks.Training.Propagation.Back;
using Encog.ML.Data;
using Encog.ML.Data.Basic;
using System.Collections.Generic;

namespace BluePaint
{

    enum enColor : int
    {
        Black = 0,
        Transparent = 1,
        Color = 2
    }

    public partial class Tool_Brain : GenericTool //  System.Windows.Forms.UserControl //  
    {

        int Schwelle = 120;
        int r = 8;
        //int rk = 1;

        Bitmap testI = null;



        BasicNetwork network = null;


        string f = string.Empty;

        public Tool_Brain()
        {
            InitializeComponent();


            f = System.Windows.Forms.Application.StartupPath + "\\SingleLine.brn";

            if (FileExists(f))
            {
                network = (BasicNetwork)(Encog.Persist.EncogDirectoryPersistence.LoadObject(new FileInfo(f)));
            }
            else
            {

                var inp = new ListExt<string>();
                var oup = new ListExt<string>();
                for (var px = -r; px <= r; px++)
                {
                    for (var py = -r; py <= r; py++)
                    {

                        if (InRange(r, px, py)) { inp.Add(px.ToString() + ";" + py.ToString()); }
                        //if (InRange(rk, px, py)) { oup.Add(px.ToString() + ";" + py.ToString()); }
                    }
                }

                oup.Add("black");
                oup.Add("transparent");
                oup.Add("original");

                #region Network
                network = new BasicNetwork();
                network.AddLayer(new BasicLayer(new ActivationSigmoid(), true, inp.Count));
                network.AddLayer(new BasicLayer(new ActivationSigmoid(), true, (int)((inp.Count + oup.Count) * 1.5)));
                network.AddLayer(new BasicLayer(new ActivationSigmoid(), true, oup.Count));
                network.Structure.FinalizeStructure();
                network.Reset();
                #endregion



            }




        }


        private bool InRange(int rad, int x, int y)
        {
            var isr = Math.Sqrt(x * x + y * y);
            return (isr <= rad);
        }



        private void btnLernen_Click(object sender, System.EventArgs e)
        {
            var All = Directory.GetFiles(txtPath.Text, "*.png", SearchOption.TopDirectoryOnly);

            var allreadyUsed = new List<string>();


            foreach (var thisf in All)
            {
                if (!thisf.ToLower().Contains("ready"))
                {

                    var P = (Bitmap)Image_FromFile(thisf);
                    var PR = (Bitmap)Image_FromFile(thisf.Trim(".png") + "-ready.png");

                    if (testI == null)
                    {
                        testI = (Bitmap)Image_FromFile(thisf);
                    }

                    for (var x = 0; x < P.Width; x++)
                    {
                        for (var y = 0; y < P.Height; y++)
                        {
                            var colorsinput = OneSet(P, x, y);
                            var colorsoutput = GetOutPixel(PR, x, y);
                            var colorsstring = ToString(colorsinput) + "X" + ToString(colorsoutput); //Beide, wegen 50/50
                            if (!allreadyUsed.Contains(colorsstring))
                            {
                                allreadyUsed.Add(colorsstring);
                            }

                        }
                    }

                }
            }


            var inputs = new double[allreadyUsed.Count][];
            var outputs = new double[allreadyUsed.Count][];
            var pos = -1;

            foreach (var thisx in allreadyUsed)
            {
                var t = thisx.SplitBy("X");
                pos++;

                inputs[pos] = ToDoubleArray(t[0]);
                outputs[pos] = ToDoubleArray(t[1]);


            }



            var dataset = new BasicMLDataSet(inputs, outputs);


            var leaerner = new Backpropagation(network, dataset);
            leaerner.LearningRate = 0.4;

            for (var i = 0; i < 300000; i++)
            {
                leaerner.Iteration();

                if (i % 20 == 0)
                {
                    SaveNetWork(f);
                    btnAnwenden_Click(null, null);
                }

                Develop.DoEvents();
                //Console.WriteLine("error: " + leaerner.Error);

            }



        }




        private double[] OneSet(Bitmap p, int x, int y)
        {

            var l = new double[network.InputCount];

            var pos = -1;

            for (var px = -r; px <= r; px++)
            {
                for (var py = -r; py <= r; py++)
                {
                    if (InRange(r, px, py))
                    {
                        pos++;
                        l[pos] = GetInputPixel(p, x + px, y + py);
                    }
                }
            }

            return l;
        }

        private double GetInputPixel(Bitmap p, int x, int y)
        {

            if (x < 0 || y < 0 || x >= p.Width || y >= p.Height) { return 0; }

            var c = p.GetPixel(x, y);

            var br = c.GetBrightness() * 255;
            var st = c.GetSaturation() * 255;  // Weiße Flächen haben keine Sättigung....

            if (c.R < 20 && c.G < 20 && c.B < 20) { st = 0; }

            if (br < Schwelle && st < Schwelle) { return ((float)c.A / 255); }

            return -1;

        }

        private void btnAnwenden_Click(object sender, System.EventArgs e)
        {
            OnForceUndoSaving();

            Bitmap P = null;

            if (testI != null)
            {
                P = (Bitmap)testI.Clone();
            }
            else
            {
                P = OnNeedCurrentPic();
            }


            if (P == null) { return; }

            var NP = new Bitmap(P.Width, P.Height);




            for (var x = 0; x < P.Width; x++)
            {
                for (var y = 0; y < P.Height; y++)
                {

                    var colorsinput = OneSet(P, x, y);
                    var result = network.Compute(new BasicMLData(colorsinput, false));



                    var res = enColor.Color;

                    if (result[0] > result[1] && result[0] > result[2])
                    {
                        res = enColor.Black;
                    }
                    else if (result[1] > result[0] && result[1] > result[2])
                    {
                        res = enColor.Transparent;
                    }



                    switch (res)
                    {
                        case enColor.Transparent:
                            NP.SetPixel(x, y, Color.Transparent);
                            break;

                        case enColor.Black:
                            NP.SetPixel(x, y, Color.Black);
                            break;

                        default:
                            //var l = Br.OutputLayer.SoftMax().SplitBy(";");

                            //var c = GetPixelColor(P, x + int.Parse(l[0]), y + int.Parse(l[1]));


                            NP.SetPixel(x, y, Color.Red);
                            break;

                    }





                }
            }

            OnOverridePic(NP);
            Develop.DoEvents();



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
                    var p = GetInputPixel(P, x, y);

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

        public void SaveNetWork(string f)
        {
            var networkFile = new FileInfo(f);
            Encog.Persist.EncogDirectoryPersistence.SaveObject(networkFile, network);
        }


        private double[] GetOutPixel(Bitmap pr, int x, int y)
        {
            var b = new double[] { 0, 0, 0 };

            var c = pr.GetPixel(x, y);

            if (c.ToArgb() == -16777216)
            {
                b[(int)enColor.Black] = 1;
            }
            else if (c.A < 20)
            {
                b[(int)enColor.Transparent] = 1;
            }
            else
            {
                b[(int)enColor.Color] = 1;
            }

            return b;

        }

        private string ToString(double[] v)
        {
            var sb = new System.Text.StringBuilder();

            foreach (var thsd in v)
            {
                sb.Append(thsd.ToString(Constants.Format_Float4));
                sb.Append(";");
            }


            return sb.ToString();

        }



        private double[] ToDoubleArray(string v)
        {
            var s = v.TrimEnd(";").SplitBy(";");

            var d = new double[s.GetUpperBound(0) + 1];

            var pos = -1;

            foreach (var thiss in s)
            {
                pos++;
                d[pos] = double.Parse(thiss);
            }
            return d;

        }

    }
}