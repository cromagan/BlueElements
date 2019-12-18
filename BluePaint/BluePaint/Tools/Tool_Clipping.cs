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

using System;
using System.Drawing;
using BlueBasics;
using static BlueBasics.modAllgemein;
using static BlueBasics.Develop;
using BluePaint.EventArgs;
using BlueBasics.Enums;
using BlueControls.EventArgs;

namespace BluePaint
{
    public partial class Tool_Clipping
    {
        public Tool_Clipping()
        {
            InitializeComponent();
        }

        private Point _MouseDown;
        private bool _AutoCropping = false;

        public override void PicChangedFromMain()
        {
            CheckMinMax();
        }

        public override void ToolFirstShown()
        {
            CheckMinMax();
            AutoZ_Click(null, null);
        }


        public override void MouseDown(MouseEventArgs1_1 e)
        {
            CheckMinMax();
            //_MouseDown = PointInsidePic(e);
            MouseMove(e);
        }

        public override void MouseMove(MouseEventArgs1_1 e)
        {
            if (_Pic == null || _PicPreview == null) { return; }

            var Pen_Blau = new Pen(Color.FromArgb(150, 0, 0, 255));
            ClearPreviewPic();
            //var NP = PointInsidePic(e);
            var gr = Graphics.FromImage(_PicPreview);
            DrawZusatz();

            gr.DrawLine(Pen_Blau, e.TrimmedX, 0, e.TrimmedX, _Pic.Height);
            gr.DrawLine(Pen_Blau, 0, e.TrimmedY, _Pic.Width, e.TrimmedY);

            if (e.Button == System.Windows.Forms.MouseButtons.Left)
            {
                gr.DrawLine(Pen_Blau, _MouseDown.X, 0, _MouseDown.X, _Pic.Height);
                gr.DrawLine(Pen_Blau, 0, _MouseDown.Y, _Pic.Width, _MouseDown.Y);
            }
            OnPicChangedByTool();

        }

        public override void MouseUp(MouseEventArgs1_1 e)
        {
            if (_Pic == null || _PicPreview == null) { return; }
            _AutoCropping = true;
            //   var NP = PointInsidePic(e);

            Links.Value = Math.Min(e.TrimmedX, _MouseDown.X) + 1;
            Recht.Value = -(_Pic.Width - Math.Max(e.TrimmedX, _MouseDown.X));
            Oben.Value = Math.Min(e.TrimmedY, _MouseDown.Y) + 1;
            Unten.Value = -(_Pic.Height - Math.Max(e.TrimmedY, _MouseDown.Y));

            _AutoCropping = false;
            ValueChanged(this, System.EventArgs.Empty);

        }

        private void ValueChanged(object sender, System.EventArgs e)
        {
            if (_AutoCropping) { return; }


            if (_Pic == null) { return; }


            ClearPreviewPic();

            DrawZusatz();

            OnPicChangedByTool();
        }


        public void DrawZusatz()
        {
            var Brush_Blau = new SolidBrush(Color.FromArgb(120, 0, 0, 255));
            var gr = Graphics.FromImage(_PicPreview);


            if (Links.Value != 0)
            {
                gr.FillRectangle(Brush_Blau, new Rectangle(0, 0, Convert.ToInt32(Links.Value), _PicPreview.Height));
            }
            if (Recht.Value != 0)
            {
                gr.FillRectangle(Brush_Blau, new Rectangle(_PicPreview.Width + Convert.ToInt32(Recht.Value), 0, (int)-Recht.Value, _PicPreview.Height));
            }

            if (Oben.Value != 0)
            {
                gr.FillRectangle(Brush_Blau, new Rectangle(0, 0, _PicPreview.Width, Convert.ToInt32(Oben.Value)));
            }
            if (Unten.Value != 0)
            {
                gr.FillRectangle(Brush_Blau, new Rectangle(0, _PicPreview.Height + Convert.ToInt32(Unten.Value), _PicPreview.Width, (int)-Unten.Value));
            }


        }


        private void ZuschnittOK_Click(object sender, System.EventArgs e)
        {
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

        private void AutoZ_Click(object sender, System.EventArgs e)
        {
            _AutoCropping = true;
            OnZoomFit();

            var pa = _Pic.GetAutoValuesForCrop(0.9);

            Links.Value = pa.Left;
            Recht.Value = pa.Right;
            Oben.Value = pa.Top;
            Unten.Value = pa.Bottom;

            _AutoCropping = false;

            ValueChanged(this, System.EventArgs.Empty);
        }

        public void Set(int Left, int Top, int Right, int Bottom)
        {

            if (Left < 0 || Top < 0 || Right > 0 || Bottom > 0)
            {
                DebugPrint(enFehlerArt.Warnung, "Fehler in den Angaben");
            }
            CheckMinMax();


            Links.Value = Left;
            Oben.Value = Top;
            Recht.Value = Right;
            Unten.Value = Bottom;
        }

        public void CheckMinMax()
        {
            if (_Pic == null) { return; }
            Links.Maximum = _Pic.Width - 1;
            Recht.Minimum = -_Pic.Width + 1;
            Oben.Maximum = _Pic.Height - 1;
            Unten.Minimum = -_Pic.Height - 1;
        }

    }

}