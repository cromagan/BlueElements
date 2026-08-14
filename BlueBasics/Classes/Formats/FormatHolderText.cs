// Licensed under AGPL-3.0; see License.md for disclaimer and details.

namespace BlueBasics.Classes;

public class FormatHolderText : FormatHolder {

    public static readonly string Keyname = "Text";

    #region Constructors

    public FormatHolderText() : base(Keyname, QuickImage.Get(ImageCode.Textfeld, 16)) {
        AllowedChars = string.Empty;
        RegexCheck = string.Empty;
        TextFormatingAllowed = false;
        AdditionalFormatCheck = AdditionalCheck.None;
        SpellCheckingEnabled = true;
        MultiLine = false;
        MaxTextLength = 4000;
        MinTextLength = 0;
    }

    #endregion

    #region Properties

    public static FormatHolder Instance => AllFormats[Keyname] ?? throw Develop.DebugError("Fehlerhafter Instanzname");

    #endregion
}