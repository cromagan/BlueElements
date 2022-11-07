// Authors:
// Christian Peter
//
// Copyright (c) 2022 Christian Peter
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
using BlueBasics.EventArgs;
using BlueBasics.Interfaces;
using BlueDatabase.Enums;
using BlueDatabase.EventArgs;
using BlueDatabase.Interfaces;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using static BlueBasics.Converter;
using static BlueBasics.IO;

namespace BlueDatabase;

public sealed class ColumnItem : IReadableTextWithChanging, IDisposableExtended, IInputFormat {

    #region Fields

    public bool? TmpAutoFilterSinnvoll = null;
    public QuickImage? TmpCaptionBitmapCode;
    public SizeF TmpCaptionTextSize = new(-1, -1);
    public int? TmpIfFilterRemoved = null;
    internal List<string>? UcaseNamesSortedByLenght;
    private readonly List<string> _afterEditAutoReplace = new();
    private readonly List<string> _dropDownItems = new();
    private readonly List<string?> _linkedCellFilter = new();
    private readonly List<string> _opticalReplace = new();
    private readonly List<string> _permissionGroupsChangeCell = new();
    private readonly List<string> _tags = new();
    private AdditionalCheck _additionalFormatCheck;
    private string _adminInfo;
    public string _timecode;
    private bool _afterEditAutoCorrect;
    private bool _afterEditDoUCase;
    private bool _afterEditQuickSortRemoveDouble;
    private int _afterEditRunden;
    private AlignmentHorizontal _align;
    private string _allowedChars;
    private string _autoFilterJoker;
    private string _autoRemove;
    private Color _backColor;
    private BildTextVerhalten _behaviorOfImageAndText;
    private string _caption;
    private string _captionBitmapCode;
    private string _captionGroup1;
    private string _captionGroup2;
    private string _captionGroup3;
    private string _cellInitValue;
    private string _constantHeightOfImageCode;
    private int _contentwidth;
    private TranslationType _doOpticalTranslation;
    private bool _dropdownAllesAbw�hlenErlaubt;
    private bool _dropdownBearbeitungErlaubt;

    private bool _dropdownWerteAndererZellenAnzeigen;
    private bool _editAllowedDespiteLock;
    private FilterOptions _filterOptions;
    private Color _foreColor;
    private DataFormat _format;
    private bool _formatierungErlaubt;
    private string _identifier;
    private bool _ignoreAtRowFilter;

    private long _key = -1;
    private long _keyColumnKey;

    private ColumnLineStyle _lineLeft;
    private ColumnLineStyle _lineRight;

    /// <summary>
    /// Die Quell-Spalte (aus der verlinkten Datenbank) ist immer
    /// </summary>
    private long _linkedCell_ColumnKeyOfLinkedDatabase;

    private string _linkedDatabaseFile;

    private bool _multiLine;
    private string _name;
    private string _prefix;
    private string _quickInfo;
    private string _regex = string.Empty;
    private bool _saveContent;
    private ScriptType _scriptType;
    private bool _showMultiLineInOneLine;
    private bool _showUndo;
    private SortierTyp _sortType;
    private bool _spellCheckingEnabled;
    private string _suffix;
    private bool _textBearbeitungErlaubt;
    private DatabaseAbstract? _tmpLinkedDatabase;
    private long _vorschlagsColumn;

    #endregion

    #region Constructors

    public ColumnItem(DatabaseAbstract database, long columnkey) : this(database, database.Column.Freename(string.Empty), columnkey) { }

    public ColumnItem(DatabaseAbstract database, string columnname, long columnkey) {
        Database = database;
        Database.Disposing += Database_Disposing;
        if (columnkey < 0) { Develop.DebugPrint(FehlerArt.Fehler, "ColumnKey <0"); }
        var ex = Database.Column.SearchByKey(columnkey);
        if (ex != null) { Develop.DebugPrint(FehlerArt.Fehler, "Key existiert bereits"); }
        _key = columnkey;

        #region Standard-Werte

        _name = columnname;
        _caption = string.Empty;
        //_CaptionBitmapCode = null;
        _format = DataFormat.Text;
        _lineLeft = ColumnLineStyle.D�nn;
        _lineRight = ColumnLineStyle.Ohne;
        _multiLine = false;
        _quickInfo = string.Empty;
        _captionGroup1 = string.Empty;
        _captionGroup2 = string.Empty;
        _captionGroup3 = string.Empty;
        //_Intelligenter_Multifilter = string.Empty;
        _foreColor = Color.Black;
        _backColor = Color.White;
        _cellInitValue = string.Empty;
        //_linkedCellRowKeyIsInColumn = -1;
        _linkedCell_ColumnKeyOfLinkedDatabase = -1;
        _sortType = SortierTyp.Original_String;
        //_ZellenZusammenfassen = false;
        //_dropDownKey = -1;
        _vorschlagsColumn = -1;
        _align = AlignmentHorizontal.Links;
        _keyColumnKey = -1;
        _identifier = string.Empty;
        _allowedChars = string.Empty;
        _adminInfo = string.Empty;
        _timecode = string.Empty;
        _contentwidth = -1;
        _captionBitmapCode = string.Empty;
        _filterOptions = FilterOptions.Enabled | FilterOptions.TextFilterEnabled | FilterOptions.ExtendedFilterEnabled;
        //_AutofilterErlaubt = true;
        //_AutofilterTextFilterErlaubt = true;
        //_AutoFilterErweitertErlaubt = true;
        _ignoreAtRowFilter = false;
        _dropdownBearbeitungErlaubt = false;
        _dropdownAllesAbw�hlenErlaubt = false;
        _textBearbeitungErlaubt = false;
        _dropdownWerteAndererZellenAnzeigen = false;
        _afterEditQuickSortRemoveDouble = false;
        _afterEditRunden = -1;
        _afterEditAutoCorrect = false;
        _afterEditDoUCase = false;
        _formatierungErlaubt = false;
        _additionalFormatCheck = AdditionalCheck.None;
        _scriptType = ScriptType.undefiniert;
        _autoRemove = string.Empty;
        _autoFilterJoker = string.Empty;
        _saveContent = true;
        //_AutoFilter_Dauerfilter = enDauerfilter.ohne;
        _spellCheckingEnabled = false;
        //_CompactView = true;
        _showUndo = true;
        _doOpticalTranslation = TranslationType.Original_Anzeigen;
        _showMultiLineInOneLine = false;
        _editAllowedDespiteLock = false;
        _suffix = string.Empty;
        _linkedDatabaseFile = string.Empty;
        _behaviorOfImageAndText = BildTextVerhalten.Nur_Text;
        _constantHeightOfImageCode = string.Empty;
        _prefix = string.Empty;
        UcaseNamesSortedByLenght = null;

        #endregion Standard-Werte

        Invalidate_TmpVariables();
    }

    #endregion

    #region Destructors

    // TODO: Finalizer nur �berschreiben, wenn "Dispose(bool disposing)" Code f�r die Freigabe nicht verwalteter Ressourcen enth�lt
    ~ColumnItem() {
        // �ndern Sie diesen Code nicht. F�gen Sie Bereinigungscode in der Methode "Dispose(bool disposing)" ein.
        Dispose(false);
    }

    #endregion

    #region Events

    public event EventHandler Changed;

    #endregion

    #region Properties

    public AdditionalCheck AdditionalFormatCheck {
        get => _additionalFormatCheck;
        set {
            if (_additionalFormatCheck == value) { return; }
            Database?.ChangeData(DatabaseDataType.AdditionalFormatCheck, this, null, ((int)_additionalFormatCheck).ToString(), ((int)value).ToString());
            OnChanged();
        }
    }

    public string AdminInfo {
        get => _adminInfo;
        set {
            if (_adminInfo == value) { return; }
            Database?.ChangeData(DatabaseDataType.ColumnAdminInfo, this, null, _adminInfo, value);
            OnChanged();
        }
    }

    public bool AfterEditAutoCorrect {
        get => _afterEditAutoCorrect;
        set {
            if (_afterEditAutoCorrect == value) { return; }
            Database?.ChangeData(DatabaseDataType.AutoCorrectAfterEdit, this, null, _afterEditAutoCorrect.ToPlusMinus(), value.ToPlusMinus());
            OnChanged();
        }
    }

    public List<string> AfterEditAutoReplace {
        get => _afterEditAutoReplace;
        set {
            if (!_afterEditAutoReplace.IsDifferentTo(value)) { return; }
            Database.ChangeData(DatabaseDataType.AutoReplaceAfterEdit, this, null, _afterEditAutoReplace.JoinWithCr(), value.JoinWithCr());
            OnChanged();
        }
    }

    public bool AfterEditDoUCase {
        get => _afterEditDoUCase;
        set {
            if (_afterEditDoUCase == value) { return; }
            Database?.ChangeData(DatabaseDataType.DoUcaseAfterEdit, this, null, _afterEditDoUCase.ToPlusMinus(), value.ToPlusMinus());
            OnChanged();
        }
    }

    public bool AfterEditQuickSortRemoveDouble {
        get => _multiLine && _afterEditQuickSortRemoveDouble;
        set {
            if (_afterEditQuickSortRemoveDouble == value) { return; }
            Database?.ChangeData(DatabaseDataType.SortAndRemoveDoubleAfterEdit, this, null, _afterEditQuickSortRemoveDouble.ToPlusMinus(), value.ToPlusMinus());
            OnChanged();
        }
    }

    public int AfterEditRunden {
        get => _afterEditRunden;
        set {
            if (_afterEditRunden == value) { return; }
            Database?.ChangeData(DatabaseDataType.RoundAfterEdit, this, null, _afterEditRunden.ToString(), value.ToString());
            OnChanged();
        }
    }

    public AlignmentHorizontal Align {
        get => _align;
        set {
            if (_align == value) { return; }
            Database?.ChangeData(DatabaseDataType.ColumnAlign, this, null, ((int)_align).ToString(), ((int)value).ToString());
            OnChanged();
        }
    }

    public string AllowedChars {
        get => _allowedChars;
        set {
            if (_allowedChars == value) { return; }
            Database?.ChangeData(DatabaseDataType.AllowedChars, this, null, _allowedChars, value);
            OnChanged();
        }
    }

    public string Am_A_Key_For_Other_Column { get; private set; }

    public string AutoFilterJoker {
        get => _autoFilterJoker;
        set {
            if (_autoFilterJoker == value) { return; }
            Database?.ChangeData(DatabaseDataType.AutoFilterJoker, this, null, _autoFilterJoker, value);
            OnChanged();
        }
    }

    public string AutoRemove {
        get => _autoRemove;
        set {
            if (_autoRemove == value) { return; }
            Database?.ChangeData(DatabaseDataType.AutoRemoveCharAfterEdit, this, null, _autoRemove, value);
            OnChanged();
        }
    }

    public Color BackColor {
        get => _backColor;
        set {
            if (_backColor.ToArgb() == value.ToArgb()) { return; }
            Database?.ChangeData(DatabaseDataType.BackColor, this, null, _backColor.ToArgb().ToString(), value.ToArgb().ToString());
            OnChanged();
        }
    }

    public BildTextVerhalten BehaviorOfImageAndText {
        get => _behaviorOfImageAndText;
        set {
            if (_behaviorOfImageAndText == value) { return; }
            Database?.ChangeData(DatabaseDataType.BehaviorOfImageAndText, this, null, ((int)_behaviorOfImageAndText).ToString(), ((int)value).ToString());
            OnChanged();
        }
    }

    public string Caption {
        get => _caption;
        set {
            value = value.Replace("<br>", "\r", RegexOptions.IgnoreCase);
            if (_caption == value) { return; }
            Database?.ChangeData(DatabaseDataType.ColumnCaption, this, null, _caption, value);
            Invalidate_TmpVariables();
            OnChanged();
        }
    }

    public string CaptionBitmapCode {
        get => _captionBitmapCode;
        set {
            if (_captionBitmapCode == value) { return; }
            Database?.ChangeData(DatabaseDataType.CaptionBitmapCode, this, null, _captionBitmapCode, value);
            _captionBitmapCode = value;
            Invalidate_TmpVariables();
            OnChanged();
        }
    }

    public string CaptionGroup1 {
        get => _captionGroup1;
        set {
            if (_captionGroup1 == value) { return; }
            Database?.ChangeData(DatabaseDataType.CaptionGroup1, this, null, _captionGroup1, value);
            OnChanged();
        }
    }

    public string CaptionGroup2 {
        get => _captionGroup2;
        set {
            if (_captionGroup2 == value) { return; }
            Database?.ChangeData(DatabaseDataType.CaptionGroup2, this, null, _captionGroup2, value);
            OnChanged();
        }
    }

