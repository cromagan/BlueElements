// Licensed under MIT; see License.md for disclaimer, details, and extended user conditions.

using Formats = BlueBasics.Classes.Formats;

namespace BlueTable.ColumnFormats;

public class BitColumnFormat : ColumnFormat {

    #region Constructors

    public BitColumnFormat() : base(Formats.BitFormat.Instance) {
        Align = AlignmentHorizontal.Zentriert;
        SortType = SortierTyp.Original_String;
        DoOpticalTranslation = TranslationType.Original_Anzeigen;
        AfterEditQuickSortRemoveDouble = false;
        ScriptType = ScriptType.Bool;
        ControlStrategy = "Listbox";
        EditableWithTextInput = false;
        DropDownItems = new(["+", "-"]);
        ShowValuesOfOtherCellsInDropdown = true;
        DefaultRenderer = "Bool";
        RendererSettings = "{ClassId=\"Bool\"}";
    }

    #endregion

    #region Properties

    public static ColumnFormat Instance => AllFormats[Formats.BitFormat.Keyname] ?? throw Develop.DebugError("Fehlerhafter Instanzname");

    #endregion
}