// Licensed under AGPL-3.0; see License.md for disclaimer and details.

using BlueBasics.Classes;
using BlueTable.Enums;
using System.Collections.ObjectModel;

namespace BlueTable.Classes;

public class ColumnFormatHolder_Color : ColumnFormatHolder {

    #region Constructors

    public ColumnFormatHolder_Color() : base(FormatHolder_Color.Instance) {
        Align = AlignmentHorizontal.Rechts;
        SortType = SortierTyp.Original_String;
        DoOpticalTranslation = TranslationType.Original_Anzeigen;
        AfterEditQuickSortRemoveDouble = false;
        ScriptType = ScriptType.String;
        DropdownDeselectAllAllowed = false;
        EditableWithDropdown = true;
        EditableWithTextInput = true;
        DropDownItems = new(Array.Empty<string>());
        ShowValuesOfOtherCellsInDropdown = true;
        DefaultRenderer = "Color";
        RendererSettings = "{ClassId=\"Color\", ShowSymbol=+, ShowHex=+, ShowName=+}";
    }

    #endregion

    #region Properties

    public static ColumnFormatHolder Instance => AllFormats.GetByKey("Color") ?? throw Develop.DebugError("Fehlerhafter Instanzname");

    #endregion
}