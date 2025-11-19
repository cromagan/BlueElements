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

    public ExtCharAscii(ExtText parent, int styleFromPos) : base(parent, styleFromPos) {
    }

    internal ExtCharAscii(ExtText parent, PadStyles style, BlueFont font, char charcode) : base(parent, style, font) {
        _char = charcode;
        InitVales();
    }

    internal ExtCharAscii(ExtText parent, int styleFromPos, char charcode) : base(parent, styleFromPos) {
        _char = charcode;
        InitVales();
    }

    #endregion

    #region Properties

    public static string ClassId => "ExtCharAscii";

    #endregion

    #region Methods

    public override void Draw(Graphics gr, Point posModificator, float scale) {
        if (_charInt < 20) { return; }
        var drawX = (Pos.X * scale) + posModificator.X;
        var drawY = (Pos.Y * scale) + posModificator.Y;

        try {
            this.GetFont().DrawString(gr, _charString, drawX, drawY, scale);
        } catch { }
    }

    public override string HtmlText() => _htmlText;

    public override bool IsLineBreak() => _isLineBreak;

    public override bool IsPossibleLineBreak() => _isPossibleLineBreak;

    public override bool IsSpace() => _isSpace;

    public override bool IsWordSeparator() => _isWordSeparator;

    public override List<string> ParseableItems() {
        if (IsDisposed) { return []; }
        List<string> result = [.. base.ParseableItems()];
        result.ParseableAdd("Char", _charString);

        return result;
    }

    public override void ParseFinished(string parsed) {
        base.ParseFinished(parsed);
        InitVales();
    }

    public override bool ParseThis(string key, string value) {
        switch (key) {
            case "char":

                var s = value.FromNonCritical();

                if (string.IsNullOrEmpty(s)) {
                    _char = '?';
                } else {
                    _char = s[0];
                }

                return true;
        }
        return base.ParseThis(key, value);
    }

    public override string PlainText() => _charString;

    protected override SizeF CalculateSize() => Font == null ? new SizeF(0, 16) : _char < 0 ? Font.CharSize(0f) : Font.CharSize(_char);

    private void InitVales() {
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