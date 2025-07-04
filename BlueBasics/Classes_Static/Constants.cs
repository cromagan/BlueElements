﻿// Authors:
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
using System.Collections.ObjectModel;
using System.Drawing;
using System.Text;

// http://www.carlosag.net/tools/codetranslator/
// http://converter.telerik.com/

namespace BlueBasics;

public static class Constants {

    #region Fields

    public const string Administrator = "#Administrator";
    public const string AllowedCharsVariableName = Char_az + Char_AZ + "_" + Char_Numerals;
    public const string Char_az = "abcdefghijklmnopqrstuvwxyz";
    public const string Char_AZ = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
    public const string Char_Buchstaben = "abcdefghijklmnopqrstuvwxyzäöüßáéíóúâêîôûàèìòùžñ";
    public const string Char_DateiSonderZeichen = "\\/:*?\"<>|\r\n";
    public const string Char_Numerals = "0123456789";
    public const string Char_PfadSonderZeichen = "*?\"<>|\r\n";
    public const string Everybody = "#Everybody";
    public const char FirstSortChar = '+';
    public const char SecondSortChar = '-';
    public const char TempGreaterThan = '\uE010';
    public const char TempH1 = '\uE001';
    public const char TempH2 = '\uE002';
    public const char TempH3 = '\uE003';
    public const char TempH4 = '\uE004';
    //public const char TempH5 = '\uE005';
    //public const char TempH6 = '\uE006';
    public const char TempH7 = '\uE007';
    public const char TempLessThan = '\uE011';
    public const char TempBold = '\uE000';

    public const string Win11 = "Windows 11";
    public static readonly SolidBrush BrushRedTransparent = new(Color.FromArgb(40, 255, 128, 128));
    public static readonly SolidBrush BrushYellowTransparent = new(Color.FromArgb(180, 255, 255, 0));
    public static readonly string Char_NotFromClip = (char)3 + ((char)22).ToString() + (char)24 + "\n";
    public static readonly int ColumnCaptionSizeY = 22;

    public static readonly string[] DateTimeFormats = ["dd.MM.yyyy HH:mm:ss",
                                                       "dd.MM.yyyy",
                                                       "dd.MM.yyyy HH:mm",
                                                       "MM/dd/yyyy HH:mm:ss",
                                                       "yyyy/MM/dd HH:mm:ss",
                                                       "MM/dd/yyyy",
                                                       "dd.MM.yy",
                                                       "yyyy/MM/dd HH:mm:ss.fff",
                                                       "dd.MM.yyyy HH:mm:ss.fff",
                                                       "yyyy-MM-dd HH:mm:ss.fff",
                                                       "yyyy_MM_dd",
                                                       "yyyy-MM-dd",
                                                       "yyyy-MM-dd HH:mm",
                                                       "yyyy-MM-dd HH:mm:ss",
                                                       "yyyy-MM-dd HH:mm:ss.fff",
                                                       "yyyy-MM-dd_HH-mm-ss",
                                                       "dd.MM.yyyy H:mm",
                                                       "d.M.yy",
                                                       "d.M.yy HH:mm:ss"
    ];

    public static readonly float DefaultTolerance = 0.0001f;
    public static readonly List<string> Gleich = ["="];

    // public static readonly string[] Umrechnungen = { "1000 μm = 1 mm", "10 mm = 1 cm", "10 cm = 1 dm", "10 dm = 1 m", "1000 m = 1 km", "1000 μg = 1 mg", "1000 mg = 1 g", "1000 g = 1 kg", "1000 kg = 1 t", "1 d = 24 h", "1 h = 60 min", "1 min = 60 s", "1000 ms = 1 s", "1000 μl = 1 ml", "10 ml = 1 cl", "10 cl = 1 dl", "10 dl = 1 l", "100 l = 1 hl", "1 kcal = 4,187 kJ", "1000 cal = 1 kcal", "1000 J = 1 kJ", "1 mph = 1,609344 km/h", "1 m/s = 3600 m/h", "1 m/s = 3,6 km/h", "1 € = 100 ct", "1 byte = 8 bit", "1 MB = 1024 byte", "1 GB = 1024 MB", "1 TB = 1024 GB" }
    public static readonly Random GlobalRnd = new();

    public static readonly Dictionary<char, string> HtmlEntities = new() {
        {'ü', "&uuml;"}, {'ä', "&auml;"}, {'ö', "&ouml;"},
        {'Ü', "&Uuml;"}, {'Ä', "&Auml;"}, {'Ö', "&Ouml;"},
        {'ß', "&szlig;"}, {'"', "&quot;"}, {'&', "&amp;"},
        {'<', "&lt;"}, {'>', "&gt;"}, {'Ø', "&Oslash;"},
        {'ø', "&oslash;"}, {'•', "&bull;"}, {'é', "&eacute;"},
        {'É', "&Eacute;"}, {'€', "&euro;"}
    };

