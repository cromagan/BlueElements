// Licensed under AGPL-3.0; see License.md for disclaimer and details.

namespace BlueTable.Classes;

public class ColumnFormatHolder_ImageCode : ColumnFormatHolder {

    private static readonly string _keyname = "ImageCode";

    #region Constructors

    public ColumnFormatHolder_ImageCode() : base(FormatHolder_Text.Instance) {
        KeyName = _keyname;
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
        RendererSettings = "{ClassId=\"ImageAndText\", ShowPic=+, ShowText=-, ImageWidth=16, ImageHeight=16}";
    }

    #endregion

    #region Properties

    public static ColumnFormatHolder Instance => AllFormats[_keyname] ?? throw Develop.DebugError("Fehlerhafter Instanzname");

    #endregion
}