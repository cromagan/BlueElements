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

public class DefaultRenderer : AbstractRenderer {

    #region Constructors

    public DefaultRenderer(string keyname) : base(keyname) { }

    #endregion

    #region Properties

    public static string ClassId => "Default";
    public override string Description => "Standard Anzeige";
    public override string MyClassId => ClassId;
    public string Präfix { get; set; } = string.Empty;

    public string Suffix { get; set; } = string.Empty;

    #endregion

    #region Methods

    public override void Draw(Graphics gr, string content, Rectangle drawarea, Design design, States state, ColumnItem? column, float scale) {
        if (column == null) { return; }
        if (string.IsNullOrEmpty(content)) { return; }
        var font = Skin.DesignOf(design, state).BFont?.Scale(scale);
        if (font == null) { return; }

        var pix16 = Table.GetPix(16, scale);

        if (!ShowMultiLine(content, column.MultiLine)) {
            DrawOneLine(gr, content, column, drawarea, 0, false, font, pix16, column.BehaviorOfImageAndText, state, scale);
        } else {
            var mei = content.SplitAndCutByCrAndBr();

            switch (column.BehaviorOfImageAndText) {
                case BildTextVerhalten.Nur_erste_Zeile_darstellen:
                    if (mei.Length > 1) {
                        DrawOneLine(gr, mei[0] + "...", column, drawarea, 0, false, font, pix16, BildTextVerhalten.Nur_Text, state, scale);
                    } else if (mei.Length == 1) {
                        DrawOneLine(gr, mei[0], column, drawarea, 0, false, font, pix16, BildTextVerhalten.Nur_Text, state, scale);
                    }
                    break;

                case BildTextVerhalten.Mehrzeilig_einzeilig_darsellen:
                    DrawOneLine(gr, mei.JoinWith("; "), column, drawarea, 0, false, font, pix16, BildTextVerhalten.Nur_Text, state, scale);
                    break;

                default: {
                        var y = 0;
                        for (var z = 0; z <= mei.GetUpperBound(0); z++) {
                            DrawOneLine(gr, mei[z], column, drawarea, y, z != mei.GetUpperBound(0), font, pix16, column.BehaviorOfImageAndText, state, scale);
                            y += GetSizeOfCellContent(column, mei[z], design, state, column.BehaviorOfImageAndText, column.DoOpticalTranslation, column.OpticalReplace, scale, column.ConstantHeightOfImageCode).Height;
                        }

                        break;
                    }
            }
        }
    }

