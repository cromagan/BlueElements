﻿#region BlueElements - a collection of useful tools, database and controls
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
    public partial class Tool_Kontrast :   GenericTool //System.Windows.Forms.UserControl //
    {

        public Tool_Kontrast()
        {
            InitializeComponent();
        }

        public override void DoAdditionalDrawing(AdditionalDrawing e, Bitmap OriginalPic)
        {

           if( OriginalPic == null) { return; }

            if (sldKontrast.Value != 0)
            {
                var _PicPreview = OriginalPic.AdjustContrast((float)sldKontrast.Value);
                e.DrawImage(_PicPreview);
                return;
            }

            if (sldGamma.Value != 1)
            {
                var _PicPreview = OriginalPic.AdjustGamma((float)sldGamma.Value);
                e.DrawImage(_PicPreview);
                return;
            }

            if (sldHelligkeit.Value != 1)
            {
                var _PicPreview = OriginalPic.AdjustBrightness((float)sldHelligkeit.Value);
                e.DrawImage(_PicPreview);
                return;
            }

        }


        private void btnKontrastErhoehen_Click(object sender, System.EventArgs e)
        {
            var _Pic = OnNeedCurrentPic();
            if (_Pic == null) { return; }
            OnOverridePic(_Pic.AdjustContrast((float)sldKontrast.Value));
            sldGamma.Value = 1f;
            sldKontrast.Value = 0f;
            sldHelligkeit.Value = 1f;
        }

        private void btnGraustufen_Click(object sender, System.EventArgs e)
        {
            var _Pic = OnNeedCurrentPic();
            if (_Pic == null) { return; }

            OnOverridePic(_Pic.Grayscale());
            sldGamma.Value = 1f;
            sldKontrast.Value = 0f;
            sldHelligkeit.Value = 1f;
        }

        private void btnAlleFarbenSchwarz_Click(object sender, System.EventArgs e)
        {
            var _Pic = OnNeedCurrentPic();
            if (_Pic == null) { return; }
            OnForceUndoSaving();

            _Pic.AllePixelZuSchwarz(1f);

            sldGamma.Value = 1f;
            sldKontrast.Value = 0f;
            sldHelligkeit.Value = 1f;

            OnDoInvalidate();
        }

        private void btnPixelHinzu_Click(object sender, System.EventArgs e)
        {
            var _Pic = OnNeedCurrentPic();
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

            sldGamma.Value = 1f;
            sldKontrast.Value = 0f;
            sldHelligkeit.Value = 1f;

            OnDoInvalidate();

        }

        private void btnAusdünnen_Click(object sender, System.EventArgs e)
        {
            var _Pic = OnNeedCurrentPic();
            if (_Pic == null) { return; }
            OnForceUndoSaving();

            _Pic.Ausdünnen(4);

            sldGamma.Value = 1f;
            sldKontrast.Value = 0f;
            sldHelligkeit.Value = 1f;

            OnDoInvalidate();
            return;
        }





        private void btnHelligkeit_Click(object sender, System.EventArgs e)
        {
            var _Pic = OnNeedCurrentPic();
            if (_Pic == null) { return; }
            OnOverridePic(_Pic.AdjustBrightness((float)sldHelligkeit.Value));
            sldGamma.Value = 1f;
            sldKontrast.Value = 0f;
            sldHelligkeit.Value = 1f;
        }

        private void btnGamma_Click(object sender, System.EventArgs e)
        {
            var _Pic = OnNeedCurrentPic();
            if (_Pic == null) { return; }
            OnOverridePic(_Pic.AdjustGamma((float)sldGamma.Value));
            sldGamma.Value = 1f;
            sldKontrast.Value = 0f;
            sldHelligkeit.Value = 1f;

        }

        private void sldHelligkeit_ValueChanged(object sender, System.EventArgs e)
        {
            sldGamma.Value = 1f;
            sldKontrast.Value = 0f;
            //sldHelligkeit.Value = 0f;

            capHelligkeit.Text = sldHelligkeit.Value.ToString();
            OnDoInvalidate();

        }


        private void sldKontrast_ValueChanged(object sender, System.EventArgs e)
        {
            sldGamma.Value = 1f;
            //sldKontrast.Value = 0f;
            sldHelligkeit.Value = 1f;

            capKontrast.Text = sldKontrast.Value.ToString();
            OnDoInvalidate();
        }

        private void sldGamma_ValueChanged(object sender, System.EventArgs e)
        {
            //sldGamma.Value = 1f;
            sldKontrast.Value = 0f;
            sldHelligkeit.Value = 1f;

            capGamma.Text = sldGamma.Value.ToString();
            OnDoInvalidate();
        }

        public override string MacroKennung()
        {
            return "Kontrast";
        }


    }

}