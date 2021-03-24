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
using BlueBasics.Enums;
using BlueControls.EventArgs;
using BlueControls.Forms;
using System;
using System.Drawing;
using static BlueBasics.Develop;
using static BlueBasics.modAllgemein;

namespace BluePaint {

    public partial class Tool_Clipping {
        public Tool_Clipping(bool aufnahme) : base() {
            InitializeComponent();

            if (aufnahme) {
                CheckMinMax();
                AutoZ_Click(null, null);
                ZuschnittOK_Click(null, null);
                Enabled = false;
                MessageBox.Show("Automatisch zugeschnitten.");
                OnCommandForMacro("AutoZuschnitt");
            }

        }

        public override void ToolFirstShown() {
            CheckMinMax();
            AutoZ_Click(null, null);
        }


        public override void MouseDown(BlueControls.EventArgs.MouseEventArgs1_1 e, Bitmap OriginalPic) {
            OnDoInvalidate();
        }

        public override void MouseMove(BlueControls.EventArgs.MouseEventArgs1_1DownAndCurrent e, Bitmap OriginalPic) {
            OnDoInvalidate();
        }

        public override void MouseUp(BlueControls.EventArgs.MouseEventArgs1_1DownAndCurrent e, Bitmap OriginalPic) {
            if (OriginalPic == null) { return; }

            Links.Value = Math.Min(e.Current.TrimmedX, e.MouseDown.TrimmedX) + 1;
            Recht.Value = -(OriginalPic.Width - Math.Max(e.Current.TrimmedX, e.MouseDown.TrimmedX));
            Oben.Value = Math.Min(e.Current.TrimmedY, e.MouseDown.TrimmedY) + 1;
            Unten.Value = -(OriginalPic.Height - Math.Max(e.Current.TrimmedY, e.MouseDown.TrimmedY));

            ValueChangedByClicking(this, System.EventArgs.Empty);

        }
        public override void DoAdditionalDrawing(AdditionalDrawing e, Bitmap OriginalPic) {
            if (OriginalPic == null) { return; }

            var Pen_Blau = new Pen(Color.FromArgb(150, 0, 0, 255));


            DrawZusatz(e, OriginalPic);


            if (e.Current == null) { return; }

            e.DrawLine(Pen_Blau, e.Current.TrimmedX, -1, e.Current.TrimmedX, OriginalPic.Height);
            e.DrawLine(Pen_Blau, -1, e.Current.TrimmedY, OriginalPic.Width, e.Current.TrimmedY);

            if (e.Current.Button == System.Windows.Forms.MouseButtons.Left && e.MouseDown != null) {
                e.DrawLine(Pen_Blau, e.MouseDown.X, -1, e.MouseDown.X, OriginalPic.Height);
                e.DrawLine(Pen_Blau, -1, e.MouseDown.Y, OriginalPic.Width, e.MouseDown.Y);
            }

        }


        private void ValueChangedByClicking(object sender, System.EventArgs e) {
            OnDoInvalidate();
        }

        public void DrawZusatz(AdditionalDrawing e, Bitmap OriginalPic) {
            var Brush_Blau = new SolidBrush(Color.FromArgb(120, 0, 0, 255));


            if (Links.Value != 0) {
                e.FillRectangle(Brush_Blau, new Rectangle(0, 0, Convert.ToInt32(Links.Value), OriginalPic.Height));
            }
            if (Recht.Value != 0) {
                e.FillRectangle(Brush_Blau, new Rectangle(OriginalPic.Width + Convert.ToInt32(Recht.Value), 0, (int)-Recht.Value, OriginalPic.Height));
            }

            if (Oben.Value != 0) {
                e.FillRectangle(Brush_Blau, new Rectangle(0, 0, OriginalPic.Width, Convert.ToInt32(Oben.Value)));
            }
            if (Unten.Value != 0) {
                e.FillRectangle(Brush_Blau, new Rectangle(0, OriginalPic.Height + Convert.ToInt32(Unten.Value), OriginalPic.Width, (int)-Unten.Value));
            }


        }


        private void ZuschnittOK_Click(object sender, System.EventArgs e) {
            var _Pic = OnNeedCurrentPic();

            var _BMP2 = _Pic.Crop((int)Links.Value, (int)Recht.Value, (int)Oben.Value, (int)Unten.Value);


            OnOverridePic(_BMP2);
            Links.Value = 0;
            Recht.Value = 0;
            Oben.Value = 0;
            Unten.Value = 0;

            CollectGarbage();

            CheckMinMax();

            OnZoomFit();
        }

        private void AutoZ_Click(object sender, System.EventArgs e) {

            var _Pic = base.OnNeedCurrentPic();
            OnZoomFit();

            var pa = _Pic.GetAutoValuesForCrop(0.9);

            Links.Value = pa.Left;
            Recht.Value = pa.Right;
            Oben.Value = pa.Top;
            Unten.Value = pa.Bottom;

            OnDoInvalidate();

        }

        public void Set(int Left, int Top, int Right, int Bottom) {

            if (Left < 0 || Top < 0 || Right > 0 || Bottom > 0) {
                DebugPrint(enFehlerArt.Warnung, "Fehler in den Angaben");
            }
            CheckMinMax();


            Links.Value = Left;
            Oben.Value = Top;
            Recht.Value = Right;
            Unten.Value = Bottom;
        }

        public void CheckMinMax() {
            var _Pic = OnNeedCurrentPic();

            if (_Pic == null) { return; }
            Links.Maximum = _Pic.Width - 1;
            Recht.Minimum = -_Pic.Width + 1;
            Oben.Maximum = _Pic.Height - 1;
            Unten.Minimum = -_Pic.Height - 1;
        }

        public override string MacroKennung() {
            return "Clipping";
        }

        public override void ExcuteCommand(string command) {
            var c = command.SplitBy(";");

            if (c[0] == "AutoZuschnitt") {
                CheckMinMax();
                AutoZ_Click(null, null);
                ZuschnittOK_Click(null, null);
            } else {
                Develop.DebugPrint_NichtImplementiert();
            }
        }

    }

}