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

using BlueControls.Forms;
using System;
using System.Collections.Generic;
using System.Drawing;

namespace BlueControls.DialogBoxes
{
    public partial class Progressbar : FloatingForm
    {

        private int eProgressbar_LastCurrent = int.MaxValue;

        private readonly Dictionary<int, DateTime> eProgressbar_TimeDic = new Dictionary<int, DateTime>();
        private int eProgressbar_LastCalulatedSeconds = int.MinValue;
        private DateTime eProgressbar_LastTimeUpdate = DateTime.Now;

        private int _count = 0;
        private string _baseText = string.Empty;

        public Progressbar()
        {
            InitializeComponent();
        }

        public Progressbar(string Text)
        {
            InitializeComponent();
            capTXT.Text = Text;
            var He = Math.Min(capTXT.TextRequiredSize().Height, (int)(System.Windows.Forms.Screen.PrimaryScreen.Bounds.Size.Height * 0.7));
            var Wi = Math.Min(capTXT.TextRequiredSize().Width, (int)(System.Windows.Forms.Screen.PrimaryScreen.Bounds.Size.Width * 0.7));
            this.Size = new Size(Wi + capTXT.Left * 2, He + capTXT.Top * 2);
        }


        public static Progressbar Show(string Text)
        {
            var P = new Progressbar(Text);
            P._baseText = Text;
            P.Show();
            return P;
        }

        //public static Progressbar Show(string Text, System.Windows.Forms.Form Parentform)
        //{
        //    return new Progressbar(Text);
        //}

        public static Progressbar Show(string Text, int Count)
        {
            var P = new Progressbar(Text);
            P._baseText = Text;
            P._count = Count;
            P.Update(0);
            P.Show();
            P.BringToFront();
            return P;
        }
        //public static Progressbar Show(string Text, int Current, int Count, System.Windows.Forms.Form Parentform)
        //{
        //    return new Progressbar(CalculateText(Text, Current,Count));
        //}


        //#region  eProgressbar 


        //private static string _PrBarText = "";



        //public static void eProgressbar(string txt, System.Windows.Forms.Form Parentform)
        //{

        //    if (string.IsNullOrEmpty(txt))
        //    {
        //        P?.Close();
        //        return;
        //    }

        //    if (frmProgressBar == null || frmProgressBar.IsDisposed)
        //    {
        //        frmProgressBar = new DialogBox(true, null);
        //        frmProgressBar.Design = enDesign.Form_BitteWarten;
        //        frmProgressBar.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
        //    }
        //    else
        //    {
        //        if (_PrBarText == txt) { return; }

        //        if (frmProgressBar.InvokeRequired)
        //        {
        //            P?.Close();
        //            eProgressbar(txt, Parentform);
        //            return;
        //        }
        //    }

        //    _PrBarText = txt;


        //    frmProgressBar.Generate_Caption(txt, Skin.Padding, Skin.Padding);

        //    if (frmProgressBar._Caption1.Width + Skin.Padding * 2 != frmProgressBar.Width)
        //    {
        //        frmProgressBar.Width = Math.Max(frmProgressBar._Caption1.Width + Skin.Padding * 2, frmProgressBar.Width);
        //        if (Parentform != null)
        //        {
        //            frmProgressBar.Left = (int)(Parentform.Left + (Parentform.Width - frmProgressBar.Width) / 2.0);
        //        }
        //        else
        //        {
        //            frmProgressBar.Position_CenterScreen(Point.Empty);
        //        }
        //    }

        //    if (frmProgressBar._Caption1.Height + Skin.Padding * 2 != frmProgressBar.Height)
        //    {
        //        frmProgressBar.Height = Math.Max(frmProgressBar._Caption1.Height + Skin.Padding * 2, frmProgressBar.Height);
        //        if (Parentform != null)
        //        {
        //            frmProgressBar.Top = (int)(Parentform.Top + (Parentform.Height - frmProgressBar.Height) / 2.0);
        //        }
        //        else
        //        {
        //            frmProgressBar.Position_CenterScreen(Point.Empty);
        //        }
        //    }


        //    frmProgressBar.ShowFloating();
        //}

