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
using BlueBasics.Enums;
using static BlueBasics.Extensions;

namespace BluePaint
{

    public partial class Tool_Resize : GenericTool //BlueControls.Forms.Form //
    {
        public Tool_Resize() : base()
        {
            InitializeComponent();
            capInfo.Text = "Bitte Skalierung in Prozent eingeben";

            flxProzent.ValueSet("100", true, false);


        }

        private void DoCapInfo()
        {
            var p = OnNeedCurrentPic();

            if (p == null)
            {
                capInfo.Text = "Kein Bild gewählt.";
                return;
            }




            if (!double.TryParse(flxProzent.Value, out var pr))
            {
                capInfo.Text = "Keine Prozentzahl angegeben.";
                return;
            }

            pr /= 100;

            var wi = (int)(p.Width * pr);
            var he = (int)(p.Height * pr);

            if (pr == 1 || pr < 0.01 || pr > 1000 || wi < 1 || he < 1)
            {
                capInfo.Text = "Bitte gültigen Wert angeben.";
                return;
            }



            capInfo.Text = "Zielgröße: " + (int)(p.Width * pr) + " x " + (int)(p.Height * pr) + " Pixel";



        }

        private void btnDoResize_Click(object sender, System.EventArgs e)
        {
            var p = OnNeedCurrentPic();
            if (p == null)
            { return; }
            if (!double.TryParse(flxProzent.Value, out var pr))
            { return; }
            pr /= 100;

            var wi = (int)(p.Width * pr);
            var he = (int)(p.Height * pr);

            if (pr == 1 || pr < 0.01 || pr > 1000 || wi < 1 || he < 1)
            { return; }

            var _BMP2 = BitmapExt.Resize(p, wi, he, enSizeModes.Verzerren, System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic, true);

            OnOverridePic(_BMP2);

            OnZoomFit();

            OnCommandForMacro("ResizeProzent;" + flxProzent.Value);
            DoCapInfo();

        }



        public override string MacroKennung()
        {
            return "Resize";
        }

        public override void ExcuteCommand(string command)
        {
            var c = command.SplitBy(";");

            if (c[0] == "ResizeProzent")
            {
                flxProzent.ValueSet(c[1], true, true);
                btnDoResize_Click(null, null);
            }
            else
            {
                Develop.DebugPrint_NichtImplementiert();
            }
        }

        private void flxProzent_ValueChanged(object sender, System.EventArgs e)
        {
            DoCapInfo();
        }

        public override void PictureChangedByMainWindow()
        {
            base.PictureChangedByMainWindow();
            DoCapInfo();
        }

    }

}