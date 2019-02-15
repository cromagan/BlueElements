using BlueElements;
using BlueElements.NeuronalNetwork;
using System;
using System.Collections.Generic;
using System.Drawing;
using static BlueElements.BitmapExtensions;


namespace OCR
{
    public partial class Form1 : BlueElements.Forms.Form
    {
        public Form1()
        {
            InitializeComponent();
        }


        private Bitmap DummyBMP = null;
        private Graphics DummyGR = null;

        private bool StopPending = false;

        List<Font> F = new List<Font>();

        FullyConnectedNetwork TestBrain = null;

        private void btnDo_Click(object sender, EventArgs e)
        {
            // Font Fx = new Font("RM Typerighter old", 20);
            Font Fx = new Font("Castellar", 20);
            Bitmap Char = GenPic(txtLetter.Text, Fx);
            picLetter.Image = Char;
            picLetter.Refresh();

            BlueElements.OCR.AnalyseChar(Char, string.Empty, out double Error, null);


            capResult.Text = BlueElements.OCR.Brain.OutputLayer.SoftMaxText();


            brainDrawer1.Brain = BlueElements.OCR.Brain;
            brainDrawer1.SnapShot(false, true);
        }



        Dictionary<string, Bitmap> Pics = new Dictionary<string, Bitmap>();

        private Bitmap GenPic(string Letter, Font f)
        {


            string key = f.ToString() + "|" + Letter;

            if (Pics.ContainsKey(key)) { return Pics[key]; }



            SizeF sizefont = DummyGraphics().MeasureString(Letter, f);


            Bitmap bmp = new Bitmap((int)sizefont.Width, (int)sizefont.Height);


            Graphics gr = Graphics.FromImage(bmp);

            gr.Clear(Color.White);

            gr.DrawString(Letter, f, new SolidBrush(Color.Black), new Point(0, 0));
            gr.Dispose();

            Bitmap bmp2 = modAllgemein.AutoCrop(bmp, 0.7);
            bmp2 = BlueElements.modAllgemein.Grayscale(bmp2);

            //bmp2 = AutoContrast(bmp2);

            if (bmp.Width < 2 && bmp2.Height < 2) { Develop.DebugPrint(f.ToString() + " zu klein"); }

            if (bmp2.Width != 10 || bmp2.Height != 10) { bmp2 = bmp2.Resize(10, 10, BlueElements.enSizeModes.WeißerRand, System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic, true); }


            picLetter.Image = bmp2;

            Pics.Add(key, bmp2);

            return bmp2;
        }


        //private static Bitmap AutoContrast(Bitmap BMP)
        //{
        //    float con = (float)0.0D;
        //    float Hel = (float)0.0D;

        //    Bitmap bmpCor = null;
        //    double h = 0;

        //    do
        //    {
        //        bmpCor = modAllgemein.SetBrightnessContrastGamma(BMP, Hel, con, 1);

        //        double[] BR = new double[101];

        //        for (int y = 0 ; y < BMP.Height ; y++)
        //        {
        //            for (int x = 0 ; x < BMP.Width ; x++)
        //            {
        //                h = bmpCor.GetPixel(x, y).GetBrightness() * 100;
        //                BR[(int)h] += 1;

        //            }
        //        }

        //        if (BR[0] > 0 && BR[100] > 0) { break; }

        //        con += (float)0.02;

        //        if (con > 0.2F) { break; }


        //    } while (true);

        //    return bmpCor;

        //}


        private Graphics DummyGraphics()
        {

            if (DummyGR == null)
            {
                DummyBMP = new Bitmap(1, 1);
                DummyGR = Graphics.FromImage(DummyBMP);
            }
            return DummyGR;

        }

