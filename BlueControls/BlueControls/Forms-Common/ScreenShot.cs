#region BlueElements - a collection of useful tools, database and controls
// Authors: 
// Christian Peter
// 
// Copyright (c) 2021 Christian Peter
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
using BlueBasics.Enums;
using BlueControls.Forms;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Runtime.CompilerServices;

namespace BlueControls {
    /// <summary>
    /// Eine Klasse, die alle möglichen Arten von Screenshots zurückgibt.
    /// </summary>
    public sealed partial class ScreenShot {



        private ScreenShot() {
            InitializeComponent();
        }






        #region  +++ Deklarationen +++ 
        //private enSelectModus Modus = enSelectModus.Unbekannt;

        private bool MousesWasUp;
        private Bitmap ClipedArea;
        private Bitmap ScreenShotBMP;
        private strScreenData FeedBack;


        private string DrawText = "";
        private int DrawSize = 20;

        private List<strScreenData> AllS = new();

        [AccessedThroughProperty(nameof(Hook))]
        private SystemInputHook _Hook;
        private SystemInputHook Hook {
            [DebuggerNonUserCode]
            get => _Hook;
            [MethodImpl(MethodImplOptions.Synchronized), DebuggerNonUserCode]
            set {
                if (_Hook != null) {
                    _Hook.MouseUp -= Hook_MouseUp;
                    _Hook.MouseDown -= Hook_MouseDown;
                    _Hook.MouseMove -= Hook_MouseMove;
                }

                _Hook = value;

                if (value != null) {
                    _Hook.MouseUp += Hook_MouseUp;
                    _Hook.MouseDown += Hook_MouseDown;
                    _Hook.MouseMove += Hook_MouseMove;
                }
            }
        }


        private Point LastMouse;
        private Point HookStartPoint;
        private Point HookEndPoint;
        private bool HookFinish;
        private Overlay Rahm;


        #endregion


        public static Bitmap GrabAllScreens() {
            do {

                try {



                    var r = modAllgemein.RectangleOfAllScreens();

                    var b = new Bitmap(r.Width, r.Height, PixelFormat.Format32bppPArgb);

                    using (var GR = Graphics.FromImage(b)) {
                        GR.CopyFromScreen(r.X, r.Y, 0, 0, b.Size);
                    }

                    return b;


                } catch {
                    modAllgemein.CollectGarbage();
                }

            } while (true);

        }



        public static Bitmap GrabArea(Rectangle R) {
            if (R.Width < 2 || R.Height < 2) {
                return null;
            }
            return GrabAllScreens().Area(R);

        }


        public static strScreenData GrabArea() {
            return GrabArea(null, -1, -1);
        }



        /// <summary>
        /// Erstellt einen Screenshot, dann kann der User einen Bereich wählen - und gibt diesen zurück.
        /// </summary>
        /// <param name="frm">Diese Form wird automatisch minimiert und wieder maximiert.</param>
        /// <param name="MaxW">Die Maximale Breite des Bildes. Evtl. wird das Bild herunterskaliert.</param>
        /// <param name="MaxH">Die Maximale Höhe des Bildes. Evtl. wird das Bild herunterskaliert.</param>
        /// <returns></returns>
        /// <remarks></remarks>
        public static strScreenData GrabArea(System.Windows.Forms.Form frm, int MaxW, int MaxH) {


            using (var x = new ScreenShot()) {
                x.DrawText = "Bitte ziehen sie einen Rahmen\r\num den gewünschten Bereich.";


                return x.GrabAreaInternal(frm, MaxW, MaxH);
            }

        }

        public static Bitmap GrabContinuus() {
            var x = new ScreenShot();
            var im = x.GrabContinuusIntern();


            x.Dispose();


            modAllgemein.CollectGarbage();

            return im;
        }





