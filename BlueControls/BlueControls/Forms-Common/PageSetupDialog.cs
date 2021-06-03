#region BlueElements - a collection of useful tools, database and controls
// Authors: 
// Christian Peter
// 
// Copyright (c) 2021 Christian Peter
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
using BlueControls.Controls;
using BlueControls.EventArgs;
using System;
using System.Drawing;
using System.Drawing.Printing;

namespace BlueControls.Forms {
    public partial class PageSetupDialog : DialogWithOkAndCancel {
        private bool Doing;

        private readonly PrintDocument OriD;
        private PrintDocument GiveBack = null;

        public static PrintDocument Show(PrintDocument _PrintDocument1, bool NurHochformat) {
            var MB = new PageSetupDialog(_PrintDocument1, NurHochformat);
            MB.ShowDialog();

            return MB.GiveBack;
        }

        private PageSetupDialog(PrintDocument _PrintDocument1, bool NurHochformat) : base() {

            // Dieser Aufruf ist für den Designer erforderlich.
            InitializeComponent();

            // Fügen Sie Initialisierungen nach dem InitializeComponent()-Aufruf hinzu.

            Doing = true;

            // _NurHoch = NurHochformat

            OriD = _PrintDocument1;

            Format.Item.Clear();

            foreach (PaperSize ps in _PrintDocument1.PrinterSettings.PaperSizes) {
                var nn = ps.Width + ";" + ps.Height;
                if (Format.Item[nn] == null) {
                    Format.Item.Add(ps.PaperName, nn, QuickImage.Get(enImageCode.Datei), true, ps.PaperName);
                }
            }

            Format.Item.Add("Manuelle Eingabe", "neu", enImageCode.Stern, true, Constants.FirstSortChar.ToString());
            Format.Item.Sort();

            if (NurHochformat) {
                Hochformat.Checked = true;
                Querformat.Enabled = false;
            } else {
                Hochformat.Checked = !_PrintDocument1.DefaultPageSettings.Landscape;
            }

            Querformat.Checked = !Hochformat.Checked;

            FillHöheBreite(_PrintDocument1.DefaultPageSettings.PaperSize.Width, _PrintDocument1.DefaultPageSettings.PaperSize.Height);

            //   CheckHochQuer()

            //Breite.Text = _PrintDocument1.DefaultPageSettings.PaperSize.Width.ToString
            //Höhe.Text = _PrintDocument1.DefaultPageSettings.PaperSize.Height.ToString

            //   Format.Text = _PrintDocument1.DefaultPageSettings.PaperSize.Width.ToString & ";" & _PrintDocument1.DefaultPageSettings.PaperSize.Height.ToString
            // Zufuhr.Text = _PrintDocument1.

            Oben.Text = Inch1000ToMM(_PrintDocument1.DefaultPageSettings.Margins.Top).ToString();
            Unten.Text = Inch1000ToMM(_PrintDocument1.DefaultPageSettings.Margins.Bottom).ToString();
            Links.Text = Inch1000ToMM(_PrintDocument1.DefaultPageSettings.Margins.Left).ToString();
            Rechts.Text = Inch1000ToMM(_PrintDocument1.DefaultPageSettings.Margins.Right).ToString();

            DrawSampleAndCheckButton();
            // PrepareForShowing(Controls)

            Doing = false;
        }



        private void Something_TextChanged(object sender, System.EventArgs e) {

            if (Doing) { return; }
            Doing = true;
            DrawSampleAndCheckButton();
            Doing = false;

            //generatePic()
        }





        private void Format_ItemClicked(object sender, BasicListItemEventArgs e) {

            if (Doing) { return; }
            Doing = true;

            if (Format.Text.Contains(";")) {
                var l = Format.Text.SplitBy(";");
                FillHöheBreite(int.Parse(l[0]), int.Parse(l[1]));
            } else {
                Format.Text = "neu";
                FillHöheBreite(-1, -1);
            }

            DrawSampleAndCheckButton();

            Doing = false;
        }

        private double Inch1000ToMM(double inch) {
            return inch switch {
                8 => 2.0F,
                16 => 4.0F,
                20 => 5.0F,
                39 => 10.0F,
                79 => 20.0F,
                98 => 25.0F,
                394 => 100.0F,
                413 => 105.0F,
                432 => 110.0F,
                583 => 148.0F,
                591 => 150.0F,
                787 => 200.0F,
                827 => 210.0F,
                1169 => 297.0F,
                1654 => 420.0F,
                _ => Math.Round(inch * 0.254, 1),
            };
        }

