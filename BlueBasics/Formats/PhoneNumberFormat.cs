// Licensed under AGPL-3.0; see License.md for disclaimer and details.

using BlueBasics.Formats;

namespace BlueBasics.Classes.Formats;

public class PhoneNumberFormat : Format {

    public static readonly string Keyname = "PhoneNumber";

    #region Constructors

    public PhoneNumberFormat() : base(Keyname, QuickImage.Get(ImageCode.Telefon, 16)) {
        //https://regex101.com/r/OzJr8j/1
        RegexCheck = @"^[+][1-9][\s0-9]*[0-9]$";
        AllowedChars = Char_Numerals + "+ ";
        TextFormatingAllowed = false;
        AdditionalFormatCheck = AdditionalCheck.None;
        SpellCheckingEnabled = false;
        MultiLine = false;
        MaxTextLength = 15;
        MinTextLength = 3;
        ForbiddenChars = "\r\n";
        QuickInfo = "Internationales Telefon-Format. Beispiel: +49 123 456";
    }

    #endregion

    #region Properties

    public static Format Instance => AllFormats[Keyname] ?? throw Develop.DebugError("Fehlerhafter Instanzname");

    #endregion
}