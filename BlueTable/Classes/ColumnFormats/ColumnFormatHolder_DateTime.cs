// Licensed under AGPL-3.0; see License.md for disclaimer and details.

namespace BlueTable.Classes;

public class ColumnFormatHolder_DateTime : ColumnFormatHolder {

    private static readonly string _keyname = "DateTime";

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
        RendererSettings = "{ClassId=\"DateTime\", Style=\"Windows 11\", UTCToLocal=+, ShowSymbol=+}";
    }

    #endregion

    #region Properties

    public static ColumnFormatHolder Instance => AllFormats[_keyname] ?? throw Develop.DebugError("Fehlerhafter Instanzname");

    #endregion
}