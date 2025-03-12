// Authors:
// Christian Peter
//
// Copyright (c) 2025 Christian Peter
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
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using BlueBasics.Enums;
using BlueBasics.Interfaces;
using static BlueBasics.Constants;
using static BlueBasics.Converter;

namespace BlueBasics;

public static partial class Extensions {

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
        List<string> l = [];
        while (true) {
            position++;
            if (position >= input.Length) { return l; }

            if (!WordSeparators.Contains(input[position])) { continue; }

            if (position > lastSeperator + 1) {
                l.Add(input.Substring(lastSeperator + 1, position - lastSeperator - 1));
            }
            lastSeperator = position;
        }
    }

    public static bool CanCut(this string txt, string start, string ende) {
        if (!txt.StartsWith(start) || !txt.EndsWith(ende)) { return false; }

        var (pose, _) = NextText(txt, 0, [ende], false, false, KlammernAlle);
        return pose == txt.Length - 1;
    }

    public static string CompareKey(this string isValue, SortierTyp sortType) {
        var compareKeySOk = SecondSortChar + "X";
        var compareKeySNok = SecondSortChar + "A";

        switch (sortType) {
            case SortierTyp.ZahlenwertInt:
                if (string.IsNullOrEmpty(isValue)) { return compareKeySNok + "A0000000000"; }
                if (int.TryParse(isValue, out var w)) {
                    return w >= 0
                        ? compareKeySOk + "A" + w.ToStringInt10()
                        : compareKeySOk + w.ToStringInt10();
                }

                return compareKeySNok + isValue;

            case SortierTyp.Original_String:
                return SecondSortChar + isValue;

            case SortierTyp.Sprachneutral_String:

                if (string.IsNullOrEmpty(isValue)) { return string.Empty; }

                return SecondSortChar + isValue.Sprachneutral();

            case SortierTyp.ZahlenwertFloat:
                if (string.IsNullOrEmpty(isValue)) { return "A0000000000,000"; }
                if (DoubleTryParse(isValue, out var dw)) {
                    var t = dw.ToStringFloat10_3();
                    if (!t.Contains(",")) { t += ",000"; }

                    if (dw >= 0) { t = "A" + t; }
                    while (t.Length < 15) { t += "0"; }
                    return compareKeySOk + t;
                }

                return compareKeySNok + isValue;

            case SortierTyp.Datum_Uhrzeit:
                return DateTimeTryParse(isValue, out var d) ? compareKeySNok + d.ToString1() : compareKeySNok + isValue;

            default:
                Develop.DebugPrint(sortType);
                return SecondSortChar + isValue;
        }
    }

    public static bool ContainsChars(this string tXt, string chars) => chars.Where((_, z) => tXt.Contains(chars.Substring(z, 1))).Any();

    public static bool ContainsOnlyChars(this string tXt, string chars) => !tXt.Where((_, z) => !chars.Contains(tXt.Substring(z, 1))).Any();

    public static bool ContainsWord(this string input, string value, RegexOptions options) => input.IndexOfWord(value, 0, options) >= 0;

    public static string ConvertFromHtmlToRich(this string txt) {
        if (string.IsNullOrEmpty(txt)) { return string.Empty; }

        // Vorallokierung mit geschätzter Größe (+10% für mögliche Ersetzungen)
        var sb = new StringBuilder(txt.Length + (txt.Length / 10));

        // Einmaliger Durchlauf durch den String
        for (int i = 0; i < txt.Length; i++) {
            if (txt[i] == '\r' && i + 1 < txt.Length && txt[i + 1] == '\n') {
                sb.Append("<br>");
                i++; // Überspringe das \n
            } else if (txt[i] == '<' && i + 1 < txt.Length && txt[i + 1] == '<' && i + 2 < txt.Length && txt[i + 2] == '>') {
                sb.Append("&lt;");
                i += 2; // Überspringe die restlichen Zeichen
            } else if (txt[i] == '<' && i + 1 < txt.Length && txt[i + 1] == '>' && i + 2 < txt.Length && txt[i + 2] == '>') {
                sb.Append("&gt;");
                i += 2; // Überspringe die restlichen Zeichen
            } else {
                sb.Append(txt[i]);
            }
        }

        return sb.ToString();
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

    public static string? Decrypt(this string cipherText, string key) {
        key = key + "!äQsWERadf§$%öü,";
        var keyBytes = new Rfc2898DeriveBytes(key, new byte[8], 1000);

        try {
            var buffer = Convert.FromBase64String(cipherText);

            //key = (key + "!äQsWERadf§$%öü,").Substring(0, 16);

            using var aes = Aes.Create();
            aes.KeySize = 128;
            aes.BlockSize = 128;
            aes.Mode = CipherMode.CBC;
            aes.Padding = PaddingMode.PKCS7;
            aes.Key = keyBytes.GetBytes(aes.KeySize / 8);
            aes.IV = new byte[aes.BlockSize / 8];
            var decryptor = aes.CreateDecryptor(aes.Key, aes.IV);

            using var memoryStream = new MemoryStream(buffer);
            using var cryptoStream = new CryptoStream(memoryStream, decryptor, CryptoStreamMode.Read);
            using var streamReader = new StreamReader(cryptoStream);
            return streamReader.ReadToEnd();
        } catch { return null; }
    }

    public static string? Encrypt(this string plainText, string key) {
        try {
            byte[] array;

            key = key + "!äQsWERadf§$%öü,";
            var keyBytes = new Rfc2898DeriveBytes(key, new byte[8], 1000);

            using (var aes = Aes.Create()) {
                aes.KeySize = 128;
                aes.BlockSize = 128;
                aes.Mode = CipherMode.CBC;
                aes.Padding = PaddingMode.PKCS7;
                aes.Key = keyBytes.GetBytes(aes.KeySize / 8);
                aes.IV = new byte[aes.BlockSize / 8];

                var encryptor = aes.CreateEncryptor(aes.Key, aes.IV);

                using var memoryStream = new MemoryStream();
                using var cryptoStream = new CryptoStream(memoryStream, encryptor, CryptoStreamMode.Write);
                using (var streamWriter = new StreamWriter(cryptoStream)) {
                    streamWriter.Write(plainText);
                }

                array = memoryStream.ToArray();
            }

            return Convert.ToBase64String(array);
        } catch { return null; }
    }

    /// <summary>
    /// Entfernt Leerzeichen und angebebene Textpaare am Anfang/Ende - mit genau einem Zeichen. Z.B. perfekt im Klammern zu entfernen
    /// </summary>
    /// <param name="txt"></param>
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

    public static bool FromPlusMinus(this string value) {
        if (string.IsNullOrEmpty(value)) { return false; }
        switch (value.ToLowerInvariant()) {
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

    /// <summary>
    /// Teilt einen String, der geparsed werden kann in seine Bestandteile auf.
    /// </summary>
    /// <param name="value">Ein String, der mit { beginnt. Z.B. {Wert=100, Wert2=150}</param>
    /// <returns>Gibt immer eine List zurück.</returns>
    public static List<KeyValuePair<string, string>> GetAllTags(this string value) {
        List<KeyValuePair<string, string>> result = [];
        if (string.IsNullOrEmpty(value) || value.Length < 3) { return result; }
        if (value.Substring(0, 1) != "{") { return result; }

        var start = 1;
        var noarunde = true;
        do {
            var (gleichpos, _) = NextText(value, start, Gleich, false, false, KlammernGeschweift);
            if (gleichpos < 0) { Develop.DebugPrint(FehlerArt.Fehler, "Parsen nicht möglich:" + value); }

            var tag = value.Substring(start, gleichpos - start).Trim().ToLowerInvariant();
            tag = tag.Trim(" ");
            tag = tag.Trim(",");
            tag = tag.Trim(" ");
            if (string.IsNullOrEmpty(tag)) { Develop.DebugPrint(FehlerArt.Fehler, "Parsen nicht möglich:" + value); }

            var (kommapos, _) = NextText(value, gleichpos, Komma, false, true, KlammernGeschweift);

            string tagval;
            if (kommapos < 0) {
                tagval = value.Substring(gleichpos + 1, value.Length - gleichpos - 2).Trim();
                noarunde = false;
            } else {
                tagval = value.Substring(gleichpos + 1, kommapos - gleichpos - 1).Trim();

                var test = value.Substring(kommapos);
                if (test is ",}" or ", }" or ", , }" or ",,}" or ", ,}") { noarunde = false; }
            }

            if (tagval.Length > 1 && tagval.StartsWith("\"") && tagval.EndsWith("\"")) {
                tagval = tagval.Substring(1, tagval.Length - 2);
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
            tXt = tXt.Replace("<br>", "\r", RegexOptions.IgnoreCase);
        }
        tXt = tXt.Replace("<", ((char)1020).ToString());
        tXt = tXt.Replace(">", ((char)1021).ToString());
        tXt = tXt.Replace("&lt;", "<");
        tXt = tXt.Replace("&gt;", ">");
        tXt = tXt.Replace("&quot;", "\"");
        return tXt;
    }

    public static int IndexOfWord(this string input, string value, int startIndex, RegexOptions options) {
        if (string.IsNullOrEmpty(input) || string.IsNullOrEmpty(value)) { return -1; }

        // Unicode-Wortgrenzen definieren: Buchstaben und Zahlen gelten als Wortzeichen
        // \p{L} - alle Buchstaben (Letter)
        // \p{N} - alle Ziffern (Number)
        // \p{M} - alle Markierungen/Akzente (Mark)

        string pattern = $@"(?<![\p{{L}}\p{{N}}\p{{M}}]){Regex.Escape(value)}(?![\p{{L}}\p{{N}}\p{{M}}])";

        // IgnoreCase-Option, falls benötigt
        RegexOptions actualOptions = options;
        if ((options & RegexOptions.IgnoreCase) != 0) {
            // CultureInvariant für bessere Handhabung internationaler Zeichen
            actualOptions |= RegexOptions.CultureInvariant;
        }

        var regex = new Regex(pattern, actualOptions);
        var match = regex.Match(input, startIndex);

        return match.Success ? match.Index : -1;
    }

    //public static int IndexOfWord(this string input, string value, int startIndex, RegexOptions options) {
    //    if (options == RegexOptions.IgnoreCase) {
    //        value = value.ToUpperInvariant();
    //        input = " " + input.ToUpperInvariant() + " ";
    //    } else {
    //        input = " " + input + " ";
    //    }

    //    startIndex++;
    //    while (true) {
    //        if (startIndex > input.Length - 1) { return -1; }
    //        startIndex = input.IndexOf(value, startIndex, StringComparison.Ordinal);
    //        if (startIndex < 0) { return -1; }
    //        if (startIndex > 0 && startIndex < input.Length - value.Length) {
    //            if (input[startIndex - 1].IsWordSeparator() && input[startIndex + value.Length].IsWordSeparator()) {
    //                return startIndex - 1; // -1, weil ein Leereichen hinzugefügt wurde.
    //            }
    //            startIndex += value.Length;
    //        }
    //    }
    //}

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

    public static bool IsDateTime(this string? txt) => txt != null && DateTimeTryParse(txt, out _);

    public static bool IsDouble(this string? txt) => txt is not null && DoubleTryParse(txt, out _);

    public static bool IsHtmlColorCode(this string? txt) => txt != null && !string.IsNullOrEmpty(txt) && txt.Length is 6 or 8 && txt.ContainsOnlyChars(Char_Numerals + "abcdefABCDEF");

    public static bool IsLong(this string? txt) => txt is not null && long.TryParse(txt, out _);

    public static bool IsNumeral(this string? txt) => txt is not null && (txt.IsLong() || txt.IsDouble());

    public static string Left(this string value, int length) {
        if (string.IsNullOrEmpty(value) || length <= 0) {
            return string.Empty;
        }

        return (length < value.Length) ? value.Substring(0, length) : value;
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

        List<string> txt = [];
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

    public static string RemoveDuplicateChars(this string input) {
        if (string.IsNullOrEmpty(input)) {
            return string.Empty;
        }

        var result = new StringBuilder();
        char? previousChar = null;

        foreach (var currentChar in input) {
            if (previousChar != currentChar) {
                result.Append(currentChar);
                previousChar = currentChar;
            }
        }

        return result.ToString();
    }

    /// <summary>
    /// Entfernt alle Größer/Kleiner-Zeichen aus dem gegebenen Text.
    /// </summary>
    /// <param name="text"></param>
    /// <returns></returns>
    public static string RemoveXmlTags(this string text) => Regex.Replace(text, "<.*?>", string.Empty);

    public static string Replace(this string txt, string alt, string neu, RegexOptions options) {
        if (options != RegexOptions.IgnoreCase) { Develop.DebugPrint(FehlerArt.Fehler, "Regex option nicht erlaubt."); }
        if (string.IsNullOrEmpty(alt)) { Develop.DebugPrint(FehlerArt.Fehler, "ALT is Empty"); }
        var oldPos = 0;
        while (true) {
            if (string.IsNullOrEmpty(txt)) { return txt; }
            var posx = txt.ToUpperInvariant().IndexOf(alt.ToUpperInvariant(), oldPos, StringComparison.Ordinal);
            if (posx >= 0) {
                txt = txt.Substring(0, posx) + neu + txt.Substring(posx + alt.Length);
                oldPos = posx + neu.Length;
            } else {
                return txt;
            }
        }
    }

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

    public static string Right(this string value, int length) {
        if (string.IsNullOrEmpty(value) || length <= 0) {
            return string.Empty;
        }

        return (length < value.Length) ? value.Substring(value.Length - length) : value;
    }

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

        w = textToSplit.Split([trennzeichen], StringSplitOptions.None);
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

    public static string[] SplitAndCutByCrAndBr(this string textToSplit) {
        textToSplit = textToSplit.Replace("<br>", "\r", RegexOptions.IgnoreCase);
        return textToSplit.SplitAndCutByCr();
    }

    /// <summary>
    /// Erstellt eine Liste aus dem String, wobei als Trenner \r\n, \r oder \n benutzt werden. Leere Einträge am Ende werden gelöscht.
    /// </summary>
    /// <param name="textToSplit"></param>
    /// <returns></returns>
    public static List<string> SplitAndCutByCrToList(this string textToSplit) {
        List<string> w = [];
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
    public static string[] SplitBy(this string textToSplit, string trennzeichen) => string.IsNullOrEmpty(textToSplit) ? [] : textToSplit.Split([trennzeichen], StringSplitOptions.None);

    /// <summary>
    /// Splittet den String, ohne etwas zu kürzen. Zeilenumrüche werden aber vereinfach (\r\n => \r). ACHTUNG: Wenn der Text leer ist, wird ein Array mit der Länge 0 zurückgegeben.
    /// </summary>
    /// <param name="textToSplit"></param>
    /// <returns></returns>
    public static string[] SplitByCr(this string textToSplit) => string.IsNullOrEmpty(textToSplit) ? [] : textToSplit.Replace("\r\n", "\r").Replace("\n", "\r").SplitBy("\r");

    /// <summary>
    /// Splittet den String, ohne etwas zu kürzen. Zeilenumrüche werden aber vereinfach (\r\n => \r). ACHTUNG: Wenn der Text leer ist, wird eine Liste mit der Länge 0 zurückgegeben.
    /// </summary>
    /// <param name="textToSplit"></param>
    /// <returns></returns>
    public static List<string> SplitByCrToList(this string textToSplit) {
        List<string> w = [];
        if (string.IsNullOrEmpty(textToSplit)) { return w; }
        w.AddRange(textToSplit.SplitByCr());
        return w;
    }

    public static string Sprachneutral(this string isValue) {
        isValue = isValue.ToLowerInvariant();
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
    //    return tXT.Substring(BG + 1, IG - BG - 1).ToLowerInvariant();
    //}
    //public static string ParseValue(this string tXT, string offTag, int startIndex) {
    //    if (string.IsNullOrEmpty(tXT)) { return string.Empty; }
    //    if (string.IsNullOrEmpty(offTag)) { return string.Empty; }
    //    var FirstCharAfterEquals = tXT.ToLowerInvariant().IndexOf(offTag.ToLowerInvariant() + "=", startIndex);
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
    /// <summary>
    /// Reduziert den Text auf Kleinbuchstaben und Zahlen. Umlaute zu ae.
    /// additinalAllowed gibgt an welche Zeichen noch erlaubt sein sollen. Z.B. Leerzeichen und ,
    /// Aber auch diese nur einmal und nicht doppelt hinereinandwer
    /// </summary>
    /// <param name="tXt"></param>
    /// <param name="additinalAllowed"></param>
    /// <param name="removedupes"></param>
    /// <returns></returns>
    public static string StarkeVereinfachung(this string tXt, string additinalAllowed, bool removedupes) {
        tXt = tXt.ToLowerInvariant().ReduceToChars(Char_Numerals + Char_Buchstaben + additinalAllowed);
        foreach (var replacement in Replacements) {
            tXt = tXt.Replace(replacement.Key, replacement.Value);
        }

        if (!removedupes) { return tXt; }

        return tXt.RemoveDuplicateChars();
    }

    /// <summary>
    /// Gibt eine Liste der KeyNamen zurück
    /// </summary>
    /// <param name="items"></param>
    /// <returns></returns>
    public static List<string> ToListOfString(this IEnumerable<IHasKeyName?>? items) {
        List<string> w = [];
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
        // Suchwort: #Camelcase
        text = text.ToLowerInvariant().Replace("_", " ");
        var info = CultureInfo.CurrentCulture.TextInfo;
        return info.ToTitleCase(text);
    }

    /// <returns></returns>
    public static string Trim(this string txt, List<List<string>> klammern) {
        var again = true;
        while (again) {
            again = false;
            txt = txt.Trim();

            foreach (var thisKlammern in klammern) {
                if (thisKlammern.Count != 2) { return txt; }
                if (thisKlammern[0].Length != 1) { return txt; }
                if (thisKlammern[1].Length != 1) { return txt; }

                if (txt.CanCut(thisKlammern[0], thisKlammern[1])) {
                    txt = txt.Substring(1, txt.Length - 2);
                    again = true;
                }
            }
        }

        return txt;
    }

    public static string Trim(this string tXt, string was) {
        if (string.IsNullOrEmpty(tXt)) { return string.Empty; }
        tXt = tXt.TrimEnd(was);
        return string.IsNullOrEmpty(tXt) ? string.Empty : tXt.TrimStart(was);
    }

    public static string TrimCr(this string tXt) => string.IsNullOrEmpty(tXt) ? string.Empty : tXt.Trim("\r");

    /// <summary>
    ///  Entfernt den angegebenen Text am Ende des Strings. Wenn mehrfach vorgandenn, wird er mehrfach entfernt.
    ///  Groß/Kleinschreibung wird ignoriert.
    /// </summary>
    /// <param name="tXt"></param>
    /// <param name="was"></param>
    /// <returns></returns>
    public static string TrimEnd(this string tXt, string was) {
        if (string.IsNullOrEmpty(tXt)) { return string.Empty; }
        if (was.Length < 1) { Develop.DebugPrint(FehlerArt.Fehler, "Trimmen nicht möglich mit: " + was); }
        was = was.ToUpperInvariant();
        while (tXt.Length >= was.Length && tXt.Substring(tXt.Length - was.Length).ToUpperInvariant() == was) {
            tXt = tXt.Remove(tXt.Length - was.Length);
        }
        return tXt;
    }

    /// <summary>
    ///  Entfernt den angegebenen Text am Anfang des Strings. Wenn mehrfach vorgandenn, wird er mehrfach entfernt.
    ///  Groß/Kleinschreibung wird ignoriert.
    /// </summary>
    /// <param name="tXt"></param>
    /// <param name="was"></param>
    /// <returns></returns>
    public static string TrimStart(this string tXt, string was) {
        if (string.IsNullOrEmpty(tXt)) { return string.Empty; }
        if (was.Length < 1) { Develop.DebugPrint(FehlerArt.Fehler, "Trimmen nicht möglich mit: " + was); }
        was = was.ToUpperInvariant();
        while (tXt.Length >= was.Length && tXt.Substring(0, was.Length).ToUpperInvariant() == was) {
            tXt = tXt.Remove(0, was.Length);
        }
        return tXt;
    }

    public static byte[] UTF8_ToByte(this string? tXt) {
        if (tXt == null || string.IsNullOrEmpty(tXt)) { return []; }
        return Encoding.UTF8.GetBytes(tXt);
    }

    #endregion
}