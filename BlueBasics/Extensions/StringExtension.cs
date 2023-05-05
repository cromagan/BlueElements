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
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using BlueBasics.Enums;
using BlueBasics.Interfaces;
using static BlueBasics.Converter;

namespace BlueBasics;

public static partial class Extensions {

    #region Fields

    public static readonly List<string> GeschKlammerAuf = new() { "{" };
    public static readonly List<string> GeschKlammerZu = new() { "}" };
    public static readonly List<string> Gleich = new() { "=" };
    public static readonly List<string> KlammerAuf = new() { "(" };
    public static readonly List<List<string>> KlammernGeschweift = new() { new List<string> { "{", "}" } };
    public static readonly List<List<string>> KlammernStd = new() { new List<string> { "(", ")" }, new List<string> { "{", "}" }, new List<string> { "[", "]" } };
    public static readonly List<string> KlammerZu = new() { ")" };
    public static readonly List<string> Komma = new() { "," };

    #endregion

    //public static readonly List<string> Tilde = new() { "~" };

    #region Methods

    /// <summary>
    /// Gibt eine Liste aller erkannten Wörter des Textes zurück.
    /// Mit Doppeleten und unsortiert.
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    public static List<string> AllWords(this string input) {
        input = " " + input + " ";
        var position = 0;
        var lastSeperator = 0;
        List<string> l = new();
        while (true) {
            position++;
            if (position >= input.Length) { return l; }

            if (!input[position].IsWordSeperator()) { continue; }

            if (position > lastSeperator + 1) {
                l.Add(input.Substring(lastSeperator + 1, position - lastSeperator - 1));
            }
            lastSeperator = position;
        }
    }

    public static bool CanCut(this string txt, string start, string ende) {
        if (!txt.StartsWith(start) || !txt.EndsWith(ende)) { return false; }

        var (pose, _) = NextText(txt, 0, new List<string> { ende }, false, false, KlammernStd);
        return pose == txt.Length - 1;
    }

    public static string CompareKey(this string isValue, SortierTyp format) {
        var compareKeySOk = Constants.SecondSortChar + "X";
        var compareKeySNok = Constants.SecondSortChar + "A";

        switch (format) {
            case SortierTyp.ZahlenwertInt:
                if (string.IsNullOrEmpty(isValue)) { return compareKeySNok + "A0000000000"; }
                if (IntTryParse(isValue, out var w)) {
                    return w >= 0
                        ? compareKeySOk + "A" + w.ToString(Constants.Format_Integer10)
                        : compareKeySOk + w.ToString(Constants.Format_Integer10);
                }

                return compareKeySNok + isValue;

            case SortierTyp.Original_String:
                return Constants.SecondSortChar + isValue;

            case SortierTyp.Sprachneutral_String:

                if (string.IsNullOrEmpty(isValue)) { return string.Empty; }

                return Constants.SecondSortChar + isValue.Sprachneutral();

            case SortierTyp.ZahlenwertFloat:
                if (string.IsNullOrEmpty(isValue)) { return "A0000000000,000"; }
                if (DoubleTryParse(isValue, out var dw)) {
                    var t = dw.ToString(Constants.Format_Float10_3);
                    if (!t.Contains(",")) { t += ",000"; }

                    if (dw >= 0) { t = "A" + t; }
                    while (t.Length < 15) { t += "0"; }
                    return compareKeySOk + t;
                }

                return compareKeySNok + isValue;

            case SortierTyp.Datum_Uhrzeit:
                return DateTimeTryParse(isValue, out var d) ? compareKeySNok + d.ToString(Constants.Format_Date) : compareKeySNok + isValue;

            default:
                Develop.DebugPrint(format);
                return Constants.SecondSortChar + isValue;
        }
    }

    public static bool ContainsChars(this string tXt, string chars) => chars.Where((_, z) => tXt.Contains(chars.Substring(z, 1))).Any();

    public static bool ContainsOnlyChars(this string tXt, string chars) => !tXt.Where((_, z) => !chars.Contains(tXt.Substring(z, 1))).Any();

