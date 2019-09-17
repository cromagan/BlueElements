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

using System;
using System.Linq;
using System.Text.RegularExpressions;
using BlueBasics.Enums;
using static BlueBasics.modAllgemein;

namespace BlueBasics
{
    public static class DataFormat
    {




        public static bool MultilinePossible(this enDataFormat format)
        {
            switch (format)
            {
                case enDataFormat.Text:
                case enDataFormat.Ganzzahl:
                case enDataFormat.Gleitkommazahl:
                case enDataFormat.BildCode:
                case enDataFormat.Datum_und_Uhrzeit:
                case enDataFormat.RelationText:
                case enDataFormat.Text_mit_Formatierung:
                case enDataFormat.Link_To_Filesystem:
                case enDataFormat.LinkedCell:
                    return true;
                default:
                    return false;
            }
        }

        public static bool CompactPossible(this enDataFormat format)
        {
            switch (format)
            {
                case enDataFormat.Bit:
                case enDataFormat.BildCode:
                case enDataFormat.Farbcode:
                case enDataFormat.Schrift:
                    return true;
                default:
                    return false;
            }
        }



        public static bool SaveSizeData(this enDataFormat format)
        {
            switch (format)
            {
                case enDataFormat.Text:
                case enDataFormat.Bit:
                case enDataFormat.Ganzzahl:
                case enDataFormat.Gleitkommazahl:
                case enDataFormat.BildCode:
                case enDataFormat.Datum_und_Uhrzeit:
                case enDataFormat.Farbcode:
                case enDataFormat.RelationText:
                case enDataFormat.Schrift:
                case enDataFormat.Text_mit_Formatierung:
                case enDataFormat.Values_für_LinkedCellDropdown:
                    return true;
                default:
                    return false;
            }
        }


        public static bool SpellCheckingPossible(this enDataFormat format)
        {
            switch (format)
            {
                case enDataFormat.Text:
                case enDataFormat.Text_mit_Formatierung:
                case enDataFormat.RelationText:
                    return true;
                default: return false;
            }
        }



        public static bool TextboxEditPossible(this enDataFormat format)
        {
            switch (format)
            {
                case enDataFormat.Text:
                case enDataFormat.Ganzzahl:
                case enDataFormat.Gleitkommazahl:
                case enDataFormat.BildCode:
                case enDataFormat.Datum_und_Uhrzeit:
                case enDataFormat.Text_mit_Formatierung:
                case enDataFormat.LinkedCell:
                case enDataFormat.RelationText:
                    return true;

                default:
                    return false;
            }
        }




        /// <summary>
        /// Prüft, ob ein String der geforderte Typ sein kann.
        /// Dabei wird die Textlänge, die Schablone und die erlaubten Zeichen geprüft.
        /// Ein Logigcheck (z.B. ob ein Datum gültig ist) wird ebenfalls ausgeführt.
        /// </summary>
        /// <returns></returns>
        /// <remarks></remarks>
        public static bool IsFormat(this string Txt, enDataFormat Format, bool MultiLine)
        {

            if (MultiLine)
            {
                var ex = Txt.SplitByCR();
                return ex.All(ThisString => string.IsNullOrEmpty(ThisString) || ThisString.IsFormat(Format));
            }

            return Txt.IsFormat(Format);
        }

        /// <summary>
        /// Prüft, ob ein String der geforderte Typ sein kann.
        /// Dabei wird die Textlänge, die Schablone und die erlaubten Zeichen geprüft.
        /// Ein Logigcheck (z.B. ob ein Datum gültig ist) wird ebenfalls ausgeführt.
        /// </summary>
        /// <returns></returns>
        /// <remarks></remarks>
        public static bool IsFormat(this string Txt, enDataFormat Format)
        {
            if (!Text_LängeCheck(Txt, Format)) { return false; }
            if (!Text_SchabloneCheck(Txt, Format)) { return false; }
            if (!Txt.ContainsOnlyChars(AllowedChars(Format))) { return false; }
            if (!Text_ValueCheck(Txt, Format)) { return false; }
            return true;
        }



