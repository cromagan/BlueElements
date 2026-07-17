// Licensed under AGPL-3.0; see License.md for disclaimer and details.

namespace BlueTable.Classes;

public class ColumnFormatHolder_Bit : ColumnFormatHolder {

    #region Constructors

    public ColumnFormatHolder_Bit() : base(FormatHolder_Bit.Instance) {
        Align = AlignmentHorizontal.Zentriert;
        SortType = SortierTyp.Original_String;
        DoOpticalTranslation = TranslationType.Original_Anzeigen;
        AfterEditQuickSortRemoveDouble = false;
        ScriptType = ScriptType.Bool;
        ValueRequired = true;
        EditableWithDropdown = true;
        EditableWithTextInput = false;
        DropDownItems = new(["+", "-"]);
        ShowValuesOfOtherCellsInDropdown = true;
        DefaultRenderer = "Bool";
        RendererSettings = "{ClassId=\"Bool\"}";
    }

    #endregion

    #region Properties

    public static ColumnFormatHolder Instance => AllFormats[FormatHolder_Bit.Keyname] ?? throw Develop.DebugError("Fehlerhafter Instanzname");

    #endregion
}