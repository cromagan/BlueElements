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
using BlueBasics;
using BlueBasics.Enums;
using BlueControls.Controls;
using BlueControls.Enums;
using BlueControls.ItemCollectionList;
using BlueDatabase;
using BlueDatabase.Enums;
using static BlueControls.ItemCollectionList.AbstractListItemExtension;

namespace BlueControls.CellRenderer;

public class Renderer_Default : Renderer_Abstract {

    #region Fields

    private string _präfix = string.Empty;

    private string _suffix = string.Empty;

    #endregion

    #region Constructors

    public Renderer_Default(string keyname) : base(keyname) { }

    #endregion

    #region Properties

    public static string ClassId => "Default";
    public override string Description => "ALT!!! Standard Anzeige";
    public override string MyClassId => ClassId;

    public string Präfix {
        get => _präfix;
        set {
            if (_präfix != value) { return; }
            _präfix = value;
            OnPropertyChanged();
        }
    }

    public string Suffix {
        get => _suffix;
        set {
            if (_suffix != value) { return; }
            _suffix = value;
            OnPropertyChanged();
        }
    }

    #endregion

    #region Methods

    public override void Draw(Graphics gr, string content, Rectangle drawarea, Design design, States state, BildTextVerhalten behaviorOfImageAndText, TranslationType doOpticalTranslation, ReadOnlyCollection<string> opticalReplace, string constantHeightOfImageCode, float scale, Alignment align) {
        if (string.IsNullOrEmpty(content)) { return; }
        var font = Skin.DesignOf(design, state).BFont?.Scale(scale);
        if (font == null) { return; }

        var pix16 = Table.GetPix(16, scale);

        var mei = content.SplitAndCutByCrAndBr();

        switch (behaviorOfImageAndText) {
            case BildTextVerhalten.Nur_erste_Zeile_darstellen:
                if (mei.Length > 1) {
                    DrawOneLine(gr, mei[0] + "...", drawarea, 0, false, font, pix16, BildTextVerhalten.Nur_Text, state, scale, doOpticalTranslation, opticalReplace, constantHeightOfImageCode, align);
                } else if (mei.Length == 1) {
                    DrawOneLine(gr, mei[0], drawarea, 0, false, font, pix16, BildTextVerhalten.Nur_Text, state, scale, doOpticalTranslation, opticalReplace, constantHeightOfImageCode, align);
                }
                break;

            case BildTextVerhalten.Mehrzeilig_einzeilig_darsellen:
                DrawOneLine(gr, mei.JoinWith("; "), drawarea, 0, false, font, pix16, BildTextVerhalten.Nur_Text, state, scale, doOpticalTranslation, opticalReplace, constantHeightOfImageCode, align);
                break;

            default: {
                    var y = 0;
                    for (var z = 0; z <= mei.GetUpperBound(0); z++) {
                        DrawOneLine(gr, mei[z], drawarea, y, z != mei.GetUpperBound(0), font, pix16, behaviorOfImageAndText, state, scale, doOpticalTranslation, opticalReplace, constantHeightOfImageCode, align);
                        y += Table.GetPix(GetSizeOfCellContent(mei[z], design, state, behaviorOfImageAndText, doOpticalTranslation, opticalReplace, constantHeightOfImageCode).Height, scale);
                    }

                    break;
                }
        }
    }

