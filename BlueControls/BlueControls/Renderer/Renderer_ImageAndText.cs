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
using BlueDatabase;
using BlueDatabase.Enums;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using static BlueBasics.Converter;

namespace BlueControls.CellRenderer;

// ReSharper disable once UnusedMember.Global
public class Renderer_ImageAndText : Renderer_Abstract {

    #region Fields

    private bool _bild_anzeigen;
    private int _constantHeight = 16;
    private int _constantWidth = 16;
    private string _defaultImage = string.Empty;
    private List<string> _imagereplacement = [];
    private string _imgpräfix = string.Empty;
    private List<string> _opticalReplace = [];
    private bool _text_anzeigen = true;

    #endregion

    #region Constructors

    public Renderer_ImageAndText() : base(false) { }

    public Renderer_ImageAndText(string imageReplacement) : base(true) {
        _bild_anzeigen = true;
        _text_anzeigen = false;
        _imagereplacement = imageReplacement.SplitAndCutByCr().ToList();
    }

    #endregion

    #region Properties

    public static string ClassId => "ImageAndText";

    public bool Bild_anzeigen {
        get => _bild_anzeigen;
        set {
            if (_bild_anzeigen == value) { return; }
            if (ReadOnly) { Develop.DebugPrint_ReadOnly(); return; }
            _bild_anzeigen = value;
            OnPropertyChanged();
            OnDoUpdateSideOptionMenu();
        }
    }

    public string Bild_ersetzen {
        get => _imagereplacement.JoinWithCr();
        set {
            var old = Bild_ersetzen;
            if (string.Equals(old, value, StringComparison.OrdinalIgnoreCase)) { return; }
            if (ReadOnly) { Develop.DebugPrint_ReadOnly(); return; }
            _imagereplacement = value.SplitByCr().ToList();
            OnPropertyChanged();

            if (string.IsNullOrEmpty(old) != string.IsNullOrEmpty(value)) {
                OnDoUpdateSideOptionMenu();
            }
        }
    }

    public string Bild_Präfix {
        get => _imgpräfix;
        set {
            if (_imgpräfix == value) { return; }
            if (ReadOnly) { Develop.DebugPrint_ReadOnly(); return; }

            var vc = string.IsNullOrEmpty(_imgpräfix) == string.IsNullOrEmpty(value);

            _imgpräfix = value;
            OnPropertyChanged();

            if (!vc) {
                OnDoUpdateSideOptionMenu();
            }
        }
    }

    public override string Description => "Kann Bilder mit einem Bild davor anzeigen.";

    public int Konstante_Breite_von_Bildern {
        get => _constantWidth;
        set {
            value = Math.Max(value, 16);
            value = Math.Min(value, 128);
            if (_constantWidth == value) { return; }
            if (ReadOnly) { Develop.DebugPrint_ReadOnly(); return; }
            _constantWidth = value;
            OnPropertyChanged();
        }
    }

    public int Konstante_Höhe_von_Bildern {
        get => _constantHeight;
        set {
            value = Math.Max(value, 16);
            value = Math.Min(value, 128);
            if (_constantHeight == value) { return; }
            if (ReadOnly) { Develop.DebugPrint_ReadOnly(); return; }
            _constantHeight = value;
            OnPropertyChanged();
        }
    }

    public string Standard_Bild {
        get => _defaultImage;
        set {
            if (_defaultImage == value) { return; }
            if (ReadOnly) { Develop.DebugPrint_ReadOnly(); return; }
            _defaultImage = value;
            OnPropertyChanged();
        }
    }

    public bool Text_anzeigen {
        get => _text_anzeigen;
        set {
            if (_text_anzeigen == value) { return; }
            _text_anzeigen = value;
            OnPropertyChanged();
            OnDoUpdateSideOptionMenu();
        }
    }

    public string Text_ersetzen {
        get => _opticalReplace.JoinWithCr();
        set {
            var old = Text_ersetzen;
            if (string.Equals(old, value, StringComparison.OrdinalIgnoreCase)) { return; }

            _opticalReplace = value.SplitByCr().ToList();

            OnPropertyChanged();

            if (string.IsNullOrEmpty(old) != string.IsNullOrEmpty(value)) {
                OnDoUpdateSideOptionMenu();
            }
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

            var image = GetImage(splitedContent[z], _constantWidth, _constantHeight)?.Scale(scale);

            var replacedText = ValueReadable(splitedContent[z], ShortenStyle.Replaced, doOpticalTranslation);

            if (rect.Bottom + pix16 > scaleddrawarea.Bottom && z < splitedContent.GetUpperBound(0)) {
                replacedText = "...";
                image = null;
            }

            Skin.Draw_FormatedText(gr, replacedText, image, align, rect, this.GetFont(scale), false);

            if (image != null) {
                y += Math.Max(image.Height, pix16);
            } else {
                y += pix16;
            }
        }
    }

