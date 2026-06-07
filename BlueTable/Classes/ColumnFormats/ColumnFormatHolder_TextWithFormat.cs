// Licensed under AGPL-3.0; see License.md for disclaimer and details.

namespace BlueTable.Classes;

public class ColumnFormatHolder_TextWithFormat : ColumnFormatHolder {

    #region Constructors

    public ColumnFormatHolder_TextWithFormat() : base(FormatHolder_TextWithFormat.Instance) {
        Align = AlignmentHorizontal.Links;
        SortType = SortierTyp.Sprachneutral_String;
        DoOpticalTranslation = TranslationType.Original_Anzeigen;
        AfterEditQuickSortRemoveDouble = false;
        ScriptType = ScriptType.String;
        DropdownDeselectAllAllowed = false;
        EditableWithDropdown = false;
        EditableWithTextInput = true;
        DropDownItems = new(Array.Empty<string>());
        ShowValuesOfOtherCellsInDropdown = false;
        DefaultRenderer = "RichText";
        RendererSettings = "{ClassId=\"RichText\"}";
    }

    #endregion

    #region Properties

    public static ColumnFormatHolder Instance => AllFormats[FormatHolder_TextWithFormat.Keyname] ?? throw Develop.DebugError("Fehlerhafter Instanzname");

    #endregion
}