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

using BlueBasics;
using BlueBasics.Enums;
using BlueControls.Controls;
using BlueControls.Interfaces;
using BlueTable;
using BlueTable.Enums;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using static BlueBasics.Converter;

namespace BlueControls.CellRenderer;

public class Renderer_DateTime : Renderer_Abstract {

    #region Fields

    private string _format = string.Empty;

    private bool _showSymbol = true;
    private bool _utcToLocal = true;

    #endregion

    #region Properties

    public static string ClassId => "DateTime";

    public override string Description => "Kann Uhrzeit/Datumsangaben verändert anzeigen.";

    public string Format {
        get => _format;
        set {
            if (_format == value) { return; }
            if (ReadOnly) { Develop.DebugPrint_ReadOnly(); return; }
            _format = value;
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

    public bool UTCToLocal {
        get => _utcToLocal;
        set {
            if (_utcToLocal == value) { return; }
            if (ReadOnly) { Develop.DebugPrint_ReadOnly(); return; }
            _utcToLocal = value;
            OnPropertyChanged();
        }
    }

    #endregion

    #region Methods

    public override void Draw(Graphics gr, string content, RowItem? affectingRow, Rectangle scaleddrawarea, TranslationType translate, Alignment align, float scale) {
        if (string.IsNullOrEmpty(content)) { return; }
        //var font = Skin.GetBlueFont(SheetStyle, PadStyles.Standard, States.Standard).Scale(SheetStyleScale);
        var replacedText = ValueReadable(content, ShortenStyle.Replaced, translate);

        QuickImage? qi = null;

        if (_showSymbol) {
            var pix = 14.CanvasToControl(scale);
            if (_utcToLocal) {
                qi = QuickImage.Get(ImageCode.Haus, pix);
            } else {
                qi = QuickImage.Get(ImageCode.Globus, pix);
            }
        }

        Skin.Draw_FormatedText(gr, replacedText, qi, align, scaleddrawarea, this.GetFont(scale), false);
    }

    public override List<GenericControl> GetProperties(int widthOfControl) {
        List<GenericControl> result =
        [   new FlexiControlForProperty<string>(() => Format),
            new FlexiControlForProperty<bool>(() => UTCToLocal),
            new FlexiControlForProperty<bool>(() => ShowSymbol)
        ];
        return result;
    }

    public override List<string> ParseableItems() {
        List<string> result = [.. base.ParseableItems()];
        result.ParseableAdd("Format", _format);
        result.ParseableAdd("UTCToLocal", _utcToLocal);
        result.ParseableAdd("ShowSymbol", _showSymbol);
        return result;
    }

    public override bool ParseThis(string key, string value) {
        switch (key) {
            case "format":
                _format = value.FromNonCritical();
                return true;

            case "utctolocal":
                _utcToLocal = value.FromPlusMinus();
                return true;

            case "showsymbol":
                _showSymbol = value.FromPlusMinus();
                return true;
        }
        return base.ParseThis(key, value);
    }

    public override string ReadableText() => "Datum/Uhrzeit";

    public override QuickImage SymbolForReadableText() => QuickImage.Get(ImageCode.Uhr);

    protected override Size CalculateContentSize(string content, TranslationType doOpticalTranslation) {
        var replacedText = ValueReadable(content, ShortenStyle.Replaced, doOpticalTranslation);
        var contentSize = this.GetFont().FormatedText_NeededSize(replacedText, null, 16);

        if (ShowSymbol) {
            contentSize.Width += 16;
        }

        return contentSize;
    }

    protected override string CalculateValueReadable(string content, ShortenStyle style, TranslationType doOpticalTranslation) {
        //if (string.IsNullOrWhiteSpace(_format) && !_utcToLocal) { return content; }

        if (!DateTimeTryParse(content, out var dt)) { return content; }

        if (_utcToLocal) { dt = dt.ToLocalTime(); }

        if (string.IsNullOrWhiteSpace(_format)) {
            if (doOpticalTranslation == TranslationType.Datum && LanguageTool.Translation != null) { return dt.ToString1(); }
            return dt.ToString5();
        }

        return dt.ToString(_format, CultureInfo.InvariantCulture);
    }

    #endregion
}