// Licensed under AGPL-3.0; see License.md for disclaimer and details.

using BlueBasics.Formats;

namespace BlueBasics.Classes.Formats;

public class LongFormat : Format {

    public static readonly string Keyname = "Long";

    #region Constructors

    public LongFormat() : base(Keyname, QuickImage.Get(ImageCode.Ganzzahl, 16)) {
        RegexCheck = @"^((-?[1-9]\d*)|0)$";
        AllowedChars = Char_Numerals + "-";
        TextFormatingAllowed = false;
        AdditionalFormatCheck = AdditionalCheck.Integer;
        SpellCheckingEnabled = false;
        MultiLine = false;
        MaxTextLength = long.MinValue.ToString1().Length;
        MinTextLength = 1;
        ForbiddenChars = "\r\n";
    }

    #endregion

    #region Properties

    public static Format Instance => AllFormats[Keyname] ?? throw Develop.DebugError("Fehlerhafter Instanzname");

    #endregion
}