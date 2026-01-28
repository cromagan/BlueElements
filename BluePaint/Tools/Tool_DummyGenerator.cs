// Authors:
// Christian Peter
//
// Copyright © 2026 Christian Peter
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

using BlueBasics;
using BlueBasics.Enums;
using BlueControls;
using BlueControls.Forms;
using System.Drawing;
using static BlueBasics.Converter;

namespace BluePaint;

public partial class Tool_DummyGenerator {

    #region Constructors

    public Tool_DummyGenerator() : base() => InitializeComponent();

    #endregion

    #region Methods

    public static Bitmap CreateDummy(string text, int width, int height) {
        var bmp = new Bitmap(width, height);
        var gr = Graphics.FromImage(bmp);
        gr.Clear(Color.White);
        gr.DrawRectangle(new Pen(Color.Black, 2), 1, 1, bmp.Width - 2, bmp.Height - 2);
        if (!string.IsNullOrEmpty(text)) {
            var f = new Font("Arial", 50, FontStyle.Bold);
            var fs = f.MeasureString(text);
            gr.TranslateTransform((float)(bmp.Width / 2.0), (float)(bmp.Height / 2.0));
            gr.RotateTransform(-90);
            BlueFont.DrawString(gr, text, f, new SolidBrush(Color.Black), (float)(-fs.Width / 2.0), (float)(-fs.Height / 2.0));
        }
        return bmp;
    }

    private void CreateDummy() {
        var w = IntParse(MathFormulaParser.Ergebnis(X.Text));
        var h = IntParse(MathFormulaParser.Ergebnis(Y.Text));
        if (w < 2) {
            Notification.Show("Bitte Breite eingeben.", ImageCode.Information);
            return;
        }
        if (h < 2) {
            Notification.Show("Bitte Höhe eingeben.", ImageCode.Information);
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