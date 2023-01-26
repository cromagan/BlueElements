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

using BlueBasics;
using BlueBasics.Enums;
using BlueDatabase.Enums;
using BlueDatabase.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlueDatabase {

    public class FormatHolder : IInputFormat {

        #region Fields

        public static readonly List<FormatHolder> AllFormats = new();

        public static readonly FormatHolder Bit = new() {
            Name = "Bit",
            AllowedChars = "+-",
            Regex = @"^([+]|[-])$",
            Suffix = string.Empty,
            Prefix = string.Empty,
            FormatierungErlaubt = false,
            AdditionalFormatCheck = AdditionalCheck.None,
            SpellCheckingEnabled = false,
            MultiLine = false,
            MaxTextLenght = 1,
        };

        public static readonly FormatHolder Date = new() {
            Name = "Date",
            Regex = @"^(0[1-9]|[12][0-9]|3[01])[.](0[1-9]|1[0-2])[.]\d{4}$",
            AllowedChars = Constants.Char_Numerals + ".",
            Suffix = string.Empty,
            Prefix = string.Empty,
            FormatierungErlaubt = false,
            AdditionalFormatCheck = AdditionalCheck.DateTime,
            SpellCheckingEnabled = false,
            MultiLine = false,
            MaxTextLenght = 10,
        };

        public static readonly FormatHolder DateTime = new() {
            Name = "DateTime",
            Regex = @"^(0[1-9]|[12][0-9]|3[01])[.](0[1-9]|1[0-2])[.]\d{4}[ ](0[0-9]|1[0-9]|2[0-3]):[0-5][0-9]:[0-5][0-9]$",
            AllowedChars = Constants.Char_Numerals + ":. ",
            Suffix = string.Empty,
            Prefix = string.Empty,
            FormatierungErlaubt = false,
            AdditionalFormatCheck = AdditionalCheck.DateTime,
            SpellCheckingEnabled = false,
            MultiLine = false,
            MaxTextLenght = 19
        };

        public static readonly FormatHolder Email = new() {
            Name = "EMail",
            //http://emailregex.com/
            Regex = @"^[a-z0-9A-Z._-]{1,40}[@][a-z0-9A-Z._-]{1,40}[.][a-zA-Z]{1,3}$",
            AllowedChars = Constants.Char_Numerals + Constants.Char_AZ + Constants.Char_az + "@.-_",
            Suffix = string.Empty,
            Prefix = string.Empty,
            FormatierungErlaubt = false,
            AdditionalFormatCheck = AdditionalCheck.None,
            SpellCheckingEnabled = false,
            MultiLine = false,
            MaxTextLenght = 4000,
        };

        public static readonly FormatHolder FilepathAndName = new() {
            Name = "FilepathAndName",
            //    https://regex101.com/r/S2CbwM/1
            Regex = @"^[A-Za-z]:\\.*[.].*$",
            AllowedChars = Constants.Char_Numerals + Constants.Char_Buchstaben + Constants.Char_Buchstaben.ToUpper() + "\\#:.+-_ ",
            Suffix = string.Empty,
            Prefix = string.Empty,
            FormatierungErlaubt = false,
            AdditionalFormatCheck = AdditionalCheck.None,
            SpellCheckingEnabled = false,
            MultiLine = false,
            MaxTextLenght = 512,
        };

        public static readonly FormatHolder Float = new() {
            Name = "Float",
            //https://regex101.com/r/onr0NZ/1
            Regex = @"(^-?([1-9]\d*)|^0)([.|,]\d*[1-9])?$",
            AllowedChars = Constants.Char_Numerals + ",-",
            Suffix = string.Empty,
            Prefix = string.Empty,
            FormatierungErlaubt = false,
            AdditionalFormatCheck = AdditionalCheck.Float,
            SpellCheckingEnabled = false,
            MultiLine = false,
            MaxTextLenght = 255,
        };

        public static readonly FormatHolder FloatPositive = new() {
            Name = "Float only Positive",
            //https://regex101.com/r/onr0NZ/1
            Regex = @"(^([1-9]\d*)|^0)([.|,]\d*[1-9])?$",
            AllowedChars = Constants.Char_Numerals + ",-",
            Suffix = string.Empty,
            Prefix = string.Empty,
            FormatierungErlaubt = false,
            AdditionalFormatCheck = AdditionalCheck.Float,
            SpellCheckingEnabled = false,
            MultiLine = false,
            MaxTextLenght = 255,
        };

        public static readonly FormatHolder Integer = new() {
            Name = "Integer",
            Regex = @"^((-?[1-9]\d*)|0)$",
            AllowedChars = Constants.Char_Numerals + "-",
            Suffix = string.Empty,
            Prefix = string.Empty,
            FormatierungErlaubt = false,
            AdditionalFormatCheck = AdditionalCheck.Integer,
            SpellCheckingEnabled = false,
            MultiLine = false,
            MaxTextLenght = 255,
        };

        public static readonly FormatHolder IntegerPositive = new() {
            Name = "Integer only Positive",
            Regex = @"^(([1-9]\d*)|0)$",
            AllowedChars = Constants.Char_Numerals + "-",
            Suffix = string.Empty,
            Prefix = string.Empty,
            FormatierungErlaubt = false,
            AdditionalFormatCheck = AdditionalCheck.Integer,
            SpellCheckingEnabled = false,
            MultiLine = false,
            MaxTextLenght = 255,
        };

        public static readonly FormatHolder PhoneNumber = new() {
            Name = "PhoneNumber",
            //https://regex101.com/r/OzJr8j/1
            Regex = @"^[+][1-9][\s0-9]*[0-9]$",
            AllowedChars = Constants.Char_Numerals + "+ ",
            Suffix = string.Empty,
            Prefix = string.Empty,
            FormatierungErlaubt = false,
            AdditionalFormatCheck = AdditionalCheck.None,
            SpellCheckingEnabled = false,
            MultiLine = false,
            MaxTextLenght = 255
        };

        public static readonly FormatHolder Text = new() {
            Name = "Text",
            AllowedChars = string.Empty,
            Regex = string.Empty,
            Suffix = string.Empty,
            Prefix = string.Empty,
            FormatierungErlaubt = false,
            AdditionalFormatCheck = AdditionalCheck.None,
            SpellCheckingEnabled = true,
            MultiLine = false,
            MaxTextLenght = 4000
        };

        public static readonly FormatHolder TextMitFormatierung = new() {
            Name = "Text with format",
            AllowedChars = string.Empty,
            Regex = string.Empty,
            Suffix = string.Empty,
            Prefix = string.Empty,
            FormatierungErlaubt = true,
            AdditionalFormatCheck = AdditionalCheck.None,
            SpellCheckingEnabled = true,
            MultiLine = true,
            MaxTextLenght = 4000,
        };

        public static readonly FormatHolder Url = new() {
            Name = "Url",
            //    https://regex101.com/r/S2CbwM/1
            Regex = @"^(https:|http:|www\.)\S*$",
            AllowedChars = Constants.Char_Numerals + Constants.Char_AZ + Constants.Char_az + ":{.-_/",
            Suffix = string.Empty,
            Prefix = string.Empty,
            FormatierungErlaubt = false,
            AdditionalFormatCheck = AdditionalCheck.None,
            SpellCheckingEnabled = false,
            MultiLine = false,
            MaxTextLenght = 4000,
        };

        #endregion

        #region Constructors

        public FormatHolder() {
            AllFormats.Add(this);
        }

        #endregion

        #region Properties

        public AdditionalCheck AdditionalFormatCheck { get; set; } = AdditionalCheck.None;
        public string AllowedChars { get; set; } = string.Empty;
        public bool FormatierungErlaubt { get; set; } = false;
        public int MaxTextLenght { get; set; } = 0;
        public bool MultiLine { get; set; } = false;
        public string Name { get; set; } = string.Empty;
        public string Prefix { get; set; } = string.Empty;
        public string Regex { get; set; } = string.Empty;
        public bool SpellCheckingEnabled { get; set; } = false;
        public string Suffix { get; set; } = string.Empty;

        #endregion
    }
}