// Licensed under AGPL-3.0; see License.md for disclaimer and details.

using BlueControls.Editoren;
using BlueControls.EventArgs;
using System.Drawing.Printing;
using static BlueBasics.ClassesStatic.Converter;
using static BlueControls.Classes.ItemCollectionList.AbstractListItemExtension;

namespace BlueControls.Forms;

public partial class PageSetupDialog : EditorEasy {

    #region Fields

    private bool _doing;

    #endregion

    #region Constructors

    public PageSetupDialog() : base() => InitializeComponent();

    #endregion

    #region Properties

    public override Type? EditorFor => typeof(PrintDocument);
    public override EditorMode SupportedModes => EditorMode.EditItem;

    #endregion

    #region Methods

    public override void Clear() {
        Format.ItemClear();
        Oben.Text = string.Empty;
        Unten.Text = string.Empty;
        Links.Text = string.Empty;
        Rechts.Text = string.Empty;
        Breite.Text = string.Empty;
        Höhe.Text = string.Empty;
        Sample.Image = null;
    }

    protected override void InitializeComponentDefaultValues() { }

    protected override void SetEnabledState(bool enabled) {
        base.SetEnabledState(enabled);
        Format.Enabled = enabled;
        Oben.Enabled = enabled;
        Unten.Enabled = enabled;
        Links.Enabled = enabled;
        Rechts.Enabled = enabled;
        Hochformat.Enabled = enabled;
        Querformat.Enabled = enabled;
        Breite.Enabled = enabled && Format.Text == "neu";
        Höhe.Enabled = enabled && Format.Text == "neu";
    }

    protected override bool SetValuesToFormula(object? toEdit) {
        if (toEdit is not PrintDocument { } printDocument1) { return false; }

        _doing = true;
        Format.ItemClear();
        foreach (PaperSize ps in printDocument1.PrinterSettings.PaperSizes) {
            var nn = ps.Width + ";" + ps.Height;
            if (Format[nn] == null) {
                Format.ItemAdd(ItemOf(ps.PaperName, nn, QuickImage.Get(ImageCode.Datei), true, ps.PaperName));
            }
        }
        Format.ItemAdd(ItemOf("Manuelle Eingabe", "neu", ImageCode.Stern, true, Constants.FirstSortChar.ToString()));
        Hochformat.Checked = !printDocument1.DefaultPageSettings.Landscape;
        Querformat.Checked = !Hochformat.Checked;
        FillHöheBreite(printDocument1.DefaultPageSettings.PaperSize.Width, printDocument1.DefaultPageSettings.PaperSize.Height);
        Oben.Text = Inch1000ToMm(printDocument1.DefaultPageSettings.Margins.Top).ToString1_2();
        Unten.Text = Inch1000ToMm(printDocument1.DefaultPageSettings.Margins.Bottom).ToString1_2();
        Links.Text = Inch1000ToMm(printDocument1.DefaultPageSettings.Margins.Left).ToString1_2();
        Rechts.Text = Inch1000ToMm(printDocument1.DefaultPageSettings.Margins.Right).ToString1_2();
        DrawSampleAndCheckButton();
        _doing = false;
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
        WriteBackToData();
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
            var z = Math.Min(Sample.Width / br, Sample.Height / ho);
            var l = (float)(FloatParse(Links.Text) * z);
            var o = (float)(FloatParse(Oben.Text) * z);
            var r = (float)(FloatParse(Rechts.Text) * z);
            var u = (float)(FloatParse(Unten.Text) * z);
            var bmp = new Bitmap((int)((br * z) - 1), (int)((ho * z) - 1));
            using var gr = Graphics.FromImage(bmp);
            gr.Clear(Color.White);
            gr.DrawRectangle(Pens.Black, 0, 0, bmp.Width - 1, bmp.Height - 1);
            gr.DrawRectangle(Pens.Gray, l, o, bmp.Width - r - l, bmp.Height - u - o);
            Sample.Image = bmp;
        } else {
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
            b = b < 0 && !string.IsNullOrEmpty(Breite.Text) ? DoubleParse(Breite.Text) : Inch1000ToMm(b);
            h = h < 0 && !string.IsNullOrEmpty(Höhe.Text) ? DoubleParse(Höhe.Text) : Inch1000ToMm(h);
        }
        if (Format.Text != "neu") {
            b = Inch1000ToMm(b);
            h = Inch1000ToMm(h);
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
        Breite.Text = b.ToString1_1();
        Höhe.Text = h.ToString1_1();
        Breite.Enabled = IsModeSupported() && Format.Text == "neu";
        Höhe.Enabled = IsModeSupported() && Format.Text == "neu";
    }

    private void Format_ItemClicked(object sender, AbstractListItemEventArgs e) {
        if (_doing) { return; }
        _doing = true;
        if (Format.Text.Contains(';')) {
            var l = Format.Text.SplitAndCutBy(";");
            FillHöheBreite(IntParse(l[0]), IntParse(l[1]));
        } else {
            Format.Text = "neu";
            FillHöheBreite(-1, -1);
        }
        DrawSampleAndCheckButton();
        WriteBackToData();
        _doing = false;
    }

    private void HochQuer_CheckedChanged(object sender, System.EventArgs e) {
        if (_doing) { return; }
        if (!((BlueControls.Controls.Button)sender).Checked) { return; }
        _doing = true;
        DrawSampleAndCheckButton();
        WriteBackToData();
        _doing = false;
    }

    private void Something_TextChanged(object sender, System.EventArgs e) {
        if (_doing) { return; }
        _doing = true;
        DrawSampleAndCheckButton();
        WriteBackToData();
        _doing = false;
    }

    private void WriteBackToData() {
        if (((IIsEditor)this).OutputItem is not PrintDocument { } doc) { return; }
        if (!Breite.Text.IsNumeral()) { return; }
        if (!Höhe.Text.IsNumeral()) { return; }
        if (!Oben.Text.IsNumeral()) { return; }
        if (!Unten.Text.IsNumeral()) { return; }
        if (!Links.Text.IsNumeral()) { return; }
        if (!Rechts.Text.IsNumeral()) { return; }
        var br = DoubleParse(Breite.Text);
        var ho = DoubleParse(Höhe.Text);
        if (br < 5 || ho < 5) { return; }
        doc.DefaultPageSettings.Landscape = Querformat.Checked;
        doc.DefaultPageSettings.PaperSize = new PaperSize("Benutzerdefiniert", (int)(br / 0.254), (int)(ho / 0.254));
        doc.DefaultPageSettings.Margins.Top = (int)(FloatParse(Oben.Text) / 0.254);
        doc.DefaultPageSettings.Margins.Bottom = (int)(FloatParse(Unten.Text) / 0.254);
        doc.DefaultPageSettings.Margins.Left = (int)(FloatParse(Links.Text) / 0.254);
        doc.DefaultPageSettings.Margins.Right = (int)(FloatParse(Rechts.Text) / 0.254);
    }

    #endregion
}