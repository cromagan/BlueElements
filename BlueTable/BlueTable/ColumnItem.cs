// Authors:
// Christian Peter
//
// Copyright (c) 2025 Christian Peter
// https://github.com/cromagan/BlueElements
//
// License: GNU Affero General Public License v3.0
// https://github.com/cromagan/BlueElements/blob/master/LICENSE
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL
// THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING
// FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER
// DEALINGS IN THE SOFTWARE.

#nullable enable

using BlueBasics;
using BlueBasics.Enums;
using BlueBasics.Interfaces;
using BlueTable.Enums;
using BlueTable.EventArgs;
using BlueTable.Interfaces;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.RegularExpressions;
using static BlueBasics.Constants;
using static BlueBasics.Converter;
using static BlueBasics.IO;
using static BlueTable.Table;

namespace BlueTable;

public sealed class ColumnItem : IReadableTextWithPropertyChangingAndKey, IColumnInputFormat, IErrorCheckable, IHasTable, IDisposableExtendedWithEvent, IEditable {

    #region Fields

    internal List<string>? UcaseNamesSortedByLenght;

    private const string TmpNewDummy = "TMPNEWDUMMY";

    private readonly List<string> _afterEditAutoReplace = [];

    private readonly List<string> _columnTags = [];

    private readonly List<string> _dropDownItems = [];

    private readonly List<string> _linkedCellFilter = [];

    private readonly object _linkedTableLock = new object();

    private readonly List<string> _permissionGroupsChangeCell = [];

    private AdditionalCheck _additionalFormatCheck;

    private string _adminInfo;

    private bool _afterEditAutoCorrect;

    private bool _afterEditDoUCase;

    private bool _afterEditQuickSortRemoveDouble;

    private AlignmentHorizontal _align;

    private string _allowedChars;

    private string _autoFilterJoker;

    private string _autoRemove;

    private Color _backColor;

    private string _caption;

    private string _captionBitmapCode;

    private string _captionGroup1;

    private string _captionGroup2;

    private string _captionGroup3;

    /// <summary>
    /// Die Quell-Spalte (aus der verlinkten Tabelle) ist immer
    /// </summary>
    private string _columnNameOfLinkedTable;

    private string _columnQuickInfo;

    private string _columnSystemInfo;

    private string _defaultRenderer;

    private TranslationType _doOpticalTranslation;

    private bool _dropdownDeselectAllAllowed;

    private bool _editableWithDropdown;

    private bool _editableWithTextInput;

    private bool _editAllowedDespiteLock;

    private FilterOptions _filterOptions;

    private int _fixedColumnWidth;

    private Color _foreColor;

    private bool _ignoreAtRowFilter;

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

    private int _maxCellLenght;

    private int _maxTextLenght;

    private bool _multiLine;

    private string _regexCheck = string.Empty;

    private bool _relationship_to_First;

    private RelationType _relationType;

    private string _rendererSettings;

    private int _roundAfterEdit;

    private bool _saveContent;

    private ScriptType _scriptType;

    private bool _showValuesOfOtherCellsInDropdown;

    private SortierTyp _sortType;

    private bool _spellCheckingEnabled;

    //private string _cellInitValue;
    private Table? _table;

    private bool _textFormatingAllowed;
    private ChunkType _value_for_Chunk;

    #endregion

    #region Constructors

