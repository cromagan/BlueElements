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

using BlueBasics;
using BlueBasics.Enums;
using BlueDatabase.Enums;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing;

namespace BlueDatabase;

/// <summary>
/// Diese Klasse enthält nur das Aussehen und gibt keinerlei Events ab.
/// </summary>
public class CellItem {

    #region Constructors

    public CellItem(string value) => Value = value;

    #endregion

    #region Properties

    //public Color BackColor { get; set; }
    //public Color FontColor { get; set; }
    //public bool Editable { get; set; }
    //public byte Symbol { get; set; }
    public string Value { get; set; }

    #endregion

    #region Methods

    /// <summary>
    /// Status des Bildes (Disabled) wird geändert. Diese Routine sollte nicht innerhalb der Table Klasse aufgerufen werden.
    /// Sie dient nur dazu, das Aussehen eines Textes wie eine Zelle zu imitieren.
    /// </summary>
    public static Size ContentSize(string keyName, DataFormat format, string originalText, Font cellfont, ShortenStyle style, int minSize, BildTextVerhalten bildTextverhalten, string prefix, string suffix, TranslationType doOpticalTranslation, ReadOnlyCollection<string> opticalReplace, double scale, string constantHeightOfImageCode) {
        var (s, qi) = GetDrawingData(keyName, format, originalText, style, bildTextverhalten, prefix, suffix, doOpticalTranslation, opticalReplace, scale, constantHeightOfImageCode);

        return cellfont.FormatedText_NeededSize(s, qi, minSize);
    }

    public static Size ContentSize(ColumnItem column, RowItem row, Font cellFont, int pix16) {
        if (column.Database is not Database db || db.IsDisposed) { return new Size(pix16, pix16); }

        if (column.Format == DataFormat.Verknüpfung_zu_anderer_Datenbank) {
            var (lcolumn, lrow, _, _) = CellCollection.LinkedCellData(column, row, false, false);
            return lcolumn != null && lrow != null ? ContentSize(lcolumn, lrow, cellFont, pix16)
                : new Size(pix16, pix16);
        }

        var contentSizex = db.Cell.GetSizeOfCellContent(column, row);
        if (contentSizex != null) { return (Size)contentSizex; }

        var contentSize = Size.Empty;

        if (column.MultiLine) {
            var tmp = db.Cell.GetString(column, row).SplitAndCutByCrAndBr();
            if (column.ShowMultiLineInOneLine) {
                contentSize = ContentSize(column.KeyName, column.Format, tmp.JoinWith("; "), cellFont, ShortenStyle.Replaced, pix16, column.BehaviorOfImageAndText, column.Prefix, column.Suffix, column.DoOpticalTranslation, column.OpticalReplace, db.GlobalScale, column.ConstantHeightOfImageCode);
            } else {
                foreach (var thisString in tmp) {
                    var tmpSize = ContentSize(column.KeyName, column.Format, thisString, cellFont, ShortenStyle.Replaced, pix16, column.BehaviorOfImageAndText, column.Prefix, column.Suffix, column.DoOpticalTranslation, column.OpticalReplace, db.GlobalScale, column.ConstantHeightOfImageCode);
                    contentSize.Width = Math.Max(tmpSize.Width, contentSize.Width);
                    contentSize.Height += Math.Max(tmpSize.Height, pix16);
                }
            }
        } else {
            var txt = db.Cell.GetString(column, row);
            contentSize = ContentSize(column.KeyName, column.Format, txt, cellFont, ShortenStyle.Replaced, pix16, column.BehaviorOfImageAndText, column.Prefix, column.Suffix, column.DoOpticalTranslation, column.OpticalReplace, db.GlobalScale, column.ConstantHeightOfImageCode);
        }
        contentSize.Width = Math.Max(contentSize.Width, pix16);
        contentSize.Height = Math.Max(contentSize.Height, pix16);
        db.Cell.SetSizeOfCellContent(column, row, contentSize);
        return contentSize;
    }