    //public static double FineTolerance = 0.0000001d; // Es werden nur 5 Nachkommastellen auf Festplatte gespeichert
    public static readonly float IntTolerance = 0.5f;

    public static readonly List<string> KlammerGeschweiftZu = ["}"];
    public static readonly List<List<string>> KlammernAlle = [["(", ")"], ["{", "}"], ["[", "]"]];
    public static readonly List<List<string>> KlammernEckig = [["[", "]"]];
    public static readonly List<List<string>> KlammernGeschweift = [["{", "}"]];
    public static readonly List<List<string>> KlammernRund = [["(", ")"]];
    public static readonly List<string> KlammerRundAuf = ["("];
    public static readonly List<string> KlammerRundZu = [")"];
    public static readonly List<string> Komma = [","];
    public static readonly Pen PenRed1 = new(Color.Red, 1);
    public static readonly HashSet<char> PossibleLineBreaks = InitializePossibleLineBreaks();

    public static readonly Dictionary<string, string> Replacements = new() {
                    {"ä", "ae"}, {"ö", "oe"}, {"ü", "ue"},
                    {"á", "a"},  {"ó", "o"},  {"ú", "u"}, {"í", "i"}, {"é", "e"},
                    {"à", "a"},  {"ò", "o"},  {"ù", "u"}, {"ì", "i"}, {"è", "e"},
                    {"â", "a"},  {"ô", "o"},  {"û", "u"}, {"î", "i"}, {"ê", "e"},
                    {"ž", "z"},
                    {"ß", "ss"}
                    //// Spanische Akzente und Sonderzeichen
                    //{"ñ", "n"},
                    // // Französische Akzente und Ligaturen
                    //{"ç", "c"},
                    //{"œ", "oe"},
                    //{"æ", "ae"},

                    //// Portugiesische Akzente und Sonderzeichen
                    //{"ã", "a"}, {"õ", "o"},
                    //{"ç", "c"},

                    //// Türkische Sonderzeichen
                    //{"ş", "s"}, {"ğ", "g"},

                    //// Skandinavische Zeichen
                    //{"å", "a"},
                    //{"ø", "o"},

                    //// Polnische Akzente
                    //{"ł", "l"},
                    //{"ń", "n"}, {"ś", "s"},
                    //{"ź", "z"}, {"ć", "c"},
                    //{"ę", "e"},

                    //// Weitere europäische Sonderzeichen
                    //{"đ", "d"},
                    //{"ħ", "h"},
                    //{"ł", "l"},
                    };

    public static readonly Dictionary<string, char> ReverseHtmlEntities = new() {
    {"&uuml;", 'ü'},
    {"&auml;", 'ä'},
    {"&ouml;", 'ö'},
    {"&Uuml;", 'Ü'},
    {"&Auml;", 'Ä'},
    {"&Ouml;", 'Ö'},
    {"&szlig;", 'ß'},
    {"&quot;", '"'},
    {"&amp;", '&'},
    {"&lt;", '<'},
    {"&gt;", '>'},
    {"&Oslash;", 'Ø'},
    {"&oslash;", 'ø'},
    {"&bull;", '•'},
    {"&eacute;", 'é'},
    {"&Eacute;", 'É'},
    {"&euro;", '€'}
};

    public static readonly Encoding Win1252 = Encoding.GetEncoding(1252);
    public static readonly HashSet<char> WordSeparators = InitializeWordSeparators();
    public static ReadOnlyCollection<string> EmptyReadOnly = Array.AsReadOnly(Array.Empty<string>());

    #endregion

    #region Methods

    private static HashSet<char> InitializePossibleLineBreaks() {
        // Alle möglichen Zeichen für Zeilenumbrüche
        const string lineBreakChars = " ?!%/\\}])-.,;_°~€|\r\n\t";

        var result = new HashSet<char>(lineBreakChars.Length);

        // Füge alle Zeichen zum HashSet hinzu
        foreach (var c in lineBreakChars) {
            _ = result.Add(c);
        }

        // Hinweis im Kommentar: Kein Doppelpunkt, weil auch 3:50 Uhr möglich ist
        return result;
    }

    private static HashSet<char> InitializeWordSeparators() {
        var separators = new HashSet<char>();

        // Füge alle Sonderzeichen hinzu
        const string specialSeparators = "~|=<>+`´\r\n\t()";
        foreach (var c in specialSeparators) {
            _ = separators.Add(c);
        }

        // Füge alle Unicode-Punktuationszeichen hinzu
        for (var c = char.MinValue; c < char.MaxValue; c++) {
            if (char.IsPunctuation(c) || char.IsSeparator(c)) {
                _ = separators.Add(c);
            }
        }

        // Unterstrich ist kein Separator
        _ = separators.Remove('_');

        return separators;
    }

    #endregion
}