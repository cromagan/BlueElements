#region BlueElements - a collection of useful tools, database and controls
// Authors: 
// Christian Peter
// 
// Copyright (c) 2020 Christian Peter
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
#endregion

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
        private string _value = string.Empty;


        #region Konstruktor

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
            get {
                return _value;
            }
            set {
                if (_value == value) { return; }

                _value = value;
                InvalidateSize();

            }
        }

        #endregion

        internal void InvalidateSize() {
            Size = Size.Empty;
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

            List<string> ret = new List<string>();

            if (!column.MultiLine) {
                ret.Add(ValueReadable(column, Row.CellGetString(column), Style, column.BildTextVerhalten, true));
                return ret;
            }

            List<string> x = Row.CellGetList(column);
            foreach (string thisstring in x) {
                ret.Add(ValueReadable(column, thisstring, Style, column.BildTextVerhalten, true));
            }

            if (x.Count == 0) {
                string tmp = ValueReadable(column, string.Empty, Style, column.BildTextVerhalten, true);
                if (!string.IsNullOrEmpty(tmp)) { ret.Add(tmp); }
            }


            return ret;
        }



        public static Tuple<string, enAlignment, QuickImage> GetDrawingData(ColumnItem column, string originalText, enShortenStyle style, enBildTextVerhalten bildTextverhalten) {
            string tmpText = CellItem.ValueReadable(column, originalText, style, bildTextverhalten, true);
            enAlignment tmpAlign = CellItem.StandardAlignment(column, bildTextverhalten);
            QuickImage tmpImageCode = CellItem.StandardImage(column, originalText, tmpText, style, bildTextverhalten);

            if (bildTextverhalten == enBildTextVerhalten.Bild_oder_Text) {
                if (tmpImageCode != null) { tmpText = string.Empty; }
                if (tmpImageCode == null && string.IsNullOrEmpty(tmpText) && !string.IsNullOrEmpty(originalText)) {
                    tmpImageCode = StandardErrorImage(16, enBildTextVerhalten.Fehlendes_Bild_zeige_Kritischzeichen);
                }
            }

            return new Tuple<string, enAlignment, QuickImage>(tmpText, tmpAlign, tmpImageCode);
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
                case enDataFormat.Text_mit_Formatierung:
                case enDataFormat.Datum_und_Uhrzeit:
                case enDataFormat.Link_To_Filesystem:
                case enDataFormat.Gleitkommazahl:
                case enDataFormat.Ganzzahl:
                case enDataFormat.Values_für_LinkedCellDropdown:
                case enDataFormat.RelationText:
                case enDataFormat.LinkedCell:  // Bei LinkedCell kommt direkt der Text der verlinkten Zelle an
                    txt = LanguageTool.ColumnReplace(txt, column, style);


                    if (removeLineBreaks) {
                        txt = txt.Replace("\r\n", " ");
                        txt = txt.Replace("\r", " ");
                    }
                    break;

                case enDataFormat.BildCode:
                case enDataFormat.Button:
                    txt = LanguageTool.ColumnReplace(txt, column, style);
                    break;

                case enDataFormat.Bit:
                    if (txt == true.ToPlusMinus()) {
                        txt = "Ja";
                        if (column == column.Database.Column.SysCorrect) { return "Ok"; }
                        if (column == column.Database.Column.SysLocked) { return "gesperrt"; }

                    } else if (txt == false.ToPlusMinus()) {
                        txt = "Nein";
                        if (column == column.Database.Column.SysCorrect) { return "fehlerhaft"; }
                        if (column == column.Database.Column.SysLocked) { return "bearbeitbar"; }
                    } else if (txt == "o" || txt == "O") {
                        txt = "Neutral";
                    } else if (txt == "?") {
                        txt = "Unbekannt";
                    }
                    txt = LanguageTool.ColumnReplace(txt, column, style);
                    break;


                case enDataFormat.FarbeInteger:
                    if (!string.IsNullOrEmpty(txt) && txt.IsFormat(enDataFormat.FarbeInteger)) {
                        Color col = Color.FromArgb(int.Parse(txt));
                        txt = col.ColorName();
                    }
                    txt = LanguageTool.ColumnReplace(txt, column, style);
                    break;


                case enDataFormat.Schrift:
                    //    Develop.DebugPrint_NichtImplementiert();
                    //if (string.IsNullOrEmpty(Txt) || Txt.Substring(0, 1) != "{") { return Txt; }

                    //if (CompactView) { return string.Empty; }
                    //return BlueFont.Get(Txt).ReadableText();
                    return txt;

                case enDataFormat.Columns_für_LinkedCellDropdown:
                    // Hier kommt die Spalten-ID  an
                    if (string.IsNullOrEmpty(txt)) { return string.Empty; }
                    if (!int.TryParse(txt, out int ColKey)) { return "Columkey kann nicht geparsed werden"; }
                    Database LinkedDatabase = column.LinkedDatabase();
                    if (LinkedDatabase == null) { return "Datenbankverknüpfung fehlt"; }
                    ColumnItem C = LinkedDatabase.Column.SearchByKey(ColKey);
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

        public static QuickImage StandardErrorImage(int gr, enBildTextVerhalten bildTextverhalten) {

            switch (bildTextverhalten) {
                case enBildTextVerhalten.Fehlendes_Bild_zeige_Fragezeichen:
                    return QuickImage.Get("Fragezeichen|" + gr + "|||||200|||80");
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

                default:
                    return null;
            }
        }


        private static QuickImage StandardImage(ColumnItem column, string originalText, string replacedText, enShortenStyle style, enBildTextVerhalten bildTextverhalten) {

            // replacedText kann auch empty sein. z.B. wenn er nicht angezeigt wird

            if (bildTextverhalten == enBildTextVerhalten.Nur_Text) { return null; }
            if (style == enShortenStyle.HTML) { return null; }

            if (bildTextverhalten == enBildTextVerhalten.Nur_Bild) { replacedText = ValueReadable(column, originalText, style, enBildTextVerhalten.Nur_Text, true); }


            switch (column.Format) {
                case enDataFormat.Text:
                case enDataFormat.Text_mit_Formatierung:
                case enDataFormat.RelationText:
                    return null; // z.B. KontextMenu

                case enDataFormat.Bit:
                    if (originalText == true.ToPlusMinus()) {
                        if (column == column.Database.Column.SysCorrect) { return QuickImage.Get("Häkchen|16||||||||80"); }
                        return QuickImage.Get(enImageCode.Häkchen, 16);
                    } else if (originalText == false.ToPlusMinus()) {
                        if (column == column.Database.Column.SysCorrect) { return QuickImage.Get(enImageCode.Warnung, 16); }
                        return QuickImage.Get(enImageCode.Kreuz, 16);
                    } else if (originalText == "o" || replacedText == "O") {
                        return QuickImage.Get(enImageCode.Kreis2, 16);
                    } else if (originalText == "?") {
                        return QuickImage.Get(enImageCode.Fragezeichen, 16);
                    } else if (string.IsNullOrEmpty(replacedText)) { // Hier Replaced Text
                        return null;
                    } else {
                        return StandardErrorImage(16, bildTextverhalten);
                    }

                case enDataFormat.Button:
                    if (column == null) { return null; }// z.B. Dropdownmenu-Textfeld mit bereits definierten Icon
                    if (string.IsNullOrEmpty(replacedText)) { return null; }
                    return QuickImage.Get("Stern|16");


                case enDataFormat.BildCode:
                    if (column == null) { return null; }// z.B. Dropdownmenu-Textfeld mit bereits definierten Icon
                    if (string.IsNullOrEmpty(replacedText)) { return null; }

                    //var code = column.Prefix + originalText + column.Suffix;
                    if (column.BildCode_ConstantHeight > 0) { replacedText = replacedText + "|" + column.BildCode_ConstantHeight; }
                    QuickImage defaultImage = QuickImage.Get(replacedText);

                    if (defaultImage != null && !defaultImage.IsError) { return defaultImage; }
                    int gr = 16;
                    if (column.BildCode_ConstantHeight > 0) { gr = column.BildCode_ConstantHeight; }
                    return StandardErrorImage(gr, bildTextverhalten);

                case enDataFormat.FarbeInteger:

                    if (!string.IsNullOrEmpty(replacedText) && replacedText.IsFormat(enDataFormat.FarbeInteger)) {
                        Color col = Color.FromArgb(int.Parse(replacedText));
                        return QuickImage.Get(enImageCode.Kreis, 16, "", col.ToHTMLCode());
                    }
                    return null;


                //case enDataFormat.Relation:
                //    if (ImageCode != null) { return ImageCode; }
                //    if (!string.IsNullOrEmpty(Txt)) { return new clsRelation(Column, null, Txt).SymbolForReadableText(); }
                //    return null;



                case enDataFormat.Link_To_Filesystem:
                    if (replacedText.FileType() == enFileFormat.Unknown) { return StandardErrorImage(48, bildTextverhalten); }
                    return QuickImage.Get(replacedText.FileType(), 48);

                case enDataFormat.Schrift:
                    //  Develop.DebugPrint_NichtImplementiert();
                    //if (string.IsNullOrEmpty(Txt) || Txt.Substring(0, 1) != "{") { return defaultImage; }
                    // return Skin.BlueFont.Get(Txt).SymbolForReadableText();
                    return null;

                case enDataFormat.LinkedCell:
                case enDataFormat.Columns_für_LinkedCellDropdown:
                case enDataFormat.Values_für_LinkedCellDropdown:
                    return null;

                default:
                    return null;

            }
        }



        public static enAlignment StandardAlignment(ColumnItem column, enBildTextVerhalten bildTextverhalten) {

            switch (column.Align) {
                case enAlignmentHorizontal.Links:
                    return enAlignment.Top_Left;
                case enAlignmentHorizontal.Rechts:
                    return enAlignment.Top_Right;
                case enAlignmentHorizontal.Zentriert:
                    return enAlignment.HorizontalCenter;
            }


            switch (column.Format) {
                case enDataFormat.Ganzzahl:
                case enDataFormat.Gleitkommazahl:
                    return enAlignment.Top_Right;

                case enDataFormat.Bit:
                    if (bildTextverhalten == enBildTextVerhalten.Nur_Bild || bildTextverhalten == enBildTextVerhalten.Bild_oder_Text) { return enAlignment.Top_HorizontalCenter; }
                    return enAlignment.Top_Left;

                default:
                    return enAlignment.Top_Left;
            }

        }




        //public static string ValueForHTMLExport(ColumnItem Column, string Einstiegstext)
        //{
        //    switch (Column.Format)
        //    {

        //        case enDataFormat.Text:
        //        case enDataFormat.Text_Ohne_Kritische_Zeichen:
        //        case enDataFormat.Text_mit_Formatierung:
        //        case enDataFormat.Datum_und_Uhrzeit:
        //        case enDataFormat.Ganzzahl:
        //        case enDataFormat.Telefonnummer:
        //        case enDataFormat.Email:
        //        case enDataFormat.InternetAdresse:
        //        case enDataFormat.BildCode:
        //        case enDataFormat.Link_To_Filesystem:
        //        case enDataFormat.Values_für_LinkedCellDropdown:
        //        case enDataFormat.RelationText:
        //            // hier nix.
        //            break;


        //        case enDataFormat.Bit:
        //            if (Einstiegstext == true.ToPlusMinus())
        //            {
        //                return "Ja";
        //            }
        //            else if (Einstiegstext == false.ToPlusMinus())
        //            {
        //                return "Nein";
        //            }
        //            else if (Einstiegstext == "o" || Einstiegstext == "O")
        //            {
        //                return "Neutral";
        //            }
        //            else if (Einstiegstext == "?")
        //            {
        //                return "Unbekannt";
        //            }
        //            else
        //            {
        //                return Einstiegstext;
        //            }


        //        case enDataFormat.Gleitkommazahl:
        //            Einstiegstext = Einstiegstext.Replace(".", ",");
        //            break;

        //        case enDataFormat.Binärdaten_Bild:
        //        case enDataFormat.Binärdaten:
        //        case enDataFormat.Farbcode:
        //            Einstiegstext = "?";
        //            break;


        //        case enDataFormat.LinkedCell:
        //            Develop.DebugPrint("Fremdzelle dürfte hier nicht ankommen");
        //            break;


        //        //case enDataFormat.Relation:
        //        //    var tmp = new clsRelation(Column, null, Einstiegstext);
        //        //    return tmp.ReadableText();


        //        case enDataFormat.Columns_für_LinkedCellDropdown:
        //            return Draw_FormatedText_TextOf(Einstiegstext, null, Column, enShortenStyle.Unreplaced);

        //        default:
        //            Develop.DebugPrint(Column.Format);
        //            return "???";
        //    }




        //    Einstiegstext = Einstiegstext.Replace("\r\n", "<br>");
        //    Einstiegstext = Einstiegstext.Replace("\r", "<br>");
        //    Einstiegstext = Einstiegstext.Replace("\n", "<br>");
        //    Einstiegstext = Einstiegstext.Trim();
        //    Einstiegstext = Einstiegstext.Trim("<br>");
        //    Einstiegstext = Einstiegstext.Trim();
        //    Einstiegstext = Einstiegstext.Trim("<br>");

        //    return Einstiegstext;

        //}


    }
}
