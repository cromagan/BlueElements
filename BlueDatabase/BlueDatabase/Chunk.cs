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

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using BlueBasics;
using BlueBasics.Enums;
using BlueBasics.Interfaces;
using BlueDatabase.Enums;
using static BlueBasics.Extensions;
using static BlueBasics.Generic;
using static BlueBasics.IO;
using static BlueBasics.Converter;
using System.Threading;
using System.Diagnostics;

namespace BlueDatabase;

[EditorBrowsable(EditorBrowsableState.Never)]
public class Chunk : IHasKeyName {

    #region Fields

    public readonly string MainFileName = string.Empty;

    private List<byte> _bytes = new List<byte>();

    private string _fileinfo = string.Empty;

    private string _keyname = string.Empty;
    private DateTime _lastcheck = DateTime.MinValue;

    #endregion

    #region Constructors

    public Chunk(string mainFileName, string chunkId) {
        MainFileName = mainFileName;
        KeyName = chunkId;
    }

    #endregion

    #region Properties

    public byte[] Bytes => _bytes.ToArray();

    public string ChunkFileName {
        get {
            if (IsMain) { return MainFileName; }

            var folder = MainFileName.FilePath();
            var databasename = MainFileName.FileNameWithoutSuffix();

            return $"{folder}{databasename}\\{KeyName}.bdbc";
        }
    }

    public long DataLenght => _bytes?.Count ?? 0;

    public bool IsMain => string.Equals(KeyName, DatabaseChunk.Chunk_MainData, StringComparison.OrdinalIgnoreCase);

    public string KeyName {
        get => _keyname;
        private set {
            _keyname = value.ToLower();
        }
    }

    public string LastEditApp { get; private set; } = string.Empty;

    public string LastEditMachineName { get; private set; } = string.Empty;

    public DateTime LastEditTimeUtc { get; private set; } = DateTime.MinValue;

    public string LastEditUser { get; private set; } = string.Empty;

    public bool LoadFailed { get; set; } = false;

    public bool SaveRequired { get; set; } = false;

    #endregion

    #region Methods

    public static (List<byte> bytes, string fileinfo, bool failed) LoadBytesFromDisk(string filename) {
        var startTime = DateTime.UtcNow;

        while (true) {
            try {
                var fileinfo = GetFileInfo(filename, true);
                var bLoaded = File.ReadAllBytes(filename);
                if (bLoaded.IsZipped()) { bLoaded = bLoaded.UnzipIt(); }
                return (bLoaded.ToList(), fileinfo, false);
            } catch (Exception ex) {
                // Home Office kann lange blockieren....
                if (DateTime.UtcNow.Subtract(startTime).TotalSeconds > 300) {
                    Develop.DebugPrint(ErrorType.Error, "Die Datei<br>" + filename + "<br>konnte trotz mehrerer Versuche nicht geladen werden.<br><br>Die Fehlermeldung lautet:<br>" + ex.Message);

                    return ([], string.Empty, true);
                }
            }

            Pause(0.5, false);
        }
    }

    public bool DataOk(int minLen) {
        if (LoadFailed) { return true; }

        return _bytes.Count >= minLen;
    }

    /// <summary>
    /// Initialisiert die Byteliste
    /// </summary>
    public void InitByteList() {
        LoadFailed = false;

        _lastcheck = DateTime.UtcNow;

        _bytes.Clear();
    }

    /// <summary>
    /// Diese Routine lädt die Datei von der Festplatte. Zur Not wartet sie bis zu 5 Minuten.
    /// Hier wird auch nochmal geprüft, ob ein Laden überhaupt möglich ist.
    /// Es kann auch NULL zurück gegeben werden, wenn es ein Reload ist und die Daten inzwischen aktuell sind.
    /// </summary>
    /// <param name="checkmode"></param>
    /// <returns></returns>
    public void LoadBytesFromDisk() {
        var c = ChunkFileName;

        if (!FileExists(c)) {
            InitByteList();
            return;
        }

        _lastcheck = DateTime.UtcNow;
        (_bytes, _fileinfo, LoadFailed) = LoadBytesFromDisk(c);
        ParseLockData();
    }

