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

#nullable enable

using BlueBasics;
using BlueBasics.Enums;
using BlueDatabase.Enums;
using System.Collections.Generic;
using System.Linq;

namespace BlueDatabase.Interfaces;

public interface IInputFormat {

    #region Properties

    public AdditionalCheck AdditionalFormatCheck { get; set; }

    //public enAlignmentHorizontal Align { get; set; }
    public string AllowedChars { get; set; }

    //public int BildCode_ConstantHeight { get; set; }
    //public BildTextVerhalten BildTextVerhalten { get; set; }
    public bool FormatierungErlaubt { get; set; }

    public int MaxTextLenght { get; set; }
    public bool MultiLine { get; set; }
    public string Prefix { get; set; }
    public string Regex { get; set; }

    //public enSortierTyp SortType { get; set; }
    public bool SpellCheckingEnabled { get; set; }

    public string Suffix { get; set; }

    #endregion

    //public enTranslationType Translate { get; set; }
}

public static class IInputFormatExtensions {

    #region Methods

    /// <summary>
    /// Setzt: AllowedChars, Regex, Präfix, Suffix, FormatierungErlaubt, AdditionlCheck, SpellChecking, MaxTextLenght und Multiline
    /// </summary>
    /// <param name="t"></param>
    /// <param name="source"></param>
    public static void GetStyleFrom(this IInputFormat? t, IInputFormat? source) {
        t.AdditionalFormatCheck = source.AdditionalFormatCheck;
        t.AllowedChars = source.AllowedChars;
        t.Prefix = source.Prefix;
        t.Regex = source.Regex;
        t.Suffix = source.Suffix;
        t.MultiLine = source.MultiLine;
        t.MaxTextLenght = source.MaxTextLenght;
        t.SpellCheckingEnabled = source.SpellCheckingEnabled;
        t.FormatierungErlaubt = source.FormatierungErlaubt;
    }

