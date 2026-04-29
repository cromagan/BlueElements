// Licensed under AGPL-3.0; see License.md for disclaimer and details.

namespace BlueTable.Classes;

public class ColumnFormatHolder_BildCode : ColumnFormatHolder {

    #region Constructors

    public ColumnFormatHolder_BildCode() : base(FormatHolder_Text.Instance) {
        KeyName = "ImageCode";
        Align = AlignmentHorizontal.Links;
        SortType = SortierTyp.Original_String;
        DoOpticalTranslation = TranslationType.Original_Anzeigen;
        AfterEditQuickSortRemoveDouble = false;
        ScriptType = ScriptType.String;
        DropdownDeselectAllAllowed = true;
        EditableWithDropdown = true;
        EditableWithTextInput = true;
        DropDownItems = new(Array.Empty<string>());
        ShowValuesOfOtherCellsInDropdown = true;
        DefaultRenderer = "ImageAndText";
        RendererSettings = "{ClassId=\"ImageAndText\", Style=\"Windows 11\", ShowPic=+, ShowText=-, ImageWidth=16, ImageHeight=16}";
    }

    #endregion

    #region Properties

    public static ColumnFormatHolder Instance => AllFormats.GetByKey("ImageCode") ?? throw Develop.DebugError("Fehlerhafter Instanzname");

    #endregion
}