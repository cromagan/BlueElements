// Licensed under AGPL-3.0; see License.md for disclaimer and details.

namespace BlueTable.Classes;

public class ColumnFormatHolder_LongOnlyPositive : ColumnFormatHolder {

    #region Constructors

    public ColumnFormatHolder_LongOnlyPositive() : base(FormatHolder_LongOnlyPositive.Instance) {
        Align = AlignmentHorizontal.Rechts;
        SortType = SortierTyp.ZahlenwertInt;
        DoOpticalTranslation = TranslationType.Zahl;
        AfterEditQuickSortRemoveDouble = false;
        ScriptType = ScriptType.Numeral;
        DropdownDeselectAllAllowed = false;
        EditableWithDropdown = false;
        EditableWithTextInput = true;
        DropDownItems = new(Array.Empty<string>());
        ShowValuesOfOtherCellsInDropdown = false;
        DefaultRenderer = "Number";
        RendererSettings = "{ClassId=\"Number\", Separator=+, DecimalPlaces=0}";
    }

    #endregion

    #region Properties

    public static ColumnFormatHolder Instance => AllFormats[FormatHolder_LongOnlyPositive._keyname] ?? throw Develop.DebugError("Fehlerhafter Instanzname");

    #endregion
}