        private Bitmap GrabContinuusIntern() {
            AllS = new List<strScreenData>();

            DrawText = "Bitte ziehen sie einen Rahmen um den gewünschten Bereich.\r\nDieser wird anschließend nach jedem Mauszug abfotografiert.\r\nBeendet wird der Modus mit der rechten Maustaste.";
            DrawSize = 10;

            AllS.Add(GrabAreaInternal(null, -1, -1));

            Rahm = new Overlay();


            Hook = new SystemInputHook();
            Hook.InstallHook();

            HookStartPoint = new Point(int.MinValue, int.MinValue);
            HookEndPoint = new Point(int.MinValue, int.MinValue);
            LastMouse = new Point(int.MinValue, int.MinValue);

            Rahm.Show();

            do {
                Develop.DoEvents();


                if (HookFinish) { break; }


                if (HookStartPoint.X > int.MinValue && HookEndPoint.X > int.MinValue) {

                    Hook.RemoveHook();
                    if (HookStartPoint.ToString() != HookEndPoint.ToString()) {
                        Rahm.Visible = false;

                        var l = new strScreenData {
                            Pic = GrabArea(AllS[0].GrabedArea()),
                            HookP1 = HookStartPoint,
                            HookP2 = HookEndPoint
                        };


                        AllS.Add(l);
                    }

                    HookStartPoint = new Point(int.MinValue, int.MinValue);
                    HookEndPoint = new Point(int.MinValue, int.MinValue);
                    LastMouse = new Point(int.MinValue, int.MinValue);


                    modAllgemein.Pause(0.5, false);


                    Rahm.Visible = true;
                    Hook.InstallHook();
                }

            } while (true);


            Hook.RemoveHook();

            Rahm.Dispose();


            var MinX = 0;
            var MinY = 0;

            var maxx = AllS[0].GrabedArea().Width;
            var maxy = AllS[0].GrabedArea().Height;

            var VersX = 0;
            var VersY = 0;


            for (var z = 1; z < AllS.Count; z++) {

                VersX = VersX + AllS[z].HookP1.X - AllS[z].HookP2.X;
                VersY = VersY + AllS[z].HookP1.Y - AllS[z].HookP2.Y;


                MinX = Math.Min(VersX, MinX);
                MinY = Math.Min(VersY, MinY);
                maxx = Math.Max(VersX + AllS[0].GrabedArea().Width, maxx);
                maxy = Math.Max(VersY + AllS[0].GrabedArea().Height, maxy);
            }


            modAllgemein.CollectGarbage();


            var bmp = new Bitmap(maxx - MinX, maxy - MinY, PixelFormat.Format32bppPArgb);
            var gr = Graphics.FromImage(bmp);
            gr.Clear(Color.White);



            VersX = MinX * -1;
            VersY = MinY * -1;

            for (var z = 0; z < AllS.Count; z++) {

                VersX = VersX + AllS[z].HookP1.X - AllS[z].HookP2.X;
                VersY = VersY + AllS[z].HookP1.Y - AllS[z].HookP2.Y;


                gr.DrawImage(AllS[z].Pic, VersX, VersY);

                AllS[z].Pic.Dispose();
            }

            return bmp;
        }




        private strScreenData GrabAreaInternal(System.Windows.Forms.Form frm, int MaxW, int MaxH) {
            try {

                System.Windows.Forms.FormWindowState WS = 0;

                //DialogBox.eBalloonToolTip_Destroy();
                //P?.Close();
                QuickInfo.Close();


                if (frm != null) {
                    WS = frm.WindowState;
                    frm.WindowState = System.Windows.Forms.FormWindowState.Minimized;
                    modAllgemein.Pause(0.5, true); // 0.3 ist zu wenig!
                }

                PrepareForm();


                var r = modAllgemein.RectangleOfAllScreens();

                Left = r.Left;
                Top = r.Top;
                Width = r.Width;
                Height = r.Height;


                OnMouseMove(null);


                ShowDialog();



                if (frm != null) { frm.WindowState = WS; }


                // New Bitmap davor, um die Bitmaptiefe zu korrigiern
                if (MaxW > 0 && MaxH > 0) {
                    // Auch hier NEW Bitmap, da evtl. das Original-Bild zurück gegeben wird.
                    FeedBack.Pic = new Bitmap(BitmapExt.Resize(ClipedArea, MaxW, MaxH, enSizeModes.Breite_oder_Höhe_Anpassen_OhneVergrößern, InterpolationMode.HighQualityBicubic, true));

                    FeedBack.IsResized = true;
                } else {
                    FeedBack.Pic = new Bitmap(ClipedArea);
                    FeedBack.IsResized = false;
                }

                ClipedArea.Dispose();

                return FeedBack;

            } catch {
                return new strScreenData();
            }


        }







        #region  Form-Ereignisse 

        protected override void OnMouseDown(System.Windows.Forms.MouseEventArgs e) {
            base.OnMouseDown(e);
            if (!MousesWasUp) { return; }
            FeedBack.Point1 = new Point(e.X, e.Y);
        }

