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
using BlueControls.Interfaces;
using System.Drawing;

namespace BlueControls.Extended_Text;

public class ExtCharAscii : ExtChar {

    #region Fields

    private readonly SizeF _calculatedSize;
    private readonly char _char;
    private readonly int _charInt;
    private readonly string _charString;
    private readonly string _htmlText;
    private readonly bool _isLineBreak;
    private readonly bool _isPossibleLineBreak;
    private readonly bool _isSpace;
    private readonly bool _isWordSeparator;

    #endregion

    #region Constructors

    internal ExtCharAscii(ExtText parent, PadStyles style, BlueFont font, char charcode) : base(parent, style, font) {
        _char = charcode;
        _charInt = (int)charcode;
        _charString = charcode.ToString();
        _htmlText = _charString.CreateHtmlCodes();
        _isLineBreak = _charInt is 11 or 13;
        _isPossibleLineBreak = Constants.PossibleLineBreaks.Contains(_char);
        _isSpace = _charInt is 32 or 0 or 9;
        _isWordSeparator = Constants.WordSeparators.Contains(_char);
        _calculatedSize = CalculateSize();
    }

    internal ExtCharAscii(ExtText parent, int styleFromPos, char charcode) : base(parent, styleFromPos) {
        _char = charcode;
        _charInt = (int)charcode;
        _charString = charcode.ToString();
        _htmlText = _charString.CreateHtmlCodes();
        _isLineBreak = _charInt is 11 or 13;
        _isPossibleLineBreak = Constants.PossibleLineBreaks.Contains(_char);
        _isSpace = _charInt is 32 or 0 or 9;
        _isWordSeparator = Constants.WordSeparators.Contains(_char);
        _calculatedSize = CalculateSize();
    }

    #endregion

    #region Methods

    public override void Draw(Graphics gr, Point posModificator, float zoom) {
        if (_charInt < 20) { return; }
        var drawX = (Pos.X * zoom) + posModificator.X;
        var drawY = (Pos.Y * zoom) + posModificator.Y;

        try {
            this.GetFont().DrawString(gr, _charString, drawX, drawY, zoom, StringFormat.GenericTypographic);
        } catch { }
    }

    public override string HtmlText() => _htmlText;

    public override bool IsLineBreak() => _isLineBreak;

    public override bool IsPossibleLineBreak() => _isPossibleLineBreak;

    public override bool IsSpace() => _isSpace;

    public override bool IsWordSeparator() => _isWordSeparator;

    public override string PlainText() => _charString;

    protected override SizeF CalculateSize() => Font == null ? new SizeF(0, 16) : _char < 0 ? Font.CharSize(0f) : Font.CharSize(_char);

    #endregion
}