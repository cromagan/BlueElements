// Authors:
// Christian Peter
//
// Copyright © 2026 Christian Peter
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

using BlueBasics;
using BlueBasics.Classes.FileSystemCaching;
using BlueBasics.ClassesStatic;
using BlueBasics.Enums;
using BlueTable.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlueTable.Classes;

[Browsable(false)]
[EditorBrowsable(EditorBrowsableState.Never)]
public class TableCSV : TableFile {

    #region Fields

    private CachedTextFile? _cachedTextFile;
    private bool _firstLineIsHeader = true;
    private char _separator = ';';

    #endregion

    #region Constructors

    public TableCSV(string tablename) : base(tablename) { }

    #endregion

    #region Destructors

    ~TableCSV() {
        Dispose(false);
    }

    #endregion

    #region Properties

    public bool FirstLineIsHeader {
        get => _firstLineIsHeader;
        set {
            if (_firstLineIsHeader == value) { return; }
            _firstLineIsHeader = value;
            IsDirty = true;
        }
    }

    public char Separator {
        get => _separator;
        set {
            if (_separator == value) { return; }
            _separator = value;
            IsDirty = true;
        }
    }

    protected override bool SaveRequired => base.SaveRequired || (_cachedTextFile != null && !_cachedTextFile.IsSaved);

    #endregion

    #region Methods

    public override bool BeSureToBeUpToDate(bool firstTime) {
        if (!base.BeSureToBeUpToDate(firstTime)) { return false; }

        if (string.IsNullOrEmpty(Filename)) { return true; }

        if (_cachedTextFile == null) {
            _cachedTextFile = CachedFileSystem.GetOrCreate<CachedTextFile>(Filename);
        }

        if (_cachedTextFile == null) { return false; }

        if (_cachedTextFile.IsStale()) {
            DropMessage(ErrorType.Info, $"CSV-Datei wurde geändert, lade neu: {KeyName}");
            _cachedTextFile.Invalidate();
            return LoadCSVFromCachedFile();
        }

        return true;
    }

    public override string GrantWriteAccess(TableDataType type, string? chunkValue) {
        var f = base.GrantWriteAccess(type, chunkValue);
        if (!string.IsNullOrEmpty(f)) { return f; }

        if (_cachedTextFile == null) {
            _cachedTextFile = CachedFileSystem.GetOrCreate<CachedTextFile>(Filename);
        }

        if (_cachedTextFile == null) { return "CachedTextFile konnte nicht erstellt werden."; }

        return string.Empty;
    }

    public void SetSeparator(char separator) {
        _separator = separator;
    }

    protected override void Dispose(bool disposing) {
        if (IsDisposed) { return; }
        base.Dispose(disposing);
    }

    protected override bool LoadMainData() {
        if (string.IsNullOrEmpty(Filename)) { return false; }

        _cachedTextFile = CachedFileSystem.GetOrCreate<CachedTextFile>(Filename);

        if (_cachedTextFile == null) {
            Freeze("CachedTextFile konnte nicht erstellt werden.");
            return false;
        }

        return LoadCSVFromCachedFile();
    }

    protected override async Task<string> SaveInternal(DateTime setfileStateUtcDateTo) {
        if (!SaveRequired) { return string.Empty; }

        var f = IsGenericEditable(false);
        if (!string.IsNullOrEmpty(f)) { return f; }

        Develop.SetUserDidSomething();
        DropMessage(ErrorType.DevelopInfo, $"Speichere CSV-Datei '{Caption}'");

        try {
            var csvContent = GenerateCSVContent();
            if (string.IsNullOrEmpty(csvContent)) {
                return "Fehler beim Generieren des CSV-Inhalts";
            }

            var bytes = csvContent.UTF8_ToByte();

            if (_cachedTextFile == null) {
                _cachedTextFile = CachedFileSystem.GetOrCreate<CachedTextFile>(Filename);
            }

            if (_cachedTextFile == null) {
                return "CachedTextFile konnte nicht erstellt werden.";
            }

            _cachedTextFile.Content = bytes;
            var result = await _cachedTextFile.Save().ConfigureAwait(false);

            if (result.IsFailed) {
                return result.FailedReason ?? "Speichern fehlgeschlagen";
            }

            LastSaveMainFileUtcDate = setfileStateUtcDateTo;
            IsDirty = false;
            OnInvalidateView();

            return string.Empty;
        } catch (Exception ex) {
            return ex.Message;
        }
    }

    private string EscapeCSVField(string field) {
        if (string.IsNullOrEmpty(field)) { return string.Empty; }

        var needsQuoting = field.Contains(_separator) ||
                           field.Contains("\"") ||
                           field.Contains("\r") ||
                           field.Contains("\n");

        if (!needsQuoting) { return field; }

        return "\"" + field.Replace("\"", "\"\"") + "\"";
    }

