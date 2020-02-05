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

using BlueBasics;
using Encog.Engine.Network.Activation;
using Encog.ML.Data.Basic;
using Encog.Neural.Networks;
using Encog.Neural.Networks.Layers;
using Encog.Neural.Networks.Training.Propagation.Back;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using static BlueBasics.FileOperations;
using static BlueBasics.modAllgemein;

namespace BluePaint
{

    //enum enColor : int
    //{
    //    Black = 0,
    //    //Transparent = 1,
    //    Color = 1
    //}

    public partial class Tool_Brain : GenericTool  //  BlueControls.Forms.Form //
    {

        //int Schwelle = 120;
        int r = 4;
        //int rk = 1;

        bool stopping = false;



        BasicNetwork network = null;


        string f = string.Empty;

        public Tool_Brain()
        {
            InitializeComponent();


            f = System.Windows.Forms.Application.StartupPath + "\\..\\..\\Ressourcen\\ComicSharpOutline.eg";

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

                        if (InRange(r, px, py))
                        {
                            inp.Add(px.ToString() + ";" + py.ToString() + ";Bright");
                        }
                    }
                }

                oup.Add("black");

                #region Network
                network = new BasicNetwork();
                network.AddLayer(new BasicLayer(new ActivationLinear(), true, inp.Count));
                network.AddLayer(new BasicLayer(new ActivationSigmoid(), true, (int)((inp.Count + oup.Count) * 1.5)));
                network.AddLayer(new BasicLayer(new ActivationSigmoid(), true, oup.Count));
                network.Structure.FinalizeStructure();
                network.Reset();
                #endregion

            }




        }


        private bool InRange(int rad, int x, int y)
        {
            var isr = Math.Sqrt(x * x + y * y) - 0.2;
            return (isr <= rad);
        }



        private void btnLernen_Click(object sender, System.EventArgs e)
        {
            btnLernen.Enabled = false;
            btnStop.Enabled = true;


            stopping = false;

            var All = Directory.GetFiles(txtPath.Text, "*.png", SearchOption.AllDirectories);

            var allreadyUsed = new List<string>();
            var allreadyUsedInputs = new List<string>();

            Bitmap testI = null;

            foreach (var thisf in All)
            {
                if (!thisf.ToLower().Contains("ready"))
                {






                    if (testI == null)
                    {
                        testI = ((Bitmap)Image_FromFile(thisf)).AdjustContrast(100f);
                        OnOverridePic(testI);
                    }


                    var count = 1;
                    if (btnDrehen.Checked) { count = 8; }

                    do
                    {

                        count--;
                        var was = ((RotateFlipType)count);

                        var P = ((Bitmap)Image_FromFile(thisf)).AdjustContrast(100f);
                        var PR = (Bitmap)Image_FromFile(thisf.Trim(".png") + "-ready.png");
                        P.RotateFlip(was);
                        PR.RotateFlip(was);


                        for (var x = 0; x < P.Width; x++)
                        {
                            for (var y = 0; y < P.Height; y++)
                            {


                                var colorsinput = OneSet(P, x, y);
                                var colorsoutput = GetOutPixel(PR, x, y);
                                var colorsInputstring = ToString(colorsinput);
                                if (!allreadyUsedInputs.Contains(colorsInputstring))
                                {
                                    allreadyUsedInputs.Add(colorsInputstring);
                                    allreadyUsed.Add(ToString(colorsinput) + "X" + ToString(colorsoutput));

                                }

                            }
                        }

                        Develop.DoEvents();
                        if (stopping)
                        {
                            btnLernen.Enabled = true;
                            return;
                        }

                    } while (count > 0);


                }
            }

            allreadyUsed.Shuffle();

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


            //var learner1 = new Encog.Neural.Networks.Training.Propagation.SCG.ScaledConjugateGradient(network, dataset);
            //var learner1 = new Encog.Neural.Networks.Training.Propagation.Quick.QuickPropagation(network, dataset);
            var learner1 = new Encog.Neural.Networks.Training.Propagation.Back.Backpropagation(network, dataset);
            //learner1.LearningRate = 0.01;


            var lastime = new DateTime(1900, 1, 1);
            var lastimeerr = DateTime.Now;

            do
            {
                learner1.Iteration();


                if (DateTime.Now.Subtract(lastimeerr).TotalSeconds > 1)
                {
                    capFehlerrate.Text = "Error: " + learner1.Error;
                    Develop.DoEvents();
                    lastimeerr = DateTime.Now;
                }

                if (stopping)
                {
                    Encog.Persist.EncogDirectoryPersistence.SaveObject(new FileInfo(f), network);
                    btnLernen.Enabled = true;
                    return;
                }

                if (DateTime.Now.Subtract(lastime).TotalSeconds > 10)
                {
                    Encog.Persist.EncogDirectoryPersistence.SaveObject(new FileInfo(f), network);
                    OnOverridePic(testI);
                    btnAnwenden_Click(null, null);
                    lastime = DateTime.Now;
                }


            } while (true);



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
                        l[pos] = GetInputPixel(p, x + px, y + py) * 2 - 1;
                    }
                }
            }

            return l;
        }

        private double GetInputPixel(Bitmap p, int x, int y)
        {

            if (x < 0 || y < 0 || x >= p.Width || y >= p.Height) { return 1f; }

            var c = p.GetPixel(x, y);

            if (c.A < 20) { return 1f; }

            var br = 1 - c.GetBrightness();

            return 1 - (br * br * br * br * br);// * c.GetSaturation();

        }

        private void btnAnwenden_Click(object sender, System.EventArgs e)
        {
            var P = OnNeedCurrentPic();
            if (P == null) { return; }

            var NP = new Bitmap(P.Width, P.Height);




            for (var x = 0; x < P.Width; x++)
            {
                for (var y = 0; y < P.Height; y++)
                {

                    var colorsinput = OneSet(P, x, y);
                    var result = network.Compute(new BasicMLData(colorsinput, false));



                    //var res = enColor.Color;

                    //if (result[0] > 0.5)
                    //{
                    //    res = enColor.Black;
                    //}
                    //else if (result[1] > result[0] && result[1] > result[2])
                    //{
                    //    res = enColor.Transparent;
                    //}



                    var b = (int)(result[0] * 255);

                    NP.SetPixel(x, y, Color.FromArgb(b, 255 - b, b));




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
            var P = OnNeedCurrentPic();
            var NP = new Bitmap(P.Width, P.Height);

            for (var x = 0; x < P.Width; x++)
            {
                for (var y = 0; y < P.Height; y++)
                {
                    var p = (int)(GetInputPixel(P, x, y) * 255);


                    NP.SetPixel(x, y, Color.FromArgb(p, p, p));




                }
            }
            OnOverridePic(NP);
        }

        private double[] GetOutPixel(Bitmap pr, int x, int y)
        {
            var b = new double[] { 1 };

            var c = pr.GetPixel(x, y);

            if (c.GetBrightness() < 0.2) { b[0] = 0; }

            return b;
            //if (c.ToArgb() == -16777216)
            //{
            //    b[(int)enColor.Black] = 1;
            //}
            ////else if (c.A < 20)
            ////{
            ////    b[(int)enColor.Transparent] = 1;
            ////}
            //else
            //{
            //    b[(int)enColor.Color] = 1;
            //}

            //return b;

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



        private void btnStop_Click(object sender, System.EventArgs e)
        {
            btnStop.Enabled = false;
            stopping = true;

        }
    }
}