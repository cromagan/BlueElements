// Licensed under AGPL-3.0; see License.md for disclaimer and details.

using BlueTable.EventArgs;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Drawing;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using static BlueBasics.ClassesStatic.Constants;
using static BlueBasics.ClassesStatic.Converter;
using static BlueBasics.ClassesStatic.IO;
using static BlueTable.Classes.ColumnErrorConstants;
using static BlueTable.Classes.Table;

namespace BlueTable.Classes;

public sealed class ColumnItem : IReadableTextWithKey, IColumnInputFormat, IErrorCheckable, IHasTable, IDisposableExtendedWithEvent, IEditable, IHasSettings {

    #region Fields

    internal List<string>? UcaseNamesSortedByLength;

    private const string TmpNewDummy = "TMPNEWDUMMY";

    private readonly List<string> _afterEditAutoReplace = [];

    private readonly List<string> _dropDownItems = [];
    private readonly List<string> _linkedCellFilter = [];
    private readonly object _linkedTableLock = new();

    private readonly List<string> _permissionGroupsChangeCell = [];

    private AdditionalCheck _additionalFormatCheck;

    private string _adminInfo;

    private bool _afterEditAutoCorrect;

    private string _afterEditAutoRemoveChar;

    private bool _afterEditDoUCase;

    private bool _afterEditQuickSortRemoveDouble;

    private int _afterEditRound;

    private AlignmentHorizontal _align;

    private string _allowedChars;

    private string _autoFilterJoker;

    private Color _backColor;

    private ColumnBackgroundStyle _backgroundStyle;

    private string _caption;

    private string _captionBitmapCode;

    private string _captionGroup1;

    private string _captionGroup2;

    private string _captionGroup3;

    /// <summary>
    /// Die Quell-Spalte (aus der verlinkten Tabelle) ist immer
    /// </summary>
    private string _columnKeyOfLinkedTable;

    private string _columnSystemInfo;
    private string _columnTags;

    private string _defaultRenderer;

    private TranslationType _doOpticalTranslation;

    private bool _editableWithDropdown;
    private bool _editableWithTextInput;
    private bool _editAllowedDespiteLock;
    private FilterOptions _filterOptions;
    private int _fixedColumnWidth;
    private Color _foreColor;
    private bool _ignoreAtRowFilter;
    private volatile int _isDisposedFlag;
    private bool _isFirst;
    private bool _isKeyColumn;
    private string _keyName;
    private ColumnLineStyle _lineStyleLeft;
    private ColumnLineStyle _lineStyleRight;

    /// <summary>
    /// Diese Variable ist der temporäre Wert und wird von _linkedTableFile abgeleitet.
    /// </summary>
    private Table? _linkedTable;

    private string _linkedTableTableName;
    private int _maxCellLength;
    private int _maxTextLength;
    private bool _multiLine;
    private string _quickInfo;
    private string _regexCheck = string.Empty;
    private bool _relationship_to_First;
    private RelationType _relationType;
    private string _rendererSettings;
    private bool _saveContent;
    private ScriptType _scriptType;
    private bool _showValuesOfOtherCellsInDropdown;
    private SortierTyp _sortType;
    private bool _spellCheckingEnabled;
    private bool _textFormatingAllowed;
    private ChunkType _value_for_Chunk;
    private bool _valueRequired;

    #endregion

    #region Constructors

    public ColumnItem(Table table, string name) {
        if (!IsValidColumnKey(name)) {
            Develop.DebugError("Spaltenname nicht erlaubt!");
        }

        Table = table;

        var ex = table.Column[name];
        if (ex is not null) {
            Develop.DebugError("Key existiert bereits");
        }

        #region Standard-Werte

        _keyName = name.ToUpperInvariant();
        _caption = string.Empty;
        _lineStyleLeft = ColumnLineStyle.Dünn;
        _lineStyleRight = ColumnLineStyle.Ohne;
        _multiLine = false;
        _isKeyColumn = false;
        _relationship_to_First = false;
        _relationType = RelationType.None;
        _value_for_Chunk = ChunkType.None;
        _isFirst = false;
        _quickInfo = string.Empty;
        _captionGroup1 = string.Empty;
        _captionGroup2 = string.Empty;
        _captionGroup3 = string.Empty;
        //_Intelligenter_Multifilter = string.Empty;
        _foreColor = Color.Black;
        _backColor = Color.White;
        //_cellInitValue = string.Empty;
        //_linkedCellRowKeyIsInColumn = -1;
        _columnKeyOfLinkedTable = string.Empty;
        _sortType = SortierTyp.Original_String;
        //_ZellenZusammenfassen = false;
        //_dropDownKey = -1;
        //_vorschlagsColumn = -1;
        _align = AlignmentHorizontal.Links;
        _backgroundStyle = ColumnBackgroundStyle.None;
        //_keyColumnKey = -1;
        _allowedChars = string.Empty;
        _adminInfo = string.Empty;
        _columnSystemInfo = string.Empty;
        _columnTags = string.Empty;
        _defaultRenderer = string.Empty;
        _rendererSettings = string.Empty;
        _maxTextLength = 4000;
        _maxCellLength = 4000;
        //ContentWidthIsValid = false;
        _captionBitmapCode = string.Empty;
        _filterOptions = FilterOptions.Enabled | FilterOptions.TextFilterEnabled | FilterOptions.ExtendedFilterEnabled;
        //_AutofilterErlaubt = true;
        //_AutofilterTextFilterErlaubt = true;
        //_AutoFilterErweitertErlaubt = true;
        _ignoreAtRowFilter = false;
        _editableWithDropdown = false;
        _valueRequired = false;
        _editableWithTextInput = false;
        _showValuesOfOtherCellsInDropdown = false;
        _afterEditQuickSortRemoveDouble = false;
        _afterEditRound = -1;
        _fixedColumnWidth = 0;
        _afterEditAutoCorrect = false;
        _afterEditDoUCase = false;
        _textFormatingAllowed = false;
        _additionalFormatCheck = AdditionalCheck.None;
        _scriptType = ScriptType.undefiniert;
        _afterEditAutoRemoveChar = string.Empty;
        _autoFilterJoker = string.Empty;
        _saveContent = true;
        //_AutoFilter_Dauerfilter = enDauerfilter.ohne;
        _spellCheckingEnabled = false;
        //_CompactView = true;
        _doOpticalTranslation = TranslationType.Original_Anzeigen;
        _editAllowedDespiteLock = false;
        _linkedTableTableName = string.Empty;
        UcaseNamesSortedByLength = null;
        Am_A_Key_For.Clear();

        #endregion

        Invalidate_LinkedTable();
    }

    #endregion

    #region Destructors

    ~ColumnItem() {
        // Ändern Sie diesen Code nicht. Fügen Sie Bereinigungscode in der Methode "Dispose(bool disposing)" ein.
        Dispose(false);
    }

    #endregion

    #region Events

    public event EventHandler? DisposingEvent;

    public event PropertyChangedEventHandler? PropertyChanged;

    #endregion

    #region Properties

    public AdditionalCheck AdditionalFormatCheck {
        get => _additionalFormatCheck;
        set {
            if (IsDisposed) { return; }
            if (_additionalFormatCheck == value) { return; }

            Table?.ChangeData(TableDataType.AdditionalFormatCheck, this, ((int)_additionalFormatCheck).ToString1(), ((int)value).ToString1());
            OnPropertyChanged();
        }
    }

    [Description("Ein Information für Administratoren. Freier Text.\r\nWird als Quickinfo angezeigt, wenn der Admininstror\r\n mit der Maus über den Spaltenkopf fährt.")]
    public string AdminInfo {
        get => _adminInfo;
        set {
            if (IsDisposed) { return; }
            if (_adminInfo == value) { return; }

            Table?.ChangeData(TableDataType.ColumnAdminInfo, this, _adminInfo, value);
            OnPropertyChanged();
        }
    }

    public bool AfterEditAutoCorrect {
        get => _afterEditAutoCorrect;
        set {
            if (IsDisposed) { return; }
            if (_afterEditAutoCorrect == value) { return; }

            Table?.ChangeData(TableDataType.AutoCorrectAfterEdit, this, _afterEditAutoCorrect.ToPlusMinus(), value.ToPlusMinus());
            OnPropertyChanged();
        }
    }

    public string AfterEditAutoRemoveChar {
        get => _afterEditAutoRemoveChar;
        set {
            if (IsDisposed) { return; }
            if (_afterEditAutoRemoveChar == value) { return; }

            Table?.ChangeData(TableDataType.AfterEditAutoRemoveChar, this, _afterEditAutoRemoveChar, value);
            OnPropertyChanged();
        }
    }

    public ReadOnlyCollection<string> AfterEditAutoReplace {
        get => new(_afterEditAutoReplace);
        set {
            if (IsDisposed) { return; }
            if (!_afterEditAutoReplace.IsDifferentTo(value)) { return; }

            Table?.ChangeData(TableDataType.AutoReplaceAfterEdit, this, string.Join('\r', _afterEditAutoReplace), string.Join('\r', value));
            OnPropertyChanged();
        }
    }

    public bool AfterEditDoUCase {
        get => _afterEditDoUCase;
        set {
            if (IsDisposed) { return; }
            if (_afterEditDoUCase == value) { return; }

            Table?.ChangeData(TableDataType.DoUcaseAfterEdit, this, _afterEditDoUCase.ToPlusMinus(), value.ToPlusMinus());
            OnPropertyChanged();
        }
    }

    public bool AfterEditQuickSortRemoveDouble {
        get => _multiLine && _afterEditQuickSortRemoveDouble;
        set {
            if (IsDisposed) { return; }
            if (_afterEditQuickSortRemoveDouble == value) { return; }

            Table?.ChangeData(TableDataType.SortAndRemoveDoubleAfterEdit, this, _afterEditQuickSortRemoveDouble.ToPlusMinus(), value.ToPlusMinus());
            OnPropertyChanged();
        }
    }

    public int AfterEditRound {
        get => _afterEditRound;
        set {
            if (IsDisposed) { return; }
            if (_afterEditRound == value) { return; }

            Table?.ChangeData(TableDataType.AfterEditRound, this, _afterEditRound.ToString1(), value.ToString1());
            OnPropertyChanged();
        }
    }

    public AlignmentHorizontal Align {
        get => _align;
        set {
            if (IsDisposed) { return; }
            if (_align == value) { return; }

            Table?.ChangeData(TableDataType.ColumnAlign, this, ((int)_align).ToString1(), ((int)value).ToString1());
            OnPropertyChanged();
        }
    }

    public string AllowedChars {
        get => _allowedChars;
        set {
            if (IsDisposed) { return; }
            if (_allowedChars == value) { return; }

            Table?.ChangeData(TableDataType.AllowedChars, this, _allowedChars, value);
            OnPropertyChanged();
        }
    }

    public List<string> Am_A_Key_For { get; } = [];

    public string AutoFilterJoker {
        get => _autoFilterJoker;
        set {
            if (IsDisposed) { return; }
            if (_autoFilterJoker == value) { return; }

            Table?.ChangeData(TableDataType.AutoFilterJoker, this, _autoFilterJoker, value);
            OnPropertyChanged();
        }
    }

    public Color BackColor {
        get => _backColor;
        set {
            if (IsDisposed) { return; }
            if (_backColor.ToArgb() == value.ToArgb()) { return; }

            Table?.ChangeData(TableDataType.BackColor, this, _backColor.ToArgb().ToString1(), value.ToArgb().ToString1());
            OnPropertyChanged();
        }
    }

    public ColumnBackgroundStyle BackgroundStyle {
        get => _backgroundStyle;
        set {
            if (IsDisposed) { return; }
            if (_backgroundStyle == value) { return; }

            Table?.ChangeData(TableDataType.BackgroundStyle, this, ((long)_backgroundStyle).ToString1(), ((long)value).ToString1());
            OnPropertyChanged();
        }
    }

    public string Caption {
        get => _caption;
        set {
            if (IsDisposed) { return; }
            value = value.Replace("<br>", "\r", RegexOptions.IgnoreCase);
            if (_caption == value) { return; }

            Table?.ChangeData(TableDataType.ColumnCaption, this, _caption, value);
            OnPropertyChanged();
        }
    }

    public string CaptionBitmapCode {
        get => _captionBitmapCode;
        set {
            if (IsDisposed) { return; }
            if (_captionBitmapCode == value) { return; }

            Table?.ChangeData(TableDataType.CaptionBitmapCode, this, _captionBitmapCode, value);
            _captionBitmapCode = value;
            OnPropertyChanged();
        }
    }

