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

using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Linq;
using BlueBasics;
using BlueBasics.Enums;

namespace BlueBrain
{
    public static class OCR
    {
        public static FullyConnectedNetwork Brain;
        public static string AllLetters = "'~#ø[]{}§<>$*%&/=\\\"()@€+-!:;?.,ÄÖÜäöüßéABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
        // public static string Filename = @"C:\01_Data\Visual Studio VSTS\BlueBasics\Ressourcen\OCR\OCR.brn";
        public static string Filename = @"D:\01_Data\Dokumente\Visual Studio\XXX overlapping\BlueBasics\Ressourcen\OCR\OCR.brn";



        public static void Init()
        {
            if (File.Exists(Filename))
            {
                Brain = new FullyConnectedNetwork(Filename);
                return;
            }


            var Koords = new List<string>();
            var Letters = AllLetters.Select((t, z) => AllLetters.Substring(z, 1)).ToList();


            for (var x = 0 ; x < 10 ; x++)
            {
                for (var y = 0 ; y < 10 ; y++)
                {
                    Koords.Add(x + "|" + y);
                }
            }

            Brain = new FullyConnectedNetwork(Koords, Letters, 200);



        }

        public static string AnalyseChar(Bitmap BMP, string TrainTo, out double Error)
        {

            //BlueBasics.Forms.PictureView x2 = new Forms.PictureView(BMP);
            //x2.ShowDialog();
            //return string.Empty;
            if (Brain == null) { Init(); }


            if (BMP.Width != 10 || BMP.Height != 10) { BMP = BMP.Resize(10, 10, enSizeModes.EmptySpace, InterpolationMode.HighQualityBicubic, true); }


            for (var x = 0 ; x < BMP.Width ; x++)
            {
                for (var y = 0 ; y < BMP.Height ; y++)
                {
                    Brain.InputLayer.SetValue(x + "|" + y, BMP.GetPixel(x, y).R / 127.5 - 1);
                }
            }

            Brain.Compute();
            if (string.IsNullOrEmpty(TrainTo))
            {
                Error = 0;
                return Brain.OutputLayer.SoftMax();
            }

            var Count = 0;
            Error = Brain.BackPropagationGradiant(TrainTo, 0.001, true);
            var v = Brain.OutputLayer.SoftMaxValue(TrainTo);
            var nv = Brain.OutputLayer.SoftMaxValue(TrainTo); 

            do
            {
                v = nv;

                if (Brain.OutputLayer.SoftMax() == TrainTo && v > 0.7 && v <= 1.0)
                {
                    return TrainTo;
                }

                Count += 1;
                Brain.BackPropagationGradiant(TrainTo, 1 - v, true);

                if (Count % 600 == 599)
                {
                    //Count = 0;
                    //IMG.Refresh();
                    //CAP.Text = TrainTo.ToNonCritical() + ": " + v + "\r\n\r\n" + Brain.OutputLayer.SoftMaxText();
                    //CAP.Refresh();
                    Develop.DoEvents();
                }

                if (Count % 30000 == 28000)
                {
                    Brain.Save(Filename);
                }


                if (Count > 30000) { return TrainTo; }

                nv = Brain.OutputLayer.SoftMaxValue(TrainTo);
                //if (nv < v)
                //{
                //    return TrainTo;
                //}



            } while (true);
        }







        //private static double GetSchwelle(Bitmap BMP)
        //{
        //    double bR = 0;
        //    for (int y = 0 ; y < BMP.Height ; y++)
        //    {
        //        for (int x = 0 ; x < BMP.Width ; x++)
        //        {
        //            bR = bR + BMP.GetPixel(x, y).GetBrightness();
        //        }
        //    }

        //    return bR / (BMP.Height * BMP.Width);
        //}


