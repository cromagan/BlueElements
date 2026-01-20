// Authors:
// Christian Peter
//
// Copyright © 2026 Christian Peter
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
using System.Collections.Generic;

namespace BlueBasics;

public class FormatHolder : IInputFormat, IReadableTextWithKey {

    #region Fields

    public static readonly List<FormatHolder> AllFormats = [];

    public static readonly FormatHolder Bit = new("Bit", QuickImage.Get(ImageCode.Häkchen, 16)) {
        AllowedChars = "+-",
        RegexCheck = "^([+]|[-])$",
        TextFormatingAllowed = false,
        AdditionalFormatCheck = AdditionalCheck.None,
        SpellCheckingEnabled = false,
        MultiLine = false,
        MaxTextLength = 1,
        QuickInfo = "Ja/Nein Werte"
    };

    public static readonly FormatHolder Color = new("Color", QuickImage.Get(ImageCode.Farbrad, 16)) {
        RegexCheck = @"^#([0-9a-f]{6}|[0-9a-f]{8})$",
        AllowedChars = Constants.Char_Numerals + "#abcdef",
        TextFormatingAllowed = false,
        AdditionalFormatCheck = AdditionalCheck.None,
        SpellCheckingEnabled = false,
        MultiLine = false,
        MaxTextLength = 9,
        QuickInfo = "Farbcode im Hex-Format. Beispiel: #aa0000"
    };

    public static readonly FormatHolder Date = new("Date", QuickImage.Get(ImageCode.Uhr, 16)) {
        RegexCheck = @"^(0[1-9]|[12][0-9]|3[01])[.](0[1-9]|1[0-2])[.]\d{4}$",
        AllowedChars = Constants.Char_Numerals + ".",
        TextFormatingAllowed = false,
        AdditionalFormatCheck = AdditionalCheck.DateTime,
        SpellCheckingEnabled = false,
        MultiLine = false,
        MaxTextLength = 10,
        QuickInfo = "Deutsches Datum. Beispiel: 31.12.2000"
    };

    public static readonly FormatHolder DateTime = new("DateTime", QuickImage.Get(ImageCode.Uhr, 16)) {
        RegexCheck = @"^(0[1-9]|[12][0-9]|3[01])[.](0[1-9]|1[0-2])[.]\d{4}[ ](0[0-9]|1[0-9]|2[0-3]):[0-5][0-9]:[0-5][0-9]$",
        AllowedChars = Constants.Char_Numerals + ":. ",
        TextFormatingAllowed = false,
        AdditionalFormatCheck = AdditionalCheck.DateTime,
        SpellCheckingEnabled = false,
        MultiLine = false,
        MaxTextLength = 19,
        QuickInfo = "Deutsches Datum und Uhrzeit. Beispiel: 31.12.2000 12:34:00"
    };

    public static readonly FormatHolder DateTimeWithMilliSeconds = new("DateTimeWithMilliSeconds", QuickImage.Get(ImageCode.Uhr, 16)) {
        RegexCheck = @"^(0[1-9]|[12][0-9]|3[01])[.](0[1-9]|1[0-2])[.]\d{4}[ ](0[0-9]|1[0-9]|2[0-3]):[0-5][0-9]:[0-5][0-9][.][0-9][0-9][0-9]$",
        AllowedChars = Constants.Char_Numerals + ":. ",
        TextFormatingAllowed = false,
        AdditionalFormatCheck = AdditionalCheck.DateTime,
        SpellCheckingEnabled = false,
        MultiLine = false,
        MaxTextLength = 23,
        QuickInfo = "Deutsches Datum und Uhrzeit mit Millisecunden. Beispiel: 31.12.2000 12:34:00.123"
    };

