#region BlueElements - a collection of useful tools, database and controls
// Authors: 
// Christian Peter
// 
// Copyright (c) 2020 Christian Peter
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
using System;
using System.Drawing;

namespace BlueControls.Forms
{
    public partial class Notification : FloatingForm
    {

        private int _Timer_Tick_count;
        private bool _Timer_Tick_WasVisible;
        private DateTime _FirstTimer; // Startzeit für UnloadAfterSek
        private const int UpDownSpeed = 250;

        //internal int UnloadAfterSek = -999;
        internal int FloatInAndOutMilliSek = -999;


        private Notification()
        {
            InitializeComponent();
        }

        private Notification(string Text)
        {
            InitializeComponent();


            capTXT.Text = Text;


            var He = Math.Min(capTXT.TextRequiredSize().Height, (int)(System.Windows.Forms.Screen.PrimaryScreen.Bounds.Size.Height * 0.7));
            var Wi = Math.Min(capTXT.TextRequiredSize().Width, (int)(System.Windows.Forms.Screen.PrimaryScreen.Bounds.Size.Width * 0.7));


            this.Size = new Size(Wi + capTXT.Left * 2, He + capTXT.Top * 2);

            this.Location = new Point(-Width - 10, Height - 10);
            FloatInAndOutMilliSek = Math.Max(3200, Text.Length * 110);

            timNote.Enabled = true;
            _FirstTimer = DateTime.Now;

        }


        public static void Show(string Text)
        {
            CloseAll();
            if (string.IsNullOrEmpty(Text)) { return; }
            var x = new Notification(Text);
            x.Show();
        }


        public static void Show(string TXT, enImageCode Pic)
        {

            if (Pic != enImageCode.None)
            {
                TXT = "<ImageCode=" + Enum.GetName(Pic.GetType(), Pic) + "|32> <zbx_store><top>" + TXT;
            }

            Show(TXT);

        }



        public static void CloseAll()
        {

            foreach (var ThisForm in AllBoxes)
            {
                if (!ThisForm.IsDisposed && ThisForm is Notification )
                {
                    try
                    {
                        ThisForm.Close();
                        CloseAll();
                        return;
                    }
                    catch (Exception ex)
                    {
                        Develop.DebugPrint(ex);
                    }
                }
            }
        }


        public new void Close()
        {


            foreach (var ThisForm in AllBoxes)
            {
                if (!ThisForm.IsDisposed && ThisForm is Notification)
                {
                    try
                    {
                        base.Close();
                        return;
                    }
                    catch (Exception ex)
                    {
                        Develop.DebugPrint(ex);
                    }
                }
            }
        }



        private void timNote_Tick(object sender, System.EventArgs e)
        {
            var MS = DateTime.Now.Subtract(_FirstTimer).TotalMilliseconds;
            _Timer_Tick_count += 1;

            if (Tag is System.Windows.Forms.Control tempVar)
            {
                if (!tempVar.Visible)
                {
                    _Timer_Tick_count = 0;
                    Close();
                    return;
                }
            }


            if (FloatInAndOutMilliSek > 0)
            {
                // Da das System oft ausgelastet ist, erst ein paar Dummy-Leerläufe, daß das Fenster dann angezeigt wird, wenn das System Luft hat
                if (_Timer_Tick_count == 6)
                {
                    _Timer_Tick_WasVisible = false;
                    _FirstTimer = DateTime.Now;
                }
                if (_Timer_Tick_count < 7) { return; }
            }







            if (FloatInAndOutMilliSek > 0)
            {
                double _Proz = 0;

                if (MS > UpDownSpeed + FloatInAndOutMilliSek * 0.3 && !_Timer_Tick_WasVisible)
                {
                    _FirstTimer = DateTime.Now.Subtract(new TimeSpan(0, 0, 0, 0, UpDownSpeed));
                    MS = UpDownSpeed;
                }

                if (MS < UpDownSpeed)
                {
                    _Proz = MS / UpDownSpeed;
                }
                else if (MS >= UpDownSpeed && MS <= UpDownSpeed + FloatInAndOutMilliSek)
                {
                    _Proz = 1;
                    _Timer_Tick_WasVisible = true;

                }
                else if (MS > UpDownSpeed + FloatInAndOutMilliSek && MS < FloatInAndOutMilliSek + UpDownSpeed * 2)
                {
                    _Proz = (FloatInAndOutMilliSek + UpDownSpeed * 2 - MS) / UpDownSpeed;
                    //Going = true;
                }
                else
                {
                    _Timer_Tick_count = 0;
                    timNote.Enabled = false;
                    Close();
                    return;
                }



                Left = System.Windows.Forms.Screen.PrimaryScreen.Bounds.Size.Width - Width - Skin.Padding * 2;
                Region = new Region(new Rectangle(0, 0, Width, (int)Math.Truncate(Height * _Proz)));
                Top = (int)Math.Truncate(System.Windows.Forms.Screen.PrimaryScreen.WorkingArea.Bottom - Height * _Proz);
                Opacity = _Proz;


            }
        }





        //private void _Timer_Tick(object sender, System.EventArgs e)
        //{
        //    var MS = DateTime.Now.Subtract(_FirstTimer).TotalMilliseconds;
        //    var Going = false;
        //    _Timer_Tick_count += 1;

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

        public void FadeOut()
        {
            if (FloatInAndOutMilliSek > 300)
            {
                if (DateTime.Now.Subtract(_FirstTimer).TotalMilliseconds > UpDownSpeed && DateTime.Now.Subtract(_FirstTimer).TotalMilliseconds < FloatInAndOutMilliSek + UpDownSpeed)
                {
                    _FirstTimer = DateTime.Now.Subtract(new TimeSpan(0, 0, 0, 0, UpDownSpeed + 10));
                }

                FloatInAndOutMilliSek = 300;
            }
        }

        private void capTXT_Click(object sender, System.EventArgs e)
        {
            FadeOut();
        }
    }
}
