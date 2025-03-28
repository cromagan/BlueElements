﻿// Authors:
// Christian Peter
//
// Copyright (c) 2025 Christian Peter
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

#nullable enable

using BlueBasics;
using BlueBasics.Enums;
using BlueControls.Controls;
using BlueControls.EventArgs;
using System;
using System.Drawing;
using System.Drawing.Printing;
using static BlueBasics.Converter;
using static BlueControls.ItemCollectionList.AbstractListItemExtension;

namespace BlueControls.Forms;

public partial class PageSetupDialog : DialogWithOkAndCancel {

    #region Fields

    private bool _doing;
    private PrintDocument? _giveBack;

    #endregion

    #region Constructors

    private PageSetupDialog(PrintDocument printDocument1, bool nurHochformat) : base() {
        // Dieser Aufruf ist für den Designer erforderlich.
        InitializeComponent();
        // Fügen Sie Initialisierungen nach dem InitializeComponent()-Aufruf hinzu.
        _doing = true;
        // _NurHoch = NurHochformat
        Format.ItemClear();
        foreach (PaperSize ps in printDocument1.PrinterSettings.PaperSizes) {
            var nn = ps.Width + ";" + ps.Height;
            if (Format[nn] == null) {
                Format.ItemAdd(ItemOf(ps.PaperName, nn, QuickImage.Get(ImageCode.Datei), true, ps.PaperName));
            }
        }
        Format.ItemAdd(ItemOf("Manuelle Eingabe", "neu", ImageCode.Stern, true, Constants.FirstSortChar.ToString()));
        //Format.Item.Sort();
        if (nurHochformat) {
            Hochformat.Checked = true;
            Querformat.Enabled = false;
        } else {
            Hochformat.Checked = !printDocument1.DefaultPageSettings.Landscape;
        }
        Querformat.Checked = !Hochformat.Checked;
        FillHöheBreite(printDocument1.DefaultPageSettings.PaperSize.Width, printDocument1.DefaultPageSettings.PaperSize.Height);
        //   CheckHochQuer()
        //Breite.Text = _PrintDocument1.DefaultPageSettings.PaperSize.Width.ToString
        //Höhe.Text = _PrintDocument1.DefaultPageSettings.PaperSize.Height.ToString
        //   Format.Text = _PrintDocument1.DefaultPageSettings.PaperSize.Width.ToString & ";" & _PrintDocument1.DefaultPageSettings.PaperSize.Height.ToString
        // Zufuhr.Text = _PrintDocument1.
        Oben.Text = Inch1000ToMm(printDocument1.DefaultPageSettings.Margins.Top).ToStringFloat2();
        Unten.Text = Inch1000ToMm(printDocument1.DefaultPageSettings.Margins.Bottom).ToStringFloat2();
        Links.Text = Inch1000ToMm(printDocument1.DefaultPageSettings.Margins.Left).ToStringFloat2();
        Rechts.Text = Inch1000ToMm(printDocument1.DefaultPageSettings.Margins.Right).ToStringFloat2();
        DrawSampleAndCheckButton();
        // PrepareForShowing(Controls)
        _doing = false;
    }

    #endregion

    #region Methods

    public static PrintDocument? Show(PrintDocument printDocument1, bool nurHochformat) {
        PageSetupDialog mb = new(printDocument1, nurHochformat);
        _ = mb.ShowDialog();
        return mb._giveBack;
    }

    protected override bool SetValue() {
        if (Canceled) { _giveBack = null; return true; }
        _giveBack = new PrintDocument();
        _giveBack.DefaultPageSettings.Landscape = Querformat.Checked;
        _giveBack.DefaultPageSettings.PaperSize = new PaperSize("Benutzerdefiniert", (int)(FloatParse(Breite.Text) / 0.254), (int)(FloatParse(Höhe.Text) / 0.254));
        _giveBack.DefaultPageSettings.Margins.Top = (int)(FloatParse(Oben.Text) / 0.254);
        _giveBack.DefaultPageSettings.Margins.Bottom = (int)(FloatParse(Unten.Text) / 0.254);
        _giveBack.DefaultPageSettings.Margins.Left = (int)(FloatParse(Links.Text) / 0.254);
        _giveBack.DefaultPageSettings.Margins.Right = (int)(FloatParse(Rechts.Text) / 0.254);
        return true;
    }

