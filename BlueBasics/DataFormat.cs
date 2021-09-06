// Authors:
// Christian Peter
//
// Copyright (c) 2021 Christian Peter
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

using BlueBasics.Enums;
using System.Linq;
using System.Text.RegularExpressions;
using static BlueBasics.Converter;

namespace BlueBasics {

    public static class DataFormat {

        #region Methods

        public static string AllowedChars(this enDataFormat format) {
            switch (format) {
                case enDataFormat.Text:
                case enDataFormat.Text_mit_Formatierung:
                case enDataFormat.BildCode:
                case enDataFormat.Columns_f�r_LinkedCellDropdown:
                case enDataFormat.Values_f�r_LinkedCellDropdown:
                case enDataFormat.RelationText:
                    return Constants.Char_Buchstaben + Constants.Char_Buchstaben.ToUpper() + Constants.Char_Numerals + Constants.Char_Satzzeichen + Constants.Char_Sonderzeichen;

                case enDataFormat.Bit:
                    return "+-";

                case enDataFormat.Ganzzahl:
                case enDataFormat.FarbeInteger:
                    return Constants.Char_Numerals + "-";

                case enDataFormat.Gleitkommazahl:
                    return Constants.Char_Numerals + "-,";

                case enDataFormat.Datum_und_Uhrzeit:
                    return Constants.Char_Numerals + " :.";

                case enDataFormat.Link_To_Filesystem:
                    return Constants.Char_Buchstaben + Constants.Char_Buchstaben.ToUpper() + Constants.Char_Numerals + ",.\\:_ +-()'";

                case enDataFormat.LinkedCell:
                    Develop.DebugPrint(enFehlerArt.Warnung, "LinkedCell kann nicht gepr�ft werden.");
                    return string.Empty;

                case enDataFormat.Button:
                    return string.Empty;

                default:
                    Develop.DebugPrint(format);
                    return string.Empty;
            }
        }

        public static bool Autofilter_m�glich(this enDataFormat format) => format switch {
            enDataFormat.Text or enDataFormat.Bit or enDataFormat.Ganzzahl or enDataFormat.Gleitkommazahl or enDataFormat.BildCode or enDataFormat.Datum_und_Uhrzeit or enDataFormat.FarbeInteger or enDataFormat.Schrift or enDataFormat.Text_mit_Formatierung or enDataFormat.Link_To_Filesystem or enDataFormat.LinkedCell or enDataFormat.Columns_f�r_LinkedCellDropdown or enDataFormat.Values_f�r_LinkedCellDropdown or enDataFormat.RelationText => true,
            _ => false,
        };

        public static bool CanBeChangedByRules(this enDataFormat format) => format switch {
            enDataFormat.Text or enDataFormat.Bit or enDataFormat.Ganzzahl or enDataFormat.Gleitkommazahl or enDataFormat.BildCode or enDataFormat.Datum_und_Uhrzeit or enDataFormat.FarbeInteger or enDataFormat.RelationText or enDataFormat.Schrift or enDataFormat.Text_mit_Formatierung or enDataFormat.LinkedCell => true,
            _ => false,
        };

        public static bool CanBeCheckedByRules(this enDataFormat format) => format switch {
            enDataFormat.Text or enDataFormat.Bit or enDataFormat.Ganzzahl or enDataFormat.Gleitkommazahl or enDataFormat.BildCode or enDataFormat.Datum_und_Uhrzeit or enDataFormat.FarbeInteger or enDataFormat.RelationText or enDataFormat.Schrift or enDataFormat.Text_mit_Formatierung or enDataFormat.Link_To_Filesystem or enDataFormat.LinkedCell or enDataFormat.Columns_f�r_LinkedCellDropdown or enDataFormat.Values_f�r_LinkedCellDropdown => true,
            _ => false,
        };

