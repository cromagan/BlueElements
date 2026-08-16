// Licensed under AGPL-3.0; see License.md for disclaimer and details.

using Formats = BlueBasics.Classes.Formats;

namespace BlueTable.ColumnFormats;

public class ColorColumnFormat : ColumnFormat {

    #region Constructors

    public ColorColumnFormat() : base(Formats.ColorFormat.Instance) {
        Align = AlignmentHorizontal.Rechts;
        SortType = SortierTyp.Original_String;
        DoOpticalTranslation = TranslationType.Original_Anzeigen;
        AfterEditQuickSortRemoveDouble = false;
        ScriptType = ScriptType.String;
        ControlStrategy = "Color";
        EditableWithTextInput = true;
        DropDownItems = new(Array.Empty<string>());
        ShowValuesOfOtherCellsInDropdown = true;
        DefaultRenderer = "Color";
        RendererSettings = "{ClassId=\"Color\", ShowSymbol=+, ShowHex=+, ShowName=+}";
    }

    #endregion

    #region Properties

    public static ColumnFormat Instance => AllFormats[Formats.ColorFormat.Keyname] ?? throw Develop.DebugError("Fehlerhafter Instanzname");

    #endregion
}