    public static bool ContainsWord(this string input, string value, RegexOptions options) => input.IndexOfWord(value, 0, options) >= 0;

    public static string ConvertFromHtmlToRich(this string txt) {
        txt = txt.Replace("\r\n", "<br>");
        txt = txt.Replace("<<>", "&lt;");
        txt = txt.Replace("<>>", "&gt;");
        return txt;
    }

    public static int CountString(this string text, string value) {
        var anz = 0;
        for (var z = 0; z <= text.Length - value.Length; z++) {
            if (text.Substring(z, value.Length) == value) { anz++; }
        }
        return anz;
    }

    public static string CreateHtmlCodes(this string tXt, bool crlftoo) {
        // http://sonderzeichentabelle.de/
        // http://www.htmlhelp.com/reference/html40/entities/special.html
        tXt = tXt.Replace("&", "&amp;"); // Wichtig! An erster Stelle! ä-> &auml; -> &amp;auml;
        tXt = tXt.Replace("ä", "&auml;");
        tXt = tXt.Replace("ö", "&ouml;");
        tXt = tXt.Replace("ü", "&uuml;");
        tXt = tXt.Replace("Ä", "&Auml;");
        tXt = tXt.Replace("Ö", "&Ouml;");
        tXt = tXt.Replace("Ü", "&Uuml;");
        tXt = tXt.Replace("ß", "&szlig;");
        tXt = tXt.Replace("É", "&Eacute;");
        tXt = tXt.Replace("é", "&eacute;");
        tXt = tXt.Replace("€", "&euro;");
        tXt = tXt.Replace("Ø", "&Oslash;");
        tXt = tXt.Replace("ø", "&oslash;");
        tXt = tXt.Replace("•", "&bull;");
        tXt = tXt.Replace("<", "&lt;");
        tXt = tXt.Replace(">", "&gt;");
        tXt = tXt.Replace("\"", "&quot;");
        if (!crlftoo) { return tXt; }
        tXt = tXt.Replace("\r\n", "<br>", RegexOptions.IgnoreCase);
        tXt = tXt.Replace("\r", "<br>", RegexOptions.IgnoreCase);
        tXt = tXt.Replace("\n", "<br>", RegexOptions.IgnoreCase);
        tXt = tXt.Replace(((char)1007).ToString(), "<H7>");
        tXt = tXt.Replace(((char)1004).ToString(), "<H4>");
        tXt = tXt.Replace(((char)1003).ToString(), "<H3>");
        tXt = tXt.Replace(((char)1020).ToString(), "<");
        tXt = tXt.Replace(((char)1021).ToString(), ">");
        return tXt;
    }

    public static string CutToUtf8Length(this string str, int byteLength) {
        // https://stackoverflow.com/questions/1225052/best-way-to-shorten-utf8-string-based-on-byte-length
        var byteArray = Encoding.UTF8.GetBytes(str);

        if (byteArray.Length <= byteLength) { return str; }

        var returnValue = string.Empty;

        var bytePointer = byteLength;

        // Check high bit to see if we're [potentially] in the middle of a multi-byte char
        if (bytePointer >= 0
            && (byteArray[bytePointer] & Convert.ToByte("10000000", 2)) > 0) {
            // If so, keep walking back until we have a byte starting with `11`,
            // which means the first byte of a multi-byte UTF8 character.
            while (bytePointer >= 0
                && Convert.ToByte("11000000", 2) != (byteArray[bytePointer] & Convert.ToByte("11000000", 2))) {
                bytePointer--;
            }
        }

        // See if we had 1s in the high bit all the way back. If so, we're toast. Return empty string.
        if (0 != bytePointer) {
            returnValue = Encoding.UTF8.GetString(byteArray, 0, bytePointer); // hat tip to @NealEhardt! Well played. ;^)
        }

        return returnValue;
    }

