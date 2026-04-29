// Licensed under AGPL-3.0; see License.md for disclaimer and details.

namespace BlueBasics.Classes;

public class FormatHolder_LongPositive : FormatHolder {

    #region Constructors

    public FormatHolder_LongPositive() : base("Long only Positive", QuickImage.Get(ImageCode.Ganzzahl, 16)) {
        RegexCheck = @"^(([1-9]\d*)|0)$";
        AllowedChars = Constants.Char_Numerals;
        TextFormatingAllowed = false;
        AdditionalFormatCheck = AdditionalCheck.Integer;
        SpellCheckingEnabled = false;
        MultiLine = false;
        MaxTextLength = long.MaxValue.ToString1().Length;
    }

    #endregion

    #region Properties

    public static FormatHolder Instance => AllFormats.GetByKey("Long only Positive") ?? throw Develop.DebugError("Fehlerhafter Instanzname");

    #endregion
}