        public static string CompareKey(string isValue, enDataFormat format) {
            var CompareKey_S_OK = Constants.SecondSortChar + "X";
            var CompareKey_S_NOK = Constants.SecondSortChar + "A";
            switch (format) {
                case enDataFormat.Ganzzahl:
                    if (string.IsNullOrEmpty(isValue)) { return CompareKey_S_NOK + "0000000000"; }
                    return int.TryParse(isValue, out var w) ? CompareKey_S_OK + w.ToString(Constants.Format_Integer10) : CompareKey_S_NOK + isValue;

                case enDataFormat.Bit:
                case enDataFormat.FarbeInteger:
                case enDataFormat.Schrift:
                    return Constants.SecondSortChar + isValue;

                case enDataFormat.Text:
                case enDataFormat.BildCode:
                case enDataFormat.Text_mit_Formatierung:
                case enDataFormat.Link_To_Filesystem:
                case enDataFormat.Values_f�r_LinkedCellDropdown:
                case enDataFormat.RelationText:
                    if (string.IsNullOrEmpty(isValue)) { return string.Empty; }
                    isValue = isValue.ToLower();
                    isValue = isValue.Replace("�", "a");
                    isValue = isValue.Replace("�", "o");
                    isValue = isValue.Replace("�", "u");
                    isValue = isValue.Replace("�", "a");
                    isValue = isValue.Replace("�", "o");
                    isValue = isValue.Replace("�", "u");
                    isValue = isValue.Replace("�", "i");
                    isValue = isValue.Replace("�", "e");
                    isValue = isValue.Replace("�", "a");
                    isValue = isValue.Replace("�", "o");
                    isValue = isValue.Replace("�", "u");
                    isValue = isValue.Replace("�", "i");
                    isValue = isValue.Replace("�", "e");
                    isValue = isValue.Replace("�", "a");
                    isValue = isValue.Replace("�", "o");
                    isValue = isValue.Replace("�", "u");
                    isValue = isValue.Replace("�", "i");
                    isValue = isValue.Replace("�", "e");
                    isValue = isValue.Replace("�", "z");
                    isValue = isValue.Replace("�", "s");
                    isValue = isValue.TrimStart("\"");
                    isValue = isValue.TrimStart("'");
                    isValue = isValue.TrimStart(" ");
                    return Constants.SecondSortChar + isValue;

                case enDataFormat.Gleitkommazahl:
                    if (string.IsNullOrEmpty(isValue)) { return "0000000000,000"; }
                    if (double.TryParse(isValue, out var dw)) {
                        var t = dw.ToString(Constants.Format_Float10_3);
                        if (!t.Contains(",")) { t += ",000"; };
                        while (t.Length < 14) { t += "0"; }
                        return CompareKey_S_OK + t;
                    } else {
                        return CompareKey_S_NOK + isValue;
                    }
                case enDataFormat.Datum_und_Uhrzeit:
                    return DateTimeTryParse(isValue, out var d) ? CompareKey_S_OK + d.ToString("u") : CompareKey_S_NOK + isValue;

                case enDataFormat.LinkedCell:
                case enDataFormat.Columns_f�r_LinkedCellDropdown:
                    // Fremdzellen setzen sich aus verschiedenen Spalten zusammen, also nur ganz primitives zur�ckgeben
                    return Constants.SecondSortChar + isValue;

                default:
                    Develop.DebugPrint(format);
                    return Constants.SecondSortChar + isValue;
            }
        }

        public static bool DropdownItemsAllowed(this enDataFormat format) => format switch {
            enDataFormat.Text or enDataFormat.Bit or enDataFormat.Ganzzahl or enDataFormat.Gleitkommazahl or enDataFormat.BildCode or enDataFormat.Datum_und_Uhrzeit or enDataFormat.Text_mit_Formatierung or enDataFormat.Link_To_Filesystem or enDataFormat.Columns_f�r_LinkedCellDropdown or enDataFormat.Values_f�r_LinkedCellDropdown or enDataFormat.RelationText => true,
            _ => false,
        };

