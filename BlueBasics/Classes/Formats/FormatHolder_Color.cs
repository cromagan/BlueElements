// Licensed under AGPL-3.0; see License.md for disclaimer and details.

namespace BlueBasics.Classes;

public class FormatHolder_Color : FormatHolder {

    public static readonly string _keyname = "Color";

    #region Constructors

    public FormatHolder_Color() : base(_keyname, QuickImage.Get(ImageCode.Farbrad, 16)) {
        RegexCheck = @"^#([0-9a-f]{6}|[0-9a-f]{8})$";
        AllowedChars = Constants.Char_Numerals + "#abcdef";
        TextFormatingAllowed = false;
        AdditionalFormatCheck = AdditionalCheck.None;
        SpellCheckingEnabled = false;
        MultiLine = false;
        MaxTextLength = 9;
        QuickInfo = "Farbcode im Hex-Format. Beispiel: #aa0000";
    }

    #endregion

    #region Properties

    public static FormatHolder Instance => AllFormats[_keyname] ?? throw Develop.DebugError("Fehlerhafter Instanzname");

    #endregion
}