    public (string text, QuickImage? qi) GetDrawingData(string colkeyname, string originalText, BildTextVerhalten bildTextverhalten, TranslationType doOpticalTranslation, ReadOnlyCollection<string> opticalReplace, float scale, string constantHeightOfImageCode) {
        var tmpText = ValueReadable(originalText, ShortenStyle.Replaced, bildTextverhalten, true, doOpticalTranslation, opticalReplace);

        #region  tmpImageCode

        QuickImage? tmpImageCode = null;

        if (bildTextverhalten != BildTextVerhalten.Nur_Text) {
            var imgtxt = tmpText;

            if (bildTextverhalten == BildTextVerhalten.Nur_Bild) {
                imgtxt = ValueReadable(originalText, ShortenStyle.Replaced, BildTextVerhalten.Nur_Text, true, doOpticalTranslation, opticalReplace);
            }

            if (!string.IsNullOrEmpty(imgtxt)) {
                var gr = ((int)Math.Truncate(16 * scale)).ToStringInt1();
                if (!string.IsNullOrEmpty(constantHeightOfImageCode)) { gr = constantHeightOfImageCode; }

                var x = (imgtxt + "||").SplitBy("|");
                var gr2 = (gr + "||").SplitBy("|");
                x[1] = gr2[0];
                x[2] = gr2[1];
                var ntxt = x.JoinWith("|").TrimEnd("|");

                tmpImageCode = QuickImage.Get(colkeyname.ToLowerInvariant() + "_" + ntxt);
                if (tmpImageCode.IsError) {
                    tmpImageCode = QuickImage.Get(ntxt);
                    if (tmpImageCode.IsError) {
                        tmpImageCode = StandardErrorImage(gr, bildTextverhalten, imgtxt);
                    }
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
                    new FlexiControlForProperty<string>(() => Suffix,cbxEinheit)
        ];
        return result;
    }

    public override void ParseFinished(string parsed) { }

    public override bool ParseThis(string key, string value) {
        switch (key.ToLower()) {
            case "prefix":
                Präfix = value.FromNonCritical();
                return true;

            case "suffix":
                Suffix = value.FromNonCritical();
                return true;
        }
        return true; // Immer true. So kann gefahrlos hin und her geschaltet werden und evtl. Werte aus anderen Renderen benutzt werden.
    }

    public override string ReadableText() => "Standard";

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

        content = LanguageTool.PrepaireText(content, style, Präfix, Suffix,   doOpticalTranslation, opticalReplace);
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
    protected override Size CalculateContentSize(ColumnItem column, string originalText, Design design, States state, BildTextVerhalten behaviorOfImageAndText, TranslationType doOpticalTranslation, ReadOnlyCollection<string> opticalReplace, float scale, string constantHeightOfImageCode) {

        var font = Skin.DesignOf(design, state).BFont?.Font(scale);

        var pix16 = Table.GetPix(16, scale);

        if (font == null) { return new Size(pix16, pix16); }

        var contentSize = Size.Empty;

        if (column.MultiLine) {
            var tmp = originalText.SplitAndCutByCrAndBr();

            switch (column.BehaviorOfImageAndText) {
                case BildTextVerhalten.Nur_erste_Zeile_darstellen:
                    if (tmp.Length > 1) {
                        contentSize = font.FormatedText_NeededSize(tmp[0] + "...", null, pix16);
                        //  contentSize = ContentSize(column, tmp[0] + "...", design, state, BildTextVerhalten.Nur_Text, column.Prefix, column.Suffix, column.DoOpticalTranslation, column.OpticalReplace, scale, column.ConstantHeightOfImageCode);
                    }
                    if (tmp.Length == 1) {
                        contentSize = font.FormatedText_NeededSize(tmp[0], null, pix16);
                        //contentSize = ContentSize(column, tmp[0], design, state, BildTextVerhalten.Nur_Text, column.Prefix, column.Suffix, column.DoOpticalTranslation, column.OpticalReplace, scale, column.ConstantHeightOfImageCode);
                    }
                    break;

                case BildTextVerhalten.Mehrzeilig_einzeilig_darsellen:
                    contentSize = font.FormatedText_NeededSize(tmp.JoinWith("; "), null, pix16);
                    //contentSize = ContentSize(column, tmp.JoinWith("; "), design, state, column.BehaviorOfImageAndText, column.Prefix, column.Suffix, column.DoOpticalTranslation, column.OpticalReplace, scale, column.ConstantHeightOfImageCode);
                    break;

                default: {
                        foreach (var thisString in tmp) {
                            var tmpSize = font.FormatedText_NeededSize(thisString, null, pix16);
                            //var tmpSize = ContentSize(column, thisString, design, state, column.BehaviorOfImageAndText, column.Prefix, column.Suffix, column.DoOpticalTranslation, column.OpticalReplace, scale, column.ConstantHeightOfImageCode);
                            contentSize.Width = Math.Max(tmpSize.Width, contentSize.Width);
                            contentSize.Height += Math.Max(tmpSize.Height, Table.GetPix(16, scale));
                        }

                        break;
                    }
            }
        } else {
            contentSize = font.FormatedText_NeededSize(originalText, null, pix16);
            //contentSize = ContentSize(column, originalText, design, state, column.BehaviorOfImageAndText, column.Prefix, column.Suffix, column.DoOpticalTranslation, column.OpticalReplace, scale, column.ConstantHeightOfImageCode);
        }
        contentSize.Width = Math.Max(contentSize.Width, pix16);
        contentSize.Height = Math.Max(contentSize.Height, pix16);
        SetSizeOfCellContent(column, KeyName, originalText, contentSize);
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

    private void DrawOneLine(Graphics gr, string drawString, ColumnItem column, Rectangle drawarea, int txtYPix, bool changeToDot, BlueFont font, int pix16, BildTextVerhalten bildTextverhalten, States state, float scale) {
        Rectangle r = new(drawarea.Left, drawarea.Top + txtYPix, drawarea.Width, pix16);

        if (r.Bottom + pix16 > drawarea.Bottom) {
            if (r.Bottom > drawarea.Bottom) { return; }
            if (changeToDot) { drawString = "..."; }// Die letzte Zeile noch ganz hinschreiben
        }

        var (text, qi) = GetDrawingData(column.KeyName, drawString, bildTextverhalten, column.DoOpticalTranslation, column.OpticalReplace, scale, column.ConstantHeightOfImageCode);
        var tmpImageCode = qi;
        if (tmpImageCode != null) { tmpImageCode = QuickImage.Get(tmpImageCode, Skin.AdditionalState(state)); }

        Skin.Draw_FormatedText(gr, text, tmpImageCode, (Alignment)column.Align, r, null, false, font, false);
    }

    private bool ShowMultiLine(string txt, bool ml) {
        if (!ml) { return false; }

        if (txt.Contains("\r")) { return true; }
        return txt.Contains("<br>");
    }

    #endregion
}