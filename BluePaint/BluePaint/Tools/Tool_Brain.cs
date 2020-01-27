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
using BlueControls;
using System.Drawing;
using BlueBrain;
using BlueBasics;
using static BlueBasics.modAllgemein;
using static BlueBasics.FileOperations;
using System.IO;
using System;

namespace BluePaint
{
    public partial class Tool_Brain
    {

        BlueBrain.FullyConnectedNetwork Br = null;


        string f = string.Empty;

        public Tool_Brain()
        {
            InitializeComponent();


            f = System.Windows.Forms.Application.StartupPath + "\\SingleLine.brn";

            if (FileExists(f))
            {
                Br = new FullyConnectedNetwork(f);
            }
            else
            {

                var inp = new ListExt<string> {  "-2,-2", "-1,-2", "0,-2", "1,-2", "2,-2",
                                        "-2,-1", "-1,-1", "0,-1", "1,-1", "2,-1",
                                        "-2,0", "-1,0",  "1,0", "2,0",
                                        "-2,1", "-1,1", "0,1", "1,1", "2,1",
                                        "-2,2", "-1,2", "0,2", "1,2", "2,2"};
                var outp = new ListExt<string> { "0", "1", "2" }; // Transparent, Black, Color

                Br = new BlueBrain.FullyConnectedNetwork(inp, outp, 25);
                Br.Save(f);
            }




        }

        private void btnLernen_Click(object sender, System.EventArgs e)
        {
            OnForceUndoSaving();

            var All = Directory.GetFiles(txtPath.Text, "*.png", SearchOption.TopDirectoryOnly);

            var ziel = 0;


            foreach (var thisf in All)
            {
                var P = (Bitmap)Image_FromFile(thisf);
                OnOverridePic(P);

                for (var x = 0; x < P.Width; x++)
                {
                    for (var y = 0; y < P.Height; y++)
                    {
                        ziel = GetPixel(P, x, y);
                        FillNetwork(P, x, y);
                        Br.BackPropagationGradiant(ziel.ToString(), 1, true);

                    }
                }







            }

        }

        private void FillNetwork(Bitmap p, int x, int y)
        {

            for (var px = -2; px <= 2; px++)
            {
                for (var py = -2; py <= 2; py++)
                {
                    if (px != 0 || py != 0)
                    {
                        var s = px.ToString() + "," + py.ToString();
                        var C = GetPixel(p, x + px, y + py);
                        Br.InputLayer.SetValue(s, C);

                    }
                }
            }
        }

        private int GetPixel(Bitmap p, int x, int y)
        {

            if (x < 0 || y < 0 || x >= p.Width || y >= p.Height) { return 0; }

            var c = p.GetPixel(x, y);

            if (c.A < 128) { return 0; }

            if (c.GetBrightness() < 0.1) { return 1; }

            return 2;


        }

        private void btnAnwenden_Click(object sender, System.EventArgs e)
        {
            OnForceUndoSaving();
        }
    }
}