    public (string text, QuickImage? qi) GetDrawingData(string originalText, BildTextVerhalten bildTextverhalten, TranslationType doOpticalTranslation, ReadOnlyCollection<string> opticalReplace, string constantHeightOfImageCode) {
        var tmpText = ValueReadable(originalText, ShortenStyle.Replaced, bildTextverhalten, true, doOpticalTranslation, opticalReplace);

        #region  tmpImageCode

        QuickImage? tmpImageCode = null;

        if (bildTextverhalten != BildTextVerhalten.Nur_Text) {
            var imgtxt = tmpText;

            if (bildTextverhalten == BildTextVerhalten.Nur_Bild) {
                imgtxt = ValueReadable(originalText, ShortenStyle.Replaced, BildTextVerhalten.Nur_Text, true, doOpticalTranslation, opticalReplace);
            }

            if (!string.IsNullOrEmpty(imgtxt)) {
                var gr = "16";
                if (!string.IsNullOrEmpty(constantHeightOfImageCode)) { gr = constantHeightOfImageCode; }

                var x = (imgtxt + "||").SplitBy("|");
                var gr2 = (gr + "||").SplitBy("|");
                x[1] = gr2[0];
                x[2] = gr2[1];
                var ntxt = x.JoinWith("|").TrimEnd("|");

                tmpImageCode = QuickImage.Get(ntxt);
                if (tmpImageCode.IsError) {
                    tmpImageCode = StandardErrorImage(gr, bildTextverhalten, imgtxt);
                }
            }
        }

        #endregion

        if (bildTextverhalten is BildTextVerhalten.Bild_oder_Text or BildTextVerhalten.Interpretiere_Bool or BildTextVerhalten.Interpretiere_Bool_CorrectState) {
            if (tmpImageCode != null) { tmpText = string.Empty; }
        }
        return (tmpText, tmpImageCode);
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
        [   new FlexiControlForProperty<string>(() => Präfix),
            new FlexiControlForProperty<string>(() => Suffix,cbxEinheit, true)
        ];
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
        }
        return true; // Immer true. So kann gefahrlos hin und her geschaltet werden und evtl. Werte aus anderen Renderen benutzt werden.
    }

    public override string ReadableText() => "Standard (alt)";

    public override QuickImage? SymbolForReadableText() => null;

    public override string ToParseableString() {
        List<string> result = [];

        result.ParseableAdd("Prefix", Präfix);

        result.ParseableAdd("Suffix", Suffix);

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
    ///
    ///
    /// <returns></returns>
    public override string ValueReadable(string content, ShortenStyle style, BildTextVerhalten bildTextverhalten, bool removeLineBreaks, TranslationType doOpticalTranslation, ReadOnlyCollection<string> opticalReplace) {
        if (bildTextverhalten == BildTextVerhalten.Nur_Bild && style != ShortenStyle.HTML) { return string.Empty; }

        content = LanguageTool.PrepaireText(content, style, Präfix, Suffix, doOpticalTranslation, opticalReplace);
        if (removeLineBreaks) {
            content = content.Replace("\r\n", " ");
            content = content.Replace("\r", " ");
        }

        if (style != ShortenStyle.HTML) { return content; }

        content = content.Replace("\r\n", "<br>");
        content = content.Replace("\r", "<br>");
        while (content.StartsWith(" ") || content.StartsWith("<br>") || content.EndsWith(" ") || content.EndsWith("<br>")) {
            content = content.Trim();
            content = content.Trim("<br>");
        }
        return content;
    }

    /// <summary>
    /// Status des Bildes (Disabled) wird geändert. Diese Routine sollte nicht innerhalb der Table Klasse aufgerufen werden.
    /// Sie dient nur dazu, das Aussehen eines Textes wie eine Zelle zu imitieren.
    /// </summary>
    ///
    protected override Size CalculateContentSize(string content, Design design, States state, BildTextVerhalten behaviorOfImageAndText, TranslationType doOpticalTranslation, ReadOnlyCollection<string> opticalReplace, string constantHeightOfImageCode) {
        var font = Skin.DesignOf(design, state).BFont?.Font(1);

        if (font == null) { return new Size(16, 16); }

        var contentSize = Size.Empty;

        var tmp = content.SplitAndCutByCrAndBr();

        switch (behaviorOfImageAndText) {
            case BildTextVerhalten.Nur_erste_Zeile_darstellen:
                if (tmp.Length > 1) {
                    contentSize = font.FormatedText_NeededSize(tmp[0] + "...", null, 16);
                }
                if (tmp.Length == 1) {
                    contentSize = font.FormatedText_NeededSize(tmp[0], null, 16);
                }
                break;

            case BildTextVerhalten.Mehrzeilig_einzeilig_darsellen:
                contentSize = font.FormatedText_NeededSize(tmp.JoinWith("; "), null, 16);
                break;

            default: {
                    foreach (var thisString in tmp) {
                        var (text, qi) = GetDrawingData(thisString, behaviorOfImageAndText, doOpticalTranslation, opticalReplace, constantHeightOfImageCode);
                        var tmpSize = font.FormatedText_NeededSize(text, qi, 16);
                        contentSize.Width = Math.Max(tmpSize.Width, contentSize.Width);
                        contentSize.Height += Math.Max(tmpSize.Height, 16);
                    }

                    break;
                }
        }

        contentSize.Width = Math.Max(contentSize.Width, 16);
        contentSize.Height = Math.Max(contentSize.Height, 16);
        SetSizeOfCellContent(content, contentSize);
        return contentSize;
    }

    private static QuickImage? StandardErrorImage(string gr, BildTextVerhalten bildTextverhalten, string originalText) {
        switch (bildTextverhalten) {
            case BildTextVerhalten.Fehlendes_Bild_zeige_Fragezeichen:
                return QuickImage.Get("Fragezeichen|" + gr);

            case BildTextVerhalten.Fehlendes_Bild_zeige_Kreis:
                return QuickImage.Get("Kreis2|" + gr);

            case BildTextVerhalten.Fehlendes_Bild_zeige_Kreuz:
                return QuickImage.Get("Kreuz|" + gr);

            case BildTextVerhalten.Fehlendes_Bild_zeige_Häkchen:
                return QuickImage.Get("Häkchen|" + gr);

            case BildTextVerhalten.Fehlendes_Bild_zeige_Infozeichen:
                return QuickImage.Get("Information|" + gr);

            case BildTextVerhalten.Fehlendes_Bild_zeige_Warnung:
                return QuickImage.Get("Warnung|" + gr);

            case BildTextVerhalten.Fehlendes_Bild_zeige_Kritischzeichen:
                return QuickImage.Get("Kritisch|" + gr);

            case BildTextVerhalten.Interpretiere_Bool:
                if (originalText == "+") { return QuickImage.Get("Häkchen|" + gr); }
                if (originalText == "-") { return QuickImage.Get("Kreuz|" + gr); }
                if (originalText is "o" or "O") { return QuickImage.Get("Kreis2|" + gr); }
                if (originalText == "?") { return QuickImage.Get("Fragezeichen|" + gr); }
                return null;

            case BildTextVerhalten.Interpretiere_Bool_CorrectState:
                if (originalText == "+") { return QuickImage.Get("Häkchen|" + gr + "||||||||80"); }
                if (originalText == "-") { return QuickImage.Get("Warnung|" + gr); }
                return null;

            case BildTextVerhalten.Bild_oder_Text:
                return null;

            default:
                return null;
        }
    }

    private void DrawOneLine(Graphics gr, string drawString, Rectangle drawarea, int txtYPix, bool changeToDot, BlueFont font, int pix16, BildTextVerhalten bildTextverhalten, States state, float scale, TranslationType doOpticalTranslation, ReadOnlyCollection<string> opticalReplace, string constantHeightOfImageCode, Alignment align) {
        Rectangle r = new(drawarea.Left, drawarea.Top + txtYPix, drawarea.Width, pix16);

        if (r.Bottom + pix16 > drawarea.Bottom) {
            if (r.Bottom > drawarea.Bottom) { return; }
            if (changeToDot) { drawString = "..."; }// Die letzte Zeile noch ganz hinschreiben
        }

        var (text, qi) = GetDrawingData(drawString, bildTextverhalten, doOpticalTranslation, opticalReplace, constantHeightOfImageCode);
        var tmpImageCode = qi?.Scale(scale);
        if (tmpImageCode != null) { tmpImageCode = QuickImage.Get(tmpImageCode, Skin.AdditionalState(state)); }

        Skin.Draw_FormatedText(gr, text, tmpImageCode, align, r, null, false, font, false);
    }

    private bool ShowMultiLine(string txt, bool ml) {
        if (!ml) { return false; }

        if (txt.Contains("\r")) { return true; }
        return txt.Contains("<br>");
    }

    #endregion
}