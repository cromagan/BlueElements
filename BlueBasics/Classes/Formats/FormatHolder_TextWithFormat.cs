// Licensed under AGPL-3.0; see License.md for disclaimer and details.

namespace BlueBasics.Classes;

public class FormatHolder_TextWithFormat : FormatHolder {

    public static readonly string _keyname = "TextWithFormat";

    #region Constructors

    public FormatHolder_TextWithFormat() : base(_keyname, QuickImage.Get(ImageCode.Word, 16)) {
        AllowedChars = string.Empty;
        RegexCheck = string.Empty;
        TextFormatingAllowed = true;
        AdditionalFormatCheck = AdditionalCheck.None;
        SpellCheckingEnabled = true;
        MultiLine = true;
        MaxTextLength = 4000;
        QuickInfo = "Text, der Kursiv, Fett, etc. unterstüzt";
    }

    #endregion

    #region Properties

    public static FormatHolder Instance => AllFormats[_keyname] ?? throw Develop.DebugError("Fehlerhafter Instanzname");

    #endregion
}