    public ColumnItem(Table table, string name) {
        if (!IsValidColumnName(name)) {
            Develop.DebugPrint(ErrorType.Error, "Spaltenname nicht erlaubt!");
        }

        Table = table;

        var ex = table.Column[name];
        if (ex != null) {
            Develop.DebugPrint(ErrorType.Error, "Key existiert bereits");
        }

        #region Standard-Werte

        _keyName = name;
        _caption = string.Empty;
        _lineStyleLeft = ColumnLineStyle.Dünn;
        _lineStyleRight = ColumnLineStyle.Ohne;
        _multiLine = false;
        _isKeyColumn = false;
        _relationship_to_First = false;
        _relationType = RelationType.None;
        _value_for_Chunk = ChunkType.None;
        _isFirst = false;
        _columnQuickInfo = string.Empty;
        _captionGroup1 = string.Empty;
        _captionGroup2 = string.Empty;
        _captionGroup3 = string.Empty;
        //_Intelligenter_Multifilter = string.Empty;
        _foreColor = Color.Black;
        _backColor = Color.White;
        //_cellInitValue = string.Empty;
        //_linkedCellRowKeyIsInColumn = -1;
        _columnNameOfLinkedTable = string.Empty;
        _sortType = SortierTyp.Original_String;
        //_ZellenZusammenfassen = false;
        //_dropDownKey = -1;
        //_vorschlagsColumn = -1;
        _align = AlignmentHorizontal.Links;
        //_keyColumnKey = -1;
        _allowedChars = string.Empty;
        _adminInfo = string.Empty;
        _columnSystemInfo = string.Empty;
        _defaultRenderer = string.Empty;
        _rendererSettings = string.Empty;
        _maxTextLenght = 4000;
        _maxCellLenght = 4000;
        //ContentWidthIsValid = false;
        _captionBitmapCode = string.Empty;
        _filterOptions = FilterOptions.Enabled | FilterOptions.TextFilterEnabled | FilterOptions.ExtendedFilterEnabled;
        //_AutofilterErlaubt = true;
        //_AutofilterTextFilterErlaubt = true;
        //_AutoFilterErweitertErlaubt = true;
        _ignoreAtRowFilter = false;
        _editableWithDropdown = false;
        _dropdownDeselectAllAllowed = false;
        _editableWithTextInput = false;
        _showValuesOfOtherCellsInDropdown = false;
        _afterEditQuickSortRemoveDouble = false;
        _roundAfterEdit = -1;
        _fixedColumnWidth = 0;
        _afterEditAutoCorrect = false;
        _afterEditDoUCase = false;
        _textFormatingAllowed = false;
        _additionalFormatCheck = AdditionalCheck.None;
        _scriptType = ScriptType.undefiniert;
        _autoRemove = string.Empty;
        _autoFilterJoker = string.Empty;
        _saveContent = true;
        //_AutoFilter_Dauerfilter = enDauerfilter.ohne;
        _spellCheckingEnabled = false;
        //_CompactView = true;
        _doOpticalTranslation = TranslationType.Original_Anzeigen;
        _editAllowedDespiteLock = false;
        _linkedTableTableName = string.Empty;
        UcaseNamesSortedByLenght = null;
        Am_A_Key_For_Other_Column = string.Empty;

        #endregion Standard-Werte

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

    //private string _vorschlagsColumn;
    // TODO: Finalizer nur überschreiben, wenn "Dispose(bool disposing)" Code für die Freigabe nicht verwalteter Ressourcen enthält
    public event PropertyChangedEventHandler? PropertyChanged;

    #endregion

    #region Properties

    public AdditionalCheck AdditionalFormatCheck {
        get => _additionalFormatCheck;
        set {
            if (IsDisposed) { return; }
            if (_additionalFormatCheck == value) { return; }

            _ = Table?.ChangeData(TableDataType.AdditionalFormatCheck, this, ((int)_additionalFormatCheck).ToString(), ((int)value).ToString());
            OnPropertyChanged();
        }
    }

    [Description("Ein Information für Administratoren. Freier Text.\r\nWird als Quickinfo angezeigt, wenn der Admininstror\r\n mit der Maus über den Spaltenkopf fährt.")]
    public string AdminInfo {
        get => _adminInfo;
        set {
            if (IsDisposed) { return; }
            if (_adminInfo == value) { return; }

            _ = Table?.ChangeData(TableDataType.ColumnAdminInfo, this, _adminInfo, value);
            OnPropertyChanged();
        }
    }

    public bool AfterEditAutoCorrect {
        get => _afterEditAutoCorrect;
        set {
            if (IsDisposed) { return; }
            if (_afterEditAutoCorrect == value) { return; }

            _ = Table?.ChangeData(TableDataType.AutoCorrectAfterEdit, this, _afterEditAutoCorrect.ToPlusMinus(), value.ToPlusMinus());
            OnPropertyChanged();
        }
    }

    //    #region Standard-Werte
    public ReadOnlyCollection<string> AfterEditAutoReplace {
        get => new(_afterEditAutoReplace);
        set {
            if (IsDisposed) { return; }
            if (!_afterEditAutoReplace.IsDifferentTo(value)) { return; }

            _ = Table?.ChangeData(TableDataType.AutoReplaceAfterEdit, this, _afterEditAutoReplace.JoinWithCr(), value.JoinWithCr());
            OnPropertyChanged();
        }
    }

    //    _key = columnkey;
    public bool AfterEditDoUCase {
        get => _afterEditDoUCase;
        set {
            if (IsDisposed) { return; }
            if (_afterEditDoUCase == value) { return; }

            _ = Table?.ChangeData(TableDataType.DoUcaseAfterEdit, this, _afterEditDoUCase.ToPlusMinus(), value.ToPlusMinus());
            OnPropertyChanged();
        }
    }

    //    var ex = table.Column.SearchByKey(columnkey);
    //    if (ex != null) {
    //        Develop.DebugPrint(ErrorType.Error, "_name existiert bereits");
    //    }
    public bool AfterEditQuickSortRemoveDouble {
        get => _multiLine && _afterEditQuickSortRemoveDouble;
        set {
            if (IsDisposed) { return; }
            if (_afterEditQuickSortRemoveDouble == value) { return; }

            _ = Table?.ChangeData(TableDataType.SortAndRemoveDoubleAfterEdit, this, _afterEditQuickSortRemoveDouble.ToPlusMinus(), value.ToPlusMinus());
            OnPropertyChanged();
        }
    }

    public AlignmentHorizontal Align {
        get => _align;
        set {
            if (IsDisposed) { return; }
            if (_align == value) { return; }

            _ = Table?.ChangeData(TableDataType.ColumnAlign, this, ((int)_align).ToString(), ((int)value).ToString());
            OnPropertyChanged();
        }
    }

    public string AllowedChars {
        get => _allowedChars;
        set {
            if (IsDisposed) { return; }
            if (_allowedChars == value) { return; }

            _ = Table?.ChangeData(TableDataType.AllowedChars, this, _allowedChars, value);
            OnPropertyChanged();
        }
    }

    public string Am_A_Key_For_Other_Column { get; private set; }

    public string AutoFilterJoker {
        get => _autoFilterJoker;
        set {
            if (IsDisposed) { return; }
            if (_autoFilterJoker == value) { return; }

            _ = Table?.ChangeData(TableDataType.AutoFilterJoker, this, _autoFilterJoker, value);
            OnPropertyChanged();
        }
    }

    public string AutoRemove {
        get => _autoRemove;
        set {
            if (IsDisposed) { return; }
            if (_autoRemove == value) { return; }

            _ = Table?.ChangeData(TableDataType.AutoRemoveCharAfterEdit, this, _autoRemove, value);
            OnPropertyChanged();
        }
    }

    public Color BackColor {
        get => _backColor;
        set {
            if (IsDisposed) { return; }
            if (_backColor.ToArgb() == value.ToArgb()) { return; }

            _ = Table?.ChangeData(TableDataType.BackColor, this, _backColor.ToArgb().ToString(), value.ToArgb().ToString());
            OnPropertyChanged();
        }
    }

    public string Caption {
        get => _caption;
        set {
            if (IsDisposed) { return; }
            value = value.Replace("<br>", "\r", RegexOptions.IgnoreCase);
            if (_caption == value) { return; }

            _ = Table?.ChangeData(TableDataType.ColumnCaption, this, _caption, value);
            OnPropertyChanged();
        }
    }

    public string CaptionBitmapCode {
        get => _captionBitmapCode;
        set {
            if (IsDisposed) { return; }
            if (_captionBitmapCode == value) { return; }

            _ = Table?.ChangeData(TableDataType.CaptionBitmapCode, this, _captionBitmapCode, value);
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

            _ = Table?.ChangeData(TableDataType.CaptionGroup1, this, _captionGroup1, value);
            OnPropertyChanged();
        }
    }

    public string CaptionGroup2 {
        get => _captionGroup2;
        set {
            if (IsDisposed) { return; }
            if (_captionGroup2 == value) { return; }

            _ = Table?.ChangeData(TableDataType.CaptionGroup2, this, _captionGroup2, value);
            OnPropertyChanged();
        }
    }

    public string CaptionGroup3 {
        get => _captionGroup3;
        set {
            if (IsDisposed) { return; }
            if (_captionGroup3 == value) { return; }

            _ = Table?.ChangeData(TableDataType.CaptionGroup3, this, _captionGroup3, value);
            OnPropertyChanged();
        }
    }

    public string CaptionsCombined {
        get {
            var txt = _captionGroup1 + "/" + _captionGroup2 + "/" + _captionGroup3;
            return txt == "//" ? "###" : txt.TrimEnd("/");
        }
    }

    public string ColumnNameOfLinkedTable {
        get => _columnNameOfLinkedTable;
        set {
            if (IsDisposed) { return; }
            if (value == "-1") { value = string.Empty; }

            if (_columnNameOfLinkedTable == value) { return; }

            _ = Table?.ChangeData(TableDataType.ColumnNameOfLinkedTable, this, _columnNameOfLinkedTable, value);
            Invalidate_ColumAndContent();
            OnPropertyChanged();
        }
    }

    public string ColumnQuickInfo {
        get => _columnQuickInfo;
        set {
            if (IsDisposed) { return; }
            if (_columnQuickInfo == value) { return; }

            _ = Table?.ChangeData(TableDataType.ColumnQuickInfo, this, _columnQuickInfo, value);
            OnPropertyChanged();
        }
    }

    public string ColumnSystemInfo {
        get => _columnSystemInfo;
        private set {
            if (IsDisposed) { return; }
            if (_columnSystemInfo == value) { return; }

            _ = Table?.ChangeData(TableDataType.ColumnSystemInfo, this, _adminInfo, value);
            OnPropertyChanged();
        }
    }

    /// <summary>
    /// Was in Textfeldern oder Tabellezeilen für ein Suffix angezeigt werden soll. Beispiel: mm
    /// </summary>
    public List<string> ColumnTags {
        get => _columnTags;
        set {
            if (IsDisposed) { return; }
            if (!_columnTags.IsDifferentTo(value)) { return; }

            _ = Table?.ChangeData(TableDataType.ColumnTags, this, _columnTags.JoinWithCr(), value.JoinWithCr());
            OnPropertyChanged();
        }
    }

    public string DefaultRenderer {
        get => _defaultRenderer;
        set {
            if (IsDisposed) { return; }
            if (_defaultRenderer == value) { return; }

            _ = Table?.ChangeData(TableDataType.DefaultRenderer, this, _defaultRenderer, value);
            OnPropertyChanged();
        }
    }

    public TranslationType DoOpticalTranslation {
        get => _doOpticalTranslation;
        set {
            if (IsDisposed) { return; }
            if (_doOpticalTranslation == value) { return; }

            _ = Table?.ChangeData(TableDataType.DoOpticalTranslation, this, ((int)_doOpticalTranslation).ToString(), ((int)value).ToString());
            OnPropertyChanged();
        }
    }

    public bool DropdownDeselectAllAllowed {
        get => _dropdownDeselectAllAllowed;
        set {
            if (IsDisposed) { return; }
            if (_dropdownDeselectAllAllowed == value) { return; }

            _ = Table?.ChangeData(TableDataType.DropdownDeselectAllAllowed, this, _dropdownDeselectAllAllowed.ToPlusMinus(), value.ToPlusMinus());
            OnPropertyChanged();
        }
    }

    public ReadOnlyCollection<string> DropDownItems {
        get => new(_dropDownItems);
        set {
            if (IsDisposed) { return; }
            if (!_dropDownItems.IsDifferentTo(value)) { return; }

            _ = Table?.ChangeData(TableDataType.DropDownItems, this, _dropDownItems.JoinWithCr(), value.JoinWithCr());
            OnPropertyChanged();
        }
    }

    public bool EditableWithDropdown {
        get => _editableWithDropdown;
        set {
            if (IsDisposed) { return; }
            if (_editableWithDropdown == value) { return; }

            _ = Table?.ChangeData(TableDataType.EditableWithDropdown, this, _editableWithDropdown.ToPlusMinus(), value.ToPlusMinus());
            OnPropertyChanged();
        }
    }

    public bool EditableWithTextInput {
        get => _editableWithTextInput;
        set {
            if (IsDisposed) { return; }
            if (_editableWithTextInput == value) { return; }

            _ = Table?.ChangeData(TableDataType.EditableWithTextInput, this, _editableWithTextInput.ToPlusMinus(), value.ToPlusMinus());
            OnPropertyChanged();
        }
    }

    public bool EditAllowedDespiteLock {
        get => _editAllowedDespiteLock;
        set {
            if (IsDisposed) { return; }
            if (_editAllowedDespiteLock == value) { return; }

            _ = Table?.ChangeData(TableDataType.EditAllowedDespiteLock, this, _editAllowedDespiteLock.ToPlusMinus(), value.ToPlusMinus());
            OnPropertyChanged();
        }
    }

    public Type? Editor { get; set; }

    public FilterOptions FilterOptions {
        get => _filterOptions;
        set {
            if (IsDisposed) { return; }
            if (_filterOptions == value) { return; }

            _ = Table?.ChangeData(TableDataType.FilterOptions, this, ((int)_filterOptions).ToString(), ((int)value).ToString());
            OnPropertyChanged();
        }
    }

    public int FixedColumnWidth {
        get => _fixedColumnWidth;
        set {
            if (IsDisposed) { return; }
            if (_fixedColumnWidth == value) { return; }
            _ = Table?.ChangeData(TableDataType.FixedColumnWidth, this, _fixedColumnWidth.ToString(), value.ToString());
            OnPropertyChanged();
        }
    }

    public Color ForeColor {
        get => _foreColor;
        set {
            if (IsDisposed) { return; }
            if (_foreColor.ToArgb() == value.ToArgb()) { return; }

            _ = Table?.ChangeData(TableDataType.ForeColor, this, _foreColor.ToArgb().ToString(), value.ToArgb().ToString());
            OnPropertyChanged();
        }
    }

    public bool IgnoreAtRowFilter {
        get => !IsAutofilterPossible() || _ignoreAtRowFilter;
        set {
            if (IsDisposed) { return; }
            if (_ignoreAtRowFilter == value) { return; }

            _ = Table?.ChangeData(TableDataType.IgnoreAtRowFilter, this, _ignoreAtRowFilter.ToPlusMinus(), value.ToPlusMinus());
            OnPropertyChanged();
        }
    }

    public bool IsDisposed { get; private set; }

    public bool IsFirst {
        get => _isFirst;
        set {
            if (IsDisposed) { return; }

            if (_isFirst == value) { return; }

            _ = Table?.ChangeData(TableDataType.IsFirst, this, _isFirst.ToPlusMinus(), value.ToPlusMinus());
            OnPropertyChanged();
        }
    }

    public bool IsKeyColumn {
        get => _isKeyColumn;
        set {
            if (IsDisposed) { return; }

            if (_isKeyColumn == value) { return; }

            _ = Table?.ChangeData(TableDataType.IsKeyColumn, this, _isKeyColumn.ToPlusMinus(), value.ToPlusMinus());
            OnPropertyChanged();
        }
    }

    public string KeyName {
        get => _keyName.ToUpperInvariant();
        set {
            if (IsDisposed) { return; }
            value = value.ToUpperInvariant();
            if (value == _keyName.ToUpperInvariant()) { return; }

            if (!ColumNameAllowed(value)) {
                Develop.DebugPrint(ErrorType.Warning, "Spaltenname nicht erlaubt: " + _keyName);
                return;
            }

            if (Table?.Column[value] != null) {
                Develop.DebugPrint(ErrorType.Warning, "Name existiert bereits!");
                return;
            }

            //if (!IsValidColumnName(value)) {
            //    Develop.DebugPrint(ErrorType.Warning, "Spaltenname nicht erlaubt!");
            //    return;
            //}

            _ = Table?.ChangeData(TableDataType.ColumnName, this, _keyName, value);
            OnPropertyChanged();
            CheckIfIAmAKeyColumn();
        }
    }

    public ColumnLineStyle LineStyleLeft {
        get => _lineStyleLeft;
        set {
            if (IsDisposed) { return; }
            if (_lineStyleLeft == value) { return; }

            _ = Table?.ChangeData(TableDataType.LineStyleLeft, this, ((int)_lineStyleLeft).ToString(), ((int)value).ToString());
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

            _ = Table?.ChangeData(TableDataType.LineStyleRight, this, ((int)_lineStyleRight).ToString(), ((int)value).ToString());
            OnPropertyChanged();
        }
    }

    /// <summary>
    /// Die Quell-Spalte (aus der verlinkten Tabelle) ist immer
    /// </summary>
    public List<string> LinkedCellFilter {
        get => _linkedCellFilter;
        set {
            if (IsDisposed || Table is not { IsDisposed: false } db) { return; }

            value = value.SortedDistinctList();

            if (!_linkedCellFilter.IsDifferentTo(value)) { return; }

            _ = db.ChangeData(TableDataType.LinkedCellFilter, this, _linkedCellFilter.JoinWithCr(), value.JoinWithCr());
            OnPropertyChanged();

            foreach (var thisColumn in db.Column) {
                thisColumn.CheckIfIAmAKeyColumn();
            }
        }
    }

    public Table? LinkedTable {
        get {
            if (IsDisposed || Table is not { IsDisposed: false }) { return null; }
            if (string.IsNullOrEmpty(_linkedTableTableName)) { return null; }

            lock (_linkedTableLock) {
                if (_linkedTable is { IsDisposed: false }) {
                    return _linkedTable;
                }
            }

            GetLinkedTable(); // Außerhalb des Locks um Deadlock zu vermeiden

            lock (_linkedTableLock) {
                return _linkedTable; // Final read mit Lock
            }
        }
    }

    public string LinkedTableTableName {
        get => _linkedTableTableName;
        set {
            if (IsDisposed) { return; }
            if (_linkedTableTableName == value) { return; }

            _ = Table?.ChangeData(TableDataType.LinkedTableTableName, this, _linkedTableTableName, value);
            Invalidate_LinkedTable();
            OnPropertyChanged();
        }
    }

    public int MaxCellLenght {
        get => _maxCellLenght;
        set {
            if (IsDisposed) { return; }
            if (_maxCellLenght == value) { return; }
            _ = Table?.ChangeData(TableDataType.MaxCellLenght, this, _maxCellLenght.ToString(), value.ToString());
            OnPropertyChanged();
        }
    }

    public int MaxTextLenght {
        get => _maxTextLenght;
        set {
            if (IsDisposed) { return; }
            if (_maxTextLenght == value) { return; }
            _ = Table?.ChangeData(TableDataType.MaxTextLenght, this, _maxTextLenght.ToString(), value.ToString());
            OnPropertyChanged();
        }
    }

    public bool MultiLine {
        get => _multiLine;
        set {
            if (IsDisposed) { return; }
            if (!MultilinePossible()) { value = false; }

            if (_multiLine == value) { return; }

            _ = Table?.ChangeData(TableDataType.MultiLine, this, _multiLine.ToPlusMinus(), value.ToPlusMinus());
            Invalidate_ColumAndContent();
            OnPropertyChanged();
        }
    }

    public ReadOnlyCollection<string> PermissionGroupsChangeCell {
        get => new(_permissionGroupsChangeCell);
        set {
            if (IsDisposed) { return; }
            if (!_permissionGroupsChangeCell.IsDifferentTo(value)) { return; }

            _ = Table?.ChangeData(TableDataType.PermissionGroupsChangeCell, this, _permissionGroupsChangeCell.JoinWithCr(), value.JoinWithCr());
            OnPropertyChanged();
        }
    }

    public string RegexCheck {
        get => _regexCheck;
        set {
            if (IsDisposed) { return; }
            if (_regexCheck == value) { return; }

            _ = Table?.ChangeData(TableDataType.RegexCheck, this, _regexCheck, value);
            OnPropertyChanged();
        }
    }

    public bool Relationship_to_First {
        get => _relationship_to_First;
        set {
            if (IsDisposed) { return; }

            if (_relationship_to_First == value) { return; }

            _ = Table?.ChangeData(TableDataType.Relationship_to_First, this, _relationship_to_First.ToPlusMinus(), value.ToPlusMinus());
            OnPropertyChanged();
        }
    }

    public RelationType RelationType {
        get => _relationType;
        set {
            if (IsDisposed) { return; }
            if (_relationType == value) { return; }

            _ = Table?.ChangeData(TableDataType.RelationType, this, ((int)_value_for_Chunk).ToString(), ((int)value).ToString());
            Invalidate_ColumAndContent();

            OnPropertyChanged();
        }
    }

    public string RendererSettings {
        get => _rendererSettings;
        set {
            if (IsDisposed) { return; }
            if (_rendererSettings == value) { return; }

            _ = Table?.ChangeData(TableDataType.RendererSettings, this, _rendererSettings, value);
            OnPropertyChanged();
        }
    }

    public int RoundAfterEdit {
        get => _roundAfterEdit;
        set {
            if (IsDisposed) { return; }
            if (_roundAfterEdit == value) { return; }

            _ = Table?.ChangeData(TableDataType.RoundAfterEdit, this, _roundAfterEdit.ToString(), value.ToString());
            OnPropertyChanged();
        }
    }

    public bool SaveContent {
        get => _saveContent;
        set {
            if (IsDisposed) { return; }
            if (_saveContent == value) { return; }
            _ = Table?.ChangeData(TableDataType.SaveContent, this, _saveContent.ToPlusMinus(), value.ToPlusMinus());
            OnPropertyChanged();
        }
    }

    public ScriptType ScriptType {
        get => _scriptType;
        set {
            if (IsDisposed) { return; }
            if (_scriptType == value) { return; }

            _ = Table?.ChangeData(TableDataType.ScriptType, this, ((int)_scriptType).ToString(), ((int)value).ToString());
            OnPropertyChanged();
        }
    }

    public bool ShowValuesOfOtherCellsInDropdown {
        get => _showValuesOfOtherCellsInDropdown;
        set {
            if (IsDisposed) { return; }
            if (_showValuesOfOtherCellsInDropdown == value) { return; }

            _ = Table?.ChangeData(TableDataType.ShowValuesOfOtherCellsInDropdown, this, _showValuesOfOtherCellsInDropdown.ToPlusMinus(), value.ToPlusMinus());
            OnPropertyChanged();
        }
    }

    public SortierTyp SortType {
        get => _sortType;
        set {
            if (IsDisposed) { return; }
            if (_sortType == value) { return; }

            _ = Table?.ChangeData(TableDataType.SortType, this, ((int)_sortType).ToString(), ((int)value).ToString());
            OnPropertyChanged();
        }
    }

    public bool SpellCheckingEnabled {
        get => _spellCheckingEnabled;
        set {
            if (IsDisposed) { return; }
            if (_spellCheckingEnabled == value) { return; }

            _ = Table?.ChangeData(TableDataType.SpellCheckingEnabled, this, _spellCheckingEnabled.ToPlusMinus(), value.ToPlusMinus());
            OnPropertyChanged();
        }
    }

    public Table? Table {
        get => _table;
        private set {
            if (IsDisposed || (value?.IsDisposed ?? true)) { value = null; }
            if (value == _table) { return; }

            if (_table != null) {
                _table.DisposingEvent -= _table_Disposing;
            }
            _table = value;

            if (_table != null) {
                _table.DisposingEvent += _table_Disposing;
            }
        }
    }

    public bool TextFormatingAllowed {
        get => _textFormatingAllowed;
        set {
            if (IsDisposed) { return; }
            if (_textFormatingAllowed == value) { return; }

            _ = Table?.ChangeData(TableDataType.TextFormatingAllowed, this, _textFormatingAllowed.ToPlusMinus(), value.ToPlusMinus());
            OnPropertyChanged();
        }
    }

    public ChunkType Value_for_Chunk {
        get => _value_for_Chunk;
        set {
            if (IsDisposed) { return; }

            if (Table is not TableChunk) { value = ChunkType.None; }

            if (_value_for_Chunk == value) { return; }

            var oldd = _value_for_Chunk;

            _ = Table?.ChangeData(TableDataType.Value_for_Chunk, this, ((int)_value_for_Chunk).ToString(), ((int)value).ToString());
            Invalidate_ColumAndContent();

            if (oldd != _value_for_Chunk) {
                Table?.ReorganizeChunks();
            }

            OnPropertyChanged();
        }
    }

    #endregion

    #region Methods

    public static bool IsValidColumnName(string name) {
        if (string.IsNullOrWhiteSpace(name)) { return false; }

        if (!name.ContainsOnlyChars(AllowedCharsVariableName)) { return false; }

        if (!Char_AZ.Contains(name.Substring(0, 1).ToUpperInvariant())) { return false; }
        if (name.Length > 128) { return false; }

        // Illegale Namen definieren (nur Oracle + SQL Server relevante Wörter)
        string[] illegalNames = {
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
    };

        // Prüfen ob Name in der Liste der illegalen Namen steht
        return !illegalNames.Contains(name, StringComparer.OrdinalIgnoreCase);
    }

    public static EditTypeTable UserEditDialogTypeInTable(ColumnItem? column, bool preverDropDown) => column is not { IsDisposed: false }
            ? EditTypeTable.None
            : UserEditDialogTypeInTable(column, preverDropDown && column.EditableWithDropdown, column.EditableWithTextInput);

    public static EditTypeTable UserEditDialogTypeInTable(ColumnItem column, bool doDropDown, bool keybordInputAllowed) {
        if (!doDropDown && !keybordInputAllowed) { return EditTypeTable.None; }

        if (column.RelationType == RelationType.DropDownValues) { return EditTypeTable.Dropdown_Single; }

        if (column.TextboxEditPossible()) {
            if (!doDropDown) {
                return EditTypeTable.Textfeld;
            }

            if (column.MultiLine && column.EditableWithDropdown) {
                return EditTypeTable.Dropdown_Single;
            }

            if (keybordInputAllowed) {
                return EditTypeTable.Textfeld_mit_Auswahlknopf;
            }

            return EditTypeTable.Dropdown_Single;
        }

        return EditTypeTable.None;
    }

    public void AddSystemInfo(string type, string user) {
        var t = ColumnSystemInfo.SplitAndCutByCrToList();
        t.Add(type + ": " + user);

        //t.TagSet(type, user);
        ColumnSystemInfo = t.SortedDistinctList().JoinWithCr();
    }

    public void AddSystemInfo(string type, Table sourcetable, string user) => AddSystemInfo(type, sourcetable.Caption + " -> " + user);

    public string AutoCorrect(string value, bool exitifLinkedFormat) {
        if (IsDisposed || Table is not { IsDisposed: false } db) { return value; }

        if (IsSystemColumn() && this != db.Column.SysChapter) { return value; }
        //if (Function == ColumnFunction.Virtelle_Spalte) { return value; }

        if (exitifLinkedFormat && _relationType != RelationType.None) { return value; }

        if (_afterEditDoUCase) { value = value.ToUpperInvariant(); }

        if (!string.IsNullOrEmpty(_autoRemove)) { value = value.RemoveChars(_autoRemove); }

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
            value = l.JoinWithCr();
        }

        if (_afterEditAutoCorrect) { value = KleineFehlerCorrect(value); }

        if (_roundAfterEdit > -1 && DoubleTryParse(value, out var erg)) {
            erg = Math.Round(erg, _roundAfterEdit, MidpointRounding.AwayFromZero);
            value = erg.ToStringFloat();
        }

        if (_afterEditQuickSortRemoveDouble) {
            var l = new List<string>(value.SplitAndCutByCr()).SortedDistinctList();
            value = l.JoinWithCr();
        }

        return value.CutToUtf8Length(_maxCellLenght);
    }