    private List<string> EscapeCSVFields(List<string> fields) {
        var result = new List<string>();
        foreach (var field in fields) {
            result.Add(EscapeCSVField(field));
        }
        return result;
    }

    private string GenerateCSVContent() {
        var sb = new StringBuilder();

        var columnNames = new List<string>();
        foreach (var col in Column) {
            if (!col.IsDisposed && col.SaveContent) {
                columnNames.Add(col.KeyName);
            }
        }

        if (columnNames.Count == 0) { return string.Empty; }

        if (_firstLineIsHeader) {
            var headerFields = EscapeCSVFields(columnNames);
            sb.AppendLine(string.Join(_separator.ToString(), headerFields));
        }

        foreach (var row in Row) {
            if (row.IsDisposed) { continue; }

            var fields = new List<string>();
            foreach (var colName in columnNames) {
                var col = Column[colName];
                if (col == null || col.IsDisposed) {
                    fields.Add(string.Empty);
                } else {
                    fields.Add(row.CellGetString(col));
                }
            }

            var escapedFields = EscapeCSVFields(fields);
            sb.AppendLine(string.Join(_separator.ToString(), escapedFields));
        }

        return sb.ToString();
    }

    private bool LoadCSVFromCachedFile() {
        if (_cachedTextFile == null) { return false; }

        if (!_cachedTextFile.EnsureContentLoaded()) {
            Freeze("CSV-Datei konnte nicht geladen werden.");
            return false;
        }

        OnLoading();

        Undo.Clear();
        Row.RemoveNullOrEmpty();
        Cell.Clear();

        var colsToRemove = Column.Where(c => !c.IsDisposed).ToList();
        foreach (var col in colsToRemove) {
            Column.Remove(col, "CSV-Neuladen");
        }

        var content = _cachedTextFile.GetContentAsString(Encoding.UTF8);

        if (!ParseCSVContent(content)) {
            Freeze("CSV-Parsen fehlgeschlagen!");
            return false;
        }

        Column.GetSystems();
        RepairAfterParse();

        IsDirty = false;
        MainChunkLoadDone = true;

        OnLoaded(true, true);

        DropMessage(ErrorType.Info, $"CSV-Datei geladen: {KeyName}");
        return true;
    }

    private bool ParseCSVContent(string content) {
        if (string.IsNullOrEmpty(content)) { return true; }

        content = content.Replace("\r\n", "\r").Replace("\n", "\r").Trim("\r".ToCharArray());
        var lines = content.SplitAndCutByCr();

        if (lines.Length == 0) { return true; }

        var startLine = 0;
        List<string> columnNames = [];

        if (_firstLineIsHeader) {
            columnNames = ParseCSVLine(lines[0]);
            startLine = 1;

            for (var i = 0; i < columnNames.Count; i++) {
                var colName = ColumnItem.MakeValidColumnName(columnNames[i]);
                if (string.IsNullOrEmpty(colName)) {
                    colName = "Column" + i.ToString();
                }

                var col = Column[colName];
                if (col == null) {
                    col = Column.GenerateAndAdd(colName);
                    if (col != null) {
                        col.Caption = columnNames[i];
                    }
                }
            }
        } else {
            var firstLineFields = ParseCSVLine(lines[0]);
            for (var i = 0; i < firstLineFields.Count; i++) {
                var colName = "Column" + i.ToString();
                var col = Column[colName];
                if (col == null) {
                    col = Column.GenerateAndAdd(colName);
                }
            }
        }

        for (var lineIndex = startLine; lineIndex < lines.Length; lineIndex++) {
            var fields = ParseCSVLine(lines[lineIndex]);
            if (fields.Count == 0) { continue; }

            var rowKey = fields.Count > 0 ? fields[0] : Guid.NewGuid().ToString();
            var row = Row.GenerateAndAdd(rowKey, "CSV-Import");

            if (row == null) { continue; }

            var colIndex = 0;
            foreach (var col in Column) {
                if (col.IsDisposed) { continue; }

                if (colIndex < fields.Count) {
                    row.CellSet(col, fields[colIndex], "CSV-Import");
                }
                colIndex++;
            }
        }

        return true;
    }

    private List<string> ParseCSVLine(string line) {
        var result = new List<string>();
        var currentField = new StringBuilder();
        var inQuotes = false;

        for (var i = 0; i < line.Length; i++) {
            var c = line[i];

            if (inQuotes) {
                if (c == '"') {
                    if (i + 1 < line.Length && line[i + 1] == '"') {
                        currentField.Append('"');
                        i++;
                    } else {
                        inQuotes = false;
                    }
                } else {
                    currentField.Append(c);
                }
            } else {
                if (c == '"') {
                    inQuotes = true;
                } else if (c == _separator) {
                    result.Add(currentField.ToString());
                    currentField.Clear();
                } else {
                    currentField.Append(c);
                }
            }
        }

        result.Add(currentField.ToString());
        return result;
    }

    #endregion
}