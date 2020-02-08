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
using BlueControls.EventArgs;
using Encog.Engine.Network.Activation;
using Encog.ML.Data.Basic;
using Encog.Neural.Networks;
using Encog.Neural.Networks.Layers;
using Encog.Neural.Networks.Training.Propagation.Back;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using static BlueBasics.FileOperations;
using static BlueBasics.modAllgemein;
using static BlueBasics.Extensions;
namespace BluePaint
{

    public partial class Tool_Brain : GenericTool  // BlueControls.Forms.Form //
    {

        int AlphaSchwelle = 200;
        //int Schwelle = 120;
        int r = 4;
        //int rk = 1;

        float isBlack = 0.2f;

        bool stopping = false;

        Bitmap _VisibleLernImageSource = null;
        Bitmap _VisibleLernImageTarget = null;

        BasicNetwork network = null;


        string f = string.Empty;

        public Tool_Brain() : base()
        {
            InitializeComponent();


            f = System.Windows.Forms.Application.StartupPath + "\\..\\..\\Ressourcen\\ComicSharpOutline.eg";

            if (!FileExists(f))
            {
                f = System.Windows.Forms.Application.StartupPath + "\\ComicSharpOutline.eg";
            }


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
                network.AddLayer(new BasicLayer(new ActivationSigmoid(), true, (int)((inp.Count + oup.Count) * 2)));
                network.AddLayer(new BasicLayer(new ActivationSigmoid(), true, (int)((inp.Count + oup.Count) * 2)));
                network.AddLayer(new BasicLayer(new ActivationSigmoid(), true, (int)((inp.Count + oup.Count) * 2)));
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

            _VisibleLernImageSource = null;
            _VisibleLernImageTarget = null;


            stopping = false;

            var All = Directory.GetFiles(txtPath.Text, "*.png", SearchOption.AllDirectories);

            var toLearn = new List<string>();
            var allreadyUsedInputs = new List<string>();



            foreach (var thisf in All)
            {
                if (!thisf.ToLower().Contains("ready"))
                {






                    if (_VisibleLernImageSource == null)
                    {
                        _VisibleLernImageSource = ((Bitmap)Image_FromFile(thisf));//.AdjustContrast(100f);
                        _VisibleLernImageTarget = (Bitmap)Image_FromFile(thisf.Trim(".png") + "-ready.png");
                        OnOverridePic(_VisibleLernImageSource);
                    }


                    var count = 1;
                    if (btnDrehen.Checked) { count = 8; }

                    do
                    {

                        count--;
                        var was = ((RotateFlipType)count);

                        var P = ((Bitmap)Image_FromFile(thisf));
                        var PR = (Bitmap)Image_FromFile(thisf.Trim(".png") + "-ready.png");
                        P.RotateFlip(was);
                        PR.RotateFlip(was);

                        GetInputLearnPixel(P, PR, 0, 0, P.Width, P.Height, allreadyUsedInputs, toLearn);
                        Develop.DoEvents();

                        if (stopping)
                        {
                            btnLernen.Enabled = true;
                            return;
                        }

                    } while (count > 0);


                }
            }

            Learn(toLearn, -1);



        }

        private void Learn(List<string> toLearn, int Count)
        {
            if (toLearn.Count == 0) { return; }

            toLearn.Shuffle();

            var inputs = new double[toLearn.Count][];
            var outputs = new double[toLearn.Count][];
            var pos = -1;

            foreach (var thisx in toLearn)
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

            var current = 0;

            do
            {
                current++;
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
                    if (Count < 0)
                    {
                        Encog.Persist.EncogDirectoryPersistence.SaveObject(new FileInfo(f), network);
                    }

                    OnOverridePic(_VisibleLernImageSource);
                    DoModus(0);
                    lastime = DateTime.Now;
                    if (Count > 0 && current > Count) { break; }
                }




            } while (true);
        }