    public static (string text, QuickImage? qi) GetDrawingData(string additionalname, DataFormat format, string originalText, ShortenStyle style, BildTextVerhalten bildTextverhalten, string prefix, string suffix, TranslationType doOpticalTranslation, ReadOnlyCollection<string> opticalReplace, double scale, string constantHeightOfImageCode) {
        var tmpText = ValueReadable(originalText, style, format, bildTextverhalten, true, prefix, suffix, doOpticalTranslation, opticalReplace);

        #region  tmpImageCode

        QuickImage? tmpImageCode = null;

        if (bildTextverhalten != BildTextVerhalten.Nur_Text && style != ShortenStyle.HTML) {
            var imgtxt = tmpText;

            if (bildTextverhalten == BildTextVerhalten.Nur_Bild) {
                imgtxt = ValueReadable(originalText, style, format, BildTextVerhalten.Nur_Text, true, prefix, suffix, doOpticalTranslation, opticalReplace);
            }

            if (!string.IsNullOrEmpty(imgtxt)) {
                var gr = Math.Truncate(16 * scale).ToString(Constants.Format_Integer1);
                if (!string.IsNullOrEmpty(constantHeightOfImageCode)) { gr = constantHeightOfImageCode; }

                var x = (imgtxt + "||").SplitBy("|");
                var gr2 = (gr + "||").SplitBy("|");
                x[1] = gr2[0];
                x[2] = gr2[1];
                var ntxt = x.JoinWith("|").TrimEnd("|");

                tmpImageCode = QuickImage.Get(additionalname.ToLower() + "_" + ntxt);
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

    /// <summary>
    /// Gibt eine einzelne Zeile richtig ersetzt mit Prä- und Suffix zurück.
    /// </summary>
    /// <param name="txt"></param>
    /// <param name="style"></param>
    /// <param name="format"></param>
    /// <param name="bildTextverhalten"></param>
    /// <param name="removeLineBreaks">bei TRUE werden Zeilenumbrüche mit Leerzeichen ersetzt</param>
    /// <param name="prefix"></param>
    /// <param name="suffix"></param>
    /// <param name="doOpticalTranslation"></param>
    /// <param name="opticalReplace"></param>
    /// <returns></returns>
    public static string ValueReadable(string txt, ShortenStyle style, DataFormat format, BildTextVerhalten bildTextverhalten, bool removeLineBreaks, string prefix, string suffix, TranslationType doOpticalTranslation, ReadOnlyCollection<string> opticalReplace) {
        if (bildTextverhalten == BildTextVerhalten.Nur_Bild && style != ShortenStyle.HTML) { return string.Empty; }

        switch (format) {
            case DataFormat.Text:
            case DataFormat.Werte_aus_anderer_Datenbank_als_DropDownItems:
            case DataFormat.RelationText:
            case DataFormat.Verknüpfung_zu_anderer_Datenbank: // Bei LinkedCell kommt direkt der Text der verlinkten Zelle an

                txt = LanguageTool.PrepaireText(txt, style, prefix, suffix, doOpticalTranslation, opticalReplace);
                if (removeLineBreaks) {
                    txt = txt.Replace("\r\n", " ");
                    txt = txt.Replace("\r", " ");
                }
                break;

            case DataFormat.Button:
                txt = LanguageTool.PrepaireText(txt, style, prefix, suffix, doOpticalTranslation, opticalReplace);
                break;

            //case DataFormat.FarbeInteger:
            //    if (!string.IsNullOrEmpty(txt) && txt.IsFormat(DataFormat.FarbeInteger)) {
            //        var col = Color.FromArgb(IntParse(txt));
            //        txt = col.ToHTMLCode().ToUpper();
            //    }
            //    txt = LanguageTool.ColumnReplace(txt, column, style);
            //    break;

            case DataFormat.Schrift:
                //    Develop.DebugPrint_NichtImplementiert();
                //if (string.IsNullOrEmpty(Txt) || Txt.Substring(0, 1) != "{") { return Txt; }
                //if (CompactView) { return string.Empty; }
                //return BlueFont.Get(Txt).ReadableText();
                return txt;

            default:
                Develop.DebugPrint(format);
                break;
        }
        if (style != ShortenStyle.HTML) { return txt; }
        txt = txt.Replace("\r\n", "<br>");
        txt = txt.Replace("\r", "<br>");
        while (txt.StartsWith(" ") || txt.StartsWith("<br>") || txt.EndsWith(" ") || txt.EndsWith("<br>")) {
            txt = txt.Trim();
            txt = txt.Trim("<br>");
        }
        return txt;
    }

    public static List<string>? ValuesReadable(ColumnItem? column, RowItem? row, ShortenStyle style) {
        if (column == null || row == null) { return null; }

        if (column.Format is DataFormat.Verknüpfung_zu_anderer_Datenbank) {
            Develop.DebugPrint(FehlerArt.Warnung, "LinkedCell sollte hier nicht ankommen.");
        }

        List<string> ret = [];
        if (!column.MultiLine) {
            ret.Add(ValueReadable(row.CellGetString(column), style, column.Format, column.BehaviorOfImageAndText, true, column.Prefix, column.Suffix, column.DoOpticalTranslation, column.OpticalReplace));
            return ret;
        }

        var x = row.CellGetList(column);
        foreach (var thisstring in x) {
            ret.Add(ValueReadable(thisstring, style, column.Format, column.BehaviorOfImageAndText, true, column.Prefix, column.Suffix, column.DoOpticalTranslation, column.OpticalReplace));
        }

        if (x.Count == 0) {
            var tmp = ValueReadable(string.Empty, style, column.Format, column.BehaviorOfImageAndText, true, column.Prefix, column.Suffix, column.DoOpticalTranslation, column.OpticalReplace);
            if (!string.IsNullOrEmpty(tmp)) { ret.Add(tmp); }
        }
        return ret;
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

    #endregion
}