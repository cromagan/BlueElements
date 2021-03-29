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
using BlueControls.EventArgs;
using System.Drawing;
using static BlueBasics.Extensions;

namespace BluePaint
{
    public partial class Tool_Eraser : GenericTool
    {
        public Tool_Eraser(bool Aufnahme) : base()
        {
            InitializeComponent();


            DrawBox.Enabled = !Aufnahme;
            Razi.Enabled = !Aufnahme;

            if (Aufnahme)
            {
                Eleminate.Checked = true;
            }

        }


        public override void DoAdditionalDrawing(AdditionalDrawing e, Bitmap OriginalPic)
        {
            if (Razi.Checked)
            {
                e.FillCircle(ColorRedTransp, e.Current.TrimmedX, e.Current.TrimmedY, 3);
            }


            if (DrawBox.Checked && e.Current != null)
            {
                var _Pic = OnNeedCurrentPic();

                e.DrawLine(Pen_RedTransp, -1, e.Current.TrimmedY, _Pic.Width, e.Current.TrimmedY);
                e.DrawLine(Pen_RedTransp, e.Current.TrimmedX, -1, e.Current.TrimmedX, _Pic.Height);

                if (e.Current.Button == System.Windows.Forms.MouseButtons.Left && e.MouseDown != null)
                {
                    e.DrawLine(Pen_RedTransp, -1, e.MouseDown.TrimmedY, _Pic.Width, e.MouseDown.TrimmedY);
                    e.DrawLine(Pen_RedTransp, e.MouseDown.TrimmedX, -1, e.MouseDown.TrimmedX, _Pic.Height);
                    e.FillRectangle(Brush_RedTransp, e.TrimmedRectangle());
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

            if (Razi.Checked)
            { return; }

            var _Pic = OnNeedCurrentPic();


            if (Eleminate.Checked)
            {

                if (e.Current.IsInPic)
                {
                    var cc = _Pic.GetPixel(e.Current.X, e.Current.Y);

                    if (cc.ToArgb() == 0)
                    { return; }

                    OnCommandForMacro("Replace;" + cc.ToArgb());

                    OnOverridePic(OriginalPic.ReplaceColor(cc, Color.Transparent));
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

        public override string MacroKennung()
        {
            return "Eraser";
        }

        public override void ExcuteCommand(string command)
        {
            var c = command.SplitBy(";");

            if (c[0] == "Replace")
            {
                var OriginalPic = OnNeedCurrentPic();
                var cc = Color.FromArgb(int.Parse(c[1]));
                OnOverridePic(OriginalPic.ReplaceColor(cc, Color.Transparent));
            }
            else
            {
                Develop.DebugPrint_NichtImplementiert();
            }
        }

    }

}