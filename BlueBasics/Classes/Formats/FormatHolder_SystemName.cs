// Licensed under AGPL-3.0; see License.md for disclaimer and details.

namespace BlueBasics.Classes;

public class FormatHolder_SystemName : FormatHolder {

    #region Fields

    public static readonly string _keyname = "SystemName";

    #endregion

    #region Constructors

    public FormatHolder_SystemName() : base(_keyname, QuickImage.Get(ImageCode.Variable, 16)) {
        AllowedChars = Constants.Char_AZ + Constants.Char_az + Constants.Char_Numerals + "_";
        RegexCheck = @"^[A-Za-z]\S*[A-Za-z0-9]$";
        TextFormatingAllowed = false;
        AdditionalFormatCheck = AdditionalCheck.None;
        SpellCheckingEnabled = false;
        MultiLine = false;
        MaxTextLength = 128;
        QuickInfo = "Werte, wie für eine System-Variabel. Beispiel: WERT_12";
    }

    #endregion

    #region Properties

    public static FormatHolder Instance => AllFormats[_keyname] ?? throw Develop.DebugError("Fehlerhafter Instanzname");

    #endregion

    #region Methods

    public static string MakeValid(string name) {
        var tmp = name.RemoveChars(Constants.Char_PfadSonderZeichen); // sonst stürzt FileNameWithoutSuffix ab

        tmp = tmp.Trim().FileNameWithoutSuffix().Replace(' ', '_').Replace('-', '_');
        tmp = tmp.StarkeVereinfachung("_", false).ToUpperInvariant();

        while (tmp.Contains("__")) {
            tmp = tmp.Replace("__", "_");
        }

        return tmp;
    }

    #endregion
}