// Licensed under AGPL-3.0; see License.md for disclaimer and details.

namespace BlueTable.Classes;

public class ColumnFormatHolder_TextMultiline : ColumnFormatHolder {

    #region Constructors

    public ColumnFormatHolder_TextMultiline() : base(FormatHolder_TextMultiline.Instance) {
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
        DefaultRenderer = "ImageAndText";
        RendererSettings = "{ClassId=\"ImageAndText\", ShowPic=-, ShowText=+}";
    }

    #endregion

    #region Properties

    public static ColumnFormatHolder Instance => AllFormats[FormatHolder_TextMultiline.Keyname] ?? throw Develop.DebugError("Fehlerhafter Instanzname");

    #endregion
}