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
using BlueDatabase.Enums;
using BlueDatabase.EventArgs;
using BlueDatabase.Interfaces;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using static BlueBasics.Constants;
using static BlueBasics.Converter;
using static BlueBasics.IO;
using static BlueDatabase.Database;

namespace BlueDatabase;

public sealed class ColumnItem : IReadableTextWithPropertyChangingAndKey, IColumnInputFormat, IErrorCheckable, IHasDatabase, IDisposableExtendedWithEvent, IEditable {

    #region Fields

    internal List<string>? UcaseNamesSortedByLenght;
    private const string TmpNewDummy = "TMPNEWDUMMY";
    private readonly List<string> _afterEditAutoReplace = [];
    private readonly List<string> _columnTags = [];
    private readonly List<string> _dropDownItems = [];
    private readonly List<string> _linkedCellFilter = [];
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
    /// Die Quell-Spalte (aus der verlinkten Datenbank) ist immer
    /// </summary>
    private string _columnNameOfLinkedDatabase;

    private string _columnQuickInfo;

    private string _columnSystemInfo;

    //private string _cellInitValue;
    private Database? _database;

    private string _defaultRenderer;
    private TranslationType _doOpticalTranslation;
    private bool _dropdownDeselectAllAllowed;
    private bool _editableWithDropdown;
    private bool _editableWithTextInput;
    private bool _editAllowedDespiteLock;
    private FilterOptions _filterOptions;
    private int _fixedColumnWidth;
    private Color _foreColor;
    private ColumnFunction _function;
    private bool _ignoreAtRowFilter;
    private string _keyName;
    private ColumnLineStyle _lineStyleLeft;
    private ColumnLineStyle _lineStyleRight;

    /// <summary>
    /// Diese Variable ist der tempor�re Wert und wird von _linkedDatabaseFile abgeleitet.
    /// </summary>
    private Database? _linkedDatabase;

    private string _linkedDatabaseTableName;
    private int _maxCellLenght;
    private int _maxTextLenght;
    private bool _multiLine;
    private string _regexCheck = string.Empty;
    private string _rendererSettings;
    private int _roundAfterEdit;
    private ScriptType _scriptType;
    private bool _showUndo;
    private bool _showValuesOfOtherCellsInDropdown;
    private SortierTyp _sortType;
    private bool _spellCheckingEnabled;
    private bool _textFormatingAllowed;

    #endregion

    #region Constructors

    public ColumnItem(Database database, string name) {
        if (!IsValidColumnName(name)) {
            Develop.DebugPrint(ErrorType.Error, "Spaltenname nicht erlaubt!");
        }

        Database = database;

        var ex = database.Column[name];
        if (ex != null) {
            Develop.DebugPrint(ErrorType.Error, "Key existiert bereits");
        }

        #region Standard-Werte

        _keyName = name;
        _caption = string.Empty;
        //_CaptionBitmapCode = null;
        _function = ColumnFunction.Normal;
        _lineStyleLeft = ColumnLineStyle.D�nn;
        _lineStyleRight = ColumnLineStyle.Ohne;
        _multiLine = false;
        _columnQuickInfo = string.Empty;
        _captionGroup1 = string.Empty;
        _captionGroup2 = string.Empty;
        _captionGroup3 = string.Empty;
        //_Intelligenter_Multifilter = string.Empty;
        _foreColor = Color.Black;
        _backColor = Color.White;
        //_cellInitValue = string.Empty;
        //_linkedCellRowKeyIsInColumn = -1;
        _columnNameOfLinkedDatabase = string.Empty;
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
        //_saveContent = true;
        //_AutoFilter_Dauerfilter = enDauerfilter.ohne;
        _spellCheckingEnabled = false;
        //_CompactView = true;
        _showUndo = true;
        _doOpticalTranslation = TranslationType.Original_Anzeigen;
        _editAllowedDespiteLock = false;
        _linkedDatabaseTableName = string.Empty;
        UcaseNamesSortedByLenght = null;
        Am_A_Key_For_Other_Column = string.Empty;

        #endregion Standard-Werte

        Invalidate_LinkedDatabase();
    }

    #endregion

    #region Destructors

    ~ColumnItem() {
        // �ndern Sie diesen Code nicht. F�gen Sie Bereinigungscode in der Methode "Dispose(bool disposing)" ein.
        Dispose(false);
    }

    #endregion

    #region Events

    public event EventHandler? DisposingEvent;

    //private string _vorschlagsColumn;
    // TODO: Finalizer nur �berschreiben, wenn "Dispose(bool disposing)" Code f�r die Freigabe nicht verwalteter Ressourcen enth�lt
    public event PropertyChangedEventHandler? PropertyChanged;

    #endregion

    #region Properties

    public AdditionalCheck AdditionalFormatCheck {
        get => _additionalFormatCheck;
        set {
            if (IsDisposed) { return; }
            if (_additionalFormatCheck == value) { return; }

            _ = Database?.ChangeData(DatabaseDataType.AdditionalFormatCheck, this, null, ((int)_additionalFormatCheck).ToString(), ((int)value).ToString(), Generic.UserName, DateTime.UtcNow, string.Empty, string.Empty);
            OnPropertyChanged("AdditionalFormatCheck");
        }
    }

    [Description("Ein Information f�r Administratoren. Freier Text.\r\nWird als Quickinfo angezeigt, wenn der Admininstror\r\n mit der Maus �ber den Spaltenkopf f�hrt.")]
    public string AdminInfo {
        get => _adminInfo;
        set {
            if (IsDisposed) { return; }
            if (_adminInfo == value) { return; }

            _ = Database?.ChangeData(DatabaseDataType.ColumnAdminInfo, this, null, _adminInfo, value, Generic.UserName, DateTime.UtcNow, string.Empty, string.Empty);
            OnPropertyChanged("AdminInfo");
        }
    }

    public bool AfterEditAutoCorrect {
        get => _afterEditAutoCorrect;
        set {
            if (IsDisposed) { return; }
            if (_afterEditAutoCorrect == value) { return; }

            _ = Database?.ChangeData(DatabaseDataType.AutoCorrectAfterEdit, this, null, _afterEditAutoCorrect.ToPlusMinus(), value.ToPlusMinus(), Generic.UserName, DateTime.UtcNow, string.Empty, string.Empty);
            OnPropertyChanged("AfterEditAutoCorrect");
        }
    }

    //    #region Standard-Werte
    public ReadOnlyCollection<string> AfterEditAutoReplace {
        get => new(_afterEditAutoReplace);
        set {
            if (IsDisposed) { return; }
            if (!_afterEditAutoReplace.IsDifferentTo(value)) { return; }

            _ = Database?.ChangeData(DatabaseDataType.AutoReplaceAfterEdit, this, null, _afterEditAutoReplace.JoinWithCr(), value.JoinWithCr(), Generic.UserName, DateTime.UtcNow, string.Empty, string.Empty);
            OnPropertyChanged("AfterEditAutoReplace");
        }
    }

    //    _key = columnkey;
    public bool AfterEditDoUCase {
        get => _afterEditDoUCase;
        set {
            if (IsDisposed) { return; }
            if (_afterEditDoUCase == value) { return; }

            _ = Database?.ChangeData(DatabaseDataType.DoUcaseAfterEdit, this, null, _afterEditDoUCase.ToPlusMinus(), value.ToPlusMinus(), Generic.UserName, DateTime.UtcNow, string.Empty, string.Empty);
            OnPropertyChanged("AfterEditDoUCase");
        }
    }

    //    var ex = database.Column.SearchByKey(columnkey);
    //    if (ex != null) {
    //        Develop.DebugPrint(ErrorType.Error, "_name existiert bereits");
    //    }
    public bool AfterEditQuickSortRemoveDouble {
        get => _multiLine && _afterEditQuickSortRemoveDouble;
        set {
            if (IsDisposed) { return; }
            if (_afterEditQuickSortRemoveDouble == value) { return; }

            _ = Database?.ChangeData(DatabaseDataType.SortAndRemoveDoubleAfterEdit, this, null, _afterEditQuickSortRemoveDouble.ToPlusMinus(), value.ToPlusMinus(), Generic.UserName, DateTime.UtcNow, string.Empty, string.Empty);
            OnPropertyChanged("AfterEditQuickSortRemoveDouble");
        }
    }

    public AlignmentHorizontal Align {
        get => _align;
        set {
            if (IsDisposed) { return; }
            if (_align == value) { return; }

            _ = Database?.ChangeData(DatabaseDataType.ColumnAlign, this, null, ((int)_align).ToString(), ((int)value).ToString(), Generic.UserName, DateTime.UtcNow, string.Empty, string.Empty);
            OnPropertyChanged("Align");
        }
    }

    public string AllowedChars {
        get => _allowedChars;
        set {
            if (IsDisposed) { return; }
            if (_allowedChars == value) { return; }

            _ = Database?.ChangeData(DatabaseDataType.AllowedChars, this, null, _allowedChars, value, Generic.UserName, DateTime.UtcNow, string.Empty, string.Empty);
            OnPropertyChanged("AllowedChars");
        }
    }

    public string Am_A_Key_For_Other_Column { get; private set; }

    public string AutoFilterJoker {
        get => _autoFilterJoker;
        set {
            if (IsDisposed) { return; }
            if (_autoFilterJoker == value) { return; }

            _ = Database?.ChangeData(DatabaseDataType.AutoFilterJoker, this, null, _autoFilterJoker, value, Generic.UserName, DateTime.UtcNow, string.Empty, string.Empty);
            OnPropertyChanged("AutoFilterJoker");
        }
    }

    public string AutoRemove {
        get => _autoRemove;
        set {
            if (IsDisposed) { return; }
            if (_autoRemove == value) { return; }

            _ = Database?.ChangeData(DatabaseDataType.AutoRemoveCharAfterEdit, this, null, _autoRemove, value, Generic.UserName, DateTime.UtcNow, string.Empty, string.Empty);
            OnPropertyChanged("AutoRemove");
        }
    }

    public Color BackColor {
        get => _backColor;
        set {
            if (IsDisposed) { return; }
            if (_backColor.ToArgb() == value.ToArgb()) { return; }

            _ = Database?.ChangeData(DatabaseDataType.BackColor, this, null, _backColor.ToArgb().ToString(), value.ToArgb().ToString(), Generic.UserName, DateTime.UtcNow, string.Empty, string.Empty);
            OnPropertyChanged("BackColor");
        }
    }

    public string Caption {
        get => _caption;
        set {
            if (IsDisposed) { return; }
            value = value.Replace("<br>", "\r", RegexOptions.IgnoreCase);
            if (_caption == value) { return; }

            _ = Database?.ChangeData(DatabaseDataType.ColumnCaption, this, null, _caption, value, Generic.UserName, DateTime.UtcNow, string.Empty, string.Empty);
            OnPropertyChanged("Caption");
        }
    }

    public string CaptionBitmapCode {
        get => _captionBitmapCode;
        set {
            if (IsDisposed) { return; }
            if (_captionBitmapCode == value) { return; }

            _ = Database?.ChangeData(DatabaseDataType.CaptionBitmapCode, this, null, _captionBitmapCode, value, Generic.UserName, DateTime.UtcNow, string.Empty, string.Empty);
            _captionBitmapCode = value;
            OnPropertyChanged("CaptionBitmapCode");
        }
    }

