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
using BlueControls;
using BlueControls.Enums;
using BlueControls.Interfaces;
using System.Collections.Generic;
using System.Drawing;
using static BlueBasics.Converter;

public static class SkinDesignExtensions {

    #region Methods

    public static void Add(this Dictionary<Design, Dictionary<States, SkinDesign>> dictControl, Design ds, States status, string font, Kontur enKontur, int x1, int y1, int x2, int y2, HintergrundArt hint, string bc1, string bc2, RahmenArt rahm, string boc1, string boc2, string pic) {
        Dictionary<States, SkinDesign> dictState;

        if (dictControl.TryGetValue(ds, out var existingDictOfControl)) {
            dictState = existingDictOfControl;
        } else {
            dictState = [];
            dictControl.Add(ds, dictState);
        }

        dictState.Add(status, font, enKontur, x1, y1, x2, y2, hint, bc1, bc2, rahm, boc1, boc2, pic);
    }

    public static void Remove(this Dictionary<Design, Dictionary<States, SkinDesign>> dictControl, Design ds, States status) {
        if (dictControl.TryGetValue(ds, out var existingDictOfControl)) {
            existingDictOfControl.Remove(status);
        }
    }

    private static void Add(this Dictionary<States, SkinDesign> dictStats, States status, string font, Kontur enKontur, int x1, int y1, int x2, int y2, HintergrundArt hint, string bc1, string bc2, RahmenArt rahm, string boc1, string boc3, string pic) {
        SkinDesign des = new() {
            Need = true,
            Kontur = enKontur,
            X1 = x1,
            Y1 = y1,
            X2 = x2,
            Y2 = y2,
            HintergrundArt = hint
        };

        if (!string.IsNullOrEmpty(bc1)) { des.BackColor1 = ColorParse(bc1); }
        if (!string.IsNullOrEmpty(bc2)) { des.BackColor2 = ColorParse(bc2); }
        des.HintergrundArt = hint;
        des.RahmenArt = rahm;
        if (!string.IsNullOrEmpty(boc1)) { des.BorderColor1 = ColorParse(boc1); }
        if (!string.IsNullOrEmpty(boc3)) { des.BorderColor2 = ColorParse(boc3); }

        des.SheetStyle = string.Empty;

        if (!string.IsNullOrEmpty(font)) {
            Skin.InitStyles();
            var fl = font.SplitAndCutBy("|");
            des.SheetStyle = fl[0];
            des.Style = (PadStyles)IntParse(fl[1]);
            des.Font = Skin.GetBlueFont(des.SheetStyle, des.Style);
        }

        des.Image = pic;
        des.Status = status;
        dictStats.Add(status, des);
    }

    #endregion
}

public class SkinDesign : IStyleableOne {

    #region Properties

    public Color BackColor1 { get; set; }
    public Color BackColor2 { get; set; }
    public Color BorderColor1 { get; set; }
    public Color BorderColor2 { get; set; }
    public BlueFont Font { get; set; } = BlueFont.DefaultFont;
    public HintergrundArt HintergrundArt { get; set; }
    public string Image { get; set; } = string.Empty;
    public Kontur Kontur { get; set; }
    public bool Need { get; set; }
    public RahmenArt RahmenArt { get; set; }
    public string SheetStyle { get; set; } = string.Empty;
    public States Status { get; set; }
    public PadStyles Style { get; set; }
    public int X1 { get; set; }

    public int X2 { get; set; }

    public int Y1 { get; set; }

    public int Y2 { get; set; }

    #endregion
}