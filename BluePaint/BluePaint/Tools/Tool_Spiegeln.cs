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

namespace BluePaint
{
    public partial class Tool_Spiegeln
    {

        public Tool_Spiegeln()
        {
            InitializeComponent();
        }

        private void SpiegelnH_Click(object sender, System.EventArgs e)
        {
            var _Pic = OnNeedCurrentPic();

            if (_Pic == null ) { return; }

            CollectGarbage();
            var ni = new Bitmap(_Pic.Width, _Pic.Height);
            var gr = Graphics.FromImage(ni);
            gr.PixelOffsetMode = PixelOffsetMode.Half;
            gr.DrawImage(_Pic.Image_Clone(), 0, ni.Height, ni.Width, -ni.Height);
            gr.Dispose();

            OnOverridePic(ni);
        }

        private void SpiegelnV_Click(object sender, System.EventArgs e)
        {
            var _Pic = OnNeedCurrentPic();
            if (_Pic == null) { return; }
            CollectGarbage();
            var ni = new Bitmap(_Pic.Width, _Pic.Height);
            var gr = Graphics.FromImage(ni);
            gr.PixelOffsetMode = PixelOffsetMode.Half;
            gr.DrawImage(_Pic.Image_Clone(), ni.Width, 0, -ni.Width, ni.Height);
            gr.Dispose();

            OnOverridePic(ni);
        }
    }

}