        public static bool DropdownItemsOfOtherCellsAllowed(this enDataFormat format) => format switch {
            enDataFormat.Text or enDataFormat.Ganzzahl or enDataFormat.Gleitkommazahl or enDataFormat.BildCode or enDataFormat.Datum_und_Uhrzeit or enDataFormat.FarbeInteger or enDataFormat.Schrift or enDataFormat.Text_mit_Formatierung or enDataFormat.LinkedCell or enDataFormat.RelationText => true,
            _ => false,
        };

        public static bool DropdownUnselectAllAllowed(this enDataFormat format) => format switch {
            enDataFormat.Text or enDataFormat.Ganzzahl or enDataFormat.Gleitkommazahl or enDataFormat.BildCode or enDataFormat.Datum_und_Uhrzeit or enDataFormat.FarbeInteger or enDataFormat.RelationText or enDataFormat.Schrift or enDataFormat.Text_mit_Formatierung or enDataFormat.Link_To_Filesystem or enDataFormat.LinkedCell => true,
            _ => false,
        };

        public static bool ExportableForLayout(this enDataFormat format) => format switch {
            enDataFormat.Text or enDataFormat.Bit or enDataFormat.Ganzzahl or enDataFormat.Gleitkommazahl or enDataFormat.BildCode or enDataFormat.Datum_und_Uhrzeit or enDataFormat.RelationText or enDataFormat.Schrift or enDataFormat.Text_mit_Formatierung or enDataFormat.Link_To_Filesystem or enDataFormat.LinkedCell or enDataFormat.Columns_f�r_LinkedCellDropdown or enDataFormat.Values_f�r_LinkedCellDropdown => true,
            _ => false,
        };

        /// <summary>
        /// Pr�ft, ob ein String der geforderte Typ sein kann.
        /// Dabei wird die Textl�nge, die Schablone und die erlaubten Zeichen gepr�ft.
        /// Ein Logigcheck (z.B. ob ein Datum g�ltig ist) wird ebenfalls ausgef�hrt.
        /// </summary>
        /// <param name="txt"></param>
        /// <param name="format"></param>
        /// <param name="multiLine"></param>
        /// <returns></returns>
        /// <remarks></remarks>
        public static bool IsFormat(this string txt, enDataFormat format, bool multiLine) {
            if (multiLine) {
                var ex = txt.SplitAndCutByCR();
                return ex.All(thisString => string.IsNullOrEmpty(thisString) || thisString.IsFormat(format));
            }
            return txt.IsFormat(format);
        }

        /// <summary>
        /// Pr�ft, ob ein String der geforderte Typ sein kann.
        /// Dabei wird die Textl�nge, die Schablone und die erlaubten Zeichen gepr�ft.
        /// Ein Logigcheck (z.B. ob ein Datum g�ltig ist) wird ebenfalls ausgef�hrt.
        /// </summary>
        /// <param name="txt"></param>
        /// <param name="format"></param>
        /// <returns></returns>
        /// <remarks></remarks>
        public static bool IsFormat(this string txt, enDataFormat format) => Text_L�ngeCheck(txt, format) && Text_SchabloneCheck(txt, format) && txt.ContainsOnlyChars(AllowedChars(format)) && Text_ValueCheck(txt, format);

        public static bool IsZahl(this enDataFormat format) => format switch {
            enDataFormat.Ganzzahl or enDataFormat.Gleitkommazahl => true,
            _ => false,
        };

        public static bool MultilinePossible(this enDataFormat format) => format switch {
            enDataFormat.Text or enDataFormat.Ganzzahl or enDataFormat.Gleitkommazahl or enDataFormat.BildCode or enDataFormat.Datum_und_Uhrzeit or enDataFormat.RelationText or enDataFormat.Text_mit_Formatierung or enDataFormat.Link_To_Filesystem or enDataFormat.LinkedCell => true,
            _ => false,
        };

        public static bool NeedLinkedKeyKennung(this enDataFormat format) => format switch {
            enDataFormat.Columns_f�r_LinkedCellDropdown => true,
            _ => false,
        };

