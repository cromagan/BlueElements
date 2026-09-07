// Licensed under MIT; see License.md for disclaimer, details, and extended user conditions.

using BlueBasics.Formats;

namespace BlueBasics.Classes.Formats;

public class BitFormat : Format {

    #region Fields

    public static readonly string Keyname = "Bit";

    #endregion

    #region Constructors

    public BitFormat() : base(Keyname, QuickImage.Get(ImageCode.Häkchen, 16)) {
        AllowedChars = "+-";
        RegexCheck = "^([+]|[-])$";
        AdditionalFormatCheck = AdditionalCheck.None;
        MultiLine = false;
        MaxTextLength = 1;
        MinTextLength = 1;
        ForbiddenChars = "\r\n";
        QuickInfo = "Ja/Nein Werte";
    }

    #endregion

    #region Properties

    public static Format Instance => AllFormats[Keyname] ?? throw Develop.DebugError("Fehlerhafter Instanzname");

    #endregion
}