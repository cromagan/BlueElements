// Licensed under AGPL-3.0; see License.md for disclaimer and details.

namespace BlueBasics.Classes;

public class FormatHolder_FilepathAndName : FormatHolder {

    #region Constructors

    public FormatHolder_FilepathAndName() : base("FilepathAndName", QuickImage.Get(ImageCode.Ordner, 16)) {
        // https://regex101.com/r/5f7WVt/1
        RegexCheck = @"^([A-Za-z]:|\\\\[^\\\/:*?""<>|\r\n]+)\\(?:[^\\\/:*?""<>|\r\n]+\\)*[^\\\/:*?""<>|\r\n]+$";
        AllowedChars = Constants.Char_Numerals + Constants.Char_Buchstaben + Constants.Char_Buchstaben.ToUpperInvariant() + "\\!$&'@^%()[]{}!&#°`:;.,=+-_ ";
        TextFormatingAllowed = false;
        AdditionalFormatCheck = AdditionalCheck.None;
        SpellCheckingEnabled = false;
        MultiLine = false;
        MaxTextLength = 512;
    }

    #endregion

    #region Properties

    public static FormatHolder Instance => AllFormats.GetByKey("FilepathAndName") ?? throw Develop.DebugError("Fehlerhafter Instanzname");

    #endregion
}