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

using System.Drawing;

namespace BluePaint
{
    public partial class Tool_Paint
    {

        public Tool_Paint()
        {
            InitializeComponent();
        }


        public override void MouseDown(System.Windows.Forms.MouseEventArgs e)
        {
            OnForceUndoSaving();
            ClearPreviewPic();
            MouseMove(e);
        }

        public override void MouseMove(System.Windows.Forms.MouseEventArgs e)
        {
            var Brush_RotTransp = new SolidBrush(Color.FromArgb(128, 255, 0, 0));

            if (e.Button == System.Windows.Forms. MouseButtons.Left)
            {

                if (IsInsidePic(e))
                {
                    var gr = Graphics.FromImage(_Pic);
                    var r = new Rectangle(e.X - 1, e.Y - 1, 3, 3);
                    gr.FillEllipse(Brushes.Black, r);
                    OnPicChangedByTool();
                }

            }
            else
            {

                if (IsInsidePic(e))
                {
                    ClearPreviewPic();
                    var gr = Graphics.FromImage(_PicPreview);
                    var r = new Rectangle(e.X - 1, e.Y - 1, 3, 3);
                    gr.FillEllipse(Brush_RotTransp, r);
                }

                OnPicChangedByTool();
            }

        }
    }
}