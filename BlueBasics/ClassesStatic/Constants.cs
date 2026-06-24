// Licensed under AGPL-3.0; see License.md for disclaimer and details.

using System.Collections.ObjectModel;
using System.Text.RegularExpressions;

// http://www.carlosag.net/tools/codetranslator/
// http://converter.telerik.com/

namespace BlueBasics.ClassesStatic;

public static partial class Constants {

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
    public const string KeyChunk = "Chunk";
    public const string KeyExtendend = "Extended";
    public const string KeyInputRowKey = "CurrentRowKey";
    public const string KeyScriptNo = "ScriptNo";
    public const string KeyTestZeile = "TestZeile";
    public const char SecondSortChar = '-';
    public const string Win11 = "Windows 11";
    public static readonly List<string> BracketCurlyClose = ["}"];
    public static readonly Dictionary<char, char> BracketRound = new() { { '(', ')' } };
    public static readonly List<string> BracketRoundClose = [")"];
    public static readonly Dictionary<char, char> Brackets = new() { { '(', ')' }, { '{', '}' }, { '[', ']' } };
    public static readonly List<string> BracketSquareClose = ["]"];
    public static readonly string Char_NotFromClip = $"{(char)3}{(char)22}{(char)24}\n";

    public static readonly List<string> Comma = [","];

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
    public static readonly ReadOnlyCollection<string> EmptyReadOnly = Array.AsReadOnly(Array.Empty<string>());
    public static readonly List<string> EqualsSign = ["="];
    public static readonly Random GlobalRnd = new();

    //public static double FineTolerance = 0.0000001d; // Es werden nur 5 Nachkommastellen auf Festplatte gespeichert
    public static readonly float IntTolerance = 0.5f;

    // Used: NextText-Hot-Loop. O(1)-Lookup statt O(n) bei string.Contains und foreach über Klammerpaare.
    public static readonly HashSet<char> NextTextSeparators = new("&.,;\\?!\" ~|=<>+-(){}[]/*`´^\r\n\t¶");

    public static readonly Pen PenRed1 = new(Color.Red, 1);

    public static readonly HashSet<char> PossibleLineBreaks = InitializePossibleLineBreaks();

    // public static readonly string[] Umrechnungen = { "1000 μm = 1 mm", "10 mm = 1 cm", "10 cm = 1 dm", "10 dm = 1 m", "1000 m = 1 km", "1000 μg = 1 mg", "1000 mg = 1 g", "1000 g = 1 kg", "1000 kg = 1 t", "1 d = 24 h", "1 h = 60 min", "1 min = 60 s", "1000 ms = 1 s", "1000 μl = 1 ml", "10 ml = 1 cl", "10 cl = 1 dl", "10 dl = 1 l", "100 l = 1 hl", "1 kcal = 4,187 kJ", "1000 cal = 1 kcal", "1000 J = 1 kJ", "1 mph = 1,609344 km/h", "1 m/s = 3600 m/h", "1 m/s = 3,6 km/h", "1 € = 100 ct", "1 byte = 8 bit", "1 MB = 1024 byte", "1 GB = 1024 MB", "1 TB = 1024 GB" }
    public static readonly List<string> RechenOperatoren = ["^", "*", "/", "+", "-"];

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

    public static readonly HashSet<char> WordSeparators = InitializeWordSeparators();

    internal static readonly Dictionary<char, char> BracketsCloseToOpen = Brackets.ToDictionary(kvp => kvp.Value, kvp => kvp.Key);

    internal static readonly HashSet<char> BracketsOpen = [.. Brackets.Keys];

    #endregion

    #region Properties

    public static Encoding Win1252 {
        get {
            if (field is not null) { return field; }
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
            field = Encoding.GetEncoding(1252);
            return field;
        }
    }

    #endregion

    #region Methods

    [GeneratedRegex(@"<[^>]+>", RegexOptions.IgnoreCase)]
    public static partial Regex HtmlTagRegex();

    [GeneratedRegex(@"\p{L}+")]
    public static partial Regex WordPatternRegex();

    [System.Runtime.CompilerServices.ModuleInitializer]
    internal static void Init() => Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

    private static HashSet<char> InitializePossibleLineBreaks() {
        // Alle möglichen Zeichen für Zeilenumbrüche
        const string lineBreakChars = " ?!%/\\}])-.,;_°~€|\r\n\t";
        return [.. lineBreakChars];
    }

    private static HashSet<char> InitializeWordSeparators() {
        var separators = new HashSet<char>("~|=<>+`´\r\n\t()");

        // Füge alle Unicode-Punktuationszeichen hinzu
        for (var c = char.MinValue; c < char.MaxValue; c++) {
            if (char.IsPunctuation(c) || char.IsSeparator(c)) {
                separators.Add(c);
            }
        }

        // Unterstrich ist kein Separator
        separators.Remove('_');

        return separators;
    }

    #endregion
}