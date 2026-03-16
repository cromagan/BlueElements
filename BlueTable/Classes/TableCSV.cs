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
            var csvContent = ExportCSV(_separator, _firstLineIsHeader);
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

        var colsToRemove = new List<ColumnItem>();
        foreach (var c in Column) {
            if (!c.IsDisposed) {
                colsToRemove.Add(c);
            }
        }
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
            columnNames = ParseCSVLine(lines[0], _separator);
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
            var firstLineFields = ParseCSVLine(lines[0], _separator);
            for (var i = 0; i < firstLineFields.Count; i++) {
                var colName = "Column" + i.ToString();
                var col = Column[colName];
                if (col == null) {
                    col = Column.GenerateAndAdd(colName);
                }
            }
        }

        for (var lineIndex = startLine; lineIndex < lines.Length; lineIndex++) {
            var fields = ParseCSVLine(lines[lineIndex], _separator);
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

    #endregion
}