// Licensed under AGPL-3.0; see License.md for disclaimer and details.

namespace BlueTable.Classes;

public class ColumnFormatHolderTextMultiline : ColumnFormatHolder {

    #region Constructors

    public ColumnFormatHolderTextMultiline() : base(FormatHolderTextMultiline.Instance) {
        Align = AlignmentHorizontal.Links;
        MultiLine = true;
        SortType = SortierTyp.Sprachneutral_String;
        DoOpticalTranslation = TranslationType.Original_Anzeigen;
        AfterEditQuickSortRemoveDouble = false;
        ScriptType = ScriptType.String;
        EditableWithDropdown = false;
        EditableWithTextInput = true;
        DropDownItems = new(Array.Empty<string>());
        ShowValuesOfOtherCellsInDropdown = false;
        DefaultRenderer = "ImageAndText";
        RendererSettings = "{ClassId=\"ImageAndText\", ShowPic=-, ShowText=+}";
    }

    #endregion

    #region Properties

    public static ColumnFormatHolder Instance => AllFormats[FormatHolderTextMultiline.Keyname] ?? throw Develop.DebugError("Fehlerhafter Instanzname");

    #endregion
}