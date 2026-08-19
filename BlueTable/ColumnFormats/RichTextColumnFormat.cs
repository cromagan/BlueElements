// Licensed under AGPL-3.0; see License.md for disclaimer and details.

using Formats = BlueBasics.Classes.Formats;

namespace BlueTable.ColumnFormats;

public class RichTextColumnFormat : ColumnFormat {

    #region Constructors

    public RichTextColumnFormat() : base(Formats.TextWithFormatFormat.Instance) {
        Align = AlignmentHorizontal.Links;
        SortType = SortierTyp.Sprachneutral_String;
        DoOpticalTranslation = TranslationType.Original_Anzeigen;
        AfterEditQuickSortRemoveDouble = false;
        ScriptType = ScriptType.String;
        ControlStrategy = "Texbox";
        ControlStrategyParameter.Set("spellcheckingenabled", true);
        ControlStrategyParameter.Set("textformatingallowed", true);
        EditableWithTextInput = true;
        DropDownItems = new(Array.Empty<string>());
        ShowValuesOfOtherCellsInDropdown = false;
        DefaultRenderer = "RichText";
        RendererSettings = "{ClassId=\"RichText\"}";
    }

    #endregion

    #region Properties

    public static ColumnFormat Instance => AllFormats[Formats.TextWithFormatFormat.Keyname] ?? throw Develop.DebugError("Fehlerhafter Instanzname");

    #endregion
}