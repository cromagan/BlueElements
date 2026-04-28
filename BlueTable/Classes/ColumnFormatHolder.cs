// Licensed under AGPL-3.0; see License.md for disclaimer and details.

using BlueBasics.Classes;
using BlueBasics.ClassesStatic;
using BlueBasics.Enums;
using BlueBasics.Interfaces;
using BlueTable.Enums;
using BlueTable.Interfaces;
using System.Collections.ObjectModel;

namespace BlueTable.Classes;

public abstract class ColumnFormatHolder : IColumnInputFormat, IReadableTextWithKey {

    #region Fields

    public static List<ColumnFormatHolder> AllFormats {
        get {
            field ??= [.. Generic.GetInstanceOfType<ColumnFormatHolder>()];
            return field;
        }
    }

    public static ColumnFormatHolder BildCode => GetByKey("Text");
    public static ColumnFormatHolder Bit => GetByKey("Bit");
    public static ColumnFormatHolder Color => GetByKey("Color");
    public static ColumnFormatHolder Date => GetByKey("Date");
    public static ColumnFormatHolder DateTime => GetByKey("DateTime");
    public static ColumnFormatHolder Email => GetByKey("EMail");
    public static ColumnFormatHolder Filepath => GetByKey("Filepath");
    public static ColumnFormatHolder FilepathAndName => GetByKey("FilepathAndName");
    public static ColumnFormatHolder Float => GetByKey("Float");
    public static ColumnFormatHolder FloatPositive => GetByKey("Float only Positive");
    public static ColumnFormatHolder Long => GetByKey("Long");
    public static ColumnFormatHolder LongPositive => GetByKey("Long only Positive");
    public static ColumnFormatHolder PhoneNumber => GetByKey("Phone Number");
    public static ColumnFormatHolder SystemName => GetByKey("Systemname");
    public static ColumnFormatHolder Text => GetByKey("Text One Line");
    public static ColumnFormatHolder TextMitFormatierung => GetByKey("Text with format");
    public static ColumnFormatHolder TextOptions => GetByKey("Items Select");
    public static ColumnFormatHolder Url => GetByKey("Url");

    private readonly FormatHolder _format;

    #endregion

    #region Constructors

    protected ColumnFormatHolder(FormatHolder format) {
        _format = format;
        KeyName = format.KeyName;
        QuickInfo = format.QuickInfo;
    }

    #endregion

    #region Properties

    // IInputFormat — delegiert an Format (Setter sind No-Op, ColumnFormatHolder wird nur als Quelle verwendet)
    public AdditionalCheck AdditionalFormatCheck { get => _format.AdditionalFormatCheck; set { } }

    public string AfterEditAutoRemoveChar { get; set; } = string.Empty;

    public bool AfterEditDoUCase { get; set; }

    public bool AfterEditQuickSortRemoveDouble { get; set; }

    public int AfterEditRound { get; set; } = -1;

    // IColumnInputFormat
    public AlignmentHorizontal Align { get; set; }

    public string AllowedChars { get => _format.AllowedChars; set { } }

    public string DefaultRenderer { get; set; } = string.Empty;

    public TranslationType DoOpticalTranslation { get; set; }

    public bool DropdownDeselectAllAllowed { get; set; }

    public ReadOnlyCollection<string> DropDownItems { get; set; } = new(Array.Empty<string>());

    public bool EditableWithDropdown { get; set; }

    public bool EditableWithTextInput { get; set; }

    // IHasKeyName
    public bool KeyIsCaseSensitive => false;

    public string KeyName { get; protected set; }

    public int MaxTextLength { get => _format.MaxTextLength; set { } }

    public bool MultiLine { get; set; }

    // IReadableTextWithKey
    public string QuickInfo { get; protected set; }

    public string RegexCheck { get => _format.RegexCheck; set { } }

    public string RendererSettings { get; set; } = string.Empty;

    public ScriptType ScriptType { get; set; }

    public bool ShowValuesOfOtherCellsInDropdown { get; set; }

    public SortierTyp SortType { get; set; }

    public bool SpellCheckingEnabled { get => _format.SpellCheckingEnabled; set { } }

    public bool TextFormatingAllowed { get => _format.TextFormatingAllowed; set { } }

    #endregion

    #region Methods

    private static ColumnFormatHolder GetByKey(string keyName) {
        return AllFormats.GetByKey(keyName) ?? throw new InvalidOperationException($"ColumnFormatHolder mit KeyName '{keyName}' nicht gefunden.");
    }

    // IReadableText
    public string ReadableText() => KeyName;

    public QuickImage? SymbolForReadableText() => _format.SymbolForReadableText();

    #endregion
}
