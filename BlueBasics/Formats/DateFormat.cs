// Licensed under AGPL-3.0; see License.md for disclaimer and details.

using BlueBasics.Formats;

namespace BlueBasics.Classes.Formats;

public class DateFormat : Format {

    public static readonly string Keyname = "Date";

    #region Constructors

    public DateFormat() : base(Keyname, QuickImage.Get(ImageCode.Uhr, 16)) {
        RegexCheck = @"^(0[1-9]|[12][0-9]|3[01])[.](0[1-9]|1[0-2])[.]\d{4}$";
        AllowedChars = Char_Numerals + ".";
        TextFormatingAllowed = false;
        AdditionalFormatCheck = AdditionalCheck.DateTime;
        SpellCheckingEnabled = false;
        MultiLine = false;
        MaxTextLength = 10;
        MinTextLength = 10;
        ForbiddenChars = "\r\n";
        QuickInfo = "Deutsches Datum. Beispiel: 31.12.2000";
    }

    #endregion

    #region Properties

    public static Format Instance => AllFormats[Keyname] ?? throw Develop.DebugError("Fehlerhafter Instanzname");

    #endregion
}