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
using BluePaint.EventArgs;

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

        public override void MouseDown(System.Windows.Forms.MouseEventArgs e)
        {
            OnForceUndoSaving();

            ClearPreviewPic();
            _MouseDown = PointInsidePic(e);


            if (Razi.Checked)
            {
                MouseMove(e);
            }

        }

        public override void MouseMove(System.Windows.Forms.MouseEventArgs e)
        {
            var Brush_RotTransp = new SolidBrush(Color.FromArgb(128, 255, 0, 0));
            var Pen_RotTransp = new Pen(Color.FromArgb(50, 255, 0, 0));
            var NP = PointInsidePic(e);
            ClearPreviewPic();

            if (e.Button == System.Windows.Forms.MouseButtons.Left)
            {

                if (Razi.Checked)
                {
                    var gr = Graphics.FromImage(_Pic);
                    var r = new Rectangle(NP.X - 4, NP.Y - 4, 9, 9);
                    gr.FillEllipse(Brushes.White, r);
                }

                if (DrawBox.Checked)
                {

                    OnSetHelper(new SetHelperEventArgs(BlueBasics.Enums.enOrientation.Ohne, BlueControls.Enums.enHelpers.FilledRectancle));
                }

            }

            if (Razi.Checked)
            {
                OnSetHelper(new SetHelperEventArgs(BlueBasics.Enums.enOrientation.Ohne, BlueControls.Enums.enHelpers.Ohne));
                var gr = Graphics.FromImage(_PicPreview);
                var r = new Rectangle(NP.X - 4, NP.Y - 4, 9, 9);
                gr.FillEllipse(Brush_RotTransp, r);
            }


            if (DrawBox.Checked && e.Button == System.Windows.Forms.MouseButtons.None)
            {
                OnSetHelper(new SetHelperEventArgs(BlueBasics.Enums.enOrientation.Ohne, BlueControls.Enums.enHelpers.HorizontalVerticalLine ));
            }

            OnPicChangedByTool();
        }

        public override void MouseUp(System.Windows.Forms.MouseEventArgs e)
        {

            var NP = PointInsidePic(e);


            if (Eleminate.Checked)
            {

                if (IsInsidePic(e))
                {
                    var cc = _Pic.GetPixel(e.X, e.Y).ToArgb();

                    if (cc == -1)
                    {
                        return;
                    }

                    for (var x = 0 ; x < _Pic.Width ; x++)
                    {
                        for (var y = 0 ; y < _Pic.Height ; y++)
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

                    var r = new Rectangle(Math.Min(_MouseDown.X, NP.X), Math.Min(_MouseDown.Y, NP.Y), Math.Abs(_MouseDown.X - NP.X) + 1, Math.Abs(_MouseDown.Y - NP.Y) + 1);
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