// Licensed under AGPL-3.0; see License.md for disclaimer and details.

using BlueBasics.Classes;
using BlueBasics.ClassesStatic;
using BlueBasics.Enums;
using BlueBasics.Interfaces;
using BlueTable.AdditionalScriptMethods;
using BlueTable.Enums;
using BlueTable.Interfaces;
using System.Collections.ObjectModel;

namespace BlueTable.Classes;

public class ColumnFormatHolder : IColumnInputFormat, IReadableTextWithKey {

    #region Fields

    public static readonly List<ColumnFormatHolder> AllFormats = [];

    public static readonly ColumnFormatHolder BildCode = new(FormatHolder.Text) {
        Align = AlignmentHorizontal.Links,
        SortType = SortierTyp.Original_String,
        DoOpticalTranslation = TranslationType.Original_Anzeigen,
        AfterEditQuickSortRemoveDouble = false,
        ScriptType = ScriptType.String,
        DropdownDeselectAllAllowed = true,
        EditableWithDropdown = true,
        EditableWithTextInput = true,
        DropDownItems = new(Array.Empty<string>()),
        ShowValuesOfOtherCellsInDropdown = true,
        DefaultRenderer = "ImageAndText",
        RendererSettings = "{ClassId=\"ImageAndText\", Style=\"Windows 11\", ShowPic=+, ShowText=-, ImageWidth=16, ImageHeight=16}"
    };

    public static readonly ColumnFormatHolder Bit = new(FormatHolder.Bit) {
        Align = AlignmentHorizontal.Zentriert,
        SortType = SortierTyp.Original_String,
        DoOpticalTranslation = TranslationType.Original_Anzeigen,
        AfterEditQuickSortRemoveDouble = false,
        ScriptType = ScriptType.Bool,
        DropdownDeselectAllAllowed = false,
        EditableWithDropdown = true,
        EditableWithTextInput = false,
        DropDownItems = new(["+", "-"]),
        ShowValuesOfOtherCellsInDropdown = true,
        DefaultRenderer = "ImageAndText",
        RendererSettings = "{ClassId=\"ImageAndText\", Style=\"Windows 11\", ShowPic=+, ShowText=-, ImageReplace=+[G]Häkchen|o[G]Kreis2|-[G]Kreuz, ImageWidth=16, ImageHeight=16}"
    };

    public static readonly ColumnFormatHolder Color = new(FormatHolder.Color) {
        Align = AlignmentHorizontal.Rechts,
        SortType = SortierTyp.Original_String,
        DoOpticalTranslation = TranslationType.Original_Anzeigen,
        AfterEditQuickSortRemoveDouble = false,
        ScriptType = ScriptType.String,
        DropdownDeselectAllAllowed = false,
        EditableWithDropdown = true,
        EditableWithTextInput = true,
        DropDownItems = new(Array.Empty<string>()),
        ShowValuesOfOtherCellsInDropdown = true,
        DefaultRenderer = "Color",
        RendererSettings = "{ClassId=\"Color\", ShowSymbol=+, ShowHex=+, ShowName=+}"
    };

    public static readonly ColumnFormatHolder Date = new(FormatHolder.Date) {
        Align = AlignmentHorizontal.Links,
        SortType = SortierTyp.Datum_Uhrzeit,
        DoOpticalTranslation = TranslationType.Datum,
        AfterEditQuickSortRemoveDouble = false,
        ScriptType = ScriptType.String,
        DropdownDeselectAllAllowed = false,
        EditableWithDropdown = false,
        EditableWithTextInput = true,
        DropDownItems = new(Array.Empty<string>()),
        ShowValuesOfOtherCellsInDropdown = false,
        DefaultRenderer = "TextOneLine",
        RendererSettings = "{ClassId=\"TextOneLine\", Style=\"Windows 11\"}"
    };