    /// <summary>
    ///
    /// </summary>
    /// <param name="value"></param>
    /// <param name="important">Steuert, ob es dringen nötig ist, dass auch auf Aktualität geprüft wird</param>
    /// <returns></returns>
    public bool NeedsReload(bool important) {
        if (LoadFailed) { return true; }
        if (string.IsNullOrEmpty(MainFileName)) { return false; } // Temporäre Datenbanken

        if (DateTime.UtcNow.Subtract(_lastcheck).TotalMinutes > 3 || important) {
            var nf = GetFileInfo(ChunkFileName, false);
            return nf != _fileinfo;
        }

        return false;
    }

    public bool Save(string filename, int minBytes) {
        if (!DataOk(minBytes)) { return false; }

        try {
            Develop.SetUserDidSomething();

            var head = GetHead();
            var datacompressed = head.Concat(_bytes).ToArray().ZipIt();
            if (datacompressed is not { }) { return false; }

            Develop.SetUserDidSomething();

            using FileStream x = new(filename, FileMode.Create, FileAccess.Write, FileShare.None);
            x.Write(datacompressed, 0, datacompressed.Length);
            x.Flush();
            x.Close();

            Develop.SetUserDidSomething();
        } catch { return false; }

        return true;
    }

    public void SaveToByteList(ColumnItem column, RowItem row) {
        if (LoadFailed) { return; }
        if (column.Database is not { IsDisposed: false } db) { return; }

        var cellContent = db.Cell.GetStringCore(column, row);
        if (string.IsNullOrEmpty(cellContent)) { return; }

        _bytes.Add((byte)Routinen.CellFormatUTF8_V403);

        var columnKeyByte = column.KeyName.UTF8_ToByte();
        SaveToByteList(_bytes, columnKeyByte.Length, 1);
        _bytes.AddRange(columnKeyByte);

        var rowKeyByte = row.KeyName.UTF8_ToByte();
        SaveToByteList(_bytes, rowKeyByte.Length, 1);
        _bytes.AddRange(rowKeyByte);

        var cellContentByte = cellContent.UTF8_ToByte();
        SaveToByteList(_bytes, cellContentByte.Length, 2);
        _bytes.AddRange(cellContentByte);
    }

    public void SaveToByteList(DatabaseDataType databaseDataType, string content, string columnname) {
        if (LoadFailed) { return; }
        _bytes.Add((byte)Routinen.ColumnUTF8_V401);
        _bytes.Add((byte)databaseDataType);

        var n = columnname.UTF8_ToByte();
        SaveToByteList(_bytes, n.Length, 1);
        _bytes.AddRange(n);

        var b = content.UTF8_ToByte();
        SaveToByteList(_bytes, b.Length, 3);
        _bytes.AddRange(b);
    }

    public void SaveToByteList(List<byte> bytes, DatabaseDataType databaseDataType, string content) {
        if (LoadFailed) { return; }
        var b = content.UTF8_ToByte();
        bytes.Add((byte)Routinen.DatenAllgemeinUTF8);
        bytes.Add((byte)databaseDataType);
        SaveToByteList(bytes, b.Length, 3);
        bytes.AddRange(b);
    }

    public void SaveToByteList(DatabaseDataType databaseDataType, string content) {
        if (LoadFailed) { return; }
        var b = content.UTF8_ToByte();
        _bytes.Add((byte)Routinen.DatenAllgemeinUTF8);
        _bytes.Add((byte)databaseDataType);
        SaveToByteList(_bytes, b.Length, 3);
        _bytes.AddRange(b);
    }

    public void SaveToByteList(List<byte> bytes, long numberToAdd, int byteCount) {
        if (LoadFailed) { return; }
        do {
            byteCount--;
            var te = (long)Math.Pow(255, byteCount);
            // ReSharper disable once PossibleLossOfFraction
            var mu = (byte)Math.Truncate((decimal)(numberToAdd / te));

            bytes.Add(mu);
            numberToAdd %= te;
        } while (byteCount > 0);
    }

