// Licensed under AGPL-3.0; see License.md for disclaimer and details.

using Formats = BlueBasics.Classes.Formats;

namespace BlueTable.ColumnFormats;

public class FloatOnlyPositiveColumnFormat : ColumnFormat {

    #region Constructors

    public FloatOnlyPositiveColumnFormat() : base(Formats.FloatOnlyPositiveFormat.Instance) {
        Align = AlignmentHorizontal.Rechts;
        SortType = SortierTyp.ZahlenwertFloat;
        DoOpticalTranslation = TranslationType.Zahl;
        AfterEditQuickSortRemoveDouble = false;
        ScriptType = ScriptType.Numeral;
        ControlStrategy = "Texbox";
        EditableWithTextInput = true;
        DropDownItems = new(Array.Empty<string>());
        ShowValuesOfOtherCellsInDropdown = false;
        DefaultRenderer = "Number";
        RendererSettings = "{ClassId=\"Number\", Separator=+, DecimalPlaces=2}";
    }

    #endregion

    #region Properties

    public static ColumnFormat Instance => AllFormats[Formats.FloatOnlyPositiveFormat.Keyname] ?? throw Develop.DebugError("Fehlerhafter Instanzname");

    #endregion
}