        protected override void OnMouseUp(System.Windows.Forms.MouseEventArgs e) {
            base.OnMouseUp(e);

            if (!MousesWasUp) {
                MousesWasUp = true;
                return;
            }


            FeedBack.Point2 = new Point(e.X, e.Y);

            var r = FeedBack.GrabedArea();

            if (r.Width < 2 || r.Height < 2) { return; }

            ClipedArea = new Bitmap(r.Width, r.Height, PixelFormat.Format32bppPArgb);

            using (var GR = Graphics.FromImage(ClipedArea)) {
                GR.Clear(Color.Black);
                GR.DrawImage(ScreenShotBMP, 0, 0, r, GraphicsUnit.Pixel);
            }


            Close();




        }


        protected override void OnMouseMove(System.Windows.Forms.MouseEventArgs e) {
            base.OnMouseMove(e);


            if (e != null && e.Button == System.Windows.Forms.MouseButtons.None && !MousesWasUp) {
                MousesWasUp = true;
                return;
            }

            var r = modAllgemein.RectangleOfAllScreens();

            Left = r.Left;
            Top = r.Top;
            Width = r.Width;
            Height = r.Height;



            using (var GR = Graphics.FromImage(BackgroundImage)) {
                GR.Clear(Color.Black);
                GR.DrawImage(ScreenShotBMP, 0, 0);

                if (e != null) {





                    PrintText(GR, e);


                    modAllgemein.Magnify(ScreenShotBMP, new Point(e.X, e.Y), GR, false);


                    if (e.Button != System.Windows.Forms.MouseButtons.None) {
                        GR.DrawLine(new Pen(Color.Red), 0, FeedBack.Point1.Y, Width, FeedBack.Point1.Y);
                        GR.DrawLine(new Pen(Color.Red), FeedBack.Point1.X, 0, FeedBack.Point1.X, Height);

                        GR.DrawLine(new Pen(Color.Red), 0, e.Y, Width, e.Y);
                        GR.DrawLine(new Pen(Color.Red), e.X, 0, e.X, Height);

                    } else {
                        GR.DrawLine(new Pen(Color.Red), 0, e.Y, Width, e.Y);
                        GR.DrawLine(new Pen(Color.Red), e.X, 0, e.X, Height);
                    }

                }

                Refresh();

            }


        }
        #endregion



        private void PrintText(Graphics GR, System.Windows.Forms.MouseEventArgs e) {


            Brush bs = new SolidBrush(Color.FromArgb(150, 0, 0, 0));
            Brush bf = new SolidBrush(Color.FromArgb(255, 255, 0, 0));
            var fn = new Font("Arial", DrawSize, FontStyle.Bold);
            var f = GR.MeasureString(DrawText, fn);

            var yPos = 0;

            if (e == null) {
                yPos = 0;
            } else {

                if (e.Y > f.Height + 50) {
                    yPos = 0;
                } else {
                    yPos = (int)(Height - f.Height - 5);
                }


            }


            GR.FillRectangle(bs, 0, yPos - 5, Width, f.Height + 10);
            GR.DrawString(DrawText, fn, bf, 2, yPos + 2);

        }


        /// <summary>
        /// Vorbereitende Routine, die das Backgroundimage mit dem Screenshot füllt.
        /// </summary>
        /// <remarks></remarks>
        private void PrepareForm() {
            ScreenShotBMP = GrabAllScreens();

            BackgroundImage = new Bitmap(ScreenShotBMP.Width, ScreenShotBMP.Height, PixelFormat.Format32bppPArgb);

            using (var GR = Graphics.FromImage(BackgroundImage)) {
                GR.DrawImage(ScreenShotBMP, 0, 0);
            }

        }

        private void Hook_MouseUp(object sender, System.Windows.Forms.MouseEventArgs e) {


            if (e.Button == System.Windows.Forms.MouseButtons.Right) {
                HookFinish = true;
                return;
            }

            if (HookEndPoint.X > int.MinValue) { return; }

            if (e.Button == System.Windows.Forms.MouseButtons.Left) { HookEndPoint = new Point(LastMouse.X, LastMouse.Y); }

        }

        private void Hook_MouseDown(object sender, System.Windows.Forms.MouseEventArgs e) {

            if (e.Button == System.Windows.Forms.MouseButtons.Right) {
                HookFinish = true;
                return;
            }

            if (HookStartPoint.X > int.MinValue) { return; }


            if (e.Button == System.Windows.Forms.MouseButtons.Left) { HookStartPoint = new Point(LastMouse.X, LastMouse.Y); }




        }

        private void Hook_MouseMove(object sender, System.Windows.Forms.MouseEventArgs e) {

            LastMouse = new Point(e.X, e.Y);
            if (Rahm != null) {
                Rahm.Left = (int)(e.X - Rahm.Width / 2.0);
                Rahm.Top = (int)(e.Y - Rahm.Width / 2.0);
            }


        }
    }
}