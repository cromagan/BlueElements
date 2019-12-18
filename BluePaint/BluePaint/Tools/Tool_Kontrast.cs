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
using System.Drawing;
using static BlueBasics.Extensions;

namespace BluePaint
{
    public partial class Tool_Kontrast : GenericTool //  System.Windows.Forms.UserControl //
    {

        public Tool_Kontrast()
        {
            InitializeComponent();
        }


        private void btnKontrastErhoehen_Click(object sender, System.EventArgs e)
        {
            if (_Pic == null) { return; }


            OnOverridePic(_Pic.AdjustContrast((float)sldKontrast.Value));

            sldKontrast.Value = 1f;

            //OnForceUndoSaving();

            //var ca = new Color();
            //var cn = new Color();

            //for (var x = 0; x < _Pic.Width; x++)
            //{
            //    for (var y = 0; y < _Pic.Height; y++)
            //    {
            //        ca = _Pic.GetPixel(x, y);
            //        if (ca.ToArgb() != Color.White.ToArgb())
            //        {
            //            cn = FromHSB(ca.GetHue(), (float)(ca.GetSaturation() * 1.2), (float)(ca.GetBrightness() * 0.9), ca.A);
            //            _Pic.SetPixel(x, y, cn);
            //        }

            //    }
            //}

            //q
            //OnPicChangedByTool();
        }

        private void btnGraustufen_Click(object sender, System.EventArgs e)
        {
            if (_Pic == null) { return; }

            OnOverridePic(_Pic.Grayscale());
            sldKontrast.Value = 1f;
        }

        private void btnAlleFarbenSchwarz_Click(object sender, System.EventArgs e)
        {
            if (_Pic == null) { return; }
            OnForceUndoSaving();

            _Pic.AllePixelZuSchwarz(1f);

            sldKontrast.Value = 1f;

            OnPicChangedByTool();
        }

        private void btnPixelHinzu_Click(object sender, System.EventArgs e)
        {
            if (_Pic == null) { return; }
            OnForceUndoSaving();

            for (var x = 0; x < _Pic.Width - 1; x++)
            {
                for (var y = 0; y < _Pic.Height - 1; y++)
                {
                    if (!_Pic.GetPixel(x + 1, y + 1).IsNearWhite(0.9)) { _Pic.SetPixel(x, y, Color.Black); }
                    if (!_Pic.GetPixel(x + 1, y).IsNearWhite(0.9)) { _Pic.SetPixel(x, y, Color.Black); }
                    if (!_Pic.GetPixel(x, y + 1).IsNearWhite(0.9)) { _Pic.SetPixel(x, y, Color.Black); }
                }
            }

            sldKontrast.Value = 1f;

            OnPicChangedByTool();

        }

        private void btnAusdünnen_Click(object sender, System.EventArgs e)
        {

            if (_Pic == null) { return; }
            OnForceUndoSaving();

            _Pic.Ausdünnen(4);

            sldKontrast.Value = 1f;

            ClearPreviewPic();

            OnPicChangedByTool();
            return;
        }

        private void sldKontrast_ValueChanged(object sender, System.EventArgs e)
        {
            capKontrast.Text = sldKontrast.Value.ToString();


            if (sldKontrast.Value == 1)
            {
                ClearPreviewPic();
            }

            else
            {
                _PicPreview = _Pic.AdjustContrast((float)sldKontrast.Value);

            }


            OnPicChangedByTool();
        }
    }

}