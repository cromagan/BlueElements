// Licensed under AGPL-3.0; see License.md for disclaimer and details.

namespace BlueBasics.Classes;

public class FormatHolder_Bit : FormatHolder {

    #region Constructors

    public FormatHolder_Bit() : base("Bit", QuickImage.Get(ImageCode.Häkchen, 16)) {
        AllowedChars = "+-";
        RegexCheck = "^([+]|[-])$";
        TextFormatingAllowed = false;
        AdditionalFormatCheck = AdditionalCheck.None;
        SpellCheckingEnabled = false;
        MultiLine = false;
        MaxTextLength = 1;
        QuickInfo = "Ja/Nein Werte";
    }

    #endregion
}