        private string CalculateText(string BaseText, int Current, int Count)
        {

            var tmpCalculatedSeconds = 0;

            if (Current < eProgressbar_LastCurrent)
            {
                eProgressbar_TimeDic.Clear();
                eProgressbar_LastTimeUpdate = DateTime.Now;
                eProgressbar_LastCalulatedSeconds = int.MinValue;
            }

            var PR = Current / (double)Count;
            if (PR > 1) { PR = 1; }
            if (PR < 0) { PR = 0; }

            if (double.IsNaN(PR)) { PR = 0; } 


            if (Current > 0)
            {
                if (eProgressbar_TimeDic.ContainsKey(Math.Max(0, Current - 100)))
                {
                    var d = eProgressbar_TimeDic[Math.Max(0, Current - 100)];
                    var ts = DateTime.Now.Subtract(d).TotalSeconds;
                    tmpCalculatedSeconds = (int)(ts / Math.Min(Current, 100) * (Count - Current));
                }
                else
                {
                    tmpCalculatedSeconds = int.MinValue;
                }
            }
            else
            {
                tmpCalculatedSeconds = 0;
            }


            //if (Current > 3 && tmpCalculatedSeconds > 6 && PR < 0.7)
            //{
            //    NeedToShow = true;
            //}
            //if (Current > 3 && tmpCalculatedSeconds > 20)
            //{
            //    NeedToShow = true;
            //}
            //if (eProgressbar_Showed)
            //{
            //    NeedToShow = true;
            //}


            eProgressbar_LastCurrent = Current;

            if (!eProgressbar_TimeDic.ContainsKey(Current))
            {
                eProgressbar_TimeDic.Add(Current, DateTime.Now);
            }

            //if (!NeedToShow)
            //{
            //    P?.Close();
            //    return;
            //}

            //eProgressbar_Showed = true;


            if (eProgressbar_LastCalulatedSeconds != tmpCalculatedSeconds && DateTime.Now.Subtract(eProgressbar_LastTimeUpdate).TotalSeconds > 5)
            {
                eProgressbar_LastTimeUpdate = DateTime.Now;
                if (Current < 2)
                {
                    eProgressbar_LastCalulatedSeconds = tmpCalculatedSeconds;
                }
                if (tmpCalculatedSeconds < eProgressbar_LastCalulatedSeconds * 0.9)
                {
                    eProgressbar_LastCalulatedSeconds = tmpCalculatedSeconds;
                }
                if (tmpCalculatedSeconds > eProgressbar_LastCalulatedSeconds * 1.5)
                {
                    eProgressbar_LastCalulatedSeconds = tmpCalculatedSeconds;
                }
            }

            var PRT = (int)(PR * 100);
            if (PRT > 100) { PRT = 100; }
            if (PRT < 0) { PRT = 0; }


            string T = null;
            if (Current <= 3)
            {
                T = "<br>Restzeit wird ermittelt<tab>";
            }
            else if (eProgressbar_LastCalulatedSeconds < -10)
            {
                T = "<br>Restzeit wird ermittelt<tab>";
            }
            else if (eProgressbar_LastCalulatedSeconds > 94)
            {
                T = "<br>" + PRT + " % - Geschätzte Restzeit:   " + (int)(eProgressbar_LastCalulatedSeconds / 60) + " Minuten<tab>";
            }
            else if (eProgressbar_LastCalulatedSeconds > 10)
            {
                T = "<br>" + PRT + " % - Geschätzte Restzeit: " + (int)(eProgressbar_LastCalulatedSeconds / 5) * 5 + " Sekunden<tab>";
            }
            else if (eProgressbar_LastCalulatedSeconds > 0)
            {
                T = "<br>" + PRT + " % - Geschätzte Restzeit: <<> 10 Sekunden<tab>";
            }
            else
            {
                T = "<br>100 % - ...abgeschlossen!<tab>";
            }


            return BaseText + "</b></i></u>" + T;
        }

        public void Update(string Text)
        {
            _baseText = Text;
            UpdateInternal(Text);
        }

        public void Update(int Current)
        {
            UpdateInternal(CalculateText(_baseText, Current, _count));
        }

        private void UpdateInternal(string Text)
        {
            if (Text != capTXT.Text)
            {
                capTXT.Text = Text;
                var Wi = Math.Max(Size.Width, capTXT.Width + Skin.Padding * 2);
                var He = Math.Max(Size.Height, capTXT.Height + Skin.Padding * 2);

                Size = new Size(Wi, He);
                Refresh();
            }
        }


    }
}
