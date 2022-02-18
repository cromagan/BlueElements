// Authors:
// Christian Peter
//
// Copyright (c) 2022 Christian Peter
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

using BlueBasics;
using BlueBasics.Enums;
using System;
using System.ComponentModel;
using System.Drawing;

namespace BlueControls.Forms {

    public partial class Notification : FloatingForm {

        #region Fields

        private bool UserClicked = false;

        #endregion

        #region Constructors

        // Startzeit für UnloadAfterSek
        private Notification() : base(Enums.enDesign.Form_DesktopBenachrichtigung) => InitializeComponent();

        private Notification(string text) : this() {
            capTXT.Text = text;
            var He = Math.Min(capTXT.TextRequiredSize().Height, (int)(System.Windows.Forms.Screen.PrimaryScreen.Bounds.Size.Height * 0.7));
            var Wi = Math.Min(capTXT.TextRequiredSize().Width, (int)(System.Windows.Forms.Screen.PrimaryScreen.Bounds.Size.Width * 0.7));
            Size = new Size(Wi + (capTXT.Left * 2), He + (capTXT.Top * 2));
            Location = new Point(-Width - 10, Height - 10);
            ScreenTime = Math.Max(3200, text.Length * 110);

            //Below müsste in Allboxes ja die letzte sein - außer sich selbst
            foreach (var thisParent in AllBoxes) {
                if (thisParent is Notification nf) {
                    if (nf != this && nf.Visible && !nf.IsDisposed) {
                        NoteBelow = nf;
                    }
                }
            }
        }

        #endregion

        #region Properties

        public Notification NoteBelow { get; private set; } = null;
        public int ScreenTime { get; private set; } = -999;

        #endregion

        //public static void CloseAll() {
        //    foreach (var ThisForm in AllBoxes) {
        //        if (!ThisForm.IsDisposed && ThisForm is Notification) {
        //            try {
        //                ThisForm.Visible = false;
        //                ThisForm.Close();
        //                CloseAll();
        //                return;
        //            } catch (Exception ex) {
        //                Develop.DebugPrint(ex);
        //            }
        //        }
        //    }
        //}

        #region Methods

        public static void Show(string text) {
            if (string.IsNullOrEmpty(text)) { return; }

            var bg = new BackgroundWorker();
            bg.DoWork += Movement;
            bg.RunWorkerAsync(text);
        }

        public static void Show(string text, enImageCode img) {
            if (img != enImageCode.None) {
                text = "<ImageCode=" + Enum.GetName(img.GetType(), img) + "|32> <zbx_store><top>" + text;
            }
            Show(text);
        }

        //public new void Close() {
        //    foreach (var ThisForm in AllBoxes) {
        //        if (!ThisForm.IsDisposed && ThisForm is Notification) {
        //            try {
        //                base.Close();
        //                return;
        //            } catch (Exception ex) {
        //                Develop.DebugPrint(ex);
        //            }
        //        }
        //    }
        //}

        //private void _Timer_Tick(object sender, System.EventArgs e)
        //{
        //    var MS = DateTime.Now.Subtract(_FirstTimer).TotalMilliseconds;
        //    var Going = false;
        //    _Timer_Tick_count++;
        //    if (Tag is System.Windows.Forms.Control tempVar)
        //    {
        //        if (!tempVar.Visible)
        //        {
        //            _Timer_Tick_count = 0;
        //            Close();
        //            return;
        //        }
        //    }
        //    if (FloatInAndOutMilliSek > 0)
        //    {
        //        // Da das System oft ausgelastet ist, erst ein paar Dummy-Leerläufe, daß das Fenster dann angezeigt wird, wenn das System Luft hat
        //        if (_Timer_Tick_count == 6)
        //        {
        //            _Timer_Tick_WasVisible = false;
        //            _FirstTimer = DateTime.Now;
        //        }
        //        if (_Timer_Tick_count < 7) { return; }
        //    }
        //    if (UnloadAfterSek > 0)
        //    {
        //        if (DateTime.Now.Subtract(_FirstTimer).TotalSeconds > UnloadAfterSek)
        //        {
        //            Close();
        //            OnCancel();
        //            return;
        //        }
        //    }
        //    if (FloatInAndOutMilliSek > 0)
        //    {
        //        double _Proz = 0;
        //        if (MS > UpDownSpeed + FloatInAndOutMilliSek * 0.3 && !_Timer_Tick_WasVisible)
        //        {
        //            _FirstTimer = DateTime.Now.Subtract(new TimeSpan(0, 0, 0, 0, UpDownSpeed));
        //            MS = UpDownSpeed;
        //        }
        //        if (MS < UpDownSpeed)
        //        {
        //            _Proz = MS / UpDownSpeed;
        //        }
        //        else if (MS >= UpDownSpeed && MS <= UpDownSpeed + FloatInAndOutMilliSek)
        //        {
        //            _Proz = 1;
        //            _Timer_Tick_WasVisible = true;
        //        }
        //        else if (MS > UpDownSpeed + FloatInAndOutMilliSek && MS < FloatInAndOutMilliSek + UpDownSpeed * 2)
        //        {
        //            _Proz = (FloatInAndOutMilliSek + UpDownSpeed * 2 - MS) / UpDownSpeed;
        //            Going = true;
        //        }
        //        else
        //        {
        //            _Timer_Tick_count = 0;
        //            Close();
        //            return;
        //        }
        //        if (this.Design == enDesign.Form_DesktopBenachrichtigung)
        //        {
        //            Left = System.Windows.Forms.Screen.PrimaryScreen.Bounds.Size.Width - Width - Skin.Padding * 2;
        //            Region = new Region(new Rectangle(0, 0, Width, (int)(Math.Truncate(Height * _Proz))));
        //            Top = (int)(Math.Truncate(System.Windows.Forms.Screen.PrimaryScreen.WorkingArea.Bottom - Height * _Proz));
        //            Opacity = _Proz;
        //        }
        //        else
        //        {
        //            Opacity = _Proz;
        //            if (Going)
        //            {
        //                Top -= (int)((1 - _Proz) * 5);
        //            }
        //        }
        //    }
        //}
        //public void FadeOut() {
        //    if (FloatInAndOutMilliSek > 300) {
        //        if (DateTime.Now.Subtract(_FirstTimer).TotalMilliseconds > UpDownSpeed && DateTime.Now.Subtract(_FirstTimer).TotalMilliseconds < FloatInAndOutMilliSek + UpDownSpeed) {
        //            _FirstTimer = DateTime.Now.Subtract(new TimeSpan(0, 0, 0, 0, UpDownSpeed + 10));
        //        }
        //        FloatInAndOutMilliSek = 300;
        //    }
        //}