    public string CaptionGroup3 {
        get => _captionGroup3;
        set {
            if (_captionGroup3 == value) { return; }
            Database?.ChangeData(DatabaseDataType.CaptionGroup3, this, null, _captionGroup3, value);
            OnChanged();
        }
    }

    public string CellInitValue {
        get => _cellInitValue;
        set {
            if (_cellInitValue == value) { return; }
            Database?.ChangeData(DatabaseDataType.CellInitValue, this, null, _cellInitValue, value);
            OnChanged();
        }
    }

    public string ConstantHeightOfImageCode {
        get => _constantHeightOfImageCode;
        set {
            if (_constantHeightOfImageCode == value) { return; }
            Database?.ChangeData(DatabaseDataType.ConstantHeightOfImageCode, this, null, _constantHeightOfImageCode, value);
            Invalidate_ColumAndContent();
            OnChanged();
        }
    }

    public int ContentWidth {
        get => _contentwidth;
        set {
            if (_contentwidth == value) { return; }
            Database?.ChangeData(DatabaseDataType.ColumnContentWidth, this, null, _contentwidth.ToString(), value.ToString());
            OnChanged();
        }
    }


    public string TimeCode {
        get => _timecode;
        set {
            if (_timecode == value) { return; }
            Database?.ChangeData(DatabaseDataType.ColumnTimeCode, this, null, _timecode, value);
            OnChanged();
        }
    }

    public DatabaseAbstract? Database { get; private set; }

    public TranslationType DoOpticalTranslation {
        get => _doOpticalTranslation;
        set {
            if (_doOpticalTranslation == value) { return; }
            Database?.ChangeData(DatabaseDataType.DoOpticalTranslation, this, null, ((int)_doOpticalTranslation).ToString(), ((int)value).ToString());
            OnChanged();
        }
    }

    public bool DropdownAllesAbw�hlenErlaubt {
        get => _dropdownAllesAbw�hlenErlaubt;
        set {
            if (_dropdownAllesAbw�hlenErlaubt == value) { return; }
            Database?.ChangeData(DatabaseDataType.DropdownDeselectAllAllowed, this, null, _dropdownAllesAbw�hlenErlaubt.ToPlusMinus(), value.ToPlusMinus());
            OnChanged();
        }
    }

    public bool DropdownBearbeitungErlaubt {
        get => _dropdownBearbeitungErlaubt;
        set {
            if (_dropdownBearbeitungErlaubt == value) { return; }
            Database?.ChangeData(DatabaseDataType.EditableWithDropdown, this, null, _dropdownBearbeitungErlaubt.ToPlusMinus(), value.ToPlusMinus());
            OnChanged();
        }
    }

    public List<string> DropDownItems {
        get => _dropDownItems;
        set {
            if (!_dropDownItems.IsDifferentTo(value)) { return; }
            Database.ChangeData(DatabaseDataType.DropDownItems, this, null, _dropDownItems.JoinWithCr(), value.JoinWithCr());
            OnChanged();
        }
    }

    public bool DropdownWerteAndererZellenAnzeigen {
        get => _dropdownWerteAndererZellenAnzeigen;
        set {
            if (_dropdownWerteAndererZellenAnzeigen == value) { return; }
            Database?.ChangeData(DatabaseDataType.ShowValuesOfOtherCellsInDropdown, this, null, _dropdownWerteAndererZellenAnzeigen.ToPlusMinus(), value.ToPlusMinus());
            OnChanged();
        }
    }

    public bool EditAllowedDespiteLock {
        get => _editAllowedDespiteLock;
        set {
            if (_editAllowedDespiteLock == value) { return; }
            Database?.ChangeData(DatabaseDataType.EditAllowedDespiteLock, this, null, _editAllowedDespiteLock.ToPlusMinus(), value.ToPlusMinus());
            OnChanged();
        }
    }

    public FilterOptions FilterOptions {
        get => _filterOptions;
        set {
            if (_filterOptions == value) { return; }
            Database?.ChangeData(DatabaseDataType.FilterOptions, this, null, ((int)_filterOptions).ToString(), ((int)value).ToString());
            Invalidate_TmpVariables();
            OnChanged();
        }
    }

    public Color ForeColor {
        get => _foreColor;
        set {
            if (_foreColor.ToArgb() == value.ToArgb()) { return; }
            Database?.ChangeData(DatabaseDataType.ForeColor, this, null, _foreColor.ToArgb().ToString(), value.ToArgb().ToString());
            OnChanged();
        }
    }

    public DataFormat Format {
        get => _format;
        set {
            if (_format == value) { return; }
            Database?.ChangeData(DatabaseDataType.ColumnFormat, this, null, ((int)_format).ToString(), ((int)value).ToString());
            Invalidate_ColumAndContent();
            OnChanged();
        }
    }

    public bool FormatierungErlaubt {
        get => _formatierungErlaubt;
        set {
            if (_formatierungErlaubt == value) { return; }
            Database?.ChangeData(DatabaseDataType.TextFormatingAllowed, this, null, _formatierungErlaubt.ToPlusMinus(), value.ToPlusMinus());
            OnChanged();
        }
    }

    public string Identifier {
        get => _identifier;
        set {
            if (_identifier == value) { return; }
            Database?.ChangeData(DatabaseDataType.ColumnIdentify, this, null, _identifier, value);
            OnChanged();
        }
    }

    public bool IgnoreAtRowFilter {
        get => !_format.Autofilter_m�glich() || _ignoreAtRowFilter;
        set {
            if (_ignoreAtRowFilter == value) { return; }
            Database?.ChangeData(DatabaseDataType.IgnoreAtRowFilter, this, null, _ignoreAtRowFilter.ToPlusMinus(), value.ToPlusMinus());
            OnChanged();
        }
    }

    public bool IsDisposed { get; private set; }

    public long Key {
        get => _key;
        set {
            if (_key == value) { return; }
            Database?.ChangeData(DatabaseDataType.ColumnKey, this, null, Key.ToString(), value.ToString());
            OnChanged();
        }
    }

    /// <summary>
    /// H�lt Werte, dieser Spalte gleich, bezugnehmend der KeyColumn(key)
    /// </summary>
    public long KeyColumnKey {
        get => _keyColumnKey;
        set {
            if (_keyColumnKey == value) { return; }
            var c = Database?.Column.SearchByKey(_keyColumnKey);
            c?.CheckIfIAmAKeyColumn();
            Database?.ChangeData(DatabaseDataType.KeyColumnKey, this, null, _keyColumnKey.ToString(), value.ToString());
            c = Database?.Column.SearchByKey(_keyColumnKey);
            c?.CheckIfIAmAKeyColumn();
            OnChanged();
        }
    }

    public ColumnLineStyle LineLeft {
        get => _lineLeft;
        set {
            if (_lineLeft == value) { return; }
            Database?.ChangeData(DatabaseDataType.LineStyleLeft, this, null, ((int)_lineLeft).ToString(), ((int)value).ToString());
            OnChanged();
        }
    }

    public ColumnLineStyle LineRight {
        get => _lineRight;
        set {
            if (_lineRight == value) { return; }
            Database?.ChangeData(DatabaseDataType.LineStyleRight, this, null, ((int)_lineRight).ToString(), ((int)value).ToString());
            OnChanged();
        }
    }

    /// <summary>
    /// Die Quell-Spalte (aus der verlinkten Datenbank) ist immer
    /// </summary>
    public long LinkedCell_ColumnKeyOfLinkedDatabase {
        get => _linkedCell_ColumnKeyOfLinkedDatabase;
        set {
            if (_linkedCell_ColumnKeyOfLinkedDatabase == value) { return; }
            Database?.ChangeData(DatabaseDataType.ColumnKeyOfLinkedDatabase, this, null, _linkedCell_ColumnKeyOfLinkedDatabase.ToString(), value.ToString());
            Invalidate_ColumAndContent();
            OnChanged();
        }
    }

    public List<string> LinkedCellFilter {
        get => _linkedCellFilter;
        set {
            if (!_linkedCellFilter.IsDifferentTo(value)) { return; }
            Database.ChangeData(DatabaseDataType.LinkedCellFilter, this, null, _linkedCellFilter.JoinWithCr(), value.JoinWithCr());
            OnChanged();
        }
    }

    public DatabaseAbstract? LinkedDatabase {
        get {
            if (_tmpLinkedDatabase != null) { return _tmpLinkedDatabase; }
            if (string.IsNullOrEmpty(_linkedDatabaseFile)) { return null; }

            var ci = new ConnectionInfo(_linkedDatabaseFile);

            //q
            //var ci = Database.ConnectionDataOfOtherTable(_linkedDatabaseFile.FileNameWithoutSuffix());//  new ConnectionInfo(_linkedDatabaseFile, this, this.DatabaseID, string.Empty);

            Tmp_LinkedDatabase = DatabaseAbstract.GetByID(ci);

            if (_tmpLinkedDatabase != null) { _tmpLinkedDatabase.UserGroup = Database.UserGroup; }
            return _tmpLinkedDatabase;
        }
    }

    public string LinkedDatabaseFile {
        get => _linkedDatabaseFile;
        set {
            if (_linkedDatabaseFile == value) { return; }
            Database?.ChangeData(DatabaseDataType.LinkedDatabase, this, null, _linkedDatabaseFile, value);
            Invalidate_TmpVariables();
            OnChanged();
        }
    }

    public bool MultiLine {
        get => _multiLine;
        set {
            if (!_format.MultilinePossible()) { value = false; }
            if (_multiLine == value) { return; }
            Database?.ChangeData(DatabaseDataType.MultiLine, this, null, _multiLine.ToPlusMinus(), value.ToPlusMinus());
            Invalidate_ColumAndContent();
            OnChanged();
        }
    }

    public string Name {
        get => _name.ToUpper();
        set {
            value = value.ToUpper();
            if (value == _name.ToUpper()) { return; }
            if (Database?.Column.Exists(value) != null) { return; }
            if (string.IsNullOrEmpty(value)) { return; }
            var old = _name;
            Database?.ChangeData(DatabaseDataType.ColumnName, this, null, old, value);
            Database?.Column_NameChanged(old, this);
            OnChanged();
        }
    }

    public List<string> OpticalReplace {
        get => _opticalReplace;
        set {
            if (!_opticalReplace.IsDifferentTo(value)) { return; }
            Database.ChangeData(DatabaseDataType.OpticalTextReplace, this, null, _opticalReplace.JoinWithCr(), value.JoinWithCr());
            OnChanged();
        }
    }

    public List<string> PermissionGroupsChangeCell {
        get => _permissionGroupsChangeCell;
        set {
            if (!_permissionGroupsChangeCell.IsDifferentTo(value)) { return; }
            Database.ChangeData(DatabaseDataType.PermissionGroupsChangeCell, this, null, _permissionGroupsChangeCell.JoinWithCr(), value.JoinWithCr());
            OnChanged();
        }
    }

    public string Prefix {
        get => _prefix;
        set {
            if (_prefix == value) { return; }
            Database?.ChangeData(DatabaseDataType.Prefix, this, null, _prefix, value);
            Invalidate_ColumAndContent();
            OnChanged();
        }
    }

    public string Quickinfo {
        get => _quickInfo;
        set {
            if (_quickInfo == value) { return; }
            Database?.ChangeData(DatabaseDataType.ColumnQuickInfo, this, null, _quickInfo, value);
            OnChanged();
        }
    }

    public string Regex {
        get => _regex;
        set {
            if (_regex == value) { return; }
            Database?.ChangeData(DatabaseDataType.RegexCheck, this, null, _regex, value);
            OnChanged();
        }
    }

    public bool SaveContent {
        get => _saveContent;
        set {
            if (_saveContent == value) { return; }
            Database?.ChangeData(DatabaseDataType.SaveContent, this, null, _saveContent.ToPlusMinus(), value.ToPlusMinus());
            OnChanged();
        }
    }

    public ScriptType ScriptType {
        get => _scriptType;
        set {
            if (_scriptType == value) { return; }
            Database?.ChangeData(DatabaseDataType.ScriptType, this, null, ((int)_scriptType).ToString(), ((int)value).ToString());
            OnChanged();
        }
    }

    public bool ShowMultiLineInOneLine {
        get => _multiLine && _showMultiLineInOneLine;
        set {
            if (_showMultiLineInOneLine == value) { return; }
            Database?.ChangeData(DatabaseDataType.ShowMultiLineInOneLine, this, null, _showMultiLineInOneLine.ToPlusMinus(), value.ToPlusMinus());
            Invalidate_ColumAndContent();
            OnChanged();
        }
    }

