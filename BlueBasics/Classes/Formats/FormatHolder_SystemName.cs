// Licensed under AGPL-3.0; see License.md for disclaimer and details.

namespace BlueBasics.Classes;

public class FormatHolder_SystemName : FormatHolder {

    #region Constructors

    public FormatHolder_SystemName() : base("Systemname", QuickImage.Get(ImageCode.Variable, 16)) {
        AllowedChars = Constants.Char_AZ + Constants.Char_az + Constants.Char_Numerals + "_";
        RegexCheck = @"^[A-Za-z]\S*[A-Za-z0-9]$";
        TextFormatingAllowed = false;
        AdditionalFormatCheck = AdditionalCheck.None;
        SpellCheckingEnabled = false;
        MultiLine = false;
        MaxTextLength = 128;
        QuickInfo = "Werte, wie für eine System-Variabel. Beispiel: WERT_12";
    }

    #endregion

    #region Properties

    public static FormatHolder Instance => AllFormats["Systemname"] ?? throw Develop.DebugError("Fehlerhafter Instanzname");

    #endregion
}