// Licensed under AGPL-3.0; see License.md for disclaimer and details.

namespace BlueBasics.Classes;

public class FormatHolder_Filepath : FormatHolder {

    private static readonly string _keyname = "Filepath";

    #region Constructors

    public FormatHolder_Filepath() : base(_keyname, QuickImage.Get(ImageCode.Ordner, 16)) {
        // https://regex101.com/r/xuJ7gR/1
        RegexCheck = @"^([A-Za-z]:|\\\\[^\\\/:*?""<>|\r\n]+\\[^\\\/:*?""<>|\r\n]+)\\(?:[^\\\/:*?""<>|\r\n]+\\)*$|^[A-Za-z]:\\$";
        AllowedChars = Constants.Char_Numerals + Constants.Char_Buchstaben + Constants.Char_Buchstaben.ToUpperInvariant() + "\\!$&'@^%()[]{}!&#°`:;.,=+-_ ";
        TextFormatingAllowed = false;
        AdditionalFormatCheck = AdditionalCheck.None;
        SpellCheckingEnabled = false;
        MultiLine = false;
        MaxTextLength = 512;
    }

    #endregion

    #region Properties

    public static FormatHolder Instance => AllFormats[_keyname] ?? throw Develop.DebugError("Fehlerhafter Instanzname");

    #endregion
}