        // public static bool NeedUTF8(this enDataFormat format) {
        //    switch (format) {
        //        case enDataFormat.Text:
        //        case enDataFormat.RelationText:
        //        case enDataFormat.Text_mit_Formatierung:
        //            return true;
        //        default:
        //            return false;
        //    }
        // }
        public static bool NeedTargetDatabase(this enDataFormat format) => format switch {
            enDataFormat.LinkedCell or enDataFormat.Columns_f�r_LinkedCellDropdown or enDataFormat.Values_f�r_LinkedCellDropdown => true,
            _ => false,
        };

        // public static bool CompactPossible(this enDataFormat format)
        // {
        //    switch (format)
        //    {
        //        case enDataFormat.Bit:
        //        case enDataFormat.BildCode:
        //        case enDataFormat.FarbeInteger:
        //        case enDataFormat.Schrift:
        //            return true;
        //        default:
        //            return false;
        //    }
        // }
        public static bool SaveSizeData(this enDataFormat format) => format switch {
            enDataFormat.Text or enDataFormat.Bit or enDataFormat.Ganzzahl or enDataFormat.Gleitkommazahl or enDataFormat.BildCode or enDataFormat.Datum_und_Uhrzeit or enDataFormat.FarbeInteger or enDataFormat.RelationText or enDataFormat.Schrift or enDataFormat.Text_mit_Formatierung or enDataFormat.Values_f�r_LinkedCellDropdown => true,
            _ => false,
        };

        public static bool SpellCheckingPossible(this enDataFormat format) => format switch {
            enDataFormat.Text or enDataFormat.Text_mit_Formatierung or enDataFormat.RelationText => true,
            _ => false,
        };

        public static bool Text_L�ngeCheck(string tXT, enDataFormat format) {
            var ml = Text_MaximaleL�nge(format);
            var il = 0;
            if (tXT != null) { il = tXT.Length; }
            if (ml > -1 && il > ml) { return false; }
            switch (format) {
                case enDataFormat.Text:
                case enDataFormat.Text_mit_Formatierung:
                case enDataFormat.RelationText:
                case enDataFormat.Columns_f�r_LinkedCellDropdown:
                case enDataFormat.Values_f�r_LinkedCellDropdown:
                case enDataFormat.Button:
                    return true;

                case enDataFormat.Datum_und_Uhrzeit:
                    if (il == 19) { return true; }
                    if (il == 16) { return true; } // 01.01.2011 11:11
                    return il == 10;

                case enDataFormat.Bit:
                    return il == 1;

                case enDataFormat.Ganzzahl:
                case enDataFormat.Gleitkommazahl:
                case enDataFormat.FarbeInteger:
                case enDataFormat.BildCode:
                    return il > 0;

                case enDataFormat.LinkedCell:
                    Develop.DebugPrint(enFehlerArt.Warnung, "LinkedCell kann nicht gepr�ft werden.");
                    return true;

                case enDataFormat.Link_To_Filesystem:
                    return il < 257;

                default:
                    Develop.DebugPrint(format);
                    return true;
            }
        }

        public static int Text_MaximaleL�nge(this enDataFormat format) {
            switch (format) {
                case enDataFormat.Text:
                case enDataFormat.Text_mit_Formatierung:
                case enDataFormat.Gleitkommazahl:
                case enDataFormat.Ganzzahl:
                case enDataFormat.BildCode:
                case enDataFormat.RelationText:
                case enDataFormat.FarbeInteger:
                case enDataFormat.LinkedCell:
                case enDataFormat.Columns_f�r_LinkedCellDropdown:
                case enDataFormat.Values_f�r_LinkedCellDropdown:
                    return -1;

                case enDataFormat.Bit:
                    return 1;

                case enDataFormat.Link_To_Filesystem:
                    return 260;

                case enDataFormat.Datum_und_Uhrzeit:
                    return 19;

                case enDataFormat.Button:
                    return 0;

                default:
                    Develop.DebugPrint(format);
                    return -1;
            }
        }

