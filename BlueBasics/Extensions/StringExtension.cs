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
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace BlueBasics {

    public static partial class Extensions {

        #region Fields

        public static List<string> GeschKlammerAuf = new() { "{" };

        public static List<string> GeschKlammerZu = new() { "}" };

        public static List<string> Gleich = new() { "=" };

        public static List<string> KlammerAuf = new() { "(" };

        public static List<string> KlammerZu = new() { ")" };

        public static List<string> Komma = new() { "," };

        #endregion

        #region Methods

        public static List<string> AllWords(this string input) {
            input = " " + input + " ";
            var position = 0;
            var LastSeperator = 0;
            List<string> l = new();
            while (true) {
                position++;
                if (position >= input.Length) { return l; }
                if (input[position].isWordSeperator()) {
                    if (position > LastSeperator + 1) {
                        l.Add(input.Substring(LastSeperator + 1, position - LastSeperator - 1));
                    }
                    LastSeperator = position;
                }
            }
        }

        public static bool ContainsChars(this string tXT, string chars) => chars.Where((_, z) => tXT.Contains(chars.Substring(z, 1))).Any();

        public static bool ContainsOnlyChars(this string tXT, string chars) => !tXT.Where((_, z) => !chars.Contains(tXT.Substring(z, 1))).Any();

        public static bool ContainsWord(this string input, string value, RegexOptions options) => input.IndexOfWord(value, 0, options) >= 0;

        public static string ConvertFromHtmlToRich(this string txt) {
            txt = txt.Replace("\r\n", "<br>");
            txt = txt.Replace("<<>", "&lt;");
            txt = txt.Replace("<>>", "&gt;");
            return txt;
        }

        public static int CountString(this string text, string value) {
            var Anz = 0;
            for (var z = 0; z <= text.Length - value.Length; z++) {
                if (text.Substring(z, value.Length) == value) { Anz++; }
            }
            return Anz;
        }

        public static string CreateHtmlCodes(this string tXT, bool crlftoo) {
            // http://sonderzeichentabelle.de/
            // http://www.htmlhelp.com/reference/html40/entities/special.html
            tXT = tXT.Replace("&", "&amp;"); // Wichtig! An erster Stelle! ä-> &auml; -> &amp;auml;
            tXT = tXT.Replace("ä", "&auml;");
            tXT = tXT.Replace("ö", "&ouml;");
            tXT = tXT.Replace("ü", "&uuml;");
            tXT = tXT.Replace("Ä", "&Auml;");
            tXT = tXT.Replace("Ö", "&Ouml;");
            tXT = tXT.Replace("Ü", "&Uuml;");
            tXT = tXT.Replace("ß", "&szlig;");
            tXT = tXT.Replace("É", "&Eacute;");
            tXT = tXT.Replace("é", "&eacute;");
            tXT = tXT.Replace("€", "&euro;");
            tXT = tXT.Replace("Ø", "&Oslash;");
            tXT = tXT.Replace("ø", "&oslash;");
            tXT = tXT.Replace("•", "&bull;");
            tXT = tXT.Replace("<", "&lt;");
            tXT = tXT.Replace(">", "&gt;");
            tXT = tXT.Replace("\"", "&quot;");
            if (!crlftoo) { return tXT; }
            tXT = tXT.Replace("\r\n", "<br>", RegexOptions.IgnoreCase);
            tXT = tXT.Replace("\r", "<br>", RegexOptions.IgnoreCase);
            tXT = tXT.Replace("\n", "<br>", RegexOptions.IgnoreCase);
            tXT = tXT.Replace(((char)1007).ToString(), "<H7>");
            tXT = tXT.Replace(((char)1004).ToString(), "<H4>");
            tXT = tXT.Replace(((char)1003).ToString(), "<H3>");
            tXT = tXT.Replace(((char)1020).ToString(), "<");
            tXT = tXT.Replace(((char)1021).ToString(), ">");
            return tXT;
        }

        /// <summary>
        /// Entfernt ( und ), { und } und " und leerzeichen am Anfang/Ende
        /// </summary>
        /// <param name="txt"></param>
        /// <param name="klammern"></param>
        /// <param name="geschklammern"></param>
        /// <param name="gänsef"></param>
        /// <returns></returns>
        public static string DeKlammere(this string txt, bool klammern, bool geschklammern, bool gänsef, bool trimspace) {
            if (trimspace) { txt = txt.Trim(); }
            if (klammern && txt.StartsWith("(") && txt.EndsWith(")")) {
                return txt.Substring(1, txt.Length - 2).DeKlammere(klammern, geschklammern, gänsef, trimspace); // Unnötige Klammern entfernen und noch Ne Runde!!!!
            }
            if (geschklammern && txt.StartsWith("{") && txt.EndsWith("}")) {
                return txt.Substring(1, txt.Length - 2).DeKlammere(klammern, geschklammern, gänsef, trimspace); // Unnötige Klammern entfernen und noch Ne Runde!!!!
            }
            if (gänsef && txt.StartsWith("\"") && txt.EndsWith("\"")) {
                return txt.Substring(1, txt.Length - 2).DeKlammere(klammern, geschklammern, gänsef, trimspace); // Unnötige Klammern entfernen und noch Ne Runde!!!!
            }
            return txt;
        }

        public static string FromNonCritical(this string txt) {
            // http://www.theasciicode.com.ar/ascii-printable-characters/braces-curly-brackets-opening-ascii-code-123.html
            if (string.IsNullOrEmpty(txt)) { return string.Empty; }
            if (txt.Length < 3) { return txt; }
            if (!txt.Contains(";") && !txt.Contains("[")) { return txt; }
            txt = txt.Replace("[A]", ";");
            txt = txt.Replace("[B]", "<");
            txt = txt.Replace("[C]", ">");
            txt = txt.Replace("[D]", "\r\n");
            txt = txt.Replace("[E]", "\r");
            txt = txt.Replace("[F]", "\n");
            txt = txt.Replace("[G]", "|");
            txt = txt.Replace("[H]", "}");
            txt = txt.Replace("[I]", "{");
            txt = txt.Replace("[J]", "=");
            txt = txt.Replace("[K]", ",");
            txt = txt.Replace("[L]", "&");
            txt = txt.Replace("[M]", "/");
            txt = txt.Replace("[Z]", "[");
            return txt;
        }

        public static bool FromPlusMinus(this string value) {
            if (string.IsNullOrEmpty(value)) { return false; }
            switch (value.ToLower()) {
                case "+":
                case "wahr":
                case "true":
                    return true;

                case "-":
                case "falsch":
                case "false":
                case "":
                    return false;

                default:
                    Develop.DebugPrint(enFehlerArt.Warnung, "'" + value + "' unbekannt!");
                    return false;
            }
        }

        public static string GenerateSlash(this string txt) => txt.Replace("[Slash]", "/");

        /// <summary>
        /// Teilt einen String, der geparsed werden kann in seine Bestandteile auf.
        /// </summary>
        /// <param name="value">Ein String, der mit { beginnt. Z.B. {Wert=100, Wert2=150}</param>
        /// <returns>Gibt immer eine List zurück.</returns>
        public static List<KeyValuePair<string, string>> GetAllTags(this string value) {
            List<KeyValuePair<string, string>> Result = new();
            if (string.IsNullOrEmpty(value) || value.Length < 3) { return Result; }
            if (value.Substring(0, 1) != "{") { return Result; }
            //var Beg = 0;
            //while (true) {
            //    Beg++;
            //    if (Beg > value.Length) { break; }
            //    var T = value.ParseTag(Beg);
            //    var V = value.ParseValue(T, Beg);
            //    if (!string.IsNullOrEmpty(T) && !string.IsNullOrEmpty(V)) {
            //        Result.Add(new KeyValuePair<string, string>(T, V));
            //    }
            //    Beg = Beg + T.Length + V.Length + 2;
            //}
            value = value.DeKlammere(false, true, false, true);
            value = value.TrimEnd(",");
            var start = 0;
            var noarunde = true;
            do {
                (var gleichpos, var _) = NextText(value, start, Gleich, false, false);
                if (gleichpos < 0) {
                    Develop.DebugPrint(enFehlerArt.Fehler, "Parsen nicht möglich:" + value);
                }
                var tag = value.Substring(start, gleichpos - start).Trim().ToLower();
                if (string.IsNullOrEmpty(tag)) {
                    Develop.DebugPrint(enFehlerArt.Fehler, "Parsen nicht möglich:" + value);
                }
                (var kommapos, var _) = NextText(value, gleichpos, Komma, false, true);
                string tagval;
                if (kommapos < 0) {
                    tagval = value.Substring(gleichpos + 1).Trim();
                    noarunde = false;
                } else {
                    tagval = value.Substring(gleichpos + 1, kommapos - gleichpos - 1).Trim();
                }
                Result.Add(new KeyValuePair<string, string>(tag, tagval));
                start = kommapos + 1;
            }
            while (noarunde);
            return Result;
        }

        public static string HTMLSpecialToNormalChar(this string tXT, bool ignoreBR) {
            // http://sonderzeichentabelle.de/
            // http://www.htmlhelp.com/reference/html40/entities/special.html
            tXT = tXT.Replace("&amp;", "&"); // Wichtig! An erster Stelle! ä-> &auml; -> &amp;auml;
            tXT = tXT.Replace("<H7>", ((char)1007).ToString());
            tXT = tXT.Replace("<H4>", ((char)1004).ToString());
            tXT = tXT.Replace("<H3>", ((char)1003).ToString());
            tXT = tXT.Replace("&auml;", "ä");
            tXT = tXT.Replace("&ouml;", "ö");
            tXT = tXT.Replace("&uuml;", "ü");
            tXT = tXT.Replace("&Auml;", "Ä");
            tXT = tXT.Replace("&Ouml;", "Ö");
            tXT = tXT.Replace("&Uuml;", "Ü");
            tXT = tXT.Replace("&szlig;", "ß");
            tXT = tXT.Replace("&Eacute;", "É");
            tXT = tXT.Replace("&eacute;", "é");
            tXT = tXT.Replace("&euro;", "€");
            tXT = tXT.Replace("&Oslash;", "Ø");
            tXT = tXT.Replace("&oslash;", "ø");
            tXT = tXT.Replace("&bull;", "•");
            if (!ignoreBR) {
                tXT = tXT.Replace("<br>", "\r");
            }
            tXT = tXT.Replace("<", ((char)1020).ToString());
            tXT = tXT.Replace(">", ((char)1021).ToString());
            tXT = tXT.Replace("&lt;", "<");
            tXT = tXT.Replace("&gt;", ">");
            tXT = tXT.Replace("&quot;", "\"");
            return tXT;
        }

        public static int IndexOfWord(this string input, string value, int startIndex, RegexOptions options) {
            if (options != RegexOptions.IgnoreCase) { Develop.DebugPrint(enFehlerArt.Fehler, "Regex option nicht erlaubt."); }
            value = value.ToUpper();
            input = " " + input.ToUpper() + " ";
            startIndex++;
            while (true) {
                if (startIndex > input.Length - 1) { return -1; }
                startIndex = input.IndexOf(value, startIndex);
                if (startIndex < 0) { return -1; }
                if (startIndex > 0 && startIndex < input.Length - value.Length) {
                    if (input[startIndex - 1].isWordSeperator() && input[startIndex + value.Length].isWordSeperator()) {
                        return startIndex - 1; // -1, weil ein Leereichen hinzugefügt wurde.
                    }
                    startIndex += value.Length;
                }
            }
        }

        public static string Insert(this string tXT, string insertTxt, string afterTXT, string WhenNotContais) {
            if (string.IsNullOrEmpty(afterTXT)) { return tXT; }
            if (string.IsNullOrEmpty(insertTxt)) { return tXT; }
            if (string.IsNullOrEmpty(tXT)) { return tXT; }
            if (!tXT.Contains(afterTXT)) { return tXT; }
            var Pos = -1;
            while (true) {
                Pos++;
                var InsterPos = Pos + afterTXT.Length;
                if (InsterPos > tXT.Length) { break; }
                if (tXT.Substring(Pos, afterTXT.Length) == afterTXT) {
                    if (InsterPos == tXT.Length || !WhenNotContais.Contains(tXT.Substring(InsterPos, 1))) {
                        tXT = tXT.Insert(InsterPos, insertTxt);
                        Pos += insertTxt.Length;
                        // Stop
                    }
                }
            }
            return tXT;
        }

        public static bool IsDouble(this string tXT) => tXT is not null && tXT.IsFormat(enDataFormat.Gleitkommazahl);

        public static bool IsHTMLColorCode(this string col) => !string.IsNullOrEmpty(col) && (col.Length == 6 || col.Length == 8) && col.ContainsOnlyChars(Constants.Char_Numerals + "abcdefABCDEF");

        public static bool IsLong(this string tXT) => tXT is not null && tXT.IsFormat(enDataFormat.Ganzzahl);

        public static bool IsNumeral(this string tXT) => tXT is not null && tXT.IsFormat(enDataFormat.Ganzzahl) || tXT.IsFormat(enDataFormat.Gleitkommazahl);

        // public static List<byte> ToByteList(this string TXT) {
        //    var x = new List<byte>();
        //    x.AddRange(Encoding.ASCII.GetBytes(TXT));
        //    return x;
        // }
        public static bool isPossibleLineBreak(this char value) {
            const string TR = " ?!%/\\}])-.,;_°~€|\r\n\t";
            // Kein Doppelpunkt, weil auch 3:50 Uhr möglich ist
            return TR.Contains(value.ToString());
        }

        public static bool isWordSeperator(this char value) {
            const string TR = "~|=<>+`´\r\n\t";
            return char.IsPunctuation(value) || char.IsSeparator(value) || TR.Contains(value.ToString());
        }

        public static (int pos, string witch) NextText(string txt, int startpos, List<string> searchfor, bool checkforSeparatorbefore, bool checkforSeparatorafter) {
            var klammern = 0;
            var Gans = false;
            var GeschwKlammern = 0;
            //var EckigeKlammern = 0;
            var pos = startpos;
            var maxl = txt.Length;
            const string TR = "&.,;\\?!\" ~|=<>+-(){}[]/*`´\r\n\t";
            do {
                if (pos >= maxl) { return (-1, string.Empty); ; }

                #region Klammer und " erledigen

                switch (txt.Substring(pos, 1)) {
                    // Gänsefüsschen, immer erlaubt
                    case "\"":
                        Gans = !Gans;
                        break;
                    //// Ekige klammern könne in { oder ( vorkommen, immer erlaubt
                    //case "[":
                    //    if (!Gans) {
                    //        EckigeKlammern++;
                    //    }
                    //    break;
                    //case "]":
                    //    if (!Gans) {
                    //        if (EckigeKlammern < 1) { return (-1, string.Empty); }
                    //        EckigeKlammern--;
                    //    }
                    //    break;
                    // Runde klammern können in { vorkommen
                    case "(":
                        if (!Gans) {
                            //if (EckigeKlammern > 0) { return (-1, string.Empty); }
                            if (klammern == 0 && GeschwKlammern == 0 && searchfor.Contains("(")) {
                                return (pos, "(");
                            }
                            klammern++;
                        }
                        break;

                    case ")":
                        if (!Gans) {
                            //if (EckigeKlammern > 0) { return (-1, string.Empty); }
                            if (klammern < 1) { return (-1, string.Empty); }
                            klammern--;
                        }
                        break;
                    // Gescheifte klammern müssen immer sauber auf und zu gemacht werdrn!
                    case "{":
                        if (!Gans) {
                            if (klammern > 0) { return (-1, string.Empty); }
                            //if (EckigeKlammern > 0) { return (-1, string.Empty); }
                            //if (GeschwKlammern) { return (-1, string.Empty); }
                            GeschwKlammern++;
                        }
                        break;

                    case "}":
                        if (!Gans) {
                            if (klammern > 0) { return (-1, string.Empty); }
                            //if (EckigeKlammern > 0) { return (-1, string.Empty); }
                            if (GeschwKlammern < 1) { return (-1, string.Empty); }
                            GeschwKlammern--;
                        }
                        break;
                }

                #endregion Klammer und " erledigen

                #region Den Text suchen

                if (klammern == 0 && !Gans && GeschwKlammern == 0) {
                    if (!checkforSeparatorbefore || pos == 0 || TR.Contains(txt.Substring(pos - 1, 1))) {
                        foreach (var thisEnd in searchfor) {
                            if (pos + thisEnd.Length <= maxl) {
                                if (txt.Substring(pos, thisEnd.Length).ToLower() == thisEnd.ToLower()) {
                                    if (!checkforSeparatorafter || pos + thisEnd.Length >= maxl || TR.Contains(txt.Substring(pos + thisEnd.Length, 1))) {
                                        return (pos, thisEnd);
                                    }
                                }
                            }
                        }
                    }
                }

                #endregion Den Text suchen

                pos++;
            } while (true);
        }

        /// <summary>
        /// Löscht alle Zeichen - außder dem erlaubten - aus dem String. Gross- und Kleinschreibung wird unterschieden.
        /// "RemoveChars" macht das Gegenteil
        /// </summary>
        /// <param name="tXT">Der zu bereinigende Text</param>
        /// <param name="chars">Die noch erlaubten Zeichen</param>
        /// <returns>Der bereinigte Text mit nur noch den erlaubten Zeichen</returns>
        /// <remarks></remarks>
        public static string ReduceToChars(this string tXT, string chars) {
            var p = 0;
            while (p < tXT.Length) {
                if (!chars.Contains(tXT.Substring(p, 1))) {
                    tXT = tXT.Replace(tXT.Substring(p, 1), string.Empty);
                } else {
                    p++;
                }
            }
            return tXT;
        }

        /// <summary>
        /// Reduziert den Text auf ein Minimum, Trennzeichen ist \r\n
        /// </summary>
        /// <param name="vText">Beispiel: Hund frißt Knochen, Hund vergräbt knochen.</param>
        /// <param name="vSearch">Beispiel: Hund * Kochen.</param>
        /// <returns>Beispiel: frißt \r\n vergräbt</returns>
        /// <remarks></remarks>
        public static List<string> ReduceToMulti(this string vText, string vSearch) {
            if (vSearch.CountString("*") != 1) { return null; }
            var e = vSearch.Split('*');
            if (e.Length != 2) { return null; }
            List<string> txt = new();
            var enx = 0;
            while (true) {
                var bgx = vText.ToUpper().IndexOf(e[0].ToUpper(), enx);
                if (bgx < 0) { break; }
                enx = vText.ToUpper().IndexOf(e[1].ToUpper(), bgx + e[0].Length);
                if (bgx + e[0].Length > enx) { break; }
                txt.Add(vText.Substring(bgx + e[0].Length, enx - bgx - e[0].Length));
            }
            return txt;
        }

        /// <summary>
        /// Löscht alle angegebnen Zeichen aus dem String. Gross- und Kleinschreibung wird unterschieden.
        /// "ReduceToChars" macht das Gegenteil
        /// </summary>
        /// <param name="tXT">Der zu bereinigende Text</param>
        /// <param name="chars">Die zu entfernenden Zeichen</param>
        /// <returns>der bereinigte Text ohne die unerwünschten Zeichen</returns>
        /// <remarks></remarks>
        public static string RemoveChars(this string tXT, string chars) {
            for (var z = 0; z < chars.Length; z++) {
                tXT = tXT.Replace(chars.Substring(z, 1), string.Empty);
            }
            return tXT;
        }

        public static string RemoveHTMLCodes(this string html) => Regex.Replace(html, "<.*?>", string.Empty);

        public static string Replace(this string tXT, string alt, string neu, RegexOptions options) {
            if (options != RegexOptions.IgnoreCase) { Develop.DebugPrint(enFehlerArt.Fehler, "Regex option nicht erlaubt."); }
            if (string.IsNullOrEmpty(alt)) { Develop.DebugPrint(enFehlerArt.Fehler, "ALT is Empty"); }
            var OldPos = 0;
            while (true) {
                if (string.IsNullOrEmpty(tXT)) { return tXT; }
                var Posx = tXT.ToUpper().IndexOf(alt.ToUpper(), OldPos);
                if (Posx >= 0) {
                    tXT = tXT.Substring(0, Posx) + neu + tXT.Substring(Posx + alt.Length);
                    OldPos = Posx + neu.Length;
                } else {
                    return tXT;
                }
            }
        }

        public static string ReplaceWord(this string input, string alt, string replacement, RegexOptions options) {
            // return Regex.Replace(input, "\\b" + Alt + "\\b", replacement);
            if (options != RegexOptions.IgnoreCase) { Develop.DebugPrint(enFehlerArt.Fehler, "Regex option nicht erlaubt."); }
            if (replacement.IndexOf(alt, StringComparison.OrdinalIgnoreCase) >= 0) {
                const string du = "@DUMMY@";
                input = ReplaceWord(input, alt, du, options);
                input = ReplaceWord(input, du, replacement, options);
                return input;
            }
            while (true) {
                var start = IndexOfWord(input, alt, 0, options);
                if (start < 0) { return input; }
                input = input.Substring(0, start) + replacement + input.Substring(start + alt.Length);
            }
        }

        public static string Reverse(this string tXT) {
            var charArray = tXT.ToCharArray();
            Array.Reverse(charArray);
            return new string(charArray);
        }

        public static string SetLenght(this string s, int anzahl) => s.Length == anzahl ? s : s.Length < anzahl ? s.PadRight(anzahl) : s.Substring(0, anzahl);

        /// <summary>
        /// Trennt den Text mittels dem Trennzeichen. Sind ein oder mehrere Trennzeichen am Ende, werden Leeren Felder diese nicht zurückgegeben.
        /// </summary>
        /// <param name="textToSplit"></param>
        /// <param name="trennzeichen"></param>
        /// <returns></returns>
        public static string[] SplitBy(this string textToSplit, string trennzeichen) {
            var w = new string[0];
            if (string.IsNullOrEmpty(textToSplit)) { return w; }
            textToSplit = textToSplit.TrimEnd(trennzeichen);
            w = textToSplit.Split(new[] { trennzeichen }, StringSplitOptions.None);
            if (w.Length == 1 && string.IsNullOrEmpty(w[0])) { w = new string[0]; }
            return w;
        }

        public static string[] SplitByCR(this string textToSplit) {
            var w = new string[0];
            if (string.IsNullOrEmpty(textToSplit)) { return w; }
            textToSplit = textToSplit.Replace("\r\n", "\r");
            textToSplit = textToSplit.Replace("\n", "\r");
            return textToSplit.SplitBy("\r");
        }

        public static List<string> SplitByCRToList(this string textToSplit) {
            List<string> w = new();
            if (string.IsNullOrEmpty(textToSplit)) { return w; }
            textToSplit = textToSplit.Replace("\r\n", "\r");
            textToSplit = textToSplit.Replace("\n", "\r");
            w.AddRange(textToSplit.SplitBy("\r"));
            return w;
        }

        //public static string ParseTag(this string tXT, int startIndex) {
        //    var IG = tXT.IndexOf("=", startIndex);
        //    if (IG < 1) { return string.Empty; }
        //    var BG = IG - 1;
        //    while (BG >= 1) {
        //        if (" ,{".Contains(tXT.Substring(BG, 1))) { break; }
        //        BG--;
        //    }
        //    return tXT.Substring(BG + 1, IG - BG - 1).ToLower();
        //}
        //public static string ParseValue(this string tXT, string offTag, int startIndex) {
        //    if (string.IsNullOrEmpty(tXT)) { return string.Empty; }
        //    if (string.IsNullOrEmpty(offTag)) { return string.Empty; }
        //    var FirstCharAfterEquals = tXT.ToLower().IndexOf(offTag.ToLower() + "=", startIndex);
        //    if (FirstCharAfterEquals < 0) { return string.Empty; }
        //    FirstCharAfterEquals = FirstCharAfterEquals + offTag.Length + 1;
        //    // while (TXT.Substring(FirstCharAfterEquals, 1) == " ")
        //    // {
        //    //    FirstCharAfterEquals++;
        //    // }
        //    var OpenBraketCount = 0;
        //    var CurrentChar = FirstCharAfterEquals - 1; // Wird im ersten Step wieder erhöht
        //    var ExitDo = false;
        //    while (!ExitDo && CurrentChar < tXT.Length - 1) {
        //        CurrentChar++;
        //        switch (tXT.Substring(CurrentChar, 1)) {
        //            case ",":
        //                if (OpenBraketCount == 0) {
        //                    if (CurrentChar < tXT.Length - 2) {
        //                        if (!Constants.Char_Numerals.Contains(tXT.Substring(CurrentChar + 1, 1))) {
        //                            CurrentChar -= 1;
        //                            ExitDo = true;
        //                        }
        //                    } else {
        //                        Develop.DebugPrint(enFehlerArt.Warnung, "Unerlaubtes Zeichen nach Komma: " + tXT);
        //                    }
        //                }
        //                break;
        //            case " ":
        //                if (tXT.Substring(CurrentChar - 1, 1) == "," && OpenBraketCount == 0) {
        //                    CurrentChar -= 2;
        //                    ExitDo = true;
        //                }
        //                break;
        //            case "{":
        //                OpenBraketCount++;
        //                break;
        //            case "}":
        //                if (OpenBraketCount == 0) {
        //                    CurrentChar--;
        //                    ExitDo = true;
        //                } else {
        //                    OpenBraketCount--;
        //                }
        //                break;
        //        }
        //    }
        //    if (OpenBraketCount != 0) { Develop.DebugPrint(enFehlerArt.Fehler, "Parse Fehler: " + tXT); }
        //    return tXT.Substring(FirstCharAfterEquals, CurrentChar - FirstCharAfterEquals + 1);
        //}
        public static string StarkeVereinfachung(this string tXT, string additinalAllowed) {
            tXT = tXT.ToLower().ReduceToChars(Constants.Char_Numerals + Constants.Char_Buchstaben + additinalAllowed);
            tXT = tXT.Replace("ä", "ae");
            tXT = tXT.Replace("ö", "oe");
            tXT = tXT.Replace("ü", "ue");
            tXT = tXT.Replace("á", "a");
            tXT = tXT.Replace("ó", "o");
            tXT = tXT.Replace("ú", "u");
            tXT = tXT.Replace("í", "i");
            tXT = tXT.Replace("é", "e");
            tXT = tXT.Replace("à", "a");
            tXT = tXT.Replace("ò", "o");
            tXT = tXT.Replace("ù", "u");
            tXT = tXT.Replace("ì", "i");
            tXT = tXT.Replace("è", "e");
            tXT = tXT.Replace("â", "a");
            tXT = tXT.Replace("ô", "o");
            tXT = tXT.Replace("û", "u");
            tXT = tXT.Replace("î", "i");
            tXT = tXT.Replace("ê", "e");
            tXT = tXT.Replace("ž", "z");
            tXT = tXT.Replace("ß", "ss");
            return tXT;
        }

        public static string ToNonCritical(this string txt) {
            // http://www.theasciicode.com.ar/ascii-printable-characters/braces-curly-brackets-opening-ascii-code-123.html
            if (string.IsNullOrEmpty(txt)) { return string.Empty; }
            txt = txt.Replace("[", "[Z]");
            txt = txt.Replace(";", "[A]");
            txt = txt.Replace("<", "[B]");
            txt = txt.Replace(">", "[C]");
            txt = txt.Replace("\r\n", "[D]");
            txt = txt.Replace("\r", "[E]");
            txt = txt.Replace("\n", "[F]");
            txt = txt.Replace("|", "[G]");
            txt = txt.Replace("}", "[H]");
            txt = txt.Replace("{", "[I]");
            txt = txt.Replace("=", "[J]");
            txt = txt.Replace(",", "[K]");
            txt = txt.Replace("&", "[L]");
            return txt;
        }

        public static string Trim(this string tXT, string was) {
            if (string.IsNullOrEmpty(tXT)) { return string.Empty; }
            tXT = tXT.TrimEnd(was);
            return string.IsNullOrEmpty(tXT) ? string.Empty : tXT.TrimStart(was);
        }

        public static string TrimCr(this string tXT) => string.IsNullOrEmpty(tXT) ? string.Empty : tXT.Trim("\r");

        public static string TrimEnd(this string tXT, string was) {
            if (string.IsNullOrEmpty(tXT)) { return string.Empty; }
            if (was.Length < 1) { Develop.DebugPrint(enFehlerArt.Fehler, "Trimmen nicht möglich mit: " + was); }
            was = was.ToUpper();
            while (tXT.Length >= was.Length && tXT.Substring(tXT.Length - was.Length).ToUpper() == was) {
                tXT = tXT.Remove(tXT.Length - was.Length);
            }
            return tXT;
        }

        public static string TrimStart(this string tXT, string was) {
            if (string.IsNullOrEmpty(tXT)) { return string.Empty; }
            if (was.Length < 1) { Develop.DebugPrint(enFehlerArt.Fehler, "Trimmen nicht möglich mit: " + was); }
            was = was.ToUpper();
            while (tXT.Length >= was.Length && tXT.Substring(0, was.Length).ToUpper() == was) {
                tXT = tXT.Remove(0, was.Length);
            }
            return tXT;
        }

        public static byte[] Unicode_ToByte(this string tXT) => Encoding.Unicode.GetBytes(tXT);

        public static byte[] UTF8_ToByte(this string tXT) => Encoding.UTF8.GetBytes(tXT);

        public static byte[] WIN1252_toByte(this string tXT) => Encoding.GetEncoding(1252).GetBytes(tXT);

        #endregion

        // var enc1252 = CodePagesEncodingProvider.Instance.GetEncoding(1252);
    }
}