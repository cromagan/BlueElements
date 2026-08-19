// Licensed under AGPL-3.0; see License.md for disclaimer and details.

using BlueBasics.Formats;

namespace BlueBasics.Classes.Formats;

public class TextWithFormatFormat : Format {

    public static readonly string Keyname = "TextWithFormat";

    #region Constructors

    public TextWithFormatFormat() : base(Keyname, QuickImage.Get(ImageCode.Word, 16)) {
        AllowedChars = string.Empty;
        RegexCheck = string.Empty;
        AdditionalFormatCheck = AdditionalCheck.None;
        MultiLine = true;
        MaxTextLength = 4000;
        MinTextLength = 0;
        QuickInfo = "TextFormat, der Kursiv, Fett, etc. unterstüzt";
    }

    #endregion

    #region Properties

    public static Format Instance => AllFormats[Keyname] ?? throw Develop.DebugError("Fehlerhafter Instanzname");

    #endregion
}