// Licensed under AGPL-3.0; see License.md for disclaimer and details.

namespace BlueBasics.Classes;

public class FormatHolder_TextMultiline : FormatHolder {

    public static readonly string _keyname = "TextMultiline";

    #region Constructors

    public FormatHolder_TextMultiline() : base(_keyname, QuickImage.Get(ImageCode.Textfeld, 16)) {
        AllowedChars = string.Empty;
        RegexCheck = string.Empty;
        TextFormatingAllowed = false;
        AdditionalFormatCheck = AdditionalCheck.None;
        SpellCheckingEnabled = true;
        MultiLine = true;
        MaxTextLength = 4000;
    }

    #endregion

    #region Properties

    public static FormatHolder Instance => AllFormats[_keyname] ?? throw Develop.DebugError("Fehlerhafter Instanzname");

    #endregion
}