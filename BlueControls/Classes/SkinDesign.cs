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

using BlueControls.Classes;
using BlueControls.Enums;
using System.Collections.Generic;
using System.Drawing;
using static BlueBasics.ClassesStatic.Converter;

public static class SkinDesignExtensions {

    #region Methods

    public static void Add(this Dictionary<Design, Dictionary<States, SkinDesign>> dictControl, Design ds, States status, string font, Contour contour, int x1, int y1, int x2, int y2, BackgroundStyle hint, string bc1, string bc2, BorderStyle rahm, string boc1, string boc2, string pic, string bc3 = "", float vm = 0.7f) {
        Dictionary<States, SkinDesign> dictState;

        if (dictControl.TryGetValue(ds, out var existingDictOfControl)) {
            dictState = existingDictOfControl;
        } else {
            dictState = [];
            dictControl.Add(ds, dictState);
        }

        dictState.Add(status, font, contour, x1, y1, x2, y2, hint, bc1, bc2, rahm, boc1, boc2, pic, bc3, vm);
    }

    public static void Remove(this Dictionary<Design, Dictionary<States, SkinDesign>> dictControl, Design ds, States status) {
        if (dictControl.TryGetValue(ds, out var existingDictOfControl)) {
            existingDictOfControl.Remove(status);
        }
    }

    private static void Add(this Dictionary<States, SkinDesign> dictStats, States status, string font, Contour contour, int x1, int y1, int x2, int y2, BackgroundStyle hint, string bc1, string bc2, BorderStyle rahm, string boc1, string boc3, string pic, string bc3, float vm) {
        var des = new SkinDesign() {
            Need = true,
            Contour = contour,
            X1 = x1,
            Y1 = y1,
            X2 = x2,
            Y2 = y2,
            BackgroundStyle = hint,
            GradientMidpoint = vm
        };

        if (!string.IsNullOrEmpty(bc1)) { des.BackColor1 = ColorParse(bc1); }
        if (!string.IsNullOrEmpty(bc2)) { des.BackColor2 = ColorParse(bc2); }
        if (!string.IsNullOrEmpty(bc3)) { des.BackColor3 = ColorParse(bc3); }
        des.BackgroundStyle = hint;
        des.BorderStyle = rahm;
        if (!string.IsNullOrEmpty(boc1)) { des.BorderColor1 = ColorParse(boc1); }
        if (!string.IsNullOrEmpty(boc3)) { des.BorderColor2 = ColorParse(boc3); }

        if (!string.IsNullOrEmpty(font)) {
            des.Font = BlueFont.Get(font);
        }

        des.Image = pic;
        des.Status = status;
        dictStats[status] = des;
    }

    #endregion
}

public class SkinDesign {

    #region Properties

    public Color BackColor1 { get; set; }
    public Color BackColor2 { get; set; }
    public Color BackColor3 { get; set; }
    public BackgroundStyle BackgroundStyle { get; set; }
    public Color BorderColor1 { get; set; }
    public Color BorderColor2 { get; set; }
    public BorderStyle BorderStyle { get; set; }
    public Contour Contour { get; set; }
    public BlueFont Font { get; set; } = BlueFont.DefaultFont;
    public float GradientMidpoint { get; set; } = 0.7f;
    public string Image { get; set; } = string.Empty;
    public bool Need { get; set; }
    public States Status { get; set; }
    public int X1 { get; set; }

    public int X2 { get; set; }

    public int Y1 { get; set; }

    public int Y2 { get; set; }

    #endregion
}