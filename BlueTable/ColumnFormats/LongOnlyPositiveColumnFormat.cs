// Licensed under AGPL-3.0; see License.md for disclaimer and details.

using Formats = BlueBasics.Classes.Formats;

namespace BlueTable.ColumnFormats;

public class LongOnlyPositiveColumnFormat : ColumnFormat {

    #region Constructors

    public LongOnlyPositiveColumnFormat() : base(Formats.LongOnlyPositiveFormat.Instance) {
        Align = AlignmentHorizontal.Rechts;
        SortType = SortierTyp.ZahlenwertInt;
        DoOpticalTranslation = TranslationType.Zahl;
        AfterEditQuickSortRemoveDouble = false;
        ScriptType = ScriptType.Numeral;
        ControlStrategy = "Textbox";
        EditableWithTextInput = true;
        DropDownItems = new(Array.Empty<string>());
        ShowValuesOfOtherCellsInDropdown = false;
        DefaultRenderer = "Number";
        RendererSettings = "{ClassId=\"Number\", Separator=+, DecimalPlaces=0}";
    }

    #endregion

    #region Properties

    public static ColumnFormat Instance => AllFormats[Formats.LongOnlyPositiveFormat.Keyname] ?? throw Develop.DebugError("Fehlerhafter Instanzname");

    #endregion
}