    /// <summary>
    /// Entfernt ( und ), { und } und " und leerzeichen am Anfang/Ende
    /// </summary>
    /// <param name="txt"></param>
    /// <param name="klammern"></param>
    /// <param name="geschklammern"></param>
    /// <param name="gänsef"></param>
    /// <param name="trimspace"></param>
    /// <returns></returns>
    public static string DeKlammere(this string txt, bool klammern, bool geschklammern, bool gänsef, bool trimspace) {
        if (trimspace) { txt = txt.Trim(); }

        if (klammern && txt.CanCut("(", ")")) {
            return txt.Substring(1, txt.Length - 2).DeKlammere(klammern, geschklammern, gänsef, trimspace);
        }

        if (geschklammern && txt.CanCut("{", "}")) {
            return txt.Substring(1, txt.Length - 2).DeKlammere(klammern, geschklammern, gänsef, trimspace);
        }

        return gänsef && txt.CanCut("\"", "\"")
            ? txt.Substring(1, txt.Length - 2).DeKlammere(klammern, geschklammern, gänsef, trimspace)
            : txt;
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
        txt = txt.Replace("[N]", "\""); // Um Anfang und Ende von Texten richtig zu finden. z.B. 1 1/2"
        txt = txt.Replace("[Z]", "[");
        return txt;
    }

    public static string FromNonCriticalWithQuote(this string txt) => txt.Substring(1, txt.Length - 2).FromNonCritical();

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
                Develop.DebugPrint(FehlerArt.Warnung, "'" + value + "' unbekannt!");
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
        List<KeyValuePair<string, string>> result = new();
        if (string.IsNullOrEmpty(value) || value.Length < 3) { return result; }
        if (value.Substring(0, 1) != "{") { return result; }

        //value = value.DeKlammere(false, true, false, true);

        //if (value.StartsWith("{") && value.EndsWith("}")) {
        //    //Develop.DebugPrint("Entklammerung fehlgeschlagen: " + value);
        //    value = value.Substring(1, value.Length - 2);
        //}