    public bool ShowUndo {
        get => _showUndo;
        set {
            if (_showUndo == value) { return; }
            Database?.ChangeData(DatabaseDataType.ShowUndo, this, null, _showUndo.ToPlusMinus(), value.ToPlusMinus());
            OnChanged();
        }
    }

    public SortierTyp SortType {
        get => _sortType;
        set {
            if (_sortType == value) { return; }
            Database?.ChangeData(DatabaseDataType.SortType, this, null, ((int)_sortType).ToString(), ((int)value).ToString());
            OnChanged();
        }
    }

    public bool SpellCheckingEnabled {
        get => _spellCheckingEnabled;
        set {
            if (_spellCheckingEnabled == value) { return; }
            Database?.ChangeData(DatabaseDataType.SpellCheckingEnabled, this, null, _spellCheckingEnabled.ToPlusMinus(), value.ToPlusMinus());
            OnChanged();
        }
    }

    /// <summary>
    /// Was in Textfeldern oder Datenbankzeilen f�r ein Suffix angezeigt werden soll. Beispiel: mm
    /// </summary>
    public string Suffix {
        get => _suffix;
        set {
            if (_suffix == value) { return; }
            Database?.ChangeData(DatabaseDataType.Suffix, this, null, _suffix, value);
            Invalidate_ColumAndContent();
            OnChanged();
        }
    }

    public List<string> Tags {
        get => _tags;
        set {
            if (!_tags.IsDifferentTo(value)) { return; }
            Database.ChangeData(DatabaseDataType.ColumnTags, this, null, _tags.JoinWithCr(), value.JoinWithCr());
            OnChanged();
        }
    }

    public bool TextBearbeitungErlaubt {
        get => Database?.PowerEdit.Subtract(DateTime.Now).TotalSeconds > 0 || _textBearbeitungErlaubt;
        set {
            if (_textBearbeitungErlaubt == value) { return; }
            Database?.ChangeData(DatabaseDataType.EditableWithTextInput, this, null, _textBearbeitungErlaubt.ToPlusMinus(), value.ToPlusMinus());
            OnChanged();
        }
    }

    public string Ueberschriften {
        get {
            var txt = _captionGroup1 + "/" + _captionGroup2 + "/" + _captionGroup3;
            return txt == "//" ? "###" : txt.TrimEnd("/");
        }
    }

    public long VorschlagsColumn {
        get => _vorschlagsColumn;
        set {
            if (_vorschlagsColumn == value) { return; }
            Database?.ChangeData(DatabaseDataType.MakeSuggestionFromSameKeyColumn, this, null, _vorschlagsColumn.ToString(), value.ToString());
            OnChanged();
        }
    }

    private DatabaseAbstract Tmp_LinkedDatabase {
        set {
            if (value == _tmpLinkedDatabase) { return; }
            Invalidate_TmpVariables();
            _tmpLinkedDatabase = value;
            if (_tmpLinkedDatabase != null) {
                _tmpLinkedDatabase.ConnectedControlsStopAllWorking += _TMP_LinkedDatabase_ConnectedControlsStopAllWorking;
                _tmpLinkedDatabase.Cell.CellValueChanged += _TMP_LinkedDatabase_Cell_CellValueChanged;
                _tmpLinkedDatabase.Disposing += _TMP_LinkedDatabase_Disposing;
            }
        }
    }

    #endregion

    #region Methods

    public static bool IsValidColumnName(string name) {
        if (string.IsNullOrWhiteSpace(name)) { return false; }
        if (!name.ContainsOnlyChars(Constants.AllowedCharsVariableName)) { return false; }

        if (!Constants.Char_AZ.Contains(name.Substring(0, 1).ToUpper())) { return false; }

        if (name.ToUpper() == "USER") { return false; } // SQL System-Name

        return true;
    }

    public static EditTypeTable UserEditDialogTypeInTable(DataFormat format, bool doDropDown, bool keybordInputAllowed, bool isMultiline) {
        if (!doDropDown && !keybordInputAllowed) { return EditTypeTable.None; }

        switch (format) {
            case DataFormat.Werte_aus_anderer_Datenbank_als_DropDownItems:
                return EditTypeTable.Dropdown_Single;

            //case DataFormat.Link_To_Filesystem:
            //    return EditTypeTable.FileHandling_InDateiSystem;

            case DataFormat.FarbeInteger:
                if (doDropDown) { return EditTypeTable.Dropdown_Single; }
                return EditTypeTable.Farb_Auswahl_Dialog;

            case DataFormat.Schrift:
                if (doDropDown) { return EditTypeTable.Dropdown_Single; }
                return EditTypeTable.Font_AuswahlDialog;

            case DataFormat.Button:
                return EditTypeTable.None;

            default:
                if (format.TextboxEditPossible()) {
                    return !doDropDown
                        ? EditTypeTable.Textfeld
                        : isMultiline
                            ? EditTypeTable.Dropdown_Single
                            : keybordInputAllowed ? EditTypeTable.Textfeld_mit_Auswahlknopf : EditTypeTable.Dropdown_Single;
                }
                Develop.DebugPrint(format);
                return EditTypeTable.None;
        }
    }

    public static EditTypeTable UserEditDialogTypeInTable(ColumnItem? column, bool doDropDown) => UserEditDialogTypeInTable(column.Format, doDropDown, column.TextBearbeitungErlaubt, column.MultiLine);

    public string AutoCorrect(string value) {
        //if (Format == DataFormat.Link_To_Filesystem) {
        //    List<string> l = new(value.SplitAndCutByCr());
        //    var l2 = l.Select(thisFile => SimplyFile(thisFile)).ToList();
        //    value = l2.SortedDistinctList().JoinWithCr();
        //}
        if (_afterEditDoUCase) { value = value.ToUpper(); }
        if (!string.IsNullOrEmpty(_autoRemove)) { value = value.RemoveChars(_autoRemove); }
        if (_afterEditAutoReplace.Count > 0) {
            List<string> l = new(value.SplitAndCutByCr());
            foreach (var thisar in _afterEditAutoReplace) {
                var rep = thisar.SplitAndCutBy("|");
                for (var z = 0; z < l.Count; z++) {
                    var r = string.Empty;
                    if (rep.Length > 1) { r = rep[1].Replace(";cr;", "\r"); }
                    var op = string.Empty;
                    if (rep.Length > 2) { op = rep[2].ToLower(); }
                    if (op == "casesensitive") {
                        if (l[z] == rep[0]) { l[z] = r; }
                    } else if (op == "instr") {
                        l[z] = l[z].Replace(rep[0], r, RegexOptions.IgnoreCase);
                        //if (l[z].ToLower() == rep[0].ToLower()) { l[z] = r; }
                    } else {
                        if (string.Equals(l[z], rep[0], StringComparison.OrdinalIgnoreCase)) { l[z] = r; }
                    }
                }
            }
            value = l.JoinWithCr();
        }
        if (_afterEditAutoCorrect) { value = KleineFehlerCorrect(value); }
        if (_afterEditRunden > -1 && DoubleTryParse(value, out var erg)) {
            erg = Math.Round(erg, _afterEditRunden);
            value = erg.ToString(CultureInfo.InvariantCulture);
        }
        if (_afterEditQuickSortRemoveDouble) {
            var l = new List<string>(value.SplitAndCutByCr()).SortedDistinctList();
            value = l.JoinWithCr();
        }
        return value;
    }