        private static void Movement(object sender, DoWorkEventArgs e) {
            Notification x = null;
            try {
                x = new((string)e.Argument);
                x.Show();

                var lowestY = System.Windows.Forms.Screen.PrimaryScreen.WorkingArea.Bottom - x.Height - 1;// - Skin.Padding;
                var pixelfromLower = System.Windows.Forms.Screen.PrimaryScreen.Bounds.Bottom - lowestY;
                x.Top = lowestY;

                var _FirstTimer = DateTime.Now;
                bool hiddenNow;
                var outime = new DateTime(0);

                do {
                    var MS = DateTime.Now.Subtract(_FirstTimer).TotalMilliseconds;
                    //var diff = MS - lastMS;
                    //lastMS = MS;

                    #region Anzeige-Status (Richtung, Prozent) bestimmen

                    var hasBelow = false;
                    hiddenNow = false;
                    var SpeedIn = 250d; // Wegen Recheoperation
                    var SpeedOut = 100d; // Wegen Recheoperation
                    var Left = System.Windows.Forms.Screen.PrimaryScreen.Bounds.Size.Width - x.Width - 1;
                    var Top = lowestY;

                    if (x.NoteBelow != null && AllBoxes.Contains(x.NoteBelow)) {
                        Top = Math.Min(x.NoteBelow.Top - x.Height - 1, lowestY);
                        hasBelow = true;
                    }

                    if (MS < SpeedIn) {
                        // Kommt von Rechts reingeflogen
                        x.Opacity = MS / SpeedIn;
                        //Left = (int)(System.Windows.Forms.Screen.PrimaryScreen.Bounds.Size.Width - (x.Width - (Skin.Padding * 2)) * x.Opacity); // Opacity == Prozent
                        Top = Math.Min(Top, lowestY);
                    } else if (x.Top >= System.Windows.Forms.Screen.PrimaryScreen.Bounds.Size.Height || x.Opacity < 0.01) {
                        //Lebensdauer überschritten
                        hiddenNow = true;
                    } else if (MS > x.ScreenTime - SpeedIn) {
                        // War lange genug da, darf wieder raus
                        if (!hasBelow) {
                            if (outime.Ticks == 0) { outime = DateTime.Now; }

                            var MSo = DateTime.Now.Subtract(outime).TotalMilliseconds;

                            x.Opacity = 1 - MSo / SpeedOut;
                            //Top = (int)(lowestY + pixelfromLower * (MSo / SpeedOut)) + 1;
                            //Left = x.Left + (int)Math.Max(diff / 17, 1);
                        } else {
                            x.Opacity = 1;
                        }
                    } else {
                        //Hauptanzeige ist gerade
                        x.Opacity = 1;
                        Top = Math.Min(Top, lowestY);
                    }

                    #endregion

                    if (x.Left != Left || x.Top != Top) {
                        x.Left = Left;
                        //x.Region = new Region(new Rectangle(0, 0, x.Width, (int)Math.Truncate(x.Height * Prozent)));
                        x.Top = Top;
                        //x.Refresh();
                        Develop.DoEvents();
                    }

                    if (_FirstTimer.Subtract(DateTime.Now).TotalMinutes > 2 || x.UserClicked) { break; }
                } while (!hiddenNow);
            } catch { }

            try {
                if (x != null && !x.IsDisposed) {
                    x.Close();
                    x.Dispose();
                }
            } catch { }
        }

        #endregion
    }
}