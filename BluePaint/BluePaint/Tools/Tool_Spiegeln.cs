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

using BlueControls;
using BlueControls.EventArgs;
using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using static BlueBasics.modAllgemein;

namespace BluePaint {
    public partial class Tool_Spiegeln : GenericTool // System.Windows.Forms.UserControl // 
    {

        private bool _ausricht = false;

        public Tool_Spiegeln() : base() {
            InitializeComponent();
        }

        private void SpiegelnH_Click(object sender, System.EventArgs e) {
            _ausricht = false;
            var _Pic = OnNeedCurrentPic();
            if (_Pic == null) { return; }
            CollectGarbage();
            _Pic.RotateFlip(RotateFlipType.RotateNoneFlipY);
            OnOverridePic(_Pic);
            OnZoomFit();
        }

        private void SpiegelnV_Click(object sender, System.EventArgs e) {
            _ausricht = false;
            var _Pic = OnNeedCurrentPic();
            if (_Pic == null) { return; }
            CollectGarbage();
            _Pic.RotateFlip(RotateFlipType.RotateNoneFlipX);
            OnOverridePic(_Pic);
            OnZoomFit();
        }

        private void btnDrehenR_Click(object sender, System.EventArgs e) {
            _ausricht = false;
            var _Pic = OnNeedCurrentPic();
            if (_Pic == null) { return; }
            CollectGarbage();
            _Pic.RotateFlip(RotateFlipType.Rotate90FlipNone);
            OnOverridePic(_Pic);
            OnZoomFit();

        }

        private void btnDrehenL_Click(object sender, System.EventArgs e) {
            _ausricht = false;
            var _Pic = OnNeedCurrentPic();
            if (_Pic == null) { return; }
            CollectGarbage();
            _Pic.RotateFlip(RotateFlipType.Rotate270FlipNone);
            OnOverridePic(_Pic);
            OnZoomFit();

        }

        private void btnAusrichten_Click(object sender, System.EventArgs e) {

            _ausricht = true;
            OnDoInvalidate();
        }

        public override void DoAdditionalDrawing(AdditionalDrawing e, Bitmap OriginalPic) {
            if (!_ausricht) { return; }

            var _Pic = OnNeedCurrentPic();

            e.DrawLine(Pen_RedTransp, -1, e.Current.TrimmedY, _Pic.Width, e.Current.TrimmedY);
            e.DrawLine(Pen_RedTransp, e.Current.TrimmedX, -1, e.Current.TrimmedX, _Pic.Height);

            if (e.Current.Button == System.Windows.Forms.MouseButtons.Left && e.MouseDown != null) {
                e.DrawLine(Pen_RedTransp, -1, e.MouseDown.TrimmedY, _Pic.Width, e.MouseDown.TrimmedY);
                e.DrawLine(Pen_RedTransp, e.MouseDown.TrimmedX, -1, e.MouseDown.TrimmedX, _Pic.Height);

                e.DrawLine(Pen_LightWhite, e.Current.TrimmedX, e.Current.TrimmedY, e.MouseDown.TrimmedX, e.MouseDown.TrimmedY);
                e.DrawLine(Pen_RedTransp, e.Current.TrimmedX, e.Current.TrimmedY, e.MouseDown.TrimmedX, e.MouseDown.TrimmedY);

            }
        }

        public override void MouseDown(MouseEventArgs1_1 e, Bitmap OriginalPic) {
            if (!_ausricht) { return; }
            MouseMove(new MouseEventArgs1_1DownAndCurrent(e, e), OriginalPic);
        }

        public override void MouseMove(MouseEventArgs1_1DownAndCurrent e, Bitmap OriginalPic) {
            if (!_ausricht) { return; }
            OnDoInvalidate();
        }

        public override void MouseUp(MouseEventArgs1_1DownAndCurrent e, Bitmap OriginalPic) {

            if (!_ausricht) { return; }

            _ausricht = false;
            CollectGarbage();

            var _Pic = OnNeedCurrentPic();

            var Wink = (float)GeometryDF.Winkel(new PointM(e.MouseDown.X, e.MouseDown.Y), new PointM(e.Current.X, e.Current.Y));

            // Make a Matrix to represent rotation by this angle.
            var rotate_at_origin = new Matrix();
            rotate_at_origin.Rotate(Wink);

            // Rotate the image's corners to see how big it will be after rotation.
            PointF[] p = { new PointF(0, 0), new PointF(_Pic.Width, 0), new PointF(_Pic.Width, _Pic.Height), new PointF(0, _Pic.Height), };
            rotate_at_origin.TransformPoints(p);

            var MinX = Math.Min(Math.Min(p[0].X, p[1].X), Math.Min(p[2].X, p[3].X));
            var MinY = Math.Min(Math.Min(p[0].Y, p[1].Y), Math.Min(p[2].Y, p[3].Y));
            var MaxX = Math.Max(Math.Max(p[0].X, p[1].X), Math.Max(p[2].X, p[3].X));
            var MaxY = Math.Max(Math.Max(p[0].Y, p[1].Y), Math.Max(p[2].Y, p[3].Y));

            var B = (int)Math.Round(MaxX - MinX);
            var H = (int)Math.Round(MaxY - MinY);
            var nBMP = new Bitmap(B, H);

            // Create the real rotation transformation.
            var rotate_at_center = new Matrix();
            rotate_at_center.RotateAt(Wink, new PointF(B / 2f, H / 2f));

            // Draw the image onto the new bitmap rotated.
            using (var gr = Graphics.FromImage(nBMP)) {
                gr.InterpolationMode = InterpolationMode.HighQualityBicubic;
                gr.Clear(Color.Magenta);
                gr.Transform = rotate_at_center;

                // Draw the image centered on the bitmap.
                var x = (B - _Pic.Width) / 2;
                var y = (H - _Pic.Height) / 2;
                gr.DrawImage(_Pic, x, y, _Pic.Width, _Pic.Height);
            }
            OnOverridePic(nBMP);

        }
    }
}