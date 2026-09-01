// Licensed under AGPL-3.0; see License.md for disclaimer and details.

using Formats = BlueBasics.Classes.Formats;

namespace BlueTable.ColumnFormats;

public class EmailColumnFormat : ColumnFormat {

    #region Constructors

    public EmailColumnFormat() : base(Formats.EMailFormat.Instance) {
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

    public static ColumnFormat Instance => AllFormats[Formats.EMailFormat.Keyname] ?? throw Develop.DebugError("Fehlerhafter Instanzname");

    #endregion
}