        public static string AllowedChars(this enDataFormat format)
        {

            switch (format)
            {
                case enDataFormat.Text:
                case enDataFormat.Text_mit_Formatierung:
                case enDataFormat.BildCode:
                case enDataFormat.Columns_für_LinkedCellDropdown:
                case enDataFormat.Values_für_LinkedCellDropdown:
                case enDataFormat.RelationText:
                    return Constants.Char_Buchstaben + Constants.Char_Buchstaben.ToUpper() + Constants.Char_Numerals + Constants.Char_Satzeichen + Constants.Char_Sonderzeichen;

                case enDataFormat.Bit:
                    return "+-";

                case enDataFormat.Ganzzahl:
                case enDataFormat.Farbcode:
                    return Constants.Char_Numerals + "-";

                case enDataFormat.Gleitkommazahl:
                    return Constants.Char_Numerals + "-,";

                case enDataFormat.Datum_und_Uhrzeit:
                    return Constants.Char_Numerals + " :.";

                case enDataFormat.Link_To_Filesystem:
                    return Constants.Char_Buchstaben + Constants.Char_Buchstaben.ToUpper() + Constants.Char_Numerals + ",.\\:_ +-()'";

                case enDataFormat.LinkedCell:
                    Develop.DebugPrint(enFehlerArt.Warnung, "LinkedCell kann nicht geprüft werden.");
                    return string.Empty;


                default:
                    Develop.DebugPrint(format);
                    return string.Empty;
            }

        }



        public static bool ExportableForLayout(this enDataFormat format)
        {
            switch (format)
            {
                case enDataFormat.Text:
                case enDataFormat.Bit:
                case enDataFormat.Ganzzahl:
                case enDataFormat.Gleitkommazahl:
                case enDataFormat.BildCode:
                case enDataFormat.Datum_und_Uhrzeit:
                case enDataFormat.RelationText:
                case enDataFormat.Schrift:
                case enDataFormat.Text_mit_Formatierung:
                case enDataFormat.Link_To_Filesystem:
                case enDataFormat.LinkedCell:
                case enDataFormat.Columns_für_LinkedCellDropdown:
                case enDataFormat.Values_für_LinkedCellDropdown:
                    return true;
                default:
                    return false;
            }
        }



        public static int Text_MaximaleLänge(this enDataFormat format)
        {
            switch (format)
            {
                case enDataFormat.Text:
                case enDataFormat.Text_mit_Formatierung:
                case enDataFormat.Gleitkommazahl:
                case enDataFormat.Ganzzahl:
                case enDataFormat.BildCode:
                case enDataFormat.RelationText:
                case enDataFormat.Farbcode:
                case enDataFormat.LinkedCell:
                case enDataFormat.Columns_für_LinkedCellDropdown:
                case enDataFormat.Values_für_LinkedCellDropdown:
                    return -1;

                case enDataFormat.Bit:
                    return 1;

                case enDataFormat.Link_To_Filesystem:
                    return 260;

                case enDataFormat.Datum_und_Uhrzeit:
                    return 19;

                default:
                    Develop.DebugPrint(format);
                    return -1;
            }



        }

        public static bool Text_LängeCheck(string TXT, enDataFormat format)
        {
            var ml = Text_MaximaleLänge(format);
            var il = TXT.Length;
            if (ml > -1 && il > ml) { return false; }

            switch (format)
            {

                case enDataFormat.Text:
                case enDataFormat.Text_mit_Formatierung:
                case enDataFormat.RelationText:
                case enDataFormat.Columns_für_LinkedCellDropdown:
                case enDataFormat.Values_für_LinkedCellDropdown:
                    return true;

                case enDataFormat.Datum_und_Uhrzeit:
                    if (il == 19) { return true; }
                    if (il == 16) { return true; } // 01.01.2011 11:11
                    return Convert.ToBoolean(il == 10);

                case enDataFormat.Bit:
                    return Convert.ToBoolean(il == 1);

                case enDataFormat.Ganzzahl:
                case enDataFormat.Gleitkommazahl:
                case enDataFormat.Farbcode:
                case enDataFormat.BildCode:
                    return Convert.ToBoolean(il > 0);


                case enDataFormat.LinkedCell:
                    Develop.DebugPrint(enFehlerArt.Warnung, "LinkedCell kann nicht geprüft werden.");
                    return true;

                case enDataFormat.Link_To_Filesystem:
                    return Convert.ToBoolean(il < 257);


                default:
                    Develop.DebugPrint(format);
                    return true;

            }
        }



