// Licensed under AGPL-3.0; see License.md for disclaimer and details.

using BlueBasics.Classes;
using BlueTable.Enums;
using System.Collections.ObjectModel;

namespace BlueTable.Classes;

public class ColumnFormatHolder_Bit : ColumnFormatHolder {

    #region Constructors

    public ColumnFormatHolder_Bit() : base(FormatHolder_Bit.Instance) {
        Align = AlignmentHorizontal.Zentriert;
        SortType = SortierTyp.Original_String;
        DoOpticalTranslation = TranslationType.Original_Anzeigen;
        AfterEditQuickSortRemoveDouble = false;
        ScriptType = ScriptType.Bool;
        DropdownDeselectAllAllowed = false;
        EditableWithDropdown = true;
        EditableWithTextInput = false;
        DropDownItems = new(["+", "-"]);
        ShowValuesOfOtherCellsInDropdown = true;
        DefaultRenderer = "ImageAndText";
        RendererSettings = "{ClassId=\"ImageAndText\", Style=\"Windows 11\", ShowPic=+, ShowText=-, ImageReplace=+[G]Häkchen|o[G]Kreis2|-[G]Kreuz, ImageWidth=16, ImageHeight=16}";
    }

    #endregion

    #region Properties

    public static ColumnFormatHolder Instance => AllFormats.GetByKey("Bit") ?? throw Develop.DebugError("Fehlerhafter Instanzname");

    #endregion
}