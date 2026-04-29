// Licensed under AGPL-3.0; see License.md for disclaimer and details.

namespace BlueBasics.Classes;

public class FormatHolder_Date : FormatHolder {

    #region Constructors

    public FormatHolder_Date() : base("Date", QuickImage.Get(ImageCode.Uhr, 16)) {
        RegexCheck = @"^(0[1-9]|[12][0-9]|3[01])[.](0[1-9]|1[0-2])[.]\d{4}$";
        AllowedChars = Constants.Char_Numerals + ".";
        TextFormatingAllowed = false;
        AdditionalFormatCheck = AdditionalCheck.DateTime;
        SpellCheckingEnabled = false;
        MultiLine = false;
        MaxTextLength = 10;
        QuickInfo = "Deutsches Datum. Beispiel: 31.12.2000";
    }

    #endregion

    #region Properties

    public static FormatHolder Instance => AllFormats.GetByKey("Date") ?? throw Develop.DebugError("Fehlerhafter Instanzname");

    #endregion
}