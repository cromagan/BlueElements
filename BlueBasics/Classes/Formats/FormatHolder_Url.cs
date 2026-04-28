// Licensed under AGPL-3.0; see License.md for disclaimer and details.

using BlueBasics.ClassesStatic;
using BlueBasics.Enums;

namespace BlueBasics.Classes;

public class FormatHolder_Url : FormatHolder {

    #region Constructors

    public FormatHolder_Url() : base("Url", QuickImage.Get(ImageCode.Globus, 16)) {
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
}
