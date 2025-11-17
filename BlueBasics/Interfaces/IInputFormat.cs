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

using BlueBasics.Enums;
using System.Collections.Generic;

namespace BlueBasics.Interfaces;

public interface IInputFormat {

    #region Properties

    public AdditionalCheck AdditionalFormatCheck { get; set; }
    public string AllowedChars { get; set; }

    public int MaxTextLength { get; set; }

    public bool MultiLine { get; set; }

    public string RegexCheck { get; set; }

    public bool SpellCheckingEnabled { get; set; }

    public bool TextFormatingAllowed { get; set; }

    #endregion
}

public static class InputFormatExtensions {

    #region Methods

    /// <summary>
    /// Setzt: AllowedChars, RegexCheck, TextFormatingAllowed, AdditionalFormatCheck, SpellCheckingEnabled, MaxTextLength und Multiline
    /// </summary>
    /// <param name="t"></param>
    /// <param name="source"></param>
    public static void GetStyleFrom(this IInputFormat? t, IInputFormat? source) {
        if (source == null || t == null) { return; }

        t.AdditionalFormatCheck = source.AdditionalFormatCheck;
        t.AllowedChars = source.AllowedChars;
        t.RegexCheck = source.RegexCheck;
        t.MultiLine = source.MultiLine;
        t.MaxTextLength = source.MaxTextLength;
        t.SpellCheckingEnabled = source.SpellCheckingEnabled;
        t.TextFormatingAllowed = source.TextFormatingAllowed;
    }

    /// <summary>
    /// Prüft den Text, ob er mit dem geforderten Format (z.B. FormatHolder.Filepath) übereinstimmt
    /// </summary>
    /// <param name="txt"></param>
    /// <param name="formatToCheck">z.B. FormatHolder.Filepath</param>
    /// <returns></returns>
    public static bool IsFormat(this string txt, IInputFormat formatToCheck) {
        var l = new List<string>();

        if (formatToCheck.MultiLine) {
            l.AddRange(txt.SplitAndCutByCr());
        } else {
            l.Add(txt);
        }

        foreach (var thisString in l) {
            if (!string.IsNullOrEmpty(thisString)) {
                if (!string.IsNullOrEmpty(formatToCheck.AllowedChars) && !thisString.ContainsOnlyChars(formatToCheck.AllowedChars)) {
                    return false;
                }

                if (!string.IsNullOrEmpty(formatToCheck.RegexCheck) && !thisString.RegexMatch(formatToCheck.RegexCheck)) {
                    return false;
                }

                if (thisString.Length > formatToCheck.MaxTextLength) { return false; }

                switch (formatToCheck.AdditionalFormatCheck) {
                    case AdditionalCheck.None:
                        break;

                    case AdditionalCheck.Integer:
                        if (!thisString.IsLong()) { return false; }
                        break;

                    case AdditionalCheck.Float:
                        if (!thisString.IsDouble()) { return false; }
                        break;

                    case AdditionalCheck.DateTime:
                        if (!thisString.IsDateTime()) { return false; }
                        break;

                    default:
                        Develop.DebugPrint(formatToCheck.AdditionalFormatCheck);
                        break;
                }
            }
        }

        return true;
    }

    public static bool IsFormatIdentical(this IInputFormat t, IInputFormat source) => t.AdditionalFormatCheck == source.AdditionalFormatCheck &&
        t.AllowedChars == source.AllowedChars &&
        t.RegexCheck == source.RegexCheck &&
        t.MultiLine == source.MultiLine &&
        t.SpellCheckingEnabled == source.SpellCheckingEnabled &&
        t.TextFormatingAllowed == source.TextFormatingAllowed &&
        t.MaxTextLength == source.MaxTextLength;

    /// <summary>
    /// Ignoriert Multiline und wenn MaxTextLength 4000 ist
    /// </summary>
    /// <param name="t"></param>
    /// <param name="source"></param>
    /// <returns></returns>
    public static bool IsFormatIdenticalSoft(this IInputFormat t, IInputFormat source) {
        var x = t.AdditionalFormatCheck == source.AdditionalFormatCheck &&
                   t.AllowedChars == source.AllowedChars &&
                   t.RegexCheck == source.RegexCheck &&
                   t.SpellCheckingEnabled == source.SpellCheckingEnabled &&
                   t.TextFormatingAllowed == source.TextFormatingAllowed;
        if (!x) { return false; }

        if (t.MaxTextLength < 4000) {
            if (t.MaxTextLength != source.MaxTextLength) { return false; }
        }

        return t.MaxTextLength >= 1;
    }

    #endregion
}