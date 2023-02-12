// Authors:
// Christian Peter
//
// Copyright (c) 2023 Christian Peter
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
using BlueBasics;
using BlueBasics.Enums;
using BlueDatabase.Enums;

namespace BlueDatabase;

/// <summary>
/// Diese Klasse enthält nur das Aussehen und gibt keinerlei Events ab. Deswegen INTERNAL!
/// </summary>
public class CellItem {

    #region Fields

    private string _value;

    #endregion

    #region Constructors

    public CellItem(string value) => _value = value;

    #endregion

    //public Size Size { get; set; }

    #region Properties

    //public Color BackColor { get; set; }
    //public Color FontColor { get; set; }
    //public bool Editable { get; set; }
    //public byte Symbol { get; set; }
    public string Value {
        get => _value;
        set {
            if (_value == value) { return; }
            _value = value;
            //InvalidateSize();
        }
    }

    #endregion

    #region Methods

    public static (string text, QuickImage? image) GetDrawingData(ColumnItem? column, string originalText, ShortenStyle style, BildTextVerhalten bildTextverhalten) {
        if (column == null) { return (originalText, null); }

        var tmpText = ValueReadable(column, originalText, style, bildTextverhalten, true);
        var tmpImageCode = StandardImage(column, originalText, tmpText, style, bildTextverhalten);

        if (bildTextverhalten is BildTextVerhalten.Bild_oder_Text or BildTextVerhalten.Interpretiere_Bool) {
            if (tmpImageCode != null) { tmpText = string.Empty; }
        }
        return (tmpText, tmpImageCode);
    }

    //public static enAlignment StandardAlignment(ColumnItem column, BildTextVerhalten bildTextverhalten) {
    //    switch (column.Align) {
    //        case enAlignmentHorizontal.Links:
    //            return enAlignment.Top_Left;

    //        case enAlignmentHorizontal.Rechts:
    //            return enAlignment.Top_Right;

    //        case enAlignmentHorizontal.Zentriert:
    //            return enAlignment.HorizontalCenter;
    //    }
    //    switch (column.Format) {
    //        case DataFormat.Ganzzahl:
    //        case DataFormat.Gleitkommazahl:
    //            return enAlignment.Top_Right;

    //        case DataFormat.Bit:
    //            if (bildTextverhalten is BildTextVerhalten.Nur_Bild or BildTextVerhalten.Bild_oder_Text) { return enAlignment.Top_HorizontalCenter; }
    //            return enAlignment.Top_Left;

    //        default:
    //            return enAlignment.Top_Left;
    //    }
    //}

