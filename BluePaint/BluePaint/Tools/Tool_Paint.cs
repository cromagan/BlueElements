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

namespace BluePaint
{
    public partial class Tool_Paint
    {

        public Tool_Paint() : base()
        {
            InitializeComponent();
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
                var _Pic = OnNeedCurrentPic();
                _Pic.FillCircle(Color.Black, e.Current.TrimmedX, e.Current.TrimmedY, 2);
                OnDoInvalidate();
            }
            else
            {
                OnDoInvalidate();
            }
        }

        public override void DoAdditionalDrawing(AdditionalDrawing e, Bitmap OriginalPic)
        {
            var c = Color.FromArgb(50, 255, 0, 0);
            e.FillCircle(c, e.Current.TrimmedX, e.Current.TrimmedY, 2);
        }

    }
}