    public static readonly ColumnFormatHolder DateTime = new(FormatHolder.DateTime) {
        Align = AlignmentHorizontal.Links,
        SortType = SortierTyp.Datum_Uhrzeit,
        DoOpticalTranslation = TranslationType.Datum,
        AfterEditQuickSortRemoveDouble = false,
        ScriptType = ScriptType.String,
        DropdownDeselectAllAllowed = false,
        EditableWithDropdown = false,
        EditableWithTextInput = true,
        DropDownItems = new(Array.Empty<string>()),
        ShowValuesOfOtherCellsInDropdown = false,
        DefaultRenderer = "DateTime",
        RendererSettings = "{ClassId=\"DateTime\", Style=\"Windows 11\", UTCToLocal=+, ShowSymbol=+}"
    };

    public static readonly ColumnFormatHolder Email = new(FormatHolder.Email) {
        Align = AlignmentHorizontal.Links,
        SortType = SortierTyp.Original_String,
        DoOpticalTranslation = TranslationType.Original_Anzeigen,
        AfterEditQuickSortRemoveDouble = false,
        ScriptType = ScriptType.String,
        DropdownDeselectAllAllowed = false,
        EditableWithDropdown = false,
        EditableWithTextInput = true,
        DropDownItems = new(Array.Empty<string>()),
        ShowValuesOfOtherCellsInDropdown = false,
        DefaultRenderer = "TextOneLine",
        RendererSettings = "{ClassId=\"TextOneLine\", Style=\"Windows 11\"}"
    };

    public static readonly ColumnFormatHolder Filepath = new(FormatHolder.Filepath) {
        Align = AlignmentHorizontal.Links,
        SortType = SortierTyp.Original_String,
        DoOpticalTranslation = TranslationType.Original_Anzeigen,
        AfterEditQuickSortRemoveDouble = false,
        ScriptType = ScriptType.String,
        DropdownDeselectAllAllowed = false,
        EditableWithDropdown = false,
        EditableWithTextInput = true,
        DropDownItems = new(Array.Empty<string>()),
        ShowValuesOfOtherCellsInDropdown = false,
        DefaultRenderer = "TextOneLine",
        RendererSettings = "{ClassId=\"TextOneLine\", Style=\"Windows 11\"}"
    };

    public static readonly ColumnFormatHolder FilepathAndName = new(FormatHolder.FilepathAndName) {
        Align = AlignmentHorizontal.Links,
        SortType = SortierTyp.Original_String,
        DoOpticalTranslation = TranslationType.Original_Anzeigen,
        AfterEditQuickSortRemoveDouble = false,
        ScriptType = ScriptType.String,
        DropdownDeselectAllAllowed = false,
        EditableWithDropdown = false,
        EditableWithTextInput = true,
        DropDownItems = new(Array.Empty<string>()),
        ShowValuesOfOtherCellsInDropdown = false,
        DefaultRenderer = "TextOneLine",
        RendererSettings = "{ClassId=\"TextOneLine\", Style=\"Windows 11\"}"
    };

    public static readonly ColumnFormatHolder Float = new(FormatHolder.Float) {
        Align = AlignmentHorizontal.Rechts,
        SortType = SortierTyp.ZahlenwertFloat,
        DoOpticalTranslation = TranslationType.Zahl,
        AfterEditQuickSortRemoveDouble = false,
        ScriptType = ScriptType.Numeral,
        DropdownDeselectAllAllowed = false,
        EditableWithDropdown = false,
        EditableWithTextInput = true,
        DropDownItems = new(Array.Empty<string>()),
        ShowValuesOfOtherCellsInDropdown = false,
        DefaultRenderer = "Number",
        RendererSettings = "{ClassId=\"Number\", Style=\"Windows 11\", Separator=+, DecimalPlaces=2}"
    };

    public static readonly ColumnFormatHolder FloatPositive = new(FormatHolder.FloatPositive) {
        Align = AlignmentHorizontal.Rechts,
        SortType = SortierTyp.ZahlenwertFloat,
        DoOpticalTranslation = TranslationType.Zahl,
        AfterEditQuickSortRemoveDouble = false,
        ScriptType = ScriptType.Numeral,
        DropdownDeselectAllAllowed = false,
        EditableWithDropdown = false,
        EditableWithTextInput = true,
        DropDownItems = new(Array.Empty<string>()),
        ShowValuesOfOtherCellsInDropdown = false,
        DefaultRenderer = "Number",
        RendererSettings = "{ClassId=\"Number\", Style=\"Windows 11\", Separator=+, DecimalPlaces=2}"
    };