    public static readonly FormatHolder Email = new("EMail", QuickImage.Get(ImageCode.Brief, 16)) {
        //https://en.wikipedia.org/wiki/Email_address#:~:text=The%20format%20of%20an%20email,a%20maximum%20of%20255%20octets.
        //http://emailregex.com/
        RegexCheck = "^[a-z0-9A-Z._-]{1,63}[@][a-z0-9A-Z.-]{1,63}[.][a-zA-Z.]{1,63}$",
        AllowedChars = Constants.Char_Numerals + Constants.Char_AZ + Constants.Char_az + "@.-_",
        TextFormatingAllowed = false,
        AdditionalFormatCheck = AdditionalCheck.None,
        SpellCheckingEnabled = false,
        MultiLine = false,
        MaxTextLength = 320
    };

    public static readonly FormatHolder Filepath = new("Filepath", QuickImage.Get(ImageCode.Ordner, 16)) {
        // https://regex101.com/r/xuJ7gR/1
        RegexCheck = @"^([A-Za-z]:|\\\\[^\\\/:*?""<>|\r\n]+\\[^\\\/:*?""<>|\r\n]+)\\(?:[^\\\/:*?""<>|\r\n]+\\)*$|^[A-Za-z]:\\$",
        AllowedChars = Constants.Char_Numerals + Constants.Char_Buchstaben + Constants.Char_Buchstaben.ToUpperInvariant() + "\\!$&'@^%()[]{}!&#°`:;.,=+-_ ",
        TextFormatingAllowed = false,
        AdditionalFormatCheck = AdditionalCheck.None,
        SpellCheckingEnabled = false,
        MultiLine = false,
        MaxTextLength = 512
    };

    public static readonly FormatHolder FilepathAndName = new("FilepathAndName", QuickImage.Get(ImageCode.Ordner, 16)) {
        // https://regex101.com/r/5f7WVt/1
        RegexCheck = @"^([A-Za-z]:|\\\\[^\\\/:*?""<>|\r\n]+)\\(?:[^\\\/:*?""<>|\r\n]+\\)*[^\\\/:*?""<>|\r\n]+$",
        AllowedChars = Constants.Char_Numerals + Constants.Char_Buchstaben + Constants.Char_Buchstaben.ToUpperInvariant() + "\\!$&'@^%()[]{}!&#°`:;.,=+-_ ",
        TextFormatingAllowed = false,
        AdditionalFormatCheck = AdditionalCheck.None,
        SpellCheckingEnabled = false,
        MultiLine = false,
        MaxTextLength = 512
    };

    public static readonly FormatHolder Float = new("Float", QuickImage.Get(ImageCode.Gleitkommazahl, 16)) {
        //https://regex101.com/r/onr0NZ/1
        RegexCheck = @"(^-?([1-9]\d*)|^0)([.|,]\d*[1-9])?$",
        AllowedChars = Constants.Char_Numerals + ".,-",
        TextFormatingAllowed = false,
        AdditionalFormatCheck = AdditionalCheck.Float,
        SpellCheckingEnabled = false,
        MultiLine = false,
        MaxTextLength = 255
    };

    public static readonly FormatHolder FloatPositive = new("Float only Positive", QuickImage.Get(ImageCode.Gleitkommazahl, 16)) {
        //https://regex101.com/r/onr0NZ/1
        RegexCheck = @"(^([1-9]\d*)|^0)([.|,]\d*[1-9])?$",
        AllowedChars = Constants.Char_Numerals + ".,",
        TextFormatingAllowed = false,
        AdditionalFormatCheck = AdditionalCheck.Float,
        SpellCheckingEnabled = false,
        MultiLine = false,
        MaxTextLength = 255
    };

    public static readonly FormatHolder Long = new("Long", QuickImage.Get(ImageCode.Ganzzahl, 16)) {
        RegexCheck = @"^((-?[1-9]\d*)|0)$",
        AllowedChars = Constants.Char_Numerals + "-",
        TextFormatingAllowed = false,
        AdditionalFormatCheck = AdditionalCheck.Integer,
        SpellCheckingEnabled = false,
        MultiLine = false,
        MaxTextLength = long.MinValue.ToString1().Length
    };

