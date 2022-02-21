// Authors:
// Christian Peter
//
// Copyright (c) 2022 Christian Peter
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
using BlueDatabase.Enums;
using System;
using System.Collections.Generic;
using System.Drawing;

namespace BlueDatabase {

    /// <summary>
    /// Diese Klasse enthält nur das Aussehen und gibt keinerlei Events ab. Deswegen INTERNAL!
    /// </summary>
    public class CellItem {

        #region Fields

        private string _value = string.Empty;

        #endregion

        #region Constructors

        public CellItem(string value, int width, int height) {
            _value = value;
            if (width > 0) { Size = new Size(width, height); }
        }

        #endregion

        #region Properties

        public Size Size { get; set; }

        //public Color BackColor { get; set; }
        //public Color FontColor { get; set; }
        //public bool Editable { get; set; }
        //public byte Symbol { get; set; }
        public string Value {
            get => _value;
            set {
                if (_value == value) { return; }
                _value = value;
                InvalidateSize();
            }
        }

        #endregion

        #region Methods

        public static Tuple<string, QuickImage> GetDrawingData(ColumnItem column, string originalText, enShortenStyle style, enBildTextVerhalten bildTextverhalten) {
            var tmpText = ValueReadable(column, originalText, style, bildTextverhalten, true);
            var tmpImageCode = StandardImage(column, originalText, tmpText, style, bildTextverhalten);

            if (bildTextverhalten is enBildTextVerhalten.Bild_oder_Text or enBildTextVerhalten.Interpretiere_Bool) {
                if (tmpImageCode != null) { tmpText = string.Empty; }
                //if (tmpImageCode == null && string.IsNullOrEmpty(tmpText) && !string.IsNullOrEmpty(originalText)) {
                //    tmpImageCode = StandardErrorImage(16, enBildTextVerhalten.Fehlendes_Bild_zeige_Kritischzeichen);
                //}

                //q
            }
            return new Tuple<string, QuickImage>(tmpText, tmpImageCode);
        }

        //public static enAlignment StandardAlignment(ColumnItem column, enBildTextVerhalten bildTextverhalten) {
        //    switch (column.Align) {
        //        case enAlignmentHorizontal.Links:
        //            return enAlignment.Top_Left;

        //        case enAlignmentHorizontal.Rechts:
        //            return enAlignment.Top_Right;

        //        case enAlignmentHorizontal.Zentriert:
        //            return enAlignment.HorizontalCenter;
        //    }
        //    switch (column.Format) {
        //        case enDataFormat.Ganzzahl:
        //        case enDataFormat.Gleitkommazahl:
        //            return enAlignment.Top_Right;

        //        case enDataFormat.Bit:
        //            if (bildTextverhalten is enBildTextVerhalten.Nur_Bild or enBildTextVerhalten.Bild_oder_Text) { return enAlignment.Top_HorizontalCenter; }
        //            return enAlignment.Top_Left;

        //        default:
        //            return enAlignment.Top_Left;
        //    }
        //}

