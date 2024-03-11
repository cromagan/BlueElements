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

using BlueBasics;
using BlueControls.Enums;
using System;
using System.Drawing;

namespace BlueControls.Extended_Text;

public class ExtCharAscii : ExtChar {
    //public const char StoreX = (char)5;
    //public const char Top = (char)4;

    #region Fields

    private readonly char _char;

    #endregion

    #region Constructors

    internal ExtCharAscii(char charcode, Design design, States state, BlueFont? font, int stufe, MarkState markState) : base(design, state, font, stufe) {
        _char = charcode;
        Marking = markState;
    }

    #endregion

    #region Properties

    public int Char => _char;

    #endregion

    #region Methods

    public override void Draw(Graphics gr, Point posModificator, float zoom) {
        if (_char < 20) { return; }
        var drawX = (Pos.X * zoom) + posModificator.X;
        var drawY = (Pos.Y * zoom) + posModificator.Y;

        try {
            Font?.DrawString(gr, _char.ToString(), drawX, drawY, zoom, StringFormat.GenericTypographic);
        } catch { }

        //if (Math.Abs(zoom - 1) < DefaultTolerance) {
        //    var BNR = QuickImage.Get(_Char - (int)enASCIIKey.ImageStart);
        //    if (BNR == null) { return; }
        //    // Sind es KEINE Integer bei DrawX / DrawY, kommt es zu extrem unschönen Effekten. Gerade Linien scheinen verschwommen zu sein. (Checkbox-Kästchen)
        //    gr.DrawImage(BNR, (int)DrawX, (int)DrawY);
        //} else {
        //    var l = QuickImage.Get(_Char - (int)enASCIIKey.ImageStart);
        //    if (l == null || l.Width == 0) { l = QuickImage.Get("Warnung|16"); }
        //    if (l.Width > 0) {
        //        gr.DrawImage(QuickImage.Get(l.Name, (int)(l.Width * zoom)), (int)DrawX, (int)DrawY);
        //    }
        //}
    }

    public override string HtmlText() => Convert.ToChar(_char).ToString().CreateHtmlCodes(false);

    public override bool IsLineBreak() => (int)_char is 11 or 13;

    public override bool IsPossibleLineBreak() => _char.IsPossibleLineBreak();

    public override bool IsSpace() => (int)_char is 32 or 0 or 9;

    public override bool IsWordSeperator() => _char.IsWordSeperator();

    public override string PlainText() => Convert.ToChar(_char).ToString();

    protected override SizeF CalculateSize() => Font == null ? new SizeF(0, 16) : _char < 0 ? Font.CharSize(0f) : Font.CharSize(_char);

    #endregion
}