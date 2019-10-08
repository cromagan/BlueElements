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
using BlueControls.Controls;
using BlueControls.EventArgs;

namespace BluePaint
{
    public partial class Tool_Eraser : GenericTool
    {
        private Point _MouseDown;

        public Tool_Eraser()
        {
            InitializeComponent();
        }

        public override void PicChangedFromMain()
        {

        }

        public override void ToolFirstShown()
        {

        }

        public override void MouseDown(MouseEventArgs1_1 e)
        {
            OnForceUndoSaving();

            ClearPreviewPic();
            _MouseDown = new Point(e.TrimmedX, e.TrimmedY);


            if (Razi.Checked)
            {
                MouseMove(e);
            }

        }

        public override void MouseMove(MouseEventArgs1_1 e)
        {
            var Brush_RotTransp = new SolidBrush(Color.FromArgb(128, 255, 0, 0));
            var Pen_RotTransp = new Pen(Color.FromArgb(50, 255, 0, 0));
            //var NP = PointInsidePic(e);


            if (e.Button == System.Windows.Forms.MouseButtons.Left)
            {

                if (Razi.Checked)
                {
                    var gr = Graphics.FromImage(_Pic);
                    var r = new Rectangle(e.TrimmedX - 4, e.TrimmedY - 4, 9, 9);
                    gr.FillEllipse(Brushes.White, r);
                }

                if (DrawBox.Checked)
                {
                    ClearPreviewPic();
                    var gr = Graphics.FromImage(_PicPreview);
                    var r = new Rectangle(Math.Min(_MouseDown.X, e.TrimmedX), Math.Min(_MouseDown.Y, e.TrimmedY), Math.Abs(_MouseDown.X - e.TrimmedX) + 1, Math.Abs(_MouseDown.Y - e.TrimmedY) + 1);
                    gr.FillRectangle(Brush_RotTransp, r);
                }

            }

            if (Razi.Checked)
            {
                ClearPreviewPic();
                var gr = Graphics.FromImage(_PicPreview);
                var r = new Rectangle(e.TrimmedX - 4, e.TrimmedY - 4, 9, 9);
                gr.FillEllipse(Brush_RotTransp, r);
            }


            if (DrawBox.Checked && e.Button == System.Windows.Forms.MouseButtons.None)
            {

                ClearPreviewPic();
                var gr = Graphics.FromImage(_PicPreview);

                gr.DrawLine(Pen_RotTransp, e.TrimmedX, 0, e.TrimmedX, _Pic.Height);
                gr.DrawLine(Pen_RotTransp, 0, e.TrimmedY, _Pic.Width, e.TrimmedY);
            }

            OnPicChangedByTool();
        }

        public override void MouseUp(MouseEventArgs1_1 e)
        {

            //var NP = PointInsidePic(e);


            if (Eleminate.Checked)
            {

                if (e.IsInPic)
                {
                    var cc = _Pic.GetPixel(e.X, e.Y).ToArgb();

                    if (cc == -1)
                    {
                        return;
                    }

                    for (var x = 0; x < _Pic.Width; x++)
                    {
                        for (var y = 0; y < _Pic.Height; y++)
                        {
                            if (_Pic.GetPixel(x, y).ToArgb() == cc)
                            {
                                _Pic.SetPixel(x, y, Color.White);
                            }
                        }
                    }

                    OnPicChangedByTool();

                }


            }


            if (DrawBox.Checked)
            {
                ClearPreviewPic();
                using (var gr = Graphics.FromImage(_Pic))
                {

                    var r = new Rectangle(Math.Min(_MouseDown.X, e.TrimmedX), Math.Min(_MouseDown.Y, e.TrimmedY), Math.Abs(_MouseDown.X - e.TrimmedX) + 1, Math.Abs(_MouseDown.Y - e.TrimmedY) + 1);
                    gr.FillRectangle(Brushes.White, r);
                }

                OnPicChangedByTool();
            }
        }

        private void DrawBox_CheckedChanged(object sender, System.EventArgs e)
        {

            if (((Button)sender).Checked)
            {
                ClearPreviewPic();
                OnPicChangedByTool();
            }


        }
    }

}