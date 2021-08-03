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

using BlueBasics;
using BlueBasics.Enums;
using BlueControls.EventArgs;
using BlueControls.Forms;
using System;
using System.Drawing;
using static BlueBasics.BitmapExt;
using static BlueBasics.Develop;
using static BlueBasics.Generic;

namespace BluePaint {

    public partial class Tool_Clipping {

        #region Constructors

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

        #endregion

        #region Methods

        public void CheckMinMax() {
            var _Pic = OnNeedCurrentPic();
            if (_Pic == null) { return; }
            Links.Maximum = _Pic.Width - 1;
            Recht.Minimum = -_Pic.Width + 1;
            Oben.Maximum = _Pic.Height - 1;
            Unten.Minimum = -_Pic.Height - 1;
        }

        public override void DoAdditionalDrawing(AdditionalDrawing e, Bitmap OriginalPic) {
            if (OriginalPic == null) { return; }
            Pen Pen_Blau = new(Color.FromArgb(150, 0, 0, 255));
            DrawZusatz(e, OriginalPic);
            if (e.Current == null) { return; }
            e.DrawLine(Pen_Blau, e.Current.TrimmedX, -1, e.Current.TrimmedX, OriginalPic.Height);
            e.DrawLine(Pen_Blau, -1, e.Current.TrimmedY, OriginalPic.Width, e.Current.TrimmedY);
            if (e.Current.Button == System.Windows.Forms.MouseButtons.Left && e.MouseDown != null) {
                e.DrawLine(Pen_Blau, e.MouseDown.X, -1, e.MouseDown.X, OriginalPic.Height);
                e.DrawLine(Pen_Blau, -1, e.MouseDown.Y, OriginalPic.Width, e.MouseDown.Y);
            }
        }

        public void DrawZusatz(AdditionalDrawing e, Bitmap OriginalPic) {
            SolidBrush Brush_Blau = new(Color.FromArgb(120, 0, 0, 255));
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

        public override void ExcuteCommand(string command) {
            var c = command.SplitBy(";");
            if (c[0] == "AutoZuschnitt") {
                CheckMinMax();
                AutoZ_Click(null, null);
                ZuschnittOK_Click(null, null);
            } else {
                DebugPrint_NichtImplementiert();
            }
        }

        public override string MacroKennung() => "Clipping";

        public override void MouseDown(MouseEventArgs1_1 e, Bitmap OriginalPic) => OnDoInvalidate();

        public override void MouseMove(MouseEventArgs1_1DownAndCurrent e, Bitmap OriginalPic) => OnDoInvalidate();

        public override void MouseUp(MouseEventArgs1_1DownAndCurrent e, Bitmap OriginalPic) {
            if (OriginalPic == null) { return; }
            Links.Value = Math.Min(e.Current.TrimmedX, e.MouseDown.TrimmedX) + 1;
            Recht.Value = -(OriginalPic.Width - Math.Max(e.Current.TrimmedX, e.MouseDown.TrimmedX));
            Oben.Value = Math.Min(e.Current.TrimmedY, e.MouseDown.TrimmedY) + 1;
            Unten.Value = -(OriginalPic.Height - Math.Max(e.Current.TrimmedY, e.MouseDown.TrimmedY));
            ValueChangedByClicking(this, System.EventArgs.Empty);
        }

        public override void OnToolChanging() => WollenSieDenZuschnittÜbernehmen();

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

        public override void ToolFirstShown() {
            CheckMinMax();
            AutoZ_Click(null, null);
        }

        private void AutoZ_Click(object sender, System.EventArgs e) {
            WollenSieDenZuschnittÜbernehmen();
            var _Pic = base.OnNeedCurrentPic();
            OnZoomFit();
            var pa = GetAutoValuesForCrop(_Pic, 0.9);
            Links.Value = pa.Left;
            Recht.Value = pa.Right;
            Oben.Value = pa.Top;
            Unten.Value = pa.Bottom;
            OnDoInvalidate();
        }

        private void ValueChangedByClicking(object sender, System.EventArgs e) => OnDoInvalidate();

        private void WollenSieDenZuschnittÜbernehmen() {
            if (Links.Value <= 0 && Recht.Value >= 0 && Oben.Value <= 0 && Unten.Value >= 0) { return; }
            if (MessageBox.Show("Soll der <b>aktuelle</b> Zuschnitt<br>übernommen werden?", enImageCode.Zuschneiden, "Ja", "Nein") == 1) { return; }
            ZuschnittOK_Click(null, System.EventArgs.Empty);
        }

        private void ZuschnittOK_Click(object sender, System.EventArgs e) {
            var _Pic = OnNeedCurrentPic();
            var _BMP2 = Crop(_Pic, (int)Links.Value, (int)Recht.Value, (int)Oben.Value, (int)Unten.Value);
            OnOverridePic(_BMP2);
            Links.Value = 0;
            Recht.Value = 0;
            Oben.Value = 0;
            Unten.Value = 0;
            CollectGarbage();
            CheckMinMax();
            OnZoomFit();
        }

        #endregion
    }
}