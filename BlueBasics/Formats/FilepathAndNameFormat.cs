// Licensed under AGPL-3.0; see License.md for disclaimer and details.

using BlueBasics.Formats;

namespace BlueBasics.Classes.Formats;

public class FilepathAndNameFormat : Format {

    #region Fields

    public static readonly string Keyname = "FilepathAndName";

    #endregion

    #region Constructors

    public FilepathAndNameFormat() : base(Keyname, QuickImage.Get(ImageCode.Ordner, 16)) {
        // https://regex101.com/r/5f7WVt/1
        RegexCheck = @"^([A-Za-z]:|\\\\[^\\\/:*?""<>|\r\n]+)\\(?:[^\\\/:*?""<>|\r\n]+\\)*[^\\\/:*?""<>|\r\n]+$";
        AllowedChars = Char_Numerals + Char_Buchstaben + Char_Buchstaben.ToUpperInvariant() + "\\!$&'@^%()[]{}!&#°`:;.,=+-_ ";
        AdditionalFormatCheck = AdditionalCheck.None;
        MultiLine = false;
        MaxTextLength = 512;
        MinTextLength = 4;
        ForbiddenChars = "\r\n";
    }

    #endregion

    #region Properties

    public static Format Instance => AllFormats[Keyname] ?? throw Develop.DebugError("Fehlerhafter Instanzname");

    #endregion
}