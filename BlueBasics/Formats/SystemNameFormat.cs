// Licensed under AGPL-3.0; see License.md for disclaimer and details.

using BlueBasics.Formats;

namespace BlueBasics.Classes.Formats;

public class SystemNameFormat : Format {

    #region Fields

    public static readonly string Keyname = "SystemName";

    #endregion

    #region Constructors

    public SystemNameFormat() : base(Keyname, QuickImage.Get(ImageCode.Variable, 16)) {
        AllowedChars = Char_AZ + Char_az + Char_Numerals + "_";
        RegexCheck = @"^[A-Za-z]\S*[A-Za-z0-9]$";
        AdditionalFormatCheck = AdditionalCheck.None;
        MultiLine = false;
        MaxTextLength = 128;
        MinTextLength = 2;
        ForbiddenChars = "\r\n";
        QuickInfo = "Werte, wie für eine System-Variabel. Beispiel: WERT_12";
    }

    #endregion

    #region Properties

    public static Format Instance => AllFormats[Keyname] ?? throw Develop.DebugError("Fehlerhafter Instanzname");

    #endregion

    #region Methods

    public static string MakeValid(string name) {
        var tmp = name.RemoveChars(Char_PfadSonderZeichen); // sonst stürzt FileNameWithoutSuffix ab

        tmp = tmp.Trim().FileNameWithoutSuffix().Replace(' ', '_').Replace('-', '_');
        tmp = tmp.StarkeVereinfachung("_", false).ToUpperInvariant();

        while (tmp.Contains("__")) {
            tmp = tmp.Replace("__", "_");
        }

        return tmp;
    }

    #endregion
}