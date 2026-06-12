// Licensed under AGPL-3.0; see License.md for disclaimer and details.

namespace BlueTable.Classes;

public class ColumnFormatHolder_ItemsSelect : ColumnFormatHolder {

    #region Fields

    private static readonly string _keyname = "ItemsSelect";

    #endregion

    #region Constructors

    public ColumnFormatHolder_ItemsSelect() : base(FormatHolder_Text.Instance) {
        KeyName = _keyname;
        Align = AlignmentHorizontal.Links;
        SortType = SortierTyp.Sprachneutral_String;
        DoOpticalTranslation = TranslationType.Übersetzen;
        AfterEditQuickSortRemoveDouble = true;
        ScriptType = ScriptType.String;
        ValueRequired = true;
        EditableWithDropdown = true;
        EditableWithTextInput = true;
        DropDownItems = new(Array.Empty<string>());
        ShowValuesOfOtherCellsInDropdown = true;
        MultiLine = true;
        DefaultRenderer = "ImageAndText";
        RendererSettings = "{ClassId=\"ImageAndText\", ShowPic=-, ShowText=+}";
    }

    #endregion

    #region Properties

    public static ColumnFormatHolder Instance => AllFormats[_keyname] ?? throw Develop.DebugError("Fehlerhafter Instanzname");

    #endregion
}