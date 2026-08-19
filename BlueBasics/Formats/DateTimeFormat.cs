// Licensed under AGPL-3.0; see License.md for disclaimer and details.

using BlueBasics.Formats;

namespace BlueBasics.Classes.Formats;

public class DateTimeFormat : Format {

    #region Fields

    public static readonly string Keyname = "DateTime";

    #endregion

    #region Constructors

    public DateTimeFormat() : base(Keyname, QuickImage.Get(ImageCode.Uhr, 16)) {
        RegexCheck = @"^(0[1-9]|[12][0-9]|3[01])[.](0[1-9]|1[0-2])[.]\d{4}[ ](0[0-9]|1[0-9]|2[0-3]):[0-5][0-9]:[0-5][0-9]$";
        AllowedChars = Char_Numerals + ":. ";
        AdditionalFormatCheck = AdditionalCheck.DateTime;
        MultiLine = false;
        MaxTextLength = 19;
        MinTextLength = 19;
        ForbiddenChars = "\r\n";
        QuickInfo = "Deutsches Datum und Uhrzeit. Beispiel: 31.12.2000 12:34:00";
    }

    #endregion

    #region Properties

    public static Format Instance => AllFormats[Keyname] ?? throw Develop.DebugError("Fehlerhafter Instanzname");

    #endregion
}