        public static QuickImage StandardErrorImage(string gr, enBildTextVerhalten bildTextverhalten, string originalText, ColumnItem column) {
            switch (bildTextverhalten) {
                case enBildTextVerhalten.Fehlendes_Bild_zeige_Fragezeichen:
                    return QuickImage.Get("Fragezeichen|" + gr);

                case enBildTextVerhalten.Fehlendes_Bild_zeige_Kreis:
                    return QuickImage.Get("Kreis2|" + gr);

                case enBildTextVerhalten.Fehlendes_Bild_zeige_Kreuz:
                    return QuickImage.Get("Kreuz|" + gr);

                case enBildTextVerhalten.Fehlendes_Bild_zeige_Häkchen:
                    return QuickImage.Get("Häkchen|" + gr);

                case enBildTextVerhalten.Fehlendes_Bild_zeige_Infozeichen:
                    return QuickImage.Get("Information|" + gr);

                case enBildTextVerhalten.Fehlendes_Bild_zeige_Warnung:
                    return QuickImage.Get("Warnung|" + gr);

                case enBildTextVerhalten.Fehlendes_Bild_zeige_Kritischzeichen:
                    return QuickImage.Get("Kritisch|" + gr);

                case enBildTextVerhalten.Interpretiere_Bool:
                    if (originalText == "+") {
                        return column == column.Database.Column.SysCorrect ? QuickImage.Get("Häkchen|" + gr.ToString() + "||||||||80") : QuickImage.Get("Häkchen|" + gr);
                    } else if (originalText == "-") {
                        return column == column.Database.Column.SysCorrect ? QuickImage.Get("Warnung|" + gr) :
                            QuickImage.Get("Kreuz|" + gr);
                    } else if (originalText.ToLower() == "o") {
                        return QuickImage.Get("Kreis2|" + gr);
                    } else if (originalText.ToLower() == "?") {
                        return QuickImage.Get("Fragezeichen|" + gr);
                    }
                    return null;

                case enBildTextVerhalten.Bild_oder_Text:
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
        public static string ValueReadable(ColumnItem column, string txt, enShortenStyle style, enBildTextVerhalten bildTextverhalten, bool removeLineBreaks) {
            if (bildTextverhalten == enBildTextVerhalten.Nur_Bild && style != enShortenStyle.HTML) { return string.Empty; }
            switch (column.Format) {
                case enDataFormat.Text:
                case enDataFormat.Link_To_Filesystem:
                case enDataFormat.Values_für_LinkedCellDropdown:
                case enDataFormat.RelationText:
                case enDataFormat.LinkedCell:  // Bei LinkedCell kommt direkt der Text der verlinkten Zelle an

                    //if (column.BildTextVerhalten == enBildTextVerhalten.Interpretiere_Bool) {
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

                case enDataFormat.Button:
                    txt = LanguageTool.ColumnReplace(txt, column, style);
                    break;

                //case enDataFormat.FarbeInteger:
                //    if (!string.IsNullOrEmpty(txt) && txt.IsFormat(enDataFormat.FarbeInteger)) {
                //        var col = Color.FromArgb(int.Parse(txt));
                //        txt = col.ToHTMLCode().ToUpper();
                //    }
                //    txt = LanguageTool.ColumnReplace(txt, column, style);
                //    break;

                case enDataFormat.Schrift:
                    //    Develop.DebugPrint_NichtImplementiert();
                    //if (string.IsNullOrEmpty(Txt) || Txt.Substring(0, 1) != "{") { return Txt; }
                    //if (CompactView) { return string.Empty; }
                    //return BlueFont.Get(Txt).ReadableText();
                    return txt;

                case enDataFormat.Columns_für_LinkedCellDropdown:
                    // Hier kommt die Spalten-ID  an
                    if (string.IsNullOrEmpty(txt)) { return string.Empty; }
                    if (!int.TryParse(txt, out var ColKey)) { return "Columkey kann nicht geparsed werden"; }
                    var LinkedDatabase = column.LinkedDatabase();
                    if (LinkedDatabase == null) { return "Datenbankverknüpfung fehlt"; }
                    var C = LinkedDatabase.Column.SearchByKey(ColKey);
                    if (C == null) { return "Columnkey nicht gefunden"; }
                    txt = LanguageTool.ColumnReplace(C.ReadableText(), column, style);
                    break;

                default:
                    Develop.DebugPrint(column.Format);
                    break;
            }
            if (style != enShortenStyle.HTML) { return txt; }
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
        public static List<string> ValuesReadable(ColumnItem column, RowItem Row, enShortenStyle Style) {
            if (column.Format == enDataFormat.LinkedCell) {
                //var LinkedData = CellCollection.LinkedCellData(column, Row, false, false);
                //if (LinkedData.Item1 != null && LinkedData.Item2 != null) { return ValuesReadable(LinkedData.Item1, LinkedData.Item2, Style); }
                //return new List<string>();
                Develop.DebugPrint(enFehlerArt.Warnung, "LinkedCell sollte hier nicht ankommen.");
            }
            List<string> ret = new();
            if (!column.MultiLine) {
                ret.Add(ValueReadable(column, Row.CellGetString(column), Style, column.BildTextVerhalten, true));
                return ret;
            }
            var x = Row.CellGetList(column);
            foreach (var thisstring in x) {
                ret.Add(ValueReadable(column, thisstring, Style, column.BildTextVerhalten, true));
            }
            if (x.Count == 0) {
                var tmp = ValueReadable(column, string.Empty, Style, column.BildTextVerhalten, true);
                if (!string.IsNullOrEmpty(tmp)) { ret.Add(tmp); }
            }
            return ret;
        }

        internal void InvalidateSize() => Size = Size.Empty;

        private static QuickImage StandardImage(ColumnItem column, string originalText, string replacedText, enShortenStyle style, enBildTextVerhalten bildTextverhalten) {
            // replacedText kann auch empty sein. z.B. wenn er nicht angezeigt wird
            if (bildTextverhalten == enBildTextVerhalten.Nur_Text) { return null; }
            if (style == enShortenStyle.HTML) { return null; }
            if (column == null) { return null; }
            if (bildTextverhalten == enBildTextVerhalten.Nur_Bild) { replacedText = ValueReadable(column, originalText, style, enBildTextVerhalten.Nur_Text, true); }
            if (string.IsNullOrEmpty(replacedText)) { return null; }

            var gr = Math.Truncate(column.Database.GlobalScale * 16).ToString();
            if (!string.IsNullOrEmpty(column.BildCode_ConstantHeight)) { gr = column.BildCode_ConstantHeight; }

            if (replacedText.Contains("|")) {
                var x = replacedText.SplitBy("|");
                replacedText = x[0];
            }

            var defaultImage = QuickImage.Get(replacedText + "|" + gr.ToString());
            if (defaultImage != null && !defaultImage.IsError) { return defaultImage; }

            return StandardErrorImage(gr, bildTextverhalten, replacedText, column);
        }

        #endregion
    }
}