    public string CaptionForEditor => "Spalte";

    public string CaptionGroup1 {
        get => _captionGroup1;
        set {
            if (IsDisposed) { return; }
            if (_captionGroup1 == value) { return; }

            Table?.ChangeData(TableDataType.CaptionGroup1, this, _captionGroup1, value);
            OnPropertyChanged();
        }
    }

    public string CaptionGroup2 {
        get => _captionGroup2;
        set {
            if (IsDisposed) { return; }
            if (_captionGroup2 == value) { return; }

            Table?.ChangeData(TableDataType.CaptionGroup2, this, _captionGroup2, value);
            OnPropertyChanged();
        }
    }

    public string CaptionGroup3 {
        get => _captionGroup3;
        set {
            if (IsDisposed) { return; }
            if (_captionGroup3 == value) { return; }

            Table?.ChangeData(TableDataType.CaptionGroup3, this, _captionGroup3, value);
            OnPropertyChanged();
        }
    }

    public string CaptionsCombined {
        get {
            var txt = _captionGroup1 + "/" + _captionGroup2 + "/" + _captionGroup3;
            return txt == "//" ? "###" : txt.TrimEnd('/');
        }
    }

    public string ColumnKeyOfLinkedTable {
        get => _columnKeyOfLinkedTable;
        set {
            if (IsDisposed) { return; }
            if (value == "-1") { value = string.Empty; }

            if (_columnKeyOfLinkedTable == value) { return; }

            Table?.ChangeData(TableDataType.ColumnKeyOfLinkedTable, this, _columnKeyOfLinkedTable, value);
            Invalidate_ColumAndContent();
            OnPropertyChanged();
        }
    }

    public string ColumnSystemInfo {
        get => _columnSystemInfo;
        private set {
            if (IsDisposed) { return; }
            if (_columnSystemInfo == value) { return; }

            Table?.ChangeData(TableDataType.ColumnSystemInfo, this, _columnSystemInfo, value);
            OnPropertyChanged();
        }
    }

    /// <summary>
    /// Was in Textfeldern oder Tabellezeilen für ein Suffix angezeigt werden soll. Beispiel: mm
    /// </summary>
    public string ColumnTags {
        get => _columnTags;
        set {
            if (IsDisposed) { return; }
            if (!_columnTags.IsDifferentTo(value)) { return; }

            Table?.ChangeData(TableDataType.ColumnTags, this, _columnTags, value);
            OnPropertyChanged();
        }
    }

    public string DefaultRenderer {
        get => _defaultRenderer;
        set {
            if (IsDisposed) { return; }
            if (_defaultRenderer == value) { return; }

            Table?.ChangeData(TableDataType.DefaultRenderer, this, _defaultRenderer, value);
            OnPropertyChanged();
        }
    }

    public TranslationType DoOpticalTranslation {
        get => _doOpticalTranslation;
        set {
            if (IsDisposed) { return; }
            if (_doOpticalTranslation == value) { return; }

            Table?.ChangeData(TableDataType.DoOpticalTranslation, this, ((int)_doOpticalTranslation).ToString1(), ((int)value).ToString1());
            OnPropertyChanged();
        }
    }

    public ReadOnlyCollection<string> DropDownItems {
        get => new(_dropDownItems);
        set {
            if (IsDisposed) { return; }
            if (!_dropDownItems.IsDifferentTo(value)) { return; }

            Table?.ChangeData(TableDataType.DropDownItems, this, string.Join('\r', _dropDownItems), string.Join('\r', value));
            OnPropertyChanged();
        }
    }

    public bool EditableWithDropdown {
        get => _editableWithDropdown;
        set {
            if (IsDisposed) { return; }
            if (_editableWithDropdown == value) { return; }

            Table?.ChangeData(TableDataType.EditableWithDropdown, this, _editableWithDropdown.ToPlusMinus(), value.ToPlusMinus());
            OnPropertyChanged();
        }
    }

    public bool EditableWithTextInput {
        get => _editableWithTextInput;
        set {
            if (IsDisposed) { return; }
            if (_editableWithTextInput == value) { return; }

            Table?.ChangeData(TableDataType.EditableWithTextInput, this, _editableWithTextInput.ToPlusMinus(), value.ToPlusMinus());
            OnPropertyChanged();
        }
    }

    public bool EditAllowedDespiteLock {
        get => _editAllowedDespiteLock;
        set {
            if (IsDisposed) { return; }
            if (_editAllowedDespiteLock == value) { return; }

            Table?.ChangeData(TableDataType.EditAllowedDespiteLock, this, _editAllowedDespiteLock.ToPlusMinus(), value.ToPlusMinus());
            OnPropertyChanged();
        }
    }

    public FilterOptions FilterOptions {
        get => _filterOptions;
        set {
            if (IsDisposed) { return; }
            if (_filterOptions == value) { return; }

            Table?.ChangeData(TableDataType.FilterOptions, this, ((int)_filterOptions).ToString1(), ((int)value).ToString1());
            OnPropertyChanged();
        }
    }

    public int FixedColumnWidth {
        get => _fixedColumnWidth;
        set {
            if (IsDisposed) { return; }
            if (_fixedColumnWidth == value) { return; }
            Table?.ChangeData(TableDataType.FixedColumnWidth, this, _fixedColumnWidth.ToString1(), value.ToString1());
            OnPropertyChanged();
        }
    }

    public Color ForeColor {
        get => _foreColor;
        set {
            if (IsDisposed) { return; }
            if (_foreColor.ToArgb() == value.ToArgb()) { return; }

            Table?.ChangeData(TableDataType.ForeColor, this, _foreColor.ToArgb().ToString1(), value.ToArgb().ToString1());
            OnPropertyChanged();
        }
    }

    public bool HasAutoRepair {
        get {
            if (_afterEditQuickSortRemoveDouble) { return true; }
            if (_afterEditAutoCorrect) { return true; }
            if (_afterEditAutoReplace.Count > 0) { return true; }
            if (_afterEditDoUCase) { return true; }
            if (!string.IsNullOrEmpty(_afterEditAutoRemoveChar)) { return true; }
            if (_afterEditRound > -1) { return true; }
            if (_textFormatingAllowed) { return true; }

            // _maxCellLength wird absichtlich ignoriert

            return false;
        }
    }

    public bool IgnoreAtRowFilter {
        get => _ignoreAtRowFilter;
        set {
            if (IsDisposed) { return; }
            if (_ignoreAtRowFilter == value) { return; }

            Table?.ChangeData(TableDataType.IgnoreAtRowFilter, this, _ignoreAtRowFilter.ToPlusMinus(), value.ToPlusMinus());
            OnPropertyChanged();
        }
    }

    public bool IsDisposed => _isDisposedFlag == 1;

    public bool IsFirst {
        get => _isFirst;
        set {
            if (IsDisposed) { return; }

            if (_isFirst == value) { return; }

            Table?.ChangeData(TableDataType.IsFirst, this, _isFirst.ToPlusMinus(), value.ToPlusMinus());
            OnPropertyChanged();
        }
    }

    public bool IsKeyColumn {
        get => _isKeyColumn;
        set {
            if (IsDisposed) { return; }

            if (_isKeyColumn == value) { return; }

            Table?.ChangeData(TableDataType.IsKeyColumn, this, _isKeyColumn.ToPlusMinus(), value.ToPlusMinus());
            OnPropertyChanged();
        }
    }

    public string KeyName {
        get => _keyName;
        set {
            if (IsDisposed) { return; }
            value = value.ToUpperInvariant();
            if (value.Equals(_keyName, StringComparison.OrdinalIgnoreCase)) { return; }

            if (!ColumNameAllowed(value)) {
                Develop.DebugPrint("Spaltenname nicht erlaubt: " + _keyName);
                return;
            }

            if (Table?.Column[value] is not null) {
                Develop.DebugPrint("Name existiert bereits!");
                return;
            }

            Table?.ChangeData(TableDataType.ColumnKey, this, _keyName, value);
            OnPropertyChanged();
            CheckIfIAmAKeyColumn();
        }
    }

    public ColumnLineStyle LineStyleLeft {
        get => _lineStyleLeft;
        set {
            if (IsDisposed) { return; }
            if (_lineStyleLeft == value) { return; }

            Table?.ChangeData(TableDataType.LineStyleLeft, this, ((int)_lineStyleLeft).ToString1(), ((int)value).ToString1());
            OnPropertyChanged();
        }
    }

    //        var c = Table?.Column.SearchByKey(_keyColumnKey);
    //        c?.CheckIfIAmAKeyColumn();
    //        Table?.ChangeData(TableDataType.KeyColumnKey, _name, null, _keyColumnKey.ToString(false), value.ToString(false), string.Empty);
    //        c = Table?.Column.SearchByKey(_keyColumnKey);
    //        c?.CheckIfIAmAKeyColumn();
    //        OnPropertyChanged(string propertyname);
    //    }
    //}
    public ColumnLineStyle LineStyleRight {
        get => _lineStyleRight;
        set {
            if (IsDisposed) { return; }
            if (_lineStyleRight == value) { return; }

            Table?.ChangeData(TableDataType.LineStyleRight, this, ((int)_lineStyleRight).ToString1(), ((int)value).ToString1());
            OnPropertyChanged();
        }
    }

    public ReadOnlyCollection<string> LinkedCellFilter {
        get => new(_linkedCellFilter);
        set {
            if (IsDisposed || Table is not { IsDisposed: false } tb) { return; }
            List<string> l = value?.SortedDistinctList().ToList() ?? [];
            if (!_linkedCellFilter.IsDifferentTo(l)) { return; }

            Table?.ChangeData(TableDataType.LinkedCellFilter, this, string.Join('\r', _linkedCellFilter), string.Join('\r', l));
            OnPropertyChanged();
            foreach (var thisColumn in tb.Column) {
                thisColumn.CheckIfIAmAKeyColumn();
            }
        }
    }

    public Table? LinkedTable {
        get {
            if (IsDisposed || Table is not { IsDisposed: false }) { return null; }
            if (string.IsNullOrEmpty(_linkedTableTableName)) { return null; }

            lock (_linkedTableLock) {
                if (_linkedTable is { IsDisposed: false }) { return _linkedTable; }
            }

            // Außerhalb des Locks um Deadlock zu vermeiden

            Invalidate_LinkedTable(); // Events sicher abmelden

            var newTable = Get(_linkedTableTableName, null);

            if (newTable is not null) {
                // Event-Registrierung vor dem Lock
                newTable.CellValueChanged += LinkedTable_CellValueChanged;
                newTable.DisposingEvent += LinkedTable_Disposing;
            }

            lock (_linkedTableLock) {
                _linkedTable = newTable; // Atomic assignment
                return _linkedTable; // Final read mit Lock
            }
        }
    }

    public string LinkedTableTableName {
        get => _linkedTableTableName;
        set {
            if (IsDisposed) { return; }
            if (_linkedTableTableName == value) { return; }

            Table?.ChangeData(TableDataType.LinkedTableTableName, this, _linkedTableTableName, value);
            Invalidate_LinkedTable();
            OnPropertyChanged();
        }
    }

    public int MaxCellLength {
        get => _maxCellLength;
        set {
            if (IsDisposed) { return; }
            if (_maxCellLength == value) { return; }
            Table?.ChangeData(TableDataType.MaxCellLength, this, _maxCellLength.ToString1(), value.ToString1());
            OnPropertyChanged();
        }
    }

    public int MaxTextLength {
        get => _maxTextLength;
        set {
            if (IsDisposed) { return; }
            if (_maxTextLength == value) { return; }
            Table?.ChangeData(TableDataType.MaxTextLength, this, _maxTextLength.ToString1(), value.ToString1());
            OnPropertyChanged();
        }
    }

    public string MostUsedValue {
        get {
            var liste = Contents();

            Dictionary<string, int> zähler = [];

            foreach (var wert in liste) {
                if (!string.IsNullOrEmpty(wert)) {
                    zähler.TryGetValue(wert, out var count);
                    zähler[wert] = count + 1;
                }
            }

            if (zähler.Count == 0) { return string.Empty; }

            return zähler.OrderByDescending(kvp => kvp.Value).FirstOrDefault().Key;
        }
    }

