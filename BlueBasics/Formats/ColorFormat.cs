// Licensed under AGPL-3.0; see License.md for disclaimer and details.

using BlueBasics.Formats;

namespace BlueBasics.Classes.Formats;

public class ColorFormat : Format {

    #region Fields

    public static readonly string Keyname = "Color";

    #endregion

    #region Constructors

    public ColorFormat() : base(Keyname, QuickImage.Get(ImageCode.Farbrad, 16)) {
        RegexCheck = @"^#([0-9a-f]{6}|[0-9a-f]{8})$";
        AllowedChars = Char_Numerals + "#abcdef";
        AdditionalFormatCheck = AdditionalCheck.None;
        MultiLine = false;
        MaxTextLength = 9;
        MinTextLength = 7;
        ForbiddenChars = "\r\n";
        QuickInfo = "Farbcode im Hex-Format. Beispiel: #aa0000";
    }

    #endregion

    #region Properties

    public static Format Instance => AllFormats[Keyname] ?? throw Develop.DebugError("Fehlerhafter Instanzname");

    #endregion
}