        private void btnTraining_Click(object sender, EventArgs e)
        {
            StopPending = false;
            btnTraining.Enabled = false;
            btnStop.Enabled = true;

            DateTime T = DateTime.Now;

            BlueElements.OCR.Init();
            Bitmap Char = null;

            do
            {

                double Error = 0;

                for (int z = 0 ; z < BlueElements.OCR.Brain.OutputLayer.Neurones.Length ; z++)
                {

                    foreach (Font ThisFont in F)
                    {
                        string Letter = BlueElements.OCR.Brain.OutputLayer.Neurones[z].Name;
                        Char = GenPic(Letter, ThisFont);
                        picLetter.Image = Char;
                        //capResult.Text = "Trainiere: " + Letter.ToNonCritical();
                        //picLetter.Refresh();
                        //capResult.Refresh();
                        //Develop.DoEvents();


                        BlueElements.OCR.AnalyseChar(Char, Letter, out double tmpError, capResult);
                        Error += tmpError;


                        if (DateTime.Now.Subtract(T).TotalSeconds > 80)
                        {
                            BlueElements.OCR.Brain.Save(BlueElements.OCR.Filename);

                            //brainDrawer1.Brain = BlueElements.OCR.Brain;
                            //brainDrawer1.SnapShot(false, true);
                            //picLetter.Refresh();

                            T = DateTime.Now;
                        }
                    }

                    Develop.DoEvents();

                }

                capResult.Text = "Fehlerquote: " + (Error / (BlueElements.OCR.Brain.OutputLayer.Neurones.Length * F.Count));

                modAllgemein.Pause(0.5, true);
            } while (!StopPending);

            BlueElements.OCR.Brain.Save(BlueElements.OCR.Filename);

            btnTraining.Enabled = true;
            btnStop.Enabled = false;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            button1.Enabled = false;

            if (TestBrain == null)
            {
                List<string> Input = new List<string>() { "Wert1", "Wert2" };
                List<string> Output = new List<string>() { "True", "False" };
                TestBrain = new FullyConnectedNetwork(Input, Output, 5, 5, 5);
            }
            brainDrawer1.Brain = TestBrain;
            brainDrawer2.Brain = TestBrain;
            brainDrawer3.Brain = TestBrain;
            brainDrawer4.Brain = TestBrain;
            double LeraningRate = 0.01;

            int DrawRate = 1000;

            // Or-Function

            for (int du = 0 ; du < 1000000 ; du++)
            {

                TestBrain.InputLayer.SetValue("Wert1", 1);
                TestBrain.InputLayer.SetValue("Wert2", 1);
                TestBrain.BackPropagationGradiant("True", LeraningRate, true);
                if (du % DrawRate == 0) { brainDrawer1.SnapShot(true, true); }



                TestBrain.InputLayer.SetValue("Wert1", 1);
                TestBrain.InputLayer.SetValue("Wert2", 0);
                TestBrain.BackPropagationGradiant("True", LeraningRate, true);
                if (du % DrawRate == 0) { brainDrawer2.SnapShot(true, true); }


                TestBrain.InputLayer.SetValue("Wert1", 0);
                TestBrain.InputLayer.SetValue("Wert2", 1);
                TestBrain.BackPropagationGradiant("True", LeraningRate, true);
                if (du % DrawRate == 0) { brainDrawer3.SnapShot(true, true); }


                TestBrain.InputLayer.SetValue("Wert1", 0);
                TestBrain.InputLayer.SetValue("Wert2", 0);
                TestBrain.BackPropagationGradiant("False", LeraningRate, true);
                if (du % DrawRate == 0) { brainDrawer4.SnapShot(true, true); }


                modAllgemein.Pause(0.0000001, false);

            }

            button1.Enabled = true;


        }

