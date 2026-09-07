// Licensed under MIT; see License.md for disclaimer, details, and extended user conditions.

using BlueBasics.Formats;

namespace BlueBasics.Classes.Formats;

public class FilepathFormat : Format {

    #region Fields

    public static readonly string Keyname = "Filepath";

    #endregion

    #region Constructors

    public FilepathFormat() : base(Keyname, QuickImage.Get(ImageCode.Ordner, 16)) {
        // https://regex101.com/r/xuJ7gR/1
        RegexCheck = @"^([A-Za-z]:|\\\\[^\\\/:*?""<>|\r\n]+\\[^\\\/:*?""<>|\r\n]+)\\(?:[^\\\/:*?""<>|\r\n]+\\)*$|^[A-Za-z]:\\$";
        AllowedChars = Char_Numerals + Char_Buchstaben + Char_Buchstaben.ToUpperInvariant() + "\\!$&'@^%()[]{}!&#°`:;.,=+-_ ";
        AdditionalFormatCheck = AdditionalCheck.None;
        MultiLine = false;
        MaxTextLength = 512;
        MinTextLength = 3;
        ForbiddenChars = "\r\n";
    }

    #endregion

    #region Properties

    public static Format Instance => AllFormats[Keyname] ?? throw Develop.DebugError("Fehlerhafter Instanzname");

    #endregion
}