    public bool MultiLine {
        get => _multiLine;
        set {
            if (IsDisposed) { return; }
            if (!MultilinePossible()) { value = false; }

            if (_multiLine == value) { return; }

            Table?.ChangeData(TableDataType.MultiLine, this, _multiLine.ToPlusMinus(), value.ToPlusMinus());
            Invalidate_ColumAndContent();
            OnPropertyChanged();
        }
    }

    string IHasSettings.Name => "Chunk_" + (Table?.KeyName ?? KeyName) + "_" + KeyName;

    public ReadOnlyCollection<string> PermissionGroupsChangeCell {
        get => new(_permissionGroupsChangeCell);
        set {
            if (IsDisposed) { return; }
            if (!_permissionGroupsChangeCell.IsDifferentTo(value)) { return; }

            Table?.ChangeData(TableDataType.PermissionGroupsChangeCell, this, string.Join('\r', _permissionGroupsChangeCell), string.Join('\r', value));
            OnPropertyChanged();
        }
    }

    public string QuickInfo {
        get => _quickInfo;
        set {
            if (IsDisposed) { return; }
            if (_quickInfo == value) { return; }

            Table?.ChangeData(TableDataType.ColumnQuickInfo, this, _quickInfo, value);
            OnPropertyChanged();
        }
    }

    public string RegexCheck {
        get => _regexCheck;
        set {
            if (IsDisposed) { return; }
            if (_regexCheck == value) { return; }

            Table?.ChangeData(TableDataType.RegexCheck, this, _regexCheck, value);
            OnPropertyChanged();
        }
    }

    public bool Relationship_to_First {
        get => _relationship_to_First;
        set {
            if (IsDisposed) { return; }

            if (_relationship_to_First == value) { return; }

            Table?.ChangeData(TableDataType.Relationship_to_First, this, _relationship_to_First.ToPlusMinus(), value.ToPlusMinus());
            OnPropertyChanged();
        }
    }

    public RelationType RelationType {
        get => _relationType;
        set {
            if (IsDisposed) { return; }
            if (_relationType == value) { return; }

            Table?.ChangeData(TableDataType.RelationType, this, ((int)_relationType).ToString1(), ((int)value).ToString1());
            Invalidate_ColumAndContent();

            OnPropertyChanged();
        }
    }

    public string RendererSettings {
        get => _rendererSettings;
        set {
            if (IsDisposed) { return; }
            if (_rendererSettings == value) { return; }

            Table?.ChangeData(TableDataType.RendererSettings, this, _rendererSettings, value);
            OnPropertyChanged();
        }
    }

    public bool SaveContent {
        get => _saveContent;
        set {
            if (IsDisposed) { return; }
            if (_saveContent == value) { return; }
            Table?.ChangeData(TableDataType.SaveContent, this, _saveContent.ToPlusMinus(), value.ToPlusMinus());
            OnPropertyChanged();
        }
    }

    public ScriptType ScriptType {
        get => _scriptType;
        set {
            if (IsDisposed) { return; }
            if (_scriptType == value) { return; }

            Table?.ChangeData(TableDataType.ScriptType, this, ((int)_scriptType).ToString1(), ((int)value).ToString1());
            OnPropertyChanged();
        }
    }

    List<string> IHasSettings.Settings { get; } = [];

    bool IHasSettings.SettingsLoaded { get; set; }

    string IHasSettings.SettingsManualFilename { get; set; } = string.Empty;

    public bool ShowValuesOfOtherCellsInDropdown {
        get => _showValuesOfOtherCellsInDropdown;
        set {
            if (IsDisposed) { return; }
            if (_showValuesOfOtherCellsInDropdown == value) { return; }

            Table?.ChangeData(TableDataType.ShowValuesOfOtherCellsInDropdown, this, _showValuesOfOtherCellsInDropdown.ToPlusMinus(), value.ToPlusMinus());
            OnPropertyChanged();
        }
    }

    public SortierTyp SortType {
        get => _sortType;
        set {
            if (IsDisposed) { return; }
            if (_sortType == value) { return; }

            Table?.ChangeData(TableDataType.SortType, this, ((int)_sortType).ToString1(), ((int)value).ToString1());
            OnPropertyChanged();
        }
    }

    public bool SpellCheckingEnabled {
        get => _spellCheckingEnabled;
        set {
            if (IsDisposed) { return; }
            if (_spellCheckingEnabled == value) { return; }

            Table?.ChangeData(TableDataType.SpellCheckingEnabled, this, _spellCheckingEnabled.ToPlusMinus(), value.ToPlusMinus());
            OnPropertyChanged();
        }
    }

    public Table? Table {
        get;
        private set {
            if (IsDisposed || (value?.IsDisposed ?? true)) { value = null; }
            if (value == field) { return; }

            field?.DisposingEvent -= _table_Disposing;
            field = value;

            field?.DisposingEvent += _table_Disposing;
        }
    }

    public bool TextFormatingAllowed {
        get => _textFormatingAllowed;
        set {
            if (IsDisposed) { return; }
            if (_textFormatingAllowed == value) { return; }

            Table?.ChangeData(TableDataType.TextFormatingAllowed, this, _textFormatingAllowed.ToPlusMinus(), value.ToPlusMinus());
            OnPropertyChanged();
        }
    }

    bool IHasSettings.UsesSettings => _value_for_Chunk != ChunkType.None;

    public ChunkType Value_for_Chunk {
        get => _value_for_Chunk;
        set {
            if (IsDisposed) { return; }

            if (Table is not TableChunk) { value = ChunkType.None; }

            if (_value_for_Chunk == value) { return; }

            var oldd = _value_for_Chunk;

            Table?.ChangeData(TableDataType.Value_for_Chunk, this, ((int)_value_for_Chunk).ToString1(), ((int)value).ToString1());
            Invalidate_ColumAndContent();

            if (oldd != _value_for_Chunk) {
                Table?.ReorganizeChunks();
            }

            OnPropertyChanged();
        }
    }

    public bool ValueRequired {
        get => _valueRequired;
        set {
            if (IsDisposed) { return; }
            if (_valueRequired == value) { return; }

            Table?.ChangeData(TableDataType.ValueRequired, this, _valueRequired.ToPlusMinus(), value.ToPlusMinus());
            OnPropertyChanged();
        }
    }

    #endregion

    #region Methods

    public static bool IsValidColumnKey(string name) {
        if (string.IsNullOrWhiteSpace(name)) { return false; }

        if (!name.ContainsOnlyChars(AllowedCharsVariableName)) { return false; }

        if (!Char_AZ.Contains(char.ToUpperInvariant(name[0]))) { return false; }
        if (name.Length > 128) { return false; }

        // Illegale Namen definieren (nur Oracle + SQL Server relevante Wörter)
        string[] illegalNames = [
        // SQL Standard Schlüsselwörter (beide Systeme)
        //"SELECT", "INSERT", "UPDATE", "DELETE", "FROM", "WHERE", "JOIN", "INNER", "LEFT", "RIGHT", "FULL", "OUTER",
        //"ON", "AS", "AND", "OR", "NOT", "NULL", "IS", "IN", "EXISTS", "BETWEEN", "LIKE", "ORDER", "BY", "GROUP",
        //"HAVING", "UNION", "ALL", "DISTINCT", "COUNT", "SUM", "AVG", "MIN", "MAX", "CASE", "WHEN", "THEN", "ELSE",
        //"END", "IF", "CREATE", "ALTER", "DROP", "TABLE", "VIEW", "INDEX", "DATABASE", "SCHEMA", "CONSTRAINT",
        //"PRIMARY", "FOREIGN", "KEY", "REFERENCES", "CHECK", "UNIQUE", "DEFAULT", "AUTO_INCREMENT", "IDENTITY",

        // SQL Datentypen (beide Systeme)
        "INT", "INTEGER", "BIGINT", "SMALLINT", "TINYINT", "DECIMAL", "NUMERIC", "FLOAT", "REAL", "DOUBLE",
        "CHAR", "VARCHAR", "NCHAR", "NVARCHAR", "DATE", "TIME", "DATETIME", "TIMESTAMP",
        "YEAR", "BINARY", "VARBINARY", "BLOB", "CLOB", "BOOLEAN", "BIT", "MONEY", "SMALLMONEY", "GUID",

        // Oracle spezifische Datentypen
        "VARCHAR2", "NUMBER", "ROWID", "MLSLABEL", "RAW", "LONG",

        // SQL Server spezifische Datentypen
        "NTEXT", "IMAGE", "UNIQUEIDENTIFIER",

        // SQL Funktionen (beide Systeme)
        "ABS", "CEIL", "FLOOR", "ROUND", "SQRT", "POWER", "EXP", "LOG", "SIN", "COS", "TAN", "UPPER", "LOWER",
        "TRIM", "LTRIM", "RTRIM", "LENGTH", "SUBSTRING", "REPLACE", "CONCAT", "GETDATE", "DATEADD",
        "DATEDIFF", "CAST", "CONVERT", "ISNULL", "COALESCE",

        // System-Namen (beide Systeme)
        "USER", "COMMENT", "TABLE_NAME", "COLUMN_NAME", "OWNER", "DATA_TYPE", "DATA_LENGTH", "OFFLINE", "ONLINE",
        "SYSTEM", "ADMIN", "MASTER", "TEMP", "TEMPORARY", "LOG", "AUDIT", "BACKUP", "RESTORE", "TRANSACTION",
        "COMMIT", "ROLLBACK", "SAVEPOINT", "LOCK", "UNLOCK", "GRANT", "REVOKE", "PRIVILEGE", "PERMISSION",
        "ROLE", "LOGIN", "PASSWORD", "SESSION", "CONNECTION", "CATALOG", "SEQUENCE",

        // Oracle spezifische Systemnamen
        "ROWNUM", "SYSDATE", "DUAL", "SYS", "SYSTEM_USER", "SESSION_USER", "CURRENT_USER", "CURRENT_DATE",
        "CURRENT_TIME", "CURRENT_TIMESTAMP",

        // SQL Server spezifische Systemnamen
        "ROWCOUNT", "IDENTITY_INSERT", "IDENTITYCOL", "ROWGUIDCOL", "TEXTSIZE", "CURSOR", "PROC", "PROCEDURE",

        // BlueTable spezifisch
        TmpNewDummy
    ];

        // Prüfen ob Name in der Liste der illegalen Namen steht
        return !illegalNames.Contains(name, StringComparer.OrdinalIgnoreCase);
    }

    public static EditTypeTable UserEditDialogTypeInTable(ColumnItem? column, bool preverDropDown) => column is not { IsDisposed: false }
            ? EditTypeTable.None
            : UserEditDialogTypeInTable(column, preverDropDown && column.EditableWithDropdown, column.EditableWithTextInput);

    public static EditTypeTable UserEditDialogTypeInTable(ColumnItem column, bool doDropDown, bool keybordInputAllowed) {
        // Wenn weder Dropdown noch Tastatureingabe erlaubt sind, gibt es keine Editier-Möglichkeit
        if (!doDropDown && !keybordInputAllowed) { return EditTypeTable.None; }

        if (column.Table?.Column?.SysRowSortIndex == column) { return EditTypeTable.DragDrop; }

        // Expliziter RelationType hat Vorrang
        if (column.RelationType == RelationType.DropDownValues) { return EditTypeTable.Dropdown_Single; }

        var hasItems = column._dropDownItems.Count > 0 || column._showValuesOfOtherCellsInDropdown;

        // Fall 1: Kein Dropdown erwünscht -> Nur Textfeld-Varianten
        if (!doDropDown) {
            return hasItems ? EditTypeTable.Textfeld_mit_Vorschlägen : EditTypeTable.Textfeld;
        }

        // Fall 2: Dropdown ist erlaubt.
        // Wir prüfen, ob die Tastatur zusätzlich erlaubt ist für spezielle Kombi-Felder
        if (keybordInputAllowed) {
            // Wenn Vorschläge existieren und Formatierung oder spezielle Sortierung aktiv ist -> Vorschlagsfeld
            if (hasItems && (column.TextFormatingAllowed || (column.MultiLine && !column.AfterEditQuickSortRemoveDouble))) {
                return EditTypeTable.Textfeld_mit_Vorschlägen;
            }

            if (column.MultiLine) {
                return EditTypeTable.Dropdown_Single;
            }

            // Standard für Tastatur + Dropdown-Option
            return EditTypeTable.Textfeld_mit_Auswahlknopf;
        }

        // Fall 3: Nur Dropdown (da Tastatur nicht erlaubt oder keine Sonderregeln griffen)
        return EditTypeTable.Dropdown_Single;
    }

