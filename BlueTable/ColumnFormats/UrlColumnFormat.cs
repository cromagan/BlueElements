// Licensed under MIT; see License.md for disclaimer, details, and extended user conditions.

using Formats = BlueBasics.Classes.Formats;

namespace BlueTable.ColumnFormats;

public class UrlColumnFormat : ColumnFormat {

    #region Constructors

    public UrlColumnFormat() : base(Formats.UrlFormat.Instance) {
        Align = AlignmentHorizontal.Links;
        SortType = SortierTyp.Original_String;
        DoOpticalTranslation = TranslationType.Original_Anzeigen;
        AfterEditQuickSortRemoveDouble = false;
        ScriptType = ScriptType.String;
        ControlStrategy = "Textbox";
        EditableWithTextInput = true;
        DropDownItems = new(Array.Empty<string>());
        ShowValuesOfOtherCellsInDropdown = false;
        DefaultRenderer = "TextOneLine";
        RendererSettings = "{ClassId=\"TextOneLine\"}";
    }

    #endregion

    #region Properties

    public static ColumnFormat Instance => AllFormats[Formats.UrlFormat.Keyname] ?? throw Develop.DebugError("Fehlerhafter Instanzname");

    #endregion
}