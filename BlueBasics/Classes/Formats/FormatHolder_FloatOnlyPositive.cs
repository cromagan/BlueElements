// Licensed under AGPL-3.0; see License.md for disclaimer and details.

namespace BlueBasics.Classes;

public class FormatHolder_FloatOnlyPositive : FormatHolder {

    public static readonly string Keyname = "FloatOnlyPositive";

    #region Constructors

    public FormatHolder_FloatOnlyPositive() : base(Keyname, QuickImage.Get(ImageCode.Gleitkommazahl, 16)) {
        //https://regex101.com/r/onr0NZ/1
        RegexCheck = @"(^([1-9]\d*)|^0)([.|,]\d*[1-9])?$";
        AllowedChars = Constants.Char_Numerals + ".,";
        TextFormatingAllowed = false;
        AdditionalFormatCheck = AdditionalCheck.Float;
        SpellCheckingEnabled = false;
        MultiLine = false;
        MaxTextLength = 255;
    }

    #endregion

    #region Properties

    public static FormatHolder Instance => AllFormats[Keyname] ?? throw Develop.DebugError("Fehlerhafter Instanzname");

    #endregion
}