    public void AddSystemInfo(string type, string user) {
        var t = ColumnSystemInfo.SplitAndCutByCr().ToList();
        t.Add(type + ": " + user);

        //t.TagSet(type, user);
        ColumnSystemInfo = string.Join('\r', t.SortedDistinctList());
    }

    public void AddSystemInfo(string type, Table sourcetable, string user) => AddSystemInfo(type, sourcetable.Caption + " -> " + user);

    public string AutoCorrect(string value, bool exitifLinkedFormat) {
        if (IsDisposed || Table is not { IsDisposed: false }) { return value; }

        if (IsSystemColumn()) { return value; }
        //if (Function == ColumnFunction.Virtelle_Spalte) { return value; }

        if (exitifLinkedFormat && _relationType != RelationType.None) { return value; }

        if (_afterEditDoUCase) { value = value.ToUpperInvariant(); }

        if (!string.IsNullOrEmpty(_afterEditAutoRemoveChar)) { value = value.RemoveChars(_afterEditAutoRemoveChar); }

        if (_textFormatingAllowed) { value = value.CreateHtmlCodes(); }

        if (_afterEditAutoReplace.Count > 0) {
            List<string> l = [.. value.SplitAndCutByCr()];
            foreach (var thisar in _afterEditAutoReplace) {
                var rep = thisar.SplitAndCutBy("|");
                for (var z = 0; z < l.Count; z++) {
                    var r = string.Empty;
                    if (rep.Length > 1) { r = rep[1].Replace(";cr;", "\r"); }
                    var op = string.Empty;
                    if (rep.Length > 2) { op = rep[2].ToLowerInvariant(); }
                    if (op == "casesensitive") {
                        if (l[z] == rep[0]) { l[z] = r; }
                    } else if (op == "instr") {
                        l[z] = l[z].Replace(rep[0], r, RegexOptions.IgnoreCase);
                        //if (l[z].ToLowerInvariant() == rep[0].ToLowerInvariant()) { l[z] = r; }
                    } else {
                        if (string.Equals(l[z], rep[0], StringComparison.OrdinalIgnoreCase)) { l[z] = r; }
                    }
                }
            }
            value = string.Join('\r', l);
        }

        if (_afterEditAutoCorrect) { value = KleineFehlerCorrect(value); }

        if (_afterEditRound > -1 && DoubleTryParse(value, out var erg)) {
            erg = Math.Round(erg, _afterEditRound, MidpointRounding.AwayFromZero);
            value = erg.ToString1_X();
        }

        if (_afterEditQuickSortRemoveDouble) {
            var l = new List<string>(value.SplitAndCutByCr()).SortedDistinctList();
            value = string.Join('\r', l);
        }

        return value.CutToUtf8Length(_maxCellLength);
    }

    public bool AutoFilterSymbolPossible() => FilterOptions.HasFlag(FilterOptions.Enabled);

    public int CalculatePreveredMaxCellLength(double prozentZuschlag) {
        if (IsDisposed || Table is not { IsDisposed: false }) { return 0; }

        //if (Format == DataFormat.Verknüpfung_zu_anderer_Tabellex) { return 35; }
        //if (Format == DataFormat.Werte_aus_anderer_Tabelle_als_DropDownItemsx) { return 15; }
        var m = Table.Row.AsParallel()
            .Select(row => row.CellGetString(this).StringtoUtf8().Length)
            .DefaultIfEmpty(0)
            .Max();

        if (m <= 0) { return 8; }
        if (m == 1) { return 1; }
        return Math.Clamp((int)(m * prozentZuschlag) + 1, _maxTextLength, 3999);
    }

    public int CalculatePreveredMaxTextLength(double prozentZuschlag) {
        if (IsDisposed || Table is not { IsDisposed: false }) { return 0; }

        ////if (Format == DataFormat.Verknüpfung_zu_anderer_Tabellex) { return 35; }
        ////if (Format == DataFormat.Werte_aus_anderer_Tabelle_als_DropDownItemsx) { return 15; }
        var m = 0;

        var l = Contents();

        foreach (var thiss in l) {
            m = Math.Max(m, thiss.Length);
        }

        if (m <= 0) { return 8; }
        if (m == 1) { return 1; }
        return Math.Min((int)(m * prozentZuschlag) + 1, 1000);
    }

    public bool CanBeChangedByRules() {
        if (_value_for_Chunk != ChunkType.None) { return false; }
        if (_relationType == RelationType.CellValues) { return false; }
        return true;
    }

    public bool CanBeCheckedByRules() => true;

    /// <summary>
    ///
    /// </summary>
    /// <param name="number">Muss 1, 2 oder 3 sein</param>
    /// <returns></returns>
    public string CaptionGroup(int number) {
        switch (number) {
            case 0:
                return _captionGroup1;

            case 1:
                return _captionGroup2;

            case 2:
                return _captionGroup3;

            default:
                Develop.DebugPrint("Nummer " + number + " nicht erlaubt.");
                return string.Empty;
        }
    }

    public bool ColumNameAllowed(string nameToTest) {
        if (!IsValidColumnKey(nameToTest)) { return false; }

        if (nameToTest.Equals(_keyName, StringComparison.OrdinalIgnoreCase)) { return true; }
        if (Table?.Column[nameToTest] is not null) { return false; }

        return true;
    }

    public List<string> Contents() => Contents(Table?.Row.ToList(), string.Empty);

    public List<string> Contents(FilterCollection fc, List<RowItem>? pinned) {
        if (IsDisposed || Table is not { IsDisposed: false }) { return []; }
        var r = fc.Rows;

        var r2 = new List<RowItem>();
        r2.AddRange(r);

        if (pinned is not null) { r2.AddIfNotExists(pinned); }

        return Contents(r2, string.Empty);
    }

    public List<string> Contents(ICollection<RowItem>? rows, string empty) {
        if (rows is null || rows.Count == 0) { return []; }

        var list = new List<string>();
        foreach (var thisRowItem in rows) {
            if (thisRowItem is not null) {
                if (!_saveContent) { thisRowItem.CheckRow(); }

                var t = thisRowItem.CellGetStringCore(this);
                if (string.IsNullOrWhiteSpace(t)) { t = empty; }

                if (!string.IsNullOrWhiteSpace(t)) {
                    if (_multiLine) {
                        list.AddRange(t.SplitByCr());
                    } else {
                        list.Add(t);
                    }
                }
            }
        }

        return list.SortedDistinctList();
    }

    public void CopyTo(ColumnItem target, bool nameAndKeyToo) {
        if (target.Table is not { IsDisposed: false } tb) { return; }
        if (!string.IsNullOrEmpty(tb.IsValueEditable(TableDataType.ColumnKey, TableChunk.Chunk_Master))) { return; }

        if (Table is not null) { Repair(); }

        if (nameAndKeyToo) { target.KeyName = KeyName; }

        target.Caption = Caption;
        target.IsKeyColumn = IsKeyColumn;
        target.RelationType = RelationType;
        target.Relationship_to_First = Relationship_to_First;
        target.Value_for_Chunk = Value_for_Chunk;
        target.IsFirst = IsFirst;
        target.CaptionBitmapCode = CaptionBitmapCode;
        target.LineStyleLeft = LineStyleLeft;
        target.LineStyleRight = LineStyleRight;
        target.BackgroundStyle = BackgroundStyle;
        target.QuickInfo = QuickInfo;
        target.ForeColor = ForeColor;
        target.BackColor = BackColor;
        target.EditAllowedDespiteLock = EditAllowedDespiteLock;
        target.PermissionGroupsChangeCell = PermissionGroupsChangeCell;
        target.ColumnTags = ColumnTags;
        target.AdminInfo = AdminInfo;
        target.FilterOptions = FilterOptions;
        target.IgnoreAtRowFilter = IgnoreAtRowFilter;
        target.MaxCellLength = MaxCellLength;
        target.FixedColumnWidth = FixedColumnWidth;
        target.AfterEditAutoCorrect = AfterEditAutoCorrect;
        target.AutoFilterJoker = AutoFilterJoker;
        target.ColumnKeyOfLinkedTable = ColumnKeyOfLinkedTable;
        target.LinkedCellFilter = LinkedCellFilter;
        target.AfterEditAutoReplace = AfterEditAutoReplace;
        target.GetStyleFrom(this);
        target.SaveContent = SaveContent;
        target.CaptionGroup1 = CaptionGroup1;
        target.CaptionGroup2 = CaptionGroup2;
        target.CaptionGroup3 = CaptionGroup3;
        target.LinkedTableTableName = LinkedTableTableName;
    }

    public string DefaultValueForColumn() {
        if (ScriptType is ScriptType.Numeral or ScriptType.Numeral_Readonly) { return "0"; }

        if (ScriptType is ScriptType.Bool or ScriptType.Bool_Readonly) { return "-"; }

        if (SortType is SortierTyp.ZahlenwertInt or SortierTyp.ZahlenwertFloat) { return "0"; }

        if (SortType is SortierTyp.Datum_Uhrzeit) { return "01.01.1900 00:00:00"; }

        if (!ValueRequired) { return string.Empty; }

        return "-";
    }

