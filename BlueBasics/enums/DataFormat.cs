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

namespace BlueBasics.Enums {

    public enum DataFormat {

        // Unbekannt = -1,
        // Nothing = 0,
        Text = 1,

        //Bit = 2,

        //Ganzzahl = 3,
        //Gleitkommazahl = 6,

        //BildCode = 13,

        // Date_GermanFormat = 15
        //Datum_und_Uhrzeit = 16,

        // Binärdaten_Bild = 19,
        // Passwort = 20, // String
        //  Text_Ohne_Kritische_Zeichen = 21,
        // Binärdaten = 39,
        // Link_To_BlueDataSystem = 42
        // Telefonnummer = 43, // Spezielle Formate
        FarbeInteger = 45, // Color

        // Email = 46, // Spezielle Formate
        // InternetAdresse = 47, // Spezielle Formate
        // Relation = 65,
        // Event = 66,
        // Tendenz = 67
        // Einschätzung = 68,
        Schrift = 69,

        //Text_mit_Formatierung = 70,

        // TextmitFormatierungUndLinkToAnotherDatabase = 71
        // Relation_And_Event_Mixed = 72,
        Link_To_Filesystem = 73,

        /// <summary>
        /// Gibt die Werte einer anderen Spalte einer
        /// anderen Datenbank als Dropdown Items wieder
        /// </summary>
        Werte_aus_anderer_Datenbank_als_DropDownItems = 76,

        RelationText = 77,

        // KeyForSame = 78
        Button = 79,

        Verknüpfung_zu_anderer_Datenbank = 80

        // bis 999 wird geprüft
    }

    public static class DataFormatExtensions {
        //public static string AllowedChars(this DataFormat format) {
        //    switch (format) {
        //        case DataFormat.Text:
        //        case DataFormat.Columns_für_LinkedCellDropdown:
        //        case DataFormat.Values_für_LinkedCellDropdown:
        //        case DataFormat.RelationText:
        //            return Constants.Char_Buchstaben + Constants.Char_Buchstaben.ToUpper() + Constants.Char_Numerals + Constants.Char_Satzzeichen + Constants.Char_Sonderzeichen;

        //        case DataFormat.Bit:
        //            return "+-";

        //        case DataFormat.FarbeInteger:
        //            return Constants.Char_Numerals + "-";

        //        case DataFormat.Link_To_Filesystem:
        //            return Constants.Char_Buchstaben + Constants.Char_Buchstaben.ToUpper() + Constants.Char_Numerals + ",.\\:_ +-()'";

        //        case DataFormat.LinkedCell:
        //            Develop.DebugPrint(enFehlerArt.Warnung, "LinkedCell kann nicht geprüft werden.");
        //            return string.Empty;

        //        case DataFormat.Button:
        //            return string.Empty;

        //        default:
        //            Develop.DebugPrint(format);
        //            return string.Empty;
        //    }
        //}

        #region Methods

        public static bool Autofilter_möglich(this DataFormat format) => format switch {
            DataFormat.Text or DataFormat.FarbeInteger or DataFormat.Schrift or DataFormat.Link_To_Filesystem or DataFormat.Verknüpfung_zu_anderer_Datenbank or DataFormat.Werte_aus_anderer_Datenbank_als_DropDownItems or DataFormat.RelationText => true,
            _ => false
        };

        public static bool CanBeChangedByRules(this DataFormat format) => format switch {
            DataFormat.Text or DataFormat.FarbeInteger or DataFormat.RelationText or DataFormat.Schrift => true,
            _ => false
        };

        public static bool CanBeCheckedByRules(this DataFormat format) => format switch {
            DataFormat.Text or DataFormat.FarbeInteger or DataFormat.RelationText or DataFormat.Schrift or DataFormat.Link_To_Filesystem or DataFormat.Verknüpfung_zu_anderer_Datenbank or DataFormat.Werte_aus_anderer_Datenbank_als_DropDownItems => true,
            _ => false
        };

