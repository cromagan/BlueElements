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
using BlueControls.EventArgs;
using BlueControls.Forms;
using System;
using System.Drawing;
using static BlueBasics.BitmapExt;
using static BlueBasics.Develop;
using static BlueBasics.Generic;

#nullable enable

namespace BluePaint {

    public partial class Tool_Clipping {

        #region Constructors

        public Tool_Clipping() : base() {
            InitializeComponent();

            CheckMinMax();
            AutoZ_Click(null, null);
            ZuschnittOK_Click(null, null);
            MessageBox.Show("Automatisch zugeschnitten.");
        }

        #endregion

        #region Methods

        public void CheckMinMax() {
            var pic = OnNeedCurrentPic();
            if (pic == null) { return; }
            Links.Maximum = pic.Width - 1;
            Recht.Minimum = -pic.Width + 1;
            Oben.Maximum = pic.Height - 1;
            Unten.Minimum = -pic.Height - 1;
        }

        public override void DoAdditionalDrawing(AdditionalDrawing e, Bitmap? originalPic) {
            if (originalPic == null) { return; }
            Pen penBlau = new(Color.FromArgb(150, 0, 0, 255));
            DrawZusatz(e, originalPic);
            if (e.Current == null) { return; }
            e.DrawLine(penBlau, e.Current.TrimmedX, -1, e.Current.TrimmedX, originalPic.Height);
            e.DrawLine(penBlau, -1, e.Current.TrimmedY, originalPic.Width, e.Current.TrimmedY);

            if (e.Current.Button == System.Windows.Forms.MouseButtons.Left && e.MouseDown != null) {
                e.DrawLine(penBlau, e.MouseDown.X, -1, e.MouseDown.X, originalPic.Height);
                e.DrawLine(penBlau, -1, e.MouseDown.Y, originalPic.Width, e.MouseDown.Y);
            }
        }

        public void DrawZusatz(AdditionalDrawing e, Bitmap? originalPic) {
            SolidBrush brushBlau = new(Color.FromArgb(120, 0, 0, 255));
            if (Links.Value != 0) {
                e.FillRectangle(brushBlau, new Rectangle(0, 0, Convert.ToInt32(Links.Value), originalPic.Height));
            }
            if (Recht.Value != 0) {
                e.FillRectangle(brushBlau, new Rectangle(originalPic.Width + Convert.ToInt32(Recht.Value), 0, (int)-Recht.Value, originalPic.Height));
            }
            if (Oben.Value != 0) {
                e.FillRectangle(brushBlau, new Rectangle(0, 0, originalPic.Width, Convert.ToInt32(Oben.Value)));
            }
            if (Unten.Value != 0) {
                e.FillRectangle(brushBlau, new Rectangle(0, originalPic.Height + Convert.ToInt32(Unten.Value), originalPic.Width, (int)-Unten.Value));
            }
        }

        public override void ExcuteCommand(string command) {
            var c = command.SplitAndCutBy(";");
            if (c[0] == "AutoZuschnitt") {
                CheckMinMax();
                AutoZ_Click(null, null);
                ZuschnittOK_Click(null, null);
            } else {
                DebugPrint_NichtImplementiert();
            }
        }

        public override void MouseDown(MouseEventArgs1_1 e, Bitmap? originalPic) => OnDoInvalidate();

        public override void MouseMove(MouseEventArgs1_1DownAndCurrent e, Bitmap? originalPic) => OnDoInvalidate();

        public override void MouseUp(MouseEventArgs1_1DownAndCurrent e, Bitmap? originalPic) {
            if (originalPic == null) { return; }
            Links.Value = Math.Min(e.Current.TrimmedX, e.MouseDown.TrimmedX) + 1;
            Recht.Value = -(originalPic.Width - Math.Max(e.Current.TrimmedX, e.MouseDown.TrimmedX));
            Oben.Value = Math.Min(e.Current.TrimmedY, e.MouseDown.TrimmedY) + 1;
            Unten.Value = -(originalPic.Height - Math.Max(e.Current.TrimmedY, e.MouseDown.TrimmedY));
            ValueChangedByClicking(this, System.EventArgs.Empty);
        }

        public override void OnToolChanging() => WollenSieDenZuschnittÜbernehmen();

        public void Set(int left, int top, int right, int bottom) {
            if (left < 0 || top < 0 || right > 0 || bottom > 0) {
                DebugPrint(enFehlerArt.Warnung, "Fehler in den Angaben");
            }
            CheckMinMax();
            Links.Value = left;
            Oben.Value = top;
            Recht.Value = right;
            Unten.Value = bottom;
        }

        internal override void ToolFirstShown() {
            CheckMinMax();
            AutoZ_Click(null, null);
        }

        private void AutoZ_Click(object? sender, System.EventArgs? e) {
            WollenSieDenZuschnittÜbernehmen();
            var pic = base.OnNeedCurrentPic();
            OnZoomFit();
            var pa = GetAutoValuesForCrop(pic, 0.9);
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

        private void ZuschnittOK_Click(object? sender, System.EventArgs? e) {
            var pic = OnNeedCurrentPic();
            var bmp2 = Crop(pic, (int)Links.Value, (int)Recht.Value, (int)Oben.Value, (int)Unten.Value);
            OnOverridePic(bmp2);
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