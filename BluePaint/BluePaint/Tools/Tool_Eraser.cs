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
using static BlueBasics.Extensions;

namespace BluePaint
{
    public partial class Tool_Eraser : GenericTool
    {
        public Tool_Eraser()
        {
            InitializeComponent();
        }


        public override void DoAdditionalDrawing(AdditionalDrawing e, Bitmap OriginalPic)
        {
            var c = Color.FromArgb(50, 255, 0, 0);
            var Brush_RotTransp = new SolidBrush(Color.FromArgb(128, 255, 0, 0));
            var Pen_RotTransp = new Pen(c);
            //e.G.DrawString("OK" + DateTime.Now.Millisecond.ToString() , new Font("Arial", 20), new SolidBrush(Color.Red), new Point(0, 0));
            //var Pen_RotTransp2 = new Pen(Color.FromArgb(50, 0, 255, 0));

            if (Razi.Checked)
            {
                e.FillCircle(c, e.Current.TrimmedX, e.Current.TrimmedY, 3);
            }


            if (DrawBox.Checked)
            {
                var _Pic = OnNeedCurrentPic();

                e.DrawLine(Pen_RotTransp, 0, e.Current.TrimmedY, _Pic.Width, e.Current.TrimmedY);
                e.DrawLine(Pen_RotTransp, e.Current.TrimmedX, 0, e.Current.TrimmedX, _Pic.Height);

                if (e.Current.Button == System.Windows.Forms.MouseButtons.Left && e.MouseDown != null)
                {
                    e.DrawLine(Pen_RotTransp, 0, e.MouseDown.TrimmedY, _Pic.Width, e.MouseDown.TrimmedY);
                    e.DrawLine(Pen_RotTransp, e.MouseDown.TrimmedX, 0, e.MouseDown.TrimmedX, _Pic.Height);
                    e.FillRectangle(Brush_RotTransp, e.TrimmedRectangle());
                }
            }


        }




        public override void MouseDown(BlueControls.EventArgs.MouseEventArgs1_1 e, Bitmap OriginalPic)
        {
            OnForceUndoSaving();
            MouseMove(new MouseEventArgs1_1DownAndCurrent(e, e), OriginalPic);
        }

        public override void MouseMove(BlueControls.EventArgs.MouseEventArgs1_1DownAndCurrent e, Bitmap OriginalPic)
        {

            if (e.Current.Button == System.Windows.Forms.MouseButtons.Left)
            {

                if (Razi.Checked)
                {
                    var _Pic = OnNeedCurrentPic();
                    _Pic.FillCircle(Color.White, e.Current.TrimmedX, e.Current.TrimmedY, 3);
                }
            }
                OnDoInvalidate();

        }

        public override void MouseUp(BlueControls.EventArgs.MouseEventArgs1_1DownAndCurrent e, Bitmap OriginalPic)
        {

            if (Razi.Checked) { return; }

            var _Pic = OnNeedCurrentPic();


            if (Eleminate.Checked)
            {

                if (e.Current.IsInPic)
                {
                    var cc = _Pic.GetPixel(e.Current.X, e.Current.Y).ToArgb();

                    if (cc == -1) { return; }

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

                    OnDoInvalidate();
                    return;
                }
            }


            if (DrawBox.Checked)
            {
                var g = Graphics.FromImage(_Pic);
                g.FillRectangle(Brushes.White, e.TrimmedRectangle());
                OnDoInvalidate();
            }
        }

        private void DrawBox_CheckedChanged(object sender, System.EventArgs e)
        {
            OnDoInvalidate();
        }
    }

}