    public static readonly ColumnFormatHolder Long = new(FormatHolder.Long) {
        Align = AlignmentHorizontal.Rechts,
        SortType = SortierTyp.ZahlenwertInt,
        DoOpticalTranslation = TranslationType.Zahl,
        AfterEditQuickSortRemoveDouble = false,
        ScriptType = ScriptType.Numeral,
        DropdownDeselectAllAllowed = false,
        EditableWithDropdown = false,
        EditableWithTextInput = true,
        DropDownItems = new(Array.Empty<string>()),
        ShowValuesOfOtherCellsInDropdown = false,
        DefaultRenderer = "Number",
        RendererSettings = "{ClassId=\"Number\", Style=\"Windows 11\", Separator=+, DecimalPlaces=0}"
    };

    public static readonly ColumnFormatHolder LongPositive = new(FormatHolder.LongPositive) {
        Align = AlignmentHorizontal.Rechts,
        SortType = SortierTyp.ZahlenwertInt,
        DoOpticalTranslation = TranslationType.Zahl,
        AfterEditQuickSortRemoveDouble = false,
        ScriptType = ScriptType.Numeral,
        DropdownDeselectAllAllowed = false,
        EditableWithDropdown = false,
        EditableWithTextInput = true,
        DropDownItems = new(Array.Empty<string>()),
        ShowValuesOfOtherCellsInDropdown = false,
        DefaultRenderer = "Number",
        RendererSettings = "{ClassId=\"Number\", Style=\"Windows 11\", Separator=+, DecimalPlaces=0}"
    };

    public static readonly ColumnFormatHolder PhoneNumber = new(FormatHolder.PhoneNumber) {
        Align = AlignmentHorizontal.Links,
        SortType = SortierTyp.Original_String,
        DoOpticalTranslation = TranslationType.Original_Anzeigen,
        AfterEditQuickSortRemoveDouble = false,
        ScriptType = ScriptType.String,
        DropdownDeselectAllAllowed = false,
        EditableWithDropdown = false,
        EditableWithTextInput = true,
        DropDownItems = new(Array.Empty<string>()),
        ShowValuesOfOtherCellsInDropdown = false,
        DefaultRenderer = "TextOneLine",
        RendererSettings = "{ClassId=\"TextOneLine\", Style=\"Windows 11\"}"
    };

    public static readonly ColumnFormatHolder SystemName = new(FormatHolder.SystemName) {
        Align = AlignmentHorizontal.Links,
        SortType = SortierTyp.Original_String,
        DoOpticalTranslation = TranslationType.Original_Anzeigen,
        AfterEditQuickSortRemoveDouble = false,
        ScriptType = ScriptType.String,
        DropdownDeselectAllAllowed = false,
        EditableWithDropdown = false,
        EditableWithTextInput = true,
        DropDownItems = new(Array.Empty<string>()),
        ShowValuesOfOtherCellsInDropdown = false,
        DefaultRenderer = "TextOneLine",
        RendererSettings = "{ClassId=\"TextOneLine\", Style=\"Windows 11\"}"
    };

    public static readonly ColumnFormatHolder Text = new(FormatHolder.Text) {
        KeyName = "Text One Line",
        Align = AlignmentHorizontal.Links,
        SortType = SortierTyp.Sprachneutral_String,
        DoOpticalTranslation = TranslationType.Original_Anzeigen,
        AfterEditQuickSortRemoveDouble = false,
        ScriptType = ScriptType.String,
        DropdownDeselectAllAllowed = false,
        EditableWithDropdown = false,
        EditableWithTextInput = true,
        DropDownItems = new(Array.Empty<string>()),
        ShowValuesOfOtherCellsInDropdown = false,
        DefaultRenderer = "TextOneLine",
        RendererSettings = "{ClassId=\"TextOneLine\", Style=\"Windows 11\"}"
    };

