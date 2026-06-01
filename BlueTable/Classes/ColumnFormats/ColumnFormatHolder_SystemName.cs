// Licensed under AGPL-3.0; see License.md for disclaimer and details.

namespace BlueTable.Classes;

public class ColumnFormatHolder_Systemname : ColumnFormatHolder {

    private static readonly string _keyname = "Systemname";

    #region Constructors

    public ColumnFormatHolder_Systemname() : base(FormatHolder_SystemName.Instance) {
        Align = AlignmentHorizontal.Links;
        SortType = SortierTyp.Original_String;
        DoOpticalTranslation = TranslationType.Original_Anzeigen;
        AfterEditQuickSortRemoveDouble = false;
        ScriptType = ScriptType.String;
        DropdownDeselectAllAllowed = false;
        EditableWithDropdown = false;
        EditableWithTextInput = true;
        DropDownItems = new(Array.Empty<string>());
        ShowValuesOfOtherCellsInDropdown = false;
        DefaultRenderer = "TextOneLine";
        RendererSettings = "{ClassId=\"TextOneLine\"}";
    }

    #endregion

    #region Properties

    public static ColumnFormatHolder Instance => AllFormats[_keyname] ?? throw Develop.DebugError("Fehlerhafter Instanzname");

    #endregion
}