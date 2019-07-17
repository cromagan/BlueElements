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

namespace BlueControls.Forms
{
    public partial class Progressbar : FloatingForm
    {

        private int eProgressbar_LastCurrent = int.MaxValue;

        private readonly Dictionary<int, DateTime> eProgressbar_TimeDic = new Dictionary<int, DateTime>();
        private int eProgressbar_LastCalulatedSeconds = int.MinValue;
        private DateTime eProgressbar_LastTimeUpdate = DateTime.Now;

        private int _count = 0;
        private string _baseText = string.Empty;

        private Progressbar()
        {
            InitializeComponent();
        }

        private Progressbar(string Text)
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


            eProgressbar_LastCurrent = Current;

            if (!eProgressbar_TimeDic.ContainsKey(Current))
            {
                eProgressbar_TimeDic.Add(Current, DateTime.Now);
            }


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
            if (Count < 1)
            {
                T = string.Empty;
            }
            else if (Current <= 3)
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