        public static bool DropdownItemsAllowed(this DataFormat format) => format switch {
            DataFormat.Text or DataFormat.Link_To_Filesystem or DataFormat.Werte_aus_anderer_Datenbank_als_DropDownItems or DataFormat.RelationText => true,
            _ => false
        };

        public static bool DropdownItemsOfOtherCellsAllowed(this DataFormat format) => format switch {
            DataFormat.Text or DataFormat.FarbeInteger or DataFormat.Schrift or DataFormat.Verknüpfung_zu_anderer_Datenbank or DataFormat.RelationText => true,
            _ => false
        };

        public static bool DropdownUnselectAllAllowed(this DataFormat format) => format switch {
            DataFormat.Text or DataFormat.FarbeInteger or DataFormat.RelationText or DataFormat.Schrift or DataFormat.Link_To_Filesystem or DataFormat.Verknüpfung_zu_anderer_Datenbank => true,
            _ => false
        };

        public static bool ExportableForLayout(this DataFormat format) => format switch {
            DataFormat.Text or DataFormat.RelationText or DataFormat.Schrift or DataFormat.Link_To_Filesystem or DataFormat.Verknüpfung_zu_anderer_Datenbank or DataFormat.Werte_aus_anderer_Datenbank_als_DropDownItems => true,
            _ => false
        };

        ///// <summary>
        ///// Prüft, ob ein String der geforderte Typ sein kann.
        ///// Dabei wird die Textlänge, die Schablone und die erlaubten Zeichen geprüft.
        ///// Ein Logigcheck (z.B. ob ein Datum gültig ist) wird ebenfalls ausgeführt.
        ///// </summary>
        ///// <param name="txt"></param>
        ///// <param name="format"></param>
        ///// <param name="multiLine"></param>
        ///// <returns></returns>
        ///// <remarks></remarks>
        //public static bool IsFormat(this string txt, DataFormat format, bool multiLine, string additionalRegex) {
        //    if (multiLine) {
        //        var ex = txt.SplitAndCutByCr();
        //        return ex.All(thisString => string.IsNullOrEmpty(thisString) || thisString.IsFormat(format, additionalRegex));
        //    }
        //    return txt.IsFormat(format, additionalRegex);
        //}

        //public static bool IsFormat(this string txt, DataFormat format) => Text_LängeCheck(txt, format) && Text_SchabloneCheck(txt, format) && txt.ContainsOnlyChars(AllowedChars(format)) && Text_ValueCheck(txt, format);

        public static bool MultilinePossible(this DataFormat format) => format switch {
            DataFormat.Text or DataFormat.RelationText or DataFormat.Link_To_Filesystem or DataFormat.Verknüpfung_zu_anderer_Datenbank => true,
            _ => false
        };

        public static bool NeedTargetDatabase(this DataFormat format) => format switch {
            DataFormat.Verknüpfung_zu_anderer_Datenbank or DataFormat.Werte_aus_anderer_Datenbank_als_DropDownItems => true,
            _ => false
        };

        public static bool SaveSizeData(this DataFormat format) => format switch {
            DataFormat.Text or DataFormat.FarbeInteger or DataFormat.RelationText or DataFormat.Schrift or DataFormat.Werte_aus_anderer_Datenbank_als_DropDownItems => true,
            _ => false
        };

        public static bool SpellCheckingPossible(this DataFormat format) => format switch {
            DataFormat.Text or DataFormat.RelationText => true,
            _ => false
        };

        //public static bool Text_LängeCheck(string txt, DataFormat format) {
        //    var ml = Text_MaximaleLänge(format);
        //    var il = 0;
        //    if (txt != null) { il = txt.Length; }
        //    if (ml > -1 && il > ml) { return false; }
        //    switch (format) {
        //        case DataFormat.Text:
        //        case DataFormat.RelationText:
        //        case DataFormat.Columns_für_LinkedCellDropdown:
        //        case DataFormat.Values_für_LinkedCellDropdown:
        //        case DataFormat.Button:
        //            return true;

