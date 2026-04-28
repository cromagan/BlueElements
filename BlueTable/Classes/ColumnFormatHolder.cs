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

    public static List<ColumnFormatHolder> AllFormats {
        get {
            field ??= [.. Generic.GetInstanceOfType<ColumnFormatHolder>()];
            return field;
        }
    }

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

    private static ColumnFormatHolder GetByKey(string keyName) {
        return AllFormats.GetByKey(keyName) ?? throw new InvalidOperationException($"ColumnFormatHolder mit KeyName '{keyName}' nicht gefunden.");
    }

    #endregion
}