    /// <summary>
    /// Alle Spaltendaten außer Systeminfo
    /// </summary>
    /// <param name="c"></param>
    public void SaveToByteList(ColumnItem c) {
        if (LoadFailed) { return; }

        var name = c.KeyName;

        SaveToByteList(DatabaseDataType.ColumnName, c.KeyName, name);
        SaveToByteList(DatabaseDataType.ColumnCaption, c.Caption, name);
        SaveToByteList(DatabaseDataType.ColumnFunction, ((int)c.Function).ToString(), name);
        SaveToByteList(DatabaseDataType.DefaultRenderer, c.DefaultRenderer, name);
        SaveToByteList(DatabaseDataType.RendererSettings, c.RendererSettings, name);
        SaveToByteList(DatabaseDataType.CaptionGroup1, c.CaptionGroup1, name);
        SaveToByteList(DatabaseDataType.CaptionGroup2, c.CaptionGroup2, name);
        SaveToByteList(DatabaseDataType.CaptionGroup3, c.CaptionGroup3, name);
        SaveToByteList(DatabaseDataType.MultiLine, c.MultiLine.ToPlusMinus(), name);
        //SaveToByteList(l, DatabaseDataType.CellInitValue, c.CellInitValue, name);
        SaveToByteList(DatabaseDataType.SortAndRemoveDoubleAfterEdit, c.AfterEditQuickSortRemoveDouble.ToPlusMinus(), name);
        SaveToByteList(DatabaseDataType.DoUcaseAfterEdit, c.AfterEditDoUCase.ToPlusMinus(), name);
        SaveToByteList(DatabaseDataType.AutoCorrectAfterEdit, c.AfterEditAutoCorrect.ToPlusMinus(), name);
        SaveToByteList(DatabaseDataType.RoundAfterEdit, c.RoundAfterEdit.ToString(), name);
        SaveToByteList(DatabaseDataType.MaxCellLenght, c.MaxCellLenght.ToString(), name);
        SaveToByteList(DatabaseDataType.FixedColumnWidth, c.FixedColumnWidth.ToString(), name);
        SaveToByteList(DatabaseDataType.AutoRemoveCharAfterEdit, c.AutoRemove, name);
        //SaveToByteList(l, DatabaseDataType.SaveContent, c.SaveContent.ToPlusMinus(), name);
        SaveToByteList(DatabaseDataType.FilterOptions, ((int)c.FilterOptions).ToString(), name);
        SaveToByteList(DatabaseDataType.AutoFilterJoker, c.AutoFilterJoker, name);
        SaveToByteList(DatabaseDataType.IgnoreAtRowFilter, c.IgnoreAtRowFilter.ToPlusMinus(), name);
        SaveToByteList(DatabaseDataType.EditableWithTextInput, c.TextBearbeitungErlaubt.ToPlusMinus(), name);
        SaveToByteList(DatabaseDataType.SpellCheckingEnabled, c.SpellCheckingEnabled.ToPlusMinus(), name);
        SaveToByteList(DatabaseDataType.ShowUndo, c.ShowUndo.ToPlusMinus(), name);
        SaveToByteList(DatabaseDataType.TextFormatingAllowed, c.FormatierungErlaubt.ToPlusMinus(), name);
        SaveToByteList(DatabaseDataType.ForeColor, c.ForeColor.ToArgb().ToString(), name);
        SaveToByteList(DatabaseDataType.BackColor, c.BackColor.ToArgb().ToString(), name);
        SaveToByteList(DatabaseDataType.LineStyleLeft, ((int)c.LineLeft).ToString(), name);
        SaveToByteList(DatabaseDataType.LineStyleRight, ((int)c.LineRight).ToString(), name);
        SaveToByteList(DatabaseDataType.EditableWithDropdown, c.DropdownBearbeitungErlaubt.ToPlusMinus(), name);
        SaveToByteList(DatabaseDataType.DropDownItems, c.DropDownItems.JoinWithCr(), name);
        SaveToByteList(DatabaseDataType.LinkedCellFilter, c.LinkedCellFilter.JoinWithCr(), name);
        //SaveToByteList(l, DatabaseDataType.OpticalTextReplace, c.OpticalReplace.JoinWithCr(), name);
        SaveToByteList(DatabaseDataType.AutoReplaceAfterEdit, c.AfterEditAutoReplace.JoinWithCr(), name);
        SaveToByteList(DatabaseDataType.RegexCheck, c.Regex, name);
        SaveToByteList(DatabaseDataType.DropdownDeselectAllAllowed, c.DropdownAllesAbwählenErlaubt.ToPlusMinus(), name);
        SaveToByteList(DatabaseDataType.ShowValuesOfOtherCellsInDropdown, c.DropdownWerteAndererZellenAnzeigen.ToPlusMinus(), name);
        SaveToByteList(DatabaseDataType.ColumnQuickInfo, c.QuickInfo, name);
        SaveToByteList(DatabaseDataType.ColumnAdminInfo, c.AdminInfo, name);
        //SaveToByteList(DatabaseDataType.ColumnSystemInfo, c.SystemInfo, name);
        //SaveToByteList(l, DatabaseDataType.ColumnContentWidth, c.ContentWidth.ToString(), name);
        SaveToByteList(DatabaseDataType.CaptionBitmapCode, c.CaptionBitmapCode, name);
        SaveToByteList(DatabaseDataType.AllowedChars, c.AllowedChars, name);
        SaveToByteList(DatabaseDataType.MaxTextLenght, c.MaxTextLenght.ToString(), name);
        SaveToByteList(DatabaseDataType.PermissionGroupsChangeCell, c.PermissionGroupsChangeCell.JoinWithCr(), name);
        SaveToByteList(DatabaseDataType.ColumnTags, c.Tags.JoinWithCr(), name);
        SaveToByteList(DatabaseDataType.EditAllowedDespiteLock, c.EditAllowedDespiteLock.ToPlusMinus(), name);
        SaveToByteList(DatabaseDataType.LinkedDatabase, c.LinkedDatabaseTableName, name);
        //SaveToByteList(l, DatabaseDataType.ConstantHeightOfImageCode, c.ConstantHeightOfImageCode, name);
        //SaveToByteList(l, DatabaseDataType.BehaviorOfImageAndText, ((int)c.BehaviorOfImageAndText).ToString(), name);
        SaveToByteList(DatabaseDataType.DoOpticalTranslation, ((int)c.DoOpticalTranslation).ToString(), name);
        SaveToByteList(DatabaseDataType.AdditionalFormatCheck, ((int)c.AdditionalFormatCheck).ToString(), name);
        SaveToByteList(DatabaseDataType.ScriptType, ((int)c.ScriptType).ToString(), name);
        //SaveToByteList(l, DatabaseDataType.KeyColumnKey, column.KeyColumnKey.ToString(false), key);
        SaveToByteList(DatabaseDataType.ColumnNameOfLinkedDatabase, c.LinkedCell_ColumnNameOfLinkedDatabase, name);
        //SaveToByteList(l, DatabaseDataType.MakeSuggestionFromSameKeyColumn, column.VorschlagsColumn.ToString(false), key);
        SaveToByteList(DatabaseDataType.ColumnAlign, ((int)c.Align).ToString(), name);
        SaveToByteList(DatabaseDataType.SortType, ((int)c.SortType).ToString(), name);
        //SaveToByteList(l, DatabaseDataType.ColumnTimeCode, column.TimeCode, key);
    }

