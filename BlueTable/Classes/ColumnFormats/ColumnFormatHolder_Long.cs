// Licensed under AGPL-3.0; see License.md for disclaimer and details.

using BlueBasics.Classes;
using BlueTable.Enums;
using System.Collections.ObjectModel;

namespace BlueTable.Classes;

public class ColumnFormatHolder_Long : ColumnFormatHolder {

    #region Constructors

    public ColumnFormatHolder_Long() : base(FormatHolder.Long) {
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
        RendererSettings = "{ClassId=\"Number\", Style=\"Windows 11\", Separator=+, DecimalPlaces=0}";
    }

    #endregion
}