        /// <summary>
        /// Gibt zur�ck, ob der Text in die vordefinierte Schablone pa�t.
        /// </summary>
        /// <param name="tXT"></param>
        /// <param name="format"></param>
        /// <returns></returns>
        /// <remarks></remarks>
        public static bool Text_SchabloneCheck(string tXT, enDataFormat format) {
            switch (format) {
                case enDataFormat.Text:
                case enDataFormat.Text_mit_Formatierung:
                case enDataFormat.BildCode:
                case enDataFormat.Columns_f�r_LinkedCellDropdown:
                case enDataFormat.Values_f�r_LinkedCellDropdown:
                case enDataFormat.RelationText:
                case enDataFormat.Link_To_Filesystem:
                case enDataFormat.Button:
                    return true;

                case enDataFormat.Bit:
                    return tXT.Length == 1;

                case enDataFormat.Gleitkommazahl:
                    if (tXT == "0") { return true; }
                    if (tXT == "-") { return false; }
                    if (tXT.Length > 1 && tXT.Substring(0, 2) == "00") { return false; }
                    if (tXT.Length > 2 && tXT.Substring(0, 3) == "-00") { return false; }
                    if (tXT.Length > 2 && tXT.IndexOf("-", 1) > -1) { return false; }
                    return true;

                case enDataFormat.Ganzzahl:
                case enDataFormat.FarbeInteger:
                    if (tXT == "0") { return true; }
                    if (tXT == "-") { return false; }
                    if (!string.IsNullOrEmpty(tXT) && tXT.Substring(0, 1) == "0") { return false; }
                    if (tXT.Length > 1 && tXT.Substring(0, 2) == "-0") { return false; }
                    if (tXT.Length > 2 && tXT.IndexOf("-", 1) > -1) { return false; }
                    return true;

                case enDataFormat.Datum_und_Uhrzeit:
                    if (new Regex(@"^\d{2}.\d{2}.\d{4}$").IsMatch(tXT)) { return true; }
                    if (new Regex(@"^\d{2}.\d{2}.\d{4} \d{2}:\d{2}:\d{2}$").IsMatch(tXT)) { return true; }
                    if (new Regex(@"^\d{2}.\d{2}.\d{4} \d{2}:\d{2}$").IsMatch(tXT)) { return true; }
                    return false;

                case enDataFormat.LinkedCell:
                    Develop.DebugPrint(enFehlerArt.Warnung, "LinkedCell kann nicht gepr�ft werden.");
                    return true;

                default:
                    Develop.DebugPrint(format);
                    return true;
            }
        }

        public static bool Text_ValueCheck(string tXT, enDataFormat format) {
            switch (format) {
                case enDataFormat.Text:
                case enDataFormat.Text_mit_Formatierung:
                case enDataFormat.BildCode:
                case enDataFormat.Columns_f�r_LinkedCellDropdown:
                case enDataFormat.Values_f�r_LinkedCellDropdown:
                case enDataFormat.Bit:
                case enDataFormat.RelationText:
                case enDataFormat.Link_To_Filesystem:
                case enDataFormat.Button:
                    return true;

                case enDataFormat.Datum_und_Uhrzeit:
                    return DateTimeTryParse(tXT, out _);

                case enDataFormat.Gleitkommazahl:
                    return float.TryParse(tXT, out _);

                case enDataFormat.Ganzzahl:
                case enDataFormat.FarbeInteger:
                    return long.TryParse(tXT, out _);

                case enDataFormat.LinkedCell:
                    Develop.DebugPrint(enFehlerArt.Warnung, "LinkedCell kann nicht gepr�ft werden.");
                    return true;

                default:
                    Develop.DebugPrint(format);
                    return true;
            }
        }

        public static bool TextboxEditPossible(this enDataFormat format) => format switch {
            enDataFormat.Text or enDataFormat.Ganzzahl or enDataFormat.Gleitkommazahl or enDataFormat.BildCode or enDataFormat.Datum_und_Uhrzeit or enDataFormat.Text_mit_Formatierung or enDataFormat.LinkedCell or enDataFormat.RelationText => true,
            _ => false,
        };

        #endregion
    }
}