    public static readonly ColumnFormatHolder TextMitFormatierung = new(FormatHolder.TextMitFormatierung) {
        Align = AlignmentHorizontal.Links,
        SortType = SortierTyp.Sprachneutral_String,
        DoOpticalTranslation = TranslationType.Original_Anzeigen,
        AfterEditQuickSortRemoveDouble = false,
        ScriptType = ScriptType.String,
        DropdownDeselectAllAllowed = false,
        EditableWithDropdown = false,
        EditableWithTextInput = true,
        DropDownItems = new(Array.Empty<string>()),
        ShowValuesOfOtherCellsInDropdown = false,
        DefaultRenderer = "ImageAndText",
        RendererSettings = "{ClassId=\"ImageAndText\", Style=\"Windows 11\", ShowPic=-, ShowText=+}"
    };

    public static readonly ColumnFormatHolder TextOptions = new(FormatHolder.Text) {
        KeyName = "Items Select",
        Align = AlignmentHorizontal.Links,
        SortType = SortierTyp.Sprachneutral_String,
        DoOpticalTranslation = TranslationType.Übersetzen,
        AfterEditQuickSortRemoveDouble = true,
        ScriptType = ScriptType.String,
        DropdownDeselectAllAllowed = true,
        EditableWithDropdown = true,
        EditableWithTextInput = true,
        DropDownItems = new(Array.Empty<string>()),
        ShowValuesOfOtherCellsInDropdown = true,
        MultiLine = true
    };

    public static readonly ColumnFormatHolder Url = new(FormatHolder.Url) {
        Align = AlignmentHorizontal.Links,
        SortType = SortierTyp.Original_String,
        DoOpticalTranslation = TranslationType.Original_Anzeigen,
        AfterEditQuickSortRemoveDouble = false,
        ScriptType = ScriptType.String,
        DropdownDeselectAllAllowed = false,
        EditableWithDropdown = false,
        EditableWithTextInput = true,
        DropDownItems = new(Array.Empty<string>()),
        ShowValuesOfOtherCellsInDropdown = false,
        DefaultRenderer = "TextOneLine",
        RendererSettings = "{ClassId=\"TextOneLine\", Style=\"Windows 11\"}"
    };

    private readonly FormatHolder _format;

    #endregion

    #region Constructors

    private ColumnFormatHolder(FormatHolder format) {
        _format = format;
        KeyName = format.KeyName;
        QuickInfo = format.QuickInfo;
        AllFormats.Add(this);
    }

    #endregion

    #region Properties

    // IHasKeyName
    public bool KeyIsCaseSensitive => false;
    public string KeyName { get; private set; };

    // IReadableTextWithKey
    public string QuickInfo { get; private set; }

    // IReadableText
    public string ReadableText() => KeyName;
    public QuickImage? SymbolForReadableText() => _format.SymbolForReadableText();

    // IInputFormat — delegiert an Format (Setter sind No-Op, ColumnFormatHolder wird nur als Quelle verwendet)
    public AdditionalCheck AdditionalFormatCheck { get => _format.AdditionalFormatCheck; set { } }
    public string AllowedChars { get => _format.AllowedChars;  }
    public int MaxTextLength { get => _format.MaxTextLength; set { } }
    public bool MultiLine { get; set; }
    public string RegexCheck { get => _format.RegexCheck; set { } }
    public bool SpellCheckingEnabled { get => _format.SpellCheckingEnabled; set { } }
    public bool TextFormatingAllowed { get => _format.TextFormatingAllowed; set { } }

    // IColumnInputFormat
    public AlignmentHorizontal Align { get; set; }
    public string DefaultRenderer { get; set; } = string.Empty;
    public bool AfterEditDoUCase { get; set; }
    public TranslationType DoOpticalTranslation { get; set; }
    public bool DropdownDeselectAllAllowed { get; set; }
    public ReadOnlyCollection<string> DropDownItems { get; set; } = new(Array.Empty<string>());
    public bool EditableWithDropdown { get; set; }
    public bool EditableWithTextInput { get; set; }
    public string AfterEditAutoRemoveChar { get; set; } = string.Empty;
    public string RendererSettings { get; set; } = string.Empty;
    public bool AfterEditQuickSortRemoveDouble { get; set; }
    public int AfterEditRound { get; set; } = -1;
    public ScriptType ScriptType { get; set; }
    public bool ShowValuesOfOtherCellsInDropdown { get; set; }
    public SortierTyp SortType { get; set; }

    #endregion
}
