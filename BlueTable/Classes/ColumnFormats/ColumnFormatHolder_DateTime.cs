// Licensed under AGPL-3.0; see License.md for disclaimer and details.

namespace BlueTable.Classes;

public class ColumnFormatHolder_DateTime : ColumnFormatHolder {

    #region Constructors

    public ColumnFormatHolder_DateTime() : base(FormatHolder_DateTime.Instance) {
        Align = AlignmentHorizontal.Links;
        SortType = SortierTyp.Datum_Uhrzeit;
        DoOpticalTranslation = TranslationType.Datum;
        AfterEditQuickSortRemoveDouble = false;
        ScriptType = ScriptType.String;
        DropdownDeselectAllAllowed = false;
        EditableWithDropdown = false;
        EditableWithTextInput = true;
        DropDownItems = new(Array.Empty<string>());
        ShowValuesOfOtherCellsInDropdown = false;
        DefaultRenderer = "DateTime";
        RendererSettings = "{ClassId=\"DateTime\", UTCToLocal=+, ShowSymbol=+}";
    }

    #endregion

    #region Properties

    public static ColumnFormatHolder Instance => AllFormats[FormatHolder_DateTime._keyname] ?? throw Develop.DebugError("Fehlerhafter Instanzname");

    #endregion
}