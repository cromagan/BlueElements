// Licensed under AGPL-3.0; see License.md for disclaimer and details.

using BlueBasics.Classes;
using BlueTable.Enums;
using System.Collections.ObjectModel;

namespace BlueTable.Classes;

public class ColumnFormatHolder_FloatPositive : ColumnFormatHolder {

    #region Constructors

    public ColumnFormatHolder_FloatPositive() : base(FormatHolder_FloatPositive.Instance) {
        Align = AlignmentHorizontal.Rechts;
        SortType = SortierTyp.ZahlenwertFloat;
        DoOpticalTranslation = TranslationType.Zahl;
        AfterEditQuickSortRemoveDouble = false;
        ScriptType = ScriptType.Numeral;
        DropdownDeselectAllAllowed = false;
        EditableWithDropdown = false;
        EditableWithTextInput = true;
        DropDownItems = new(Array.Empty<string>());
        ShowValuesOfOtherCellsInDropdown = false;
        DefaultRenderer = "Number";
        RendererSettings = "{ClassId=\"Number\", Style=\"Windows 11\", Separator=+, DecimalPlaces=2}";
    }

    #endregion

    #region Properties

    public static ColumnFormatHolder Instance => AllFormats["Float only Positive"] ?? throw Develop.DebugError("Fehlerhafter Instanzname");

    #endregion
}