    public bool AutoFilterSymbolPossible() => FilterOptions.HasFlag(FilterOptions.Enabled) && Format.Autofilter_m�glich();

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
                Develop.DebugPrint(FehlerArt.Warnung, "Nummer " + number + " nicht erlaubt.");
                return string.Empty;
        }
    }

    /// <summary>
    /// �berschreibt alle Spalteeigenschaften mit der der Vorlage.
    /// </summary>
    /// <param name="source"></param>
    public void CloneFrom(ColumnItem source, bool nameAndKeyToo) {
        if (nameAndKeyToo) {
            Name = source.Name;
            //Database.ChangeData(DatabaseDataType.ColumnKey, this, null, this.Key.ToString(), source.Key.ToString());

            Key = source.Key;
        }

        Caption = source.Caption;
        CaptionBitmapCode = source.CaptionBitmapCode;
        Format = source.Format;
        LineLeft = source.LineLeft;
        LineRight = source.LineRight;
        MultiLine = source.MultiLine;
        Quickinfo = source.Quickinfo;
        ForeColor = source.ForeColor;
        BackColor = source.BackColor;
        EditAllowedDespiteLock = source.EditAllowedDespiteLock;
        Identifier = source.Identifier;
        PermissionGroupsChangeCell = source.PermissionGroupsChangeCell;
        Tags = source.Tags;
        AdminInfo = source.AdminInfo;
        TimeCode = source.TimeCode;
        ContentWidth = source.ContentWidth;
        FilterOptions = source.FilterOptions;
        IgnoreAtRowFilter = source.IgnoreAtRowFilter;
        DropdownBearbeitungErlaubt = source.DropdownBearbeitungErlaubt;
        DropdownAllesAbw�hlenErlaubt = source.DropdownAllesAbw�hlenErlaubt;
        TextBearbeitungErlaubt = source.TextBearbeitungErlaubt;
        SpellCheckingEnabled = source.SpellCheckingEnabled;
        DropdownWerteAndererZellenAnzeigen = source.DropdownWerteAndererZellenAnzeigen;
        AfterEditQuickSortRemoveDouble = source.AfterEditQuickSortRemoveDouble;
        AfterEditRunden = source.AfterEditRunden;
        AfterEditDoUCase = source.AfterEditDoUCase;
        AfterEditAutoCorrect = source.AfterEditAutoCorrect;
        AutoRemove = source.AutoRemove;
        SaveContent = source.SaveContent;
        CellInitValue = source.CellInitValue;
        AutoFilterJoker = source.AutoFilterJoker;
        KeyColumnKey = source.KeyColumnKey;
        //LinkedCell_RowKeyIsInColumn = source.LinkedCell_RowKeyIsInColumn;
        LinkedCell_ColumnKeyOfLinkedDatabase = source.LinkedCell_ColumnKeyOfLinkedDatabase;
        //DropdownKey = source.DropdownKey;
        VorschlagsColumn = source.VorschlagsColumn;
        Align = source.Align;
        SortType = source.SortType;
        DropDownItems = source.DropDownItems;
        LinkedCellFilter = source.LinkedCellFilter;
        OpticalReplace = source.OpticalReplace;
        AfterEditAutoReplace = source.AfterEditAutoReplace;
        this.GetStyleFrom(source); // regex, Allowed Chars, etc.
        ScriptType = source.ScriptType;
        ShowUndo = source.ShowUndo;
        ShowMultiLineInOneLine = source.ShowMultiLineInOneLine;
        CaptionGroup1 = source.CaptionGroup1;
        CaptionGroup2 = source.CaptionGroup2;
        CaptionGroup3 = source.CaptionGroup3;
        //LinkedKeyKennung = source.LinkedKeyKennung;
        LinkedDatabaseFile = source.LinkedDatabaseFile;
        BehaviorOfImageAndText = source.BehaviorOfImageAndText;
        ConstantHeightOfImageCode = source.ConstantHeightOfImageCode;
        //BestFile_StandardSuffix = source.BestFile_StandardSuffix;
        //BestFile_StandardFolder = source.BestFile_StandardFolder;
    }

    public string CompareKey() {
        var tmp = string.IsNullOrEmpty(_caption) ? _name + Constants.FirstSortChar + _name : _caption + Constants.FirstSortChar + _name;
        tmp = tmp.Trim(' ');
        tmp = tmp.TrimStart('-');
        tmp = tmp.Trim(' ');
        return tmp;
    }

    public List<string> Contents() => Contents(null as FilterCollection, null);

    /// <summary>
    ///
    /// </summary>
    /// <param name="filter">Wird kein Filter �bergeben, werden alle Inhalte zur�ckgegeben!</param>
    /// <param name="pinned"></param>
    /// <returns></returns>
    public List<string> Contents(List<FilterItem>? filter, List<RowItem>? pinned) {
        if (filter == null || filter.Count == 0) { return Contents(); }
        var ficol = new FilterCollection(filter[0].Database);
        ficol.AddRange(filter);
        return Contents(ficol, pinned);
    }

    /// <summary>
    ///
    /// </summary>
    /// <param name="filter">Wird kein Filter �bergeben, werden alle Inhalte zur�ckgegeben!</param>
    /// <param name="pinned"></param>
    /// <returns></returns>
    public List<string> Contents(FilterItem filter, List<RowItem>? pinned) {
        var x = new FilterCollection(filter.Database) { filter };
        return Contents(x, pinned);
    }

    public List<string> Contents(List<RowItem?>? pinned) {
        List<string> list = new();

        if (pinned == null || pinned.Count == 0) { return list; }

        foreach (var thisRowItem in pinned) {
            if (thisRowItem != null) {
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

    /// <summary>
    ///
    /// </summary>
    /// <param name="filter">Wird kein Filter �bergeben, werden alle Inhalte zur�ckgegeben!</param>
    /// <param name="pinned"></param>
    /// <returns></returns>
    public List<string> Contents(FilterCollection filter, List<RowItem>? pinned) {
        List<string> list = new();
        if (Database == null) { return list; }


        Database.RefreshColumnsData(filter);



        foreach (var thisRowItem in Database.Row) {
            if (thisRowItem != null) {
                var add = thisRowItem.MatchesTo(filter);
                if (!add && pinned != null) { add = pinned.Contains(thisRowItem); }
                if (add) {
                    if (_multiLine) {
                        list.AddRange(thisRowItem.CellGetList(this));
                    } else {
                        if (thisRowItem.CellGetString(this).Length > 0) {
                            list.Add(thisRowItem.CellGetString(this));
                        }
                    }
                }
            }
        }
        return list.SortedDistinctList();
    }

    /// <summary>
    ///
    /// </summary>
    /// <param name="filter">Wird kein Filter �bergeben, werden alle Inhalte gel�scht!</param>
    /// <param name="pinned"></param>
    /// <returns></returns>
    public void DeleteContents(FilterCollection filter, List<RowItem>? pinned) {
        foreach (var thisRowItem in Database.Row) {
            if (thisRowItem != null) {
                if (thisRowItem.MatchesTo(filter) || pinned.Contains(thisRowItem)) { thisRowItem.CellSet(this, ""); }
            }
        }
    }

    public void Dispose() {
        // �ndern Sie diesen Code nicht. F�gen Sie Bereinigungscode in der Methode "Dispose(bool disposing)" ein.
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    public string ErrorReason() {
        if (Key < 0) { return "Interner Fehler: ID nicht definiert"; }
        if (string.IsNullOrEmpty(_name)) { return "Der Spaltenname ist nicht definiert."; }

        if (!IsValidColumnName(Name)) { return "Der Spaltenname ist ung�ltig."; }

        if (Database.Column.Any(thisColumn => thisColumn != this && thisColumn != null && string.Equals(_name, thisColumn.Name, StringComparison.OrdinalIgnoreCase))) {
            return "Spalten-Name bereits vorhanden.";
        }

        if (string.IsNullOrEmpty(_caption)) { return "Spalten Beschriftung fehlt."; }
        if (!_saveContent && string.IsNullOrEmpty(_identifier)) { return "Inhalt der Spalte muss gespeichert werden."; }
        if (!_saveContent && _showUndo) { return "Wenn der Inhalt der Spalte nicht gespeichert wird, darf auch kein Undo geloggt werden."; }
        if (((int)_format).ToString() == _format.ToString()) { return "Format fehlerhaft."; }
        if (_format.NeedTargetDatabase()) {
            if (LinkedDatabase == null) { return "Verkn�pfte Datenbank fehlt oder existiert nicht."; }
            if (LinkedDatabase == Database) { return "Zirkelbezug mit verkn�pfter Datenbank."; }
        }
        if (!_format.Autofilter_m�glich() && _filterOptions != FilterOptions.None) { return "Bei diesem Format keine Filterung erlaubt."; }
        if (_filterOptions != FilterOptions.None && !_filterOptions.HasFlag(FilterOptions.Enabled)) { return "Filter Kombination nicht m�glich."; }
        if (_filterOptions != FilterOptions.Enabled_OnlyAndAllowed && _filterOptions.HasFlag(FilterOptions.OnlyAndAllowed)) { return "Filter Kombination nicht m�glich."; }
        if (_filterOptions != FilterOptions.Enabled_OnlyOrAllowed && _filterOptions.HasFlag(FilterOptions.OnlyOrAllowed)) { return "Filter Kombination nicht m�glich."; }
        if (_filterOptions.HasFlag(FilterOptions.OnlyAndAllowed) || _filterOptions.HasFlag(FilterOptions.OnlyOrAllowed)) {
            if (!_multiLine) {
                return "Dieser Filter kann nur bei Mehrzeiligen Spalten benutzt werden.";
            }
        }
        switch (_format) {
            case DataFormat.RelationText:
                if (!_multiLine) { return "Bei diesem Format muss mehrzeilig ausgew�hlt werden."; }
                if (_keyColumnKey > -1) { return "Diese Format darf keine Verkn�pfung zu einer Schl�sselspalte haben."; }
                if (IsFirst()) { return "Diese Format ist bei der ersten (intern) erste Spalte nicht erlaubt."; }
                if (!string.IsNullOrEmpty(_cellInitValue)) { return "Diese Format kann keinen Initial-Text haben."; }
                if (_vorschlagsColumn > 0) { return "Diese Format kann keine Vorschlags-Spalte haben."; }
                break;

            //case DataFormat.Verkn�pfung_zu_anderer_Datenbank_Skriptgesteuert:
            //    //case DataFormat.Verkn�pfung_zu_anderer_Datenbank:
            //    if (!string.IsNullOrEmpty(_cellInitValue)) { return "Dieses Format kann keinen Initial-Text haben."; }
            //    if (_keyColumnKey > -1) { return "Dieses Format darf keine Verkn�pfung zu einer Schl�sselspalte haben."; }
            //    if (IsFirst()) { return "Dieses Format ist bei der ersten (intern) erste Spalte nicht erlaubt."; }
            //    if (_linkedCellRowKeyIsInColumn is < 0 and not (-9999)) { return "Die Angabe der Spalte, aus der der Schl�sselwert geholt wird, fehlt."; }
            //    if (_vorschlagsColumn > 0) { return "Dieses Format kann keine Vorschlags-Spalte haben."; }
            //    if (_linkedCellRowKeyIsInColumn >= 0) {
            //        var c = LinkedDatabase().Column.SearchByKey(_linkedCell_ColumnKeyOfLinkedDatabase);
            //        if (c == null) { return "Die verkn�pfte Spalte existiert nicht."; }
            //        //this.GetStyleFrom(c);
            //        //BildTextVerhalten = c.BildTextVerhalten;

            //        //_MultiLine = c.MultiLine;// != ) { return "Multiline stimmt nicht mit der Ziel-Spalte Multiline �berein"; }
            //        //} else {
            //        //    if (!_MultiLine) { return "Dieses Format muss mehrzeilig sein, da es von der Ziel-Spalte gesteuert wird."; }
            //    }
            //    break;

            case DataFormat.Werte_aus_anderer_Datenbank_als_DropDownItems:
                //Develop.DebugPrint("Values_f�r_LinkedCellDropdown Verwendung bei:" + Database.Filename); //TODO: 29.07.2021 Values_f�r_LinkedCellDropdown Format entfernen
                if (!string.IsNullOrEmpty(_cellInitValue)) { return "Dieses Format kann keinen Initial-Text haben."; }
                if (KeyColumnKey > -1) { return "Dieses Format darf keine Verkn�pfung zu einer Schl�sselspalte haben."; }
                if (_vorschlagsColumn > 0) { return "Dieses Format kann keine Vorschlags-Spalte haben."; }
                break;

                //case DataFormat.Link_To_Filesystem:
                //    if (!string.IsNullOrEmpty(_cellInitValue)) { return "Dieses Format kann keinen Initial-Text haben."; }
                //    if (_multiLine && !_afterEditQuickSortRemoveDouble) { return "Format muss sortiert werden."; }
                //    if (_vorschlagsColumn > 0) { return "Dieses Format kann keine Vorschlags-Spalte haben."; }
                //    if (!string.IsNullOrEmpty(_autoRemove)) { return "Dieses Format darf keine Autokorrektur-Ma�nahmen haben haben."; }
                //    if (_afterEditAutoCorrect) { return "Dieses Format darf keine Autokorrektur-Ma�nahmen haben haben."; }
                //    if (_afterEditDoUCase) { return "Dieses Format darf keine Autokorrektur-Ma�nahmen haben haben."; }
                //    if (_afterEditRunden != -1) { return "Dieses Format darf keine Autokorrektur-Ma�nahmen haben haben."; }
                //    if (AfterEditAutoReplace.Count > 0) { return "Dieses Format darf keine Autokorrektur-Ma�nahmen haben haben."; }
                //    break;
        }

        if (_multiLine) {
            if (!_format.MultilinePossible()) { return "Format unterst�tzt keine mehrzeiligen Texte."; }
            if (_afterEditRunden != -1) { return "Runden nur bei einzeiligen Texten m�glich"; }
        } else {
            if (_showMultiLineInOneLine) { return "Wenn mehrzeilige Texte einzeilig dargestellt werden sollen, muss mehrzeilig angew�hlt sein."; }
            if (_afterEditQuickSortRemoveDouble) { return "Sortierung kann nur bei mehrzeiligen Feldern erfolgen."; }
        }
        //if (_SpellCheckingEnabled && !_Format.SpellCheckingPossible()) { return "Rechtschreibpr�fung bei diesem Format nicht m�glich."; }
        if (_editAllowedDespiteLock && !_textBearbeitungErlaubt && !_dropdownBearbeitungErlaubt) { return "Wenn die Zeilensperre ignoriert werden soll, muss eine Bearbeitungsmethode definiert sein."; }
        var tmpEditDialog = UserEditDialogTypeInTable(_format, false, true, _multiLine);
        if (_textBearbeitungErlaubt) {
            if (tmpEditDialog == EditTypeTable.Dropdown_Single) { return "Format unterst�tzt nur Dropdown-Men�."; }
            if (tmpEditDialog == EditTypeTable.None) { return "Format unterst�tzt keine Standard-Bearbeitung."; }
        } else {
            if (_vorschlagsColumn > -1) { return "'Vorschlags-Text-Spalte' nur bei Texteingabe m�glich."; }
            //if (!string.IsNullOrEmpty(_AllowedChars)) { return "'Erlaubte Zeichen' nur bei Texteingabe n�tig."; }
        }
        if (_dropdownBearbeitungErlaubt) {
            //if (_SpellCheckingEnabled) { return "Entweder Dropdownmen� oder Rechtschreibpr�fung."; }
            if (tmpEditDialog == EditTypeTable.None) { return "Format unterst�tzt keine Auswahlmen�-Bearbeitung."; }
        }
        if (!_dropdownBearbeitungErlaubt && !_textBearbeitungErlaubt) {
            if (_permissionGroupsChangeCell.Count > 0) { return "Bearbeitungsberechtigungen entfernen, wenn keine Bearbeitung erlaubt ist."; }
        }
        if (!string.IsNullOrEmpty(_cellInitValue)) {
            if (IsFirst()) { return "Die erste Spalte darf keinen InitialWert haben."; }
            if (_vorschlagsColumn > -1) { return "InitialWert und Vorschlagspalten-Initial-Text gemeinsam nicht m�glich"; }
        }
        foreach (var thisS in _permissionGroupsChangeCell) {
            if (thisS.Contains("|")) { return "Unerlaubtes Zeichen bei den Gruppen, die eine Zelle bearbeiten d�rfen."; }
            if (thisS.ToUpper() == "#ADMINISTRATOR") { return "'#Administrator' bei den Bearbeitern entfernen."; }
        }
        if (_dropdownBearbeitungErlaubt || tmpEditDialog == EditTypeTable.Dropdown_Single) {
            if (_format != DataFormat.Werte_aus_anderer_Datenbank_als_DropDownItems) {
                if (!_dropdownWerteAndererZellenAnzeigen && _dropDownItems.Count == 0) { return "Keine Dropdown-Items vorhanden bzw. Alles hinzuf�gen nicht angew�hlt."; }
            }
        } else {
            if (_dropdownWerteAndererZellenAnzeigen) { return "Dropdownmenu nicht ausgew�hlt, 'alles hinzuf�gen' pr�fen."; }
            if (_dropdownAllesAbw�hlenErlaubt) { return "Dropdownmenu nicht ausgew�hlt, 'alles abw�hlen' pr�fen."; }
            if (_dropDownItems.Count > 0) { return "Dropdownmenu nicht ausgew�hlt, Dropdown-Items vorhanden."; }
        }
        if (_dropdownWerteAndererZellenAnzeigen && !_format.DropdownItemsOfOtherCellsAllowed()) { return "'Dropdownmenu alles hinzuf�gen' bei diesem Format nicht erlaubt."; }
        if (_dropdownAllesAbw�hlenErlaubt && !_format.DropdownUnselectAllAllowed()) { return "'Dropdownmenu alles abw�hlen' bei diesem Format nicht erlaubt."; }
        if (_dropDownItems.Count > 0 && !_format.DropdownItemsAllowed()) { return "Manuelle 'Dropdow-Items' bei diesem Format nicht erlaubt."; }
        if (_behaviorOfImageAndText != BildTextVerhalten.Nur_Text) {
            //if (_Format is DataFormat.Text) {
            //    // Performance-Teschnische Gr�nde
            //    _BildTextVerhalten = BildTextVerhalten.Nur_Text;
            //    //return "Bei diesem Format muss das Bild/Text-Verhalten 'Nur Text' sein.";
            //}
        }
        if (!string.IsNullOrEmpty(_suffix)) {
            if (_multiLine) { return "Einheiten und Mehrzeilig darf nicht kombiniert werden."; }
        }
        if (_afterEditRunden > 6) { return "Beim Runden maximal 6 Nachkommastellen m�glich"; }
        if (_filterOptions == FilterOptions.None) {
            if (!string.IsNullOrEmpty(_autoFilterJoker)) { return "Wenn kein Autofilter erlaubt ist, immer anzuzeigende Werte entfernen"; }
        }
        //if (string.IsNullOrEmpty(_linkedKeyKennung) && _format.NeedLinkedKeyKennung()) { return "Spaltenkennung f�r verlinkte Datenbanken fehlt."; }
        if (_opticalReplace.Count > 0) {
            if (_format is not DataFormat.Text and
                not DataFormat.RelationText) { return "Format unterst�tzt keine Ersetzungen."; }
            if (_filterOptions.HasFlag(FilterOptions.ExtendedFilterEnabled)) { return "Entweder 'Ersetzungen' oder 'erweiternden Autofilter'"; }
            if (!string.IsNullOrEmpty(_autoFilterJoker)) { return "Entweder 'Ersetzungen' oder 'Autofilter Joker'"; }
        }
        if (_keyColumnKey > -1) {
            if (!string.IsNullOrEmpty(Am_A_Key_For_Other_Column)) { return "Eine Schl�sselspalte darf selbst keine Verkn�pfung zu einer anderen Spalte haben: " + Am_A_Key_For_Other_Column; }
            var c = Database.Column.SearchByKey(_keyColumnKey);
            if (c == null) { return "Die verkn�pfte Schl�sselspalte existiert nicht."; }
        }
        if (IsFirst()) {
            if (_keyColumnKey > -1) { return "Die (intern) erste Spalte darf keine Verkn�pfung zu einer andern Schl�sselspalte haben."; }
        }
        if (_format is not DataFormat.Verkn�pfung_zu_anderer_Datenbank and not DataFormat.Werte_aus_anderer_Datenbank_als_DropDownItems) {
            //if (_LinkedCell_RowKeyIsInColumn > -1) { return "Nur verlinkte Zellen k�nnen Daten �ber verlinkte Zellen enthalten."; }
            if (_linkedCell_ColumnKeyOfLinkedDatabase > -1) { return "Nur verlinkte Zellen k�nnen Daten �ber verlinkte Zellen enthalten."; }
        }
        return string.Empty;
    }

    public bool ExportableTextformatForLayout() => _format.ExportableForLayout();

    public List<string> GetUcaseNamesSortedByLenght() {
        if (UcaseNamesSortedByLenght != null) { return UcaseNamesSortedByLenght; }
        var tmp = Contents((FilterCollection)null, null);
        for (var z = 0; z < tmp.Count; z++) {
            tmp[z] = tmp[z].Length.ToString(Constants.Format_Integer10) + tmp[z].ToUpper();
        }
        tmp.Sort();
        for (var z = 0; z < tmp.Count; z++) {
            tmp[z] = tmp[z].Substring(10);
        }
        UcaseNamesSortedByLenght = tmp;
        return UcaseNamesSortedByLenght;
    }

    //public void Parse(string ToParse) {
    //    Develop.DebugPrint(enFehlerArt.Fehler, "Kann nur �ber die Datenbank geparsed werden.");
    //}
    //public object Clone()
    //{
    //    if (!IsOk())
    //    {
    //        Develop.DebugPrint(enFehlerArt.Fehler, "Quell-Spalte fehlerhaft:\r\nQuelle: " + Name + "\r\nFehler: " + ErrorReason());
    //    }
    //    return new ColumnItem(this, false);
    //}
    public void GetUniques(List<RowItem?> rows, out List<string> einzigartig, out List<string> nichtEinzigartig) {
        einzigartig = new List<string>();
        nichtEinzigartig = new List<string>();
        foreach (var thisRow in rows) {
            if (thisRow != null) {
                var tmp = MultiLine ? thisRow.CellGetList(this) : new List<string> { thisRow.CellGetString(this) };
                foreach (var thisString in tmp) {
                    if (einzigartig.Contains(thisString)) {
                        nichtEinzigartig.AddIfNotExists(thisString);
                    } else {
                        einzigartig.AddIfNotExists(thisString);
                    }
                }
            }
        }
        einzigartig.RemoveString(nichtEinzigartig, false);
    }

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
    //            NewReplacer.Add(th.ToString() + "|" + te);
    //            if (th >= ZumDropdownHinzuAb && th < ZumDropdownHinzuBis) {
    //                NewAuswahl.Add(th.ToString());
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

    public int Index() => Database.Column.IndexOf(this);

    /// <summary>
    /// Der Invalidate, der am meisten invalidiert: Alle tempor�ren Variablen und auch jede Zell-Gr��e der Spalte.
    /// </summary>
    public void Invalidate_ColumAndContent() {
        TmpCaptionTextSize = new SizeF(-1, -1);
        Invalidate_ContentWidth();
        Invalidate_TmpVariables();
        foreach (var thisRow in Database.Row) {
            if (thisRow != null) { CellCollection.Invalidate_CellContentSize(this, thisRow); }
        }
        Database.OnViewChanged();
    }

    public bool IsFirst() => Database?.Column[0] == this;

    public bool IsOk() => string.IsNullOrEmpty(ErrorReason());

    public ColumnItem? Next() {
        var columnCount = Index();
        do {
            columnCount++;
            if (columnCount >= Database.Column.Count) { return null; }
            if (Database.Column[columnCount] != null) { return Database.Column[columnCount]; }
        } while (true);
    }

    public void OnChanged() => Changed?.Invoke(this, System.EventArgs.Empty);

    public ColumnItem? Previous() {
        var columnCount = Index();
        do {
            columnCount--;
            if (columnCount < 0) { return null; }
            if (Database?.Column[columnCount] != null) { return Database.Column[columnCount]; }
        } while (true);
    }

    //public bool ZellenZusammenfassen
    //{
    //    get
    //    {
    //        return _ZellenZusammenfassen;
    //    }
    //    set
    //    {
    //        if (_ZellenZusammenfassen == value) { return; }
    //        Database.AddPending(enDatabaseDataType.co_ZellenZusammenfassen, this, _ZellenZusammenfassen.ToPlusMinus(), value.ToPlusMinus(), true);
    //        OnChanged();
    //    }
    //}
    public string QuickInfoText(string additionalText) {
        var T = string.Empty;
        if (!string.IsNullOrEmpty(_quickInfo)) { T += _quickInfo; }
        if (Database.IsAdministrator() && !string.IsNullOrEmpty(_adminInfo)) { T = T + "<br><br><b><u>Administrator-Info:</b></u><br>" + _adminInfo; }
        if (Database.IsAdministrator() && _tags.Count > 0) { T = T + "<br><br><b><u>Spalten-Tags:</b></u><br>" + Tags.JoinWith("<br>"); }
        T = T.Trim();
        T = T.Trim("<br>");
        T = T.Trim();
        if (!string.IsNullOrEmpty(T) && !string.IsNullOrEmpty(additionalText)) {
            T = "<b><u>" + additionalText + "</b></u><br><br>" + T;
        }
        return T;
    }

    public string ReadableText() {
        var ret = _caption;
        if (Database.Column.Any(thisColumnItem => thisColumnItem != null && thisColumnItem != this && string.Equals(thisColumnItem.Caption, _caption, StringComparison.OrdinalIgnoreCase))) {
            var done = false;
            if (!string.IsNullOrEmpty(_captionGroup3)) {
                ret = _captionGroup3 + "/" + ret;
                done = true;
            }
            if (!string.IsNullOrEmpty(_captionGroup2)) {
                ret = _captionGroup2 + "/" + ret;
                done = true;
            }
            if (!string.IsNullOrEmpty(_captionGroup1)) {
                ret = _captionGroup1 + "/" + ret;
                done = true;
            }
            if (!done) {
                ret = _name; //_Caption + " (" + _Name + ")";
            }
        }
        ret = ret.Replace("\n", "\r").Replace("\r\r", "\r");
        var i = ret.IndexOf("-\r", StringComparison.Ordinal);
        if (i > 0 && i < ret.Length - 3) {
            var tzei = ret.Substring(i + 2, 1);
            if (tzei.ToLower() == tzei) {
                ret = ret.Substring(0, i) + ret.Substring(i + 2);
            }
        }
        return ret.Replace("\r", " ").Replace("  ", " ").TrimEnd(":");
    }

    public void Repair() {
        // Unbekannt = -1,
        // Nothing = 0,
        //    Text = 1,

        //Bit = 2,

        //// Bin�rdaten_Bild = 19,
        //// Passwort = 20, // String
        ////  Text_Ohne_Kritische_Zeichen = 21,
        //// Bin�rdaten = 39,
        //// Link_To_BlueDataSystem = 42
        //// Telefonnummer = 43, // Spezielle Formate
        //FarbeInteger = 45, // Color

        //// Email = 46, // Spezielle Formate
        //// InternetAdresse = 47, // Spezielle Formate
        //// Relation = 65,
        //// Event = 66,
        //// Tendenz = 67
        //// Einsch�tzung = 68,
        //Schrift = 69,

        //Text_mit_Formatierung = 70,

        //// TextmitFormatierungUndLinkToAnotherDatabase = 71
        //// Relation_And_Event_Mixed = 72,
        //Link_To_Filesystem = 73,

        //LinkedCell = 74,
        //Columns_f�r_LinkedCellDropdown = 75,

        //Values_f�r_LinkedCellDropdown = 76,

        //RelationText = 77,

        //// KeyForSame = 78
        //Button = 79

        //// bis 999 wird gepr�ft
        ///

        CheckIfIAmAKeyColumn();

        try {
            switch ((int)_format) {
                case (int)DataFormat.Button:
                    ScriptType = ScriptType.Nicht_vorhanden;
                    break;

                case 21: //Text_Ohne_Kritische_Zeichen
                    SetFormatForText();
                    break;

                case 15:// Date_GermanFormat = 15
                case 16://Datum_und_Uhrzeit = 16,
                    SetFormatForDateTime();
                    break;

                case 2:   //Bit = 3,
                    SetFormatForBit();
                    break;

                case 3:   //Ganzzahl = 3,
                    SetFormatForInteger();
                    break;

                case 6:  //Gleitkommazahl = 6,
                    SetFormatForFloat();
                    break;

                case 13:         //BildCode = 13,
                    SetFormatForBildCode();
                    break;

                case 70:
                    SetFormatForTextMitFormatierung();
                    break;

                case 73: // Link To Filesystem
                    SetFormatForText();
                    break;

                case 74: //(int)DataFormat.Verkn�pfung_zu_anderer_Datenbank_Skriptgesteuert:

                    //if (LinkedCell_RowKeyIsInColumn != -9999) {
                    _format = DataFormat.Verkn�pfung_zu_anderer_Datenbank;
                    _linkedCellFilter.Clear();
                    //LinkedCellFilter.Add(LinkedCell_RowKeyIsInColumn.ToString());
                    //LinkedCell_RowKeyIsInColumn = -1;
                    //}
                    break;

                case 75:
                    _format = DataFormat.Werte_aus_anderer_Datenbank_als_DropDownItems;

                    break;
            }

            if (ScriptType == ScriptType.undefiniert) {
                if (MultiLine) {
                    ScriptType = ScriptType.List;
                } else if (Format is DataFormat.Text) {
                    if (SortType is SortierTyp.ZahlenwertFloat or SortierTyp.ZahlenwertInt) {
                        ScriptType = ScriptType.Numeral;
                    }
                    ScriptType = ScriptType.String;
                } else if (Format == DataFormat.Schrift) {
                    ScriptType = ScriptType.Nicht_vorhanden;
                }
            }

            if (_format is DataFormat.Verkn�pfung_zu_anderer_Datenbank) {
                var c = LinkedDatabase?.Column.SearchByKey(_linkedCell_ColumnKeyOfLinkedDatabase);
                if (c != null) {
                    this.GetStyleFrom(c);
                    BehaviorOfImageAndText = c.BehaviorOfImageAndText;
                    ScriptType = c.ScriptType; // 29.06.2022 Wieder aktivert. Grund: Plananalyse waren zwei vershieden Typen bei den Zeitn. So erschien immer automatisch eine 0 bei den Stnden, und es war nicht ersichtlich warum.
                    DoOpticalTranslation = c.DoOpticalTranslation;
                }
            }

            if (ScriptType == ScriptType.undefiniert) {
                Develop.DebugPrint(FehlerArt.Warnung, "Umsetzung fehlgeschlagen: " + Caption + " " + Database.ConnectionData.TableName);
            }

            ResetSystemToDefault(false);
        } catch (Exception ex) {
            Develop.DebugPrint(ex);
        }
    }

    public void ResetSystemToDefault(bool setAll) {
        if (string.IsNullOrEmpty(_identifier)) { return; }
        //if (SetAll && !IsParsing) { Develop.DebugPrint(enFehlerArt.Warnung, "Ausserhalb des parsens!"); }
        // ACHTUNG: Die SetAll Befehle OHNE _, die m�ssen geloggt werden.
        if (setAll) {
            LineLeft = ColumnLineStyle.D�nn;
            LineRight = ColumnLineStyle.Ohne;
            ForeColor = Color.FromArgb(0, 0, 0);
            //CaptionBitmapCode = null;
        }
        switch (_identifier) {
            case "System: Creator":
                _name = "SYS_Creator";
                _format = DataFormat.Text;
                if (setAll) {
                    Caption = "Ersteller";
                    DropdownBearbeitungErlaubt = true;
                    DropdownWerteAndererZellenAnzeigen = true;
                    SpellCheckingEnabled = false;
                    ForeColor = Color.FromArgb(0, 0, 128);
                    BackColor = Color.FromArgb(185, 186, 255);
                }
                break;

            case "System: Changer":
                _name = "SYS_Changer";
                _format = DataFormat.Text;
                _spellCheckingEnabled = false;
                _textBearbeitungErlaubt = false;
                _dropdownBearbeitungErlaubt = false;
                _showUndo = false;
                _scriptType = ScriptType.String_Readonly;
                _permissionGroupsChangeCell.Clear();
                if (setAll) {
                    Caption = "�nderer";
                    ForeColor = Color.FromArgb(0, 128, 0);
                    BackColor = Color.FromArgb(185, 255, 185);
                }
                break;

            case "System: Chapter":
                _name = "SYS_Chapter";
                _format = DataFormat.Text;
                _afterEditAutoCorrect = true; // Verhindert \r am Ende und somit anzeigefehler
                if (setAll) {
                    Caption = "Kapitel";
                    ForeColor = Color.FromArgb(0, 0, 0);
                    BackColor = Color.FromArgb(255, 255, 150);
                    LineLeft = ColumnLineStyle.Dick;
                    if (_multiLine) { _showMultiLineInOneLine = true; }
                }
                break;

            case "System: Date Created":
                _name = "SYS_DateCreated";
                _spellCheckingEnabled = false;

                if (setAll) {
                    SetFormatForDateTime();
                    Caption = "Erstell-Datum";
                    ForeColor = Color.FromArgb(0, 0, 128);
                    BackColor = Color.FromArgb(185, 185, 255);
                    LineLeft = ColumnLineStyle.Dick;
                }
                break;

            case "System: Date Changed":
                _name = "SYS_DateChanged";
                _spellCheckingEnabled = false;
                _showUndo = false;
                // SetFormatForDateTime(); --Sriptt Type Chaos
                _textBearbeitungErlaubt = false;
                _spellCheckingEnabled = false;
                _dropdownBearbeitungErlaubt = false;
                _scriptType = ScriptType.String_Readonly;
                _permissionGroupsChangeCell.Clear();

                if (setAll) {
                    SetFormatForDateTime();
                    Caption = "�nder-Datum";
                    ForeColor = Color.FromArgb(0, 128, 0);
                    BackColor = Color.FromArgb(185, 255, 185);
                    LineLeft = ColumnLineStyle.Dick;
                }
                break;

            case "System: Correct":
                _name = "SYS_Correct";
                _caption = "Fehlerfrei";
                _spellCheckingEnabled = false;
                _format = DataFormat.Text;
                //_AutoFilterErweitertErlaubt = false;
                _autoFilterJoker = string.Empty;
                //_AutofilterTextFilterErlaubt = false;
                _ignoreAtRowFilter = true;
                _filterOptions = FilterOptions.Enabled;
                _scriptType = ScriptType.Nicht_vorhanden;
                _align = AlignmentHorizontal.Zentriert;
                _dropDownItems.Clear();
                _linkedCellFilter.Clear();
                _permissionGroupsChangeCell.Clear();
                _textBearbeitungErlaubt = false;
                _dropdownBearbeitungErlaubt = false;
                _behaviorOfImageAndText = BildTextVerhalten.Interpretiere_Bool;

                if (setAll) {
                    ForeColor = Color.FromArgb(128, 0, 0);
                    BackColor = Color.FromArgb(255, 185, 185);
                    LineLeft = ColumnLineStyle.Dick;
                }
                break;

            case "System: Locked":
                _name = "SYS_Locked";
                _spellCheckingEnabled = false;
                _format = DataFormat.Text;
                _scriptType = ScriptType.Bool;
                _filterOptions = FilterOptions.Enabled;
                _autoFilterJoker = string.Empty;
                _ignoreAtRowFilter = true;
                _behaviorOfImageAndText = BildTextVerhalten.Interpretiere_Bool;
                _align = AlignmentHorizontal.Zentriert;

                if (_textBearbeitungErlaubt || _dropdownBearbeitungErlaubt) {
                    _quickInfo = "Eine abgeschlossene Zeile kann<br>nicht mehr bearbeitet werden.";
                    _textBearbeitungErlaubt = false;
                    _dropdownBearbeitungErlaubt = true;
                    _editAllowedDespiteLock = true;
                    _dropDownItems.AddIfNotExists("+");
                    _dropDownItems.AddIfNotExists("-");
                } else {
                    _dropDownItems.Clear();
                }

                if (setAll) {
                    Caption = "Abgeschlossen";
                    ForeColor = Color.FromArgb(128, 0, 0);
                    BackColor = Color.FromArgb(255, 185, 185);
                }
                break;

            case "System: State":
                _name = "SYS_RowState";
                _caption = "veraltet und kann gel�scht werden: Zeilenstand";
                _identifier = "";
                break;

            case "System: ID":
                _name = "SYS_ID";
                _caption = "veraltet und kann gel�scht werden: Zeilen-ID";
                _identifier = "";
                break;

            case "System: Last Used Layout":
                _name = "SYS_Layout";
                _caption = "veraltet und kann gel�scht werden:  Letztes Layout";
                _identifier = "";
                break;

            default:
                Develop.DebugPrint("Unbekannte Kennung: " + _identifier);
                break;
        }
    }

    public void SetFormat(VarType format) {
        switch (format) {
            case VarType.Text:
                SetFormatForText();
                break;

            case VarType.Date:
                SetFormatForDate();
                break;

            case VarType.DateTime:
                SetFormatForDateTime();
                break;

            case VarType.Email:
                SetFormatForEmail();
                break;

            case VarType.Float:
                SetFormatForFloat();
                break;

            case VarType.Integer:
                SetFormatForInteger();
                break;

            case VarType.PhoneNumber:
                SetFormatForPhoneNumber();
                break;

            case VarType.TextMitFormatierung:
                SetFormatForTextMitFormatierung();
                break;

            case VarType.Url:
                SetFormatForUrl();
                break;

            case VarType.Bit:
                SetFormatForBit();
                break;

            default:
                Develop.DebugPrint(FehlerArt.Warnung);
                break;
        }
    }

    public void SetFormatForBildCode() {
        ((IInputFormat)this).SetFormat(VarType.Text); // Standard Verhalten

        Format = DataFormat.Text;
        Align = AlignmentHorizontal.Links;
        SortType = SortierTyp.Original_String;
        DoOpticalTranslation = TranslationType.Original_Anzeigen;
        AfterEditQuickSortRemoveDouble = false;
        BehaviorOfImageAndText = BildTextVerhalten.Bild_oder_Text;
        ScriptType = ScriptType.String;
    }

    public void SetFormatForBit() {
        ((IInputFormat)this).SetFormat(VarType.Bit); // Standard Verhalten

        Format = DataFormat.Text;
        Align = AlignmentHorizontal.Zentriert;
        SortType = SortierTyp.Original_String;
        DoOpticalTranslation = TranslationType.Original_Anzeigen;
        AfterEditQuickSortRemoveDouble = false;
        BehaviorOfImageAndText = BildTextVerhalten.Interpretiere_Bool;
        ScriptType = ScriptType.Bool;

        DropdownAllesAbw�hlenErlaubt = false;
        DropdownBearbeitungErlaubt = true;
        TextBearbeitungErlaubt = false;

        var l = new List<string>();
        l.AddRange(DropDownItems);
        l.AddIfNotExists("+");
        l.AddIfNotExists("-");
        DropDownItems = l;
    }

    public void SetFormatForDate() {
        ((IInputFormat)this).SetFormat(VarType.Date);

        Format = DataFormat.Text;
        Align = AlignmentHorizontal.Links;
        SortType = SortierTyp.Datum_Uhrzeit;
        DoOpticalTranslation = TranslationType.Datum;
        AfterEditQuickSortRemoveDouble = false;
        BehaviorOfImageAndText = BildTextVerhalten.Nur_Text;
        ScriptType = ScriptType.DateTime;
    }

    public void SetFormatForDateTime() {
        ((IInputFormat)this).SetFormat(VarType.DateTime);

        Format = DataFormat.Text;
        Align = AlignmentHorizontal.Links;
        SortType = SortierTyp.Datum_Uhrzeit;
        DoOpticalTranslation = TranslationType.Datum;
        AfterEditQuickSortRemoveDouble = false;
        BehaviorOfImageAndText = BildTextVerhalten.Nur_Text;
        ScriptType = ScriptType.DateTime;
    }

    public void SetFormatForEmail() {
        ((IInputFormat)this).SetFormat(VarType.Email);

        Format = DataFormat.Text;
        Align = AlignmentHorizontal.Links;
        SortType = SortierTyp.Original_String;
        DoOpticalTranslation = TranslationType.Original_Anzeigen;
        AfterEditQuickSortRemoveDouble = true;
        BehaviorOfImageAndText = BildTextVerhalten.Nur_Text;
        ScriptType = ScriptType.String;
    }

    public void SetFormatForFloat() {
        ((IInputFormat)this).SetFormat(VarType.Float);

        Format = DataFormat.Text;
        Align = AlignmentHorizontal.Rechts;
        DoOpticalTranslation = TranslationType.Zahl;
        SortType = SortierTyp.ZahlenwertFloat;
        AfterEditQuickSortRemoveDouble = false;
        BehaviorOfImageAndText = BildTextVerhalten.Nur_Text;
        ScriptType = ScriptType.Numeral;
    }

    public void SetFormatForInteger() {
        ((IInputFormat)this).SetFormat(VarType.Integer);

        Format = DataFormat.Text;
        Align = AlignmentHorizontal.Rechts;
        SortType = SortierTyp.ZahlenwertInt;
        DoOpticalTranslation = TranslationType.Original_Anzeigen;
        AfterEditQuickSortRemoveDouble = false;
        BehaviorOfImageAndText = BildTextVerhalten.Nur_Text;
        ScriptType = ScriptType.Numeral;
    }

    public void SetFormatForPhoneNumber() {
        ((IInputFormat)this).SetFormat(VarType.PhoneNumber);

        Format = DataFormat.Text;
        Align = AlignmentHorizontal.Links;
        SortType = SortierTyp.Original_String;
        DoOpticalTranslation = TranslationType.Original_Anzeigen;
        AfterEditQuickSortRemoveDouble = false;
        BehaviorOfImageAndText = BildTextVerhalten.Nur_Text;
        ScriptType = ScriptType.String;
    }

    public void SetFormatForText() {
        ((IInputFormat)this).SetFormat(VarType.Text);

        Format = DataFormat.Text;
        Align = AlignmentHorizontal.Links;
        SortType = SortierTyp.Sprachneutral_String;
        DoOpticalTranslation = TranslationType.Original_Anzeigen;
        AfterEditQuickSortRemoveDouble = false;
        BehaviorOfImageAndText = BildTextVerhalten.Nur_Text;
        ScriptType = ScriptType.String;
    }

    public void SetFormatForTextMitFormatierung() {
        ((IInputFormat)this).SetFormat(VarType.TextMitFormatierung);

        Format = DataFormat.Text;
        Align = AlignmentHorizontal.Links;
        SortType = SortierTyp.Sprachneutral_String;
        DoOpticalTranslation = TranslationType.Original_Anzeigen;
        AfterEditQuickSortRemoveDouble = false;
        BehaviorOfImageAndText = BildTextVerhalten.Nur_Text;
        ScriptType = ScriptType.String;
    }

    public void SetFormatForTextOptions() {
        ((IInputFormat)this).SetFormat(VarType.Text);

        MultiLine = true; // Verhalten von Setformat �berschreiben

        Format = DataFormat.Text;
        Align = AlignmentHorizontal.Links;
        SortType = SortierTyp.Sprachneutral_String;
        DoOpticalTranslation = TranslationType.�bersetzen;
        AfterEditQuickSortRemoveDouble = true;
        BehaviorOfImageAndText = BildTextVerhalten.Nur_Text;
        ScriptType = ScriptType.List;
    }

    public void SetFormatForUrl() {
        ((IInputFormat)this).SetFormat(VarType.Url);

        Format = DataFormat.Text;
        Align = AlignmentHorizontal.Links;
        SortType = SortierTyp.Original_String;
        DoOpticalTranslation = TranslationType.Original_Anzeigen;
        AfterEditQuickSortRemoveDouble = false;
        BehaviorOfImageAndText = BildTextVerhalten.Nur_Text;
        ScriptType = ScriptType.String;
    }

    //public string SimplyFile(string fullFileName) {
    //    if (_format != DataFormat.Link_To_Filesystem) {
    //        Develop.DebugPrint(FehlerArt.Fehler, "Nur bei Link_To_Filesystem erlaubt!");
    //    }
    //    var tmpfile = fullFileName.FileNameWithoutSuffix();
    //    if (string.Equals(tmpfile, fullFileName, StringComparison.OrdinalIgnoreCase)) { return tmpfile; }
    //    if (string.Equals(BestFile(tmpfile, false), fullFileName, StringComparison.OrdinalIgnoreCase)) { return tmpfile; }
    //    tmpfile = fullFileName.FileNameWithSuffix();
    //    return string.Equals(tmpfile, fullFileName, StringComparison.OrdinalIgnoreCase)
    //        ? tmpfile
    //        : string.Equals(BestFile(tmpfile, false), fullFileName, StringComparison.OrdinalIgnoreCase) ? tmpfile : fullFileName;
    //}

    public void Statisik(List<FilterItem>? filter, List<RowItem?>? pinnedRows) {
        if (Database == null) { return; }
        var r = Database.Row.CalculateVisibleRows(filter, pinnedRows);

        if (r == null || r.Count < 1) { return; }

        var d = new Dictionary<string, int>();

        foreach (var thisRow in r) {
            var keyValue = thisRow.CellGetString(this);
            if (string.IsNullOrEmpty(keyValue)) { keyValue = "[empty]"; }

            keyValue = keyValue.Replace("\r", ";");

            var count = 0;
            if (d.ContainsKey(keyValue)) {
                d.TryGetValue(keyValue, out count);
                d.Remove(keyValue);
            }
            count++;
            d.Add(keyValue, count);
        }

        var l = new List<string> {
            "Statisik der vorkommenden Werte der Spalte: " + ReadableText(),
            " - nur aktuell angezeigte Zeilen",
            " - Zelleninhalte werden als ganzes behandelt",
            " "
        };

        do {
            var maxCount = 0;
            var keyValue = string.Empty;

            foreach (var thisKey in d.Where(thisKey => thisKey.Value > maxCount)) {
                keyValue = thisKey.Key;
                maxCount = thisKey.Value;
            }

            d.Remove(keyValue);
            l.Add(maxCount + " - " + keyValue);
        } while (d.Count > 0);

        l.Save(TempFile(string.Empty, string.Empty, "txt"), System.Text.Encoding.UTF8, true);
    }

    public double? Summe(FilterCollection? filter) {
        double summ = 0;
        foreach (var thisrow in Database.Row) {
            if (thisrow != null && thisrow.MatchesTo(filter)) {
                if (!thisrow.CellIsNullOrEmpty(this)) {
                    if (!thisrow.CellGetString(this).IsDouble()) { return null; }
                    summ += thisrow.CellGetDouble(this);
                }
            }
        }
        return summ;
    }

    public double? Summe(List<RowData?> sort) {
        double summ = 0;
        foreach (var thisrow in sort) {
            if (thisrow != null && !thisrow.Row.CellIsNullOrEmpty(this)) {
                if (!thisrow.Row.CellGetString(this).IsDouble()) { return null; }
                summ += thisrow.Row.CellGetDouble(this);
            }
        }
        return summ;
    }

    public QuickImage? SymbolForReadableText() => this == Database.Column.SysRowChanger
        ? QuickImage.Get(ImageCode.Person)
        : this == Database.Column.SysRowCreator
            ? QuickImage.Get(ImageCode.Person)
            : _format switch {
                DataFormat.RelationText => QuickImage.Get(ImageCode.Herz, 16),
                DataFormat.FarbeInteger => QuickImage.Get(ImageCode.Pinsel, 16),
                DataFormat.Verkn�pfung_zu_anderer_Datenbank => QuickImage.Get(ImageCode.Fernglas, 16),
                DataFormat.Button => QuickImage.Get(ImageCode.Kugel, 16),
                _ => _format.TextboxEditPossible()
                    ? _multiLine ? QuickImage.Get(ImageCode.Textfeld, 16, Color.Red, Color.Transparent) : QuickImage.Get(ImageCode.Textfeld)
                    : QuickImage.Get("Pfeil_Unten_Scrollbar|14|||||0")
            };

    public bool UserEditDialogTypeInFormula(EditTypeFormula editTypeToCheck) {
        switch (_format) {
            case DataFormat.Text:
            case DataFormat.RelationText:
                if (editTypeToCheck == EditTypeFormula.Textfeld) { return true; } // Textfeld immer erlauben auch wenn beide Bearbeitungen nicht erlaubt sind. Einfach der �bersichtlichktei
                if (_multiLine && editTypeToCheck == EditTypeFormula.Textfeld_mit_Auswahlknopf) { return false; }
                if (_dropdownBearbeitungErlaubt && editTypeToCheck == EditTypeFormula.Textfeld_mit_Auswahlknopf) { return true; }
                if (_dropdownBearbeitungErlaubt && _dropdownWerteAndererZellenAnzeigen && editTypeToCheck == EditTypeFormula.SwapListBox) { return true; }
                //if (_MultiLine && _DropdownBearbeitungErlaubt && EditType_To_Check == enEditTypeFormula.Listbox_3_Zeilen) { return true; }
                if (_multiLine && _dropdownBearbeitungErlaubt && editTypeToCheck == EditTypeFormula.Listbox) { return true; }
                if (editTypeToCheck == EditTypeFormula.nur_als_Text_anzeigen) { return true; }
                if (!_multiLine && editTypeToCheck == EditTypeFormula.Ja_Nein_Knopf && _behaviorOfImageAndText == BildTextVerhalten.Interpretiere_Bool) { return true; }
                return false;

            case DataFormat.Verkn�pfung_zu_anderer_Datenbank:
                if (editTypeToCheck == EditTypeFormula.None) { return true; }
                //if (EditType_To_Check != enEditTypeFormula.Textfeld &&
                //    EditType_To_Check != enEditTypeFormula.nur_als_Text_anzeigen) { return false; }
                if (Database.IsLoading) { return true; }

                //var skriptgesteuert = LinkedCell_RowKeyIsInColumn == -9999;
                //if (skriptgesteuert) {
                //    return editTypeToCheck is enEditTypeFormula.Textfeld or enEditTypeFormula.nur_als_Text_anzeigen;
                //}

                if (LinkedDatabase == null) { return false; }
                if (_linkedCell_ColumnKeyOfLinkedDatabase < 0) { return false; }
                var col = LinkedDatabase.Column.SearchByKey(_linkedCell_ColumnKeyOfLinkedDatabase);
                if (col == null) { return false; }
                return col.UserEditDialogTypeInFormula(editTypeToCheck);

            case DataFormat.Werte_aus_anderer_Datenbank_als_DropDownItems:
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
            case DataFormat.FarbeInteger:
                return editTypeToCheck == EditTypeFormula.Farb_Auswahl_Dialog;

            case DataFormat.Schrift:
                return editTypeToCheck == EditTypeFormula.Font_AuswahlDialog;

            case DataFormat.Button:
                //if (EditType_To_Check == enEditTypeFormula.Button) { return true; }
                return false;

            default:
                Develop.DebugPrint(_format);
                return false;
        }
    }

    public string Verwendung() {
        var t = "<b><u>Verwendung von " + ReadableText() + "</b></u><br>";
        if (!string.IsNullOrEmpty(_identifier)) {
            t += " - Systemspalte<br>";
        }
        return t + Database.Column_UsedIn(this);
    }

    internal EditTypeFormula CheckFormulaEditType(EditTypeFormula toCheck) {
        if (UserEditDialogTypeInFormula(toCheck)) { return toCheck; }// Alles OK!
        for (var z = 0; z <= 999; z++) {
            var w = (EditTypeFormula)z;
            if (w.ToString() != z.ToString()) {
                if (UserEditDialogTypeInFormula(w)) {
                    return w;
                }
            }
        }
        return EditTypeFormula.None;
    }

    internal void CheckIfIAmAKeyColumn() {
        Am_A_Key_For_Other_Column = string.Empty;
        foreach (var thisColumn in Database.Column) {
            if (thisColumn.KeyColumnKey == Key) { Am_A_Key_For_Other_Column = "Spalte " + thisColumn.ReadableText() + " verweist auf diese Spalte"; } // Werte Gleichhalten
                                                                                                                                                      //if (thisColumn.LinkedCell_RowKeyIsInColumn == Key) { Am_A_Key_For_Other_Column = "Spalte " + thisColumn.ReadableText() + " verweist auf diese Spalte"; } // LinkdeCells pflegen
                                                                                                                                                      //if (ThisColumn.LinkedCell_ColumnValueFoundIn == Key) { I_Am_A_Key_For_Other_Column = "Spalte " + ThisColumn.ReadableText() + " verweist auf diese Spalte"; } // LinkdeCells pflegen

            if (thisColumn.Format == DataFormat.Verkn�pfung_zu_anderer_Datenbank) {
                foreach (var thisV in thisColumn._linkedCellFilter) {
                    if (IntTryParse(thisV, out var key)) {
                        if (key == Key) { Am_A_Key_For_Other_Column = "Spalte " + thisColumn.ReadableText() + " verweist auf diese Spalte"; }
                    }
                }
            }
        }
        //if (_format == DataFormat.Columns_f�r_LinkedCellDropdown) { Am_A_Key_For_Other_Column = "Die Spalte selbst durch das Format"; }
    }

    /// <summary>
    /// Wenn sich ein Zelleninhalt ver�ndert hat, muss die Spalte neu berechnet werden.
    /// </summary>
    internal void Invalidate_ContentWidth() => ContentWidth = -1;

    internal void Invalidate_TmpVariables() {
        TmpCaptionTextSize = new SizeF(-1, -1);
        TmpCaptionBitmapCode = null;
        if (_tmpLinkedDatabase != null) {
            //_TMP_LinkedDatabase.RowKeyChanged -= _TMP_LinkedDatabase_RowKeyChanged;
            //_TMP_LinkedDatabase.ColumnKeyChanged -= _TMP_LinkedDatabase_ColumnKeyChanged;
            _tmpLinkedDatabase.ConnectedControlsStopAllWorking -= _TMP_LinkedDatabase_ConnectedControlsStopAllWorking;
            _tmpLinkedDatabase.Cell.CellValueChanged -= _TMP_LinkedDatabase_Cell_CellValueChanged;
            _tmpLinkedDatabase.Disposing -= _TMP_LinkedDatabase_Disposing;
            _tmpLinkedDatabase = null;
        }
        ContentWidth = -1;
    }

    internal string ParsableColumnKey() => ColumnCollection.ParsableColumnKey(this);

    /// <summary>
    /// Setzt den Wert in die dazugeh�rige Variable.
    /// </summary>
    /// <param name="type"></param>
    /// <param name="newvalue"></param>
    /// <returns></returns>
    internal string SetValueInternal(DatabaseDataType type, string newvalue) {
        //Develop.CheckStackForOverflow();

        switch (type) {
            case DatabaseDataType.ColumnKey:
                _key = LongParse(newvalue);
                Invalidate_TmpVariables();
                break;

            case DatabaseDataType.ColumnName:
                _name = newvalue;
                Invalidate_TmpVariables();
                break;

            case DatabaseDataType.ColumnCaption:
                _caption = newvalue;
                break;

            case DatabaseDataType.ColumnFormat:
                _format = (DataFormat)IntParse(newvalue);
                break;

            case DatabaseDataType.ForeColor:
                _foreColor = Color.FromArgb(IntParse(newvalue));
                break;

            case DatabaseDataType.BackColor:
                _backColor = Color.FromArgb(IntParse(newvalue));
                break;

            case DatabaseDataType.LineStyleLeft:
                _lineLeft = (ColumnLineStyle)IntParse(newvalue);
                break;

            case DatabaseDataType.LineStyleRight:
                _lineRight = (ColumnLineStyle)IntParse(newvalue);
                break;

            case DatabaseDataType.ColumnQuickInfo:
                _quickInfo = newvalue;
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

            case DatabaseDataType.ColumnIdentify:
                _identifier = newvalue;
                ResetSystemToDefault(false);
                Database.Column.GetSystems();
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

            case DatabaseDataType.OpticalTextReplace:
                _opticalReplace.SplitAndCutByCr(newvalue);
                break;

            case DatabaseDataType.AutoReplaceAfterEdit:
                _afterEditAutoReplace.SplitAndCutByCr(newvalue);
                break;

            case DatabaseDataType.RegexCheck:
                _regex = newvalue;
                break;

            case DatabaseDataType.ColumnTags:
                _tags.SplitAndCutByCr(newvalue);
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

            case DatabaseDataType.FilterOptions:
                _filterOptions = (FilterOptions)IntParse(newvalue);
                break;

            case DatabaseDataType.IgnoreAtRowFilter:
                _ignoreAtRowFilter = newvalue.FromPlusMinus();
                break;

            case DatabaseDataType.ShowUndo:
                _showUndo = newvalue.FromPlusMinus();
                break;

            case DatabaseDataType.ShowMultiLineInOneLine:
                _showMultiLineInOneLine = newvalue.FromPlusMinus();
                break;

            case DatabaseDataType.EditableWithTextInput:
                _textBearbeitungErlaubt = newvalue.FromPlusMinus();
                break;

            case DatabaseDataType.EditableWithDropdown:
                _dropdownBearbeitungErlaubt = newvalue.FromPlusMinus();
                break;

            case DatabaseDataType.SpellCheckingEnabled:
                _spellCheckingEnabled = newvalue.FromPlusMinus();
                break;

            case DatabaseDataType.DropdownDeselectAllAllowed:
                _dropdownAllesAbw�hlenErlaubt = newvalue.FromPlusMinus();
                break;

            case DatabaseDataType.ShowValuesOfOtherCellsInDropdown:
                _dropdownWerteAndererZellenAnzeigen = newvalue.FromPlusMinus();
                break;

            case DatabaseDataType.SortAndRemoveDoubleAfterEdit:
                _afterEditQuickSortRemoveDouble = newvalue.FromPlusMinus();
                break;

            case DatabaseDataType.RoundAfterEdit:
                _afterEditRunden = IntParse(newvalue);
                break;

            case DatabaseDataType.DoUcaseAfterEdit:
                _afterEditDoUCase = newvalue.FromPlusMinus();
                break;

            case DatabaseDataType.AutoCorrectAfterEdit:
                _afterEditAutoCorrect = newvalue.FromPlusMinus();
                break;

            case DatabaseDataType.SaveContent:
                _saveContent = newvalue.FromPlusMinus();
                break;

            case DatabaseDataType.AutoRemoveCharAfterEdit:
                _autoRemove = newvalue;
                break;

            case DatabaseDataType.ColumnAdminInfo:
                _adminInfo = newvalue;
                break;

            case DatabaseDataType.ColumnTimeCode:
                _timecode = newvalue;
                break;

            case DatabaseDataType.ColumnContentWidth:
                _contentwidth = IntParse(newvalue);
                break;

            case DatabaseDataType.CaptionBitmapCode:
                _captionBitmapCode = newvalue;
                break;

            case DatabaseDataType.Suffix:
                _suffix = newvalue;
                break;

            case DatabaseDataType.LinkedDatabase:
                _linkedDatabaseFile = newvalue;
                break;

            case DatabaseDataType.ConstantHeightOfImageCode:
                if (newvalue == "0") { newvalue = string.Empty; }
                _constantHeightOfImageCode = newvalue;
                break;

            case DatabaseDataType.Prefix:
                _prefix = newvalue;
                break;

            case DatabaseDataType.DoOpticalTranslation:
                _doOpticalTranslation = (TranslationType)IntParse(newvalue);
                break;

            case DatabaseDataType.AdditionalFormatCheck:
                _additionalFormatCheck = (AdditionalCheck)IntParse(newvalue);
                break;

            case DatabaseDataType.ScriptType:
                _scriptType = (ScriptType)IntParse(newvalue);
                break;

            case DatabaseDataType.BehaviorOfImageAndText:
                _behaviorOfImageAndText = (BildTextVerhalten)IntParse(newvalue);
                break;

            case DatabaseDataType.EditAllowedDespiteLock:
                _editAllowedDespiteLock = newvalue.FromPlusMinus();
                break;

            case DatabaseDataType.TextFormatingAllowed:
                _formatierungErlaubt = newvalue.FromPlusMinus();
                break;

            case DatabaseDataType.CellInitValue:
                _cellInitValue = newvalue;
                break;

            case DatabaseDataType.KeyColumnKey:
                _keyColumnKey = LongParse(newvalue);
                break;

            case DatabaseDataType.ColumnKeyOfLinkedDatabase:
                _linkedCell_ColumnKeyOfLinkedDatabase = LongParse(newvalue);
                break;

            case DatabaseDataType.SortType:
                if (string.IsNullOrEmpty(newvalue)) {
                    _sortType = SortierTyp.Original_String;
                } else {
                    _sortType = (SortierTyp)LongParse(newvalue);
                }
                break;

            case DatabaseDataType.MakeSuggestionFromSameKeyColumn:
                _vorschlagsColumn = LongParse(newvalue);
                break;

            case DatabaseDataType.ColumnAlign:
                var tmpalign = (AlignmentHorizontal)IntParse(newvalue);
                if (tmpalign == (AlignmentHorizontal)(-1)) { tmpalign = AlignmentHorizontal.Links; }
                _align = tmpalign;
                break;

            default:
                if (!string.Equals(type.ToString(), ((int)type).ToString(), StringComparison.Ordinal)) {
                    return "Interner Fehler: F�r den Datentyp  '" + type + "'  wurde keine Laderegel definiert.";
                }
                break;
        }
        return string.Empty;
    }

    private void _TMP_LinkedDatabase_Cell_CellValueChanged(object sender, CellEventArgs e) {
        var tKey = CellCollection.KeyOfCell(e.Column, e.Row);
        foreach (var thisRow in Database.Row) {
            if (Database.Cell.GetStringBehindLinkedValue(this, thisRow) == tKey) {
                CellCollection.Invalidate_CellContentSize(this, thisRow);
                Invalidate_ContentWidth();
                Database.Cell.OnCellValueChanged(new CellEventArgs(this, thisRow));
                thisRow.DoAutomatic(true, false, 5, "value changed");
            }
        }
    }

    //private void _TMP_LinkedDatabase_ColumnKeyChanged(object sender, KeyChangedEventArgs e) {
    //    Database.BlockReload(false);
    //    if (_Format != DataFormat.Columns_f�r_LinkedCellDropdown) {
    //        var os = e.KeyOld.ToString();
    //        var ns = e.KeyNew.ToString();
    //        foreach (var ThisRow in Database.Row) {
    //            if (Database.Cell.GetStringBehindLinkedValue(this, ThisRow) == os) {
    //                Database.Cell.SetValueBehindLinkedValue(this, ThisRow, ns);
    //            }
    //        }
    //    }
    //    if (_Format != DataFormat.LinkedCell) {
    //        var os = e.KeyOld.ToString() + "|";
    //        var ns = e.KeyNew.ToString() + "|";
    //        foreach (var ThisRow in Database.Row) {
    //            var val = Database.Cell.GetStringBehindLinkedValue(this, ThisRow);
    //            if (val.StartsWith(os)) {
    //                Database.Cell.SetValueBehindLinkedValue(this, ThisRow, val.Replace(os, ns));
    //            }
    //        }
    //    }
    //}

    private void _TMP_LinkedDatabase_ConnectedControlsStopAllWorking(object sender, MultiUserFileStopWorkingEventArgs e) => Database.OnConnectedControlsStopAllWorking(this, e);

    //public bool IsParsing {
    //    get {
    //        Develop.DebugPrint(enFehlerArt.Fehler, "Kann nur �ber die Datenbank geparsed werden.");
    //        return false;
    //    }
    //}
    private void _TMP_LinkedDatabase_Disposing(object sender, System.EventArgs e) {
        Invalidate_TmpVariables();
        Database.Dispose();
    }

    //private void _TMP_LinkedDatabase_RowKeyChanged(object sender, KeyChangedEventArgs e) {
    //    if (_Format != DataFormat.LinkedCell) {
    //        var os = "|" + e.KeyOld.ToString();
    //        var ns = "|" + e.KeyNew.ToString();
    //        foreach (var ThisRow in Database.Row) {
    //            var val = Database.Cell.GetStringBehindLinkedValue(this, ThisRow);
    //            if (val.EndsWith(os)) {
    //                Database.Cell.SetValueBehindLinkedValue(this, ThisRow, val.Replace(os, ns));
    //            }
    //        }
    //    }
    //}

    private void Database_Disposing(object sender, System.EventArgs e) => Dispose();

    private void Dispose(bool disposing) {
        if (!IsDisposed) {
            if (disposing) {
                // TODO: Verwalteten Zustand (verwaltete Objekte) bereinigen
            }
            Database.Disposing -= Database_Disposing;
            Invalidate_TmpVariables();
            Database = null;

            //DropDownItems.Changed -= DropDownItems_ListOrItemChanged;
            //DropDownItems.Dispose();

            //LinkedCellFilter.Changed -= LinkedCellFilters_ListOrItemChanged;
            //LinkedCellFilter.Dispose();

            //Tags.Changed -= Tags_ListOrItemChanged;
            //Tags.Dispose();

            //PermissionGroupsChangeCell.Changed -= PermissionGroups_ChangeCell_ListOrItemChanged;
            //PermissionGroupsChangeCell.Dispose();

            //OpticalReplace.Changed -= OpticalReplacer_ListOrItemChanged;
            //OpticalReplace.Dispose();

            //AfterEditAutoReplace.Changed -= AfterEdit_AutoReplace_ListOrItemChanged;
            //AfterEditAutoReplace.Dispose();
            // TODO: Nicht verwaltete Ressourcen (nicht verwaltete Objekte) freigeben und Finalizer �berschreiben
            // TODO: Gro�e Felder auf NULL setzen
            IsDisposed = true;
        }
    }

    private string KleineFehlerCorrect(string txt) {
        if (string.IsNullOrEmpty(txt)) { return string.Empty; }
        const char h4 = (char)1004; // H4 = Normaler Text, nach links rutschen
        const char h3 = (char)1003; // �berschrift
        const char h2 = (char)1002; // �berschrift
        const char h1 = (char)1001; // �berschrift
        const char h7 = (char)1007; // bold
        if (FormatierungErlaubt) { txt = txt.HtmlSpecialToNormalChar(false); }
        string oTxt;
        do {
            oTxt = txt;
            if (oTxt.ToLower().Contains(".at")) { break; }
            if (oTxt.ToLower().Contains(".de")) { break; }
            if (oTxt.ToLower().Contains(".com")) { break; }
            if (oTxt.ToLower().Contains("http")) { break; }
            if (oTxt.ToLower().Contains("ftp")) { break; }
            if (oTxt.ToLower().Contains(".xml")) { break; }
            if (oTxt.ToLower().Contains(".doc")) { break; }
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
        if (FormatierungErlaubt) {
            txt = txt.CreateHtmlCodes(true);
            txt = txt.Replace("<br>", "\r");
        }
        return txt;
    }

    #endregion
}