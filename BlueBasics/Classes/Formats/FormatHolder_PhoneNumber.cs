// Licensed under AGPL-3.0; see License.md for disclaimer and details.

namespace BlueBasics.Classes;

public class FormatHolder_PhoneNumber : FormatHolder {

    #region Constructors

    public FormatHolder_PhoneNumber() : base("Phone Number", QuickImage.Get(ImageCode.Telefon, 16)) {
        //https://regex101.com/r/OzJr8j/1
        RegexCheck = @"^[+][1-9][\s0-9]*[0-9]$";
        AllowedChars = Constants.Char_Numerals + "+ ";
        TextFormatingAllowed = false;
        AdditionalFormatCheck = AdditionalCheck.None;
        SpellCheckingEnabled = false;
        MultiLine = false;
        MaxTextLength = 15;
        QuickInfo = "Internationales Telefon-Format. Beispiel: +49 123 456";
    }

    #endregion

    #region Properties

    public static FormatHolder Instance => AllFormats.GetByKey("Phone Number") ?? throw Develop.DebugError("Fehlerhafter Instanzname");

    #endregion
}