        private void FillHöheBreite(double B, double h) {
            var nn1 = B + ";" + h;
            var nn2 = h + ";" + B;

            if (Format.Item[nn1] != null) {
                Format.Text = nn1;
            } else if (Format.Item[nn2] != null) {
                Format.Text = nn2;
            } else {
                Format.Text = "neu";
                Breite.Enabled = true;
                Höhe.Enabled = true;
                B = B < 0 && !string.IsNullOrEmpty(Breite.Text) ? double.Parse(Breite.Text) : Inch1000ToMM(B);

                h = h < 0 && !string.IsNullOrEmpty(Höhe.Text) ? double.Parse(Höhe.Text) : Inch1000ToMM(h);
            }

            if (Format.Text != "neu") {

                B = Inch1000ToMM(B);
                h = Inch1000ToMM(h);

                Breite.Enabled = false;
                Höhe.Enabled = false;

                switch (Format.Text) {
                    case "827;1169":
                        //A4
                        h = 297;
                        B = 210;
                        break;
                    case "1169;1654":
                        //A3
                        h = 420;
                        B = 297;
                        break;
                    case "583;827":
                        //A5
                        h = 210;
                        B = 148;
                        break;
                    case "413;583":
                        //A6
                        h = 148;
                        B = 105;
                        break;
                }
            }

            Breite.Text = Math.Round(B, 1).ToString();
            Höhe.Text = Math.Round(h, 1).ToString();
        }

        private void Abmasse_TextChanged(object sender, System.EventArgs e) {
            if (Doing) { return; }
            Doing = true;
            DrawSampleAndCheckButton();
            Doing = false;
        }

        private void HochQuer_CheckedChanged(object sender, System.EventArgs e) {

            if (Doing) { return; }
            if (!((Button)sender).Checked) { return; }

            Doing = true;
            DrawSampleAndCheckButton();

            Doing = false;
        }

        private void DrawSampleAndCheckButton() {

            var makeP = true;

            if (!Breite.Text.IsFormat(enDataFormat.Gleitkommazahl)) { makeP = false; }
            if (!Höhe.Text.IsFormat(enDataFormat.Gleitkommazahl)) { makeP = false; }
            if (!Oben.Text.IsFormat(enDataFormat.Gleitkommazahl)) { makeP = false; }
            if (!Unten.Text.IsFormat(enDataFormat.Gleitkommazahl)) { makeP = false; }
            if (!Links.Text.IsFormat(enDataFormat.Gleitkommazahl)) { makeP = false; }
            if (!Rechts.Text.IsFormat(enDataFormat.Gleitkommazahl)) { makeP = false; }

            double br = 0;
            double ho = 0;

            if (makeP) {
                br = double.Parse(Breite.Text);
                if (br < 5) { makeP = false; }
                ho = double.Parse(Höhe.Text);
                if (ho < 5) { makeP = false; }
            }

            if (Querformat.Checked) { modAllgemein.Swap(ref br, ref ho); }

            if (makeP) {
                OK_Enabled = true;

                var Z = Math.Min(Sample.Width / br, Sample.Height / ho);

                var l = (float)(float.Parse(Links.Text) * Z);
                var o = (float)(float.Parse(Oben.Text) * Z);
                var r = (float)(float.Parse(Rechts.Text) * Z);
                var u = (float)(float.Parse(Unten.Text) * Z);

                var i = new Bitmap((int)((br * Z) - 1), (int)((ho * Z) - 1));

                var gr = Graphics.FromImage(i);

                gr.Clear(Color.White);
                gr.DrawRectangle(Pens.Black, 0, 0, i.Width - 1, i.Height - 1);

                gr.DrawRectangle(Pens.Gray, l, o, i.Width - r - l, i.Height - u - o);

                Sample.Image = i;

            } else {
                OK_Enabled = false;
                Sample.Image = null;

            }
        }

        protected override void SetValue(bool canceled) {
            if (canceled) { GiveBack = null; return; }

            GiveBack = new PrintDocument();

            GiveBack.DefaultPageSettings.Landscape = Querformat.Checked;
            GiveBack.DefaultPageSettings.PaperSize = new PaperSize("Benutzerdefiniert", (int)(float.Parse(Breite.Text) / 0.254), (int)(float.Parse(Höhe.Text) / 0.254));
            GiveBack.DefaultPageSettings.Margins.Top = (int)(float.Parse(Oben.Text) / 0.254);
            GiveBack.DefaultPageSettings.Margins.Bottom = (int)(float.Parse(Unten.Text) / 0.254);
            GiveBack.DefaultPageSettings.Margins.Left = (int)(float.Parse(Links.Text) / 0.254);
            GiveBack.DefaultPageSettings.Margins.Right = (int)(float.Parse(Rechts.Text) / 0.254);
        }
    }
}