        public static string AnalysePic(Bitmap BMP)
        {
            if (Brain == null) { Init(); }


            var Again = false;
            var PixAnz = BMP.Width * BMP.Height;

            BMP = modAllgemein.Grayscale(BMP);

            do
            {
                Again = false;
                var Dark = 0;
                var Light = 0;
                var NearBlack = 0;
                var NearWhite = 0;

                for (var y = 0 ; y < BMP.Height ; y++)
                {
                    for (var x = 0 ; x < BMP.Width ; x++)
                    {
                        var c = BMP.GetPixel(x, y);
                        var h = c.GetBrightness();
                        if (h < 0.4)
                        {
                            Dark++;
                            if (c.IsNearBlack(0.05)) { NearBlack++; }
                        }
                        else
                        {
                            Light++;
                            if (c.IsNearWhite(0.95)) { NearWhite++; }
                        }
                    }
                }

                if (Dark > Light * 5)
                {
                    Again = true;
                    BMP = modAllgemein.Invert(BMP);
                    modAllgemein.Swap(ref NearBlack, ref NearWhite);
                }

                //BlueBasics.Forms.PictureView x1 = new Forms.PictureView(BMP);
                //x1.ShowDialog();

                if (NearBlack < PixAnz / 50 && NearWhite < PixAnz / 5)
                {
                    BMP = modAllgemein.SetBrightnessContrastGamma(BMP, 0, 0.2f, 1.1f);
                    //BlueBasics.Forms.PictureView x2 = new Forms.PictureView(BMP);
                    //x2.ShowDialog();
                    Again = true;
                    Develop.DebugPrint_NichtImplementiert();
                }
                else if (NearBlack < PixAnz / 50 && NearWhite > PixAnz / 5)
                {
                    BMP = modAllgemein.SetBrightnessContrastGamma(BMP, -0.2f, 0.1f, 1.1f);
                    Again = true;
                    //BlueBasics.Forms.PictureView x3 = new Forms.PictureView(BMP);
                    //x3.ShowDialog();
                    Develop.DebugPrint_NichtImplementiert();
                }
                else if (NearBlack > PixAnz / 50 && NearWhite < PixAnz / 5)
                {
                    BMP = modAllgemein.SetBrightnessContrastGamma(BMP, 0.05f, 0.1f, 1);
                    //BlueBasics.Forms.PictureView x4 = new Forms.PictureView(BMP);
                    //x4.ShowDialog();
                    Again = true;
                }
            } while (Again);

            return AnalysePicColorsCorrected(BMP);

        }
        private static string AnalysePicColorsCorrected(Bitmap BMP)
        {
            var tx = "";

            var Ende = -1;
            var Anfang = -1;


            for (var y = 0 ; y < BMP.Height ; y++)
            {
                var Dark = 0;
                var Light = 0;

                for (var x = 0 ; x < BMP.Width ; x++)
                {
                    var h = BMP.GetPixel(x, y).GetBrightness();
                    if (h < 0.4)
                    {
                        Dark += 1;
                    }
                    else
                    {
                        Light += 1;
                    }
                }



                if (Light == 0 || Dark == 0 && Anfang >= 0) { Ende = y - 1; }

                if (Light != 0 && Dark != 0 && Anfang < 0)
                {
                    Anfang = y;
                    Ende = -1;
                }



                if (Anfang != -1 && Ende != -1)
                {
                    tx = tx + AnalyseBMPAreaY(modAllgemein.Crop(BMP, 0, 0, Anfang, -(BMP.Height - Ende) + 1)) + "\r\n";
                    Anfang = -1;
                    Ende = -1;
                }

            }


            return tx.Trim("\r\n"[0]).Trim();
        }

        private static string AnalyseBMPAreaY(Bitmap BMP)
        {

            var tx = "";

            var Ende = -1;
            var Anfang = -1;

            //BlueBasics.Forms.PictureView x2 = new Forms.PictureView(BMP);
            //x2.ShowDialog();




            for (var x = 0 ; x < BMP.Width ; x++)
            {
                var Dark = 0;

                for (var y = 0 ; y < BMP.Height ; y++)
                {
                    var h = BMP.GetPixel(x, y).GetBrightness();
                    if (h < 0.4)
                    {
                        Dark += 1;
                    }

                }


                if (Dark != 0 && Anfang < 0)
                {
                    Anfang = x;
                    Ende = -1;
                }

                if (Dark == 0 && Anfang >= 0) { Ende = x - 1; }


                if (Anfang != -1 && Ende != -1)
                {
                    var c = AnalyseBMPAreaXY(modAllgemein.Crop(BMP, Anfang, -(BMP.Width - Ende) + 1, 0, 0));
                    if (string.IsNullOrEmpty(c)) { c = "?"; }
                    tx = tx + c;
                    Anfang = -1;
                    Ende = -1;
                }
            }


            return tx;
        }

        private static string AnalyseBMPAreaXY(Bitmap BMP)
        {
            BMP = modAllgemein.AutoCrop(BMP,0.7);
            //BlueBasics.Forms.PictureView x2 = new Forms.PictureView(BMP);
            //x2.ShowDialog();
            return AnalyseChar(BMP, string.Empty, out _);
        }











    }

}