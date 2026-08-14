// Licensed under AGPL-3.0; see License.md for disclaimer and details.

using Formats = BlueBasics.Classes.Formats;

namespace BlueTable.ColumnFormats;

public class DateTimeColumnFormat : Classes.ColumnFormat {

    #region Constructors

    public DateTimeColumnFormat() : base(Formats.DateTimeFormat.Instance) {
        Align = AlignmentHorizontal.Links;
        SortType = SortierTyp.Datum_Uhrzeit;
        DoOpticalTranslation = TranslationType.Datum;
        AfterEditQuickSortRemoveDouble = false;
        ScriptType = ScriptType.String;
        EditableWithDropdown = false;
        EditableWithTextInput = true;
        DropDownItems = new(Array.Empty<string>());
        ShowValuesOfOtherCellsInDropdown = false;
        DefaultRenderer = "DateTime";
        RendererSettings = "{ClassId=\"DateTime\", UTCToLocal=+, ShowSymbol=+}";
    }

    #endregion

    #region Properties

    public static Classes.ColumnFormat Instance => AllFormats[Formats.DateTimeFormat.Keyname] ?? throw Develop.DebugError("Fehlerhafter Instanzname");

    #endregion
}