    public static bool IsFormat(this string txt, IInputFormat formatToCheck) {
        var l = new List<string>();

        if (formatToCheck.MultiLine) {
            l.AddRange(txt.SplitAndCutByCr());
        } else {
            l.Add(txt);
        }

        if (txt.Length > formatToCheck.MaxTextLenght) { return false; }

        foreach (var thisString in l.Where(thisString => !string.IsNullOrEmpty(thisString))) {
            if (!string.IsNullOrEmpty(formatToCheck.AllowedChars) && !thisString.ContainsOnlyChars(formatToCheck.AllowedChars)) { return false; }
            if (!string.IsNullOrEmpty(formatToCheck.Regex) && !thisString.RegexMatch(formatToCheck.Regex)) { return false; }

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
        return true;
        //return l.All(thisString => string.IsNullOrEmpty(thisString) || thisString.IsFormat(format, additionalRegex));
    }

    public static bool IsFormatIdentical(this IInputFormat t, IInputFormat source) => t.AdditionalFormatCheck == source.AdditionalFormatCheck &&
        t.AllowedChars == source.AllowedChars &&
        t.Prefix == source.Prefix &&
        t.Regex == source.Regex &&
        t.Suffix == source.Suffix &&
        t.MultiLine == source.MultiLine &&
        t.SpellCheckingEnabled == source.SpellCheckingEnabled &&
        t.FormatierungErlaubt == source.FormatierungErlaubt &&
        t.MaxTextLenght == source.MaxTextLenght;

    /// <summary>
    /// Setzt: AllowedChars, Regex, Präfix, Suffix, FormatierungErlaubt, AdditionlCheck, SpellChecking, MaxTextLenght und Multiline
    /// </summary>
    /// <param name="t"></param>
    /// <param name="type"></param>
    public static void SetFormat(this IInputFormat t, VarType type) {
        switch (type) {
            case VarType.Text:
                t.AllowedChars = string.Empty;
                t.Regex = string.Empty;
                t.Suffix = string.Empty;
                t.Prefix = string.Empty;
                t.FormatierungErlaubt = false;
                t.AdditionalFormatCheck = AdditionalCheck.None;
                t.SpellCheckingEnabled = true;
                t.MultiLine = false;
                t.MaxTextLenght = 4000;
                return;

            case VarType.Bit:
                t.AllowedChars = "+-";
                t.Regex = @"^([+]|[-])$";
                t.Suffix = string.Empty;
                t.Prefix = string.Empty;
                t.FormatierungErlaubt = false;
                t.AdditionalFormatCheck = AdditionalCheck.None;
                t.SpellCheckingEnabled = false;
                t.MultiLine = false;
                t.MaxTextLenght = 1;
                return;

            case VarType.TextMitFormatierung:
                t.AllowedChars = string.Empty;
                t.Regex = string.Empty;
                t.Suffix = string.Empty;
                t.Prefix = string.Empty;
                t.FormatierungErlaubt = true;
                t.AdditionalFormatCheck = AdditionalCheck.None;
                t.SpellCheckingEnabled = true;
                t.MultiLine = true;
                t.MaxTextLenght = 4000;
                return;

            case VarType.Date:
                t.Regex = @"^(0[1-9]|[12][0-9]|3[01])[.](0[1-9]|1[0-2])[.]\d{4}$";
                t.AllowedChars = Constants.Char_Numerals + ".";
                t.Suffix = string.Empty;
                t.Prefix = string.Empty;
                t.FormatierungErlaubt = false;
                t.AdditionalFormatCheck = AdditionalCheck.DateTime;
                t.SpellCheckingEnabled = false;
                t.MultiLine = false;
                t.MaxTextLenght = 10;
                return;

            case VarType.Url:
                //    https://regex101.com/r/S2CbwM/1
                t.Regex = @"^(https:|http:|www\.)\S*$";
                t.AllowedChars = Constants.Char_Numerals + Constants.Char_AZ + Constants.Char_az + ":.-_/";
                t.Suffix = string.Empty;
                t.Prefix = string.Empty;
                t.FormatierungErlaubt = false;
                t.AdditionalFormatCheck = AdditionalCheck.None;
                t.SpellCheckingEnabled = false;
                t.MultiLine = false;
                t.MaxTextLenght = 4000;
                return;

            case VarType.Email:
                //http://emailregex.com/
                t.Regex = @"^[a-z0-9A-Z._-]{1,40}[@][a-z0-9A-Z._-]{1,40}[.][a-zA-Z]{1,3}$";
                t.AllowedChars = Constants.Char_Numerals + Constants.Char_AZ + Constants.Char_az + "@.-_";
                t.Suffix = string.Empty;
                t.Prefix = string.Empty;
                t.FormatierungErlaubt = false;
                t.AdditionalFormatCheck = AdditionalCheck.None;
                t.SpellCheckingEnabled = false;
                t.MultiLine = false;
                t.MaxTextLenght = 4000;
                return;

            case VarType.Float:
                //https://regex101.com/r/onr0NZ/1
                t.Regex = @"(^-?([1-9]\d*)|^0)([.|,]\d*[1-9])?$";
                t.AllowedChars = Constants.Char_Numerals + ",-";
                t.Suffix = string.Empty;
                t.Prefix = string.Empty;
                t.FormatierungErlaubt = false;
                t.AdditionalFormatCheck = AdditionalCheck.Float;
                t.SpellCheckingEnabled = false;
                t.MultiLine = false;
                t.MaxTextLenght = 255;
                return;

            case VarType.FloatPositive:
                //https://regex101.com/r/onr0NZ/1
                t.Regex = @"(^([1-9]\d*)|^0)([.|,]\d*[1-9])?$";
                t.AllowedChars = Constants.Char_Numerals + ",-";
                t.Suffix = string.Empty;
                t.Prefix = string.Empty;
                t.FormatierungErlaubt = false;
                t.AdditionalFormatCheck = AdditionalCheck.Float;
                t.SpellCheckingEnabled = false;
                t.MultiLine = false;
                t.MaxTextLenght = 255;
                return;

            case VarType.Integer:
                t.Regex = @"^((-?[1-9]\d*)|0)$";
                t.AllowedChars = Constants.Char_Numerals + "-";
                t.Suffix = string.Empty;
                t.Prefix = string.Empty;
                t.FormatierungErlaubt = false;
                t.AdditionalFormatCheck = AdditionalCheck.Integer;
                t.SpellCheckingEnabled = false;
                t.MultiLine = false;
                t.MaxTextLenght = 255;
                return;

            case VarType.IntegerPositive:
                t.Regex = @"^(([1-9]\d*)|0)$";
                t.AllowedChars = Constants.Char_Numerals + "-";
                t.Suffix = string.Empty;
                t.Prefix = string.Empty;
                t.FormatierungErlaubt = false;
                t.AdditionalFormatCheck = AdditionalCheck.Integer;
                t.SpellCheckingEnabled = false;
                t.MultiLine = false;
                t.MaxTextLenght = 255;
                return;

            case VarType.PhoneNumber:
                //https://regex101.com/r/OzJr8j/1
                t.Regex = @"^[+][1-9][\s0-9]*[0-9]$";
                t.AllowedChars = Constants.Char_Numerals + "+ ";
                t.Suffix = string.Empty;
                t.Prefix = string.Empty;
                t.FormatierungErlaubt = false;
                t.AdditionalFormatCheck = AdditionalCheck.None;
                t.SpellCheckingEnabled = false;
                t.MultiLine = false;
                t.MaxTextLenght = 255;
                return;

            case VarType.DateTime:
                t.Regex = @"^(0[1-9]|[12][0-9]|3[01])[.](0[1-9]|1[0-2])[.]\d{4}[ ](0[0-9]|1[0-9]|2[0-3]):[0-5][0-9]:[0-5][0-9]$";
                t.AllowedChars = Constants.Char_Numerals + ":. ";
                t.Suffix = string.Empty;
                t.Prefix = string.Empty;
                t.FormatierungErlaubt = false;
                t.AdditionalFormatCheck = AdditionalCheck.DateTime;
                t.SpellCheckingEnabled = false;
                t.MultiLine = false;
                t.MaxTextLenght = 20;
                return;

            default:
                Develop.DebugPrint(t);
                break;
        }
    }

    #endregion
}