// Licensed under AGPL-3.0; see License.md for disclaimer and details.

namespace BlueBasics.Classes;

public class FormatHolder_DateTimeWithMilliSeconds : FormatHolder {

    public static readonly string Keyname = "DateTimeWithMilliSeconds";

    #region Constructors

    public FormatHolder_DateTimeWithMilliSeconds() : base(Keyname, QuickImage.Get(ImageCode.Uhr, 16)) {
        RegexCheck = @"^(0[1-9]|[12][0-9]|3[01])[.](0[1-9]|1[0-2])[.]\d{4}[ ](0[0-9]|1[0-9]|2[0-3]):[0-5][0-9]:[0-5][0-9][.][0-9][0-9][0-9]$";
        AllowedChars = Constants.Char_Numerals + ":. ";
        TextFormatingAllowed = false;
        AdditionalFormatCheck = AdditionalCheck.DateTime;
        SpellCheckingEnabled = false;
        MultiLine = false;
        MaxTextLength = 23;
        QuickInfo = "Deutsches Datum und Uhrzeit mit Millisecunden. Beispiel: 31.12.2000 12:34:00.123";
    }

    #endregion

    #region Properties

    public static FormatHolder Instance => AllFormats[Keyname] ?? throw Develop.DebugError("Fehlerhafter Instanzname");

    #endregion
}