    public bool AutoFilterSymbolPossible() => FilterOptions.HasFlag(FilterOptions.Enabled) && IsAutofilterPossible();

    public int CalculatePreveredMaxCellLenght(double prozentZuschlag) {
        if (IsDisposed || Table is not { IsDisposed: false }) { return 0; }

        //if (Format == DataFormat.Verknüpfung_zu_anderer_Tabellex) { return 35; }
        //if (Format == DataFormat.Werte_aus_anderer_Tabelle_als_DropDownItemsx) { return 15; }
        var m = 0;

        foreach (var thisRow in Table.Row) {
            var t = thisRow.CellGetString(this);
            m = Math.Max(m, t.StringtoUtf8().Length);
        }

        if (m <= 0) { return 8; }
        if (m == 1) { return 1; }
        var erg = Math.Max((int)(m * prozentZuschlag) + 1, _maxTextLenght);
        return Math.Min(erg, 3999);
    }

    public int CalculatePreveredMaxTextLenght(double prozentZuschlag) {
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

    public bool CanBeCheckedByRules() {
        return true;
    }

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
                Develop.DebugPrint(ErrorType.Warning, "Nummer " + number + " nicht erlaubt.");
                return string.Empty;
        }
    }

    public void CloneFrom(ColumnItem source, bool nameAndKeyToo) {
        if (Table is not { IsDisposed: false } db) { return; }
        if (!string.IsNullOrEmpty(db.CanWriteMainFile())) { return; }

        if (source.Table != null) { source.Repair(); }

        if (nameAndKeyToo) { KeyName = source.KeyName; }

        Caption = source.Caption;
        IsKeyColumn = source.IsKeyColumn;
        RelationType = source.RelationType;
        Relationship_to_First = source.Relationship_to_First;
        Value_for_Chunk = source.Value_for_Chunk;
        IsFirst = source.IsFirst;
        CaptionBitmapCode = source.CaptionBitmapCode;
        LineStyleLeft = source.LineStyleLeft;
        LineStyleRight = source.LineStyleRight;
        MultiLine = source.MultiLine;
        ColumnQuickInfo = source.ColumnQuickInfo;
        ForeColor = source.ForeColor;
        BackColor = source.BackColor;
        EditAllowedDespiteLock = source.EditAllowedDespiteLock;
        PermissionGroupsChangeCell = source.PermissionGroupsChangeCell;
        ColumnTags = source.ColumnTags;
        AdminInfo = source.AdminInfo;
        DefaultRenderer = source.DefaultRenderer;
        RendererSettings = source.RendererSettings;
        FilterOptions = source.FilterOptions;
        IgnoreAtRowFilter = source.IgnoreAtRowFilter;
        EditableWithDropdown = source.EditableWithDropdown;
        DropdownDeselectAllAllowed = source.DropdownDeselectAllAllowed;
        EditableWithTextInput = source.EditableWithTextInput;
        SpellCheckingEnabled = source.SpellCheckingEnabled;
        ShowValuesOfOtherCellsInDropdown = source.ShowValuesOfOtherCellsInDropdown;
        AfterEditQuickSortRemoveDouble = source.AfterEditQuickSortRemoveDouble;
        RoundAfterEdit = source.RoundAfterEdit;
        MaxCellLenght = source.MaxCellLenght;
        FixedColumnWidth = source.FixedColumnWidth;
        AfterEditDoUCase = source.AfterEditDoUCase;
        AfterEditAutoCorrect = source.AfterEditAutoCorrect;
        AutoRemove = source.AutoRemove;
        AutoFilterJoker = source.AutoFilterJoker;
        ColumnNameOfLinkedTable = source.ColumnNameOfLinkedTable;
        Align = source.Align;
        SortType = source.SortType;
        DropDownItems = source.DropDownItems;
        LinkedCellFilter = source.LinkedCellFilter;
        AfterEditAutoReplace = source.AfterEditAutoReplace;
        this.GetStyleFrom(source); // regex, Allowed Chars, etc.
        ScriptType = source.ScriptType;
        SaveContent = source.SaveContent;
        CaptionGroup1 = source.CaptionGroup1;
        CaptionGroup2 = source.CaptionGroup2;
        CaptionGroup3 = source.CaptionGroup3;
        LinkedTableTableName = source.LinkedTableTableName;
    }

    public bool ColumNameAllowed(string nameToTest) {
        if (!IsValidColumnName(nameToTest)) { return false; }

        if (nameToTest.Equals(_keyName, StringComparison.OrdinalIgnoreCase)) { return true; }
        if (Table?.Column[nameToTest] != null) { return false; }

        return true;
    }

    public List<string> Contents() => Contents(Table?.Row.ToList());

    public List<string> Contents(FilterCollection fc, List<RowItem>? pinned) {
        if (IsDisposed || Table is not { IsDisposed: false }) { return []; }
        var r = fc.Rows;

        var r2 = new List<RowItem>();
        r2.AddRange(r);

        if (pinned != null) { _ = r2.AddIfNotExists(pinned); }

        return Contents(r2);
    }

    public List<string> Contents(IEnumerable<RowItem>? rows) {
        if (rows == null || !rows.Any()) { return []; }

        var list = new List<string>();
        foreach (var thisRowItem in rows) {
            if (thisRowItem != null) {
                if (!_saveContent) { _ = thisRowItem.CheckRow(); }

                if (_multiLine) {
                    list.AddRange(thisRowItem.CellGetList(this));
                } else {
                    if (thisRowItem.CellGetString(this).Length > 0) {
                        list.Add(thisRowItem.CellGetString(this));
                    }
                }
            }
        }

        return list.SortedDistinctList();
    }

    public bool CopyAble() {
        if (_relationType == RelationType.DropDownValues) { return false; }
        return true;
    }

    public void Dispose() {
        // Ändern Sie diesen Code nicht. Fügen Sie Bereinigungscode in der Methode "Dispose(bool disposing)" ein.
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    public bool DropdownItemsAllowed() {
        return true;
    }

    public bool DropdownItemsOfOtherCellsAllowed() {
        if (_relationType == RelationType.DropDownValues) { return false; }
        return true;
    }

    public bool DropdownUnselectAllAllowed() {
        if (_value_for_Chunk != ChunkType.None) { return false; }

        return true;
    }

    public string ErrorReason() {
        if (IsDisposed || Table is not { IsDisposed: false } db) { return "Tabelle verworfen"; }
        if (string.IsNullOrEmpty(_keyName)) { return "Der Spaltenname ist nicht definiert."; }

        if (!IsValidColumnName(_keyName)) { return "Der Spaltenname ist ungültig."; }

        if (_maxCellLenght < _maxTextLenght) { return "Zellengröße zu klein!"; }
        if (_maxCellLenght < 1) { return "Zellengröße zu klein!"; }
        if (_maxCellLenght > 4000) { return "Zellengröße zu groß!"; }
        if (_maxTextLenght > 4000) { return "Maximallänge zu groß!"; }

        if (Table.Column.Any(thisColumn => thisColumn != this && thisColumn != null && string.Equals(_keyName, thisColumn._keyName, StringComparison.OrdinalIgnoreCase))) {
            return "Spalten-Name bereits vorhanden.";
        }

        if (string.IsNullOrEmpty(_caption)) { return "Spalten Beschriftung fehlt."; }

        if (_relationType != RelationType.None) {
            if (LinkedTable is not { IsDisposed: false } db2) { return "Verknüpfte Tabelle fehlt oder existiert nicht."; }
            if (db == db2) { return "Zirkelbezug mit verknüpfter Tabelle."; }
            var c = db2.Column[_columnNameOfLinkedTable];
            if (c == null) { return "Die verknüpfte Schlüsselspalte existiert nicht."; }
            if (_linkedCellFilter.Count == 0) {
                if (_relationType != RelationType.DropDownValues) {
                    return "Keine Filter für verknüpfte Tabelle definiert.";
                }
            }
        } else {
            if (!string.IsNullOrEmpty(_columnNameOfLinkedTable)) { return "Nur verlinkte Zellen können Daten über verlinkte Zellen enthalten."; }
        }

        if (!IsAutofilterPossible()) {
            if (_filterOptions != FilterOptions.None) { return "Bei diesem Format keine Filterung erlaubt."; }
            if (!_ignoreAtRowFilter) { return "Dieses Format muss bei Zeilenfiltern ignoriert werden."; }
        }
        if (_filterOptions != FilterOptions.None && !_filterOptions.HasFlag(FilterOptions.Enabled)) { return "Filter Kombination nicht möglich."; }
        if (_filterOptions != FilterOptions.Enabled_OnlyAndAllowed && _filterOptions.HasFlag(FilterOptions.OnlyAndAllowed)) { return "Filter Kombination nicht möglich."; }
        if (_filterOptions != FilterOptions.Enabled_OnlyOrAllowed && _filterOptions.HasFlag(FilterOptions.OnlyOrAllowed)) { return "Filter Kombination nicht möglich."; }
        if (_filterOptions.HasFlag(FilterOptions.OnlyAndAllowed) || _filterOptions.HasFlag(FilterOptions.OnlyOrAllowed)) {
            if (!_multiLine) {
                return "Dieser Filter kann nur bei mehrzeiligen Spalten benutzt werden.";
            }
        }

        if (!_saveContent) {
            if (_fixedColumnWidth < 16) { return "Bei Spalten ohne Inhaltsspeicherung muss eine feste Spaltenbreite angegeben werden."; }
            if (_scriptType is not ScriptType.Bool and not ScriptType.String and not ScriptType.Numeral and not ScriptType.List) {
                return "Spalten ohne Inhaltsspeicherung müssen im Skript gesetzt werden und deswegen vorhanden sein.";
            }
            if (!_ignoreAtRowFilter) { return "Spalten ohne Inhaltsspeicherung müssen bei Zeilenfiltern ignoriert werden."; }

            if (_isKeyColumn) {
                return "Bei Schlüsselspalten muss der Inhalt gespeichert werden.";
            }
            if (_isFirst) {
                return "Bei der ersten Spalte muss der Inhalt gespeichert werden.";
            }

            if (_value_for_Chunk != ChunkType.None) {
                return "Chunk Spalten der Inhalt gespeichert werden.";
            }

            if (_relationType != RelationType.None) {
                return "Bei Spalten mit Verknüpfung zu anderen Tabellen der Inhalt gespeichert werden.";
            }

            if (_relationship_to_First) {
                return "Bei Spalten mit Beziehungen Inhalt gespeichert werden.";
            }
        }

        if (_isFirst) {
            //if (_scriptType is not ScriptType.String_Readonly and not ScriptType.Bool_Readonly and not ScriptType.Nicht_vorhanden and not ScriptType.List_Readonly) {
            //    return "Diese Spalte darf im Skript nur als ReadOnly vorhanden sein.";
            //}

            if (_relationship_to_First || _relationType != RelationType.None) {
                return "Beziehungen zu anderen Zeilen und Erstpalte nicht kombinierbar.";
            }
        }

        if (_isKeyColumn) {
            if (_relationship_to_First || (_relationType != RelationType.None && _relationType != RelationType.DropDownValues)) {
                return "Beziehungen zu anderen Zeilen und Schlüsselspalte nicht kombinierbar.";
            }

            if (_scriptType is not ScriptType.String_Readonly and not ScriptType.Bool_Readonly and not ScriptType.List_Readonly and not ScriptType.Numeral_Readonly) {
                return "Schlüsselspalten müssen im Script als Readonly vorhanden sein.";
            }
        }

        if (_value_for_Chunk != ChunkType.None) {
            if (Table is not TableChunk) {
                return "Chunk-Spalten nur in Tabellen des Typs CBCB erlaubt.";
            }

            if (_scriptType is not ScriptType.String_Readonly and not ScriptType.Bool_Readonly and not ScriptType.Nicht_vorhanden and not ScriptType.List_Readonly and not ScriptType.Numeral_Readonly) {
                return "Diese Spalte darf im Skript nur als ReadOnly vorhanden sein.";
            }
            if (!string.IsNullOrEmpty(_autoFilterJoker)) { return "Der Autofilter-Joker darf bei dieser Spalte nicht gesetzt sein."; }
            if (_filterOptions.HasFlag(FilterOptions.ExtendedFilterEnabled)) { return "Erweiterte Filter sind bei dieser Spalte nicht erlaubt."; }
            if (!_filterOptions.HasFlag(FilterOptions.TextFilterEnabled)) { return "Texteingabe-Filter sind bei dieser Spalte nötig."; }
            if (!_ignoreAtRowFilter) { return "Diese Spalte muss bei Zeilenfiltern ignoriert werden."; }

            if (!_filterOptions.HasFlag(FilterOptions.Enabled)) { return "Auto-Filter müssen bei dieser Spalte erlaubt sein."; }

            if (_relationship_to_First || _relationType != RelationType.None) {
                return "Beziehungen zu anderen Zeilen und Chunk-Wert nicht kombinierbar.";
            }
        }

        if (_relationship_to_First) {
            if (!_multiLine) { return "Bei dieser Funktion muss mehrzeilig ausgewählt werden."; }
            //if (_keyColumnKey > -1) { return "Diese Format darf keine Verknüpfung zu einer Schlüsselspalte haben."; }
            if (db.Column.First == this) { return "Diese Funktion ist bei der ersten Spalte nicht erlaubt."; }
            //if (!string.IsNullOrEmpty(_cellInitValue)) { return "Diese Format kann keinen Initial-Text haben."; }
            //if (!string.IsNullOrEmpty(_vorschlagsColumn)) { return "Diese Format kann keine Vorschlags-Spalte haben."; }
        }

        if (_relationType == RelationType.CellValues) {
            if (_scriptType is not ScriptType.Nicht_vorhanden) {
                return "Spalten mit Verlinkungen zu anderen Tabellen können im Skript nicht verwendet werden. ImportLinked im Skript benutzen und den Skript-Type auf nicht vorhanden setzen.";
            }
        }

        if (_multiLine) {
            if (!MultilinePossible()) { return "Format unterstützt keine mehrzeiligen Texte."; }
            if (_roundAfterEdit != -1) { return "Runden nur bei einzeiligen Texten möglich"; }
        } else {
            if (_afterEditQuickSortRemoveDouble) { return "Sortierung kann nur bei mehrzeiligen Feldern erfolgen."; }
        }

        if (_spellCheckingEnabled && !SpellCheckingPossible()) { return "Rechtschreibprüfung bei diesem Format nicht möglich."; }
        if (_editAllowedDespiteLock && !_editableWithTextInput && !_editableWithDropdown) { return "Wenn die Zeilensperre ignoriert werden soll, muss eine Bearbeitungsmethode definiert sein."; }
        var tmpEditDialog = UserEditDialogTypeInTable(this, false, true);
        if (_editableWithTextInput) {
            if (tmpEditDialog == EditTypeTable.Dropdown_Single) { return "Format unterstützt nur Dropdown-Menü."; }
            if (tmpEditDialog == EditTypeTable.None) { return "Format unterstützt keine Standard-Bearbeitung."; }
        }

        if (_editableWithDropdown) {
            //if (_SpellCheckingEnabled) { return "Entweder Dropdownmenü oder Rechtschreibprüfung."; }
            if (tmpEditDialog == EditTypeTable.None) { return "Format unterstützt keine Auswahlmenü-Bearbeitung."; }
        }
        if (!_editableWithDropdown && !_editableWithTextInput) {
            if (_permissionGroupsChangeCell.Count > 0) { return "Bearbeitungsberechtigungen entfernen, wenn keine Bearbeitung erlaubt ist."; }
        }

        foreach (var thisS in _permissionGroupsChangeCell) {
            if (thisS.Contains("|")) { return "Unerlaubtes Zeichen bei den Gruppen, die eine Zelle bearbeiten dürfen."; }
            if (string.Equals(thisS, Administrator, StringComparison.OrdinalIgnoreCase)) { return "'#Administrator' bei den Bearbeitern entfernen."; }
        }
        if (_editableWithDropdown || tmpEditDialog == EditTypeTable.Dropdown_Single) {
            if (_relationType != RelationType.DropDownValues) {
                if (!_showValuesOfOtherCellsInDropdown && _dropDownItems.Count == 0) { return "Keine Dropdown-Items vorhanden bzw. Alles hinzufügen nicht angewählt."; }
            }
        } else {
            if (_showValuesOfOtherCellsInDropdown) { return "Dropdownmenu nicht ausgewählt, 'alles hinzufügen' prüfen."; }
            if (_dropdownDeselectAllAllowed) { return "Dropdownmenu nicht ausgewählt, 'alles abwählen' prüfen."; }
            if (_dropDownItems.Count > 0) { return "Dropdownmenu nicht ausgewählt, Dropdown-Items vorhanden."; }
        }
        if (_showValuesOfOtherCellsInDropdown && !DropdownItemsOfOtherCellsAllowed()) { return "'Dropdownmenu alles hinzufügen' bei diesem Format nicht erlaubt."; }
        if (_dropdownDeselectAllAllowed && !DropdownUnselectAllAllowed()) { return "'Dropdownmenu alles abwählen' bei diesem Format nicht erlaubt."; }
        if (_dropDownItems.Count > 0 && !DropdownItemsAllowed()) { return "Manuelle 'Dropdow-Items' bei diesem Format nicht erlaubt."; }

        if (_roundAfterEdit > 5) { return "Beim Runden maximal 5 Nachkommastellen möglich"; }
        if (_filterOptions == FilterOptions.None) {
            if (!string.IsNullOrEmpty(_autoFilterJoker)) { return "Wenn kein Autofilter erlaubt ist, immer anzuzeigende Werte entfernen"; }
        }

        if (_relationType == RelationType.DropDownValues ||
            _scriptType == ScriptType.Row) {
            if (_roundAfterEdit != -1 || _afterEditAutoReplace.Count > 0 || _afterEditAutoCorrect || _afterEditDoUCase || _afterEditQuickSortRemoveDouble || !string.IsNullOrEmpty(_allowedChars)) {
                return "Dieses Format unterstützt keine automatischen Bearbeitungen wie Runden, Ersetzungen, Fehlerbereinigung, immer Großbuchstaben, Erlaubte Zeichen oder Sortierung.";
            }
        }

        return string.Empty;
    }

    public List<string> GetUcaseNamesSortedByLenght() {
        if (IsDisposed || Table is not { IsDisposed: false } db) { return []; }

        if (UcaseNamesSortedByLenght != null) { return UcaseNamesSortedByLenght; }
        var tmp = Contents(db.Row.ToList());
        tmp.Sort((s1, s2) => s2.Length.CompareTo(s1.Length));
        UcaseNamesSortedByLenght = tmp;
        return UcaseNamesSortedByLenght;
    }

    public void Invalidate_ColumAndContent() {
        if (IsDisposed || Table is not { IsDisposed: false }) { return; }
        Invalidate_LinkedTable();
        Table.OnViewChanged();
    }

    public bool IsAutofilterPossible() {
        return true;
    }

    public string IsNowEditable() {
        if (Table is not { IsDisposed: false } db) { return "Tabelle verworfen"; }
        return db.GrantWriteAccess(TableDataType.ColumnName, TableChunk.Chunk_Master);
    }

    //}
    ///// <summary>
    ///// Füllt die Ersetzungen mittels eines übergebenen Enums aus.
    ///// </summary>
    ///// <param name="t">Beispiel: GetType(enDesign)</param>
    ///// <param name="ZumDropdownHinzuAb">Erster Wert der Enumeration, der Hinzugefügt werden soll. Inklusive deses Wertes</param>
    ///// <param name="ZumDropdownHinzuBis">Letzter Wert der Enumeration, der nicht mehr hinzugefügt wird, also exklusives diese Wertes</param>
    //public void GetValuesFromEnum(Type t, int ZumDropdownHinzuAb, int ZumDropdownHinzuBis) {
    //    List<string> NewReplacer = new();
    //    List<string> NewAuswahl = new();
    //    var items = Enum.GetValues(t);
    //    foreach (var thisItem in items) {
    //        var te = Enum.GetName(t, thisItem);
    //        var th = (int)thisItem;
    //        if (!string.IsNullOrEmpty(te)) {
    //            NewReplacer.GenerateAndAdd(th + "|" + te);
    //            if (th >= ZumDropdownHinzuAb && th < ZumDropdownHinzuBis) {
    //                NewAuswahl.GenerateAndAdd(th.ToString(false));
    //            }
    //        }
    //    }
    //    NewReplacer.Reverse();
    //    if (OpticalReplace.IsDifferentTo(NewReplacer)) {
    //        OpticalReplace.Clear();
    //        OpticalReplace.AddRange(NewReplacer);
    //    }
    //    if (DropDownItems.IsDifferentTo(NewAuswahl)) {
    //        DropDownItems.Clear();
    //        DropDownItems.AddRange(NewAuswahl);
    //    }
    //}
    /// <summary>
    /// Der Invalidate, der am meisten invalidiert: Alle temporären Variablen und auch jede Zell-Größe der Spalte.
    /// </summary>

    public bool IsSystemColumn() =>
        _keyName.ToUpperInvariant() is "SYS_CORRECT" or
            "SYS_CHANGER" or
            "SYS_CREATOR" or
            "SYS_CHAPTER" or
            "SYS_DATECREATED" or
            "SYS_DATECHANGED" or
            "SYS_LOCKED" or
            "SYS_ROWSTATE" or
            "SYS_ROWCOLOR";

    public bool MultilinePossible() {
        if (_value_for_Chunk != ChunkType.None) { return false; }
        return true;
    }

    public string ReadableText() {
        if (IsDisposed || Table is not { IsDisposed: false }) { return string.Empty; }

        var ret = _caption;
        if (Table.Column.Any(thisColumnItem => thisColumnItem != null && thisColumnItem != this && string.Equals(thisColumnItem.Caption, _caption, StringComparison.OrdinalIgnoreCase))) {
            //var done = false;
            if (!string.IsNullOrEmpty(_captionGroup3)) {
                ret = _captionGroup3 + "/" + ret;
                //done = true;
            }
            if (!string.IsNullOrEmpty(_captionGroup2)) {
                ret = _captionGroup2 + "/" + ret;
                //done = true;
            }
            if (!string.IsNullOrEmpty(_captionGroup1)) {
                ret = _captionGroup1 + "/" + ret;
                //done = true;
            }
            //if (!done) {
            //    ret = _name; //_Caption + " (" + _Name + ")";
            //}
        }
        ret = ret.Replace("\n", "\r").Replace("\r\r", "\r");
        var i = ret.IndexOf("-\r", StringComparison.Ordinal);
        if (i > 0 && i < ret.Length - 3) {
            var tzei = ret.Substring(i + 2, 1);
            if (tzei.ToLowerInvariant() == tzei) {
                ret = ret.Substring(0, i) + ret.Substring(i + 2);
            }
        }
        return ret.Replace("\r", " ").Replace("  ", " ").TrimEnd(":");
    }

    public void Repair() {
        if (Table is not { IsDisposed: false } db) { return; }
        if (!string.IsNullOrEmpty(db.CanWriteMainFile())) { return; }

        if (IsDisposed || Table is not { IsDisposed: false }) { return; }
        if (IsDisposed) { return; }

        //if (_function.ToString() == ((int)_function).ToString()) {
        //    this.GetStyleFrom(ColumnFormatHolder.Text);
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

        //        if (!_linkedTableTableName.Contains("|") && _linkedTableTableName.IsFormat(FormatHolder.FilepathAndName)) {
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

        //            MaxTextLenght = c.MaxTextLenght;
        //            MaxCellLenght = c.MaxCellLenght;
        //        }
        //        break;

        //    case ColumnFunction.RelationText:
        //        Relationship_to_First = true;
        //        _function = ColumnFunction.Normal;
        //        break;

        //    case ColumnFunction.Split_Name:
        //        _value_for_Chunk = ChunkType.ByName;
        //        _function = ColumnFunction.Normal;
        //        _ = Table?.ChangeData(TableDataType.Value_for_Chunk, this, null, "0", ((int)_value_for_Chunk).ToString(), Generic.UserName, DateTime.UtcNow, "Neue Spaltenfunktionen", string.Empty);
        //        break;

        //    case ColumnFunction.Split_Medium:
        //        _value_for_Chunk = ChunkType.ByHash_2Chars;
        //        _function = ColumnFunction.Normal;
        //        _ = Table?.ChangeData(TableDataType.Value_for_Chunk, this, null, "0", ((int)_value_for_Chunk).ToString(), Generic.UserName, DateTime.UtcNow, "Neue Spaltenfunktionen", string.Empty);
        //        break;

        //    case ColumnFunction.Split_Large:
        //        _value_for_Chunk = ChunkType.ByHash_3Chars;
        //        _function = ColumnFunction.Normal;
        //        _ = Table?.ChangeData(TableDataType.Value_for_Chunk, this, null, "0", ((int)_value_for_Chunk).ToString(), Generic.UserName, DateTime.UtcNow, "Neue Spaltenfunktionen", string.Empty);
        //        break;

        //    case ColumnFunction.Werte_aus_anderer_Tabelle_als_DropDownItems:
        //        RelationType = RelationType.DropDownValues;
        //        _function = ColumnFunction.Normal;
        //        break;
        //}

        if (MaxCellLenght < MaxTextLenght) { MaxCellLenght = MaxTextLenght; }

        PermissionGroupsChangeCell = RepairUserGroups(PermissionGroupsChangeCell).AsReadOnly();

        ResetSystemToDefault(false);
        CheckIfIAmAKeyColumn();

        SystemInfoReset(false);
    }

    public void ResetSystemToDefault(bool setOpticalToo) {
        if (!IsSystemColumn()) { return; }
        // ACHTUNG: Die setOpticalToo Befehle OHNE _, die müssen geloggt werden.
        if (setOpticalToo) {
            LineStyleLeft = ColumnLineStyle.Dünn;
            LineStyleRight = ColumnLineStyle.Ohne;
            ForeColor = Color.FromArgb(0, 0, 0);
            //CaptionBitmapCode = null;
        }

        _saveContent = true;
        _relationType = RelationType.None;

        switch (_keyName.ToUpperInvariant()) {
            case "SYS_CREATOR":

                _isFirst = false;
                _maxTextLenght = 20;
                _maxCellLenght = 20;
                if (setOpticalToo) {
                    Caption = "Ersteller";
                    EditableWithDropdown = true;
                    ShowValuesOfOtherCellsInDropdown = true;
                    SpellCheckingEnabled = false;
                    ForeColor = Color.FromArgb(0, 0, 128);
                    BackColor = Color.FromArgb(185, 186, 255);
                }
                break;

            case "SYS_CHANGER":
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
                _maxTextLenght = 20;
                _maxCellLenght = 20;
                if (setOpticalToo) {
                    Caption = "Änderer";
                    ForeColor = Color.FromArgb(0, 128, 0);
                    BackColor = Color.FromArgb(185, 255, 185);
                }
                break;

            case "SYS_CHAPTER":

                _multiLine = true;

                if (setOpticalToo) {
                    Caption = "Kapitel";
                    ForeColor = Color.FromArgb(0, 0, 0);
                    BackColor = Color.FromArgb(255, 255, 150);
                    LineStyleLeft = ColumnLineStyle.Dick;
                }
                break;

            case "SYS_DATECREATED":
                _spellCheckingEnabled = false;
                _ignoreAtRowFilter = true;

                this.GetStyleFrom(FormatHolder.DateTime); // Ja, FormatHolder, da wird der Script-Type nicht verändert
                MaxCellLenght = MaxTextLenght;
                if (setOpticalToo) {
                    Caption = "Erstell-Datum";
                    ForeColor = Color.FromArgb(0, 0, 128);
                    BackColor = Color.FromArgb(185, 185, 255);
                    LineStyleLeft = ColumnLineStyle.Dick;
                }

                break;

            case "SYS_ROWSTATE":
                _isKeyColumn = false;
                _isFirst = false;
                _relationship_to_First = false;
                _relationType = RelationType.None;
                _value_for_Chunk = ChunkType.None;
                _spellCheckingEnabled = false;
                _ignoreAtRowFilter = true;
                this.GetStyleFrom(FormatHolder.DateTime); // Ja, FormatHolder, da wird der Script-Type nicht verändert
                MaxCellLenght = MaxTextLenght;
                if (setOpticalToo) {
                    Caption = "Zeilen-Status";
                    ForeColor = Color.FromArgb(128, 0, 0);
                    BackColor = Color.FromArgb(255, 185, 185);
                    //LineLeft = ColumnLineStyle.Dick;
                }
                //_scriptType = ScriptType.Nicht_vorhanden;  // um Script-Prüfung zu reduzieren

                break;

            case "SYS_ROWCOLOR":
                _isKeyColumn = false;
                _isFirst = false;
                _relationship_to_First = false;
                _relationType = RelationType.None;
                _value_for_Chunk = ChunkType.None;
                _spellCheckingEnabled = false;
                _ignoreAtRowFilter = true;
                this.GetStyleFrom(ColumnFormatHolder.Color); // Ja, FormatHolder, da wird der Script-Type nicht verändert
                MaxCellLenght = MaxTextLenght;
                if (setOpticalToo) {
                    Caption = "Zeilenfarbe";
                    ForeColor = Color.FromArgb(0, 0, 0);
                    BackColor = Color.FromArgb(255, 255, 255);
                    //LineLeft = ColumnLineStyle.Dick;
                    AdminInfo = "Muss Werte im Format RGB oder ARGB enthalten.\r\nBeispiel: #ff0000 oder #ff120320";
                }
                //_scriptType = ScriptType.Nicht_vorhanden;  // um Script-Prüfung zu reduzieren

                break;

            case "SYS_DATECHANGED":
                _isKeyColumn = false;
                _isFirst = false;
                _relationship_to_First = false;
                _relationType = RelationType.None;
                _value_for_Chunk = ChunkType.None;
                _spellCheckingEnabled = false;
                _ignoreAtRowFilter = true;

                this.GetStyleFrom(FormatHolder.DateTimeWithMilliSeconds); // Ja, FormatHolder, da wird der Script-Type nicht verändert
                MaxCellLenght = MaxTextLenght;
                _editableWithTextInput = false;
                _spellCheckingEnabled = false;
                _editableWithDropdown = false;
                _scriptType = ScriptType.Nicht_vorhanden; // um Script-Prüfung zu reduzieren
                _permissionGroupsChangeCell.Clear();

                if (setOpticalToo) {
                    Caption = "Änder-Datum";
                    ForeColor = Color.FromArgb(0, 128, 0);
                    BackColor = Color.FromArgb(185, 255, 185);
                    LineStyleLeft = ColumnLineStyle.Dick;
                }
                break;

            case "SYS_CORRECT":
                _caption = "Fehlerfrei";
                _isFirst = false;
                _spellCheckingEnabled = false;
                _relationship_to_First = false;
                _relationType = RelationType.None;
                _value_for_Chunk = ChunkType.None;
                //_AutoFilterErweitertErlaubt = false;
                _autoFilterJoker = string.Empty;
                //_AutofilterTextFilterErlaubt = false;
                _ignoreAtRowFilter = true;
                _filterOptions = FilterOptions.Enabled;

                if (_scriptType is not ScriptType.String_Readonly and not ScriptType.Bool_Readonly and not ScriptType.List_Readonly) {
                    _scriptType = ScriptType.Nicht_vorhanden; // Wichtig! Weil eine Routine ErrorCol !=0 den Wert setzt und evtl. eine Endlosschleife auslöst
                }

                _align = AlignmentHorizontal.Zentriert;
                _dropDownItems.Clear();
                _linkedCellFilter.Clear();
                _permissionGroupsChangeCell.Clear();
                _editableWithTextInput = false;
                _editableWithDropdown = false;
                _maxTextLenght = 1;
                _maxCellLenght = 1;
                _showValuesOfOtherCellsInDropdown = false;
                _adminInfo = "Diese Spalte kann nur über ein Skript bearbeitet<br>werden, mit dem Befehl 'SetError'";

                if (setOpticalToo) {
                    ForeColor = Color.FromArgb(128, 0, 0);
                    BackColor = Color.FromArgb(255, 185, 185);
                    LineStyleLeft = ColumnLineStyle.Dick;
                }
                break;

            case "SYS_LOCKED":
                _isFirst = false;
                _spellCheckingEnabled = false;
                _relationship_to_First = false;
                _relationType = RelationType.None;
                _value_for_Chunk = ChunkType.None;
                _filterOptions = FilterOptions.Enabled;
                _autoFilterJoker = string.Empty;
                _ignoreAtRowFilter = true;
                _align = AlignmentHorizontal.Zentriert;
                _maxTextLenght = 1;
                _maxCellLenght = 1;

                if (_editableWithTextInput || _editableWithDropdown) {
                    _columnQuickInfo = "Eine abgeschlossene Zeile kann<br>nicht mehr bearbeitet werden.";
                    _editableWithTextInput = false;
                    _editableWithDropdown = true;
                    _editAllowedDespiteLock = true;
                    _ = _dropDownItems.AddIfNotExists("+");
                    _ = _dropDownItems.AddIfNotExists("-");
                } else {
                    _dropDownItems.Clear();
                }

                if (setOpticalToo) {
                    Caption = "Abgeschlossen";
                    ForeColor = Color.FromArgb(128, 0, 0);
                    BackColor = Color.FromArgb(255, 185, 185);
                }
                break;

            //case "System: State":
            //    _name = "SYS_RowState";
            //    _caption = "veraltet und kann gelöscht werden: Zeilenstand";
            //    _identifierx = string.Empty;
            //    break;

            //case "System: ID":
            //    _name = "SYS_ID";
            //    _caption = "veraltet und kann gelöscht werden: Zeilen-ID";
            //    _identifierx = string.Empty;
            //    break;

            //case "System: Last Used Layout":
            //    _name = "SYS_Layout";
            //    _caption = "veraltet und kann gelöscht werden:  Letztes Layout";
            //    _identifierx = string.Empty;
            //    break;

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

    public void Statistik(List<RowItem> rows, bool ignoreMultiLine) {
        if (IsDisposed || Table is not { IsDisposed: false }) { return; }

        if (rows.Count < 1) { return; }

        var d = new Dictionary<string, int>();

        var values = new List<string>();

        foreach (var thisRow in rows) {
            if (ignoreMultiLine) {
                var keyValue = thisRow.CellGetString(this);
                if (string.IsNullOrEmpty(keyValue)) { values.Add("[empty]"); } else {
                    values.Add(keyValue.Replace("\r", ";"));
                }
            } else {
                var keyValue = thisRow.CellGetList(this);
                if (keyValue.Count == 0) { values.Add("[empty]"); } else {
                    values.AddRange(keyValue);
                }
            }
        }

        foreach (var keyValue in values) {
            var count = 0;
            if (d.ContainsKey(keyValue)) {
                _ = d.TryGetValue(keyValue, out count);
                _ = d.Remove(keyValue);
            }
            count++;
            d.Add(keyValue, count);
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

            _ = d.Remove(keyValue);
            l.Add(maxCount + " - " + keyValue);
        } while (d.Count > 0);

        _ = l.WriteAllText(TempFile(string.Empty, string.Empty, "txt"), Encoding.UTF8, true);
    }

    public double? Summe(FilterCollection fc) {
        if (IsDisposed || Table is not { IsDisposed: false } db) { return null; }

        double summ = 0;
        foreach (var thisrow in db.Row) {
            if (thisrow != null && thisrow.MatchesTo(fc.ToArray())) {
                if (!thisrow.CellIsNullOrEmpty(this)) {
                    if (!thisrow.CellGetString(this).IsDouble()) { return null; }
                    summ += thisrow.CellGetDouble(this);
                }
            }
        }
        return summ;
    }

    //        default:
    //            Develop.DebugPrint(ErrorType.Warning);
    //            break;
    //    }
    //}
    //public string SimplyFile(string fullFileName) {
    //    if (_format != DataFormat.Link_To_Filesystem) {
    //        Develop.DebugPrint(ErrorType.Error, "Nur bei Link_To_Filesystem erlaubt!");
    //    }
    //    var tmpfile = fullFileName.FileNameWithoutSuffix();
    //    if (string.Equals(tmpfile, fullFileName, StringComparison.OrdinalIgnoreCase)) { return tmpfile; }
    //    if (string.Equals(BestFile(tmpfile, false), fullFileName, StringComparison.OrdinalIgnoreCase)) { return tmpfile; }
    //    tmpfile = fullFileName.FileNameWithSuffix();
    //    return string.Equals(tmpfile, fullFileName, StringComparison.OrdinalIgnoreCase)
    //        ? tmpfile
    //        : string.Equals(BestFile(tmpfile, false), fullFileName, StringComparison.OrdinalIgnoreCase) ? tmpfile : fullFileName;
    //}
    public double? Summe(IEnumerable<RowItem>? sort) {
        if (sort == null) { return null; }

        double summ = 0;
        foreach (var thisrow in sort) {
            if (thisrow != null && !thisrow.CellIsNullOrEmpty(this)) {
                if (!thisrow.CellGetString(this).IsDouble()) { return null; }
                summ += thisrow.CellGetDouble(this);
            }
        }
        return summ;
    }

    //        case FormatHolder.Bit:
    //            SetFormatForBit();
    //            break;
    public QuickImage? SymbolForReadableText() {
        if (IsDisposed) { return QuickImage.Get(ImageCode.Warnung); }
        if (IsDisposed || Table is not { IsDisposed: false } db) { return QuickImage.Get(ImageCode.Warnung); }

        if (this == db.Column.SysRowChanger) { return QuickImage.Get(ImageCode.Person); }
        if (this == db.Column.SysRowCreator) { return QuickImage.Get(ImageCode.Person); }
        if (this == db.Column.SysRowCreateDate) { return QuickImage.Get(ImageCode.Uhr); }
        if (this == db.Column.SysRowChangeDate) { return QuickImage.Get(ImageCode.Uhr); }

        if (this == db.Column.SysLocked) { return QuickImage.Get(ImageCode.Schloss); }

        if (this == db.Column.SysCorrect) { return QuickImage.Get(ImageCode.Warnung); }

        if (_isFirst) { return QuickImage.Get(ImageCode.Stern, 16); }
        if (_isKeyColumn) { return QuickImage.Get(ImageCode.Schlüssel, 16); }

        if (_value_for_Chunk != ChunkType.None) { return QuickImage.Get(ImageCode.Diskette, 16); }

        if (_relationship_to_First) { return QuickImage.Get(ImageCode.Herz, 16); }

        if (_relationType == RelationType.CellValues) { return QuickImage.Get(ImageCode.Fernglas, 16); }

        if (!_saveContent) { return QuickImage.Get(ImageCode.Tabelle, 16); }

        foreach (var thisFormat in FormatHolder.AllFormats) {
            if (thisFormat.IsFormatIdenticalSoft(this)) { return thisFormat.Image; }
        }

        if (_editableWithDropdown) {
            return QuickImage.Get("Pfeil_Unten_Scrollbar|14|||||0");
        }

        if (TextboxEditPossible()) {
            return _multiLine ? QuickImage.Get(ImageCode.Textfeld, 16, Color.Red, Color.Transparent) :
                           QuickImage.Get(ImageCode.Textfeld);
        }

        return QuickImage.Get(ImageCode.Warnung);
    }

    public void SystemInfoReset(bool always) {
        if (always || string.IsNullOrEmpty(ColumnSystemInfo)) {
            ColumnSystemInfo = "Seit UTC: " + DateTime.UtcNow.ToString5();
        }
    }

    public bool TextboxEditPossible() {
        //if (_value_for_Chunk != ChunkType.None) { return false; }
        if (_relationType == RelationType.DropDownValues) { return false; }
        return true;
    }

    public override string ToString() => IsDisposed ? string.Empty : _keyName + " -> " + Caption;

    /// <summary>
    /// CallByFileName Aufrufe werden nicht geprüft
    /// </summary>
    /// <returns></returns>
    public bool UsedInScript() {
        if (IsDisposed || Table is not { IsDisposed: false }) { return false; }

        foreach (var thiss in Table.EventScript) {
            if (thiss.Script.ContainsWord(_keyName, RegexOptions.IgnoreCase)) { return true; }
        }

        return false;
    }

    //        case FormatHolder.TextMitFormatierung:
    //            SetFormatForTextMitFormatierung();
    //            break;
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
            var col = LinkedTable?.Column[_columnNameOfLinkedTable];
            if (col == null) { return false; }
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
        if (editTypeToCheck == EditTypeFormula.nur_als_Text_anzeigen) { return true; }
        if (!_multiLine && editTypeToCheck == EditTypeFormula.Ja_Nein_Knopf) { return true; }
        return false;
    }

    internal static string MakeValidColumnName(string columnname) => columnname.Trim().ToUpperInvariant().Replace(" ", "_").Replace("__", "_").ReduceToChars(AllowedCharsVariableName);

    internal void Optimize() {
        if (!IsSystemColumn()) {
            // Maximale Text-Länge beeinflusst stark die Ladezeit vom Server
            var l = CalculatePreveredMaxCellLenght(1.2f);
            if (l < MaxCellLenght) { MaxCellLenght = l; }

            if (MaxTextLenght > MaxCellLenght) { MaxTextLenght = MaxCellLenght; }

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
    /// <param name="newvalue"></param>
    /// <returns></returns>
    internal string SetValueInternal(TableDataType type, string newvalue) {
        if (type.IsObsolete()) { return string.Empty; }

        switch (type) {
            case TableDataType.ColumnName:
                var oldname = _keyName;
                _keyName = newvalue.ToUpperInvariant();
                var ok = Table?.Column.ChangeName(oldname, _keyName) ?? "Tabelle verworfen";

                if (!string.IsNullOrEmpty(ok)) {
                    var reason = "Schwerer Spalten Umbenennungsfehler, " + ok;

                    Table?.Freeze(reason);
                    return reason;
                }
                break;

            case TableDataType.ColumnCaption:
                _caption = newvalue;
                break;

            case TableDataType.ForeColor:
                _foreColor = Color.FromArgb(IntParse(newvalue));
                break;

            case TableDataType.BackColor:
                _backColor = Color.FromArgb(IntParse(newvalue));
                break;

            case TableDataType.LineStyleLeft:
                _lineStyleLeft = (ColumnLineStyle)IntParse(newvalue);
                break;

            case TableDataType.LineStyleRight:
                _lineStyleRight = (ColumnLineStyle)IntParse(newvalue);
                break;

            case TableDataType.ColumnQuickInfo:
                _columnQuickInfo = newvalue;
                break;

            case TableDataType.CaptionGroup1:
                _captionGroup1 = newvalue;
                break;

            case TableDataType.CaptionGroup2:
                _captionGroup2 = newvalue;
                break;

            case TableDataType.CaptionGroup3:
                _captionGroup3 = newvalue;
                break;

            case TableDataType.MultiLine:
                _multiLine = newvalue.FromPlusMinus();
                break;

            case TableDataType.IsFirst:
                _isFirst = newvalue.FromPlusMinus();
                Table?.Column.GetSystems();
                break;

            case TableDataType.IsKeyColumn:
                _isKeyColumn = newvalue.FromPlusMinus();
                break;

            case TableDataType.Relationship_to_First:
                _relationship_to_First = newvalue.FromPlusMinus();
                break;

            case TableDataType.DropDownItems:
                _dropDownItems.SplitAndCutByCr_QuickSortAndRemoveDouble(newvalue);
                break;

            case TableDataType.LinkedCellFilter:
                _linkedCellFilter.SplitAndCutByCr(newvalue);
                break;

            //case TableDataType.OpticalTextReplace:
            //    _opticalReplace.SplitAndCutByCr(newvalue);
            //    break;

            case TableDataType.AutoReplaceAfterEdit:
                _afterEditAutoReplace.SplitAndCutByCr(newvalue);
                break;

            case TableDataType.RegexCheck:
                _regexCheck = newvalue;
                break;

            case TableDataType.ColumnTags:
                _columnTags.SplitAndCutByCr(newvalue);
                break;

            case TableDataType.AutoFilterJoker:
                _autoFilterJoker = newvalue;
                break;

            case TableDataType.PermissionGroupsChangeCell:
                _permissionGroupsChangeCell.SplitAndCutByCr_QuickSortAndRemoveDouble(newvalue);
                break;

            case TableDataType.AllowedChars:
                _allowedChars = newvalue;
                break;

            case TableDataType.MaxTextLenght:
                _maxTextLenght = IntParse(newvalue);
                //_maxCellLenght = _maxTextLenght;
                break;

            case TableDataType.FilterOptions:
                _filterOptions = (FilterOptions)IntParse(newvalue);
                break;

            case TableDataType.RelationType:
                _relationType = (RelationType)IntParse(newvalue);
                break;

            case TableDataType.Value_for_Chunk:
                _value_for_Chunk = (ChunkType)IntParse(newvalue);
                Table?.Column.GetSystems();
                break;

            case TableDataType.IgnoreAtRowFilter:
                _ignoreAtRowFilter = newvalue.FromPlusMinus();
                break;

            case TableDataType.SaveContent:
                _saveContent = newvalue.FromPlusMinus();
                break;

            case TableDataType.EditableWithTextInput:
                _editableWithTextInput = newvalue.FromPlusMinus();
                break;

            case TableDataType.EditableWithDropdown:
                _editableWithDropdown = newvalue.FromPlusMinus();
                break;

            case TableDataType.SpellCheckingEnabled:
                _spellCheckingEnabled = newvalue.FromPlusMinus();
                break;

            case TableDataType.DropdownDeselectAllAllowed:
                _dropdownDeselectAllAllowed = newvalue.FromPlusMinus();
                break;

            case TableDataType.ShowValuesOfOtherCellsInDropdown:
                _showValuesOfOtherCellsInDropdown = newvalue.FromPlusMinus();
                break;

            case TableDataType.SortAndRemoveDoubleAfterEdit:
                _afterEditQuickSortRemoveDouble = newvalue.FromPlusMinus();
                break;

            case TableDataType.RoundAfterEdit:
                _roundAfterEdit = IntParse(newvalue);
                break;

            case TableDataType.FixedColumnWidth:
                _fixedColumnWidth = IntParse(newvalue);
                break;

            case TableDataType.MaxCellLenght:
                _maxCellLenght = IntParse(newvalue);
                break;

            case TableDataType.DoUcaseAfterEdit:
                _afterEditDoUCase = newvalue.FromPlusMinus();
                break;

            case TableDataType.AutoCorrectAfterEdit:
                _afterEditAutoCorrect = newvalue.FromPlusMinus();
                break;

            case TableDataType.AutoRemoveCharAfterEdit:
                _autoRemove = newvalue;
                break;

            case TableDataType.ColumnAdminInfo:
                _adminInfo = newvalue;
                break;

            case TableDataType.ColumnSystemInfo:
                _columnSystemInfo = newvalue;
                break;

            case TableDataType.RendererSettings:
                _rendererSettings = newvalue;
                break;

            case TableDataType.DefaultRenderer:
                _defaultRenderer = newvalue;
                break;

            //case TableDataType.ColumnContentWidth:
            //    _contentwidth = IntParse(newvalue);
            //    ContentWidthIsValid = true;
            //    break;

            case TableDataType.CaptionBitmapCode:
                _captionBitmapCode = newvalue;
                break;

            case TableDataType.LinkedTableTableName:
                _linkedTableTableName = newvalue;
                Invalidate_LinkedTable();
                break;

            //case TableDataType.ConstantHeightOfImageCode:
            //    if (newvalue == "0") { newvalue = string.Empty; }
            //    _constantHeightOfImageCode = newvalue;
            //    break;

            //case (TableDataType)160: //TableDataType.Suffix:
            //    ManipulateRendererSettings("Suffix", newvalue);
            //    break;

            //case (TableDataType)177: //TableDataType.Prefix:
            //    ManipulateRendererSettings("Prefix", newvalue);
            //    break;

            case TableDataType.DoOpticalTranslation:
                _doOpticalTranslation = (TranslationType)IntParse(newvalue);
                break;

            case TableDataType.AdditionalFormatCheck:
                _additionalFormatCheck = (AdditionalCheck)IntParse(newvalue);
                break;

            case TableDataType.ScriptType:
                _scriptType = (ScriptType)IntParse(newvalue);
                Table?.Row.InvalidateAllCheckData();
                break;

            //case TableDataType.BehaviorOfImageAndText:
            //    _behaviorOfImageAndText = (BildTextVerhalten)IntParse(newvalue);
            //    break;

            case TableDataType.EditAllowedDespiteLock:
                _editAllowedDespiteLock = newvalue.FromPlusMinus();
                break;

            case TableDataType.TextFormatingAllowed:
                _textFormatingAllowed = newvalue.FromPlusMinus();
                break;

            //case TableDataType.CellInitValue:
            //    _cellInitValue = newvalue;
            //    break;

            case TableDataType.ColumnNameOfLinkedTable:

                _columnNameOfLinkedTable = newvalue.IsFormat(FormatHolder.Long) ? string.Empty : newvalue;

                break;

            case TableDataType.SortType:
                _sortType = string.IsNullOrEmpty(newvalue) ? SortierTyp.Original_String : (SortierTyp)LongParse(newvalue);
                break;

            case TableDataType.ColumnAlign:
                var tmpalign = (AlignmentHorizontal)IntParse(newvalue);
                if (tmpalign == (AlignmentHorizontal)(-1)) { tmpalign = AlignmentHorizontal.Links; }
                _align = tmpalign;
                break;

            default:
                if (!string.Equals(type.ToString(), ((int)type).ToString(), StringComparison.Ordinal)) {
                    return "Interner Fehler: Für den Datentyp '" + type + "' wurde keine Laderegel definiert.";
                }
                break;
        }
        return string.Empty;
    }

    private void _table_Disposing(object sender, System.EventArgs e) => Dispose();

    private void _TMP_Linked_table_Disposing(object sender, System.EventArgs e) => Invalidate_LinkedTable();

    private void _TMP_LinkedTable_Cell_CellValueChanged(object sender, CellEventArgs e) {
        if (e.Column.KeyName != ColumnNameOfLinkedTable) { return; }

        if (_relationType == RelationType.CellValues && LinkedTable != null) {
            var (fc, info) = CellCollection.GetFilterReverse(this, e.Column, e.Row);
            var val = e.Row.CellGetString(e.Column);

            if (fc != null && string.IsNullOrWhiteSpace(info)) {
                foreach (var thisRow in fc.Rows) {
                    if (thisRow.CellGetString(this) != val) {
                        _ = CellCollection.LinkedCellData(this, thisRow, true, false);
                    }
                }
                fc.Dispose();
            }
        }
    }

    private void CheckIfIAmAKeyColumn() {
        Am_A_Key_For_Other_Column = string.Empty;

        if (IsDisposed || Table is not { IsDisposed: false } db) { return; }

        foreach (var c in db.Column) {
            //if (thisColumn.KeyColumnKey == _name) { Am_A_Key_For_Other_Column = "Spalte " + thisColumn.ReadableText() + " verweist auf diese Spalte"; } // Werte Gleichhalten
            //if (thisColumn.LinkedCell_RowKeyIsInColumn == _name) { Am_A_Key_For_Other_Column = "Spalte " + thisColumn.ReadableText() + " verweist auf diese Spalte"; } // LinkdeCells pflegen
            //if (ThisColumn.LinkedCell_ColumnValueFoundIn == _name) { I_Am_A_Key_For_Other_Column = "Spalte " + ThisColumn.ReadableText() + " verweist auf diese Spalte"; } // LinkdeCells pflegen
            if (c.RelationType == RelationType.CellValues) {
                foreach (var thisitem in c.LinkedCellFilter) {
                    var tmp = thisitem.SplitBy("|");

                    if (tmp[2].ToLowerInvariant().Contains("~" + _keyName.ToLowerInvariant() + "~")) {
                        Am_A_Key_For_Other_Column = "Spalte " + c.ReadableText() + " verweist auf diese Spalte";
                    }
                }
            }
        }
        //if (_format == DataFormat.Columns_für_LinkedCellDropdown) { Am_A_Key_For_Other_Column = "Die Spalte selbst durch das Format"; }
    }

    private void Dispose(bool disposing) {
        if (!IsDisposed) {
            if (disposing) {
                OnDisposingEvent();
                // TODO: Verwalteten Zustand (verwaltete Objekte) bereinigen
                Table = null;
                Invalidate_LinkedTable();
            }

            _afterEditAutoReplace.Clear();
            _dropDownItems.Clear();
            _linkedCellFilter.Clear();
            _permissionGroupsChangeCell.Clear();
            _columnTags.Clear();

            // TODO: Nicht verwaltete Ressourcen (nicht verwaltete Objekte) freigeben und Finalizer überschreiben
            // TODO: Große Felder auf NULL setzen
            IsDisposed = true;
        }
    }

    private void GetLinkedTable() {
        Invalidate_LinkedTable(); // Events sicher abmelden

        if (IsDisposed || Table is not { IsDisposed: false }) { return; }

        var newTable = Get(_linkedTableTableName, false, null);

        if (newTable != null) {
            // Event-Registrierung vor dem Lock
            newTable.Cell.CellValueChanged += _TMP_LinkedTable_Cell_CellValueChanged;
            newTable.DisposingEvent += _TMP_Linked_table_Disposing;
        }

        lock (_linkedTableLock) {
            _linkedTable = newTable; // Atomic assignment
        }
    }

    private void Invalidate_LinkedTable() {
        Table? tableToCleanup = null;

        lock (_linkedTableLock) {
            tableToCleanup = _linkedTable;
            _linkedTable = null; // Sofort auf null setzen (fail-fast)
        }

        // Event-Abmeldung außerhalb des Locks um Deadlocks zu vermeiden
        if (tableToCleanup != null) {
            try {
                tableToCleanup.Cell.CellValueChanged -= _TMP_LinkedTable_Cell_CellValueChanged;
            } catch (Exception) {
                // Events können bereits abgemeldet sein - ignorieren
            }

            try {
                tableToCleanup.DisposingEvent -= _TMP_Linked_table_Disposing;
            } catch (Exception) {
                // Events können bereits abgemeldet sein - ignorieren
            }
        }
    }

    private string KleineFehlerCorrect(string txt) {
        if (string.IsNullOrEmpty(txt)) { return string.Empty; }
        if (TextFormatingAllowed) { txt = txt.HtmlSpecialToNormalChar(false); }
        string oTxt;
        do {
            oTxt = txt;
            if (oTxt.ToLowerInvariant().Contains(".at")) { break; }
            if (oTxt.ToLowerInvariant().Contains(".de")) { break; }
            if (oTxt.ToLowerInvariant().Contains(".com")) { break; }
            if (oTxt.ToLowerInvariant().Contains("http")) { break; }
            if (oTxt.ToLowerInvariant().Contains("ftp")) { break; }
            if (oTxt.ToLowerInvariant().Contains(".xml")) { break; }
            if (oTxt.ToLowerInvariant().Contains(".doc")) { break; }
            if (oTxt.IsDateTime()) { break; }
            txt = txt.Replace("\r\n", "\r");
            // 1/2 l Milch
            // 3-5 Stunden
            // 180°C
            // Nach Zahlen KEINE leerzeichen einfügen. Es gibt so viele dinge.... 90er Schichtsalat
            txt = txt.Insert(" ", ",", "1234567890, \r");
            txt = txt.Insert(" ", "!", " !?)\r");
            txt = txt.Insert(" ", "?", " !?)\r");
            txt = txt.Insert(" ", ".", " 1234567890.!?/)\r");
            txt = txt.Insert(" ", ")", " .;!?\r");
            txt = txt.Insert(" ", ";", " 1234567890\r");
            txt = txt.Insert(" ", ":", "1234567890 \\/\r"); // auch 3:50 Uhr
                                                            // H4= Normaler Text
            txt = txt.Replace(" " + TempH4, TempH4 + " "); // H4 = Normaler Text, nach links rutschen
            txt = txt.Replace("\r" + TempH4, TempH4 + "\r");
            // Die restlichen Hs'
            txt = txt.Replace(TempH3 + " ", " " + TempH3); // Überschrift, nach Rechts
            txt = txt.Replace(TempH2 + " ", " " + TempH2); // Überschrift, nach Rechts
            txt = txt.Replace(TempH1 + " ", " " + TempH1); // Überschrift, nach Rechts
            txt = txt.Replace(TempBold + " ", " " + TempBold); // Bold, nach Rechts
            txt = txt.Replace(TempH3 + "\r", "\r" + TempH3); // Überschrift, nach Rechts
            txt = txt.Replace(TempH2 + "\r", "\r" + TempH2); // Überschrift, nach Rechts
            txt = txt.Replace(TempH1 + "\r", "\r" + TempH1); // Überschrift, nach Rechts
            txt = txt.Replace(TempBold + "\r", "\r" + TempBold); // Bold, nach Rechts
            txt = txt.Replace(TempBold + TempH4.ToString(), TempH4.ToString());
            txt = txt.Replace(TempH3 + TempH4.ToString(), TempH4.ToString());
            txt = txt.Replace(TempH2 + TempH4.ToString(), TempH4.ToString());
            txt = txt.Replace(TempH1 + TempH4.ToString(), TempH4.ToString());
            txt = txt.Replace(TempH4 + TempH4.ToString(), TempH4.ToString());
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
            txt = txt.Trim("\r");
            txt = txt.TrimEnd("\t");
        } while (oTxt != txt);

        if (TextFormatingAllowed) {
            txt = txt.CreateHtmlCodes(true);
            txt = txt.Replace("<br>", "\r");
        }
        return txt;
    }

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