        //        case DataFormat.Bit:
        //            return il == 1;

        //        case DataFormat.FarbeInteger:
        //        case DataFormat.BildCode:
        //            return il > 0;

        //        case DataFormat.LinkedCell:
        //            Develop.DebugPrint(enFehlerArt.Warnung, "LinkedCell kann nicht geprüft werden.");
        //            return true;

        //        case DataFormat.Link_To_Filesystem:
        //            return il < 257;

        //        default:
        //            Develop.DebugPrint(format);
        //            return true;
        //    }
        //}

        //public static int Text_MaximaleLänge(this DataFormat format) {
        //    switch (format) {
        //        case DataFormat.Text:
        //        case DataFormat.BildCode:
        //        case DataFormat.RelationText:
        //        case DataFormat.FarbeInteger:
        //        case DataFormat.LinkedCell:
        //        case DataFormat.Columns_für_LinkedCellDropdown:
        //        case DataFormat.Values_für_LinkedCellDropdown:
        //            return -1;

        //        case DataFormat.Bit:
        //            return 1;

        //        case DataFormat.Link_To_Filesystem:
        //            return 260;

        //        case DataFormat.Button:
        //            return 0;

        //        default:
        //            Develop.DebugPrint(format);
        //            return -1;
        //    }
        //}

        ///// <summary>
        ///// Gibt zurück, ob der Text in die vordefinierte Schablone paßt.
        ///// </summary>
        ///// <param name="txt"></param>
        ///// <param name="format"></param>
        ///// <returns></returns>
        ///// <remarks></remarks>
        //public static bool Text_SchabloneCheck(string txt, DataFormat format) {
        //    switch (format) {
        //        case DataFormat.Text:
        //        case DataFormat.BildCode:
        //        case DataFormat.Columns_für_LinkedCellDropdown:
        //        case DataFormat.Values_für_LinkedCellDropdown:
        //        case DataFormat.RelationText:
        //        case DataFormat.Link_To_Filesystem:
        //        case DataFormat.Button:
        //            return true;

        //        case DataFormat.Bit:
        //            return txt.Length == 1;

        //        case DataFormat.FarbeInteger:
        //            if (txt == "0") { return true; }
        //            if (txt == "-") { return false; }
        //            if (!string.IsNullOrEmpty(txt) && txt.Substring(0, 1) == "0") { return false; }
        //            if (txt.Length > 1 && txt.Substring(0, 2) == "-0") { return false; }
        //            if (txt.Length > 2 && txt.IndexOf("-", 1) > -1) { return false; }
        //            return true;

        //        case DataFormat.LinkedCell:
        //            Develop.DebugPrint(enFehlerArt.Warnung, "LinkedCell kann nicht geprüft werden.");
        //            return true;

        //        default:
        //            Develop.DebugPrint(format);
        //            return true;
        //    }
        //}

        //public static bool Text_ValueCheck(string tXT, DataFormat format) {
        //    switch (format) {
        //        case DataFormat.Text:
        //        case DataFormat.BildCode:
        //        case DataFormat.Columns_für_LinkedCellDropdown:
        //        case DataFormat.Values_für_LinkedCellDropdown:
        //        case DataFormat.Bit:
        //        case DataFormat.RelationText:
        //        case DataFormat.Link_To_Filesystem:
        //        case DataFormat.Button:
        //            return true;

        //        case DataFormat.FarbeInteger:
        //            return long.TryParse(tXT, out _);

        //        case DataFormat.LinkedCell:
        //            Develop.DebugPrint(enFehlerArt.Warnung, "LinkedCell kann nicht geprüft werden.");
        //            return true;

        //        default:
        //            Develop.DebugPrint(format);
        //            return true;
        //    }
        //}

        public static bool TextboxEditPossible(this DataFormat format) => format switch {
            DataFormat.Text or DataFormat.Verknüpfung_zu_anderer_Datenbank or DataFormat.RelationText => true,
            _ => false
        };

        #endregion
    }
}