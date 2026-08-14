// Licensed under AGPL-3.0; see License.md for disclaimer and details.

using BlueBasics.Formats;

namespace BlueBasics.Classes.Formats;

public class UrlFormat : Format {

    public static readonly string Keyname = "Url";

    #region Constructors

    public UrlFormat() : base(Keyname, QuickImage.Get(ImageCode.Globus, 16)) {
        //    https://regex101.com/r/S2CbwM/1
        RegexCheck = @"^(https:|http:|www\.)\S*$";
        AllowedChars = Char_Numerals + Char_AZ + Char_az + "äöüÄÖÜ:?=&.,-_/";
        TextFormatingAllowed = false;
        AdditionalFormatCheck = AdditionalCheck.None;
        SpellCheckingEnabled = false;
        MultiLine = false;
        MaxTextLength = 2048;
        MinTextLength = 4;
        ForbiddenChars = "\r\n";
    }

    #endregion

    #region Properties

    public static Format Instance => AllFormats[Keyname] ?? throw Develop.DebugError("Fehlerhafter Instanzname");

    #endregion
}