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
using System;
using System.Collections.Generic;
using System.Drawing;
using static BlueBasics.Converter;

namespace BlueControls.CellRenderer;

// ReSharper disable once UnusedMember.Global
public class Renderer_Number : Renderer_Abstract {

    #region Fields

    private int _nachkomma = 2;
    private string _präfix = string.Empty;

    private string _suffix = string.Empty;

    private bool _trennzeichen = true;

    #endregion

    #region Constructors

    public Renderer_Number() : base(false) { }

    #endregion

    #region Properties

    public static string ClassId => "Number";

    public override string Description => "Kann Zahlenwerte formatiert anzeigen.";

    public int Nachkomma {
        get => _nachkomma;
        set {
            if (value < 0) { value = 0; }
            if (value > 5) { value = 5; }

            if (ReadOnly) { Develop.DebugPrint_ReadOnly(); return; }
            _nachkomma = value;
            OnPropertyChanged();
        }
    }

    public string Präfix {
        get => _präfix;
        set {
            if (_präfix == value) { return; }
            if (ReadOnly) { Develop.DebugPrint_ReadOnly(); return; }
            _präfix = value;
            OnPropertyChanged();
        }
    }

    public string Suffix {
        get => _suffix;
        set {
            if (_suffix == value) { return; }
            if (ReadOnly) { Develop.DebugPrint_ReadOnly(); return; }
            _suffix = value;
            OnPropertyChanged();
        }
    }

    public bool Trennzeichen {
        get => _trennzeichen;
        set {
            if (_trennzeichen == value) { return; }
            if (ReadOnly) { Develop.DebugPrint_ReadOnly(); return; }
            _trennzeichen = value;
            OnPropertyChanged();
        }
    }

    #endregion

    #region Methods

    public override void Draw(Graphics gr, string content, Rectangle scaleddrawarea, TranslationType doOpticalTranslation, Alignment align, float scale) {
        if (string.IsNullOrEmpty(content)) { return; }

        var pix16 = (int)(16 * scale);

        var splitedContent = content.SplitAndCutByCrAndBr();

        var y = 0;
        for (var z = 0; z <= splitedContent.GetUpperBound(0); z++) {
            var rect = new Rectangle(scaleddrawarea.Left, scaleddrawarea.Top + y, scaleddrawarea.Width, pix16);

            if (rect.Bottom > scaleddrawarea.Bottom) { break; }

            var replacedText = ValueReadable(splitedContent[z], ShortenStyle.Replaced, doOpticalTranslation);

            Skin.Draw_FormatedText(gr, replacedText, null, align, rect, this.GetFont(scale), false);

            y += pix16;
        }
    }

    public override List<GenericControl> GetProperties(int widthOfControl) {
        List<GenericControl> result =
        [   new FlexiControlForProperty<string>(() => Präfix),
            new FlexiControlForProperty<string>(() => Suffix,Renderer_TextOneLine.Suffixe(), true),
            new FlexiControlForProperty<bool>(() => Trennzeichen),
            new FlexiControlForProperty<int>(() => Nachkomma)
        ];
        return result;
    }

    public override List<string> ParseableItems() {
        List<string> result = [.. base.ParseableItems()];

        result.ParseableAdd("Prefix", _präfix);

        result.ParseableAdd("Suffix", _suffix);

        result.ParseableAdd("Separator", _trennzeichen);
        result.ParseableAdd("DecimalPlaces", _nachkomma);
        return result;
    }

    public override bool ParseThis(string key, string value) {
        switch (key.ToLower()) {
            case "prefix":
                _präfix = value.FromNonCritical();
                return true;

            case "suffix":
                _suffix = value.FromNonCritical();
                return true;

            case "separator":
                _trennzeichen = value.FromPlusMinus();
                return true;

            case "decimalplaces":
                _nachkomma = IntParse(value);
                return true;
        }
        return base.ParseThis(key, value);
    }

    public override string ReadableText() => "Formatierte Zahlen";

    public override QuickImage SymbolForReadableText() => QuickImage.Get(ImageCode.Eins);

    protected override Size CalculateContentSize(string content, TranslationType doOpticalTranslation) {
        //var font = Skin.GetBlueFont(SheetStyle, PadStyles.Standard, States.Standard);

        var contentSize = Size.Empty;

        var splitedContent = content.SplitAndCutByCrAndBr();

        foreach (var thisString in splitedContent) {
            var replacedText = ValueReadable(thisString, ShortenStyle.Replaced, doOpticalTranslation);

            var tmpSize = this.GetFont().FormatedText_NeededSize(replacedText, null, 16);
            contentSize.Width = Math.Max(tmpSize.Width, contentSize.Width);
            contentSize.Height += Math.Max(tmpSize.Height, 16);
        }

        contentSize.Width = Math.Max(contentSize.Width, 16);
        contentSize.Height = Math.Max(contentSize.Height, 16);

        return contentSize;
    }

    /// <summary>
    /// Gibt eine einzelne Zeile richtig ersetzt mit Prä- und Suffix zurück.
    /// </summary>
    /// <param name="content"></param>
    /// <param name="style"></param>
    /// <param name="translate"></param>
    /// <returns></returns>
    protected override string CalculateValueReadable(string content, ShortenStyle style, TranslationType translate) {
        var t_präfix = _präfix;
        var t_suffix = _suffix;

        if (translate != TranslationType.Original_Anzeigen) {
            if (!string.IsNullOrEmpty(_präfix)) { t_präfix = LanguageTool.DoTranslate(_präfix, true); }
            if (!string.IsNullOrEmpty(_suffix)) { t_suffix = LanguageTool.DoTranslate(_suffix, true); }
        }

        var txt = content;

        if (DoubleTryParse(content, out var value)) {
            if (_trennzeichen) {
                txt = value.ToString($"N{_nachkomma}", System.Globalization.CultureInfo.InstalledUICulture);
            } else {
                txt = value.ToString($"F{_nachkomma}", System.Globalization.CultureInfo.InstalledUICulture);
            }
        }

        if (!string.IsNullOrEmpty(t_präfix)) { txt = $"{t_präfix} {txt}"; }
        if (!string.IsNullOrEmpty(t_suffix)) { txt = $"{txt} {t_suffix}"; }

        return txt;
    }

    #endregion
}