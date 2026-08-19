// Licensed under AGPL-3.0; see License.md for disclaimer and details.

using BlueBasics.Formats;

namespace BlueBasics.Classes.Formats;

public class EMailFormat : Format {

    #region Fields

    public static readonly string Keyname = "EMail";

    #endregion

    #region Constructors

    public EMailFormat() : base(Keyname, QuickImage.Get(ImageCode.Brief, 16)) {
        //https://en.wikipedia.org/wiki/Email_address#:~:text=The%20format%20of%20an%20email,a%20maximum%20of%20255%20octets.
        //http://emailregex.com/
        RegexCheck = "^[a-z0-9A-Z._-]{1,63}[@][a-z0-9A-Z.-]{1,63}[.][a-zA-Z.]{1,63}$";
        AllowedChars = Char_Numerals + Char_AZ + Char_az + "@.-_";
        AdditionalFormatCheck = AdditionalCheck.None;
        MultiLine = false;
        MaxTextLength = 320;
        MinTextLength = 5;
        ForbiddenChars = "\r\n";
    }

    #endregion

    #region Properties

    public static Format Instance => AllFormats[Keyname] ?? throw Develop.DebugError("Fehlerhafter Instanzname");

    #endregion
}