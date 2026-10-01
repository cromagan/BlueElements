// Licensed under MIT; see License.md for disclaimer, details, and extended user conditions.

using Formats = BlueBasics.Classes.Formats;

namespace BlueTable.ColumnFormats;

public class ItemsMultiSelectColumnFormat : ColumnFormat {

    #region Fields

    private static readonly string _keyname = "ItemsMultiSelect";

    #endregion

    #region Constructors

    public ItemsMultiSelectColumnFormat() : base(Formats.TextFormat.Instance) {
        KeyName = _keyname;
        Align = AlignmentHorizontal.Links;
        SortType = SortierTyp.Sprachneutral_String;
        DoOpticalTranslation = TranslationType.Übersetzen;
        AfterEditQuickSortRemoveDouble = true;
        ScriptType = ScriptType.String;
        ControlStrategy = "Listbox";
        EditableWithTextInput = true;
        DropDownItems = new(Array.Empty<string>());
        ShowValuesOfOtherCellsInDropdown = true;
        MultiLine = true;
        DefaultRenderer = "ImageAndText";
        RendererSettings = "{ClassId=\"ImageAndText\", ShowPic=-, ShowText=+}";
    }

    #endregion

    #region Properties

    public static ColumnFormat Instance => AllFormats[_keyname] ?? throw Develop.DebugError("Fehlerhafter Instanzname");

    #endregion
}