        public static bool Text_ValueCheck(string TXT, enDataFormat Format)
        {

            switch (Format)
            {
                case enDataFormat.Text:
                case enDataFormat.Text_mit_Formatierung:
                case enDataFormat.BildCode:
                case enDataFormat.Columns_für_LinkedCellDropdown:
                case enDataFormat.Values_für_LinkedCellDropdown:
                case enDataFormat.Bit:
                case enDataFormat.RelationText:
                case enDataFormat.Link_To_Filesystem:
                    return true;

                case enDataFormat.Datum_und_Uhrzeit:
                    return DateTimeTryParse(TXT, out _);

                case enDataFormat.Gleitkommazahl:
                case enDataFormat.Ganzzahl:
                case enDataFormat.Farbcode:
                    return true; // Macht Alls der Text_SchabloneCheck

                case enDataFormat.LinkedCell:
                    Develop.DebugPrint(enFehlerArt.Warnung, "LinkedCell kann nicht geprüft werden.");
                    return true;


                default:
                    Develop.DebugPrint(Format);
                    return true;
            }
        }

        /// <summary>
        /// Gibt zurück, ob der Text in die vordefinierte Schablone paßt.
        /// </summary>
        /// <param name="TXT"></param>
        /// <param name="Format"></param>
        /// <returns></returns>
        /// <remarks></remarks>
        public static bool Text_SchabloneCheck(string TXT, enDataFormat Format)
        {

            switch (Format)
            {
                case enDataFormat.Text:
                case enDataFormat.Text_mit_Formatierung:
                case enDataFormat.BildCode:
                case enDataFormat.Columns_für_LinkedCellDropdown:
                case enDataFormat.Values_für_LinkedCellDropdown:
                case enDataFormat.RelationText:
                case enDataFormat.Link_To_Filesystem:
                    return true;


                case enDataFormat.Bit:
                    return Convert.ToBoolean(TXT.Length == 1);

                case enDataFormat.Gleitkommazahl:
                    if (TXT == "0") { return true; }
                    if (TXT == "-") { return false; }
                    if (TXT.Length > 1 && TXT.Substring(0, 2) == "00") { return false; }
                    if (TXT.Length > 2 && TXT.Substring(0, 3) == "-00") { return false; }
                    if (TXT.Length > 2 && TXT.IndexOf("-", 1) > -1) { return false; }
                    return true;



                case enDataFormat.Ganzzahl:
                case enDataFormat.Farbcode:
                    if (TXT == "0") { return true; }
                    if (TXT == "-") { return false; }
                    if (!string.IsNullOrEmpty(TXT) && TXT.Substring(0, 1) == "0") { return false; }
                    if (TXT.Length > 1 && TXT.Substring(0, 2) == "-0") { return false; }
                    if (TXT.Length > 2 && TXT.IndexOf("-", 1) > -1) { return false; }
                    return true;


                case enDataFormat.Datum_und_Uhrzeit:
                    if (new Regex(@"^\d{2}.\d{2}.\d{4}$").IsMatch(TXT)) { return true; }
                    if (new Regex(@"^\d{2}.\d{2}.\d{4} \d{2}:\d{2}:\d{2}$").IsMatch(TXT)) { return true; }
                    if (new Regex(@"^\d{2}.\d{2}.\d{4} \d{2}:\d{2}$").IsMatch(TXT)) { return true; }
                    return false;

                case enDataFormat.LinkedCell:
                    Develop.DebugPrint(enFehlerArt.Warnung, "LinkedCell kann nicht geprüft werden.");
                    return true;

                default:
                    Develop.DebugPrint(Format);
                    return true;
            }
        }






