// Authors:
// Christian Peter
//
// Copyright (c) 2023 Christian Peter
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
using System.Collections.Generic;
using System.Drawing;

public static class clsDesignExtensions {

    #region Methods

    public static void Add(this Dictionary<Design, Dictionary<States, clsDesign>> dictControl, Design ds, States status, Kontur enKontur, int x1, int y1, int x2, int y2, HintergrundArt hint, float verlauf, string bc1, string bc2, string bc3, RahmenArt rahm, string boc1, string boc2, string boc3, string f, string pic) {
        Dictionary<States, clsDesign> dictState;

        if (dictControl.TryGetValue(ds, out var existingDictOfControl)) {
            dictState = existingDictOfControl;
        } else {
            dictState = new Dictionary<States, clsDesign>();
            dictControl.Add(ds, dictState);
        }

        dictState.Add(status, enKontur, x1, y1, x2, y2, hint, verlauf, bc1, bc2, bc3, rahm, boc1, boc2, boc3, f, pic);
    }

    public static void Add(this Dictionary<States, clsDesign> dictStats, States status, Kontur enKontur, int x1, int y1, int x2, int y2, HintergrundArt hint, float verlauf, string bc1, string bc2, string bc3, RahmenArt rahm, string boc1, string boc2, string boc3, string f, string pic) {
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
        if (!string.IsNullOrEmpty(bc1)) { des.BackColor1 = bc1.FromHtmlCode(); }
        if (!string.IsNullOrEmpty(bc2)) { des.BackColor2 = bc2.FromHtmlCode(); }
        if (!string.IsNullOrEmpty(bc3)) { des.BackColor3 = bc3.FromHtmlCode(); }
        des.HintergrundArt = hint;
        des.RahmenArt = rahm;
        if (!string.IsNullOrEmpty(boc1)) { des.BorderColor1 = boc1.FromHtmlCode(); }
        if (!string.IsNullOrEmpty(boc2)) { des.BorderColor2 = boc2.FromHtmlCode(); }
        if (!string.IsNullOrEmpty(boc3)) { des.BorderColor3 = boc3.FromHtmlCode(); }
        if (!string.IsNullOrEmpty(f)) { des.bFont = BlueFont.Get(f); }
        //if (!string.IsNullOrEmpty(pic)) { des.Image = QuickImage.Get(pic); }
        des.Image = pic;
        des.Status = status;
        dictStats.Add(status, des);
    }

    public static void Remove(this Dictionary<Design, Dictionary<States, clsDesign>> dictControl, Design ds, States status) {
        if (dictControl.TryGetValue(ds, out var existingDictOfControl)) {
            existingDictOfControl.Remove(status);
        }
    }

    #endregion
}

public class clsDesign {

    #region Fields

    public Color BackColor1;
    public Color BackColor2;
    public Color BackColor3;
    public BlueFont bFont;
    public Color BorderColor1;
    public Color BorderColor2;
    public Color BorderColor3;
    public HintergrundArt HintergrundArt;
    public string Image;
    public Kontur Kontur;
    public bool Need;
    public RahmenArt RahmenArt;
    public States Status;
    public float Verlauf;
    public int X1;
    public int X2;
    public int Y1;
    public int Y2;

    #endregion
}