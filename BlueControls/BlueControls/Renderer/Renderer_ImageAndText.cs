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

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing;
using System.Linq;
using BlueBasics;
using BlueBasics.Enums;
using BlueControls.Controls;
using BlueControls.Enums;
using BlueControls.ItemCollectionList;
using BlueDatabase;
using BlueDatabase.Enums;
using static BlueControls.ItemCollectionList.AbstractListItemExtension;
using static BlueBasics.Converter;

namespace BlueControls.CellRenderer;

public class Renderer_ImageAndText : Renderer_Abstract {

    #region Fields

    private bool _bild_anzeigen = false;
    private int _constantHeight = 16;
    private int _constantWidth = 16;
    private List<string> _opticalReplace = new();
    // private string _präfix = string.Empty;
    // private string _suffix = string.Empty;
    private bool _text_anzeigen = true;

    #endregion

    #region Constructors

    public Renderer_ImageAndText() : base() { }

    #endregion

    #region Properties

    public static string ClassId => "ImageAndText";

    public bool Bild_anzeigen {
        get => _bild_anzeigen;
        set {
            if (_bild_anzeigen == value) { return; }
            _bild_anzeigen = value;
            OnPropertyChanged();
            OnDoUpdateSideOptionMenu();
        }
    }

    public override string Description => "Kann Bilder mit einem Bild davor anzeigen.";

    public int Konstante_Breite_von_Bildern {
        get => _constantWidth;
        set {
            value = Math.Max(value, 16);
            value = Math.Min(value, 128);
            if (_constantWidth == value) { return; }
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
            _constantHeight = value;
            OnPropertyChanged();
        }
    }

    public override string MyClassId => ClassId;

    //public string Präfix {
    //    get => _präfix;
    //    set {
    //        if (_präfix != value) { return; }
    //        _präfix = value;
    //        OnPropertyChanged();
    //    }
    //}

    //public string Suffix {
    //    get => _suffix;
    //    set {
    //        if (_suffix != value) { return; }
    //        _suffix = value;
    //        OnPropertyChanged();
    //    }
    //}

    public bool Text_anzeigen {
        get => _text_anzeigen;
        set {
            if (_text_anzeigen == value) { return; }
            _text_anzeigen = value;
            OnPropertyChanged();
            OnDoUpdateSideOptionMenu();
        }
    }

    public List<string> Text_ersetzen {
        get => _opticalReplace;
        set {
            if (!_opticalReplace.IsDifferentTo(value)) { return; }
            _opticalReplace = value;
            OnPropertyChanged();
        }
    }

    private BildTextVerhalten BehaviorOfImageAndText {
        get {
            if (_bild_anzeigen && _text_anzeigen) { return BildTextVerhalten.Wenn_möglich_Bild_und_immer_Text; }

            if (_text_anzeigen) { return BildTextVerhalten.Nur_Text; }

            return BildTextVerhalten.Nur_Bild;
        }
    }

    #endregion

    #region Methods

    public override void Draw(Graphics gr, string content, Rectangle drawarea, Design design, States state, BildTextVerhalten behaviorOfImageAndText, TranslationType doOpticalTranslation, ReadOnlyCollection<string> opticalReplace, string constantHeightOfImageCode, float scale, Alignment align) {
        if (string.IsNullOrEmpty(content)) { return; }
        var font = Skin.DesignOf(design, state).BFont?.Scale(scale);
        if (font == null) { return; }

        var pix16 = Table.GetPix(16, scale);

        var maxW = Table.GetPix(_constantWidth, scale);
        var constH = Table.GetPix(_constantHeight, scale);

        var splitedContent = content.SplitAndCutByCrAndBr();

        var y = 0;
        for (var z = 0; z <= splitedContent.GetUpperBound(0); z++) {
            var rect = new Rectangle(drawarea.Left, drawarea.Top + y, drawarea.Width, pix16);

            if (rect.Bottom > drawarea.Bottom) { break; }

            var image = GetImage(splitedContent[z], maxW, constH);

            var replacedText = CalculateValueReadable(splitedContent[z], ShortenStyle.Replaced, BehaviorOfImageAndText, false, doOpticalTranslation, _opticalReplace.AsReadOnly());

            if (rect.Bottom + pix16 > drawarea.Bottom && z < splitedContent.GetUpperBound(0)) {
                replacedText = "...";
                image = null;
            }

            Skin.Draw_FormatedText(gr, replacedText, image, align, rect, font, false);

            if (image != null) {
                y += Math.Max(image.Height, pix16);
            } else {
                y += pix16;
            }
        }
    }

    public override List<GenericControl> GetProperties(int widthOfControl) {
        var cbxEinheit = new List<AbstractListItem>
        {
            ItemOf("µm", ImageCode.Lineal),
            ItemOf("mm", ImageCode.Lineal),
            ItemOf("cm", ImageCode.Lineal),
            ItemOf("dm", ImageCode.Lineal),
            ItemOf("m", ImageCode.Lineal),
            ItemOf("km", ImageCode.Lineal),
            ItemOf("mm²", ImageCode.GrößeÄndern),
            ItemOf("m²", ImageCode.GrößeÄndern),
            ItemOf("µg", ImageCode.Gewicht),
            ItemOf("mg", ImageCode.Gewicht),
            ItemOf("g", ImageCode.Gewicht),
            ItemOf("kg", ImageCode.Gewicht),
            ItemOf("t", ImageCode.Gewicht),
            ItemOf("h", ImageCode.Uhr),
            ItemOf("min", ImageCode.Uhr),
            ItemOf("St.", ImageCode.Eins)
        };

        List<GenericControl> result =
        [
            new FlexiControlForProperty<bool>(() =>  Text_anzeigen)
        ];

        if (Text_anzeigen) {
            //result.Add(new FlexiControlForProperty<string>(() => Präfix));
            //result.Add(new FlexiControlForProperty<string>(() => Suffix, cbxEinheit, true));
            result.Add(new FlexiControlForProperty<List<string>>(() => Text_ersetzen, 5));
        }

        result.Add(new FlexiControlForProperty<bool>(() => Bild_anzeigen));

        if (Bild_anzeigen) {
            result.Add(new FlexiControlForProperty<int>(() => Konstante_Höhe_von_Bildern));
            result.Add(new FlexiControlForProperty<int>(() => Konstante_Breite_von_Bildern));
        }

        return result;
    }

