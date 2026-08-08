// Licensed under AGPL-3.0; see License.md for disclaimer and details.

using BlueControls.Editoren;
using BlueControls.EventArgs;
using System.Windows.Forms;
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

    public override Type? EditorFor => typeof(PageSetupData);
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
        if (toEdit is not PageSetupData { } data) { return false; }

        _doing = true;
        Format.ItemClear();
        foreach (var f in data.VerfügbareFormate) {
            var nn = FormatKey(f.BreiteMm, f.HöheMm);
            if (Format[nn] is null) {
                Format.ItemAdd(ItemOf(f.Name, nn, QuickImage.Get(ImageCode.Datei), true, f.Name));
            }
        }
        Format.ItemAdd(ItemOf("Manuelle Eingabe", "neu", ImageCode.Stern, true, Constants.FirstSortChar.ToString()));
        Hochformat.Checked = !data.Querformat;
        Querformat.Checked = !Hochformat.Checked;
        FillHöheBreite(data.BreiteMm, data.HöheMm);
        Oben.Text = data.RandObenMm.ToString1_2();
        Unten.Text = data.RandUntenMm.ToString1_2();
        Links.Text = data.RandLinksMm.ToString1_2();
        Rechts.Text = data.RandRechtsMm.ToString1_2();
        DrawSampleAndCheckButton();
        _doing = false;
        return true;
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

    private void FillHöheBreite(float bMm, float hMm) {
        var nn1 = FormatKey(bMm, hMm);
        var nn2 = FormatKey(hMm, bMm);
        if (Format[nn1] is not null) {
            Format.Text = nn1;
        } else if (Format[nn2] is not null) {
            Format.Text = nn2;
        } else {
            Format.Text = "neu";
        }
        Breite.Text = bMm.ToString1_1();
        Höhe.Text = hMm.ToString1_1();
        Breite.Enabled = IsModeSupported() && Format.Text == "neu";
        Höhe.Enabled = IsModeSupported() && Format.Text == "neu";
    }

    private static string FormatKey(float breiteMm, float höheMm) => $"{(int)Math.Round(breiteMm)};{(int)Math.Round(höheMm)}";

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
        if (((IIsEditor)this).OutputItem is not PageSetupData { } data) { return; }
        if (!Breite.Text.IsNumeral()) { return; }
        if (!Höhe.Text.IsNumeral()) { return; }
        if (!Oben.Text.IsNumeral()) { return; }
        if (!Unten.Text.IsNumeral()) { return; }
        if (!Links.Text.IsNumeral()) { return; }
        if (!Rechts.Text.IsNumeral()) { return; }
        var br = (float)DoubleParse(Breite.Text);
        var ho = (float)DoubleParse(Höhe.Text);
        if (br < 5 || ho < 5) { return; }

        data.BreiteMm = br;
        data.HöheMm = ho;
        data.Querformat = Querformat.Checked;
        data.RandObenMm = FloatParse(Oben.Text);
        data.RandUntenMm = FloatParse(Unten.Text);
        data.RandLinksMm = FloatParse(Links.Text);
        data.RandRechtsMm = FloatParse(Rechts.Text);
    }

    #endregion
}