        //value = value.TrimEnd(",");
        var start = 1;
        var noarunde = true;
        do {
            var (gleichpos, _) = NextText(value, start, Gleich, false, false, KlammernGeschweift);
            if (gleichpos < 0) {
                Develop.DebugPrint(FehlerArt.Fehler, "Parsen nicht möglich:" + value);
            }
            var tag = value.Substring(start, gleichpos - start).Trim().ToLower();
            if (string.IsNullOrEmpty(tag)) {
                Develop.DebugPrint(FehlerArt.Fehler, "Parsen nicht möglich:" + value);
            }
            var (kommapos, _) = NextText(value, gleichpos, Komma, false, true, KlammernGeschweift);
            string tagval;
            if (kommapos < 0) {
                tagval = value.Substring(gleichpos + 1, value.Length - gleichpos - 2).Trim();
                noarunde = false;
            } else {
                tagval = value.Substring(gleichpos + 1, kommapos - gleichpos - 1).Trim();

                var test = value.Substring(kommapos);
                if (test is ",}" or ", }") { noarunde = false; }
            }
            result.Add(new KeyValuePair<string, string>(tag, tagval));
            start = kommapos + 1;
        }
        while (noarunde);
        return result;
    }

    public static string HtmlSpecialToNormalChar(this string tXt, bool ignoreBr) {
        // http://sonderzeichentabelle.de/
        // http://www.htmlhelp.com/reference/html40/entities/special.html
        tXt = tXt.Replace("&amp;", "&"); // Wichtig! An erster Stelle! ä-> &auml; -> &amp;auml;
        tXt = tXt.Replace("<H7>", ((char)1007).ToString());
        tXt = tXt.Replace("<H4>", ((char)1004).ToString());
        tXt = tXt.Replace("<H3>", ((char)1003).ToString());
        tXt = tXt.Replace("&auml;", "ä");
        tXt = tXt.Replace("&ouml;", "ö");
        tXt = tXt.Replace("&uuml;", "ü");
        tXt = tXt.Replace("&Auml;", "Ä");
        tXt = tXt.Replace("&Ouml;", "Ö");
        tXt = tXt.Replace("&Uuml;", "Ü");
        tXt = tXt.Replace("&szlig;", "ß");
        tXt = tXt.Replace("&Eacute;", "É");
        tXt = tXt.Replace("&eacute;", "é");
        tXt = tXt.Replace("&euro;", "€");
        tXt = tXt.Replace("&Oslash;", "Ø");
        tXt = tXt.Replace("&oslash;", "ø");
        tXt = tXt.Replace("&bull;", "•");
        if (!ignoreBr) {
            tXt = tXt.Replace("<br>", "\r");
        }
        tXt = tXt.Replace("<", ((char)1020).ToString());
        tXt = tXt.Replace(">", ((char)1021).ToString());
        tXt = tXt.Replace("&lt;", "<");
        tXt = tXt.Replace("&gt;", ">");
        tXt = tXt.Replace("&quot;", "\"");
        return tXt;
    }

    public static int IndexOfWord(this string input, string value, int startIndex, RegexOptions options) {
        if (options != RegexOptions.IgnoreCase) { Develop.DebugPrint(FehlerArt.Fehler, "Regex option nicht erlaubt."); }
        value = value.ToUpper();
        input = " " + input.ToUpper() + " ";
        startIndex++;
        while (true) {
            if (startIndex > input.Length - 1) { return -1; }
            startIndex = input.IndexOf(value, startIndex, StringComparison.Ordinal);
            if (startIndex < 0) { return -1; }
            if (startIndex > 0 && startIndex < input.Length - value.Length) {
                if (input[startIndex - 1].IsWordSeperator() && input[startIndex + value.Length].IsWordSeperator()) {
                    return startIndex - 1; // -1, weil ein Leereichen hinzugefügt wurde.
                }
                startIndex += value.Length;
            }
        }
    }

    public static string Insert(this string tXt, string insertTxt, string afterTxt, string whenNotContais) {
        if (string.IsNullOrEmpty(afterTxt)) { return tXt; }
        if (string.IsNullOrEmpty(insertTxt)) { return tXt; }
        if (string.IsNullOrEmpty(tXt)) { return tXt; }
        if (!tXt.Contains(afterTxt)) { return tXt; }
        var pos = -1;
        while (true) {
            pos++;
            var insterPos = pos + afterTxt.Length;
            if (insterPos > tXt.Length) { break; }
            if (tXt.Substring(pos, afterTxt.Length) == afterTxt) {
                if (insterPos == tXt.Length || !whenNotContais.Contains(tXt.Substring(insterPos, 1))) {
                    tXt = tXt.Insert(insterPos, insertTxt);
                    pos += insertTxt.Length;
                    // Stop
                }
            }
        }
        return tXt;
    }

    public static bool IsDateTime(this string? txt) => DateTimeTryParse(txt, out _);

    public static bool IsDouble(this string? txt) => txt is not null && DoubleTryParse(txt, out _);

    public static bool IsHtmlColorCode(this string? txt) => txt != null && !string.IsNullOrEmpty(txt) && txt.Length is 6 or 8 && txt.ContainsOnlyChars(Constants.Char_Numerals + "abcdefABCDEF");

    public static bool IsLong(this string? txt) => txt is not null && LongTryParse(txt, out _);

    public static bool IsNumeral(this string? txt) => txt is not null && (txt.IsLong() || txt.IsDouble());

    // public static List<byte> ToByteList(this string TXT) {
    //    var x = new List<byte>();
    //    x.AddRange(Encoding.ASCII.GetBytes(TXT));
    //    return x;
    // }
    public static bool IsPossibleLineBreak(this char value) {
        const string tr = " ?!%/\\}])-.,;_°~€|\r\n\t";
        // Kein Doppelpunkt, weil auch 3:50 Uhr möglich ist
        return tr.Contains(value.ToString());
    }

    public static bool IsWordSeperator(this char value) {
        const string tr = "~|=<>+`´\r\n\t";
        return char.IsPunctuation(value) || char.IsSeparator(value) || tr.Contains(value.ToString());
    }

    public static (int pos, string which) NextText(string txt, int startpos, List<string> searchfor, bool checkforSeparatorbefore, bool checkforSeparatorafter, List<List<string>>? klammern) {
        var gans = false;
        var pos = startpos;
        var maxl = txt.Length;
        const string tr = "&.,;\\?!\" ~|=<>+-(){}[]/*`´\r\n\t¶";

        var historie = string.Empty;

        do {
            if (pos >= maxl) { return (-1, string.Empty); }

            var ch = txt.Substring(pos, 1);

            var machtezu = false;

            #region Gänsefüßchen und Klammern zu machen

            if (gans) {
                // Wenn ein Gänsefüßchen offen ist, NUR auf weitere Gänsefüßchen reagieren - in einem String darf alles sein.
                if (ch == "\"") { gans = false; machtezu = true; }
            } else {
                if (klammern != null) {
                    foreach (var thisc in klammern) {
                        if (ch == thisc[1]) {
                            if (!historie.EndsWith(thisc[0])) {
                                return (-1, string.Empty);
                            }

                            historie = historie.Substring(0, historie.Length - 1);
                            machtezu = true;
                            break;
                        }
                    }
                }
            }

            #endregion

            #region Den Text suchen

            if (!gans && string.IsNullOrEmpty(historie)) {
                if (!checkforSeparatorbefore || pos == 0 || tr.Contains(txt.Substring(pos - 1, 1))) {
                    foreach (var thisEnd in searchfor) {
                        if (pos + thisEnd.Length <= maxl) {
                            if (string.Equals(txt.Substring(pos, thisEnd.Length), thisEnd, StringComparison.OrdinalIgnoreCase)) {
                                if (!checkforSeparatorafter || pos + thisEnd.Length >= maxl || tr.Contains(txt.Substring(pos + thisEnd.Length, 1))) {
                                    return (pos, thisEnd);
                                }
                            }
                        }
                    }
                }
            }

            #endregion

            #region Gänsefüßchen und Klammern aufmachen

            if (!gans && !machtezu) {
                if (ch == "\"") {
                    gans = true;  // Ab hier fogt ein String
                } else {
                    // Nur die andern Klammern-Paare prüfen. Bei einem Klammer Fehler -1 zurück geben.
                    if (klammern != null) {
                        foreach (var thisc in klammern) {
                            if (ch == thisc[0]) { historie += thisc[0]; break; }
                        }
                    }
                }
            }

            #endregion

            pos++;
        } while (true);
    }

    /// <summary>
    /// Löscht alle Zeichen - außder dem erlaubten - aus dem String. Gross- und Kleinschreibung wird unterschieden.
    /// "RemoveChars" macht das Gegenteil
    /// </summary>
    /// <param name="tXt">Der zu bereinigende Text</param>
    /// <param name="chars">Die noch erlaubten Zeichen</param>
    /// <returns>Der bereinigte Text mit nur noch den erlaubten Zeichen</returns>
    /// <remarks></remarks>
    public static string ReduceToChars(this string tXt, string chars) {
        var p = 0;
        while (p < tXt.Length) {
            if (!chars.Contains(tXt.Substring(p, 1))) {
                tXt = tXt.Replace(tXt.Substring(p, 1), string.Empty);
            } else {
                p++;
            }
        }
        return tXt;
    }

    /// <summary>
    /// Gibt eine Liste von Textstellen zurück, die dem Sternchen entsprechen
    /// </summary>
    /// <param name="text">Beispiel: Hund frisst Knochen, Hund vergräbt Knochen.</param>
    /// <param name="search">Beispiel: Hund * Kochen.</param>
    /// <param name="compare"></param>
    /// <returns>Beispiel: Eine Liste mit: {"frisst", "vergräbt"}</returns>
    /// <remarks></remarks>
    public static List<string>? ReduceToMulti(this string text, string search, StringComparison compare) {
        if (search.CountString("*") != 1) { return null; }
        var e = search.Split('*');
        if (e.Length != 2) { return null; }

        List<string> txt = new();
        var enx = 0;
        while (true) {
            var bgx = text.IndexOf(e[0], enx, compare);
            if (bgx < 0) { break; }
            enx = text.IndexOf(e[1], bgx + e[0].Length, compare);
            if (bgx + e[0].Length > enx) { break; }
            txt.Add(text.Substring(bgx + e[0].Length, enx - bgx - e[0].Length));
        }
        return txt;
    }

    public static bool RegexMatch(this string txt, string regex) => string.IsNullOrEmpty(regex) || new Regex(regex).IsMatch(txt);

    /// <summary>
    /// Löscht alle angegebnen Zeichen aus dem String. Gross- und Kleinschreibung wird unterschieden.
    /// "ReduceToChars" macht das Gegenteil
    /// </summary>
    /// <param name="tXt">Der zu bereinigende Text</param>
    /// <param name="chars">Die zu entfernenden Zeichen</param>
    /// <returns>der bereinigte Text ohne die unerwünschten Zeichen</returns>
    /// <remarks></remarks>
    public static string RemoveChars(this string tXt, string chars) {
        for (var z = 0; z < chars.Length; z++) {
            tXt = tXt.Replace(chars.Substring(z, 1), string.Empty);
        }
        return tXt;
    }

    /// <summary>
    /// Entfernt alle < > aus dem gegebenen Text.
    /// </summary>
    /// <param name="text"></param>
    /// <returns></returns>
    public static string RemoveXmlTags(this string text) => Regex.Replace(text, "<.*?>", string.Empty);

    public static string Replace(this string tXt, string alt, string neu, RegexOptions options) {
        if (options != RegexOptions.IgnoreCase) { Develop.DebugPrint(FehlerArt.Fehler, "Regex option nicht erlaubt."); }
        if (string.IsNullOrEmpty(alt)) { Develop.DebugPrint(FehlerArt.Fehler, "ALT is Empty"); }
        var oldPos = 0;
        while (true) {
            if (string.IsNullOrEmpty(tXt)) { return tXt; }
            var posx = tXt.ToUpper().IndexOf(alt.ToUpper(), oldPos, StringComparison.Ordinal);
            if (posx >= 0) {
                tXt = tXt.Substring(0, posx) + neu + tXt.Substring(posx + alt.Length);
                oldPos = posx + neu.Length;
            } else {
                return tXt;
            }
        }
    }

    public static string ReplaceLowerSign(this string tXt) => tXt.Replace("<", "<<>");

    public static string ReplaceWord(this string input, string alt, string replacement, RegexOptions options) {
        // return Regex.Replace(input, "\\b" + Alt + "\\b", replacement);
        if (options != RegexOptions.IgnoreCase) { Develop.DebugPrint(FehlerArt.Fehler, "Regex option nicht erlaubt."); }
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

    public static string Reverse(this string tXt) {
        var charArray = tXt.ToCharArray();
        Array.Reverse(charArray);
        return new string(charArray);
    }

    public static string SetLenght(this string s, int anzahl) => s.Length == anzahl ? s : s.Length < anzahl ? s.PadRight(anzahl) : s.Substring(0, anzahl);

    /// <summary>
    /// Trennt den Text mittels dem Trennzeichen. Sind ein oder mehrere Trennzeichen am Ende, werden die leeren Felder nicht zurückgegeben.
    /// </summary>
    /// <param name="textToSplit"></param>
    /// <param name="trennzeichen"></param>
    /// <returns></returns>
    public static string[] SplitAndCutBy(this string textToSplit, string trennzeichen) {
        var w = Array.Empty<string>();
        if (string.IsNullOrEmpty(textToSplit)) { return w; }

        textToSplit = textToSplit.TrimEnd(trennzeichen);

        if (string.IsNullOrEmpty(textToSplit)) { return w; }

        w = textToSplit.Split(new[] { trennzeichen }, StringSplitOptions.None);
        //if (w.Length == 1 && string.IsNullOrEmpty(w[0])) { w = new string[0]; }
        return w;
    }

    /// <summary>
    /// Erstellt eine Liste aus dem String, wobei als Trenner \r\n, \r oder \n benutzt werden. Leere Einträge am Ende werden gelöscht.
    /// </summary>
    /// <param name="textToSplit"></param>
    /// <returns></returns>
    public static string[] SplitAndCutByCr(this string textToSplit) {
        var w = Array.Empty<string>();
        if (string.IsNullOrEmpty(textToSplit)) { return w; }
        textToSplit = textToSplit.Replace("\r\n", "\r").Replace("\n", "\r");
        return textToSplit.SplitAndCutBy("\r");
    }

    /// <summary>
    /// Erstellt eine Liste aus dem String, wobei als Trenner \r\n, \r oder \n benutzt werden. Leere Einträge am Ende werden gelöscht.
    /// </summary>
    /// <param name="textToSplit"></param>
    /// <returns></returns>
    public static List<string> SplitAndCutByCrToList(this string textToSplit) {
        List<string> w = new();
        if (string.IsNullOrEmpty(textToSplit)) { return w; }
        w.AddRange(textToSplit.SplitAndCutByCr());
        return w;
    }

    /// <summary>
    /// Splittet den String, ohne etwas zu kürzen.ACHTUNG: Wenn der Text leer ist, wird ein Array mit der Länge 0 zurückgegeben.
    /// </summary>
    /// <param name="textToSplit"></param>
    /// <param name="trennzeichen"></param>
    /// <returns></returns>
    public static string[] SplitBy(this string textToSplit, string trennzeichen) => string.IsNullOrEmpty(textToSplit) ? Array.Empty<string>() : textToSplit.Split(new[] { trennzeichen }, StringSplitOptions.None);

    /// <summary>
    /// Splittet den String, ohne etwas zu kürzen. Zeilenumrüche werden aber vereinfach (\r\n => \r). ACHTUNG: Wenn der Text leer ist, wird ein Array mit der Länge 0 zurückgegeben.
    /// </summary>
    /// <param name="textToSplit"></param>
    /// <returns></returns>
    public static string[] SplitByCr(this string textToSplit) => string.IsNullOrEmpty(textToSplit) ? Array.Empty<string>() : textToSplit.Replace("\r\n", "\r").Replace("\n", "\r").SplitBy("\r");

    /// <summary>
    /// Splittet den String, ohne etwas zu kürzen. Zeilenumrüche werden aber vereinfach (\r\n => \r). ACHTUNG: Wenn der Text leer ist, wird eine Liste mit der Länge 0 zurückgegeben.
    /// </summary>
    /// <param name="textToSplit"></param>
    /// <returns></returns>
    public static List<string> SplitByCrToList(this string textToSplit) {
        List<string> w = new();
        if (string.IsNullOrEmpty(textToSplit)) { return w; }
        w.AddRange(textToSplit.SplitByCr());
        return w;
    }

    public static string Sprachneutral(this string isValue) {
        isValue = isValue.ToLower();
        isValue = isValue.Replace("ä", "a");
        isValue = isValue.Replace("ö", "o");
        isValue = isValue.Replace("ü", "u");
        isValue = isValue.Replace("á", "a");
        isValue = isValue.Replace("ó", "o");
        isValue = isValue.Replace("ú", "u");
        isValue = isValue.Replace("í", "i");
        isValue = isValue.Replace("é", "e");
        isValue = isValue.Replace("à", "a");
        isValue = isValue.Replace("ò", "o");
        isValue = isValue.Replace("ù", "u");
        isValue = isValue.Replace("ì", "i");
        isValue = isValue.Replace("è", "e");
        isValue = isValue.Replace("â", "a");
        isValue = isValue.Replace("ô", "o");
        isValue = isValue.Replace("û", "u");
        isValue = isValue.Replace("î", "i");
        isValue = isValue.Replace("ê", "e");
        isValue = isValue.Replace("ž", "z");
        isValue = isValue.Replace("ß", "s");
        isValue = isValue.TrimStart("\"");
        isValue = isValue.TrimStart("'");
        isValue = isValue.TrimStart(" ");
        return isValue;
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
    public static string StarkeVereinfachung(this string tXt, string additinalAllowed) {
        tXt = tXt.ToLower().ReduceToChars(Constants.Char_Numerals + Constants.Char_Buchstaben + additinalAllowed);
        tXt = tXt.Replace("ä", "ae");
        tXt = tXt.Replace("ö", "oe");
        tXt = tXt.Replace("ü", "ue");
        tXt = tXt.Replace("á", "a");
        tXt = tXt.Replace("ó", "o");
        tXt = tXt.Replace("ú", "u");
        tXt = tXt.Replace("í", "i");
        tXt = tXt.Replace("é", "e");
        tXt = tXt.Replace("à", "a");
        tXt = tXt.Replace("ò", "o");
        tXt = tXt.Replace("ù", "u");
        tXt = tXt.Replace("ì", "i");
        tXt = tXt.Replace("è", "e");
        tXt = tXt.Replace("â", "a");
        tXt = tXt.Replace("ô", "o");
        tXt = tXt.Replace("û", "u");
        tXt = tXt.Replace("î", "i");
        tXt = tXt.Replace("ê", "e");
        tXt = tXt.Replace("ž", "z");
        tXt = tXt.Replace("ß", "ss");
        return tXt;
    }

    /// <summary>
    /// Gibt eine Liste der KeyNamen zurück
    /// </summary>
    /// <param name="items"></param>
    /// <returns></returns>
    public static List<string> ToListOfString(this IEnumerable<IHasKeyName?>? items) {
        List<string> w = new();
        if (items == null || !items.Any()) { return w; }

        w.AddRange(from thisItem in items where thisItem != null where !string.IsNullOrEmpty(thisItem.KeyName) select thisItem.KeyName);
        return w;
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
        txt = txt.Replace("/", "[M]");
        txt = txt.Replace("\"", "[N]");
        return txt;
    }

    public static string ToNonCriticalWithQuote(this string txt) => "\"" + txt.ToNonCritical() + "\"";

    public static string ToTitleCase(this string text) {
        text = text.ToLower().Replace("_", " ");
        var info = CultureInfo.CurrentCulture.TextInfo;
        return info.ToTitleCase(text);
    }

    public static string Trim(this string tXt, string was) {
        if (string.IsNullOrEmpty(tXt)) { return string.Empty; }
        tXt = tXt.TrimEnd(was);
        return string.IsNullOrEmpty(tXt) ? string.Empty : tXt.TrimStart(was);
    }

    public static string TrimCr(this string tXt) => string.IsNullOrEmpty(tXt) ? string.Empty : tXt.Trim("\r");

    public static string TrimEnd(this string tXt, string was) {
        if (string.IsNullOrEmpty(tXt)) { return string.Empty; }
        if (was.Length < 1) { Develop.DebugPrint(FehlerArt.Fehler, "Trimmen nicht möglich mit: " + was); }
        was = was.ToUpper();
        while (tXt.Length >= was.Length && tXt.Substring(tXt.Length - was.Length).ToUpper() == was) {
            tXt = tXt.Remove(tXt.Length - was.Length);
        }
        return tXt;
    }

    public static string TrimStart(this string tXt, string was) {
        if (string.IsNullOrEmpty(tXt)) { return string.Empty; }
        if (was.Length < 1) { Develop.DebugPrint(FehlerArt.Fehler, "Trimmen nicht möglich mit: " + was); }
        was = was.ToUpper();
        while (tXt.Length >= was.Length && tXt.Substring(0, was.Length).ToUpper() == was) {
            tXt = tXt.Remove(0, was.Length);
        }
        return tXt;
    }

    public static byte[] Unicode_ToByte(this string tXt) => Encoding.Unicode.GetBytes(tXt);

    public static byte[] UTF8_ToByte(this string tXt) => Encoding.UTF8.GetBytes(tXt);

    public static byte[] WIN1252_toByte(this string tXt) => Encoding.GetEncoding(1252).GetBytes(tXt);

    #endregion

    // var enc1252 = CodePagesEncodingProvider.Instance.GetEncoding(1252);
}