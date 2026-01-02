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

using BlueBasics.Enums;
using BlueBasics.Interfaces;
using System;
using System.Collections.Generic;
using System.Globalization;

using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
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
                        ? compareKeySOk + "A" + w.ToString10()
                        : compareKeySOk + w.ToString10();
                }

                return compareKeySNok + isValue;

            case SortierTyp.Original_String:
                return SecondSortChar + isValue;

            case SortierTyp.Sprachneutral_String:

                if (string.IsNullOrEmpty(isValue)) { return string.Empty; }

                return SecondSortChar + isValue.Sprachneutral();

            case SortierTyp.ZahlenwertFloat:
                if (DoubleTryParse(isValue, out var dw)) {
                    var t = dw.ToString10_3();
                    if (!t.Contains(",")) { t += ",000"; }

                    if (dw < 0) {
                        // Transformiere negative Werte, damit größere Beträge (stärker negativ) lexikographisch zuerst kommen.
                        // Vorgehen: Ziffern (ohne Komma) 9-komplementieren.
                        // t Format: -dddddddddd,ddd (Länge 15), bereits 10 Stellen vor Komma + 3 Nachkommastellen.
                        if (t.Length >= 15 && t[0] == '-') {
                            var core = t.Substring(1); // 14 Zeichen: 10 Ziffern + ',' + 3 Ziffern
                            var chars = core.ToCharArray();
                            for (var i = 0; i < chars.Length; i++) {
                                var c = chars[i];
                                if (c is >= '0' and <= '9') { chars[i] = (char)('0' + ('9' - c)); }
                            }
                            core = new string(chars);
                            t = "-" + core; // Länge bleibt 15
                        }
                    } else {
                        t = "A" + t; // positives Kennzeichen
                    }

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

    public static bool ContainsIgnoreCase(this string source, string toCheck) => source?.IndexOf(toCheck, StringComparison.OrdinalIgnoreCase) >= 0;

    public static bool ContainsOnlyChars(this string tXt, string chars) => !tXt.Where((_, z) => !chars.Contains(tXt.Substring(z, 1))).Any();

    public static bool ContainsWord(this string input, string value, RegexOptions options) => input.IndexOfWord(value, 0, options) >= 0;

    public static string ConvertFromHtmlToRich(this string txt) {
        if (string.IsNullOrEmpty(txt)) { return string.Empty; }

        // Vorallokierung mit geschätzter Größe (+10% für mögliche Ersetzungen)
        var sb = new StringBuilder(txt.Length + (txt.Length / 10));

        // Einmaliger Durchlauf durch den String
        for (var i = 0; i < txt.Length; i++) {
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

    /// <summary>
    /// Zählt, wie oft ein einzelnes Zeichen bis zu einer bestimmten Position vorkommt
    /// </summary>
    public static int CountChar(this string text, char character, int? maxPosition = null) {
        if (string.IsNullOrEmpty(text)) { return 0; }

        var endPos = maxPosition.HasValue ? Math.Min(maxPosition.Value, text.Length) : text.Length;
        if (endPos <= 0) {
            return 0;
        }

        var count = 0;
        for (var i = 0; i < endPos; i++) {
            if (text[i] == character) {
                count++;
            }
        }

        return count;
    }

    /// <summary>
    /// Zählt, wie oft ein String-Wert im gesamten Text vorkommt
    /// </summary>
    public static int CountString(this string text, string value) {
        if (string.IsNullOrEmpty(text) || string.IsNullOrEmpty(value)) {
            return 0;
        }

        var count = 0;
        var index = 0;

        while ((index = text.IndexOf(value, index, StringComparison.Ordinal)) != -1) {
            count++;
            index += value.Length;
        }

        return count;
    }

    /// <summary>
    /// Primitive Methode. Erswetzt nur Umlaute und \r
    /// Besser: Siehe MethodStringAsciiToHTML
    /// </summary>
    /// <param name="text"></param>
    /// <returns></returns>
    public static string CreateHtmlCodes(this string text) {
        if (string.IsNullOrEmpty(text)) { return string.Empty; }

        var result = new StringBuilder(text.Length * 2); // Geschätzte Größe für HTML-Entitäten

        for (var i = 0; i < text.Length; i++) {
            var currentChar = text[i];

            // Handle \r\n first (multi-character sequence)
            if (currentChar == '\r' && i + 1 < text.Length && text[i + 1] == '\n') {
                result.Append("<br>");
                i++; // Skip the \n
                continue;
            }

            // Handle single characters
            if (HtmlEntities.TryGetValue(currentChar, out var entity)) {
                result.Append(entity);
            } else {
                result.Append(currentChar);
            }
        }

        return result.ToString();
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
        key += "!äQsWERadf§$%öü,";
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

            using var memoryStream = new System.IO.MemoryStream(buffer);
            using var cryptoStream = new CryptoStream(memoryStream, decryptor, CryptoStreamMode.Read);
            using var streamReader = new System.IO.StreamReader(cryptoStream);
            return streamReader.ReadToEnd();
        } catch { return null; }
    }

    public static string? Encrypt(this string plainText, string key) {
        try {
            byte[] array;

            key += "!äQsWERadf§$%öü,";
            var keyBytes = new Rfc2898DeriveBytes(key, new byte[8], 1000);

            using (var aes = Aes.Create()) {
                aes.KeySize = 128;
                aes.BlockSize = 128;
                aes.Mode = CipherMode.CBC;
                aes.Padding = PaddingMode.PKCS7;
                aes.Key = keyBytes.GetBytes(aes.KeySize / 8);
                aes.IV = new byte[aes.BlockSize / 8];

                var encryptor = aes.CreateEncryptor(aes.Key, aes.IV);

                using var memoryStream = new System.IO.MemoryStream();
                using var cryptoStream = new CryptoStream(memoryStream, encryptor, CryptoStreamMode.Write);
                using (var streamWriter = new System.IO.StreamWriter(cryptoStream)) {
                    streamWriter.Write(plainText);
                }

                array = memoryStream.ToArray();
            }

            return Convert.ToBase64String(array);
        } catch { return null; }
    }

    public static string EscapeUnicode(this string input) {
        if (string.IsNullOrEmpty(input)) { return input; }

        try {
            var result = new StringBuilder();
            foreach (var c in input) {
                if (c > 127) { // Nicht-ASCII Zeichen
                    result.Append($"\\u{(int)c:X4}");
                } else {
                    result.Append(c);
                }
            }
            return result.ToString();
        } catch (Exception ex) {
            Develop.DebugPrint("Fehler beim Konvertieren", ex);
            return input;
        }
    }

    public static string FromNonCritical(this string txt) {
        if (string.IsNullOrEmpty(txt)) { return string.Empty; }
        if (txt.Length < 3) { return txt; }

        // Quick check - wenn keine codierten Zeichen vorhanden, direkt zurückgeben
        if (!txt.Contains("[")) { return txt; }

        var result = new List<char>(txt.Length);

        for (var i = 0; i < txt.Length; i++) {
            // Prüfe auf Pattern [X]
            if (i <= txt.Length - 3 && txt[i] == '[' && txt[i + 2] == ']') {
                var patternChar = txt[i + 1];

                switch (patternChar) {
                    case 'A':
                        result.Add(';');
                        break;

                    case 'B':
                        result.Add('<');
                        break;

                    case 'C':
                        result.Add('>');
                        break;

                    case 'D':
                        result.Add('\r');
                        result.Add('\n');
                        break;

                    case 'E':
                        result.Add('\r');
                        break;

                    case 'F':
                        result.Add('\n');
                        break;

                    case 'G':
                        result.Add('|');
                        break;

                    case 'H':
                        result.Add('}');
                        break;

                    case 'I':
                        result.Add('{');
                        break;

                    case 'J':
                        result.Add('=');
                        break;

                    case 'K':
                        result.Add(',');
                        break;

                    case 'L':
                        result.Add('&');
                        break;

                    case 'M':
                        result.Add('/');
                        break;

                    case 'N':
                        result.Add('"');
                        break;

                    case 'Z':
                        result.Add('[');
                        break;

                    default:
                        // Kein bekanntes Pattern, original Zeichen beibehalten
                        result.Add(txt[i]);
                        continue; // i wird nicht um 2 erhöht
                }

                i += 2; // Skip die nächsten 2 Zeichen (X])
            } else {
                result.Add(txt[i]);
            }
        }

        return new string([.. result]);
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
                Develop.DebugPrint(ErrorType.Warning, "'" + value + "' unbekannt!");
                return false;
        }
    }

    /// <summary>
    /// Teilt einen String, der geparsed werden kann in seine Bestandteile auf.
    /// </summary>
    /// <param name="value">Ein String, der mit { beginnt. Z.B. {Wert=100, Wert2=150}</param>
    /// <returns>Gibt immer eine List zurück.</returns>
    public static List<KeyValuePair<string, string>>? GetAllTags(this string value) {
        if (string.IsNullOrEmpty(value) || value.Length < 2) { return null; }
        if (value.Substring(0, 1) != "{") { return null; }
        if (value.Substring(value.Length - 1, 1) != "}") { return null; }

        List<KeyValuePair<string, string>> result = [];

        if (value == "{}") { return result; }

        var start = 1;
        var noarunde = true;
        do {
            var (gleichpos, _) = NextText(value, start, Gleich, false, false, KlammernGeschweift);
            if (gleichpos < 0) { Develop.DebugPrint(ErrorType.Error, "Parsen nicht möglich:" + value); }

            var tag = value.Substring(start, gleichpos - start).Trim().ToLowerInvariant();
            tag = tag.Trim(" ");
            tag = tag.Trim(",");
            tag = tag.Trim(" ");
            if (string.IsNullOrEmpty(tag)) {
                Develop.DebugPrint(ErrorType.Warning, "Parsen nicht möglich:" + value);
                return null;
            }

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

    public static int IndexOfWord(this string input, string value, int startIndex, RegexOptions options) {
        if (string.IsNullOrEmpty(input) || string.IsNullOrEmpty(value)) { return -1; }

        // Unicode-Wortgrenzen definieren: Buchstaben und Zahlen gelten als Wortzeichen
        // \p{L} - alle Buchstaben (Letter)
        // \p{N} - alle Ziffern (Number)
        // \p{M} - alle Markierungen/Akzente (Mark)

        var pattern = $@"(?<![\p{{L}}\p{{N}}\p{{M}}]){Regex.Escape(value)}(?![\p{{L}}\p{{N}}\p{{M}}])";

        // IgnoreCase-Option, falls benötigt
        var actualOptions = options;
        if ((options & RegexOptions.IgnoreCase) != 0) {
            // CultureInvariant für bessere Handhabung internationaler Zeichen
            actualOptions |= RegexOptions.CultureInvariant;
        }

        var regex = new Regex(pattern, actualOptions);
        var match = regex.Match(input, startIndex);

        return match.Success ? match.Index : -1;
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

    public static bool IsDateTime(this string? txt) => txt != null && DateTimeTryParse(txt, out _);

    public static bool IsDouble(this string? txt) => txt is not null && DoubleTryParse(txt, out _);

    public static bool IsLong(this string? txt) => txt is not null && long.TryParse(txt, out _);

    public static bool IsNumeral(this string? txt) => txt is not null && (txt.IsLong() || txt.IsDouble());

    public static string Left(this string value, int length) {
        if (string.IsNullOrEmpty(value) || length <= 0) {
            return string.Empty;
        }

        return length < value.Length ? value.Substring(0, length) : value;
    }

    public static (int pos, string which) NextText(string txt, int startpos, List<string> searchfor, bool checkforSeparatorbefore, bool checkforSeparatorafter, List<List<string>>? klammern) {
        var gans = false;
        var pos = startpos;
        var maxl = txt.Length;
        const string tr = "&.,;\\?!\" ~|=<>+-(){}[]/*`´^\r\n\t¶";

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

    public static (string normalizedText, string error) NormalizedText(this string txt, bool convertToUpperCase, bool supportDoubleQuotes, bool supportSingleQuotes, bool removesallpace, char lineBreakReplacement) {
        if (string.IsNullOrEmpty(txt)) { return (string.Empty, string.Empty); }

        var result = new StringBuilder();
        var inQuotes = false;
        var quoteChar = '\0';
        var comment = false;

        for (var pos = 0; pos < txt.Length; pos++) {
            var currentChar = txt[pos];
            var addChar = true;

            if (inQuotes) {
                // Innerhalb von Anführungszeichen
                if (currentChar == quoteChar) {
                    // Ende von Anführungszeichen
                    inQuotes = false;
                } else if (currentChar == '\r') {
                    // Zeilenumbruch innerhalb von Anführungszeichen ist ein Fehler
                    var errorText = result.ToString();
                    var lineNumber = errorText.CountChar('¶', pos) + 1;
                    return (errorText, $"Fehler mit Anführungszeichen in Zeile {lineNumber}");
                }
            } else {
                // Außerhalb von Anführungszeichen
                if ((supportDoubleQuotes && currentChar == '"') || (supportSingleQuotes && currentChar == '\'')) {
                    // Start von Anführungszeichen
                    if (!comment) {
                        inQuotes = true;
                        quoteChar = currentChar;
                    }
                } else if (currentChar == '/' && pos < txt.Length - 1 && txt[pos + 1] == '/') {
                    // Kommentar beginnt
                    comment = true;
                } else if (currentChar == '\r') {
                    // Zeilenumbruch

                    result.Append(lineBreakReplacement);

                    comment = false;
                    addChar = false;
                } else if (currentChar == '\n') {
                    // \n ignorieren wenn lineBreakReplacement gesetzt

                    addChar = false;
                } else if (currentChar is ' ' or '\t') {
                    // Leerzeichen und Tabs normalisieren, mehrfache Leerzeichen vermeiden

                    if (!removesallpace) {
                        if (result.Length > 0 &&
                          result[result.Length - 1] != ' ' &&
                          result[result.Length - 1] != lineBreakReplacement) {
                            result.Append(' ');
                        }
                    }
                    addChar = false;
                }
            }
            if (!comment && addChar) {
                var charToAdd = convertToUpperCase && !inQuotes ? char.ToUpperInvariant(currentChar) : currentChar;
                result.Append(charToAdd);
            }
        }

        if (inQuotes) {
            var errorText = result.ToString();
            var lineNumber = errorText.CountChar('¶', txt.Length - 1) + 1;
            return (errorText, $"Nicht geschlossene Anführungszeichen in Zeile {lineNumber}");
        }

        return (result.ToString().Trim(), string.Empty);
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
        if (search.CountChar('*') != 1) { return null; }
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

    public static bool RegexMatch(this string txt, string regex) {
        try {
            return string.IsNullOrEmpty(regex) || new Regex(regex).IsMatch(txt);
        } catch {
            return false;
        }
    }

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
        if (string.IsNullOrEmpty(txt) || string.IsNullOrEmpty(alt)) { return txt; }
        return Regex.Replace(txt, Regex.Escape(alt), neu, options);
    }

    public static string ReplaceWord(this string input, string alt, string replacement, RegexOptions options) {
        if (string.IsNullOrEmpty(input) || string.IsNullOrEmpty(alt)) { return input; }

        // Exakt dasselbe Pattern wie in IndexOfWord
        var pattern = $@"(?<![\p{{L}}\p{{N}}\p{{M}}]){Regex.Escape(alt)}(?![\p{{L}}\p{{N}}\p{{M}}])";

        //var actualOptions = options;
        //if ((options & RegexOptions.IgnoreCase) != 0) {
        //    actualOptions |= RegexOptions.CultureInvariant;
        //}

        return Regex.Replace(input, pattern, replacement, options);
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

        return length < value.Length ? value.Substring(value.Length - length) : value;
    }

    /// <summary>
    /// Trennt den Text mittels dem Trennzeichen. Sind ein oder mehrere Trennzeichen am Ende, werden die leeren Felder nicht zurückgegeben.
    /// </summary>
    /// <param name="textToSplit"></param>
    /// <param name="trennzeichen"></param>
    /// <returns></returns>
    public static string[] SplitAndCutBy(this string textToSplit, string trennzeichen) {
        if (string.IsNullOrEmpty(textToSplit)) { return []; }

        var parts = textToSplit.Split([trennzeichen], StringSplitOptions.None);

        // Von hinten nach vorne die leeren Einträge entfernen
        var lastNonEmpty = parts.Length - 1;
        while (lastNonEmpty >= 0 && string.IsNullOrEmpty(parts[lastNonEmpty])) {
            lastNonEmpty--;
        }

        if (lastNonEmpty < 0) { return []; }

        if (lastNonEmpty == parts.Length - 1) { return parts; }// Keine leeren Einträge am Ende

        // Array mit der richtiger Größe erstellen
        var result = new string[lastNonEmpty + 1];
        Array.Copy(parts, result, lastNonEmpty + 1);
        return result;
    }

    /// <summary>
    /// Erstellt eine Liste aus dem String, wobei als Trenner \r\n, \r oder \n benutzt werden. Leere Einträge am Ende werden gelöscht.
    /// </summary>
    /// <param name="textToSplit"></param>
    /// <returns></returns>
    public static string[] SplitAndCutByCr(this string textToSplit) {
        if (string.IsNullOrEmpty(textToSplit)) { return []; }
        textToSplit = textToSplit.Replace("\r\n", "\r").Replace("\n", "\r");
        return textToSplit.SplitAndCutBy("\r");
    }

    public static string[] SplitAndCutByCrAndBr(this string textToSplit) {
        textToSplit = textToSplit.Replace("<br>", "\r", RegexOptions.IgnoreCase);
        return textToSplit.SplitAndCutByCr();
    }

    /// <summary>
    /// Splittet den String, ohne etwas zu kürzen.
    /// </summary>
    /// <param name="textToSplit"></param>
    /// <param name="trennzeichen"></param>
    /// <returns></returns>
    public static string[] SplitBy(this string textToSplit, string trennzeichen) => textToSplit.Split([trennzeichen], StringSplitOptions.None);

    /// <summary>
    /// Splittet den String, ohne etwas zu kürzen. Zeilenumrüche werden aber vereinfach (\r\n => \r).
    /// </summary>
    /// <param name="textToSplit"></param>
    /// <returns></returns>
    public static string[] SplitByCr(this string textToSplit) => textToSplit.Replace("\r\n", "\r").Replace("\n", "\r").SplitBy("\r");

    public static string Sprachneutral(this string isValue) {
        if (string.IsNullOrEmpty(isValue)) { return string.Empty; }

        var result = new char[isValue.Length];
        var writeIndex = 0;

        // Normalisierung in einem Durchgang
        for (var i = 0; i < isValue.Length; i++) {
            var c = char.ToLowerInvariant(isValue[i]);

            result[writeIndex++] = c switch {
                'ä' or 'á' or 'à' or 'â' => 'a',
                'ö' or 'ó' or 'ò' or 'ô' => 'o',
                'ü' or 'ú' or 'ù' or 'û' => 'u',
                'í' or 'ì' or 'î' => 'i',
                'é' or 'è' or 'ê' => 'e',
                'ž' => 'z',
                'ß' => 's',
                _ => c
            };
        }

        // Trimming vom Anfang
        var startIndex = 0;
        while (startIndex < writeIndex) {
            var c = result[startIndex];
            if (c is '"' or '\'' or ' ') {
                startIndex++;
            } else {
                break;
            }
        }

        return startIndex >= writeIndex ? string.Empty : new string(result, startIndex, writeIndex - startIndex);
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
    //                        Develop.DebugPrint(ErrorType.Warning, "Unerlaubtes Zeichen nach Komma: " + tXT);
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
    //    if (OpenBraketCount != 0) { Develop.DebugPrint(ErrorType.Error, "Parse Fehler: " + tXT); }
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

        return !removedupes ? tXt : tXt.RemoveDuplicateChars();
    }

    /// <summary>
    /// Gibt eine Liste der KeyNamen zurück
    /// </summary>
    /// <param name="items"></param>
    /// <returns></returns>
    public static List<string> ToListOfString(this IEnumerable<IHasKeyName?>? items) {
        List<string> w = [];
        if (items?.Any() != true) { return w; }

        w.AddRange(from thisItem in items where thisItem != null where !string.IsNullOrEmpty(thisItem.KeyName) select thisItem.KeyName);
        return w;
    }

    public static string ToNonCritical(this string txt) {
        if (string.IsNullOrEmpty(txt)) {
            return string.Empty;
        }

        var result = new List<char>(txt.Length * 2);

        for (var i = 0; i < txt.Length; i++) {
            var c = txt[i];

            // Handle \r\n first (multi-character)
            if (c == '\r' && i < txt.Length - 1 && txt[i + 1] == '\n') {
                result.AddRange("[D]");
                i++; // Skip the \n
                continue;
            }

            // Handle single characters
            switch (c) {
                case '[':
                    result.AddRange("[Z]");
                    break;

                case ';':
                    result.AddRange("[A]");
                    break;

                case '<':
                    result.AddRange("[B]");
                    break;

                case '>':
                    result.AddRange("[C]");
                    break;

                case '\r':
                    result.AddRange("[E]");
                    break;

                case '\n':
                    result.AddRange("[F]");
                    break;

                case '|':
                    result.AddRange("[G]");
                    break;

                case '}':
                    result.AddRange("[H]");
                    break;

                case '{':
                    result.AddRange("[I]");
                    break;

                case '=':
                    result.AddRange("[J]");
                    break;

                case ',':
                    result.AddRange("[K]");
                    break;

                case '&':
                    result.AddRange("[L]");
                    break;

                case '/':
                    result.AddRange("[M]");
                    break;

                case '"':
                    result.AddRange("[N]");
                    break;

                default:
                    result.Add(c);
                    break;
            }
        }

        return new string([.. result]);
    }

    public static string ToTitleCase(this string text) {
        // Suchwort: #Camelcase
        text = text.ToLowerInvariant().Replace("_", " ");
        var info = CultureInfo.CurrentCulture.TextInfo;
        return info.ToTitleCase(text);
    }

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
        if (was.Length < 1) { Develop.DebugPrint(ErrorType.Error, "Trimmen nicht möglich mit: " + was); }
        was = was.ToUpperInvariant();
        while (tXt.Length >= was.Length && tXt.Substring(tXt.Length - was.Length).Equals(was, StringComparison.OrdinalIgnoreCase)) {
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
        if (was.Length < 1) { Develop.DebugPrint(ErrorType.Error, "Trimmen nicht möglich mit: " + was); }
        was = was.ToUpperInvariant();
        while (tXt.Length >= was.Length && tXt.Substring(0, was.Length).Equals(was, StringComparison.OrdinalIgnoreCase)) {
            tXt = tXt.Remove(0, was.Length);
        }
        return tXt;
    }

    public static string UnEscapeUnicode(this string input) {
        if (string.IsNullOrEmpty(input)) { return input; }

        try {
            var result = new StringBuilder();
            for (var i = 0; i < input.Length; i++) {
                if (i <= input.Length - 6 && input[i] == '\\' && input[i + 1] == 'u') {
                    var hexCode = input.Substring(i + 2, 4);
                    if (int.TryParse(hexCode, NumberStyles.HexNumber, null, out var unicodeValue)) {
                        result.Append((char)unicodeValue);
                        i += 5; // Überspringe die nächsten 5 Zeichen (\u + 4 Hex-Zeichen)
                    } else {
                        result.Append(input[i]);
                    }
                } else {
                    result.Append(input[i]);
                }
            }
            return result.ToString();
        } catch (Exception ex) {
            Develop.DebugPrint("Fehler beim Rückkonvertieren", ex);
            return input;
        }
    }

    public static byte[] UTF8_ToByte(this string? tXt) => string.IsNullOrEmpty(tXt) ? [] : Encoding.UTF8.GetBytes(tXt);

    #endregion
}