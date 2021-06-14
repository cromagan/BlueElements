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

using BlueBasics;
using BlueControls.Enums;
using System.Collections.Generic;
using System.Drawing;
using static BlueBasics.Extensions;

public static class clsDesignExtensions {

    #region Methods

    // Button.Design.Add(enStates.Standard, enKontur.Rechteck, 0, 0, 0, 0, enHintergrundArt.Solide, 0, "EAEAEA", "", "", enRahmenArt.Solide_1px, "B6B6B6", "", "", "{Name=Calibri, Size=10[K]15}", "");
    public static void Add(this Dictionary<enDesign, Dictionary<enStates, clsDesign>> l, enDesign ds, enStates status, enKontur enKontur, int x1, int y1, int x2, int y2, enHintergrundArt hint, float verlauf, string bc1, string bc2, string bc3, enRahmenArt rahm, string boc1, string boc2, string boc3, string f, string pic) {
        Dictionary<enStates, clsDesign> l2;
        if (l.TryGetValue(ds, out var lg)) {
            l2 = lg;
        } else {
            l2 = new Dictionary<enStates, clsDesign>();
            l.Add(ds, l2);
        }
        l2.Add(status, enKontur, x1, x2, y1, y2, hint, verlauf, bc1, bc2, bc3, rahm, boc1, boc2, boc3, f, pic);
    }

    public static void Add(this Dictionary<enStates, clsDesign> l, enStates status, enKontur enKontur, int x1, int y1, int x2, int y2, enHintergrundArt hint, float verlauf, string bc1, string bc2, string bc3, enRahmenArt rahm, string boc1, string boc2, string boc3, string f, string pic) {
        clsDesign des = new() {
            Need = true,
            Kontur = enKontur,
            X1 = x1,
            Y1 = y1,
            X2 = x2,
            Y2 = y2,
            HintergrundArt = hint,
            Verlauf = verlauf
        };
        if (!string.IsNullOrEmpty(bc1)) { des.BackColor1 = bc1.FromHTMLCode(); }
        if (!string.IsNullOrEmpty(bc2)) { des.BackColor2 = bc2.FromHTMLCode(); }
        if (!string.IsNullOrEmpty(bc3)) { des.BackColor3 = bc3.FromHTMLCode(); }
        des.HintergrundArt = hint;
        des.RahmenArt = rahm;
        if (!string.IsNullOrEmpty(boc1)) { des.BorderColor1 = boc1.FromHTMLCode(); }
        if (!string.IsNullOrEmpty(boc2)) { des.BorderColor2 = boc2.FromHTMLCode(); }
        if (!string.IsNullOrEmpty(boc3)) { des.BorderColor3 = boc3.FromHTMLCode(); }
        if (!string.IsNullOrEmpty(f)) { des.bFont = BlueControls.BlueFont.Get(f); }
        //if (!string.IsNullOrEmpty(pic)) { des.Image = QuickImage.Get(pic); }
        des.Image = pic;
        des.Status = status;
        l.Add(status, des);
    }

    #endregion
}

public class clsDesign {

    #region Fields

    public Color BackColor1;
    public Color BackColor2;
    public Color BackColor3;
    public BlueControls.BlueFont bFont;
    public Color BorderColor1;
    public Color BorderColor2;
    public Color BorderColor3;
    public enHintergrundArt HintergrundArt;
    public string Image;
    public enKontur Kontur;
    public bool Need;
    public enRahmenArt RahmenArt;
    public enStates Status;
    public float Verlauf;
    public int X1;
    public int X2;
    public int Y1;
    public int Y2;

    #endregion
}