    public override List<GenericControl> GetProperties(int widthOfControl) {
        List<GenericControl> result =
        [
            new FlexiControlForProperty<bool>(() =>  Text_anzeigen)
        ];

        if (Text_anzeigen) {
            //result.Add(new FlexiControlForProperty<string>(() => Präfix));
            //result.Add(new FlexiControlForProperty<string>(() => Suffix, cbxEinheit, true));
            result.Add(new FlexiControlForProperty<string>(() => Text_ersetzen, 5));
        }

        result.Add(new FlexiControlForProperty<bool>(() => Bild_anzeigen));

        if (Bild_anzeigen) {
            result.Add(new FlexiControlForProperty<int>(() => Konstante_Breite_von_Bildern));
            result.Add(new FlexiControlForProperty<int>(() => Konstante_Höhe_von_Bildern));
            result.Add(new FlexiControlForProperty<string>(() => Standard_Bild));
            if (string.IsNullOrEmpty(Bild_ersetzen)) {
                result.Add(new FlexiControlForProperty<string>(() => Bild_Präfix));
            }

            if (string.IsNullOrEmpty(Bild_Präfix)) {
                result.Add(new FlexiControlForProperty<string>(() => Bild_ersetzen, 5));
            }
        }

        return result;
    }

    public override List<string> ParseableItems() {
        List<string> result = [.. base.ParseableItems()];

        result.ParseableAdd("ShowPic", _bild_anzeigen);
        result.ParseableAdd("ShowText", _text_anzeigen);

        // nur wenn Text angezeigt wird. Hilf berechnungen (durch Erkennung) zu reduzieren
        if (_text_anzeigen) {
            result.ParseableAdd("TextReplace", _opticalReplace, true);
        }

        // nur wenn Bild angezeigt wird. Hilf berechnungen (durch Erkennung) zu reduzieren
        if (_bild_anzeigen) {
            result.ParseableAdd("ImagePrefix", _imgpräfix);

            result.ParseableAdd("ImageReplace", _imagereplacement, true);

            result.ParseableAdd("ImageWidth", _constantWidth);
            result.ParseableAdd("ImageHeight", _constantHeight);
            result.ParseableAdd("DefaultImage", _defaultImage);
        }

        return result;
    }

    public override bool ParseThis(string key, string value) {
        switch (key.ToLower()) {
            case "defaultimage":
                _defaultImage = value.FromNonCritical();
                return true;

            case "imageprefix":
                _imgpräfix = value.FromNonCritical();
                return true;

            case "showpic":
                _bild_anzeigen = value.FromPlusMinus();
                return true;

            case "showtext":
                _text_anzeigen = value.FromPlusMinus();
                return true;

            case "replace":
            case "textreplace":
                _opticalReplace = value.SplitBy("|").ToList().FromNonCritical();
                return true;

            case "imagereplace":
                _imagereplacement = value.SplitBy("|").ToList().FromNonCritical();
                return true;

            case "imagewidth":
                _constantWidth = IntParse(value.FromNonCritical());
                return true;

            case "imageheight":
                _constantHeight = IntParse(value.FromNonCritical());
                return true;
        }
        return base.ParseThis(key, value);
    }

    public override string ReadableText() => "Standard";

    public override QuickImage SymbolForReadableText() => QuickImage.Get(ImageCode.Textfeld);

    /// <summary>
    /// Status des Bildes (Disabled) wird geändert. Diese Routine sollte nicht innerhalb der Table Klasse aufgerufen werden.
    /// Sie dient nur dazu, das Aussehen eines Textes wie eine Zelle zu imitieren.
    /// </summary>
    ///
    protected override Size CalculateContentSize(string content, TranslationType doOpticalTranslation) {
        //var font = Skin.GetBlueFont(SheetStyle, PadStyles.Standard, States.Standard);

        var contentSize = Size.Empty;

        var splitedContent = content.SplitAndCutByCrAndBr();

        foreach (var thisString in splitedContent) {
            var image = GetImage(thisString, _constantWidth, _constantHeight);

            var replacedText = ValueReadable(thisString, ShortenStyle.Replaced, doOpticalTranslation);

            var tmpSize = this.GetFont().FormatedText_NeededSize(replacedText, image, 16);
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
        if (!_text_anzeigen) { return string.Empty; }

        content = LanguageTool.PrepaireText(content, style, string.Empty, string.Empty, translate, _opticalReplace.AsReadOnly());

        if (style != ShortenStyle.HTML) { return content; }

        content = content.Replace("\r\n", "; ");
        content = content.Replace("\r", "; ");

        return content;
    }

    private QuickImage? GetImage(string name, int constw, int consth) {
        if (!_bild_anzeigen || string.IsNullOrEmpty(name)) { return null; }

        if (_imagereplacement.Count > 0) {
            foreach (var image in _imagereplacement) {
                if (image.Contains("|")) {
                    var t = image.SplitBy("|");
                    if (string.Equals(t[0], name, StringComparison.OrdinalIgnoreCase)) {
                        name = t[1];
                        break;
                    }
                }
            }
        }

        var i = QuickImage.Get(QuickImage.GenerateCode(_imgpräfix + name, constw, consth, ImageCodeEffect.Ohne, string.Empty, string.Empty, 100, 100, 0, 0, string.Empty));

        if (i.IsError) {
            if (!string.IsNullOrEmpty(_defaultImage)) {
                i = QuickImage.Get(QuickImage.GenerateCode(_defaultImage, constw, consth, ImageCodeEffect.Ohne, string.Empty, string.Empty, 100, 100, 0, 0, string.Empty));
                if (!i.IsError) { return i; }
            }
            return null;
        }
        return i;
    }

    #endregion
}