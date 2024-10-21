// Authors:
// Christian Peter
//
// Copyright (c) 2024 Christian Peter
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

using System.Collections.Generic;
using System.Drawing;
using BlueBasics;
using BlueControls;
using BlueControls.Enums;
using BlueControls.Interfaces;
using static BlueBasics.Converter;

public static class SkinDesignExtensions {

    #region Methods

    public static void Add(this Dictionary<Design, Dictionary<States, SkinDesign>> dictControl, Design ds, States status, Kontur enKontur, int x1, int y1, int x2, int y2, HintergrundArt hint, string bc1, string bc2, RahmenArt rahm, string boc1, string boc2, string f, string pic) {
        Dictionary<States, SkinDesign> dictState;

        if (dictControl.TryGetValue(ds, out var existingDictOfControl)) {
            dictState = existingDictOfControl;
        } else {
            dictState = [];
            dictControl.Add(ds, dictState);
        }

        dictState.Add(status, enKontur, x1, y1, x2, y2, hint, bc1, bc2, rahm, boc1, boc2, f, pic);
    }

    public static void Remove(this Dictionary<Design, Dictionary<States, SkinDesign>> dictControl, Design ds, States status) {
        if (dictControl.TryGetValue(ds, out var existingDictOfControl)) {
            _ = existingDictOfControl.Remove(status);
        }
    }

    private static void Add(this IDictionary<States, SkinDesign> dictStats, States status, Kontur enKontur, int x1, int y1, int x2, int y2, HintergrundArt hint, string bc1, string bc2, RahmenArt rahm, string boc1, string boc3, string f, string pic) {
        SkinDesign des = new() {
            Need = true,
            Kontur = enKontur,
            X1 = x1,
            Y1 = y1,
            X2 = x2,
            Y2 = y2,
            HintergrundArt = hint
        };

        if (!string.IsNullOrEmpty(bc1)) { des.BackColor1 = bc1.FromHtmlCode(); }
        if (!string.IsNullOrEmpty(bc2)) { des.BackColor2 = bc2.FromHtmlCode(); }
        des.HintergrundArt = hint;
        des.RahmenArt = rahm;
        if (!string.IsNullOrEmpty(boc1)) { des.BorderColor1 = boc1.FromHtmlCode(); }
        if (!string.IsNullOrEmpty(boc3)) { des.BorderColor2 = boc3.FromHtmlCode(); }

        des.SheetStyle = string.Empty;

        if (!string.IsNullOrEmpty(f)) {
            if (f.StartsWith("{")) {
                des.Font = BlueFont.Get(f);
            } else {
                if (Skin.StyleDb == null) { Skin.InitStyles(); }

                var fl = (f + "|0|0").SplitAndCutBy("|");
                des.SheetStyle = fl[0];
                des.Font = Skin.GetBlueFont(fl[0], (PadStyles)IntParse(fl[1]), (States)IntParse(fl[2]), 1f);
            }
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
    public BlueFont Font { get; set; }
    public HintergrundArt HintergrundArt { get; set; }
    public string Image { get; set; } = string.Empty;
    public Kontur Kontur { get; set; }
    public bool Need { get; set; }
    public RahmenArt RahmenArt { get; set; }
    public string SheetStyle { get; set; } = string.Empty;
    public States Status { get; set; }
    public PadStyles Stil { get; set; }
    public int X1 { get; set; }

    public int X2 { get; set; }

    public int Y1 { get; set; }

    public int Y2 { get; set; }

    #endregion
}