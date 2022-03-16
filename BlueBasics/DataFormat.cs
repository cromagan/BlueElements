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

using BlueBasics.Enums;

namespace BlueBasics {

    public static class DataFormat {
        //public static string AllowedChars(this enDataFormat format) {
        //    switch (format) {
        //        case enDataFormat.Text:
        //        case enDataFormat.Columns_für_LinkedCellDropdown:
        //        case enDataFormat.Values_für_LinkedCellDropdown:
        //        case enDataFormat.RelationText:
        //            return Constants.Char_Buchstaben + Constants.Char_Buchstaben.ToUpper() + Constants.Char_Numerals + Constants.Char_Satzzeichen + Constants.Char_Sonderzeichen;

        //        case enDataFormat.Bit:
        //            return "+-";

        //        case enDataFormat.FarbeInteger:
        //            return Constants.Char_Numerals + "-";

        //        case enDataFormat.Link_To_Filesystem:
        //            return Constants.Char_Buchstaben + Constants.Char_Buchstaben.ToUpper() + Constants.Char_Numerals + ",.\\:_ +-()'";

        //        case enDataFormat.LinkedCell:
        //            Develop.DebugPrint(enFehlerArt.Warnung, "LinkedCell kann nicht geprüft werden.");
        //            return string.Empty;

        //        case enDataFormat.Button:
        //            return string.Empty;

        //        default:
        //            Develop.DebugPrint(format);
        //            return string.Empty;
        //    }
        //}

        #region Methods

        public static bool Autofilter_möglich(this enDataFormat format) => format switch {
            enDataFormat.Text or enDataFormat.FarbeInteger or enDataFormat.Schrift or enDataFormat.Link_To_Filesystem or enDataFormat.Verknüpfung_zu_anderer_Datenbank or enDataFormat.Werte_aus_anderer_Datenbank_als_DropDownItems or enDataFormat.RelationText => true,
            _ => false
        };

        public static bool CanBeChangedByRules(this enDataFormat format) => format switch {
            enDataFormat.Text or enDataFormat.FarbeInteger or enDataFormat.RelationText or enDataFormat.Schrift => true,
            _ => false
        };

        public static bool CanBeCheckedByRules(this enDataFormat format) => format switch {
            enDataFormat.Text or enDataFormat.FarbeInteger or enDataFormat.RelationText or enDataFormat.Schrift or enDataFormat.Link_To_Filesystem or enDataFormat.Verknüpfung_zu_anderer_Datenbank or enDataFormat.Werte_aus_anderer_Datenbank_als_DropDownItems => true,
            _ => false
        };

        public static bool DropdownItemsAllowed(this enDataFormat format) => format switch {
            enDataFormat.Text or enDataFormat.Link_To_Filesystem or enDataFormat.Werte_aus_anderer_Datenbank_als_DropDownItems or enDataFormat.RelationText => true,
            _ => false
        };

        public static bool DropdownItemsOfOtherCellsAllowed(this enDataFormat format) => format switch {
            enDataFormat.Text or enDataFormat.FarbeInteger or enDataFormat.Schrift or enDataFormat.Verknüpfung_zu_anderer_Datenbank or enDataFormat.RelationText => true,
            _ => false
        };

        public static bool DropdownUnselectAllAllowed(this enDataFormat format) => format switch {
            enDataFormat.Text or enDataFormat.FarbeInteger or enDataFormat.RelationText or enDataFormat.Schrift or enDataFormat.Link_To_Filesystem or enDataFormat.Verknüpfung_zu_anderer_Datenbank => true,
            _ => false
        };

