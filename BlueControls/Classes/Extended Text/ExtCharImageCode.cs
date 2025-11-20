// Authors:
// Christian Peter
//
// Copyright (c) 2025 Christian Peter
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
using System.Collections.Generic;
using System.Drawing;
using static BlueBasics.Constants;

namespace BlueControls.Extended_Text;

internal class ExtCharImageCode : ExtChar {

    #region Fields

    private QuickImage? _qi;

    #endregion

    #region Constructors

    public ExtCharImageCode(ExtText parent, int styleFromPos) : base(parent, styleFromPos) { }

    public ExtCharImageCode(ExtText parent, PadStyles style, BlueFont font, QuickImage? qi) : base(parent, style, font) => _qi = qi;

    public ExtCharImageCode(ExtText parent, int styleFromPos, QuickImage? qi) : base(parent, styleFromPos) => _qi = qi;

    #endregion

    #region Properties

    public static string ClassId => "ExtCharImageCode";

    #endregion

    //public ExtCharImageCode(ExtText parent, PadStyles style, BlueFont font, string imagecode) : base(parent, style, font) => _qi = QuickImage.Get(imagecode);

    #region Methods

    //internal ExtCharImageCode(ExtText parent, int styleFromPos, string imagecode) : base(parent, styleFromPos) => _qi = QuickImage.Get(imagecode);
    public override void Draw(Graphics gr, Point posModificator, float scale) {
        // Sind es KEINE Integer bei DrawX / DrawY, kommt es zu extrem unschönen Effekten. Gerade Linien scheinen verschwommen zu sein. (Checkbox-Kästchen)

        var drawX = (int)((Pos.X * scale) + posModificator.X);
        var drawY = (int)((Pos.Y * scale) + posModificator.Y);

        if (_qi == null) { return; }

        try {
            gr.DrawImage(Math.Abs(scale - 1) < DefaultTolerance ? _qi : _qi.Scale(scale), drawX, drawY);
        } catch { }
    }

    public override string HtmlText() => _qi == null ? string.Empty : "<imagecode=" + _qi.Code + ">";

    public override bool IsLineBreak() => false;

    public override bool IsPossibleLineBreak() => true;

    public override bool IsSpace() => false;

    public override bool IsWordSeparator() => true;

    public override List<string> ParseableItems() {
        if (IsDisposed) { return []; }
        List<string> result = [.. base.ParseableItems()];
        result.ParseableAdd("Image", _qi?.ToString() ?? string.Empty);

        return result;
    }

    public override bool ParseThis(string key, string value) {
        switch (key) {
            case "image":
                _qi = QuickImage.Get(value.FromNonCritical());
                return true;
        }
        return base.ParseThis(key, value);
    }

    public override string PlainText() => string.Empty;

    protected override SizeF CalculateSize() => _qi == null ? SizeF.Empty : new SizeF(_qi.Width + 1, _qi.Height + 1);

    #endregion
}