    public string CaptionForEditor => "Spalte";

    public string CaptionGroup1 {
        get => _captionGroup1;
        set {
            if (IsDisposed) { return; }
            if (_captionGroup1 == value) { return; }

            _ = Database?.ChangeData(DatabaseDataType.CaptionGroup1, this, null, _captionGroup1, value, Generic.UserName, DateTime.UtcNow, string.Empty, string.Empty);
            OnPropertyChanged("CaptionGroup1");
        }
    }

    public string CaptionGroup2 {
        get => _captionGroup2;
        set {
            if (IsDisposed) { return; }
            if (_captionGroup2 == value) { return; }

            _ = Database?.ChangeData(DatabaseDataType.CaptionGroup2, this, null, _captionGroup2, value, Generic.UserName, DateTime.UtcNow, string.Empty, string.Empty);
            OnPropertyChanged("CaptionGroup2");
        }
    }

    public string CaptionGroup3 {
        get => _captionGroup3;
        set {
            if (IsDisposed) { return; }
            if (_captionGroup3 == value) { return; }

            _ = Database?.ChangeData(DatabaseDataType.CaptionGroup3, this, null, _captionGroup3, value, Generic.UserName, DateTime.UtcNow, string.Empty, string.Empty);
            OnPropertyChanged("CaptionGroup3");
        }
    }

    public string CaptionsCombined {
        get {
            var txt = _captionGroup1 + "/" + _captionGroup2 + "/" + _captionGroup3;
            return txt == "//" ? "###" : txt.TrimEnd("/");
        }
    }

    public string ColumnNameOfLinkedDatabase {
        get => _columnNameOfLinkedDatabase;
        set {
            if (IsDisposed) { return; }
            if (value == "-1") { value = string.Empty; }

            if (_columnNameOfLinkedDatabase == value) { return; }

            _ = Database?.ChangeData(DatabaseDataType.ColumnNameOfLinkedDatabase, this, null, _columnNameOfLinkedDatabase, value, Generic.UserName, DateTime.UtcNow, string.Empty, string.Empty);
            Invalidate_ColumAndContent();
            OnPropertyChanged("ColumnNameOfLinkedDatabase");
        }
    }

    public string ColumnQuickInfo {
        get => _columnQuickInfo;
        set {
            if (IsDisposed) { return; }
            if (_columnQuickInfo == value) { return; }

            _ = Database?.ChangeData(DatabaseDataType.ColumnQuickInfo, this, null, _columnQuickInfo, value, Generic.UserName, DateTime.UtcNow, string.Empty, string.Empty);
            OnPropertyChanged("ColumnQuickInfo");
        }
    }

    public string ColumnSystemInfo {
        get => _columnSystemInfo;
        private set {
            if (IsDisposed) { return; }
            if (_columnSystemInfo == value) { return; }

            _ = Database?.ChangeData(DatabaseDataType.ColumnSystemInfo, this, null, _adminInfo, value, Generic.UserName, DateTime.UtcNow, string.Empty, string.Empty);
            OnPropertyChanged("ColumnSystemInfo");
        }
    }

    /// <summary>
    /// Was in Textfeldern oder Datenbankzeilen f�r ein Suffix angezeigt werden soll. Beispiel: mm
    /// </summary>
    public List<string> ColumnTags {
        get => _columnTags;
        set {
            if (IsDisposed) { return; }
            if (!_columnTags.IsDifferentTo(value)) { return; }

            _ = Database?.ChangeData(DatabaseDataType.ColumnTags, this, null, _columnTags.JoinWithCr(), value.JoinWithCr(), Generic.UserName, DateTime.UtcNow, string.Empty, string.Empty);
            OnPropertyChanged("ColumnTags");
        }
    }

    public Database? Database {
        get => _database;
        private set {
            if (IsDisposed || (value?.IsDisposed ?? true)) { value = null; }
            if (value == _database) { return; }

            if (_database != null) {
                _database.DisposingEvent -= _database_Disposing;
            }
            _database = value;

            if (_database != null) {
                _database.DisposingEvent += _database_Disposing;
            }
        }
    }

    public string DefaultRenderer {
        get => _defaultRenderer;
        set {
            if (IsDisposed) { return; }
            if (_defaultRenderer == value) { return; }

            _ = Database?.ChangeData(DatabaseDataType.DefaultRenderer, this, null, _defaultRenderer, value, Generic.UserName, DateTime.UtcNow, string.Empty, string.Empty);
            OnPropertyChanged("DefaultRenderer");
        }
    }

    public TranslationType DoOpticalTranslation {
        get => _doOpticalTranslation;
        set {
            if (IsDisposed) { return; }
            if (_doOpticalTranslation == value) { return; }

            _ = Database?.ChangeData(DatabaseDataType.DoOpticalTranslation, this, null, ((int)_doOpticalTranslation).ToString(), ((int)value).ToString(), Generic.UserName, DateTime.UtcNow, string.Empty, string.Empty);
            OnPropertyChanged("DoOpticalTranslation");
        }
    }

    public bool DropdownDeselectAllAllowed {
        get => _dropdownDeselectAllAllowed;
        set {
            if (IsDisposed) { return; }
            if (_dropdownDeselectAllAllowed == value) { return; }

            _ = Database?.ChangeData(DatabaseDataType.DropdownDeselectAllAllowed, this, null, _dropdownDeselectAllAllowed.ToPlusMinus(), value.ToPlusMinus(), Generic.UserName, DateTime.UtcNow, string.Empty, string.Empty);
            OnPropertyChanged("DropdownDeselectAllAllowed");
        }
    }

    public ReadOnlyCollection<string> DropDownItems {
        get => new(_dropDownItems);
        set {
            if (IsDisposed) { return; }
            if (!_dropDownItems.IsDifferentTo(value)) { return; }

            _ = Database?.ChangeData(DatabaseDataType.DropDownItems, this, null, _dropDownItems.JoinWithCr(), value.JoinWithCr(), Generic.UserName, DateTime.UtcNow, string.Empty, string.Empty);
            OnPropertyChanged("DropDownItems");
        }
    }

    public bool EditableWithDropdown {
        get => _editableWithDropdown;
        set {
            if (IsDisposed) { return; }
            if (_editableWithDropdown == value) { return; }

            _ = Database?.ChangeData(DatabaseDataType.EditableWithDropdown, this, null, _editableWithDropdown.ToPlusMinus(), value.ToPlusMinus(), Generic.UserName, DateTime.UtcNow, string.Empty, string.Empty);
            OnPropertyChanged("EditableWithDropdown");
        }
    }

    public bool EditableWithTextInput {
        get => _editableWithTextInput;
        set {
            if (IsDisposed) { return; }
            if (_editableWithTextInput == value) { return; }

            _ = Database?.ChangeData(DatabaseDataType.EditableWithTextInput, this, null, _editableWithTextInput.ToPlusMinus(), value.ToPlusMinus(), Generic.UserName, DateTime.UtcNow, string.Empty, string.Empty);
            OnPropertyChanged("EditableWithTextInput");
        }
    }

    public bool EditAllowedDespiteLock {
        get => _editAllowedDespiteLock;
        set {
            if (IsDisposed) { return; }
            if (_editAllowedDespiteLock == value) { return; }

            _ = Database?.ChangeData(DatabaseDataType.EditAllowedDespiteLock, this, null, _editAllowedDespiteLock.ToPlusMinus(), value.ToPlusMinus(), Generic.UserName, DateTime.UtcNow, string.Empty, string.Empty);
            OnPropertyChanged("EditAllowedDespiteLock");
        }
    }

    public Type? Editor { get; set; }

    public FilterOptions FilterOptions {
        get => _filterOptions;
        set {
            if (IsDisposed) { return; }
            if (_filterOptions == value) { return; }

            _ = Database?.ChangeData(DatabaseDataType.FilterOptions, this, null, ((int)_filterOptions).ToString(), ((int)value).ToString(), Generic.UserName, DateTime.UtcNow, string.Empty, string.Empty);
            OnPropertyChanged("FilterOptions");
        }
    }

    public int FixedColumnWidth {
        get => _fixedColumnWidth;
        set {
            if (IsDisposed) { return; }
            if (_fixedColumnWidth == value) { return; }
            _ = Database?.ChangeData(DatabaseDataType.FixedColumnWidth, this, null, _fixedColumnWidth.ToString(), value.ToString(), Generic.UserName, DateTime.UtcNow, string.Empty, string.Empty);
            OnPropertyChanged("FixedColumnWidth");
        }
    }

    public Color ForeColor {
        get => _foreColor;
        set {
            if (IsDisposed) { return; }
            if (_foreColor.ToArgb() == value.ToArgb()) { return; }

            _ = Database?.ChangeData(DatabaseDataType.ForeColor, this, null, _foreColor.ToArgb().ToString(), value.ToArgb().ToString(), Generic.UserName, DateTime.UtcNow, string.Empty, string.Empty);
            OnPropertyChanged("ForeColor");
        }
    }

    public ColumnFunction Function {
        get => _function;
        set {
            if (IsDisposed) { return; }
            if (_function == value) { return; }

            var wasSplit = _function is ColumnFunction.Split_Medium or ColumnFunction.Split_Large or ColumnFunction.Split_Name;
            var oldd = _function;

            _ = Database?.ChangeData(DatabaseDataType.ColumnFunction, this, null, ((int)_function).ToString(), ((int)value).ToString(), Generic.UserName, DateTime.UtcNow, string.Empty, string.Empty);
            Invalidate_ColumAndContent();

            var willBeSplit = _function is ColumnFunction.Split_Medium or ColumnFunction.Split_Large or ColumnFunction.Split_Name;

            if ((wasSplit || willBeSplit) && oldd != _function) {
                Database?.Column.GetSystems();
                Database?.ReorganizeChunks();
            }

            OnPropertyChanged("Function");
        }
    }

    public bool IgnoreAtRowFilter {
        get => !_function.Autofilter_m�glich() || _ignoreAtRowFilter;
        set {
            if (IsDisposed) { return; }
            if (_ignoreAtRowFilter == value) { return; }

            _ = Database?.ChangeData(DatabaseDataType.IgnoreAtRowFilter, this, null, _ignoreAtRowFilter.ToPlusMinus(), value.ToPlusMinus(), Generic.UserName, DateTime.UtcNow, string.Empty, string.Empty);
            OnPropertyChanged("IgnoreAtRowFilter");
        }
    }

    public bool IsDisposed { get; private set; }

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

            if (Database?.Column[value] != null) {
                Develop.DebugPrint(ErrorType.Warning, "Name existiert bereits!");
                return;
            }

            //if (!IsValidColumnName(value)) {
            //    Develop.DebugPrint(ErrorType.Warning, "Spaltenname nicht erlaubt!");
            //    return;
            //}