        public static string CompareKey(string IsValue, enDataFormat Format)
        {
            var CompareKey_S_OK = Constants.SecondSortChar + "X";
            var CompareKey_S_NOK = Constants.SecondSortChar + "A";


            switch (Format)
            {
                case enDataFormat.Ganzzahl:
                    if (string.IsNullOrEmpty(IsValue)) { return CompareKey_S_NOK + "0000000000"; }

                    if (IsValue.IsNumeral())
                    {
                        return CompareKey_S_OK + IsValue.Nummer(10);
                    }
                    else
                    {
                        return CompareKey_S_NOK + IsValue;
                    }

                case enDataFormat.Bit:
                case enDataFormat.Farbcode:
                case enDataFormat.Schrift:
                    return Constants.SecondSortChar + IsValue;

                case enDataFormat.Text:
                case enDataFormat.BildCode:
                case enDataFormat.Text_mit_Formatierung:
                case enDataFormat.Link_To_Filesystem:
                case enDataFormat.Values_für_LinkedCellDropdown:
                case enDataFormat.RelationText:
                    if (string.IsNullOrEmpty(IsValue)) { return string.Empty; }

                    IsValue = IsValue.ToLower();
                    IsValue = IsValue.Replace("ä", "a");
                    IsValue = IsValue.Replace("ö", "o");
                    IsValue = IsValue.Replace("ü", "u");

                    IsValue = IsValue.Replace("á", "a");
                    IsValue = IsValue.Replace("ó", "o");
                    IsValue = IsValue.Replace("ú", "u");
                    IsValue = IsValue.Replace("í", "i");
                    IsValue = IsValue.Replace("é", "e");

                    IsValue = IsValue.Replace("à", "a");
                    IsValue = IsValue.Replace("ò", "o");
                    IsValue = IsValue.Replace("ù", "u");
                    IsValue = IsValue.Replace("ì", "i");
                    IsValue = IsValue.Replace("è", "e");

                    IsValue = IsValue.Replace("â", "a");
                    IsValue = IsValue.Replace("ô", "o");
                    IsValue = IsValue.Replace("û", "u");
                    IsValue = IsValue.Replace("î", "i");
                    IsValue = IsValue.Replace("ê", "e");

                    IsValue = IsValue.Replace("ž", "z");

                    IsValue = IsValue.Replace("ß", "s");

                    IsValue = IsValue.TrimStart("\"");
                    IsValue = IsValue.TrimStart("'");
                    IsValue = IsValue.TrimStart(" ");
                    return Constants.SecondSortChar + IsValue;

                case enDataFormat.Gleitkommazahl:
                    if (string.IsNullOrEmpty(IsValue)) { return "0000000000,0"; }

                    if (IsValue.IsDouble())
                    {
                        return CompareKey_S_OK + double.Parse(IsValue).Nummer(10, 1);
                    }
                    else
                    {
                        return CompareKey_S_NOK + IsValue;
                    }


                case enDataFormat.Datum_und_Uhrzeit:

                    if (DateTimeTryParse(IsValue, out var d))
                    {
                        return CompareKey_S_OK + d.ToString("u");
                    }
                    else
                    {
                        return CompareKey_S_NOK + IsValue;
                    }


                case enDataFormat.LinkedCell:
                case enDataFormat.Columns_für_LinkedCellDropdown:
                    // Fremdzellen setzen sich aus verschiedenen Spalten zusammen, also nur ganz primitives zurückgeben
                    return Constants.SecondSortChar + IsValue;


                default:
                    Develop.DebugPrint(Format);
                    return Constants.SecondSortChar + IsValue;
            }
        }







        public static bool Autofilter_möglich(this enDataFormat format)
        {
            switch (format)
            {
                case enDataFormat.Text:
                case enDataFormat.Bit:
                case enDataFormat.Ganzzahl:
                case enDataFormat.Gleitkommazahl:
                case enDataFormat.BildCode:
                case enDataFormat.Datum_und_Uhrzeit:
                case enDataFormat.Farbcode:
                case enDataFormat.Schrift:
                case enDataFormat.Text_mit_Formatierung:
                case enDataFormat.Link_To_Filesystem:
                case enDataFormat.LinkedCell:
                case enDataFormat.Columns_für_LinkedCellDropdown:
                case enDataFormat.Values_für_LinkedCellDropdown:
                case enDataFormat.RelationText:
                    return true;
                default:
                    return false;
            }
        }




        public static bool NeedUTF8(this enDataFormat format)
        {
            switch (format)
            {
                case enDataFormat.Text:
                case enDataFormat.RelationText:
                case enDataFormat.Text_mit_Formatierung:
                    return true;
                default:
                    return false;
            }
        }




