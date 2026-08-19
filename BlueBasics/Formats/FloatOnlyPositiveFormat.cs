// Licensed under AGPL-3.0; see License.md for disclaimer and details.

using BlueBasics.Formats;

namespace BlueBasics.Classes.Formats;

public class FloatOnlyPositiveFormat : Format {

    #region Fields

    public static readonly string Keyname = "FloatOnlyPositive";

    #endregion

    #region Constructors

    public FloatOnlyPositiveFormat() : base(Keyname, QuickImage.Get(ImageCode.Gleitkommazahl, 16)) {
        //https://regex101.com/r/onr0NZ/1
        RegexCheck = @"(^([1-9]\d*)|^0)([.|,]\d*[1-9])?$";
        AllowedChars = Char_Numerals + ".,";
        AdditionalFormatCheck = AdditionalCheck.Float;
        MultiLine = false;
        MaxTextLength = 255;
        MinTextLength = 1;
        ForbiddenChars = "\r\n";
    }

    #endregion

    #region Properties

    public static Format Instance => AllFormats[Keyname] ?? throw Develop.DebugError("Fehlerhafter Instanzname");

    #endregion
}