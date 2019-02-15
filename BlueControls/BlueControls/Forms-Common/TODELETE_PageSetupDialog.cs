using System;
using System.Drawing;
using System.Drawing.Printing;
using BlueBasics;
using BlueBasics.Enums;
using BlueControls.Controls;
using BlueControls.EventArgs;
using BlueControls.ItemCollection.ItemCollectionList;

namespace BlueControls.Forms
{
    public partial class PageSetupDialog
    {
        private bool Doing;

        private readonly PrintDocument OriD;

        //  ReadOnly _NurHoch As Boolean

        private bool cancelx = true;


        public PageSetupDialog(PrintDocument _PrintDocument1, bool NurHochformat)
        {

            // Dieser Aufruf ist für den Designer erforderlich.
            InitializeComponent();

            // Fügen Sie Initialisierungen nach dem InitializeComponent()-Aufruf hinzu.

            Doing = true;

            // _NurHoch = NurHochformat

            OriD = _PrintDocument1;

            Format.Item.Clear();

            foreach (PaperSize ps in _PrintDocument1.PrinterSettings.PaperSizes)
            {
                var nn = ps.Width + ";" + ps.Height;
                if (Format.Item[nn] == null)
                {
                    Format.Item.Add(new TextListItem(nn, ps.PaperName, QuickImage.Get(enImageCode.Datei), true, ps.PaperName));
                }
            }

            Format.Item.Add(new TextListItem("neu", "Manuelle Eingabe", enImageCode.Stern, true, Constants.FirstSortChar.ToString()));
            Format.Item.Sort();

            if (NurHochformat)
            {
                Hochformat.Checked = true;
                Querformat.Enabled = false;
            }
            else
            {
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


        private void Ok_Click(object sender, System.EventArgs e)
        {
            cancelx = false;
            Close();
        }

        private void Something_TextChanged(object sender, System.EventArgs e)
        {

            if (Doing) { return; }
            Doing = true;
            DrawSampleAndCheckButton();
            Doing = false;

            //generatePic()
        }


        public bool Canceled()
        {
            return cancelx;
        }

        public new void ShowDialog()
        {
            cancelx = true;

            base.ShowDialog();

            if (cancelx) { return; }


            OriD.DefaultPageSettings.Landscape = Querformat.Checked;
            OriD.DefaultPageSettings.PaperSize = new PaperSize("Benutzerdefiniert", Convert.ToInt32(float.Parse(Breite.Text) / 0.254), Convert.ToInt32(float.Parse(Höhe.Text) / 0.254));
            OriD.DefaultPageSettings.Margins.Top = Convert.ToInt32(float.Parse(Oben.Text) / 0.254);
            OriD.DefaultPageSettings.Margins.Bottom = Convert.ToInt32(float.Parse(Unten.Text) / 0.254);
            OriD.DefaultPageSettings.Margins.Left = Convert.ToInt32(float.Parse(Links.Text) / 0.254);
            OriD.DefaultPageSettings.Margins.Right = Convert.ToInt32(float.Parse(Rechts.Text) / 0.254);
        }

        private void canc_Click(object sender, System.EventArgs e)
        {
            cancelx = true;
            Close();
        }

        private void Format_Item_Click(object sender, BasicListItemEventArgs e)
        {

            if (Doing) { return; }
            Doing = true;


            if (Format.Text.Contains(";"))
            {
                var l = Format.Text.SplitBy(";");
                FillHöheBreite(int.Parse(l[0]), int.Parse(l[1]));
            }
            else
            {
                Format.Text = "neu";
                FillHöheBreite(-1, -1);
            }


            DrawSampleAndCheckButton();


            Doing = false;
        }


        private double Inch1000ToMM(double Inch)
        {
            switch (Inch)
            {
                case 8: return 2.0F;
                case 16: return 4.0F;
                case 20: return 5.0F;
                case 39: return 10.0F;
                case 79: return 20.0F;
                case 98: return 25.0F;
                case 394: return 100.0F;
                case 413: return 105.0F;
                case 432: return 110.0F;
                case 583: return 148.0F;
                case 591: return 150.0F;
                case 787: return 200.0F;
                case 827: return 210.0F;
                case 1169: return 297.0F;
                case 1654: return 420.0F;
                default: return Math.Round(Inch * 0.254, 1);
            }
        }


        private void FillHöheBreite(double B, double h)
        {
            var nn1 = B + ";" + h;
            var nn2 = h + ";" + B;

            if (Format.Item[nn1] != null)
            {
                Format.Text = nn1;
            }
            else if (Format.Item[nn2] != null)
            {
                Format.Text = nn2;
            }
            else
            {
                Format.Text = "neu";
                Breite.Enabled = true;
                Höhe.Enabled = true;
                if (B < 0 && !string.IsNullOrEmpty(Breite.Text))
                {
                    B = double.Parse(Breite.Text);
                }
                else
                {
                    B = Inch1000ToMM(B);
                }

                if (h < 0 && !string.IsNullOrEmpty(Höhe.Text))
                {
                    h = double.Parse(Höhe.Text);
                }
                else
                {
                    h = Inch1000ToMM(h);
                }


            }


            if (Format.Text != "neu")
            {

                B = Inch1000ToMM(B);
                h = Inch1000ToMM(h);

                Breite.Enabled = false;
                Höhe.Enabled = false;


                switch (Format.Text)
                {
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


        private void Abmasse_TextChanged(object sender, System.EventArgs e)
        {
            if (Doing) { return; }
            Doing = true;
            DrawSampleAndCheckButton();
            Doing = false;
        }

        private void HochQuer_CheckedChanged(object sender, System.EventArgs e)
        {

            if (Doing) { return; }
            if (!((Button)sender).Checked) { return; }

            Doing = true;
            DrawSampleAndCheckButton();

            Doing = false;
        }


        private void DrawSampleAndCheckButton()
        {

            var makeP = true;


            if (!Breite.Text.IsFormat(enDataFormat.Gleitkommazahl)) { makeP = false; }
            if (!Höhe.Text.IsFormat(enDataFormat.Gleitkommazahl)) { makeP = false; }
            if (!Oben.Text.IsFormat(enDataFormat.Gleitkommazahl)) { makeP = false; }
            if (!Unten.Text.IsFormat(enDataFormat.Gleitkommazahl)) { makeP = false; }
            if (!Links.Text.IsFormat(enDataFormat.Gleitkommazahl)) { makeP = false; }
            if (!Rechts.Text.IsFormat(enDataFormat.Gleitkommazahl)) { makeP = false; }

            double br = 0;
            double ho = 0;

            if (makeP)
            {
                br = double.Parse(Breite.Text);
                if (br < 5) { makeP = false; }
                ho = double.Parse(Höhe.Text);
                if (ho < 5) { makeP = false; }
            }

            if (Querformat.Checked) { modAllgemein.Swap(ref br, ref ho); }


            if (makeP)
            {
                Ok.Enabled = true;


                var Z = Math.Min(Sample.Width / br, Sample.Height / ho);


                var l = (float)(float.Parse(Links.Text) * Z);
                var o = (float)(float.Parse(Oben.Text) * Z);
                var r = (float)(float.Parse(Rechts.Text) * Z);
                var u = (float)(float.Parse(Unten.Text) * Z);


                var i = new Bitmap(Convert.ToInt32(br * Z - 1), Convert.ToInt32(ho * Z - 1));

                var gr = Graphics.FromImage(i);

                gr.Clear(Color.White);
                gr.DrawRectangle(Pens.Black, 0, 0, i.Width - 1, i.Height - 1);


                gr.DrawRectangle(Pens.Gray, l, o, i.Width - r - l, i.Height - u - o);


                Sample.Image = i;


            }
            else
            {
                Ok.Enabled = false;
                Sample.Image = null;

            }
        }

    }
}