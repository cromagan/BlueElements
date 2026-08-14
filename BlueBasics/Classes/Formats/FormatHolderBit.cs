// Licensed under AGPL-3.0; see License.md for disclaimer and details.

namespace BlueBasics.Classes;

public class FormatHolderBit : FormatHolder {

    #region Fields

    public static readonly string Keyname = "Bit";

    #endregion

    #region Constructors

    public FormatHolderBit() : base(Keyname, QuickImage.Get(ImageCode.Häkchen, 16)) {
        AllowedChars = "+-";
        RegexCheck = "^([+]|[-])$";
        TextFormatingAllowed = false;
        AdditionalFormatCheck = AdditionalCheck.None;
        SpellCheckingEnabled = false;
        MultiLine = false;
        MaxTextLength = 1;
        MinTextLength = 1;
        ForbiddenChars = "\r\n";
        QuickInfo = "Ja/Nein Werte";
    }

    #endregion

    #region Properties

    public static FormatHolder Instance => AllFormats[Keyname] ?? throw Develop.DebugError("Fehlerhafter Instanzname");

    #endregion
}