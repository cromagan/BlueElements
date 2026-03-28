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
using BlueBasics.ClassesStatic;
using BlueControls.Classes;
using BlueControls.Enums;
using System.Collections.Generic;
using System.Drawing;

namespace BlueControls.Extended_Text;

public class ExtCharAscii : ExtChar {

    #region Fields

    private char _char;
    private int _charInt;
    private string _charString = string.Empty;
    private string _htmlText = string.Empty;
    private bool _isLineBreak;
    private bool _isPossibleLineBreak;
    private bool _isSpace;
    private bool _isWordSeparator;

    #endregion

    #region Constructors

    public ExtCharAscii(ExtText parent, int styleFromPos) : base(parent, styleFromPos) { }

    internal ExtCharAscii(ExtText parent, PadStyles style, List<string> overrideTags, char charcode) : base(parent, style, overrideTags) {
        _char = charcode;
        InitValues();
    }

    internal ExtCharAscii(ExtText parent, int styleFromPos, char charcode) : base(parent, styleFromPos) {
        _char = charcode;
        InitValues();
    }

    #endregion

    #region Properties

    public static string ClassId => "ExtCharAscii";

    #endregion

    #region Methods

    public override void Draw(Graphics gr, Point controlPos, Size controlSize, float zoom) {
        if (_charInt < 20) { return; }
        try {
            Font?.DrawString(gr, _charString, zoom, controlPos.X, controlPos.Y);
        } catch { }
    }

    public override string HtmlText() => _htmlText;

    public override bool IsLineBreak() => _isLineBreak;

    public override bool IsPossibleLineBreak() => _isPossibleLineBreak;

    public override bool IsSpace() => _isSpace;

    public override bool IsWordSeparator() => _isWordSeparator;

    public override string PlainText() => _charString;

    internal override void DrawWithFont(Graphics gr, Point controlPos, Size controlSize, float zoom, BlueFont font) {
        if (_charInt < 20) { return; }
        try {
            font.DrawString(gr, _charString, zoom, controlPos.X, controlPos.Y);
        } catch { }
    }

    protected override SizeF CalculateSizeCanvas() => Font == null ? new SizeF(0, 16) : _char < 0 ? Font.CharSize(0f) : Font.CharSize(_char);

    private void InitValues() {
        _charInt = _char;
        _charString = _char.ToString();
        _htmlText = _charString.CreateHtmlCodes();
        _isLineBreak = _charInt is 11 or 13;
        _isPossibleLineBreak = Constants.PossibleLineBreaks.Contains(_char);
        _isSpace = _charInt is 32 or 0 or 9;
        _isWordSeparator = Constants.WordSeparators.Contains(_char);
    }

    #endregion
}