        private void btnStop_Click(object sender, EventArgs e)
        {
            btnStop.Enabled = false;
            StopPending = true;
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            F.Add(new Font("Arial", 50));
            F.Add(new Font("Agency FB", 20));
            F.Add(new Font("Baskerville Old Face", 20));
            F.Add(new Font("MV Boli", 20));
            F.Add(new Font("Calibri", 50));
            F.Add(new Font("Calibri", 20));
            F.Add(new Font("Calibri", 10));
            F.Add(new Font("Arial Black", 50));
            F.Add(new Font("Arial Black", 20));
            F.Add(new Font("Arial Black", 10));
            //F.Add(new Font("Arial Narrow", 50));
            F.Add(new Font("Arial Narrow", 20));
            //F.Add(new Font("Arial Narrow", 10));
            F.Add(new Font("Comic Sans MS", 50));
            F.Add(new Font("Comic Sans MS", 20));
            F.Add(new Font("Comic Sans MS", 10));
            F.Add(new Font("Times New Roman", 50));
            F.Add(new Font("Times New Roman", 20));
            F.Add(new Font("Times New Roman", 10));
            F.Add(new Font("Arial Rounded MT Bold", 25));
            F.Add(new Font("Bodoni MT Black", 25));
            //F.Add(new Font("Bernard MT Condensed", 25));
            F.Add(new Font("Bahnschrift Light", 25));
            F.Add(new Font("Broadway", 25));
            F.Add(new Font("Cooper Black", 25));
            F.Add(new Font("Franklin Gothic Medium Cond", 25));
            //F.Add(new Font("Wide Latin", 25));
            F.Add(new Font("Candara", 25));
            F.Add(new Font("Microsoft Sans Serif", 25));
            F.Add(new Font("Microsoft Sans Serif", 12));
            F.Add(new Font("Courier New", 12));
            F.Add(new Font("Courier New", 25));
            F.Add(new Font("Century", 25));
            F.Add(new Font("Sitka Small", 25));
            F.Add(new Font("Berlin Sans FB", 25));
            F.Add(new Font("Yu Gothic UI", 25));
            F.Add(new Font("Solid Edge ISO", 25));
            F.Add(new Font("Solid Edge ISO", 15));
            F.Add(new Font("Solid Edge ANSI Unicode", 25));
            F.Add(new Font("Solid Edge ANSI Unicode", 15));
            F.Add(new Font("Tahoma", 25));
            F.Add(new Font("Tahoma", 12));
            F.Add(new Font("Verdana", 25));
            F.Add(new Font("RM Typerighter old", 30));
            F.Add(new Font("RM Typerighter old", 20));
            F.Add(new Font("Comic Sans MS", 20, FontStyle.Italic | FontStyle.Bold));
            F.Add(new Font("Comic Sans MS", 10, FontStyle.Italic | FontStyle.Bold));
            F.Add(new Font("Comic Sans MS", 20, FontStyle.Italic));
            F.Add(new Font("Comic Sans MS", 10, FontStyle.Italic));
            F.Add(new Font("Comic Sans MS", 20, FontStyle.Bold));
            F.Add(new Font("Comic Sans MS", 10, FontStyle.Bold));
            F.Add(new Font("Calibri", 20, FontStyle.Italic | FontStyle.Bold));
            F.Add(new Font("Calibri", 10, FontStyle.Italic | FontStyle.Bold));
            F.Add(new Font("Calibri", 20, FontStyle.Italic));
            F.Add(new Font("Calibri", 10, FontStyle.Italic));
            F.Add(new Font("Calibri", 20, FontStyle.Bold));
            F.Add(new Font("Calibri", 10, FontStyle.Bold));
            F.Add(new Font("Times New Roman", 20, FontStyle.Italic | FontStyle.Bold));
            F.Add(new Font("Times New Roman", 10, FontStyle.Italic | FontStyle.Bold));
            F.Add(new Font("Times New Roman", 20, FontStyle.Italic));
            F.Add(new Font("Times New Roman", 10, FontStyle.Italic));
            F.Add(new Font("Times New Roman", 20, FontStyle.Bold));
            F.Add(new Font("Times New Roman", 10, FontStyle.Bold));
            F.Add(new Font("Arial", 20, FontStyle.Italic | FontStyle.Bold));
            F.Add(new Font("Arial", 10, FontStyle.Italic | FontStyle.Bold));
            F.Add(new Font("Arial", 20, FontStyle.Italic));
            F.Add(new Font("Arial", 10, FontStyle.Italic));
            F.Add(new Font("Arial", 20, FontStyle.Bold));
            F.Add(new Font("Arial", 10, FontStyle.Bold));
            F.Add(new Font("Arial", 20));
            F.Add(new Font("Arial", 10));
        }
    }
}