        public static bool ExportableForLayout(this enDataFormat format) => format switch {
            enDataFormat.Text or enDataFormat.RelationText or enDataFormat.Schrift or enDataFormat.Link_To_Filesystem or enDataFormat.Verknüpfung_zu_anderer_Datenbank or enDataFormat.Werte_aus_anderer_Datenbank_als_DropDownItems => true,
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
        //public static bool IsFormat(this string txt, enDataFormat format, bool multiLine, string additionalRegex) {
        //    if (multiLine) {
        //        var ex = txt.SplitAndCutByCr();
        //        return ex.All(thisString => string.IsNullOrEmpty(thisString) || thisString.IsFormat(format, additionalRegex));
        //    }
        //    return txt.IsFormat(format, additionalRegex);
        //}

        //public static bool IsFormat(this string txt, enDataFormat format) => Text_LängeCheck(txt, format) && Text_SchabloneCheck(txt, format) && txt.ContainsOnlyChars(AllowedChars(format)) && Text_ValueCheck(txt, format);

        public static bool MultilinePossible(this enDataFormat format) => format switch {
            enDataFormat.Text or enDataFormat.RelationText or enDataFormat.Link_To_Filesystem or enDataFormat.Verknüpfung_zu_anderer_Datenbank => true,
            _ => false
        };

        public static bool NeedTargetDatabase(this enDataFormat format) => format switch {
            enDataFormat.Verknüpfung_zu_anderer_Datenbank or enDataFormat.Werte_aus_anderer_Datenbank_als_DropDownItems => true,
            _ => false
        };

        public static bool SaveSizeData(this enDataFormat format) => format switch {
            enDataFormat.Text or enDataFormat.FarbeInteger or enDataFormat.RelationText or enDataFormat.Schrift or enDataFormat.Werte_aus_anderer_Datenbank_als_DropDownItems => true,
            _ => false
        };

        public static bool SpellCheckingPossible(this enDataFormat format) => format switch {
            enDataFormat.Text or enDataFormat.RelationText => true,
            _ => false
        };

        //public static bool Text_LängeCheck(string txt, enDataFormat format) {
        //    var ml = Text_MaximaleLänge(format);
        //    var il = 0;
        //    if (txt != null) { il = txt.Length; }
        //    if (ml > -1 && il > ml) { return false; }
        //    switch (format) {
        //        case enDataFormat.Text:
        //        case enDataFormat.RelationText:
        //        case enDataFormat.Columns_für_LinkedCellDropdown:
        //        case enDataFormat.Values_für_LinkedCellDropdown:
        //        case enDataFormat.Button:
        //            return true;

        //        case enDataFormat.Bit:
        //            return il == 1;

        //        case enDataFormat.FarbeInteger:
        //        case enDataFormat.BildCode:
        //            return il > 0;

        //        case enDataFormat.LinkedCell:
        //            Develop.DebugPrint(enFehlerArt.Warnung, "LinkedCell kann nicht geprüft werden.");
        //            return true;

        //        case enDataFormat.Link_To_Filesystem:
        //            return il < 257;

        //        default:
        //            Develop.DebugPrint(format);
        //            return true;
        //    }
        //}

        //public static int Text_MaximaleLänge(this enDataFormat format) {
        //    switch (format) {
        //        case enDataFormat.Text:
        //        case enDataFormat.BildCode:
        //        case enDataFormat.RelationText:
        //        case enDataFormat.FarbeInteger:
        //        case enDataFormat.LinkedCell:
        //        case enDataFormat.Columns_für_LinkedCellDropdown:
        //        case enDataFormat.Values_für_LinkedCellDropdown:
        //            return -1;

        //        case enDataFormat.Bit:
        //            return 1;

        //        case enDataFormat.Link_To_Filesystem:
        //            return 260;

        //        case enDataFormat.Button:
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
        //public static bool Text_SchabloneCheck(string txt, enDataFormat format) {
        //    switch (format) {
        //        case enDataFormat.Text:
        //        case enDataFormat.BildCode:
        //        case enDataFormat.Columns_für_LinkedCellDropdown:
        //        case enDataFormat.Values_für_LinkedCellDropdown:
        //        case enDataFormat.RelationText:
        //        case enDataFormat.Link_To_Filesystem:
        //        case enDataFormat.Button:
        //            return true;

        //        case enDataFormat.Bit:
        //            return txt.Length == 1;

        //        case enDataFormat.FarbeInteger:
        //            if (txt == "0") { return true; }
        //            if (txt == "-") { return false; }
        //            if (!string.IsNullOrEmpty(txt) && txt.Substring(0, 1) == "0") { return false; }
        //            if (txt.Length > 1 && txt.Substring(0, 2) == "-0") { return false; }
        //            if (txt.Length > 2 && txt.IndexOf("-", 1) > -1) { return false; }
        //            return true;

        //        case enDataFormat.LinkedCell:
        //            Develop.DebugPrint(enFehlerArt.Warnung, "LinkedCell kann nicht geprüft werden.");
        //            return true;

        //        default:
        //            Develop.DebugPrint(format);
        //            return true;
        //    }
        //}

        //public static bool Text_ValueCheck(string tXT, enDataFormat format) {
        //    switch (format) {
        //        case enDataFormat.Text:
        //        case enDataFormat.BildCode:
        //        case enDataFormat.Columns_für_LinkedCellDropdown:
        //        case enDataFormat.Values_für_LinkedCellDropdown:
        //        case enDataFormat.Bit:
        //        case enDataFormat.RelationText:
        //        case enDataFormat.Link_To_Filesystem:
        //        case enDataFormat.Button:
        //            return true;

        //        case enDataFormat.FarbeInteger:
        //            return long.TryParse(tXT, out _);

        //        case enDataFormat.LinkedCell:
        //            Develop.DebugPrint(enFehlerArt.Warnung, "LinkedCell kann nicht geprüft werden.");
        //            return true;

        //        default:
        //            Develop.DebugPrint(format);
        //            return true;
        //    }
        //}

        public static bool TextboxEditPossible(this enDataFormat format) => format switch {
            enDataFormat.Text or enDataFormat.Verknüpfung_zu_anderer_Datenbank or enDataFormat.RelationText => true,
            _ => false
        };

        #endregion
    }
}