    /// <summary>
    /// Wartet bis zu 120 Sekunden, bis die Initallladung ausgeführt wurde
    /// </summary>
    public void WaitInitialDone() {
        var t = Stopwatch.StartNew();

        var x = 0;

        while (_lastcheck.Year < 2000) {
            Thread.Sleep(1);
            if (t.ElapsedMilliseconds > 1200000) {
                Develop.MonitorMessage?.Invoke("Chunk-Laden", "", $"Abbruch, Chunk wurde nicht richtig initialisiert", 0);
                return;
            }
            x += 1;
            if (x > 5000) {
                x = 0;
                Develop.MonitorMessage?.Invoke("Chunk-Laden", "", $"Warte auf Abschluss der Initialsierung", 0);
            }
        }
    }

    internal bool Delete() {
        _bytes.Clear();
        string filename = ChunkFileName;
        return DeleteFile(filename, false);
    }

    internal bool DoExtendedSave(int minbytes) {
        string filename = ChunkFileName;

        string backup = filename.FilePath() + filename.FileNameWithoutSuffix() + ".bak";
        string tempfile = TempFile(filename.FilePath() + filename.FileNameWithoutSuffix() + ".tmp-" + UserName.ToUpperInvariant());

        if (!Save(tempfile, minbytes)) { return false; }
        if (FileExists(backup)) {
            if (!DeleteFile(backup, false)) { return false; }
        }
        Develop.SetUserDidSomething();
        // Haupt-Datei wird zum Backup umbenannt
        MoveFile(filename, backup, false); // Kein Abbruch hier, die Datei könnte ja nicht existieren
        Develop.SetUserDidSomething();

        // --- TmpFile wird zum Haupt ---
        const int maxRetries = 5;
        const int retryDelayMs = 1000;

        for (int attempt = 1; attempt <= maxRetries; attempt++) {
            if (MoveFile(tempfile, filename, false)) {
                Develop.SetUserDidSomething();
                _lastcheck = DateTime.UtcNow;
                _fileinfo = GetFileInfo(ChunkFileName, true);
                return true;
            }

            // Paralleler Prozess hat gespeichert?!?
            Develop.SetUserDidSomething();
            if (FileExists(filename)) {
                _ = DeleteFile(tempfile, false);
                Develop.SetUserDidSomething();
                return false;
            }

            if (attempt < maxRetries) {
                Thread.Sleep(retryDelayMs);
                continue;
            }

            Develop.DebugPrint(ErrorType.Error, $"Chunk defekt nach {maxRetries} Versuchen:\r\n{filename}\r\n{tempfile}");
            return false;
        }

        return false;
    }