        private void GetInputLearnPixel(Bitmap sourceBMP, Bitmap targetBMP, int xs, int ys, int width, int height, List<string> allreadyUsedInputs, List<string> toLearn)
        {
            for (var x = Math.Max(xs, 0); x < Math.Min(xs + width, sourceBMP.Width); x++)
            {
                for (var y = Math.Max(ys, 0); y < Math.Min(ys + height, sourceBMP.Height); y++)
                {


                    var colorsinput = OneSet(sourceBMP, x, y);
                    var colorsoutput = GetOutPixel(targetBMP, x, y);
                    var colorsInputstring = ToString(colorsinput);
                    if (!allreadyUsedInputs.Contains(colorsInputstring))
                    {
                        allreadyUsedInputs.Add(colorsInputstring);
                        toLearn.Add(ToString(colorsinput) + "X" + ToString(colorsoutput));

                    }

                }
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

            //if (c.A < 20) { return 1f; }



            return BrightenExpo(c);

        }


        private List<Color> UsedColors(Bitmap P)
        {

            var c = new List<Color>();


            for (var x = 0; x < P.Width - 1; x++)
            {
                for (var y = 0; y < P.Height - 1; y++)
                {

                    var col = P.GetPixel(x, y);

                    //if (!c.Contains(col))
                    //{
                    //    if (BrightenExpo(col) > 0.99) { c.Add(col); }
                    //}

                    if (!c.Contains(col))
                    {
                        if (P.GetPixel(x + 1, y + 1).ToArgb() == col.ToArgb() && P.GetPixel(x + 1, y).ToArgb() == col.ToArgb() && P.GetPixel(x, y + 1).ToArgb() == col.ToArgb())
                        {
                            if (BrightenExpo(col) > 0.3) { c.Add(col); }
                        }
                    }




                }
            }
            return c;
        }


        private float BrightenExpo(Color col)
        {
            var br = 1 - col.GetBrightness();

            return 1 - ((br * br * br) * (Math.Min((int)(col.A * 1.2), 255) / 255));

        }



        public int BlackValue(Bitmap P, int x, int y, int modus)
        {
            var colorsinput = OneSet(P, x, y);
            var result = network.Compute(new BasicMLData(colorsinput, false));


            if (modus == 1)
            {
                if (result[0] < 0.7) { return 255; }
                return 0;

            }



            return 255 - (int)(result[0] * 255);


        }


        //public bool Black(Bitmap P, int x, int y)
        //{


        //    return (BlackValue(P, x, y) < 180);
        //}


        private void btnAnwendenFarbe_Click(object sender, System.EventArgs e)
        {
            OnCommandForMacro("Anwenden;1");
            DoModus(1);
        }
        private void btnAnwendenSW_Click(object sender, System.EventArgs e)
        {
            OnCommandForMacro("Anwenden;0");
            DoModus(0);
        }

        private Color GetNearestColor(List<Color> allcolors, Bitmap p, int x, int y)
        {
            var checkcol = p.GetPixel(x, y);

            var Col = GetNearestColor(allcolors, GetPixelColor(p, x, y));
            if (!Col.IsMagenta()) { return Col; }


            var left = false;
            var right = false;
            var up = false;
            var down = false;


            var upleft = false;
            var upright = false;
            var downleft = false;
            var downright = false;

            for (var ters = 1; ters <= 15; ters++)
            {


                if (!down)
                {
                    var c = GetPixelColor(p, x, y + ters);
                    if (c.A < AlphaSchwelle) { return Color.Transparent; }
                    if (BrightenExpo(c) < 0.5)
                    {
                        down = true;
                    }
                    else
                    {
                        Col = GetNearestColor(allcolors, c);
                        if (!Col.IsMagenta()) { return Col; }
                    }
                }

                if (!up)
                {
                    var c = GetPixelColor(p, x, y - ters);
                    if (c.A < AlphaSchwelle) { return Color.Transparent; }
                    if (BrightenExpo(c) < 0.5)
                    {
                        up = true;
                    }
                    else
                    {
                        Col = GetNearestColor(allcolors, c);
                        if (!Col.IsMagenta()) { return Col; }
                    }


                }

                if (!left)
                {
                    var c = GetPixelColor(p, x - ters, y);
                    if (c.A < AlphaSchwelle) { return Color.Transparent; }
                    if (BrightenExpo(c) < 0.5)
                    {
                        left = true;
                    }
                    else
                    {
                        Col = GetNearestColor(allcolors, c);
                        if (!Col.IsMagenta()) { return Col; }
                    }

                }

                if (!right)
                {
                    var c = GetPixelColor(p, x + ters, y);
                    if (c.A < AlphaSchwelle) { return Color.Transparent; }
                    if (BrightenExpo(c) < 0.5)
                    {
                        right = true;
                    }
                    else
                    {
                        Col = GetNearestColor(allcolors, c);
                        if (!Col.IsMagenta()) { return Col; }
                    }

                }

                if (!upleft)
                {
                    var c = GetPixelColor(p, x - ters, y - ters);
                    if (c.A < AlphaSchwelle) { return Color.Transparent; }
                    if (BrightenExpo(c) < 0.5)
                    {
                        upleft = true;
                    }
                    else
                    {
                        Col = GetNearestColor(allcolors, c);
                        if (!Col.IsMagenta()) { return Col; }
                    }

                }

                if (!upright)
                {
                    var c = GetPixelColor(p, x + ters, y - ters);
                    if (c.A < AlphaSchwelle) { return Color.Transparent; }
                    if (BrightenExpo(c) < 0.5)
                    {
                        upright = true;
                    }
                    else
                    {
                        Col = GetNearestColor(allcolors, c);
                        if (!Col.IsMagenta()) { return Col; }
                    }

                }


                if (!downleft)
                {
                    var c = GetPixelColor(p, x - ters, y + ters);
                    if (c.A < AlphaSchwelle) { return Color.Transparent; }
                    if (BrightenExpo(c) < 0.5)
                    {
                        downleft = true;
                    }
                    else
                    {
                        Col = GetNearestColor(allcolors, c);
                        if (!Col.IsMagenta()) { return Col; }
                    }

                }

                if (!downright)
                {
                    var c = GetPixelColor(p, x + ters, y + ters);
                    if (c.A < AlphaSchwelle) { return Color.Transparent; }
                    if (BrightenExpo(c) < 0.5)
                    {
                        downright = true;
                    }
                    else
                    {
                        Col = GetNearestColor(allcolors, c);
                        if (!Col.IsMagenta()) { return Col; }
                    }

                }


                if (up && down && left && right && downleft && downright && upleft && upright)
                {
                    up = false;
                    down = false;
                    left = false;
                    right = false;

                    upleft = false;
                    upright = false;
                    downleft = false;
                    downright = false;
                }


            }

            return Color.Magenta;
        }

        private Color GetNearestColor(List<Color> allcolors, Color checkcol)
        {

            if (checkcol.A < AlphaSchwelle) { return Color.Transparent; }


            if (BrightenExpo(checkcol) < 0.4) { return Color.Magenta; }

            return checkcol.ClosestHSVColor(allcolors, 0.5f, 1f);

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
            if (c.GetBrightness() < isBlack) { b[0] = 0; }
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



        private void btnStop_Click(object sender, System.EventArgs e)
        {
            btnStop.Enabled = false;
            stopping = true;

        }


        public override string MacroKennung()
        {
            return "NormalizeOutline";
        }

        public override void ExcuteCommand(string command)
        {
            var c = command.SplitBy(";");

            switch (c[0])
            {

                case "Anwenden":
                    DoModus(int.Parse(c[1]));
                    break;


                default:

                    Develop.DebugPrint_NichtImplementiert();
                    break;
            }
        }

        //public override void MouseUp(MouseEventArgs1_1DownAndCurrent e, Bitmap OriginalPic)
        //{
        //    base.MouseUp(e, OriginalPic);


        //    if (!e.Current.IsInPic) { return; }

        //    var toLearn = new List<string>();
        //    var allreadyUsedInputs = new List<string>();

        //    var x = e.Current.X;
        //    var y = e.Current.Y;

        //    GetInputLearnPixel(_VisibleLernImageSource, _VisibleLernImageTarget, x - 5, y - 5, 10, 10, allreadyUsedInputs, toLearn);
        //    stopping = false;

        //    btnStop.Enabled = true;
        //    {

        //        Learn(toLearn, 1000);
        //        Develop.DoEvents();



        //        if (Black(_VisibleLernImageSource, x, y))
        //        {
        //            if (GetOutPixel(_VisibleLernImageTarget, x, y)[0] < isBlack)
        //            {
        //                return;
        //            }
        //        }
        //        else
        //        {
        //            if (GetOutPixel(_VisibleLernImageTarget, x, y)[0] >= isBlack)
        //            {
        //                return;
        //            }

        //        }


        //    }

        //}



        private void DoModus(int modus)
        {
            var sourceBMP = OnNeedCurrentPic();
            if (sourceBMP == null) { return; }

            var newBMP = new Bitmap(sourceBMP.Width, sourceBMP.Height);

            var cols = UsedColors(sourceBMP);

            // -------------------- Only Color --------------------------------
            #region Generate Color Image

            var onlyColorBMP = new Bitmap(sourceBMP.Width, sourceBMP.Height);
            for (var x = 0; x < onlyColorBMP.Width; x++)
            {
                for (var y = 0; y < onlyColorBMP.Height; y++)
                {

                    switch (modus)
                    {
                        case 0: // BW
                            onlyColorBMP.SetPixel(x, y, Color.Transparent);
                            break;

                        case 1: // Farbe
                        case 2: // BlurFarbe
                            var nc = GetNearestColor(cols, sourceBMP, x, y);
                            onlyColorBMP.SetPixel(x, y, nc);
                            break;

                        default:
                            Develop.DebugPrint("Modus nicht definiert");
                            return;
                    }
                }
            }

            if (modus == 2)
            {
                onlyColorBMP = onlyColorBMP.ImageBlurFilter(BlueBasics.Enums.BlurType.GaussianBlur3x3);
            }

            #endregion
            // ----------------------- Brain --------------------------------------







            var g = Graphics.FromImage(newBMP);
            g.Clear(Color.Transparent);



            for (var x = 0; x < sourceBMP.Width; x++)
            {
                for (var y = 0; y < sourceBMP.Height; y++)
                {

                    var backc = onlyColorBMP.GetPixel(x, y);
                    var overcolor = Color.FromArgb(BlackValue(sourceBMP, x, y, modus), 0, 0, 0);

                    //newBMP.SetPixel(x, y, BlueBasics.Extensions.Blend(overcolor, backc, backc.A / 255));

                    g.FillRectangle(new SolidBrush(backc), x, y, 1, 1);
                    g.FillRectangle(new SolidBrush(overcolor), x, y, 1, 1);

                    //newBMP.SetPixel(x, y, MixAlphaColor(onlyColorBMP.GetPixel(x, y), Color.FromArgb(BlackValue(sourceBMP, x, y, modus), 0, 0, 0)));

                    //var col =;
                    //var a1 = BlackValue(sourceBMP, x, y, modus);

                    //var r = 


                    //newBMP.SetPixel(x, y,);
                    //newBMP.SetPixel(x, y, Color.FromArgb(BlackValue(sourceBMP, x, y, modus), 0, 0, 0));

                    //if (Black(sourceBMP, x, y))
                    //{
                    //    var V = BlackValue(sourceBMP, x, y);
                    //    newBMP.SetPixel(x, y, Color.FromArgb(V, V, V));
                    //}
                    //else
                    //{
                    //    if (modus == 1)
                    //    {
                    //        newBMP.SetPixel(x, y, onlyColorBMP.GetPixel(x,y));
                    //    }
                    //    else
                    //    {
                    //        newBMP.SetPixel(x, y, Color.Transparent);
                    //    }


                    //}
                }
            }

            OnOverridePic(newBMP);
            Develop.DoEvents();
        }

        private void btnAnwendenBlurSW_Click(object sender, System.EventArgs e)
        {
            OnCommandForMacro("Anwenden;2");
            DoModus(2);

        }
    }
}