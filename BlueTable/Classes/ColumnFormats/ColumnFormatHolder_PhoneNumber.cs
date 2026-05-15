// Licensed under AGPL-3.0; see License.md for disclaimer and details.

namespace BlueTable.Classes;

public class ColumnFormatHolder_PhoneNumber : ColumnFormatHolder {

    #region Constructors

    public ColumnFormatHolder_PhoneNumber() : base(FormatHolder_PhoneNumber.Instance) {
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
        RendererSettings = "{ClassId=\"TextOneLine\", Style=\"Windows 11\"}";
    }

    #endregion

    #region Properties

    public static ColumnFormatHolder Instance => AllFormats["PhoneNumber"] ?? throw Develop.DebugError("Fehlerhafter Instanzname");

    #endregion
}