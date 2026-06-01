// Licensed under AGPL-3.0; see License.md for disclaimer and details.

namespace BlueBasics.Classes;

public class FormatHolder_Bit : FormatHolder {

    public static readonly string _keyname = "Bit";

    #region Constructors

    public FormatHolder_Bit() : base(_keyname, QuickImage.Get(ImageCode.Häkchen, 16)) {
        AllowedChars = "+-";
        RegexCheck = "^([+]|[-])$";
        TextFormatingAllowed = false;
        AdditionalFormatCheck = AdditionalCheck.None;
        SpellCheckingEnabled = false;
        MultiLine = false;
        MaxTextLength = 1;
        QuickInfo = "Ja/Nein Werte";
    }

    #endregion

    #region Properties

    public static FormatHolder Instance => AllFormats[_keyname] ?? throw Develop.DebugError("Fehlerhafter Instanzname");

    #endregion
}