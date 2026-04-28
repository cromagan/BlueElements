// Licensed under AGPL-3.0; see License.md for disclaimer and details.

using BlueBasics.Classes;
using BlueTable.Enums;
using System.Collections.ObjectModel;

namespace BlueTable.Classes;

public class ColumnFormatHolder_TextOptions : ColumnFormatHolder {

    #region Constructors

    public ColumnFormatHolder_TextOptions() : base(FormatHolder.Text) {
        KeyName = "Items Select";
        Align = AlignmentHorizontal.Links;
        SortType = SortierTyp.Sprachneutral_String;
        DoOpticalTranslation = TranslationType.Übersetzen;
        AfterEditQuickSortRemoveDouble = true;
        ScriptType = ScriptType.String;
        DropdownDeselectAllAllowed = true;
        EditableWithDropdown = true;
        EditableWithTextInput = true;
        DropDownItems = new(Array.Empty<string>());
        ShowValuesOfOtherCellsInDropdown = true;
        MultiLine = true;
    }

    #endregion
}