    public static QuickImage? StandardErrorImage(string gr, BildTextVerhalten bildTextverhalten, string originalText, ColumnItem? column) {
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
                if (originalText == "+") {
                    return column == column.Database.Column.SysCorrect ? QuickImage.Get("Häkchen|" + gr + "||||||||80") : QuickImage.Get("Häkchen|" + gr);
                }

                if (originalText == "-") {
                    return column == column.Database.Column.SysCorrect ? QuickImage.Get("Warnung|" + gr) :
                        QuickImage.Get("Kreuz|" + gr);
                }
                if (originalText.ToLower() == "o") {
                    return QuickImage.Get("Kreis2|" + gr);
                }
                if (originalText.ToLower() == "?") {
                    return QuickImage.Get("Fragezeichen|" + gr);
                }

                return null;

            case BildTextVerhalten.Bild_oder_Text:
                return null;

            default:
                return null;
        }
    }

    /// <summary>
    /// Gibt eine einzelne Zeile richtig ersetzt mit Prä- und Suffix zurück.
    /// </summary>
    /// <param name="column"></param>
    /// <param name="txt"></param>
    /// <param name="style"></param>
    /// <param name="removeLineBreaks">bei TRUE werden Zeilenumbrüche mit Leerzeichen ersetzt</param>
    /// <returns></returns>

    public static string ValueReadable(ColumnItem? column, string txt, ShortenStyle style, BildTextVerhalten bildTextverhalten, bool removeLineBreaks) {
        if (bildTextverhalten == BildTextVerhalten.Nur_Bild && style != ShortenStyle.HTML) { return string.Empty; }

        if (column == null) { return txt; }

        switch (column.Format) {
            case DataFormat.Text:
            case DataFormat.Werte_aus_anderer_Datenbank_als_DropDownItems:
            case DataFormat.RelationText:
            case DataFormat.Verknüpfung_zu_anderer_Datenbank: // Bei LinkedCell kommt direkt der Text der verlinkten Zelle an

                //if (column.BildTextVerhalten == BildTextVerhalten.Interpretiere_Bool) {
                //    if (txt == "+") {
                //        if (column == column.Database.Column.SysCorrect) { return "Ok"; }
                //        if (column == column.Database.Column.SysLocked) { return "gesperrt"; }
                //        return "ja";
                //    } else if (txt == "-") {
                //        if (column == column.Database.Column.SysCorrect) { return "fehlerhaft"; }
                //        if (column == column.Database.Column.SysLocked) { return "bearbeitbar"; }
                //        return "nein";
                //    }
                //}

                txt = LanguageTool.ColumnReplace(txt, column, style);
                if (removeLineBreaks) {
                    txt = txt.Replace("\r\n", " ");
                    txt = txt.Replace("\r", " ");
                }
                break;

            case DataFormat.Button:
                txt = LanguageTool.ColumnReplace(txt, column, style);
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

            //case DataFormat.Columns_für_LinkedCellDropdown:
            //    // Hier kommt die Spalten-ID  an
            //    if (string.IsNullOrEmpty(txt)) { return string.Empty; }
            //    if (!IntTryParse(txt, out var ColKey)) { return "Columkey kann nicht geparsed werden"; }
            //    var LinkedDatabase = column.LinkedDatabase();
            //    if (LinkedDatabase == null) { return "Datenbankverknüpfung fehlt"; }
            //    var C = LinkedDatabase.Column.SearchByKey(ColKey);
            //    if (C == null) { return "Columnkey nicht gefunden"; }
            //    txt = LanguageTool.ColumnReplace(C.ReadableText(), column, style);
            //    break;

            default:
                Develop.DebugPrint(column.Format);
                break;
        }
        if (style != ShortenStyle.HTML) { return txt; }
        txt = txt.Replace("\r\n", "<br>");
        txt = txt.Replace("\r", "<br>");
        //if (txt.Contains("\r")) { Develop.DebugPrint(enFehlerArt.Info, "\\r enthalten:" + txt); }
        //if (txt.Contains("\n")) { Develop.DebugPrint(enFehlerArt.Info, "\\n enthalten:" + txt); }
        while (txt.StartsWith(" ") || txt.StartsWith("<br>") || txt.EndsWith(" ") || txt.EndsWith("<br>")) {
            txt = txt.Trim();
            txt = txt.Trim("<br>");
        }
        return txt;
    }

    /// <summary>
    /// Jede Zeile für sich richtig formatiert.
    /// </summary>
    /// <returns></returns>

    public static List<string> ValuesReadable(ColumnItem? column, RowItem Row, ShortenStyle style) {
        if (column.Format is DataFormat.Verknüpfung_zu_anderer_Datenbank) {
            //var LinkedData = CellCollection.LinkedCellData(column, Row, false, false);
            //if (LinkedData.Item1 != null && LinkedData.Item2 != null) { return ValuesReadable(LinkedData.Item1, LinkedData.Item2, Style); }
            //return new List<string>();
            Develop.DebugPrint(FehlerArt.Warnung, "LinkedCell sollte hier nicht ankommen.");
        }
        List<string> ret = new();
        if (!column.MultiLine) {
            ret.Add(ValueReadable(column, Row.CellGetString(column), style, column.BehaviorOfImageAndText, true));
            return ret;
        }
        var x = Row.CellGetList(column);
        foreach (var thisstring in x) {
            ret.Add(ValueReadable(column, thisstring, style, column.BehaviorOfImageAndText, true));
        }
        if (x.Count == 0) {
            var tmp = ValueReadable(column, string.Empty, style, column.BehaviorOfImageAndText, true);
            if (!string.IsNullOrEmpty(tmp)) { ret.Add(tmp); }
        }
        return ret;
    }

    //internal void InvalidateSize() => Size = Size.Empty;

    private static QuickImage? StandardImage(ColumnItem? column, string originalText, string replacedText, ShortenStyle style, BildTextVerhalten bildTextverhalten) {
        // replacedText kann auch empty sein. z.B. wenn er nicht angezeigt wird
        if (bildTextverhalten == BildTextVerhalten.Nur_Text) { return null; }
        if (style == ShortenStyle.HTML) { return null; }
        if (column == null) { return null; }
        if (bildTextverhalten == BildTextVerhalten.Nur_Bild) { replacedText = ValueReadable(column, originalText, style, BildTextVerhalten.Nur_Text, true); }
        if (string.IsNullOrEmpty(replacedText)) { return null; }

        var gr = Math.Truncate(column.Database.GlobalScale * 16).ToString(Constants.Format_Integer1);
        if (!string.IsNullOrEmpty(column.ConstantHeightOfImageCode)) { gr = column.ConstantHeightOfImageCode; }

        var x = (replacedText + "||").SplitBy("|");
        var gr2 = (gr + "||").SplitBy("|");
        x[1] = gr2[0];
        x[2] = gr2[1];
        var ntxt = x.JoinWith("|").TrimEnd("|");

        var defaultImage = QuickImage.Get(ntxt);
        return defaultImage != null && !defaultImage.IsError ? defaultImage : StandardErrorImage(gr, bildTextverhalten, replacedText, column);
    }

    #endregion
}