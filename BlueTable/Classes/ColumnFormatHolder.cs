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

    public static readonly ColumnFormatHolder BildCode = new ColumnFormatHolder_BildCode();
    public static readonly ColumnFormatHolder Bit = new ColumnFormatHolder_Bit();
    public static readonly ColumnFormatHolder Color = new ColumnFormatHolder_Color();
    public static readonly ColumnFormatHolder Date = new ColumnFormatHolder_Date();
    public static readonly ColumnFormatHolder DateTime = new ColumnFormatHolder_DateTime();
    public static readonly ColumnFormatHolder Email = new ColumnFormatHolder_Email();
    public static readonly ColumnFormatHolder Filepath = new ColumnFormatHolder_Filepath();
    public static readonly ColumnFormatHolder FilepathAndName = new ColumnFormatHolder_FilepathAndName();
    public static readonly ColumnFormatHolder Float = new ColumnFormatHolder_Float();
    public static readonly ColumnFormatHolder FloatPositive = new ColumnFormatHolder_FloatPositive();
    public static readonly ColumnFormatHolder Long = new ColumnFormatHolder_Long();
    public static readonly ColumnFormatHolder LongPositive = new ColumnFormatHolder_LongPositive();
    public static readonly ColumnFormatHolder PhoneNumber = new ColumnFormatHolder_PhoneNumber();
    public static readonly ColumnFormatHolder SystemName = new ColumnFormatHolder_SystemName();
    public static readonly ColumnFormatHolder Text = new ColumnFormatHolder_Text();
    public static readonly ColumnFormatHolder TextMitFormatierung = new ColumnFormatHolder_TextMitFormatierung();
    public static readonly ColumnFormatHolder TextOptions = new ColumnFormatHolder_TextOptions();
    public static readonly ColumnFormatHolder Url = new ColumnFormatHolder_Url();

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

    // IReadableText
    public string ReadableText() => KeyName;

    public QuickImage? SymbolForReadableText() => _format.SymbolForReadableText();

    #endregion
}
