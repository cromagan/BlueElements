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

#nullable enable

using System;
using System.Drawing;
using BlueBasics;
using BlueControls.Enums;

namespace BlueControls.Extended_Text;

internal class ExtCharImageCode : ExtChar {

    #region Fields

    private readonly QuickImage? _qi;

    #endregion

    #region Constructors

    public ExtCharImageCode(QuickImage? qi, Design design, States state, BlueFont? font, int stufe) : base(design, state, font, stufe) => _qi = qi;

    public ExtCharImageCode(string imagecode, Design design, States state, BlueFont? font, int stufe) : base(design, state, font, stufe) => _qi = QuickImage.Get(imagecode);

    #endregion

    #region Methods

    public override void Draw(Graphics gr, Point posModificator, float zoom) {
        // Sind es KEINE Integer bei DrawX / DrawY, kommt es zu extrem unschönen Effekten. Gerade Linien scheinen verschwommen zu sein. (Checkbox-Kästchen)

        var drawX = (int)((Pos.X * zoom) + posModificator.X);
        var drawY = (int)((Pos.Y * zoom) + posModificator.Y);

        if (_qi == null) { return; }

        try {
            if (Math.Abs(zoom - 1) < 0.001) {
                gr.DrawImage(_qi, drawX, drawY);
            } else {
                gr.DrawImage(_qi.Scale(zoom), drawX, drawY);
            }
        } catch { }
    }

    public override string HtmlText() {
        if (_qi == null) { return string.Empty; }
        return "<IMAGECODE=" + _qi.Code + ">";
    }

    public override bool IsLineBreak() => false;

    public override bool IsPossibleLineBreak() => true;

    public override bool IsSpace() => false;

    public override bool IsWordSeperator() => true;

    public override string PlainText() => string.Empty;

    protected override SizeF CalculateSize() => _qi == null ? SizeF.Empty : new SizeF(_qi.Width + 1, _qi.Height + 1);

    #endregion
}