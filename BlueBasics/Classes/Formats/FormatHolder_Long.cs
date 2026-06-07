// Licensed under AGPL-3.0; see License.md for disclaimer and details.

namespace BlueBasics.Classes;

public class FormatHolder_Long : FormatHolder {

    public static readonly string Keyname = "Long";

    #region Constructors

    public FormatHolder_Long() : base(Keyname, QuickImage.Get(ImageCode.Ganzzahl, 16)) {
        RegexCheck = @"^((-?[1-9]\d*)|0)$";
        AllowedChars = Constants.Char_Numerals + "-";
        TextFormatingAllowed = false;
        AdditionalFormatCheck = AdditionalCheck.Integer;
        SpellCheckingEnabled = false;
        MultiLine = false;
        MaxTextLength = long.MinValue.ToString1().Length;
    }

    #endregion

    #region Properties

    public static FormatHolder Instance => AllFormats[Keyname] ?? throw Develop.DebugError("Fehlerhafter Instanzname");

    #endregion
}