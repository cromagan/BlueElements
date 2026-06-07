// Licensed under AGPL-3.0; see License.md for disclaimer and details.

namespace BlueBasics.Classes;

public class FormatHolder_Text : FormatHolder {

    public static readonly string Keyname = "Text";

    #region Constructors

    public FormatHolder_Text() : base(Keyname, QuickImage.Get(ImageCode.Textfeld, 16)) {
        AllowedChars = string.Empty;
        RegexCheck = string.Empty;
        TextFormatingAllowed = false;
        AdditionalFormatCheck = AdditionalCheck.None;
        SpellCheckingEnabled = true;
        MultiLine = false;
        MaxTextLength = 4000;
    }

    #endregion

    #region Properties

    public static FormatHolder? Instance => AllFormats[Keyname] ?? throw Develop.DebugError("Fehlerhafter Instanzname");

    #endregion
}