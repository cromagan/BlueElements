// Licensed under AGPL-3.0; see License.md for disclaimer and details.

using BlueControls.Classes;
using static BlueBasics.ClassesStatic.Converter;

namespace BluePaint;

public partial class Tool_DummyGenerator {

    #region Fields

    private static readonly Brush BrushBlack = new SolidBrush(Color.Black);
    private static readonly Pen PenBlack = new Pen(Color.Black, 2);

    #endregion

    #region Constructors

    public Tool_DummyGenerator() : base() => InitializeComponent();

    #endregion

    #region Methods

    public static Bitmap CreateDummy(string text, int width, int height) {
        var bmp = new Bitmap(width, height);
        using var gr = Graphics.FromImage(bmp);
        gr.Clear(Color.White);
        gr.DrawRectangle(PenBlack, 1, 1, bmp.Width - 2, bmp.Height - 2);
        if (!string.IsNullOrEmpty(text)) {
            using var f = new Font("Arial", 50, FontStyle.Bold);
            var fs = f.MeasureString(text);
            gr.TranslateTransform((float)(bmp.Width / 2.0), (float)(bmp.Height / 2.0));
            gr.RotateTransform(-90);
            BlueFont.DrawString(gr, text, f, BrushBlack, (float)(-fs.Width / 2.0), (float)(-fs.Height / 2.0));
        }
        return bmp;
    }

    private void CreateDummy() {
        var w = IntParse(MathFormulaParser.Ergebnis(X.Text));
        var h = IntParse(MathFormulaParser.Ergebnis(Y.Text));
        if (w < 2) {
            QuickNote.Show(NoteSymbols.Warning, "Breite eingeben", X);
            return;
        }
        if (h < 2) {
            QuickNote.Show(NoteSymbols.Warning, "Höhe eingeben", Y);
            return;
        }

        var newPic = CreateDummy(TXT.Text, w, h);

        OnOverridePic(newPic, true);
    }

    private void Erstellen_Click(object sender, System.EventArgs e) {
        CreateDummy();
        OnZoomFit();
    }

    #endregion
}