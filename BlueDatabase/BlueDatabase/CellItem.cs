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
using System.Collections.Generic;
using System.Drawing;

namespace BlueDatabase
{


    /// <summary>
    /// Diese Klasse enthält nur das Aussehen und gibt keinerlei Events ab. Deswegen INTERNAL!
    /// </summary>
    public class CellItem
    {
        private string _value = string.Empty;


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
        /// Jede Zeile für sich richtig formatiert.
        /// </summary>
        /// <returns></returns>
        public static List<string> ValuesReadable(ColumnItem column, RowItem Row, enShortenStyle Style)
        {

            if (column.Format == enDataFormat.LinkedCell)
            {
                //var LinkedData = CellCollection.LinkedCellData(column, Row, false, false);
                //if (LinkedData.Item1 != null && LinkedData.Item2 != null) { return ValuesReadable(LinkedData.Item1, LinkedData.Item2, Style); }
                //return new List<string>();
                Develop.DebugPrint(enFehlerArt.Warnung, "LinkedCell sollte hier nicht ankommen.");
            }

            var ret = new List<string>();

            if (!column.MultiLine)
            {
                ret.Add(ValueReadable(column, Row.CellGetString(column), Style, column.CompactView));
                return ret;
            }

            var x = Row.CellGetList(column);
            foreach (var thisstring in x)
            {
                ret.Add(ValueReadable(column, thisstring, Style, column.CompactView));
            }

            if (x.Count == 0)
            {
                var tmp = ValueReadable(column, string.Empty, Style, column.CompactView);
                if (!string.IsNullOrEmpty(tmp)) { ret.Add(tmp); }
            }


            return ret;
        }



        /// <summary>
        /// Gibt eine einzelne Zeile richtig formatiert zurück. Zeilenumbrüche werden eleminiert.
        /// </summary>
        /// <param name="column"></param>
        /// <param name="Txt"></param>
        /// <param name="Style"></param>
        /// <returns></returns>
        public static string ValueReadable(ColumnItem column, string Txt, enShortenStyle Style, bool CompactView)
        {

            switch (column.Format)
            {
                case enDataFormat.Text:
                case enDataFormat.Text_mit_Formatierung:
                case enDataFormat.Datum_und_Uhrzeit:
                case enDataFormat.Link_To_Filesystem:
                case enDataFormat.Gleitkommazahl:
                case enDataFormat.Ganzzahl:
                case enDataFormat.Values_für_LinkedCellDropdown:
                case enDataFormat.RelationText:
                case enDataFormat.LinkedCell:  // Bei LinkedCell kommt direkt der Text der verlinkten Zelle an
                    //if (Txt == null || string.IsNullOrEmpty(Txt)) { return string.Empty; }
                    Txt = LanguageTool.ColumnReplace(Txt, column, Style, CompactView);
                    Txt = Txt.Replace("\r\n", " ");
                    Txt = Txt.Replace("\r", " ");
                    break;

                case enDataFormat.BildCode:
                case enDataFormat.Button:
                    if (CompactView && Style != enShortenStyle.HTML) { return string.Empty; }
                    Txt = LanguageTool.ColumnReplace(Txt, column, Style, CompactView);
                    break;

                case enDataFormat.Bit:
                    if (CompactView && Style != enShortenStyle.HTML) { return string.Empty; }
                    if (Txt == true.ToPlusMinus())
                    {
                        Txt = "Ja";
                        if (column == column.Database.Column.SysCorrect) { return "Ok"; }
                        if (column == column.Database.Column.SysLocked) { return "gesperrt"; }

                    }
                    else if (Txt == false.ToPlusMinus())
                    {
                        Txt = "Nein";
                        if (column == column.Database.Column.SysCorrect) { return "fehlerhaft"; }
                        if (column == column.Database.Column.SysLocked) { return "bearbeitbar"; }
                    }
                    else if (Txt == "o" || Txt == "O")
                    {
                        Txt = "Neutral";
                    }
                    else if (Txt == "?")
                    {
                        Txt = "Unbekannt";
                    }
                    Txt = LanguageTool.ColumnReplace(Txt, column, Style, CompactView);
                    break;


                case enDataFormat.FarbeInteger:
                    if (Style == enShortenStyle.HTML) { break; }
                    if (CompactView) { return string.Empty; }
                    if (!string.IsNullOrEmpty(Txt) && Txt.IsFormat(enDataFormat.FarbeInteger))
                    {
                        var col = Color.FromArgb(int.Parse(Txt));
                        Txt = col.ColorName();
                    }
                    Txt = LanguageTool.ColumnReplace(Txt, column, Style, CompactView);
                    break;


                case enDataFormat.Schrift:
                    if (Style == enShortenStyle.HTML) { break; }
                    //    Develop.DebugPrint_NichtImplementiert();
                    //if (string.IsNullOrEmpty(Txt) || Txt.Substring(0, 1) != "{") { return Txt; }

                    //if (CompactView) { return string.Empty; }
                    //return BlueFont.Get(Txt).ReadableText();
                    return Txt;

                case enDataFormat.Columns_für_LinkedCellDropdown:
                    // Hier kommt die Spalten-ID  an
                    if (string.IsNullOrEmpty(Txt)) { return string.Empty; }
                    if (!int.TryParse(Txt, out var ColKey)) { return "Columkey kann nicht geparsed werden"; }
                    var LinkedDatabase = column.LinkedDatabase();
                    if (LinkedDatabase == null) { return "Datenbankverknüpfung fehlt"; }
                    var C = LinkedDatabase.Column.SearchByKey(ColKey);
                    if (C == null) { return "Columnkey nicht gefunden"; }

                    Txt = LanguageTool.ColumnReplace(C.ReadableText(), column, Style, CompactView);
                    break;

                default:
                    Develop.DebugPrint(column.Format);
                    break;
            }

            if (Style != enShortenStyle.HTML) { return Txt; }


            if (Txt.Contains("\r")) { Develop.DebugPrint(enFehlerArt.Info, "\\r enthalten:" + Txt); }
            if (Txt.Contains("\n")) { Develop.DebugPrint(enFehlerArt.Info, "\\n enthalten:" + Txt); }


            while (Txt.StartsWith(" ") || Txt.StartsWith("<br>") || Txt.EndsWith(" ") || Txt.EndsWith("<br>"))
            {
                Txt = Txt.Trim();
                Txt = Txt.Trim("<br>");
            }

            return Txt;
        }