    public static readonly FormatHolder LongPositive = new("Long only Positive", QuickImage.Get(ImageCode.Ganzzahl, 16)) {
        RegexCheck = @"^(([1-9]\d*)|0)$",
        AllowedChars = Constants.Char_Numerals,
        TextFormatingAllowed = false,
        AdditionalFormatCheck = AdditionalCheck.Integer,
        SpellCheckingEnabled = false,
        MultiLine = false,
        MaxTextLength = long.MaxValue.ToString1().Length
    };

    public static readonly FormatHolder PhoneNumber = new("Phone Number", QuickImage.Get(ImageCode.Telefon, 16)) {
        //https://regex101.com/r/OzJr8j/1
        RegexCheck = @"^[+][1-9][\s0-9]*[0-9]$",
        AllowedChars = Constants.Char_Numerals + "+ ",
        TextFormatingAllowed = false,
        AdditionalFormatCheck = AdditionalCheck.None,
        SpellCheckingEnabled = false,
        MultiLine = false,
        MaxTextLength = 15,
        QuickInfo = "Internationales Telefon-Format. Beispiel: +49 123 456"
    };

    public static readonly FormatHolder SystemName = new("Systemname", QuickImage.Get(ImageCode.Variable, 16)) {
        AllowedChars = Constants.Char_AZ + Constants.Char_az + Constants.Char_Numerals + "_",
        RegexCheck = @"^[A-Za-z]\S*[A-Za-z0-9]$",
        TextFormatingAllowed = false,
        AdditionalFormatCheck = AdditionalCheck.None,
        SpellCheckingEnabled = false,
        MultiLine = false,
        MaxTextLength = 128,
        QuickInfo = "Werte, wie für eine System-Variabel. Beispiel: WERT_12"
    };

    public static readonly FormatHolder Text = new("Text", QuickImage.Get(ImageCode.Textfeld, 16)) {
        AllowedChars = string.Empty,
        RegexCheck = string.Empty,
        TextFormatingAllowed = false,
        AdditionalFormatCheck = AdditionalCheck.None,
        SpellCheckingEnabled = true,
        MultiLine = false,
        MaxTextLength = 4000
    };

    public static readonly FormatHolder TextMitFormatierung = new("Text with format", QuickImage.Get(ImageCode.Word, 16)) {
        AllowedChars = string.Empty,
        RegexCheck = string.Empty,
        TextFormatingAllowed = true,
        AdditionalFormatCheck = AdditionalCheck.None,
        SpellCheckingEnabled = true,
        MultiLine = true,
        MaxTextLength = 4000,
        QuickInfo = "Text, der Kursiv, Fett, etc. unterstüzt"
    };

    public static readonly FormatHolder Url = new("Url", QuickImage.Get(ImageCode.Globus, 16)) {
        //    https://regex101.com/r/S2CbwM/1
        RegexCheck = @"^(https:|http:|www\.)\S*$",
        AllowedChars = Constants.Char_Numerals + Constants.Char_AZ + Constants.Char_az + "äöüÄÖÜ:?=&.,-_/",
        TextFormatingAllowed = false,
        AdditionalFormatCheck = AdditionalCheck.None,
        SpellCheckingEnabled = false,
        MultiLine = false,
        MaxTextLength = 2048
    };

    private readonly QuickImage _image;

    #endregion

    #region Constructors

    public FormatHolder(string keyname, QuickImage img) {
        KeyName = keyname;
        _image = img;
        AllFormats.Add(this);
    }

    #endregion

    #region Properties

    public AdditionalCheck AdditionalFormatCheck { get; set; } = AdditionalCheck.None;
    public string AllowedChars { get; set; } = string.Empty;
    public bool KeyIsCaseSensitive => false;
    public string KeyName { get; protected set; }
    public int MaxTextLength { get; set; }
    public bool MultiLine { get; set; }
    public string QuickInfo { get; set; } = string.Empty;
    public string RegexCheck { get; set; } = string.Empty;

    public bool SpellCheckingEnabled { get; set; }

    public bool TextFormatingAllowed { get; set; }

    #endregion

    #region Methods

    public string ReadableText() => KeyName;

    public QuickImage SymbolForReadableText() => _image;

    #endregion
}