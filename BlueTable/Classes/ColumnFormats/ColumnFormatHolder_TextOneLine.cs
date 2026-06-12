// Licensed under AGPL-3.0; see License.md for disclaimer and details.

namespace BlueTable.Classes;

public class ColumnFormatHolder_TextOneLine : ColumnFormatHolder {

    #region Fields

    private static readonly string _keyname = "TextOneLine";

    #endregion

    #region Constructors

    public ColumnFormatHolder_TextOneLine() : base(FormatHolder_Text.Instance) {
        KeyName = _keyname;
        Align = AlignmentHorizontal.Links;
        SortType = SortierTyp.Sprachneutral_String;
        DoOpticalTranslation = TranslationType.Original_Anzeigen;
        AfterEditQuickSortRemoveDouble = false;
        ScriptType = ScriptType.String;
        ValueRequired = false;
        EditableWithDropdown = false;
        EditableWithTextInput = true;
        DropDownItems = new(Array.Empty<string>());
        ShowValuesOfOtherCellsInDropdown = false;
        DefaultRenderer = "TextOneLine";
        RendererSettings = "{ClassId=\"TextOneLine\"}";
    }

    #endregion

    #region Properties

    public static ColumnFormatHolder Instance => AllFormats[_keyname] ?? throw Develop.DebugError("Fehlerhafter Instanzname");

    #endregion
}