            _ = Database?.ChangeData(DatabaseDataType.ColumnName, this, null, _keyName, value, Generic.UserName, DateTime.UtcNow, string.Empty, string.Empty);
            OnPropertyChanged("KeyName");
            CheckIfIAmAKeyColumn();
        }
    }

    public ColumnLineStyle LineStyleLeft {
        get => _lineStyleLeft;
        set {
            if (IsDisposed) { return; }
            if (_lineStyleLeft == value) { return; }

            _ = Database?.ChangeData(DatabaseDataType.LineStyleLeft, this, null, ((int)_lineStyleLeft).ToString(), ((int)value).ToString(), Generic.UserName, DateTime.UtcNow, string.Empty, string.Empty);
            OnPropertyChanged("LineStyleLeft");
        }
    }

    //        var c = Database?.Column.SearchByKey(_keyColumnKey);
    //        c?.CheckIfIAmAKeyColumn();
    //        Database?.ChangeData(DatabaseDataType.KeyColumnKey, _name, null, _keyColumnKey.ToString(false), value.ToString(false), string.Empty);
    //        c = Database?.Column.SearchByKey(_keyColumnKey);
    //        c?.CheckIfIAmAKeyColumn();
    //        OnPropertyChanged(string propertyname);
    //    }
    //}
    public ColumnLineStyle LineStyleRight {
        get => _lineStyleRight;
        set {
            if (IsDisposed) { return; }
            if (_lineStyleRight == value) { return; }

            _ = Database?.ChangeData(DatabaseDataType.LineStyleRight, this, null, ((int)_lineStyleRight).ToString(), ((int)value).ToString(), Generic.UserName, DateTime.UtcNow, string.Empty, string.Empty);
            OnPropertyChanged("LineStyleRight");
        }
    }

    /// <summary>
    /// Die Quell-Spalte (aus der verlinkten Datenbank) ist immer
    /// </summary>
    public List<string> LinkedCellFilter {
        get => _linkedCellFilter;
        set {
            if (IsDisposed || Database is not { IsDisposed: false } db) { return; }

            value = value.SortedDistinctList();

            if (!_linkedCellFilter.IsDifferentTo(value)) { return; }

            _ = db.ChangeData(DatabaseDataType.LinkedCellFilter, this, null, _linkedCellFilter.JoinWithCr(), value.JoinWithCr(), Generic.UserName, DateTime.UtcNow, string.Empty, string.Empty);
            OnPropertyChanged("LinkedCellFilter");

            foreach (var thisColumn in db.Column) {
                thisColumn.CheckIfIAmAKeyColumn();
            }
        }
    }

    public Database? LinkedDatabase {
        get {
            if (IsDisposed || Database is not { IsDisposed: false }) { return null; }

            if (string.IsNullOrEmpty(_linkedDatabaseTableName)) { return null; }

            if (_linkedDatabase is { IsDisposed: false }) {
                return _linkedDatabase;
            }

            GetLinkedDatabase();
            return _linkedDatabase;
        }
    }

    public string LinkedDatabaseTableName {
        get => _linkedDatabaseTableName;
        set {
            if (IsDisposed) { return; }
            if (_linkedDatabaseTableName == value) { return; }

            _ = Database?.ChangeData(DatabaseDataType.LinkedDatabaseTableName, this, null, _linkedDatabaseTableName, value, Generic.UserName, DateTime.UtcNow, string.Empty, string.Empty);
            Invalidate_LinkedDatabase();
            OnPropertyChanged("LinkedDatabaseTableName");
        }
    }

    public int MaxCellLenght {
        get => _maxCellLenght;
        set {
            if (IsDisposed) { return; }
            if (_maxCellLenght == value) { return; }
            _ = Database?.ChangeData(DatabaseDataType.MaxCellLenght, this, null, _maxCellLenght.ToString(), value.ToString(), Generic.UserName, DateTime.UtcNow, string.Empty, string.Empty);
            OnPropertyChanged("MaxCellLenght");
        }
    }

    public int MaxTextLenght {
        get => _maxTextLenght;
        set {
            if (IsDisposed) { return; }
            if (_maxTextLenght == value) { return; }
            _ = Database?.ChangeData(DatabaseDataType.MaxTextLenght, this, null, _maxTextLenght.ToString(), value.ToString(), Generic.UserName, DateTime.UtcNow, string.Empty, string.Empty);
            OnPropertyChanged("MaxTextLenght");
        }
    }

    public bool MultiLine {
        get => _multiLine;
        set {
            if (IsDisposed) { return; }
            if (!_function.MultilinePossible()) { value = false; }

            if (_multiLine == value) { return; }

            _ = Database?.ChangeData(DatabaseDataType.MultiLine, this, null, _multiLine.ToPlusMinus(), value.ToPlusMinus(), Generic.UserName, DateTime.UtcNow, string.Empty, string.Empty);
            Invalidate_ColumAndContent();
            OnPropertyChanged("MultiLine");
        }
    }

    public ReadOnlyCollection<string> PermissionGroupsChangeCell {
        get => new(_permissionGroupsChangeCell);
        set {
            if (IsDisposed) { return; }
            if (!_permissionGroupsChangeCell.IsDifferentTo(value)) { return; }

            _ = Database?.ChangeData(DatabaseDataType.PermissionGroupsChangeCell, this, null, _permissionGroupsChangeCell.JoinWithCr(), value.JoinWithCr(), Generic.UserName, DateTime.UtcNow, string.Empty, string.Empty);
            OnPropertyChanged("PermissionGroupsChangeCell");
        }
    }

    public string RegexCheck {
        get => _regexCheck;
        set {
            if (IsDisposed) { return; }
            if (_regexCheck == value) { return; }

            _ = Database?.ChangeData(DatabaseDataType.RegexCheck, this, null, _regexCheck, value, Generic.UserName, DateTime.UtcNow, string.Empty, string.Empty);
            OnPropertyChanged("RegexCheck");
        }
    }

    public string RendererSettings {
        get => _rendererSettings;
        set {
            if (IsDisposed) { return; }
            if (_rendererSettings == value) { return; }

            _ = Database?.ChangeData(DatabaseDataType.RendererSettings, this, null, _rendererSettings, value, Generic.UserName, DateTime.UtcNow, string.Empty, string.Empty);
            OnPropertyChanged("RendererSettings");
        }
    }

    public int RoundAfterEdit {
        get => _roundAfterEdit;
        set {
            if (IsDisposed) { return; }
            if (_roundAfterEdit == value) { return; }

            _ = Database?.ChangeData(DatabaseDataType.RoundAfterEdit, this, null, _roundAfterEdit.ToString(), value.ToString(), Generic.UserName, DateTime.UtcNow, string.Empty, string.Empty);
            OnPropertyChanged("RoundAfterEdit");
        }
    }

    public ScriptType ScriptType {
        get => _scriptType;
        set {
            if (IsDisposed) { return; }
            if (_scriptType == value) { return; }

            _ = Database?.ChangeData(DatabaseDataType.ScriptType, this, null, ((int)_scriptType).ToString(), ((int)value).ToString(), Generic.UserName, DateTime.UtcNow, string.Empty, string.Empty);
            OnPropertyChanged("ScriptType");
        }
    }

    public bool ShowUndo {
        get => _showUndo;
        set {
            if (IsDisposed) { return; }
            if (_showUndo == value) { return; }

            _ = Database?.ChangeData(DatabaseDataType.ShowUndo, this, null, _showUndo.ToPlusMinus(), value.ToPlusMinus(), Generic.UserName, DateTime.UtcNow, string.Empty, string.Empty);
            OnPropertyChanged("ShowUndo");
        }
    }

    public bool ShowValuesOfOtherCellsInDropdown {
        get => _showValuesOfOtherCellsInDropdown;
        set {
            if (IsDisposed) { return; }
            if (_showValuesOfOtherCellsInDropdown == value) { return; }

            _ = Database?.ChangeData(DatabaseDataType.ShowValuesOfOtherCellsInDropdown, this, null, _showValuesOfOtherCellsInDropdown.ToPlusMinus(), value.ToPlusMinus(), Generic.UserName, DateTime.UtcNow, string.Empty, string.Empty);
            OnPropertyChanged("ShowValuesOfOtherCellsInDropdown");
        }
    }

    public SortierTyp SortType {
        get => _sortType;
        set {
            if (IsDisposed) { return; }
            if (_sortType == value) { return; }

            _ = Database?.ChangeData(DatabaseDataType.SortType, this, null, ((int)_sortType).ToString(), ((int)value).ToString(), Generic.UserName, DateTime.UtcNow, string.Empty, string.Empty);
            OnPropertyChanged("SortType");
        }
    }

    public bool SpellCheckingEnabled {
        get => _spellCheckingEnabled;
        set {
            if (IsDisposed) { return; }
            if (_spellCheckingEnabled == value) { return; }

            _ = Database?.ChangeData(DatabaseDataType.SpellCheckingEnabled, this, null, _spellCheckingEnabled.ToPlusMinus(), value.ToPlusMinus(), Generic.UserName, DateTime.UtcNow, string.Empty, string.Empty);
            OnPropertyChanged("SpellCheckingEnabled");
        }
    }

    public bool TextFormatingAllowed {
        get => _textFormatingAllowed;
        set {
            if (IsDisposed) { return; }
            if (_textFormatingAllowed == value) { return; }

            _ = Database?.ChangeData(DatabaseDataType.TextFormatingAllowed, this, null, _textFormatingAllowed.ToPlusMinus(), value.ToPlusMinus(), Generic.UserName, DateTime.UtcNow, string.Empty, string.Empty);
            OnPropertyChanged("TextFormatingAllowed");
        }
    }

    #endregion

    #region Methods

    public static bool IsValidColumnName(string name) {
        if (string.IsNullOrWhiteSpace(name)) { return false; }

        if (!name.ContainsOnlyChars(AllowedCharsVariableName)) { return false; }

        if (!Char_AZ.Contains(name.Substring(0, 1).ToUpperInvariant())) { return false; }
        if (name.Length > 128) { return false; }

        if (name.ToUpperInvariant() == "USER") { return false; } // SQL System-Name
        if (name.ToUpperInvariant() == "COMMENT") { return false; } // SQL System-Name
        if (name.ToUpperInvariant() == "TABLE_NAME") { return false; } // SQL System-Name
        if (name.ToUpperInvariant() == "COLUMN_NAME") { return false; } // SQL System-Name
        if (name.ToUpperInvariant() == "OWNER") { return false; } // SQL System-Name
        if (name.ToUpperInvariant() == "DATA_TYPE") { return false; } // SQL System-Name
        if (name.ToUpperInvariant() == "DATA_LENGTH") { return false; } // SQL System-Name
        if (name.ToUpperInvariant() == "OFFLINE") { return false; } // SQL System-Name
        if (name.ToUpperInvariant() == "ONLINE") { return false; } // SQL System-Name

        return name.ToUpperInvariant() != TmpNewDummy; // BlueDatabase name bei neuen Spalten
    }

    public static EditTypeTable UserEditDialogTypeInTable(ColumnItem? column, bool preverDropDown) => column is not { IsDisposed: false }
            ? EditTypeTable.None
            : UserEditDialogTypeInTable(column.Function, preverDropDown && column.EditableWithDropdown, column.EditableWithTextInput, column.MultiLine);

    public static EditTypeTable UserEditDialogTypeInTable(ColumnFunction function, bool doDropDown, bool keybordInputAllowed, bool isMultiline) {
        if (!doDropDown && !keybordInputAllowed) { return EditTypeTable.None; }

        switch (function) {
            case ColumnFunction.Werte_aus_anderer_Datenbank_als_DropDownItems:
                return EditTypeTable.Dropdown_Single;

            //case ColumnFunction.FarbeInteger:
            //    if (doDropDown) { return EditTypeTable.Dropdown_Single; }
            //    return EditTypeTable.Farb_Auswahl_Dialog;

            //case ColumnFunction.Schrift:
            //    if (doDropDown) { return EditTypeTable.Dropdown_Single; }
            //    return EditTypeTable.Font_AuswahlDialog;

            case ColumnFunction.Zeile:
            case ColumnFunction.Split_Medium:
            case ColumnFunction.Split_Large:
            case ColumnFunction.Split_Name:
            case ColumnFunction.First:
            case ColumnFunction.Schl�sselspalte:
                return EditTypeTable.None;

            default:
                if (function.TextboxEditPossible()) {
                    if (!doDropDown) {
                        return EditTypeTable.Textfeld;
                    }

                    if (isMultiline) {
                        return EditTypeTable.Dropdown_Single;
                    }

                    if (keybordInputAllowed) {
                        return EditTypeTable.Textfeld_mit_Auswahlknopf;
                    }

                    return EditTypeTable.Dropdown_Single;
                }

                Develop.DebugPrint(function);
                return EditTypeTable.None;
        }
    }

    public void AddSystemInfo(string type, string user) {
        var t = ColumnSystemInfo.SplitAndCutByCrToList();
        t.Add(type + ": " + user);

        //t.TagSet(type, user);
        ColumnSystemInfo = t.SortedDistinctList().JoinWithCr();
    }

    public void AddSystemInfo(string type, Database sourcedatabase, string user) => AddSystemInfo(type, sourcedatabase.Caption + " -> " + user);

    public string AutoCorrect(string value, bool exitifLinkedFormat) {
        if (IsDisposed || Database is not { IsDisposed: false } db) { return value; }

        if (IsSystemColumn() && this != db.Column.SysChapter) { return value; }
        //if (Function == ColumnFunction.Virtelle_Spalte) { return value; }

        if (exitifLinkedFormat) {
            if (_function is ColumnFunction.Verkn�pfung_zu_anderer_Datenbank
                          or ColumnFunction.Werte_aus_anderer_Datenbank_als_DropDownItems) { return value; }
        }

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

    public bool AutoFilterSymbolPossible() => FilterOptions.HasFlag(FilterOptions.Enabled) && Function.Autofilter_m�glich();

    public int CalculatePreveredMaxCellLenght(double prozentZuschlag) {
        if (IsDisposed || Database is not { IsDisposed: false }) { return 0; }

        //if (Format == DataFormat.Verkn�pfung_zu_anderer_Datenbank) { return 35; }
        //if (Format == DataFormat.Werte_aus_anderer_Datenbank_als_DropDownItems) { return 15; }
        var m = 0;

        foreach (var thisRow in Database.Row) {
            var t = thisRow.CellGetString(this);
            m = Math.Max(m, t.StringtoUtf8().Length);
        }

        if (m <= 0) { return 8; }
        if (m == 1) { return 1; }
        var erg = Math.Max((int)(m * prozentZuschlag) + 1, _maxTextLenght);
        return Math.Min(erg, 3999);
    }

    public int CalculatePreveredMaxTextLenght(double prozentZuschlag) {
        if (IsDisposed || Database is not { IsDisposed: false }) { return 0; }

        ////if (Format == DataFormat.Verkn�pfung_zu_anderer_Datenbank) { return 35; }
        ////if (Format == DataFormat.Werte_aus_anderer_Datenbank_als_DropDownItems) { return 15; }
        var m = 0;

        var l = Contents();

        foreach (var thiss in l) {
            m = Math.Max(m, thiss.Length);
        }

        if (m <= 0) { return 8; }
        if (m == 1) { return 1; }
        return Math.Min((int)(m * prozentZuschlag) + 1, 1000);
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
        if (!string.IsNullOrEmpty(Database.EditableErrorReason(Database, EditableErrorReasonType.EditAcut))) { return; }

        if (source.Database != null) { source.Repair(); }

        if (nameAndKeyToo) { KeyName = source.KeyName; }

        Caption = source.Caption;
        CaptionBitmapCode = source.CaptionBitmapCode;
        Function = source.Function;
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
        ColumnNameOfLinkedDatabase = source.ColumnNameOfLinkedDatabase;
        Align = source.Align;
        SortType = source.SortType;
        DropDownItems = source.DropDownItems;
        LinkedCellFilter = source.LinkedCellFilter;
        AfterEditAutoReplace = source.AfterEditAutoReplace;
        this.GetStyleFrom(source); // regex, Allowed Chars, etc.
        ScriptType = source.ScriptType;
        ShowUndo = source.ShowUndo;
        CaptionGroup1 = source.CaptionGroup1;
        CaptionGroup2 = source.CaptionGroup2;
        CaptionGroup3 = source.CaptionGroup3;
        LinkedDatabaseTableName = source.LinkedDatabaseTableName;
    }

    public bool ColumNameAllowed(string nameToTest) {
        if (!IsValidColumnName(nameToTest)) { return false; }

        if (nameToTest.Equals(_keyName, StringComparison.OrdinalIgnoreCase)) { return true; }
        if (Database?.Column[nameToTest] != null) { return false; }

        return true;
    }

    public List<string> Contents() => Contents(Database?.Row.ToList());

    public List<string> Contents(FilterCollection fc, List<RowItem>? pinned) {
        if (IsDisposed || Database is not { IsDisposed: false }) { return []; }
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
                if (_function == ColumnFunction.Virtuelle_Spalte) { _ = thisRowItem.CheckRow(); }

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

    public void Dispose() {
        // �ndern Sie diesen Code nicht. F�gen Sie Bereinigungscode in der Methode "Dispose(bool disposing)" ein.
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    public string ErrorReason() {
        if (IsDisposed || Database is not { IsDisposed: false } db) { return "Datenbank verworfen"; }
        if (string.IsNullOrEmpty(_keyName)) { return "Der Spaltenname ist nicht definiert."; }

        if (!IsValidColumnName(_keyName)) { return "Der Spaltenname ist ung�ltig."; }

        if (_maxCellLenght < _maxTextLenght) { return "Zellengr��e zu klein!"; }
        if (_maxCellLenght < 1) { return "Zellengr��e zu klein!"; }
        if (_maxCellLenght > 4000) { return "Zellengr��e zu gro�!"; }
        if (_maxTextLenght > 4000) { return "Maximall�nge zu gro�!"; }

        if (Database.Column.Any(thisColumn => thisColumn != this && thisColumn != null && string.Equals(_keyName, thisColumn._keyName, StringComparison.OrdinalIgnoreCase))) {
            return "Spalten-Name bereits vorhanden.";
        }

        if (string.IsNullOrEmpty(_caption)) { return "Spalten Beschriftung fehlt."; }

        if (((int)_function).ToString() == _function.ToString()) { return "Format fehlerhaft."; }

        if (_function.NeedTargetDatabase()) {
            if (LinkedDatabase is not { IsDisposed: false } db2) { return "Verkn�pfte Datenbank fehlt oder existiert nicht."; }
            if (db == db2) { return "Zirkelbezug mit verkn�pfter Datenbank."; }
            var c = db2.Column[_columnNameOfLinkedDatabase];
            if (c == null) { return "Die verkn�pfte Schl�sselspalte existiert nicht."; }
            if (_linkedCellFilter.Count == 0) {
                if (Function != ColumnFunction.Werte_aus_anderer_Datenbank_als_DropDownItems) {
                    return "Keine Filter f�r verkn�pfte Datenbank definiert.";
                }
            }
        } else {
            if (!string.IsNullOrEmpty(_columnNameOfLinkedDatabase)) { return "Nur verlinkte Zellen k�nnen Daten �ber verlinkte Zellen enthalten."; }
        }

        if (!_function.Autofilter_m�glich()) {
            if (_filterOptions != FilterOptions.None) { return "Bei diesem Format keine Filterung erlaubt."; }
            if (!_ignoreAtRowFilter) { return "Dieses Format muss bei Zeilenfiltern ignoriert werden."; }
        }
        if (_filterOptions != FilterOptions.None && !_filterOptions.HasFlag(FilterOptions.Enabled)) { return "Filter Kombination nicht m�glich."; }
        if (_filterOptions != FilterOptions.Enabled_OnlyAndAllowed && _filterOptions.HasFlag(FilterOptions.OnlyAndAllowed)) { return "Filter Kombination nicht m�glich."; }
        if (_filterOptions != FilterOptions.Enabled_OnlyOrAllowed && _filterOptions.HasFlag(FilterOptions.OnlyOrAllowed)) { return "Filter Kombination nicht m�glich."; }
        if (_filterOptions.HasFlag(FilterOptions.OnlyAndAllowed) || _filterOptions.HasFlag(FilterOptions.OnlyOrAllowed)) {
            if (!_multiLine) {
                return "Dieser Filter kann nur bei mehrzeiligen Spalten benutzt werden.";
            }
        }
        switch (_function) {
            case ColumnFunction.Zeile:
                if (_scriptType is not ScriptType.Row) { return "Zeilenspalten m�ssen im Skript als Zeile vorhanden sein."; }
                if (!_ignoreAtRowFilter) { return "Dieses Format muss bei Zeilenfiltern ignoriert werden."; }
                break;

            case ColumnFunction.Virtuelle_Spalte:
                if (_showUndo) { return "Virtuelle Spalten unterst�tzen kein Undo."; }
                if (_fixedColumnWidth < 16) { return "Virtuelle Spalten brauchen eine feste Spaltenbreite."; }
                if (_scriptType is not ScriptType.Bool and not ScriptType.String and not ScriptType.Numeral and not ScriptType.List) {
                    return "Virtuelle Spalten m�ssen im Skript gesetzt werden und deswegen vorhanden sein.";
                }
                if (!_ignoreAtRowFilter) { return "Dieses Format muss bei Zeilenfiltern ignoriert werden."; }
                //if (!db.CanDoPrepareFormulaCheckScript()) { return "F�r virtuelle Spalten zuerst das Skript 'Formular Vorbereiten' erstellen."; }

                break;

            case ColumnFunction.RelationText:
                if (!_multiLine) { return "Bei dieser Funktion muss mehrzeilig ausgew�hlt werden."; }
                //if (_keyColumnKey > -1) { return "Diese Format darf keine Verkn�pfung zu einer Schl�sselspalte haben."; }
                if (IsFirst()) { return "Diese Funktion ist bei der ersten Spalte nicht erlaubt. (Ansicht: Alle Spalten)"; }
                //if (!string.IsNullOrEmpty(_cellInitValue)) { return "Diese Format kann keinen Initial-Text haben."; }
                //if (!string.IsNullOrEmpty(_vorschlagsColumn)) { return "Diese Format kann keine Vorschlags-Spalte haben."; }
                break;

            case ColumnFunction.Split_Name:
            case ColumnFunction.Split_Large:
            case ColumnFunction.Split_Medium:
                if (_scriptType is not ScriptType.String_Readonly and not ScriptType.Bool_Readonly and not ScriptType.Nicht_vorhanden and not ScriptType.List_Readonly) {
                    return "Diese Spalte darf im Skript nur als ReadOnly vorhanden sein.";
                }
                if (!string.IsNullOrEmpty(_autoFilterJoker)) { return "Der Autofilter-Joker darf bei dieser Spalte nicht gesetzt sein."; }
                if (_filterOptions.HasFlag(FilterOptions.ExtendedFilterEnabled)) { return "Erweiterte Filter sind bei dieser Spalte nicht erlaubt."; }
                if (!_filterOptions.HasFlag(FilterOptions.TextFilterEnabled)) { return "Texteingabe-Filter sind bei dieser Spalte n�tig."; }
                if (!_ignoreAtRowFilter) { return "Diese Spalte muss bei Zeilenfiltern ignoriert werden."; }

                if (!_filterOptions.HasFlag(FilterOptions.Enabled)) { return "Auto-Filter m�ssen bei dieser Spalte erlaubt sein."; }
                if (_showUndo) { return "Undo bei dieser Spalte nicht erlaubt."; }
                break;

            case ColumnFunction.First:
            case ColumnFunction.Schl�sselspalte:
                if (_scriptType is not ScriptType.String_Readonly and not ScriptType.Bool_Readonly and not ScriptType.Nicht_vorhanden and not ScriptType.List_Readonly) {
                    return "Diese Spalte darf im Skript nur als ReadOnly vorhanden sein.";
                }
                break;

            case ColumnFunction.Verkn�pfung_zu_anderer_Datenbank:
                if (_scriptType is not ScriptType.Nicht_vorhanden) {
                    return "Verkn�pfung_zu_anderer_Datenbank kann im Skript nicht verwendet werden. ImportLinked im Skript benutzen.";
                }
                break;

            case ColumnFunction.Werte_aus_anderer_Datenbank_als_DropDownItems:
                //Develop.DebugPrint("Values_f�r_LinkedCellDropdown Verwendung bei:" + Database.Filename); //TODO: 29.07.2021 Values_f�r_LinkedCellDropdown Format entfernen
                //if (!string.IsNullOrEmpty(_cellInitValue)) { return "Dieses Format kann keinen Initial-Text haben."; }
                //if (KeyColumnKey > -1) { return "Dieses Format darf keine Verkn�pfung zu einer Schl�sselspalte haben."; }
                //if (_vorschlagsColumn > 0) { return "Dieses Format kann keine Vorschlags-Spalte haben."; }
                //if (MaxTextLenght < 15) { return "Maximall�nge bei diesem Format mindestens 15!"; }

                break;
        }

        if (_multiLine) {
            if (!_function.MultilinePossible()) { return "Format unterst�tzt keine mehrzeiligen Texte."; }
            if (_roundAfterEdit != -1) { return "Runden nur bei einzeiligen Texten m�glich"; }
        } else {
            if (_afterEditQuickSortRemoveDouble) { return "Sortierung kann nur bei mehrzeiligen Feldern erfolgen."; }
        }

        if (_spellCheckingEnabled && !_function.SpellCheckingPossible()) { return "Rechtschreibpr�fung bei diesem Format nicht m�glich."; }
        if (_editAllowedDespiteLock && !_editableWithTextInput && !_editableWithDropdown) { return "Wenn die Zeilensperre ignoriert werden soll, muss eine Bearbeitungsmethode definiert sein."; }
        var tmpEditDialog = UserEditDialogTypeInTable(_function, false, true, _multiLine);
        if (_editableWithTextInput) {
            if (tmpEditDialog == EditTypeTable.Dropdown_Single) { return "Format unterst�tzt nur Dropdown-Men�."; }
            if (tmpEditDialog == EditTypeTable.None) { return "Format unterst�tzt keine Standard-Bearbeitung."; }
        }

        if (_editableWithDropdown) {
            //if (_SpellCheckingEnabled) { return "Entweder Dropdownmen� oder Rechtschreibpr�fung."; }
            if (tmpEditDialog == EditTypeTable.None) { return "Format unterst�tzt keine Auswahlmen�-Bearbeitung."; }
        }
        if (!_editableWithDropdown && !_editableWithTextInput) {
            if (_permissionGroupsChangeCell.Count > 0) { return "Bearbeitungsberechtigungen entfernen, wenn keine Bearbeitung erlaubt ist."; }
        }

        foreach (var thisS in _permissionGroupsChangeCell) {
            if (thisS.Contains("|")) { return "Unerlaubtes Zeichen bei den Gruppen, die eine Zelle bearbeiten d�rfen."; }
            if (string.Equals(thisS, Administrator, StringComparison.OrdinalIgnoreCase)) { return "'#Administrator' bei den Bearbeitern entfernen."; }
        }
        if (_editableWithDropdown || tmpEditDialog == EditTypeTable.Dropdown_Single) {
            if (_function != ColumnFunction.Werte_aus_anderer_Datenbank_als_DropDownItems) {
                if (!_showValuesOfOtherCellsInDropdown && _dropDownItems.Count == 0) { return "Keine Dropdown-Items vorhanden bzw. Alles hinzuf�gen nicht angew�hlt."; }
            }
        } else {
            if (_showValuesOfOtherCellsInDropdown) { return "Dropdownmenu nicht ausgew�hlt, 'alles hinzuf�gen' pr�fen."; }
            if (_dropdownDeselectAllAllowed) { return "Dropdownmenu nicht ausgew�hlt, 'alles abw�hlen' pr�fen."; }
            if (_dropDownItems.Count > 0) { return "Dropdownmenu nicht ausgew�hlt, Dropdown-Items vorhanden."; }
        }
        if (_showValuesOfOtherCellsInDropdown && !_function.DropdownItemsOfOtherCellsAllowed()) { return "'Dropdownmenu alles hinzuf�gen' bei diesem Format nicht erlaubt."; }
        if (_dropdownDeselectAllAllowed && !_function.DropdownUnselectAllAllowed()) { return "'Dropdownmenu alles abw�hlen' bei diesem Format nicht erlaubt."; }
        if (_dropDownItems.Count > 0 && !_function.DropdownItemsAllowed()) { return "Manuelle 'Dropdow-Items' bei diesem Format nicht erlaubt."; }

        if (_roundAfterEdit > 5) { return "Beim Runden maximal 5 Nachkommastellen m�glich"; }
        if (_filterOptions == FilterOptions.None) {
            if (!string.IsNullOrEmpty(_autoFilterJoker)) { return "Wenn kein Autofilter erlaubt ist, immer anzuzeigende Werte entfernen"; }
        }

        if (_function != ColumnFunction.Zeile && _scriptType == ScriptType.Row) {
            return "Der Skripttyp 'Zeile' kann nur bei der Spaltenfuntion 'Zeile' gew�hlt werden.";
        }

        if (_function is ColumnFunction.Zeile or ColumnFunction.Werte_aus_anderer_Datenbank_als_DropDownItems) {
            if (_roundAfterEdit != -1 || _afterEditAutoReplace.Count > 0 || _afterEditAutoCorrect || _afterEditDoUCase || _afterEditQuickSortRemoveDouble || !string.IsNullOrEmpty(_allowedChars)) {
                return "Dieses Format unterst�tzt keine automatischen Bearbeitungen wie Runden, Ersetzungen, Fehlerbereinigung, immer Gro�buchstaben, Erlaubte Zeichen oder Sortierung.";
            }
        }

        return string.Empty;
    }

    public List<string> GetUcaseNamesSortedByLenght() {
        if (IsDisposed || Database is not { IsDisposed: false } db) { return []; }

        if (UcaseNamesSortedByLenght != null) { return UcaseNamesSortedByLenght; }
        var tmp = Contents(db.Row.ToList());
        tmp.Sort((s1, s2) => s2.Length.CompareTo(s1.Length));
        UcaseNamesSortedByLenght = tmp;
        return UcaseNamesSortedByLenght;
    }

    public void Invalidate_ColumAndContent() {
        if (IsDisposed || Database is not { IsDisposed: false }) { return; }
        Invalidate_LinkedDatabase();
        Database.OnViewChanged();
    }

    //}
    ///// <summary>
    ///// F�llt die Ersetzungen mittels eines �bergebenen Enums aus.
    ///// </summary>
    ///// <param name="t">Beispiel: GetType(enDesign)</param>
    ///// <param name="ZumDropdownHinzuAb">Erster Wert der Enumeration, der Hinzugef�gt werden soll. Inklusive deses Wertes</param>
    ///// <param name="ZumDropdownHinzuBis">Letzter Wert der Enumeration, der nicht mehr hinzugef�gt wird, also exklusives diese Wertes</param>
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
    /// Der Invalidate, der am meisten invalidiert: Alle tempor�ren Variablen und auch jede Zell-Gr��e der Spalte.
    /// </summary>
    public bool IsFirst() => Database?.Column.First() == this;

    public bool IsSystemColumn() =>
        _keyName.ToUpperInvariant() is "SYS_CORRECT" or
            "SYS_CHANGER" or
            "SYS_CREATOR" or
            "SYS_CHAPTER" or
            "SYS_DATECREATED" or
            "SYS_DATECHANGED" or
            "SYS_LOCKED" or
            "SYS_ROWSTATE";

    public void OnPropertyChanged(string propertyname) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyname));

    public string ReadableText() {
        if (IsDisposed || Database is not { IsDisposed: false }) { return string.Empty; }

        var ret = _caption;
        if (Database.Column.Any(thisColumnItem => thisColumnItem != null && thisColumnItem != this && string.Equals(thisColumnItem.Caption, _caption, StringComparison.OrdinalIgnoreCase))) {
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
        if (!string.IsNullOrEmpty(Database.EditableErrorReason(Database, EditableErrorReasonType.EditAcut))) { return; }

        if (IsDisposed || Database is not { IsDisposed: false }) { return; }
        if (IsDisposed) { return; }

        if (_function == ColumnFunction.Zeile) { ScriptType = ScriptType.Row; }

        if (_function.ToString() == ((int)_function).ToString()) {
            this.GetStyleFrom(ColumnFormatHolder.Text);
        }

        if (ScriptType == ScriptType.undefiniert) {
            if (MultiLine) {
                ScriptType = ScriptType.List;
            } else if (Function is ColumnFunction.Normal) {
                if (SortType is SortierTyp.ZahlenwertFloat or SortierTyp.ZahlenwertInt) {
                    ScriptType = ScriptType.Numeral;
                }
                ScriptType = ScriptType.String;
            } else {
                ScriptType = ScriptType.Nicht_vorhanden;
            }
        }

        if (ScriptType == (ScriptType)6) {
            // vorher Datum
            ScriptType = ScriptType.String;
        }

        if (_function is ColumnFunction.Verkn�pfung_zu_anderer_Datenbank) {

            #region Aus Dateinamen den Tablename extrahieren

            if (!_linkedDatabaseTableName.Contains("|") && _linkedDatabaseTableName.IsFormat(FormatHolder.FilepathAndName)) {
                _linkedDatabaseTableName = _linkedDatabaseTableName.ToUpperInvariant().TrimEnd(".MDB").TrimEnd(".BDB").TrimEnd(".MBDB").TrimEnd(".CBDB");
                LinkedDatabaseTableName = MakeValidTableName(_linkedDatabaseTableName);
            }

            #endregion

            #region Aus Connection-info den Tablename extrahieren

            if (_linkedDatabaseTableName.Contains("|")) {
                var l = _linkedDatabaseTableName.Split('|');
                if (IsValidTableName(l[0])) { LinkedDatabaseTableName = l[0]; }
            }

            #endregion

            var c = LinkedDatabase?.Column[_columnNameOfLinkedDatabase];
            if (c is { IsDisposed: false }) {
                this.GetStyleFrom((IInputFormat)c);
                ScriptType = ScriptType.Nicht_vorhanden;
                DoOpticalTranslation = c.DoOpticalTranslation;

                //AfterEditQuickSortRemoveDouble = c.AfterEditQuickSortRemoveDouble;
                //Align = c.Align;

                //DropdownAllesAbw�hlenErlaubt = c.DropdownAllesAbw�hlenErlaubt;
                //DropdownBearbeitungErlaubt = c.DropdownBearbeitungErlaubt;
                //DropDownItems = c.DropDownItems;
                //DropdownWerteAndererZellenAnzeigen = c.DropdownWerteAndererZellenAnzeigen;
                //Function = c.Function;
                //SortType = c.SortType;
                //TextBearbeitungErlaubt = c.TextBearbeitungErlaubt;
                if (Function == ColumnFunction.Verkn�pfung_zu_anderer_Datenbank) {
                    MaxTextLenght = c.MaxTextLenght;
                    MaxCellLenght = c.MaxCellLenght;
                }
            }
        }

        if (MaxCellLenght < MaxTextLenght) { MaxCellLenght = MaxTextLenght; }

        PermissionGroupsChangeCell = RepairUserGroups(PermissionGroupsChangeCell).AsReadOnly();

        ResetSystemToDefault(false);
        CheckIfIAmAKeyColumn();

        SystemInfoReset(false);
    }

    public void ResetSystemToDefault(bool setOpticalToo) {
        if (!IsSystemColumn()) { return; }
        // ACHTUNG: Die setOpticalToo Befehle OHNE _, die m�ssen geloggt werden.
        if (setOpticalToo) {
            LineStyleLeft = ColumnLineStyle.D�nn;
            LineStyleRight = ColumnLineStyle.Ohne;
            ForeColor = Color.FromArgb(0, 0, 0);
            //CaptionBitmapCode = null;
        }
        switch (_keyName.ToUpperInvariant()) {
            case "SYS_CREATOR":
                _function = ColumnFunction.Normal;
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
                _function = ColumnFunction.Normal;
                _ignoreAtRowFilter = true;
                _spellCheckingEnabled = false;
                _editableWithTextInput = false;
                _editableWithDropdown = false;
                _showUndo = false;
                _scriptType = ScriptType.Nicht_vorhanden;  // um Script-Pr�fung zu reduzieren
                _permissionGroupsChangeCell.Clear();
                _maxTextLenght = 20;
                _maxCellLenght = 20;
                if (setOpticalToo) {
                    Caption = "�nderer";
                    ForeColor = Color.FromArgb(0, 128, 0);
                    BackColor = Color.FromArgb(185, 255, 185);
                }
                break;

            case "SYS_CHAPTER":
                if (_function is not ColumnFunction.Normal
                             and not ColumnFunction.Schl�sselspalte
                             and not ColumnFunction.Split_Medium
                             and not ColumnFunction.Split_Large
                             and not ColumnFunction.Split_Name
                             and not ColumnFunction.First 
                             and not ColumnFunction.Verkn�pfung_zu_anderer_Datenbank) {
                    _function = ColumnFunction.Normal;
                }

                if (_function == ColumnFunction.Normal) {
                    _afterEditAutoCorrect = true; // Verhindert \r am Ende und somit anzeigefehler
                } else {
                    _afterEditAutoCorrect = false;
                }

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

                this.GetStyleFrom(FormatHolder.DateTime); // Ja, FormatHolder, da wird der Script-Type nicht ver�ndert
                MaxCellLenght = MaxTextLenght;
                if (setOpticalToo) {
                    Caption = "Erstell-Datum";
                    ForeColor = Color.FromArgb(0, 0, 128);
                    BackColor = Color.FromArgb(185, 185, 255);
                    LineStyleLeft = ColumnLineStyle.Dick;
                }

                break;

            case "SYS_ROWSTATE":
                _spellCheckingEnabled = false;
                _ignoreAtRowFilter = true;

                this.GetStyleFrom(FormatHolder.DateTime); // Ja, FormatHolder, da wird der Script-Type nicht ver�ndert
                MaxCellLenght = MaxTextLenght;
                if (setOpticalToo) {
                    Caption = "Zeilen-Status";
                    ForeColor = Color.FromArgb(128, 0, 0);
                    BackColor = Color.FromArgb(255, 185, 185);
                    //LineLeft = ColumnLineStyle.Dick;
                }
                //_scriptType = ScriptType.Nicht_vorhanden;  // um Script-Pr�fung zu reduzieren

                break;

            case "SYS_DATECHANGED":
                _spellCheckingEnabled = false;
                _ignoreAtRowFilter = true;
                _showUndo = false;

                this.GetStyleFrom(FormatHolder.DateTimeWithMilliSeconds); // Ja, FormatHolder, da wird der Script-Type nicht ver�ndert
                MaxCellLenght = MaxTextLenght;
                _editableWithTextInput = false;
                _spellCheckingEnabled = false;
                _editableWithDropdown = false;
                _scriptType = ScriptType.Nicht_vorhanden; // um Script-Pr�fung zu reduzieren
                _permissionGroupsChangeCell.Clear();

                if (setOpticalToo) {
                    Caption = "�nder-Datum";
                    ForeColor = Color.FromArgb(0, 128, 0);
                    BackColor = Color.FromArgb(185, 255, 185);
                    LineStyleLeft = ColumnLineStyle.Dick;
                }
                break;

            case "SYS_CORRECT":
                _caption = "Fehlerfrei";
                _spellCheckingEnabled = false;
                _function = ColumnFunction.Normal;
                //_AutoFilterErweitertErlaubt = false;
                _autoFilterJoker = string.Empty;
                //_AutofilterTextFilterErlaubt = false;
                _ignoreAtRowFilter = true;
                _filterOptions = FilterOptions.Enabled;
                _scriptType = ScriptType.Nicht_vorhanden; // Wichtig! Weil eine Routine ErrorCol !=0 den Wert setzt und evtl. eine Endlosschleife ausl�st
                _align = AlignmentHorizontal.Zentriert;
                _dropDownItems.Clear();
                _linkedCellFilter.Clear();
                _permissionGroupsChangeCell.Clear();
                _editableWithTextInput = false;
                _editableWithDropdown = false;
                _maxTextLenght = 1;
                _maxCellLenght = 1;
                _showValuesOfOtherCellsInDropdown = false;
                _adminInfo = "Diese Spalte kann nur �ber ein Skript bearbeitet<br>werden, mit dem Befehl 'SetError'";

                if (setOpticalToo) {
                    ForeColor = Color.FromArgb(128, 0, 0);
                    BackColor = Color.FromArgb(255, 185, 185);
                    LineStyleLeft = ColumnLineStyle.Dick;
                }
                break;

            case "SYS_LOCKED":
                _spellCheckingEnabled = false;
                _function = ColumnFunction.Normal;
                _scriptType = ScriptType.Bool;
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
            //    _caption = "veraltet und kann gel�scht werden: Zeilenstand";
            //    _identifierx = string.Empty;
            //    break;

            //case "System: ID":
            //    _name = "SYS_ID";
            //    _caption = "veraltet und kann gel�scht werden: Zeilen-ID";
            //    _identifierx = string.Empty;
            //    break;

            //case "System: Last Used Layout":
            //    _name = "SYS_Layout";
            //    _caption = "veraltet und kann gel�scht werden:  Letztes Layout";
            //    _identifierx = string.Empty;
            //    break;

            default:
                Develop.DebugPrint("Unbekannte Kennung: " + _keyName);
                break;
        }
    }

    public void Statistik(List<RowItem> rows, bool ignoreMultiLine) {
        if (IsDisposed || Database is not { IsDisposed: false }) { return; }

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
        if (IsDisposed || Database is not { IsDisposed: false } db) { return null; }

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
        if (IsDisposed || Database is not { IsDisposed: false } db) { return QuickImage.Get(ImageCode.Warnung); }

        if (this == db.Column.SysRowChanger) { return QuickImage.Get(ImageCode.Person); }

        if (this == db.Column.SysRowChanger) { return QuickImage.Get(ImageCode.Person); }
        if (this == db.Column.SysRowCreator) { return QuickImage.Get(ImageCode.Person); }
        if (this == db.Column.SysRowCreateDate) { return QuickImage.Get(ImageCode.Uhr); }
        if (this == db.Column.SysRowChangeDate) { return QuickImage.Get(ImageCode.Uhr); }

        if (this == db.Column.SysLocked) { return QuickImage.Get(ImageCode.Schloss); }

        if (this == db.Column.SysCorrect) { return QuickImage.Get(ImageCode.Warnung); }

        if (_function == ColumnFunction.First) { return QuickImage.Get(ImageCode.Stern, 16); }
        if (_function == ColumnFunction.Schl�sselspalte) { return QuickImage.Get(ImageCode.Schl�ssel, 16); }

        if (_function is ColumnFunction.Split_Medium or ColumnFunction.Split_Name or ColumnFunction.Split_Large) { return QuickImage.Get(ImageCode.Diskette, 16); }

        if (_function == ColumnFunction.RelationText) { return QuickImage.Get(ImageCode.Herz, 16); }

        if (_function == ColumnFunction.Verkn�pfung_zu_anderer_Datenbank) { return QuickImage.Get(ImageCode.Fernglas, 16); }

        if (_function == ColumnFunction.Virtuelle_Spalte) { return QuickImage.Get(ImageCode.Tabelle, 16); }

        if (_function == ColumnFunction.Zeile) { return QuickImage.Get(ImageCode.Zeile, 16); }

        foreach (var thisFormat in FormatHolder.AllFormats) {
            if (thisFormat.IsFormatIdenticalSoft(this)) { return thisFormat.Image; }
        }

        if (_editableWithDropdown) {
            return QuickImage.Get("Pfeil_Unten_Scrollbar|14|||||0");
        }

        if (_function.TextboxEditPossible()) {
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

    //        case FormatHolder.Url:
    //            SetFormatForUrl();
    //            break;
    public override string ToString() => IsDisposed ? string.Empty : _keyName + " -> " + Caption;

    /// <summary>
    /// CallByFileName Aufrufe werden nicht gepr�ft
    /// </summary>
    /// <returns></returns>
    public bool UsedInScript() {
        if (IsDisposed || Database is not { IsDisposed: false }) { return false; }

        foreach (var thiss in Database.EventScript) {
            if (thiss.Script.ContainsWord(_keyName, RegexOptions.IgnoreCase)) { return true; }
        }

        return false;
    }

    //        case FormatHolder.TextMitFormatierung:
    //            SetFormatForTextMitFormatierung();
    //            break;
    public bool UserEditDialogTypeInFormula(EditTypeFormula editTypeToCheck) {
        switch (_function) {
            case ColumnFunction.Virtuelle_Spalte:
                if (editTypeToCheck == EditTypeFormula.Textfeld) { return true; } // Textfeld immer erlauben auch wenn beide Bearbeitungen nicht erlaubt sind. Einfach der �bersichtlichktei
                return false;

            case ColumnFunction.Normal:
            case ColumnFunction.RelationText:
                if (editTypeToCheck == EditTypeFormula.Textfeld) { return true; } // Textfeld immer erlauben auch wenn beide Bearbeitungen nicht erlaubt sind. Einfach der �bersichtlichktei
                if (_multiLine && editTypeToCheck == EditTypeFormula.Textfeld_mit_Auswahlknopf) { return false; }
                if (_editableWithDropdown && editTypeToCheck == EditTypeFormula.Textfeld_mit_Auswahlknopf) { return true; }
                if (_editableWithDropdown && _showValuesOfOtherCellsInDropdown && editTypeToCheck == EditTypeFormula.SwapListBox) { return true; }
                //if (_MultiLine && _DropdownBearbeitungErlaubt && EditType_To_Check == enEditTypeFormula.Listbox_3_Zeilen) { return true; }
                if (_multiLine && _editableWithDropdown && editTypeToCheck == EditTypeFormula.Listbox) { return true; }
                if (editTypeToCheck == EditTypeFormula.nur_als_Text_anzeigen) { return true; }
                if (!_multiLine && editTypeToCheck == EditTypeFormula.Ja_Nein_Knopf) { return true; }
                return false;

            case ColumnFunction.Verkn�pfung_zu_anderer_Datenbank:
                if (editTypeToCheck == EditTypeFormula.None) { return true; }
                var col = LinkedDatabase?.Column[_columnNameOfLinkedDatabase];
                if (col == null) { return false; }
                return col.UserEditDialogTypeInFormula(editTypeToCheck);

            case ColumnFunction.Werte_aus_anderer_Datenbank_als_DropDownItems:
                if (editTypeToCheck == EditTypeFormula.Textfeld_mit_Auswahlknopf) { return true; }
                return false;

            //case DataFormat.Bit:
            //    if (_MultiLine) { return false; }
            //    if (editType_To_Check == enEditTypeFormula.Ja_Nein_Knopf) {
            //        return !_DropdownWerteAndererZellenAnzeigen && DropDownItems.Count <= 0;
            //    }
            //    if (editType_To_Check == enEditTypeFormula.Textfeld_mit_Auswahlknopf) { return true; }
            //    return false;

            //case DataFormat.Link_To_Filesystem:
            //    if (_multiLine) {
            //        //if (EditType_To_Check == enEditType.Listbox) { return true; }
            //        if (editTypeToCheck == EditTypeFormula.Gallery) { return true; }
            //    } else {
            //        if (editTypeToCheck == EditTypeFormula.EasyPic) { return true; }
            //    }
            //    return false;
            //case DataFormat.Relation:
            //    switch (EditType_To_Check)
            //    {
            //        case enEditTypeFormula.Listbox_1_Zeile:
            //        case enEditTypeFormula.Listbox_3_Zeilen:
            //        case enEditTypeFormula.Listbox_6_Zeilen:
            //            return true;
            //        default:
            //            return false;
            //    }
            //case ColumnFunction.FarbeInteger:
            //    return editTypeToCheck == EditTypeFormula.Farb_Auswahl_Dialog;

            //case ColumnFunction.Schrift:
            //    return editTypeToCheck == EditTypeFormula.Font_AuswahlDialog;

            case ColumnFunction.Zeile:
                //if (EditType_To_Check == enEditTypeFormula.Button) { return true; }
                return false;

            case ColumnFunction.First:
            case ColumnFunction.Split_Medium:
            case ColumnFunction.Split_Name:
            case ColumnFunction.Split_Large:
            case ColumnFunction.Schl�sselspalte:
                if (editTypeToCheck == EditTypeFormula.als_�berschrift_anzeigen) { return true; }
                return false;

            default:
                Develop.DebugPrint(_function);
                return false;
        }
    }

    internal static string MakeValidColumnName(string columnname) => columnname.Trim().ToUpperInvariant().Replace(" ", "_").Replace("__", "_").ReduceToChars(AllowedCharsVariableName);

    internal string EditableErrorReason(EditableErrorReasonType mode, bool checkEditmode) {
        if (IsDisposed || Database is not { IsDisposed: false } db) { return "Die Datenbank wurde verworfen."; }
        if (IsDisposed) { return "Die Spalte wurde verworfen."; }

        var f = db.EditableErrorReason(mode);
        if (!string.IsNullOrEmpty(f)) { return f; }

        if (mode == EditableErrorReasonType.OnlyRead) { return string.Empty; }

        //if (!SaveContent) { return "Der Spalteninhalt wird nicht gespeichert."; }

        if (checkEditmode) {
            if (!EditableWithTextInput && !EditableWithDropdown && !db.PowerEdit) {
                return "Die Inhalte dieser Spalte k�nnen nicht manuell bearbeitet werden, da keine Bearbeitungsmethode erlaubt ist.";
            }

            if (UserEditDialogTypeInTable(Function, false, true, MultiLine && EditableWithDropdown) == EditTypeTable.None) {
                return "Interner Programm-Fehler: Es ist keine Bearbeitungsmethode f�r den Typ des Spalteninhalts '" + Function + "' definiert.";
            }
        }

        return string.Empty;
    }

    internal void Optimize() {
        if (!IsSystemColumn()) {
            // Maximale Text-L�nge beeinflusst stark die Ladezeit vom Server
            var l = CalculatePreveredMaxCellLenght(1.2f);
            if (l < MaxCellLenght) { MaxCellLenght = l; }

            if (MaxTextLenght > MaxCellLenght) { MaxTextLenght = MaxCellLenght; }

            // ScriptType beeinflusst, ob eine Zeile neu durchgerechnet werden muss nach einer �nderung dieser Zelle
            if (!UsedInScript()) {
                ScriptType = ScriptType.Nicht_vorhanden;
            }

            //// Beeinflussst stark den �ufbau bei gro�en Zeilen.
            //// Aber erst ab 20, da es ansonsten wenige verschiedene Eintr�ge sind, es und es sich nicht reniert.
            //// Zudem k�nnen zu kleine Werte ein Berechnungsfehler sein
            //Invalidate_ContentWidth();
            //if (_contentwidth is int v && v > 20 && FixedColumnWidth == 0) {
            //    FixedColumnWidth = v;
            //}
        }
    }

    /// <summary>
    /// Setzt den Wert in die dazugeh�rige Variable.
    /// </summary>
    /// <param name="type"></param>
    /// <param name="newvalue"></param>
    /// <returns></returns>
    internal string SetValueInternal(DatabaseDataType type, string newvalue) {
        if (type.IsObsolete()) { return string.Empty; }

        switch (type) {
            case DatabaseDataType.ColumnName:
                var oldname = _keyName;
                _keyName = newvalue.ToUpperInvariant();
                var ok = Database?.Column.ChangeName(oldname, _keyName) ?? "Datenbank verworfen";

                if (!string.IsNullOrEmpty(ok)) {
                    var reason = "Schwerer Spalten Umbenennungsfehler, " + ok;

                    Database?.Freeze(reason);
                    return reason;
                }
                break;

            case DatabaseDataType.ColumnCaption:
                _caption = newvalue;
                break;

            case DatabaseDataType.ColumnFunction:
                _function = (ColumnFunction)IntParse(newvalue);
                break;

            case DatabaseDataType.ForeColor:
                _foreColor = Color.FromArgb(IntParse(newvalue));
                break;

            case DatabaseDataType.BackColor:
                _backColor = Color.FromArgb(IntParse(newvalue));
                break;

            case DatabaseDataType.LineStyleLeft:
                _lineStyleLeft = (ColumnLineStyle)IntParse(newvalue);
                break;

            case DatabaseDataType.LineStyleRight:
                _lineStyleRight = (ColumnLineStyle)IntParse(newvalue);
                break;

            case DatabaseDataType.ColumnQuickInfo:
                _columnQuickInfo = newvalue;
                break;

            case DatabaseDataType.CaptionGroup1:
                _captionGroup1 = newvalue;
                break;

            case DatabaseDataType.CaptionGroup2:
                _captionGroup2 = newvalue;
                break;

            case DatabaseDataType.CaptionGroup3:
                _captionGroup3 = newvalue;
                break;

            case DatabaseDataType.MultiLine:
                _multiLine = newvalue.FromPlusMinus();
                break;

            case DatabaseDataType.DropDownItems:
                _dropDownItems.SplitAndCutByCr_QuickSortAndRemoveDouble(newvalue);
                break;

            case DatabaseDataType.LinkedCellFilter:
                _linkedCellFilter.SplitAndCutByCr(newvalue);
                break;

            //case DatabaseDataType.OpticalTextReplace:
            //    _opticalReplace.SplitAndCutByCr(newvalue);
            //    break;

            case DatabaseDataType.AutoReplaceAfterEdit:
                _afterEditAutoReplace.SplitAndCutByCr(newvalue);
                break;

            case DatabaseDataType.RegexCheck:
                _regexCheck = newvalue;
                break;

            case DatabaseDataType.ColumnTags:
                _columnTags.SplitAndCutByCr(newvalue);
                break;

            case DatabaseDataType.AutoFilterJoker:
                _autoFilterJoker = newvalue;
                break;

            case DatabaseDataType.PermissionGroupsChangeCell:
                _permissionGroupsChangeCell.SplitAndCutByCr_QuickSortAndRemoveDouble(newvalue);
                break;

            case DatabaseDataType.AllowedChars:
                _allowedChars = newvalue;
                break;

            case DatabaseDataType.MaxTextLenght:
                _maxTextLenght = IntParse(newvalue);
                //_maxCellLenght = _maxTextLenght;
                break;

            case DatabaseDataType.FilterOptions:
                _filterOptions = (FilterOptions)IntParse(newvalue);
                break;

            case DatabaseDataType.IgnoreAtRowFilter:
                _ignoreAtRowFilter = newvalue.FromPlusMinus();
                break;

            case DatabaseDataType.ShowUndo:
                _showUndo = newvalue.FromPlusMinus();
                break;

            case DatabaseDataType.EditableWithTextInput:
                _editableWithTextInput = newvalue.FromPlusMinus();
                break;

            case DatabaseDataType.EditableWithDropdown:
                _editableWithDropdown = newvalue.FromPlusMinus();
                break;

            case DatabaseDataType.SpellCheckingEnabled:
                _spellCheckingEnabled = newvalue.FromPlusMinus();
                break;

            case DatabaseDataType.DropdownDeselectAllAllowed:
                _dropdownDeselectAllAllowed = newvalue.FromPlusMinus();
                break;

            case DatabaseDataType.ShowValuesOfOtherCellsInDropdown:
                _showValuesOfOtherCellsInDropdown = newvalue.FromPlusMinus();
                break;

            case DatabaseDataType.SortAndRemoveDoubleAfterEdit:
                _afterEditQuickSortRemoveDouble = newvalue.FromPlusMinus();
                break;

            case DatabaseDataType.RoundAfterEdit:
                _roundAfterEdit = IntParse(newvalue);
                break;

            case DatabaseDataType.FixedColumnWidth:
                _fixedColumnWidth = IntParse(newvalue);
                break;

            case DatabaseDataType.MaxCellLenght:
                _maxCellLenght = IntParse(newvalue);
                break;

            case DatabaseDataType.DoUcaseAfterEdit:
                _afterEditDoUCase = newvalue.FromPlusMinus();
                break;

            case DatabaseDataType.AutoCorrectAfterEdit:
                _afterEditAutoCorrect = newvalue.FromPlusMinus();
                break;

            case DatabaseDataType.AutoRemoveCharAfterEdit:
                _autoRemove = newvalue;
                break;

            case DatabaseDataType.ColumnAdminInfo:
                _adminInfo = newvalue;
                break;

            case DatabaseDataType.ColumnSystemInfo:
                _columnSystemInfo = newvalue;
                break;

            case DatabaseDataType.RendererSettings:
                _rendererSettings = newvalue;
                break;

            case DatabaseDataType.DefaultRenderer:
                _defaultRenderer = newvalue;
                break;

            //case DatabaseDataType.ColumnContentWidth:
            //    _contentwidth = IntParse(newvalue);
            //    ContentWidthIsValid = true;
            //    break;

            case DatabaseDataType.CaptionBitmapCode:
                _captionBitmapCode = newvalue;
                break;

            case DatabaseDataType.LinkedDatabaseTableName:
                _linkedDatabaseTableName = newvalue;
                Invalidate_LinkedDatabase();
                break;

            //case DatabaseDataType.ConstantHeightOfImageCode:
            //    if (newvalue == "0") { newvalue = string.Empty; }
            //    _constantHeightOfImageCode = newvalue;
            //    break;

            //case (DatabaseDataType)160: //DatabaseDataType.Suffix:
            //    ManipulateRendererSettings("Suffix", newvalue);
            //    break;

            //case (DatabaseDataType)177: //DatabaseDataType.Prefix:
            //    ManipulateRendererSettings("Prefix", newvalue);
            //    break;

            case DatabaseDataType.DoOpticalTranslation:
                _doOpticalTranslation = (TranslationType)IntParse(newvalue);
                break;

            case DatabaseDataType.AdditionalFormatCheck:
                _additionalFormatCheck = (AdditionalCheck)IntParse(newvalue);
                break;

            case DatabaseDataType.ScriptType:
                _scriptType = (ScriptType)IntParse(newvalue);
                Database?.Row.InvalidateAllCheckData();
                break;

            //case DatabaseDataType.BehaviorOfImageAndText:
            //    _behaviorOfImageAndText = (BildTextVerhalten)IntParse(newvalue);
            //    break;

            case DatabaseDataType.EditAllowedDespiteLock:
                _editAllowedDespiteLock = newvalue.FromPlusMinus();
                break;

            case DatabaseDataType.TextFormatingAllowed:
                _textFormatingAllowed = newvalue.FromPlusMinus();
                break;

            //case DatabaseDataType.CellInitValue:
            //    _cellInitValue = newvalue;
            //    break;

            case DatabaseDataType.ColumnNameOfLinkedDatabase:

                _columnNameOfLinkedDatabase = newvalue.IsFormat(FormatHolder.Long) ? string.Empty : newvalue;

                break;

            case DatabaseDataType.SortType:
                _sortType = string.IsNullOrEmpty(newvalue) ? SortierTyp.Original_String : (SortierTyp)LongParse(newvalue);
                break;

            case DatabaseDataType.ColumnAlign:
                var tmpalign = (AlignmentHorizontal)IntParse(newvalue);
                if (tmpalign == (AlignmentHorizontal)(-1)) { tmpalign = AlignmentHorizontal.Links; }
                _align = tmpalign;
                break;

            default:
                if (!string.Equals(type.ToString(), ((int)type).ToString(), StringComparison.Ordinal)) {
                    return "Interner Fehler: F�r den Datentyp '" + type + "' wurde keine Laderegel definiert.";
                }
                break;
        }
        return string.Empty;
    }

    private void _database_Disposing(object sender, System.EventArgs e) => Dispose();

    private void _TMP_Linked_database_Disposing(object sender, System.EventArgs e) => Invalidate_LinkedDatabase();

    private void _TMP_LinkedDatabase_Cell_CellValueChanged(object sender, CellEventArgs e) {
        //if (IsDisposed || Database is not { IsDisposed: false } db) { return; }

        if (e.Column.KeyName != ColumnNameOfLinkedDatabase) { return; }

        //if (Function == ColumnFunction.Verkn�pfung_zu_anderer_Datenbank) {
        //    foreach (var thisRow in Database.Row) {
        //        if (Database.Cell.GetStringCore(this, thisRow) == e.Row.KeyName) {
        //            thisRow.InvalidateCheckData();
        //            Database.Cell.OnCellValueChanged(new CellEventArgs(this, thisRow, e.Reason));
        //            thisRow.DoSystemColumns(db, this, Generic.UserName, DateTime.UtcNow, Reason.SetCommand);
        //        }
        //    }
        //}

        if (Function == ColumnFunction.Verkn�pfung_zu_anderer_Datenbank && LinkedDatabase != null) {
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

        if (IsDisposed || Database is not { IsDisposed: false } db) { return; }

        foreach (var c in db.Column) {
            //if (thisColumn.KeyColumnKey == _name) { Am_A_Key_For_Other_Column = "Spalte " + thisColumn.ReadableText() + " verweist auf diese Spalte"; } // Werte Gleichhalten
            //if (thisColumn.LinkedCell_RowKeyIsInColumn == _name) { Am_A_Key_For_Other_Column = "Spalte " + thisColumn.ReadableText() + " verweist auf diese Spalte"; } // LinkdeCells pflegen
            //if (ThisColumn.LinkedCell_ColumnValueFoundIn == _name) { I_Am_A_Key_For_Other_Column = "Spalte " + ThisColumn.ReadableText() + " verweist auf diese Spalte"; } // LinkdeCells pflegen
            if (c.Function == ColumnFunction.Verkn�pfung_zu_anderer_Datenbank) {
                foreach (var thisitem in c.LinkedCellFilter) {
                    var tmp = thisitem.SplitBy("|");

                    if (tmp[2].ToLowerInvariant().Contains("~" + _keyName.ToLowerInvariant() + "~")) {
                        Am_A_Key_For_Other_Column = "Spalte " + c.ReadableText() + " verweist auf diese Spalte";
                    }
                }
            }
        }
        //if (_format == DataFormat.Columns_f�r_LinkedCellDropdown) { Am_A_Key_For_Other_Column = "Die Spalte selbst durch das Format"; }
    }

    private void Dispose(bool disposing) {
        if (!IsDisposed) {
            if (disposing) {
                OnDisposingEvent();
                // TODO: Verwalteten Zustand (verwaltete Objekte) bereinigen
                Database = null;
                Invalidate_LinkedDatabase();
            }

            _afterEditAutoReplace.Clear();
            _dropDownItems.Clear();
            _linkedCellFilter.Clear();
            _permissionGroupsChangeCell.Clear();
            _columnTags.Clear();

            // TODO: Nicht verwaltete Ressourcen (nicht verwaltete Objekte) freigeben und Finalizer �berschreiben
            // TODO: Gro�e Felder auf NULL setzen
            IsDisposed = true;
        }
    }

    private void GetLinkedDatabase() {
        Invalidate_LinkedDatabase(); // Um evtl. Events zu l�schen

        if (IsDisposed || Database is not { IsDisposed: false }) { return; }

        //foreach (var thisFile in AllFiles) {
        //    if (thisFile.TableName.Equals(_linkedDatabaseTableName, StringComparison.OrdinalIgnoreCase)) {
        //        _linkedDatabase = thisFile;
        //        break;
        //    }
        //}

        //if (_linkedDatabase == null && IsValidTableName(_linkedDatabaseTableName)) {
        _linkedDatabase = Database.Get(_linkedDatabaseTableName, false, null);
        //}

        if (_linkedDatabase != null) {
            _linkedDatabase.Cell.CellValueChanged += _TMP_LinkedDatabase_Cell_CellValueChanged;
            _linkedDatabase.DisposingEvent += _TMP_Linked_database_Disposing;
        }
    }

    private void Invalidate_LinkedDatabase() {
        if (_linkedDatabase != null) {
            _linkedDatabase.Cell.CellValueChanged -= _TMP_LinkedDatabase_Cell_CellValueChanged;
            _linkedDatabase.DisposingEvent -= _TMP_Linked_database_Disposing;
            _linkedDatabase = null;
        }
    }

    private string KleineFehlerCorrect(string txt) {
        if (string.IsNullOrEmpty(txt)) { return string.Empty; }
        const char h4 = (char)1004; // H4 = Normaler Text, nach links rutschen
        const char h3 = (char)1003; // �berschrift
        const char h2 = (char)1002; // �berschrift
        const char h1 = (char)1001; // �berschrift
        const char h7 = (char)1007; // bold
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
            // 180�C
            // Nach Zahlen KEINE leerzeichen einf�gen. Es gibt so viele dinge.... 90er Schichtsalat
            txt = txt.Insert(" ", ",", "1234567890, \r");
            txt = txt.Insert(" ", "!", " !?)\r");
            txt = txt.Insert(" ", "?", " !?)\r");
            txt = txt.Insert(" ", ".", " 1234567890.!?/)\r");
            txt = txt.Insert(" ", ")", " .;!?\r");
            txt = txt.Insert(" ", ";", " 1234567890\r");
            txt = txt.Insert(" ", ":", "1234567890 \\/\r"); // auch 3:50 Uhr
                                                            // H4= Normaler Text
            txt = txt.Replace(" " + h4, h4 + " "); // H4 = Normaler Text, nach links rutschen
            txt = txt.Replace("\r" + h4, h4 + "\r");
            // Dei restlichen Hs'
            txt = txt.Replace(h3 + " ", " " + h3); // �berschrift, nach Rechts
            txt = txt.Replace(h2 + " ", " " + h2); // �berschrift, nach Rechts
            txt = txt.Replace(h1 + " ", " " + h1); // �berschrift, nach Rechts
            txt = txt.Replace(h7 + " ", " " + h7); // Bold, nach Rechts
            txt = txt.Replace(h3 + "\r", "\r" + h3); // �berschrift, nach Rechts
            txt = txt.Replace(h2 + "\r", "\r" + h2); // �berschrift, nach Rechts
            txt = txt.Replace(h1 + "\r", "\r" + h1); // �berschrift, nach Rechts
            txt = txt.Replace(h7 + "\r", "\r" + h7); // Bold, nach Rechts
            txt = txt.Replace(h7 + h4.ToString(), h4.ToString());
            txt = txt.Replace(h3 + h4.ToString(), h4.ToString());
            txt = txt.Replace(h2 + h4.ToString(), h4.ToString());
            txt = txt.Replace(h1 + h4.ToString(), h4.ToString());
            txt = txt.Replace(h4 + h4.ToString(), h4.ToString());
            txt = txt.Replace(" �", "�");
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

    private void OnDisposingEvent() => DisposingEvent?.Invoke(this, System.EventArgs.Empty);

    #endregion
}