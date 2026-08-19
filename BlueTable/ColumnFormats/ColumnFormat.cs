// Licensed under AGPL-3.0; see License.md for disclaimer and details.

using BlueBasics.Formats;
using System.Collections.ObjectModel;

namespace BlueTable.ColumnFormats;

public abstract class ColumnFormat : IColumnInputFormat, IReadableTextWithKey {

    #region Fields

    public static readonly AssemblyAwareCache<ColumnFormat> AllFormats = new();
    private readonly Format _format;

    #endregion

    #region Constructors

    protected ColumnFormat(Format format) {
        _format = format;
        KeyName = format.KeyName;
        QuickInfo = format.QuickInfo;
    }

    #endregion

    #region Properties

    // IInputFormat — delegiert an Format (Setter sind No-Op, ColumnFormat wird nur als Quelle verwendet)
    public AdditionalCheck AdditionalFormatCheck { get => _format.AdditionalFormatCheck; set { } }

    public bool AfterEditDoUCase { get; set; }

    public bool AfterEditQuickSortRemoveDouble { get; set; }

    public int AfterEditRound { get; set; } = -1;

    // IColumnInputFormat
    public AlignmentHorizontal Align { get; set; }

    public string AllowedChars { get => _format.AllowedChars; set { } }

    public string ControlStrategy { get; set; } = "None";
    public JsonObject ControlStrategyParameter { get; set; } = new();
    public string DefaultRenderer { get; set; } = string.Empty;

    public TranslationType DoOpticalTranslation { get; set; }

    public ReadOnlyCollection<string> DropDownItems { get; set; } = new(Array.Empty<string>());
    public bool EditableWithTextInput { get; set; }
    public string ForbiddenChars { get => _format.ForbiddenChars; set { } }

    // IHasKeyName
    public string KeyName { get; protected set; }

    public int MaxTextLength { get => _format.MaxTextLength; set { } }
    public int MinTextLength { get => _format.MinTextLength; set { } }
    public bool MultiLine { get; set; }

    // IReadableTextWithKey
    public string QuickInfo { get; protected set; }

    public string RegexCheck { get => _format.RegexCheck; set { } }
    public string RendererSettings { get; set; } = string.Empty;
    public ScriptType ScriptType { get; set; }
    public bool ShowValuesOfOtherCellsInDropdown { get; set; }
    public SortierTyp SortType { get; set; }

    #endregion

    #region Methods

    // IReadableText
    public string ReadableText() => KeyName;

    public QuickImage? SymbolForReadableText() => _format.SymbolForReadableText();

    #endregion
}