    internal string IsEditable(EditableErrorReasonType reason) {
        if (NeedsReload(false)) { return "Daten müssen neu geladen werden."; }

        if (DateTime.UtcNow.Subtract(LastEditTimeUtc).TotalMinutes < 2) {
            if (LastEditUser != UserName) { return $"Aktueller Bearbeiter: {LastEditUser}"; }
            if (LastEditApp != Develop.AppExe()) { return $"Anderes Programm bearbeitet: {LastEditApp}"; }
            if (LastEditMachineName != Environment.MachineName) { return $"Anderes Computer bearbeitet: {LastEditMachineName}"; }
            return string.Empty;
        }

        if (reason is not EditableErrorReasonType.EditAcut and not EditableErrorReasonType.EditCurrently) { return string.Empty; }

        if (!DoExtendedSave(0)) {
            LastEditTimeUtc = DateTime.MinValue;
            return "Bearbeitung konnte nicht gesetzt werden.";
        }

        return string.Empty;
    }

    internal void SaveToByteList(RowItem thisRow) {
        if (LoadFailed) { return; }
        if (thisRow.Database is not { } db) { return; }

        foreach (var thisColumn in db.Column) {
            if (thisColumn.Function != ColumnFunction.Virtuelle_Spalte) {
                SaveToByteList(thisColumn, thisRow);
            }
        }
    }

    internal void SaveToByteListEOF() {
        SaveToByteList(DatabaseDataType.EOF, "END");
    }

    private List<byte> GetHead() {
        if (LoadFailed) { return []; }

        var headBytes = new List<byte>();

        // Zuerst Werte setzen
        LastEditTimeUtc = DateTime.UtcNow;
        LastEditUser = UserName;
        LastEditApp = Develop.AppExe();
        LastEditMachineName = Environment.MachineName;

        // Dann die Werte zur ByteList hinzufügen
        SaveToByteList(headBytes, DatabaseDataType.Version, Database.DatabaseVersion);
        SaveToByteList(DatabaseDataType.Werbung, "                                                                    BlueDataBase - (c) by Christian Peter                                                                                        ");

        SaveToByteList(headBytes, DatabaseDataType.LastEditTimeUTC, LastEditTimeUtc.ToString5());
        SaveToByteList(headBytes, DatabaseDataType.LastEditUser, LastEditUser);
        SaveToByteList(headBytes, DatabaseDataType.LastEditApp, LastEditApp);
        SaveToByteList(headBytes, DatabaseDataType.LastEditMachineName, LastEditMachineName);

        return headBytes;
    }

    private void ParseLockData() {
        int pointer = 0;
        var data = _bytes.ToArray();
        var filename = ChunkFileName;

        while (pointer < data.Length) {
            var (newPointer, type, value, _, _) = Database.Parse(data, pointer, filename);
            pointer = newPointer;

            switch (type) {
                case DatabaseDataType.LastEditTimeUTC:
                    LastEditTimeUtc = DateTimeParse(value);
                    break;

                case DatabaseDataType.LastEditUser:
                    LastEditUser = value;
                    break;

                case DatabaseDataType.LastEditApp:
                    LastEditApp = value;
                    break;

                case DatabaseDataType.LastEditMachineName:
                    LastEditMachineName = value;
                    break;

                case DatabaseDataType.Werbung:
                    return;
            }
        }
    }

    #endregion
}