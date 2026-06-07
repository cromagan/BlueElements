// Licensed under AGPL-3.0; see License.md for disclaimer and details.

namespace BlueBasics.Classes;

public class FormatHolder_Url : FormatHolder {

    public static readonly string Keyname = "Url";

    #region Constructors

    public FormatHolder_Url() : base(Keyname, QuickImage.Get(ImageCode.Globus, 16)) {
        //    https://regex101.com/r/S2CbwM/1
        RegexCheck = @"^(https:|http:|www\.)\S*$";
        AllowedChars = Constants.Char_Numerals + Constants.Char_AZ + Constants.Char_az + "äöüÄÖÜ:?=&.,-_/";
        TextFormatingAllowed = false;
        AdditionalFormatCheck = AdditionalCheck.None;
        SpellCheckingEnabled = false;
        MultiLine = false;
        MaxTextLength = 2048;
    }

    #endregion

    #region Properties

    public static FormatHolder Instance => AllFormats[Keyname] ?? throw Develop.DebugError("Fehlerhafter Instanzname");

    #endregion
}