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


namespace BluePaint
{
    public partial class Tool_Bruchlinie
    {



        public Tool_Bruchlinie() : base()
        {
            InitializeComponent();
        }


        private void Bruch_Click(object sender, System.EventArgs e)
        {
            var _Pic = OnNeedCurrentPic();

            if (_Pic == null) { return; }


            var XRi = Convert.ToInt32(_Pic.Width / 10.0);
            var YRI = Convert.ToInt32(_Pic.Height / 10.0);

            var ChangeY = false;
            var ChangeX = false;
            var ModX = 0;
            var ModY = 0;


            OnForceUndoSaving();
            Point Nach;
            switch (((Button)sender).Name.ToLower())
            {
                case "bruch_oben":
                    Nach = new Point(0, 5);
                    YRI = -5;
                    ModY = -5;
                    ChangeY = true;
                    break;

                case "bruch_unten":
                    Nach = new Point(0, _Pic.Height - 6);
                    YRI = 5;
                    ModY = 5;
                    ChangeY = true;
                    break;

                case "bruch_links":
                    Nach = new Point(5, 0);
                    XRi = -5;
                    ModX = -5;
                    ChangeX = true;
                    break;

                case "bruch_rechts":
                    Nach = new Point(_Pic.Width - 6, 0);
                    XRi = 5;
                    ModX = 5;
                    ChangeX = true;
                    break;

                default:
                    return;
            }




            var gr = Graphics.FromImage(_Pic);


            for (var z = 0; z <= 10; z++)
            {
                var von = Nach;

                Nach.X += XRi;
                Nach.Y += YRI;


                for (var x1 = -1; x1 <= 1; x1++)
                {
                    for (var y1 = -1; y1 <= 1; y1++)
                    {
                        gr.DrawLine(new Pen(Color.FromArgb(255, 255, 255), 8), von.X + ModX + x1, von.Y + ModY + y1, Nach.X + ModX + x1, Nach.Y + ModY + y1);
                    }
                }


                gr.DrawLine(new Pen(Color.FromArgb(128, 128, 128)), von, Nach);


                if (ChangeX) { XRi = XRi * -1; }
                if (ChangeY) { YRI = YRI * -1; }
            }

            OnDoInvalidate();
        }
    }
}