    private static double Inch1000ToMm(double inch) {
        switch (inch) {
            case 8:
                return 2.0F;

            case 16:
                return 4.0F;

            case 20:
                return 5.0F;

            case 39:
                return 10.0F;

            case 79:
                return 20.0F;

            case 98:
                return 25.0F;

            case 394:
                return 100.0F;

            case 413:
                return 105.0F;

            case 432:
                return 110.0F;

            case 583:
                return 148.0F;

            case 591:
                return 150.0F;

            case 787:
                return 200.0F;

            case 827:
                return 210.0F;

            case 1169:
                return 297.0F;

            case 1654:
                return 420.0F;

            default:
                return Math.Round(inch * 0.254, 1, MidpointRounding.AwayFromZero);
        }
    }

    private void Abmasse_TextChanged(object sender, System.EventArgs e) {
        if (_doing) { return; }
        _doing = true;
        DrawSampleAndCheckButton();
        _doing = false;
    }

    private void DrawSampleAndCheckButton() {
        var makeP = Breite.Text.IsNumeral();
        if (!Höhe.Text.IsNumeral()) { makeP = false; }
        if (!Oben.Text.IsNumeral()) { makeP = false; }
        if (!Unten.Text.IsNumeral()) { makeP = false; }
        if (!Links.Text.IsNumeral()) { makeP = false; }
        if (!Rechts.Text.IsNumeral()) { makeP = false; }
        double br = 0;
        double ho = 0;
        if (makeP) {
            br = DoubleParse(Breite.Text);
            if (br < 5) { makeP = false; }
            ho = DoubleParse(Höhe.Text);
            if (ho < 5) { makeP = false; }
        }
        if (Querformat.Checked) { Generic.Swap(ref br, ref ho); }
        if (makeP) {
            OK_Enabled = true;
            var z = Math.Min(Sample.Width / br, Sample.Height / ho);
            var l = (float)(FloatParse(Links.Text) * z);
            var o = (float)(FloatParse(Oben.Text) * z);
            var r = (float)(FloatParse(Rechts.Text) * z);
            var u = (float)(FloatParse(Unten.Text) * z);
            Bitmap i = new((int)((br * z) - 1), (int)((ho * z) - 1));
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

    private void FillHöheBreite(double b, double h) {
        var nn1 = b + ";" + h;
        var nn2 = h + ";" + b;
        if (Format[nn1] != null) {
            Format.Text = nn1;
        } else if (Format[nn2] != null) {
            Format.Text = nn2;
        } else {
            Format.Text = "neu";
            Breite.Enabled = true;
            Höhe.Enabled = true;
            b = b < 0 && !string.IsNullOrEmpty(Breite.Text) ? DoubleParse(Breite.Text) : Inch1000ToMm(b);
            h = h < 0 && !string.IsNullOrEmpty(Höhe.Text) ? DoubleParse(Höhe.Text) : Inch1000ToMm(h);
        }
        if (Format.Text != "neu") {
            b = Inch1000ToMm(b);
            h = Inch1000ToMm(h);
            Breite.Enabled = false;
            Höhe.Enabled = false;
            switch (Format.Text) {
                case "827;1169":
                    //A4
                    h = 297;
                    b = 210;
                    break;

                case "1169;1654":
                    //A3
                    h = 420;
                    b = 297;
                    break;

                case "583;827":
                    //A5
                    h = 210;
                    b = 148;
                    break;

                case "413;583":
                    //A6
                    h = 148;
                    b = 105;
                    break;
            }
        }
        Breite.Text = b.ToStringFloat1();
        Höhe.Text = h.ToStringFloat1();
    }

    private void Format_ItemClicked(object sender, AbstractListItemEventArgs e) {
        if (_doing) { return; }
        _doing = true;
        if (Format.Text.Contains(";")) {
            var l = Format.Text.SplitAndCutBy(";");
            FillHöheBreite(IntParse(l[0]), IntParse(l[1]));
        } else {
            Format.Text = "neu";
            FillHöheBreite(-1, -1);
        }
        DrawSampleAndCheckButton();
        _doing = false;
    }

    private void HochQuer_CheckedChanged(object sender, System.EventArgs e) {
        if (_doing) { return; }
        if (!((Button)sender).Checked) { return; }
        _doing = true;
        DrawSampleAndCheckButton();
        _doing = false;
    }

    private void Something_TextChanged(object sender, System.EventArgs e) {
        if (_doing) { return; }
        _doing = true;
        DrawSampleAndCheckButton();
        _doing = false;
        //generatePic()
    }

    #endregion
}