    public void Dispose() {
        // Ändern Sie diesen Code nicht. Fügen Sie Bereinigungscode in der Methode "Dispose(bool disposing)" ein.
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    public bool DropdownItemsOfOtherCellsAllowed() {
        if (_relationType == RelationType.DropDownValues) { return false; }
        return true;
    }

    public bool EmptyPossible() {
        if (_isFirst) { return false; }

        return true;
    }

    public string ErrorReason() {
        if (IsDisposed || Table is not { IsDisposed: false } tb) { return TableDisposed; }
        return ErrorReason_KeyAndSizes(tb)
            ?? ErrorReason_Relations(tb)
            ?? ErrorReason_Filters()
            ?? ErrorReason_NoSaveContent()
            ?? ErrorReason_SpecialColumns()
            ?? ErrorReason_RelationshipToFirst(tb)
            ?? ErrorReason_Multiline()
            ?? ErrorReason_Editing()
            ?? ErrorReason_PostChecks()
            ?? ErrorReason_ChapterColumn()
            ?? string.Empty;
    }

    public List<(string value, RowItem row)> GetCellContentsSortedByLength() {
        if (IsDisposed || Table is not { IsDisposed: false } tb) { return []; }

        var dict = new Dictionary<string, RowItem>(StringComparer.OrdinalIgnoreCase);

        foreach (var row in tb.Row) {
            var cellValue = row.CellGetString(this);
            if (string.IsNullOrWhiteSpace(cellValue)) { continue; }

            dict[cellValue] = row;
        }

        var result = dict.Select(kvp => (kvp.Key, kvp.Value)).ToList();
        result.Sort((a, b) => b.Key.Length.CompareTo(a.Key.Length));

        return result;
    }

    public void Invalidate_ColumAndContent() {
        if (IsDisposed || Table is not { IsDisposed: false }) { return; }
        Invalidate_LinkedTable();
        Table.OnViewChanged();
    }

    string IEditable.IsNowEditable() {
        if (Table is not { IsDisposed: false } tb) { return "Tabelle verworfen"; }
        return tb.AcquireWriteAccess(TableDataType.ColumnKey);
    }

    public bool IsSystemColumn() =>
        _keyName is SystemColumnKeys.CellNote or
            SystemColumnKeys.Correct or
            SystemColumnKeys.Changer or
            SystemColumnKeys.Creator or
            SystemColumnKeys.DateCreated or
            SystemColumnKeys.DateChanged or
            SystemColumnKeys.Locked or
            SystemColumnKeys.RowState or
            SystemColumnKeys.RowKey or
            SystemColumnKeys.RowSortIndex;

    /// <summary>
    /// Gibt an, ob diese Spalte in mindestens einer Spaltenanordnung als Kapitel-Spalte
    /// (<see cref="ColumnViewCollection.ColumnForChapter"/>) verwendet wird.
    /// </summary>
    public bool IsChapterColumn() {
        if (Table is not { IsDisposed: false } tb) { return false; }
        foreach (var ca in tb.ColumnArrangements) {
            if (ca.ColumnForChapter == this) { return true; }
        }
        return false;
    }

    public bool MultilinePossible() {
        if (_value_for_Chunk != ChunkType.None) { return false; }
        if (_relationType == RelationType.DropDownValues) { return false; }
        return true;
    }

    public string ReadableText() {
        if (IsDisposed || Table is not { IsDisposed: false }) { return string.Empty; }

        var ret = _caption;
        if (Table.Column.Any(thisColumnItem => thisColumnItem is not null && thisColumnItem != this && string.Equals(thisColumnItem.Caption, _caption, StringComparison.OrdinalIgnoreCase))) {
            if (!string.IsNullOrEmpty(_captionGroup3)) {
                ret = _captionGroup3 + "/" + ret;
            }
            if (!string.IsNullOrEmpty(_captionGroup2)) {
                ret = _captionGroup2 + "/" + ret;
            }
            if (!string.IsNullOrEmpty(_captionGroup1)) {
                ret = _captionGroup1 + "/" + ret;
            }
        }
        ret = ret.Replace('\n', '\r').Replace("\r\r", "\r");
        var i = ret.IndexOf("-\r", StringComparison.Ordinal);
        if (i > 0 && i < ret.Length - 2) {
            var tzei = ret[i + 2];
            if (char.IsLetter(tzei)) {
                ret = ret[..i] + ret[(i + 2)..];
            }
        }
        return ret.Replace('\r', ' ').Replace("  ", " ").TrimEnd(':');
    }

    public void Repair() {
        if (Table is not { IsDisposed: false } tb) { return; }
        if (!string.IsNullOrEmpty(tb.IsValueEditable(TableDataType.ColumnKey, TableChunk.Chunk_Master))) { return; }

        //if (IsDisposed || Table is not { IsDisposed: false }) { return; }
        //if (IsDisposed) { return; }

        //if (_function.ToString() == ((int)_function).ToString()) {
        //    this.GetStyleFrom(ColumnFormatHolder_TextOneLine.Instance);
        //}

        if (!Enum.IsDefined(typeof(ScriptType), _scriptType)) { ScriptType = ScriptType.Nicht_vorhanden; }

        //switch (_function) {
        //    case ColumnFunction.Virtuelle_Spalte:
        //        SaveContent = false;
        //        _function = ColumnFunction.Normal;
        //        break;

        //    case ColumnFunction.First:
        //        IsFirst = true;
        //        _function = ColumnFunction.Normal;
        //        break;

        //    case ColumnFunction.Schlüsselspalte:
        //        IsKeyColumn = true;
        //        _function = ColumnFunction.Normal;
        //        break;

        //    case ColumnFunction.Verknüpfung_zu_anderer_Tabelle:
        //        RelationType = RelationType.CellValues;
        //        _function = ColumnFunction.Normal;

        //        #region Aus Dateinamen den Tablename extrahieren

        //        if (!_linkedTableTableName.Contains("|") && _linkedTableTableName.IsFormat(FormatHolder_FilepathAndName.Instance)) {
        //            _linkedTableTableName = _linkedTableTableName.ToUpperInvariant().TrimEnd(".MDB").TrimEnd(".BDB").TrimEnd(".MBDB").TrimEnd(".CBDB");
        //            LinkedTableTableName = MakeValidTableName(_linkedTableTableName);
        //        }

        //        #endregion

        //        #region Aus Connection-info den Tablename extrahieren

        //        if (_linkedTableTableName.Contains("|")) {
        //            var l = _linkedTableTableName.Split('|');
        //            if (IsValidTableName(l[0])) { LinkedTableTableName = l[0]; }
        //        }

        //        #endregion

        //        var c = LinkedTable?.Column[_columnNameOfLinkedTable];
        //        if (c is { IsDisposed: false }) {
        //            this.GetStyleFrom((IInputFormat)c);
        //            ScriptType = ScriptType.Nicht_vorhanden;
        //            DoOpticalTranslation = c.DoOpticalTranslation;

        //            MaxTextLength = c.MaxTextLength;
        //            MaxCellLength = c.MaxCellLength;
        //        }
        //        break;

        //    case ColumnFunction.RelationText:
        //        Relationship_to_First = true;
        //        _function = ColumnFunction.Normal;
        //        break;

        //    case ColumnFunction.Split_Name:
        //        _value_for_Chunk = ChunkType.ByName;
        //        _function = ColumnFunction.Normal;
        //        Table?.ChangeData(TableDataType.Value_for_Chunk, this, null, "0", ((int)_value_for_Chunk).ToString(), Generic.UserName, DateTime.UtcNow, "Neue Spaltenfunktionen", string.Empty);
        //        break;

        //    case ColumnFunction.Split_Medium:
        //        _value_for_Chunk = ChunkType.ByHash_2Chars;
        //        _function = ColumnFunction.Normal;
        //        Table?.ChangeData(TableDataType.Value_for_Chunk, this, null, "0", ((int)_value_for_Chunk).ToString(), Generic.UserName, DateTime.UtcNow, "Neue Spaltenfunktionen", string.Empty);
        //        break;

        //    case ColumnFunction.Split_Large:
        //        _value_for_Chunk = ChunkType.ByHash_3Chars;
        //        _function = ColumnFunction.Normal;
        //        Table?.ChangeData(TableDataType.Value_for_Chunk, this, null, "0", ((int)_value_for_Chunk).ToString(), Generic.UserName, DateTime.UtcNow, "Neue Spaltenfunktionen", string.Empty);
        //        break;

        //    case ColumnFunction.Werte_aus_anderer_Tabelle_als_DropDownItems:
        //        RelationType = RelationType.DropDownValues;
        //        _function = ColumnFunction.Normal;
        //        break;
        //}

        if (MaxCellLength < MaxTextLength) { MaxCellLength = MaxTextLength; }

        PermissionGroupsChangeCell = RepairUserGroups(PermissionGroupsChangeCell).AsReadOnly();

        _valueRequired = false;

        if (_scriptType is ScriptType.Bool or ScriptType.Numeral) {
            _valueRequired = true;
        }

        if (_isFirst) { _valueRequired = true; }

        ResetSystemToDefault(false);
        CheckIfIAmAKeyColumn();

        SystemInfoReset(false);
    }

    public void ResetSystemToDefault(bool allDefaultValues) {
        if (!IsSystemColumn()) { return; }
        // ACHTUNG: Die setOpticalToo Befehle OHNE _, die müssen geloggt werden.
        if (allDefaultValues) {
            LineStyleLeft = ColumnLineStyle.Dünn;
            LineStyleRight = ColumnLineStyle.Ohne;
            ForeColor = Color.FromArgb(0, 0, 0);
            //CaptionBitmapCode = null;
        }

        _saveContent = true;

        //_relationType = RelationType.None;

        switch (_keyName) {
            case SystemColumnKeys.Creator:
                _valueRequired = true;
                _isFirst = false;
                _maxTextLength = 20;
                _maxCellLength = 20;
                if (allDefaultValues) {
                    this.GetStyleFrom(ColumnFormatHolder_TextOneLine.Instance); // HIer ColumnFormatHolder
                    Caption = "Ersteller";
                    EditableWithDropdown = true;
                    ShowValuesOfOtherCellsInDropdown = true;
                    SpellCheckingEnabled = false;
                    ForeColor = Color.FromArgb(0, 0, 128);
                    BackColor = Color.FromArgb(185, 186, 255);
                }

                break;

            case SystemColumnKeys.Changer:
                _valueRequired = true;
                _relationship_to_First = false;
                _relationType = RelationType.None;
                _value_for_Chunk = ChunkType.None;
                _isKeyColumn = false;
                _isFirst = false;
                _ignoreAtRowFilter = true;
                _spellCheckingEnabled = false;
                _editableWithTextInput = false;
                _editableWithDropdown = false;
                _scriptType = ScriptType.Nicht_vorhanden;  // um Script-Prüfung zu reduzieren
                _permissionGroupsChangeCell.Clear();
                _maxTextLength = 20;
                _maxCellLength = 20;
                if (allDefaultValues) {
                    this.GetStyleFrom(ColumnFormatHolder_TextOneLine.Instance); // HIer ColumnFormatHolder
                    Caption = "Änderer";
                    ForeColor = Color.FromArgb(0, 128, 0);
                    BackColor = Color.FromArgb(185, 255, 185);
                }
                break;

            case SystemColumnKeys.Chapter_Obsolete:
                KeyName = "CHAPTER";
                _valueRequired = false;

                //_multiLine = true;

                //if (setOpticalToo) {
                //    Caption = "Kapitel";
                //    ForeColor = Color.FromArgb(0, 0, 0);
                //    BackColor = Color.FromArgb(255, 255, 150);
                //    LineStyleLeft = ColumnLineStyle.Dick;
                //}
                break;

            case SystemColumnKeys.DateCreated:
                _spellCheckingEnabled = false;
                _ignoreAtRowFilter = true;
                _valueRequired = true;

                this.GetStyleFrom(FormatHolder_DateTime.Instance); // Ja, FormatHolder, da wird der Script-Type nicht verändert
                MaxCellLength = MaxTextLength;
                if (allDefaultValues) {
                    this.GetStyleFrom(ColumnFormatHolder_DateTime.Instance); // HIer ColumnFormatHolder
                    Caption = "Erstell-Datum";
                    ForeColor = Color.FromArgb(0, 0, 128);
                    BackColor = Color.FromArgb(185, 185, 255);
                    LineStyleLeft = ColumnLineStyle.Dick;
                }

                break;

            case SystemColumnKeys.RowKey:
                _valueRequired = true;
                _spellCheckingEnabled = false;
                _ignoreAtRowFilter = true;
                _editableWithTextInput = false;
                _editableWithDropdown = false;
                if (_scriptType is not ScriptType.String_Readonly and not ScriptType.List_Readonly) {
                    _scriptType = ScriptType.Nicht_vorhanden; // Wichtig! Weil eine Routine ErrorCol !=0 den Wert setzt und evtl. eine Endlosschleife auslöst
                }
                _maxTextLength = 50;
                _maxCellLength = 50;
                if (allDefaultValues) {
                    this.GetStyleFrom(ColumnFormatHolder_TextOneLine.Instance);
                    Caption = "Zeilen-Schlüssel";
                    ForeColor = Color.FromArgb(0, 0, 128);
                    BackColor = Color.FromArgb(185, 185, 255);
                }

                break;

            case SystemColumnKeys.RowState:
                _isKeyColumn = false;
                _isFirst = false;
                _relationship_to_First = false;
                _relationType = RelationType.None;
                _value_for_Chunk = ChunkType.None;
                _spellCheckingEnabled = false;
                _ignoreAtRowFilter = true;
                _valueRequired = false;
                this.GetStyleFrom(FormatHolder_DateTime.Instance); // Ja, FormatHolder, da wird der Script-Type nicht verändert
                MaxCellLength = MaxTextLength;
                if (allDefaultValues) {
                    this.GetStyleFrom(ColumnFormatHolder_DateTime.Instance); // HIer ColumnFormatHolder
                    Caption = "Zeilen-Status";
                    ForeColor = Color.FromArgb(128, 0, 0);
                    BackColor = Color.FromArgb(255, 185, 185);
                    //LineLeft = ColumnLineStyle.Dick;
                }
                _scriptType = ScriptType.Nicht_vorhanden;  // um Script-Prüfung zu reduzieren

                break;

            case SystemColumnKeys.RowColor_Obsolete:
                KeyName = "ROWCOLOROLD";
                Caption = "LÖSCHEN";
                AdminInfo = "Früher mal Zeilenfarbe, wird nun im Skript 'Vorbereiten' gesetzt.";
                _valueRequired = false;
                break;

            case SystemColumnKeys.DateChanged:
                _isKeyColumn = false;
                _isFirst = false;
                _relationship_to_First = false;
                _relationType = RelationType.None;
                _value_for_Chunk = ChunkType.None;
                _spellCheckingEnabled = false;
                _ignoreAtRowFilter = true;
                _valueRequired = true;

                this.GetStyleFrom(FormatHolder_DateTimeWithMilliSeconds.Instance); // Ja, FormatHolder, da wird der Script-Type nicht verändert
                MaxCellLength = MaxTextLength;
                _editableWithTextInput = false;
                _spellCheckingEnabled = false;
                _editableWithDropdown = false;
                _scriptType = ScriptType.Nicht_vorhanden; // um Script-Prüfung zu reduzieren
                _permissionGroupsChangeCell.Clear();

                if (allDefaultValues) {
                    this.GetStyleFrom(ColumnFormatHolder_DateTime.Instance); // HIer ColumnFormatHolder
                    Caption = "Änder-Datum";
                    ForeColor = Color.FromArgb(0, 128, 0);
                    BackColor = Color.FromArgb(185, 255, 185);
                    LineStyleLeft = ColumnLineStyle.Dick;
                }
                break;

            case SystemColumnKeys.Correct:
                _valueRequired = true;
                _isFirst = false;
                _spellCheckingEnabled = false;
                _relationship_to_First = false;
                _relationType = RelationType.None;
                _value_for_Chunk = ChunkType.None;
                _autoFilterJoker = string.Empty;

                if (_scriptType is not ScriptType.String_Readonly and not ScriptType.Bool_Readonly and not ScriptType.List_Readonly) {
                    _scriptType = ScriptType.Nicht_vorhanden; // Wichtig! Weil eine Routine ErrorCol !=0 den Wert setzt und evtl. eine Endlosschleife auslöst
                }

                _dropDownItems.Clear();
                _linkedCellFilter.Clear();
                _permissionGroupsChangeCell.Clear();
                _editableWithTextInput = false;
                _editableWithDropdown = false;
                _maxTextLength = 1;
                _maxCellLength = 1;
                _showValuesOfOtherCellsInDropdown = false;

                if (allDefaultValues) {
                    AdminInfo = "Diese Spalte kann nur über ein Skript bearbeitet<br>werden, mit dem Befehl 'SetError'";
                    Caption = "Fehlerfrei";
                    FilterOptions = FilterOptions.Enabled;
                    IgnoreAtRowFilter = true;
                    Align = AlignmentHorizontal.Rechts;
                    this.GetStyleFrom(ColumnFormatHolder_Bit.Instance); // HIer ColumnFormatHolder
                    ForeColor = Color.FromArgb(128, 0, 0);
                    BackColor = Color.FromArgb(255, 185, 185);
                    LineStyleLeft = ColumnLineStyle.Dick;
                }
                break;

            case SystemColumnKeys.CellNote:
                _valueRequired = false;
                _isFirst = false;
                _spellCheckingEnabled = false;
                _relationship_to_First = false;
                _relationType = RelationType.None;
                _value_for_Chunk = ChunkType.None;
                _filterOptions = FilterOptions.None;
                _autoFilterJoker = string.Empty;
                _ignoreAtRowFilter = true;
                _align = AlignmentHorizontal.Links;
                _editableWithTextInput = false;
                _editableWithDropdown = false;
                _maxTextLength = 2000;
                _maxCellLength = 2000;
                _multiLine = true;

                if (allDefaultValues) {
                    this.GetStyleFrom(ColumnFormatHolder_TextOneLine.Instance);
                    DefaultRenderer = "CellNote";
                    Caption = "Notizen";
                    ForeColor = Color.FromArgb(80, 80, 80);
                    BackColor = Color.FromArgb(255, 255, 230);
                }
                break;

            case SystemColumnKeys.Locked:
                _valueRequired = true;
                _isFirst = false;
                _spellCheckingEnabled = false;
                _relationship_to_First = false;
                _relationType = RelationType.None;
                _value_for_Chunk = ChunkType.None;
                _filterOptions = FilterOptions.Enabled;
                _autoFilterJoker = string.Empty;
                _ignoreAtRowFilter = true;
                _align = AlignmentHorizontal.Zentriert;
                _maxTextLength = 1;
                _maxCellLength = 1;

                if (_editableWithTextInput || _editableWithDropdown) {
                    _quickInfo = "Eine abgeschlossene Zeile kann<br>nicht mehr bearbeitet werden.";
                    _editableWithTextInput = false;
                    _editableWithDropdown = true;
                    _editAllowedDespiteLock = true;
                    _dropDownItems.AddIfNotExists("+");
                    _dropDownItems.AddIfNotExists("-");
                } else {
                    _dropDownItems.Clear();
                }

                if (allDefaultValues) {
                    this.GetStyleFrom(ColumnFormatHolder_Bit.Instance); // HIer ColumnFormatHolder
                    Caption = "Abgeschlossen";
                    ForeColor = Color.FromArgb(128, 0, 0);
                    BackColor = Color.FromArgb(255, 185, 185);
                }
                break;

            case SystemColumnKeys.RowSortIndex:
                _valueRequired = true;
                _isFirst = false;
                _isKeyColumn = false;
                _relationship_to_First = false;
                _relationType = RelationType.None;
                _value_for_Chunk = ChunkType.None;
                _spellCheckingEnabled = false;
                _editableWithDropdown = false;
                _permissionGroupsChangeCell.Clear();
                _maxTextLength = 19;
                _maxCellLength = 19;
                _sortType = SortierTyp.ZahlenwertInt;

                this.GetStyleFrom(FormatHolder_LongOnlyPositive.Instance);
                if (allDefaultValues) {
                    _editableWithTextInput = true;
                    ScriptType = ScriptType.Nicht_vorhanden;
                    Align = AlignmentHorizontal.Rechts;
                    IgnoreAtRowFilter = true;
                    Caption = "Sortierung";
                    DefaultRenderer = "Button";
                    RendererSettings = "{ClassId=\"Button\", ShowPic=-, ShowText=+, ShowCheckState=-, Padding={-4, -2}}";
                    ForeColor = Color.FromArgb(0, 0, 0);
                    BackColor = Color.FromArgb(255, 255, 255);
                }
                break;

            default:
                Develop.DebugPrint("Unbekannte Kennung: " + _keyName);
                break;
        }
    }

    public bool SpellCheckingPossible() {
        if (_relationType == RelationType.DropDownValues) { return false; }
        if (_value_for_Chunk != ChunkType.None) { return false; }

        return true;
    }

    public void Statistik(ICollection<RowItem> rows, bool ignoreMultiLine) {
        if (IsDisposed || Table is not { IsDisposed: false }) { return; }

        if (rows.Count < 1) { return; }

        var d = new Dictionary<string, int>();

        var values = new List<string>();

        foreach (var thisRow in rows) {
            if (ignoreMultiLine) {
                var keyValue = thisRow.CellGetString(this);
                if (string.IsNullOrEmpty(keyValue)) { values.Add("[empty]"); } else {
                    values.Add(keyValue.Replace('\r', ';'));
                }
            } else {
                var keyValue = thisRow.CellGetList(this);
                if (keyValue.Count == 0) { values.Add("[empty]"); } else {
                    values.AddRange(keyValue);
                }
            }
        }

        foreach (var keyValue in values) {
            if (d.TryGetValue(keyValue, out var existingCount)) {
                d.Remove(keyValue);
            }
            d.Add(keyValue, existingCount + 1);
        }

        List<string> l =
        [
            "Statistik der vorkommenden Werte der Spalte: " + ReadableText(),
            " - nur aktuell angezeigte Zeilen",
            ignoreMultiLine ? " - Zelleninhalte werden als ganzes behandelt" : " - Zelleninhalte werden gesplittet",
            " "
        ];

        do {
            var maxCount = 0;
            var keyValue = string.Empty;

            foreach (var thisKey in d) {
                if (thisKey.Value > maxCount) {
                    keyValue = thisKey.Key;
                    maxCount = thisKey.Value;
                }
            }

            d.Remove(keyValue);
            l.Add(maxCount + " - " + keyValue);
        } while (d.Count > 0);

        l.WriteAllText(TempFile(string.Empty, string.Empty, "txt"), Encoding.UTF8, true);
    }

    public double? Summe(FilterCollection fc) => Summe(fc.Rows);

    public double? Summe(IEnumerable<RowItem>? rows) {
        if (rows is null) { return null; }

        double summ = 0;
        foreach (var thisrow in rows) {
            var val = thisrow.CellGetString(this);
            if (!val.IsNumeral()) { return null; }
            summ += DoubleParse(val);
        }
        return summ;
    }

    public QuickImage? SymbolForReadableText() {
        if (IsDisposed) { return QuickImage.Get(ImageCode.Warnung); }
        if (IsDisposed || Table is not { IsDisposed: false } tb) { return QuickImage.Get(ImageCode.Warnung); }

        if (this == tb.Column.SysRowChanger) { return QuickImage.Get(ImageCode.Person); }
        if (this == tb.Column.SysRowCreator) { return QuickImage.Get(ImageCode.Person); }
        if (this == tb.Column.SysRowCreateDate) { return QuickImage.Get(ImageCode.Uhr); }
        if (this == tb.Column.SysRowChangeDate) { return QuickImage.Get(ImageCode.Uhr); }

        if (this == tb.Column.SysLocked) { return QuickImage.Get(ImageCode.Schloss); }

        if (this == tb.Column.SysCorrect) { return QuickImage.Get(ImageCode.Warnung); }

        if (_isFirst) { return QuickImage.Get(ImageCode.Stern, 16); }
        if (_isKeyColumn) { return QuickImage.Get(ImageCode.Schlüssel, 16); }

        if (_value_for_Chunk != ChunkType.None) { return QuickImage.Get(ImageCode.Diskette, 16); }

        if (_relationship_to_First) { return QuickImage.Get(ImageCode.Herz, 16); }

        if (_relationType == RelationType.CellValues) { return QuickImage.Get(ImageCode.Fernglas, 16); }

        if (!_saveContent) { return QuickImage.Get(ImageCode.Tabelle, 16); }

        var match = FormatHolder.AllFormats.Instances.FirstOrDefault(f => f.IsFormatIdenticalSoft(this));
        if (match is not null) { return match.SymbolForReadableText(); }

        if (_editableWithDropdown) {
            return QuickImage.Get("Pfeil_Unten_Scrollbar|14|||||0");
        }

        return _multiLine ? QuickImage.Get(ImageCode.Textfeld, 16, Color.Red, Color.Transparent) :
                       QuickImage.Get(ImageCode.Textfeld);
    }

    public void SystemInfoReset(bool always) {
        if (always || string.IsNullOrEmpty(ColumnSystemInfo)) {
            ColumnSystemInfo = "Seit UTC: " + DateTime.UtcNow.ToString5();
        }
    }

    public override string ToString() => IsDisposed ? string.Empty : _keyName + " -> " + Caption;

    /// <summary>
    /// CallByFileName Aufrufe werden nicht geprüft
    /// </summary>
    /// <returns></returns>
    public bool UsedInScript() {
        if (IsDisposed || Table is not { IsDisposed: false }) { return false; }

        foreach (var thiss in Table.EventScript) {
            if (thiss.Script.IndexOfWord(_keyName, 0, RegexOptions.IgnoreCase) >= 0) { return true; }
        }

        return false;
    }

    public bool UserEditDialogTypeInFormula(EditTypeFormula editTypeToCheck) {
        if (!_saveContent) {
            if (editTypeToCheck == EditTypeFormula.Textfeld) { return true; } // Textfeld immer erlauben auch wenn beide Bearbeitungen nicht erlaubt sind. Einfach der Übersichtlichktei
            return false;
        }

        if (_value_for_Chunk != ChunkType.None) {
            if (editTypeToCheck == EditTypeFormula.als_Überschrift_anzeigen) { return true; }
            return false;
        }

        if (_relationType == RelationType.CellValues) {
            if (editTypeToCheck == EditTypeFormula.None) { return true; }
            var col = LinkedTable?.Column[_columnKeyOfLinkedTable];
            if (col is null) { return false; }
            return col.UserEditDialogTypeInFormula(editTypeToCheck);
        }

        if (_relationType == RelationType.DropDownValues) {
            if (editTypeToCheck == EditTypeFormula.Textfeld_mit_Auswahlknopf) { return true; }
            return false;
        }

        if (editTypeToCheck == EditTypeFormula.Textfeld) { return true; } // Textfeld immer erlauben auch wenn beide Bearbeitungen nicht erlaubt sind. Einfach der Übersichtlichktei
        if (_multiLine && editTypeToCheck == EditTypeFormula.Textfeld_mit_Auswahlknopf) { return false; }
        if (_editableWithDropdown && editTypeToCheck == EditTypeFormula.Textfeld_mit_Auswahlknopf) { return true; }
        if (_editableWithDropdown && _showValuesOfOtherCellsInDropdown && editTypeToCheck == EditTypeFormula.SwapListBox) { return true; }

        if (_multiLine && _editableWithDropdown && editTypeToCheck == EditTypeFormula.Listbox) { return true; }
        if (editTypeToCheck == EditTypeFormula.Textfeld_mit_Suggestions) {
            return _editableWithTextInput && (_dropDownItems.Count > 0 || _showValuesOfOtherCellsInDropdown);
        }
        if (editTypeToCheck == EditTypeFormula.nur_als_Text_anzeigen) { return true; }
        if (!_multiLine && editTypeToCheck == EditTypeFormula.Ja_Nein_Knopf) { return true; }
        return false;
    }

    internal void Optimize() {
        if (!IsSystemColumn()) {
            // Maximale Text-Länge beeinflusst stark die Ladezeit vom Server
            var l = CalculatePreveredMaxCellLength(1.2f);
            if (l < MaxCellLength) { MaxCellLength = l; }

            if (MaxTextLength > MaxCellLength) { MaxTextLength = MaxCellLength; }

            // ScriptType beeinflusst, ob eine Zeile neu durchgerechnet werden muss nach einer Änderung dieser Zelle
            if (!UsedInScript()) {
                ScriptType = ScriptType.Nicht_vorhanden;
            }

            //// Beeinflussst stark den Áufbau bei großen Zeilen.
            //// Aber erst ab 20, da es ansonsten wenige verschiedene Einträge sind, es und es sich nicht reniert.
            //// Zudem können zu kleine Werte ein Berechnungsfehler sein
            //Invalidate_ContentWidth();
            //if (_contentwidth is int v && v > 20 && FixedColumnWidth == 0) {
            //    FixedColumnWidth = v;
            //}
        }
    }

    /// <summary>
    /// Setzt den Wert in die dazugehörige Variable.
    /// </summary>
    /// <param name="type"></param>
    /// <param name="value"></param>
    /// <returns></returns>
    internal string SetValueInternal(TableDataType type, string value) {
        if (type.IsObsolete()) { return string.Empty; }

        switch (type) {
            case TableDataType.ColumnKey:
                var oldKey = _keyName;
                _keyName = value.ToUpperInvariant();
                var f = Table?.Column.ChangeKey(oldKey, _keyName) ?? "Tabelle verworfen";

                if (!string.IsNullOrEmpty(f)) {
                    var reason = $"Schwerer Spaltenkey Umbenennungsfehler, {f}";
                    Table?.Freeze(reason);
                    return reason;
                }
                break;

            case TableDataType.ColumnCaption:
                _caption = value;
                break;

            case TableDataType.ForeColor:
                _foreColor = Color.FromArgb(IntParse(value));
                break;

            case TableDataType.BackColor:
                _backColor = Color.FromArgb(IntParse(value));
                break;

            case TableDataType.LineStyleLeft:
                _lineStyleLeft = (ColumnLineStyle)IntParse(value);
                break;

            case TableDataType.LineStyleRight:
                _lineStyleRight = (ColumnLineStyle)IntParse(value);
                break;

            case TableDataType.BackgroundStyle:
                _backgroundStyle = (ColumnBackgroundStyle)IntParse(value);
                break;

            case TableDataType.ColumnQuickInfo:
                _quickInfo = value;
                break;

            case TableDataType.CaptionGroup1:
                _captionGroup1 = value;
                break;

            case TableDataType.CaptionGroup2:
                _captionGroup2 = value;
                break;

            case TableDataType.CaptionGroup3:
                _captionGroup3 = value;
                break;

            case TableDataType.MultiLine:
                _multiLine = value.FromPlusMinus();
                break;

            case TableDataType.IsFirst:
                _isFirst = value.FromPlusMinus();
                Table?.Column.GetSystems();
                break;

            case TableDataType.IsKeyColumn:
                _isKeyColumn = value.FromPlusMinus();
                break;

            case TableDataType.Relationship_to_First:
                _relationship_to_First = value.FromPlusMinus();
                break;

            case TableDataType.DropDownItems:
                _dropDownItems.SplitAndCutByCr_QuickSortAndRemoveDouble(value);
                break;

            case TableDataType.LinkedCellFilter:
                _linkedCellFilter.Clear();
                _linkedCellFilter.AddRange(value.SplitAndCutByCr());
                break;

            case TableDataType.AutoReplaceAfterEdit:
                _afterEditAutoReplace.Clear();
                _afterEditAutoReplace.AddRange(value.SplitAndCutByCr());
                break;

            case TableDataType.RegexCheck:
                _regexCheck = value;
                break;

            case TableDataType.ColumnTags:
                _columnTags = value;
                break;

            case TableDataType.AutoFilterJoker:
                _autoFilterJoker = value;
                break;

            case TableDataType.PermissionGroupsChangeCell:
                _permissionGroupsChangeCell.SplitAndCutByCr_QuickSortAndRemoveDouble(value);
                break;

            case TableDataType.AllowedChars:
                _allowedChars = value;
                break;

            case TableDataType.MaxTextLength:
                _maxTextLength = IntParse(value);
                break;

            case TableDataType.FilterOptions:
                _filterOptions = (FilterOptions)IntParse(value);
                break;

            case TableDataType.RelationType:
                _relationType = (RelationType)IntParse(value);
                break;

            case TableDataType.Value_for_Chunk:
                _value_for_Chunk = (ChunkType)IntParse(value);
                Table?.Column.GetSystems();
                break;

            case TableDataType.IgnoreAtRowFilter:
                _ignoreAtRowFilter = value.FromPlusMinus();
                break;

            case TableDataType.SaveContent:
                _saveContent = value.FromPlusMinus();
                break;

            case TableDataType.EditableWithTextInput:
                _editableWithTextInput = value.FromPlusMinus();
                break;

            case TableDataType.EditableWithDropdown:
                _editableWithDropdown = value.FromPlusMinus();
                break;

            case TableDataType.SpellCheckingEnabled:
                _spellCheckingEnabled = value.FromPlusMinus();
                break;

            case TableDataType.ValueRequired:
                _valueRequired = value.FromPlusMinus();
                break;

            case TableDataType.ShowValuesOfOtherCellsInDropdown:
                _showValuesOfOtherCellsInDropdown = value.FromPlusMinus();
                break;

            case TableDataType.SortAndRemoveDoubleAfterEdit:
                _afterEditQuickSortRemoveDouble = value.FromPlusMinus();
                break;

            case TableDataType.AfterEditRound:
                _afterEditRound = IntParse(value);
                break;

            case TableDataType.FixedColumnWidth:
                _fixedColumnWidth = IntParse(value);
                break;

            case TableDataType.MaxCellLength:
                _maxCellLength = IntParse(value);
                break;

            case TableDataType.DoUcaseAfterEdit:
                _afterEditDoUCase = value.FromPlusMinus();
                break;

            case TableDataType.AutoCorrectAfterEdit:
                _afterEditAutoCorrect = value.FromPlusMinus();
                break;

            case TableDataType.AfterEditAutoRemoveChar:
                _afterEditAutoRemoveChar = value;
                break;

            case TableDataType.ColumnAdminInfo:
                _adminInfo = value;
                break;

            case TableDataType.ColumnSystemInfo:
                _columnSystemInfo = value;
                break;

            case TableDataType.RendererSettings:
                _rendererSettings = value;
                break;

            case TableDataType.DefaultRenderer:
                _defaultRenderer = value;
                break;

            case TableDataType.CaptionBitmapCode:
                _captionBitmapCode = value;
                break;

            case TableDataType.LinkedTableTableName:
                _linkedTableTableName = value;
                Invalidate_LinkedTable();
                break;

            case TableDataType.DoOpticalTranslation:
                _doOpticalTranslation = (TranslationType)IntParse(value);
                break;

            case TableDataType.AdditionalFormatCheck:
                _additionalFormatCheck = (AdditionalCheck)IntParse(value);
                break;

            case TableDataType.ScriptType:
                _scriptType = (ScriptType)IntParse(value);
                Table?.Row.InvalidateAllCheckData();
                break;

            case TableDataType.EditAllowedDespiteLock:
                _editAllowedDespiteLock = value.FromPlusMinus();
                break;

            case TableDataType.TextFormatingAllowed:
                _textFormatingAllowed = value.FromPlusMinus();
                break;

            case TableDataType.ColumnKeyOfLinkedTable:

                _columnKeyOfLinkedTable = value.IsFormat(FormatHolder_Long.Instance) ? string.Empty : value;

                break;

            case TableDataType.SortType:
                _sortType = string.IsNullOrEmpty(value) ? SortierTyp.Original_String : (SortierTyp)LongParse(value);
                break;

            case TableDataType.ColumnAlign:
                var tmpalign = (AlignmentHorizontal)IntParse(value);
                if (tmpalign == (AlignmentHorizontal)(-1)) { tmpalign = AlignmentHorizontal.Links; }
                _align = tmpalign;
                break;

            default:
                if (!string.Equals(type.ToString(), ((int)type).ToString1(), StringComparison.Ordinal)) {
                    return "Interner Fehler: Für den Datentyp '" + type + "' wurde keine Laderegel definiert.";
                }
                break;
        }
        return string.Empty;
    }

    private static string KleineFehlerCorrect(string txt) {
        if (string.IsNullOrEmpty(txt)) { return string.Empty; }
        //if (TextFormatingAllowed) { txt = txt.HtmlSpecialToNormalChar(false); }
        string oTxt;
        do {
            oTxt = txt;
            if (oTxt.Contains(".at", StringComparison.OrdinalIgnoreCase)) { break; }
            if (oTxt.Contains(".de", StringComparison.OrdinalIgnoreCase)) { break; }
            if (oTxt.Contains(".com", StringComparison.OrdinalIgnoreCase)) { break; }
            if (oTxt.Contains("http", StringComparison.OrdinalIgnoreCase)) { break; }
            if (oTxt.Contains("ftp", StringComparison.OrdinalIgnoreCase)) { break; }
            if (oTxt.Contains(".xml", StringComparison.OrdinalIgnoreCase)) { break; }
            if (oTxt.Contains(".doc", StringComparison.OrdinalIgnoreCase)) { break; }
            if (oTxt.IsDateTime()) { break; }
            txt = txt.Replace("\r\n", "\r");
            // 1/2 l Milch
            // 3-5 Stunden
            // 180°C
            // Nach Zahlen KEINE leerzeichen einfügen. Es gibt so viele dinge.... 90er Schichtsalat
            txt = txt.Insert(' ', ',', "1234567890, \r");
            txt = txt.Insert(' ', '!', " !?)\r");
            txt = txt.Insert(' ', '?', " !?)\r");
            txt = txt.Insert(' ', '.', " 1234567890.!?/)\r");
            txt = txt.Insert(' ', ')', " .;!?\r");
            // txt = txt.Insert(" ", ";", " 1234567890\r"); ----> t&ouml;t gibt probleme
            txt = txt.Insert(' ', ':', "1234567890 \\/\r"); // auch 3:50 Uhr
            txt = txt.Replace(" °", "°");
            txt = txt.Replace(" .", ".");
            txt = txt.Replace(" ,", ",");
            txt = txt.Replace(" :", ":");
            txt = txt.Replace(" ?", "?");
            txt = txt.Replace(" !", "!");
            txt = txt.Replace(" )", ")");
            txt = txt.Replace("( ", "(");
            txt = txt.Replace("/ ", "/");
            txt = txt.Replace(" /", "/");
            txt = txt.Replace("\r ", "\r");
            txt = txt.Replace(" \r", "\r");
            txt = txt.Replace("     ", " "); // Wenn das hier nicht da ist, passieren wirklich fehler...
            txt = txt.Replace("    ", " ");
            txt = txt.Replace("   ", " "); // Wenn das hier nicht da ist, passieren wirklich fehler...
            txt = txt.Replace("  ", " ");
            txt = txt.Trim(' ');
            txt = txt.Trim('\r');
            txt = txt.TrimEnd('\t');
        } while (oTxt != txt);

        //if (TextFormatingAllowed) {
        //    txt = txt.CreateHtmlCodes(true);
        //    txt = txt.Replace("<br>", "\r");
        //}
        return txt;
    }

    private void _table_Disposing(object? sender, System.EventArgs e) => Dispose();

    private void CheckIfIAmAKeyColumn() {
        Am_A_Key_For.Clear();

        if (IsDisposed || Table is not { IsDisposed: false } tb) { return; }

        foreach (var c in tb.Column) {
            if (c.RelationType == RelationType.CellValues) {
                foreach (var thisitem in c.LinkedCellFilter) {
                    var tmp = thisitem.SplitBy("|");

                    if (tmp[2].Contains($"~{_keyName}~", StringComparison.OrdinalIgnoreCase)) {
                        Am_A_Key_For.Add(c.KeyName);
                    }
                }
            }
        }
    }

    private void Dispose(bool disposing) {
        if (Interlocked.CompareExchange(ref _isDisposedFlag, 1, 0) != 0) { return; }

        if (disposing) {
            OnDisposingEvent();
            DisposingEvent = null;
            PropertyChanged = null;
            Table = null;
            Invalidate_LinkedTable();
        }

        _afterEditAutoReplace.Clear();
        _dropDownItems.Clear();
        _linkedCellFilter.Clear();
        _permissionGroupsChangeCell.Clear();
    }

    private string? ErrorReason_Editing() {
        if (_spellCheckingEnabled && !SpellCheckingPossible()) { return SpellCheckNotPossible; }
        if (_editAllowedDespiteLock && !_editableWithTextInput && !_editableWithDropdown) { return EditDespiteLockNeedsMethod; }
        var tmpEditDialog = UserEditDialogTypeInTable(this, false, true);

        if (_editableWithTextInput) {
            if (tmpEditDialog == EditTypeTable.Dropdown_Single) { return FormatDropdownOnly; }
            if (tmpEditDialog == EditTypeTable.None) { return FormatNoStandardEdit; }
        }

        if (_editableWithDropdown) {
            if (tmpEditDialog == EditTypeTable.None) { return FormatNoDropdownEdit; }
        }

        if (!_editableWithDropdown && !_editableWithTextInput) {
            if (_permissionGroupsChangeCell.Count > 0) { return RemoveEditPermissions; }
            if (_showValuesOfOtherCellsInDropdown) { return DropdownNotSelectedAddAll; }
            if (_dropDownItems.Count > 0) { return DropdownNotSelectedItems; }
        }

        foreach (var thisS in _permissionGroupsChangeCell) {
            if (thisS.Contains('|')) { return InvalidGroupChar; }
            if (string.Equals(thisS, Administrator, StringComparison.OrdinalIgnoreCase)) { return AdministratorNotAllowed; }
        }

        if (_editableWithDropdown || tmpEditDialog == EditTypeTable.Dropdown_Single) {
            if (_relationType != RelationType.DropDownValues) {
                if (!_showValuesOfOtherCellsInDropdown && _dropDownItems.Count == 0) { return NoDropdownItems; }
            }
        }

        if (_showValuesOfOtherCellsInDropdown && !DropdownItemsOfOtherCellsAllowed()) { return AddOtherCellsNotAllowed; }
        if (!_valueRequired && !EmptyPossible()) { return EmptyNotAllowed; }
        return null;
    }

    private string? ErrorReason_Filters() {
        if (_filterOptions != FilterOptions.None && !_filterOptions.HasFlag(FilterOptions.Enabled)) { return FilterCombinationInvalid; }
        if (_filterOptions != FilterOptions.Enabled_OnlyAndAllowed && _filterOptions.HasFlag(FilterOptions.OnlyAndAllowed)) { return FilterCombinationInvalid; }
        if (_filterOptions != FilterOptions.Enabled_OnlyOrAllowed && _filterOptions.HasFlag(FilterOptions.OnlyOrAllowed)) { return FilterCombinationInvalid; }
        if (_filterOptions.HasFlag(FilterOptions.OnlyAndAllowed) || _filterOptions.HasFlag(FilterOptions.OnlyOrAllowed)) {
            if (!_multiLine) {
                return FilterRequiresMultiline;
            }
        }

        return null;
    }

    private string? ErrorReason_KeyAndSizes(Table tb) {
        if (string.IsNullOrEmpty(_keyName)) { return ColumnKeyUndefined; }
        if (!IsValidColumnKey(_keyName)) { return ColumnKeyInvalid; }
        if (_maxCellLength < _maxTextLength) { return CellSizeTooSmall; }
        if (_maxCellLength < 1) { return CellSizeTooSmall; }
        if (_maxCellLength > 4000) { return CellSizeTooLarge; }
        if (_maxTextLength > 4000) { return MaxLengthTooLarge; }

        if (tb.Column is { IsDisposed: false } col && col.Any(thisColumn => thisColumn != this && thisColumn is not null && string.Equals(_keyName, thisColumn._keyName, StringComparison.OrdinalIgnoreCase))) {
            return ColumnKeyDuplicate;
        }

        if (string.IsNullOrEmpty(_caption)) { return CaptionMissing; }
        if (_scriptType == ScriptType.undefiniert) { return ScriptTypeUndefined; }
        if (string.IsNullOrEmpty(_defaultRenderer)) { return RendererMissing; }
        return null;
    }

    private string? ErrorReason_Multiline() {
        if (_multiLine) {
            if (!MultilinePossible()) { return MultilineNotSupported; }
            if (_afterEditRound != -1) { return RoundOnlySingleLine; }
        } else {
            if (_afterEditQuickSortRemoveDouble) { return SortOnlyMultiline; }
        }

        return null;
    }

    private string? ErrorReason_NoSaveContent() {
        if (_saveContent) { return null; }
        if (_fixedColumnWidth < 16) { return FixedWidthRequired; }
        if (!_ignoreAtRowFilter) { return MustIgnoreRowFilter; }
        if (_isKeyColumn) { return KeyColumnMustSaveContent; }
        if (_isFirst) { return FirstColumnMustSaveContent; }
        if (_value_for_Chunk != ChunkType.None) { return ChunkMustSaveContent; }
        if (_relationType != RelationType.None) { return LinkedMustSaveContent; }
        if (_relationship_to_First) { return RelationMustSaveContent; }
        return null;
    }

    private string? ErrorReason_PostChecks() {
        if (_afterEditRound > 5) { return RoundMaxFiveDecimals; }
        if (_filterOptions == FilterOptions.None) {
            if (!string.IsNullOrEmpty(_autoFilterJoker)) { return NoAutoFilterRemoveJoker; }
        }

        if (_relationType == RelationType.DropDownValues) {
            if (_afterEditRound != -1 || _afterEditAutoReplace.Count > 0 || _afterEditAutoCorrect || _afterEditDoUCase || _afterEditQuickSortRemoveDouble || !string.IsNullOrEmpty(_allowedChars)) {
                return RelationNoAutoEdit;
            }
        }

        if (_relationType != RelationType.None && Table?.UniqueValues is { } uv) {
            if (uv.Any(uvd => uvd.KeyColumns.Contains(this))) { return LinkedColumnInUniqueDefinition; }
        }

        if (!_valueRequired) {
            if (_scriptType is ScriptType.Bool or ScriptType.Numeral) { return ValueRequiredMissingScript; }
            if (_isFirst) { return ValueRequiredMissingFirst; }
        }

        return null;
    }

    private string? ErrorReason_ChapterColumn() {
        if (Table is not { IsDisposed: false } tb) { return null; }
        if (tb.Column.SysRowSortIndex is not { IsDisposed: false }) { return null; }
        if (!IsChapterColumn()) { return null; }
        if (_multiLine) { return ChapterColumnMultilineWithRowSort; }
        return null;
    }

    private string? ErrorReason_Relations(Table tb) {
        if (_relationType != RelationType.None) {
            if (LinkedTable is not { IsDisposed: false } l_tb) { return LinkedTableMissing; }
            if (tb == l_tb) { return CircularReference; }
            var c = l_tb.Column[_columnKeyOfLinkedTable];
            if (c is null) { return LinkedKeyColumnMissing; }
            if (LinkedCellFilter.Count == 0) {
                if (_relationType != RelationType.DropDownValues) {
                    return NoLinkedFilterDefined;
                }
            }

            if (_relationType == RelationType.CellValues) {
                if (_scriptType is not ScriptType.Nicht_vorhanden) {
                    return LinkedCellScriptInvalid;
                }

                var linkCheck = CellCollection.ValidateLinkedCellFilterConfig(l_tb, this);
                if (linkCheck.IsFailed) {
                    return $"{CellLinkError}: {linkCheck.FailedReason}";
                }
            }
        } else {
            if (!string.IsNullOrEmpty(_columnKeyOfLinkedTable)) { return LinkedDataOnlyWithLinkedCells; }
        }

        return null;
    }

    private string? ErrorReason_RelationshipToFirst(Table tb) {
        if (!_relationship_to_First) { return null; }
        if (!_multiLine) { return RelationRequiresMultiline; }
        if (tb.Column.First == this) { return RelationNotAllowedOnFirstColumn; }
        return null;
    }

    private string? ErrorReason_SpecialColumns() {
        if (_isFirst) {
            if (_relationship_to_First || _relationType != RelationType.None) {
                return FirstColumnNoRelation;
            }
        }

        if (_isKeyColumn) {
            if (_relationship_to_First) {
                return KeyColumnNoRowRelation;
            }

            if (_scriptType is not ScriptType.String_Readonly and not ScriptType.Bool_Readonly and not ScriptType.List_Readonly and not ScriptType.Numeral_Readonly and not ScriptType.Nicht_vorhanden &&
                _relationType != RelationType.CellValues) {
                return KeyColumnScriptReadonly;
            }
        }

        if (_value_for_Chunk != ChunkType.None) {
            if (Table is not TableChunk) {
                return ChunkOnlyInCbcb;
            }

            if (_scriptType is not ScriptType.String_Readonly and not ScriptType.Bool_Readonly and not ScriptType.Nicht_vorhanden and not ScriptType.List_Readonly and not ScriptType.Numeral_Readonly) {
                return ChunkScriptReadonly;
            }

            if (!string.IsNullOrEmpty(_autoFilterJoker)) { return ChunkAutoFilterJokerInvalid; }
            if (_filterOptions.HasFlag(FilterOptions.ExtendedFilterEnabled)) { return ChunkExtendedFilterInvalid; }
            if (!_filterOptions.HasFlag(FilterOptions.TextFilterEnabled)) { return ChunkTextFilterRequired; }
            if (!_ignoreAtRowFilter) { return ChunkMustIgnoreRowFilter; }
            if (!_filterOptions.HasFlag(FilterOptions.Enabled)) { return ChunkAutoFilterRequired; }

            if (_relationship_to_First || _relationType != RelationType.None) {
                return ChunkNoRelation;
            }
        }

        return null;
    }

    private void Invalidate_LinkedTable() {
        Table? tableToCleanup = null;

        lock (_linkedTableLock) {
            tableToCleanup = _linkedTable;
            _linkedTable = null; // Sofort auf null setzen (fail-fast)
        }

        // Event-Abmeldung außerhalb des Locks um Deadlocks zu vermeiden
        if (tableToCleanup is not null) {
            try {
                tableToCleanup.CellValueChanged -= LinkedTable_CellValueChanged;
            } catch { }

            try {
                tableToCleanup.DisposingEvent -= LinkedTable_Disposing;
            } catch { }
        }
    }

    private void LinkedTable_CellValueChanged(object? sender, CellEventArgs e) {
        if (e.Column.KeyName != ColumnKeyOfLinkedTable) { return; }
        if (_relationType != RelationType.CellValues) { return; }

        var (fc, info) = CellCollection.GetFilterReverse(this, e.Column, e.Row);
        var val = e.Row.CellGetString(e.Column);

        if (fc is not null && string.IsNullOrWhiteSpace(info)) {
            foreach (var thisRow in fc.Rows) {
                if (thisRow.CellGetStringCore(this) != val) {
                    thisRow.LinkedCellData(this, true, false);
                }
            }
            fc.Dispose();
        }
    }

    private void LinkedTable_Disposing(object? sender, System.EventArgs e) => Invalidate_LinkedTable();

    private void OnDisposingEvent() => DisposingEvent?.Invoke(this, System.EventArgs.Empty);

    private void OnPropertyChanged([CallerMemberName] string propertyName = "unknown") => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

    #endregion

    //private void ManipulateRendererSettings(string settingname, string newvalue) {
    //    if (string.IsNullOrEmpty(newvalue)) { return; }

    //    string tmp2 = string.Empty;

    //    if (!string.IsNullOrEmpty(_rendererSettings)) { tmp2 = ", "; }

    //    tmp2 = tmp2 + settingname + "=" + newvalue.ToNonCriticalWithQuote();

    //    var tmp = _rendererSettings;

    //    if (string.IsNullOrEmpty(tmp)) { tmp = "{}"; }

    //    tmp = tmp.Substring(0, tmp.Length - 1) + tmp2 + "}";

    //    RendererSettings = tmp;
    //}
}