        public static QuickImage StandardImage(ColumnItem column, string Txt)
        {

            switch (column.Format)
            {
                case enDataFormat.Text:
                case enDataFormat.Text_mit_Formatierung:
                case enDataFormat.RelationText:
                    return null; // z.B. KontextMenu

                case enDataFormat.Bit:
                    if (Txt == true.ToPlusMinus())
                    {
                        if (column == column.Database.Column.SysCorrect) { return QuickImage.Get("Häkchen|16||||||||80"); }
                        //if (column == column.Database.Column.SysLocked) { return QuickImage.Get(enImageCode.Schloss, 16,"00AA00",string.Empty); }
                        return QuickImage.Get(enImageCode.Häkchen, 16);
                    }
                    else if (Txt == false.ToPlusMinus())
                    {
                        if (column == column.Database.Column.SysCorrect) { return QuickImage.Get(enImageCode.Warnung, 16); }
                        //if (column == column.Database.Column.SysLocked) { return QuickImage.Get(enImageCode.Schlüssel, 16, "FFBB00", string.Empty); }
                        return QuickImage.Get(enImageCode.Kreuz, 16);
                    }
                    else if (Txt == "o" || Txt == "O")
                    {
                        return QuickImage.Get(enImageCode.Kreis2, 16);
                    }
                    else if (Txt == "?")
                    {
                        return QuickImage.Get(enImageCode.Fragezeichen, 16);
                    }
                    else if (string.IsNullOrEmpty(Txt))
                    {
                        return null;
                    }
                    else
                    {
                        return QuickImage.Get(enImageCode.Kritisch, 16);
                    }

                case enDataFormat.Button:
                    if (column == null) { return null; }// z.B. Dropdownmenu-Textfeld mit bereits definierten Icon
                    if (string.IsNullOrEmpty(Txt)) { return null; }
                    return  QuickImage.Get("Stern|16");


                case enDataFormat.BildCode:
                    if (column == null) { return null; }// z.B. Dropdownmenu-Textfeld mit bereits definierten Icon
                    if (string.IsNullOrEmpty(Txt)) { return null; }

                    var code = column.Prefix + Txt + column.Suffix;
                    if (column.BildCode_ConstantHeight > 0) { code = code + "|" + column.BildCode_ConstantHeight; }
                    var defaultImage = QuickImage.Get(code);

                    if (defaultImage != null && !defaultImage.IsError) { return defaultImage; }



                    var gr = 16;

                    if (column.BildCode_ConstantHeight > 0) { gr = column.BildCode_ConstantHeight; }
                    switch (column.BildCode_ImageNotFound)
                    {
                        case enImageNotFound.Show_Error_QuestionMark:
                            return QuickImage.Get("Fragezeichen|" + gr + "|||||200|||80");
                        case enImageNotFound.Show_Yellow_Circle:
                            return QuickImage.Get("Kreis2|" + gr);
                        case enImageNotFound.Show_Red_Cross:
                            return QuickImage.Get("Kreuz|" + gr);
                        case enImageNotFound.Show_Green_Checkmark:
                            return QuickImage.Get("Häkchen|" + gr);

                        case enImageNotFound.Show_Info_Sign:
                            return QuickImage.Get("Information|" + gr);
                        case enImageNotFound.Show_Warning_Sign:
                            return QuickImage.Get("Warnung|" + gr);
                        case enImageNotFound.Show_Critical_Sign:
                            return QuickImage.Get("Kritisch|" + gr);

                        default:
                            return null;
                    }




                case enDataFormat.FarbeInteger:

                    if (!string.IsNullOrEmpty(Txt) && Txt.IsFormat(enDataFormat.FarbeInteger))
                    {
                        var col = Color.FromArgb(int.Parse(Txt));
                        return QuickImage.Get(enImageCode.Kreis, 16, "", col.ToHTMLCode());
                    }
                    return null;


                //case enDataFormat.Relation:
                //    if (ImageCode != null) { return ImageCode; }
                //    if (!string.IsNullOrEmpty(Txt)) { return new clsRelation(Column, null, Txt).SymbolForReadableText(); }
                //    return null;



                case enDataFormat.Link_To_Filesystem:
                    if (Txt.FileType() == enFileFormat.Unknown) { return null; }
                    return QuickImage.Get(Txt.FileType(), 48);

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



        public static enAlignment StandardAlignment(ColumnItem column, bool compact)
        {


            switch (column.Align)
            {
                case enAlignmentHorizontal.Links: return enAlignment.Top_Left;
                case enAlignmentHorizontal.Rechts: return enAlignment.Top_Right;
                case enAlignmentHorizontal.Zentriert: return enAlignment.HorizontalCenter;
            }


            switch (column.Format)
            {
                case enDataFormat.Ganzzahl:
                case enDataFormat.Gleitkommazahl:
                    return enAlignment.Top_Right;

                case enDataFormat.Bit:
                    if (compact) { return enAlignment.Top_HorizontalCenter; }
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
