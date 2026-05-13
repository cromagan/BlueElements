// Licensed under AGPL-3.0; see License.md for disclaimer and details.

namespace BlueBasics.Classes;

public class FormatHolder_Email : FormatHolder {

    #region Constructors

    public FormatHolder_Email() : base("EMail", QuickImage.Get(ImageCode.Brief, 16)) {
        //https://en.wikipedia.org/wiki/Email_address#:~:text=The%20format%20of%20an%20email,a%20maximum%20of%20255%20octets.
        //http://emailregex.com/
        RegexCheck = "^[a-z0-9A-Z._-]{1,63}[@][a-z0-9A-Z.-]{1,63}[.][a-zA-Z.]{1,63}$";
        AllowedChars = Constants.Char_Numerals + Constants.Char_AZ + Constants.Char_az + "@.-_";
        TextFormatingAllowed = false;
        AdditionalFormatCheck = AdditionalCheck.None;
        SpellCheckingEnabled = false;
        MultiLine = false;
        MaxTextLength = 320;
    }

    #endregion

    #region Properties

    public static FormatHolder Instance => AllFormats["EMail"] ?? throw Develop.DebugError("Fehlerhafter Instanzname");

    #endregion
}