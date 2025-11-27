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
using BlueBasics.Enums;
using BlueControls.Controls;
using BlueControls.Interfaces;
using BlueTable;
using BlueTable.Enums;
using System.Collections.Generic;
using System.Drawing;
using static BlueBasics.Converter;

namespace BlueControls.CellRenderer;

public class Renderer_Color : Renderer_Abstract {

    #region Fields

    private bool _showHex = true;
    private bool _showName = true;
    private bool _showSymbol = true;

    #endregion

    #region Properties

    public static string ClassId => "Color";

    public override string Description => "Kann Hex-Farbcode (RGB oder ARGB) anzeigen (z.B. #ff0000) ";

    public bool ShowHex {
        get => _showHex;
        set {
            if (_showHex == value) { return; }
            if (ReadOnly) { Develop.DebugPrint_ReadOnly(); return; }
            _showHex = value;
            OnPropertyChanged();
        }
    }

    public bool ShowName {
        get => _showName;
        set {
            if (_showName == value) { return; }
            if (ReadOnly) { Develop.DebugPrint_ReadOnly(); return; }
            _showName = value;
            OnPropertyChanged();
        }
    }

    public bool ShowSymbol {
        get => _showSymbol;
        set {
            if (_showSymbol == value) { return; }
            if (ReadOnly) { Develop.DebugPrint_ReadOnly(); return; }
            _showSymbol = value;
            OnPropertyChanged();
        }
    }

    #endregion

    #region Methods

    public override void Draw(Graphics gr, string content, RowItem? affectingRow, Rectangle scaleddrawarea, TranslationType translate, Alignment align, float scale) {
        if (string.IsNullOrEmpty(content)) { return; }
        //var font = Skin.GetBlueFont(SheetStyle, PadStyles.Standard, States.Standard).Scale(SheetStyleScale);
        QuickImage? qi = null;
        var pix = (int)(16 * scale);

        var replacedText = content;
        if (ColorTryParse(content, out var col)) {
            replacedText = ValueReadable(content, ShortenStyle.Replaced, translate);

            if (_showSymbol) {
                qi = QuickImage.Get(ImageCode.Kreis, pix, Color.Transparent, col);
            }
        }

        if (_showSymbol && qi == null) { qi = QuickImage.Get(ImageCode.Fragezeichen, pix); }

        Skin.Draw_FormatedText(gr, replacedText, qi, align, scaleddrawarea, this.GetFont(scale), false);
    }

    public override List<GenericControl> GetProperties(int widthOfControl) {
        List<GenericControl> result =
        [   new FlexiControlForProperty<bool>(() => ShowSymbol),
            new FlexiControlForProperty<bool>(() => ShowHex),
            new FlexiControlForProperty<bool>(() => ShowName)
        ];
        return result;
    }

    public override List<string> ParseableItems() {
        List<string> result = [.. base.ParseableItems()];
        result.ParseableAdd("ShowSymbol", _showSymbol);
        result.ParseableAdd("ShowHex", _showHex);
        result.ParseableAdd("ShowName", _showName);
        return result;
    }

    public override bool ParseThis(string key, string value) {
        switch (key) {
            case "showsymbol":
                _showSymbol = value.FromPlusMinus();
                return true;

            case "showhex":
                _showHex = value.FromPlusMinus();
                return true;

            case "showname":
                _showName = value.FromPlusMinus();
                return true;
        }
        return base.ParseThis(key, value);
    }

    public override string ReadableText() => "Farbanzeige";

    public override QuickImage SymbolForReadableText() => QuickImage.Get(ImageCode.Farbrad);

    protected override Size CalculateContentSize(string content, TranslationType doOpticalTranslation) {
        var replacedText = ValueReadable(content, ShortenStyle.Replaced, doOpticalTranslation);
        var contentSize = this.GetFont().FormatedText_NeededSize(replacedText, null, 16);

        if (ShowSymbol) {
            contentSize.Width += 18;
        }

        return contentSize;
    }

    protected override string CalculateValueReadable(string content, ShortenStyle style, TranslationType doOpticalTranslation) {
        if (!_showHex && !_showName) { return string.Empty; }

        if (!ColorTryParse(content, out var col)) { return content; }

        if (_showName && _showHex) {
            var n = col.Name();

            if (string.IsNullOrEmpty(n)) { return col.ToHtmlCode(); }
            return $"{col.ToHtmlCode()} {n}";
        } else if (_showName) {
            var n = col.Name();
            if (!string.IsNullOrEmpty(n)) { return n; }
        }

        return col.ToHtmlCode();
    }

    #endregion
}