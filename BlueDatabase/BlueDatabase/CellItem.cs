#region BlueElements - a collection of useful tools, database and controls
// Authors: 
// Christian Peter
// 
// Copyright (c) 2019 Christian Peter
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
using System.Drawing;

namespace BlueDatabase
{


    /// <summary>
    /// Diese Klasse enthält nur das Aussehen und gibt keinerlei Events ab. Deswegen INTERNAL!
    /// </summary>
    public class CellItem
    {
        string _value = string.Empty;


        #region Konstruktor

        public CellItem(string value, int width, int height)
        {
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

        public string Value
        {
            get
            {
                return _value;
            }
            set
            {
                if (_value == value) { return; }

                _value = value;
                InvalidateSize();

            }
        }

        #endregion

        internal void InvalidateSize()
        {
            Size = Size.Empty;
        }


        /// <summary>
        /// Gibt den kompletten Inhalt der Zelle als lesbaren Text zurück. Jede Zeile für sich richtig formatiert.
        /// </summary>
        /// <returns></returns>
        public static string ValueCompleteReadable(ColumnItem column, RowItem Row, enShortenStyle Style)
        {

            if (column.Format == enDataFormat.LinkedCell)
            {
              column.Database.Cell.LinkedCellData(column, Row, out var LCColumn, out var LCrow);
                if (LCColumn != null && LCrow != null) { return ValueCompleteReadable(column, Row, Style); }
                return string.Empty;
            }


            if (!column.MultiLine)
            {
                return ValueReadable(Row.CellGetString(column), column, Style);
            }

            var x = Row.CellGetList(column);
            string txt = string.Empty;

            foreach (var thisstring in x)
            {
                if (!string.IsNullOrEmpty(txt)) { txt = txt + "\r"; }

                txt = txt + ValueReadable(thisstring, column, Style);
            }
            return txt;
        }


        //public static string CleanFormat(string value, ColumnItem column, RowItem row)
        //{
        //    if (string.IsNullOrEmpty(value)) { return string.Empty; }
        //    if (column == null) { return string.Empty; }
        //    if (row == null) { return string.Empty; }

        //    switch (column.Format)
        //    {
        //        case enDataFormat.Text:
        //        case enDataFormat.InternetAdresse:
        //        case enDataFormat.RelationText:
        //            return value;

        //        case enDataFormat.Text_Ohne_Kritische_Zeichen:
        //        case enDataFormat.Text_mit_Formatierung:
        //        case enDataFormat.BildCode:
        //            return value.ReduceToChars(column.Format.AllowedChars());


        //        case enDataFormat.Email:
        //            return value.ReduceToChars(Constants.Char_Buchstaben + Constants.Char_Buchstaben.ToUpper() + Constants.Char_Numerals + "@_-.");

        //        case enDataFormat.Telefonnummer:
        //            if (value.ReduceToChars("+/().- " + Constants.Char_Numerals) != value) { return string.Empty; }
        //            return value.ReduceToChars(Constants.Char_Numerals + "+");


        //        case enDataFormat.Datum_und_Uhrzeit:
        //            if (value.Length == 6) { value = value + "1800"; }
        //            if (!value.IsFormat(column.Format)) { return string.Empty; }
        //            if (value.Length != 10) { return string.Empty; }// Nicht definiert!
        //            return modAllgemein.DateTimeParse(value).ToString("yyyy-MM-dd HH:mm:ss");

        //        case enDataFormat.Link_To_Filesystem:
        //            return value;


        //        case enDataFormat.Ganzzahl:
        //            if (value.ReduceToChars("- " + Constants.Char_Numerals) != value) { return string.Empty; }
        //            return value.ReduceToChars("-" + Constants.Char_Numerals).TrimStart('0');

        //        case enDataFormat.Gleitkommazahl:
        //            if (value.ReduceToChars(Constants.Char_Numerals + "-,. ") != value) { return string.Empty; }
        //            value = value.ReduceToChars(Constants.Char_Numerals + "-,.").Replace(".", ","); if (value == ",") { return string.Empty; }
        //            return value;

        //        case enDataFormat.Bit:
        //            return string.Empty;

        //        case enDataFormat.Columns_für_LinkedCellDropdown:
        //            if (int.TryParse(value, out var ColKey))
        //            {
        //                var C = column.LinkedDatabase().Column.SearchByKey(ColKey);
        //                if (C != null) { return C.ReadableText(); }
        //            }


        //            //column.Database.Cell.LinkedCellData(column, row, out var ContentHolderCellColumn, out _);
        //            //if (ContentHolderCellColumn != null) { return column.ReadableText(); }
        //            return value;

        //        default:
        //            Develop.DebugPrint(column.Format);
        //            return value;
        //    }

        //}

        /// <summary>
        /// Gibt eine Einzelne Zeile richtig formatiert zurück.
        /// </summary>
        /// <param name="Txt"></param>
        /// <param name="column"></param>
        /// <param name="Style"></param>
        /// <returns></returns>
        public static string ValueReadable(string Txt, ColumnItem column, enShortenStyle Style)
        {

            switch (column.Format)
            {
                case enDataFormat.Text:
                case enDataFormat.Text_mit_Formatierung:
                case enDataFormat.Text_Ohne_Kritische_Zeichen:
                case enDataFormat.Datum_und_Uhrzeit:
                case enDataFormat.Binärdaten:
                case enDataFormat.Link_To_Filesystem:
                case enDataFormat.Telefonnummer:
                case enDataFormat.Email:
                case enDataFormat.InternetAdresse:
                case enDataFormat.Gleitkommazahl:
                case enDataFormat.Ganzzahl:
                case enDataFormat.Values_für_LinkedCellDropdown:
                case enDataFormat.RelationText:
                case enDataFormat.LinkedCell:  // Bei LinkedCell kommt direkt der Text der verlinkten Zelle an
                    if (Txt == null || string.IsNullOrEmpty(Txt)) { return string.Empty; }
                    Txt = ColumnItem.ColumnReplace(Txt, column, Style);
                    return Txt.Replace("\r\n", " ");


                case enDataFormat.BildCode:
                    if (column.CompactView && Style != enShortenStyle.HTML) { return string.Empty; }
                    Txt = ColumnItem.ColumnReplace(Txt, column, Style);
                    return Txt; 


                case enDataFormat.Bit:
                    if (column.CompactView && Style != enShortenStyle.HTML) { return string.Empty; }
                    if (Txt == true.ToPlusMinus())
                    {
                        Txt =  "Ja";
                    }
                    else if (Txt == false.ToPlusMinus())
                    {
                        Txt = "Nein";
                    }
                    else if (Txt == "o" || Txt == "O")
                    {
                        Txt = "Neutral";
                    }
                    else if (Txt == "?")
                    {
                        Txt = "Unbekannt";
                    }
                    Txt = ColumnItem.ColumnReplace(Txt, column, Style);
                    return Txt;




                case enDataFormat.Farbcode:
                    if (Style == enShortenStyle.HTML) { return Txt; }
                    if (column.CompactView) { return string.Empty; }
                    if (!string.IsNullOrEmpty(Txt) && Txt.IsFormat(enDataFormat.Farbcode))
                    {
                        var col = Color.FromArgb(int.Parse(Txt));
                        Txt =  col.ColorName();
                    }
                    Txt = ColumnItem.ColumnReplace(Txt, column, Style);
                    return Txt;

                case enDataFormat.Binärdaten_Bild:
                    return string.Empty;


                case enDataFormat.Schrift:
                    if (Style == enShortenStyle.HTML) { return Txt; }
                    Develop.DebugPrint_NichtImplementiert();
                    //if (string.IsNullOrEmpty(Txt) || Txt.Substring(0, 1) != "{") { return Txt; }

                    //if (Column.CompactView) { return string.Empty; }
                    //return BlueFont.Get(Txt).ReadableText();
                    return string.Empty;

                case enDataFormat.Columns_für_LinkedCellDropdown:
                    // Hier kommt die Spalten-ID  an
                    if (string.IsNullOrEmpty(Txt)) { return string.Empty; }
                    if (!int.TryParse(Txt, out var ColKey)) { return "Columkey kann nicht geparsed werden"; }
                    var LinkedDatabase = column.LinkedDatabase();
                    if (LinkedDatabase == null) { return "Datenbankverknüpfung fehlt"; }
                    var C = LinkedDatabase.Column.SearchByKey(ColKey);
                    if (C == null) { return "Columnkey nicht gefunden"; }
  
                    Txt = ColumnItem.ColumnReplace(C.ReadableText(), column, Style);
                    return Txt;

                default:
                    Develop.DebugPrint(column.Format);
                    return Txt;
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
