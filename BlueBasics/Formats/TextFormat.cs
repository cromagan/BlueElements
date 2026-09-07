// Licensed under MIT; see License.md for disclaimer, details, and extended user conditions.

using BlueBasics.Formats;

namespace BlueBasics.Classes.Formats;

public class TextFormat : Format {

    public static readonly string Keyname = "Text";

    #region Constructors

    public TextFormat() : base(Keyname, QuickImage.Get(ImageCode.Textfeld, 16)) {
        AllowedChars = string.Empty;
        RegexCheck = string.Empty;
        AdditionalFormatCheck = AdditionalCheck.None;
        MultiLine = false;
        MaxTextLength = 4000;
        MinTextLength = 0;
    }

    #endregion

    #region Properties

    public static Format Instance => AllFormats[Keyname] ?? throw Develop.DebugError("Fehlerhafter Instanzname");

    #endregion
}