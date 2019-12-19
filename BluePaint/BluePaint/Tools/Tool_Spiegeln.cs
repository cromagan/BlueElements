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

using BluePaint.EventArgs;
using System.Drawing;
using System.Drawing.Drawing2D;
using static BlueBasics.modAllgemein;
using static BlueBasics.Extensions;
using BlueControls.EventArgs;
using BlueControls;
using BlueBasics;

namespace BluePaint
{
    public partial class Tool_Spiegeln : GenericTool // System.Windows.Forms.UserControl // 
    {

        private bool _ausricht = false;

        public Tool_Spiegeln()
        {
            InitializeComponent();
        }

        private void SpiegelnH_Click(object sender, System.EventArgs e)
        {
            _ausricht = false;
            var _Pic = OnNeedCurrentPic();
            if (_Pic == null) { return; }
            CollectGarbage();
            _Pic.RotateFlip(RotateFlipType.RotateNoneFlipY);
            OnOverridePic(_Pic);
            OnZoomFit();
        }

        private void SpiegelnV_Click(object sender, System.EventArgs e)
        {
            _ausricht = false;
            var _Pic = OnNeedCurrentPic();
            if (_Pic == null) { return; }
            CollectGarbage();
            _Pic.RotateFlip(RotateFlipType.RotateNoneFlipX);
            OnOverridePic(_Pic);
            OnZoomFit();
        }

        private void btnDrehenR_Click(object sender, System.EventArgs e)
        {
            _ausricht = false;
            var _Pic = OnNeedCurrentPic();
            if (_Pic == null) { return; }
            CollectGarbage();
            _Pic.RotateFlip(RotateFlipType.Rotate90FlipNone);
            OnOverridePic(_Pic);
            OnZoomFit();

        }

        private void btnDrehenL_Click(object sender, System.EventArgs e)
        {
            _ausricht = false;
            var _Pic = OnNeedCurrentPic();
            if (_Pic == null) { return; }
            CollectGarbage();
            _Pic.RotateFlip(RotateFlipType.Rotate270FlipNone);
            OnOverridePic(_Pic);
            OnZoomFit();

        }

        private void btnAusrichten_Click(object sender, System.EventArgs e)
        {

            _ausricht = true;
            OnDoInvalidate();
        }

        public override void DoAdditionalDrawing(AdditionalDrawing e, Bitmap OriginalPic)
        {
            if (!_ausricht) { return; }


            var _Pic = OnNeedCurrentPic();

            e.DrawLine(Pen_RedTransp, 0, e.Current.TrimmedY, _Pic.Width, e.Current.TrimmedY);
            e.DrawLine(Pen_RedTransp, e.Current.TrimmedX, 0, e.Current.TrimmedX, _Pic.Height);

            if (e.Current.Button == System.Windows.Forms.MouseButtons.Left && e.MouseDown != null)
            {
                e.DrawLine(Pen_RedTransp, 0, e.MouseDown.TrimmedY, _Pic.Width, e.MouseDown.TrimmedY);
                e.DrawLine(Pen_RedTransp, e.MouseDown.TrimmedX, 0, e.MouseDown.TrimmedX, _Pic.Height);

                e.DrawLine(Pen_LightWhite, e.Current.TrimmedX, e.Current.TrimmedY, e.MouseDown.TrimmedX, e.MouseDown.TrimmedY);
                e.DrawLine(Pen_RedTransp, e.Current.TrimmedX, e.Current.TrimmedY, e.MouseDown.TrimmedX, e.MouseDown.TrimmedY);

            }
        }


        public override void MouseDown(BlueControls.EventArgs.MouseEventArgs1_1 e, Bitmap OriginalPic)
        {
            if (!_ausricht) { return; }
            MouseMove(new MouseEventArgs1_1DownAndCurrent(e, e), OriginalPic);
        }

        public override void MouseMove(BlueControls.EventArgs.MouseEventArgs1_1DownAndCurrent e, Bitmap OriginalPic)
        {
            if (!_ausricht) { return; }
            OnDoInvalidate();
        }

        public override void MouseUp(BlueControls.EventArgs.MouseEventArgs1_1DownAndCurrent e, Bitmap OriginalPic)
        {

            if (!_ausricht) { return; }

            _ausricht = false;

            var _Pic = OnNeedCurrentPic();

            var _middle = new PointDF(_Pic.Width / 2, _Pic.Height / 2);
            var Dist = GeometryDF.Winkel(new PointDF(e.Current.X, e.Current.Y), new PointDF(e.MouseDown.X, e.MouseDown.Y));


            var Lang = GeometryDF.Länge(_middle, new PointDF(0, 0)); // Länge vom Eck zur Mitte;
            var Wink_LO = GeometryDF.Länge(_middle, new PointDF(0, 0)); // Winkel: Mitte bis LO
            var Wink_RO = GeometryDF.Länge(_middle, new PointDF(_Pic.Width, 0)); // Winkel: Mitte bis RO
            var Wink_LU = GeometryDF.Länge(_middle, new PointDF(0, _Pic.Height)); // Winkel: Mitte bis LU
            var Wink_RU = GeometryDF.Länge(_middle, new PointDF(_Pic.Width, _Pic.Height)); // Winkel: Mitte bis RU

            var P_LO = new PointDF(_middle, Lang, Wink_LO + Dist);
            var P_RO = new PointDF(_middle, Lang, Wink_RO + Dist);
            var P_RU = new PointDF(_middle, Lang, Wink_RU + Dist);
            var P_LU = new PointDF(_middle, Lang, Wink_LU + Dist);


            var maxX = int.MinValue;
            var maxY = int.MinValue;
            var minX = int.MaxValue;
            var minY = int.MaxValue;

            Develop.DebugPrint_NichtImplementiert();
            OnDoInvalidate();

            //if (Eleminate.Checked)
            //{

            //    if (e.Current.IsInPic)
            //    {
            //        var cc = _Pic.GetPixel(e.Current.X, e.Current.Y).ToArgb();

            //        if (cc == -1) { return; }

            //        for (var x = 0; x < _Pic.Width; x++)
            //        {
            //            for (var y = 0; y < _Pic.Height; y++)
            //            {
            //                if (_Pic.GetPixel(x, y).ToArgb() == cc)
            //                {
            //                    _Pic.SetPixel(x, y, Color.White);
            //                }
            //            }
            //        }

            //        OnDoInvalidate();
            //        return;
            //    }
            //}


            //if (DrawBox.Checked)
            //{
            //    var g = Graphics.FromImage(_Pic);
            //    g.FillRectangle(Brushes.White, e.TrimmedRectangle());
            //    OnDoInvalidate();
            //}
        }
    }
}