        public static bool NeedTargetDatabase(this enDataFormat format)
        {
            switch (format)
            {
                case enDataFormat.LinkedCell:
                case enDataFormat.Columns_für_LinkedCellDropdown:
                case enDataFormat.Values_für_LinkedCellDropdown:
                    return true;
                default:
                    return false;
            }
        }

        public static bool IsZahl(this enDataFormat format)
        {
            switch (format)
            {
                case enDataFormat.Ganzzahl:
                case enDataFormat.Gleitkommazahl:
                    return true;
                default:
                    return false;
            }
        }


        public static bool DropdownItemsOfOtherCellsAllowed(this enDataFormat format)
        {
            switch (format)
            {
                case enDataFormat.Text:
                case enDataFormat.Ganzzahl:
                case enDataFormat.Gleitkommazahl:
                case enDataFormat.BildCode:
                case enDataFormat.Datum_und_Uhrzeit:
                case enDataFormat.Farbcode:
                case enDataFormat.Schrift:
                case enDataFormat.Text_mit_Formatierung:
                case enDataFormat.LinkedCell:
                case enDataFormat.RelationText:
                    return true;
                default:
                    return false;
            }
        }

        public static bool NeedLinkedKeyKennung(this enDataFormat format)
        {
            switch (format)
            {
                case enDataFormat.Columns_für_LinkedCellDropdown:
                    return true;
                default:
                    return false;
            }
        }



        public static bool DropdownUnselectAllAllowed(this enDataFormat format)
        {
            switch (format)
            {
                case enDataFormat.Text:
                case enDataFormat.Ganzzahl:
                case enDataFormat.Gleitkommazahl:
                case enDataFormat.BildCode:
                case enDataFormat.Datum_und_Uhrzeit:
                case enDataFormat.Farbcode:
                case enDataFormat.RelationText:
                case enDataFormat.Schrift:
                case enDataFormat.Text_mit_Formatierung:
                case enDataFormat.Link_To_Filesystem:
                case enDataFormat.LinkedCell:
                    return true;
                default:
                    return false;
            }
        }


        public static bool DropdownItemsAllowed(this enDataFormat format)
        {
            switch (format)
            {
                case enDataFormat.Text:
                case enDataFormat.Bit:
                case enDataFormat.Ganzzahl:
                case enDataFormat.Gleitkommazahl:
                case enDataFormat.BildCode:
                case enDataFormat.Datum_und_Uhrzeit:
                case enDataFormat.Text_mit_Formatierung:
                case enDataFormat.Link_To_Filesystem:
                case enDataFormat.Columns_für_LinkedCellDropdown:
                case enDataFormat.Values_für_LinkedCellDropdown:
                case enDataFormat.RelationText:
                    return true;
                default:
                    return false;
            }
        }


        public static bool CanBeChangedByRules(this enDataFormat format)
        {
            switch (format)
            {
                case enDataFormat.Text:
                case enDataFormat.Bit:
                case enDataFormat.Ganzzahl:
                case enDataFormat.Gleitkommazahl:
                case enDataFormat.BildCode:
                case enDataFormat.Datum_und_Uhrzeit:
                case enDataFormat.Farbcode:
                case enDataFormat.RelationText:
                case enDataFormat.Schrift:
                case enDataFormat.Text_mit_Formatierung:
                    return true;
                default:
                    return false;
            }
        }


        public static bool CanBeCheckedByRules(this enDataFormat format)
        {
            switch (format)
            {
                case enDataFormat.Text:
                case enDataFormat.Bit:
                case enDataFormat.Ganzzahl:
                case enDataFormat.Gleitkommazahl:
                case enDataFormat.BildCode:
                case enDataFormat.Datum_und_Uhrzeit:
                case enDataFormat.Farbcode:
                case enDataFormat.RelationText:
                case enDataFormat.Schrift:
                case enDataFormat.Text_mit_Formatierung:
                case enDataFormat.Link_To_Filesystem:
                case enDataFormat.LinkedCell:
                case enDataFormat.Columns_für_LinkedCellDropdown:
                case enDataFormat.Values_für_LinkedCellDropdown:
                    return true;
                default:
                    return false;
            }
        }

    }
}
