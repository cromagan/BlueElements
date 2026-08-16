// Licensed under AGPL-3.0; see License.md for disclaimer and details.

using Formats = BlueBasics.Classes.Formats;

namespace BlueTable.ColumnFormats;

public class ImageAndTextColumnFormat : ColumnFormat {

    #region Fields

    private static readonly string _keyname = "ImageCode";

    #endregion

    #region Constructors

    public ImageAndTextColumnFormat() : base(Formats.TextFormat.Instance) {
        KeyName = _keyname;
        Align = AlignmentHorizontal.Links;
        SortType = SortierTyp.Original_String;
        DoOpticalTranslation = TranslationType.Original_Anzeigen;
        AfterEditQuickSortRemoveDouble = false;
        ScriptType = ScriptType.String;
        ControlStrategy = "Combobox";
        EditableWithTextInput = true;
        DropDownItems = new(Array.Empty<string>());
        ShowValuesOfOtherCellsInDropdown = true;
        DefaultRenderer = "ImageAndText";
        RendererSettings = "{ClassId=\"ImageAndText\", ShowPic=+, ShowText=-, ImageWidth=16, ImageHeight=16}";
    }

    #endregion

    #region Properties

    public static ColumnFormat Instance => AllFormats[_keyname] ?? throw Develop.DebugError("Fehlerhafter Instanzname");

    #endregion
}