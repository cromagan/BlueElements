// Licensed under AGPL-3.0; see License.md for disclaimer and details.

namespace BlueBasics.Classes;

public class FormatHolder_TextMitFormatierung : FormatHolder {

    #region Constructors

    public FormatHolder_TextMitFormatierung() : base("Text with format", QuickImage.Get(ImageCode.Word, 16)) {
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
}