    public override bool ParseThis(string key, string value) {
        switch (key.ToLower()) {
            //case "prefix":
            //    _präfix = value.FromNonCritical();
            //    return true;

            //case "suffix":
            //    _suffix = value.FromNonCritical();
            //return true;
            case "showpic":
                _bild_anzeigen = value.FromPlusMinus();
                return true;

            case "showtext":
                _text_anzeigen = value.FromPlusMinus();
                return true;

            case "replace":
                _opticalReplace = value.SplitBy("|").ToList().FromNonCritical();
                return true;


            case "imagewidth":
                _constantWidth = IntParse(value.FromNonCritical());
                return true;


            case "imageheight":
                _constantHeight = IntParse(value.FromNonCritical());
                return true;

        }
        return true; // Immer true. So kann gefahrlos hin und her geschaltet werden und evtl. Werte aus anderen Renderen benutzt werden.
    }

    public override string ReadableText() => "Standard";

    public override QuickImage? SymbolForReadableText() => null;

    public override string ToParseableString() {
        List<string> result = [];

        result.ParseableAdd("ShowPic", _bild_anzeigen);
        result.ParseableAdd("ShowText", _text_anzeigen);

        //result.ParseableAdd("Prefix", _präfix);

        //result.ParseableAdd("Suffix", _suffix);

        result.ParseableAdd("Replace", _opticalReplace, true);

        result.ParseableAdd("ImageWidth", _constantWidth);
        result.ParseableAdd("ImageHeight", _constantHeight);

        return result.Parseable(base.ToParseableString());
    }

    /// <summary>
    /// Gibt eine einzelne Zeile richtig ersetzt mit Prä- und Suffix zurück.
    /// </summary>
    /// <param name="content"></param>
    /// <param name="style"></param>
    /// <param name="bildTextverhalten"></param>
    /// <param name="removeLineBreaks">bei TRUE werden Zeilenumbrüche mit Leerzeichen ersetzt</param>
    /// <param name="doOpticalTranslation"></param>
    /// <param name="opticalReplace"></param>
    /// <returns></returns>
    protected override string CalculateValueReadable(string content, ShortenStyle style, BildTextVerhalten bildTextverhaltenx, bool removeLineBreaksx, TranslationType doOpticalTranslation, ReadOnlyCollection<string> opticalReplacex) {
        //if (_bildTextverhalten == BildTextVerhalten.Nur_Bild && style != ShortenStyle.HTML) { return string.Empty; }

        content = LanguageTool.PrepaireText(content, style, string.Empty, string.Empty, doOpticalTranslation, _opticalReplace.AsReadOnly());

        if (style != ShortenStyle.HTML) { return content; }

        content = content.Replace("\r\n", "; ");
        content = content.Replace("\r", "; ");

        return content;
    }

    /// <summary>
    /// Status des Bildes (Disabled) wird geändert. Diese Routine sollte nicht innerhalb der Table Klasse aufgerufen werden.
    /// Sie dient nur dazu, das Aussehen eines Textes wie eine Zelle zu imitieren.
    /// </summary>
    ///
    protected override Size CalculateContentSize(string content, Design design, States state, BildTextVerhalten behaviorOfImageAndText, TranslationType doOpticalTranslation, ReadOnlyCollection<string> opticalReplace, string constantHeightOfImageCode) {
        var font = Skin.DesignOf(design, state).BFont?.Font();

        if (font == null) { return new Size(16, 16); }

        var contentSize = Size.Empty;

        var splitedContent = content.SplitAndCutByCrAndBr();

        foreach (var thisString in splitedContent) {
            var image = GetImage(thisString, _constantWidth, _constantHeight);

            var replacedText = CalculateValueReadable(thisString, ShortenStyle.Replaced, BehaviorOfImageAndText, false, doOpticalTranslation, opticalReplace);

            var tmpSize = font.FormatedText_NeededSize(replacedText, image, 16);
            contentSize.Width = Math.Max(tmpSize.Width, contentSize.Width);
            contentSize.Height += Math.Max(tmpSize.Height, 16);
        }

        contentSize.Width = Math.Max(contentSize.Width, 16);
        contentSize.Height = Math.Max(contentSize.Height, 16);

        return contentSize;
    }

    private QuickImage? GetImage(string name, int constw, int consth) {
        if (!_bild_anzeigen || string.IsNullOrEmpty(name)) { return null; }

        var i = QuickImage.Get(QuickImage.GenerateCode(name, constw, consth, ImageCodeEffect.Ohne, string.Empty, string.Empty, 100, 100, 0, 0, string.Empty));

        if (i.IsError) { return null; }
        return i;

    }

    #endregion
}