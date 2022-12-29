// Authors:
// Christian Peter
//
// Copyright (c) 2023 Christian Peter
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
using BlueDatabase.Enums;
using BlueDatabase.EventArgs;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using static BlueBasics.Converter;
using BlueBasics.MultiUserFile;
using static BlueBasics.Generic;
using static BlueBasics.IO;

namespace BlueDatabase;

[Browsable(false)]
[EditorBrowsable(EditorBrowsableState.Never)]
public sealed class Database : DatabaseAbstract {

    #region Fields

    public ListExt<WorkItem>? Works;

    #endregion

    #region Constructors

    public Database(Stream stream, string tablename) : this(stream, string.Empty, true, false, tablename) { }

    public Database(bool readOnly, string tablename) : this(null, string.Empty, readOnly, true, tablename) { }

    public Database(string filename, bool readOnly, bool create, string tablename) : this(null, filename, readOnly, create, tablename) { }

    private Database(Stream? stream, string filename, bool readOnly, bool create, string tablename) : base(tablename, readOnly) {
        AllFiles.Add(this);

        //_muf.Loaded += OnLoaded;
        //_muf.Loading += OnLoading;
        //_muf.SavedToDisk += OnSavedToDisk;
        //_muf.DiscardPendingChanges += DiscardPendingChanges;
        //_muf.HasPendingChanges += HasPendingChanges;
        //_muf.ParseExternal += Parse;
        //_muf.ToListOfByte += ToListOfByte;

        Develop.StartService();

        Works = new ListExt<WorkItem>();

        Initialize();

        if (!string.IsNullOrEmpty(filename)) {
            //DropConstructorMessage?.Invoke(this, new MessageEventArgs(enFehlerArt.Info, "Lade Datenbank aus Dateisystem: \r\n" + filename.FileNameWithoutSuffix()));
            LoadFromFile(filename, create);
        } else if (stream != null) {
            LoadFromStream(stream);
        }
        //RepairAfterParse();
    }

    #endregion

    #region Properties

    public static string DatabaseID => "BlueDatabase";

    public override ConnectionInfo ConnectionData => new(TableName, this, DatabaseID, Filename);

    //_muf != null ? _muf.Filename : string.Empty;
    public string Filename { get; set; }

    #endregion

    #region Methods

    public override string AdditionaFilesPfadWhole() {
        var x = base.AdditionaFilesPfadWhole();
        if (!string.IsNullOrEmpty(x)) { return x; }

        if (!string.IsNullOrEmpty(Filename)) {
            var t = (Filename.FilePath() + "AdditionaFiles\\").CheckPath();
            if (DirectoryExists(t)) {
                _additionaFilesPfadtmp = t;
                return t;
            }
        }
        _additionaFilesPfadtmp = string.Empty;
        return string.Empty;
    }

    public override List<ConnectionInfo>? AllAvailableTables(List<DatabaseAbstract>? allreadychecked) {
        if (string.IsNullOrWhiteSpace(Filename)) { return null; } // Stream-Datenbank

        if (allreadychecked != null) {
            foreach (var thisa in allreadychecked) {
                if (thisa is Database db) {
                    if (string.Equals(db.Filename.FilePath(), Filename.FilePath())) { return null; }
                }
            }
        }

        var nn = Directory.GetFiles(Filename.FilePath(), "*.mdb", SearchOption.AllDirectories);
        var gb = new List<ConnectionInfo>();
        foreach (var thisn in nn) {
            gb.Add(ConnectionDataOfOtherTable(thisn.FileNameWithoutSuffix(), false));
        }
        return gb;
    }

    public override ConnectionInfo? ConnectionDataOfOtherTable(string tableName, bool checkExists) {
        if (string.IsNullOrEmpty(Filename)) { return null; }

        var f = Filename.FilePath() + tableName.FileNameWithoutSuffix() + ".mdb";

        if (checkExists && !File.Exists(f)) { return null; }

        return new ConnectionInfo(f);
    }

    public bool IsFileAllowedToLoad(string fileName) {
        foreach (var thisFile in AllFiles) {
            if (thisFile is Database db) {
                if (string.Equals(db.Filename, fileName, StringComparison.OrdinalIgnoreCase)) {
                    thisFile.Save();
                    Develop.DebugPrint(FehlerArt.Warnung, "Doppletes Laden von " + fileName);
                    return false;
                }
            }
        }

        return true;
    }

    public void LoadFromFile(string fileNameToLoad, bool createWhenNotExisting) {
        if (string.Equals(fileNameToLoad, Filename, StringComparison.OrdinalIgnoreCase)) { return; }
        if (!string.IsNullOrEmpty(Filename)) { Develop.DebugPrint(FehlerArt.Fehler, "Geladene Dateien können nicht als neue Dateien geladen werden."); }
        if (string.IsNullOrEmpty(fileNameToLoad)) { Develop.DebugPrint(FehlerArt.Fehler, "Dateiname nicht angegeben!"); }
        //fileNameToLoad = modConverter.SerialNr2Path(fileNameToLoad);
        if (!createWhenNotExisting && !CanWriteInDirectory(fileNameToLoad.FilePath())) { SetReadOnly(); }
        if (!IsFileAllowedToLoad(fileNameToLoad)) { return; }
        if (!FileExists(fileNameToLoad)) {
            if (createWhenNotExisting) {
                if (ReadOnly) {
                    Develop.DebugPrint(FehlerArt.Fehler, "Readonly kann keine Datei erzeugen<br>" + fileNameToLoad);
                    return;
                }
                SaveAsAndChangeTo(fileNameToLoad);
            } else {
                Develop.DebugPrint(FehlerArt.Warnung, "Datei existiert nicht: " + fileNameToLoad);  // Readonly deutet auf Backup hin, in einem anderne Verzeichnis (Linked)
                SetReadOnly();
                return;
            }
        }
        Filename = fileNameToLoad;
        //ReCreateWatcher();
        // Wenn ein Dateiname auf Nix gesezt wird, z.B: bei Bitmap import
        if (string.IsNullOrEmpty(Filename)) { return; }

        //LoadingEventArgs ec = new(_initialLoadDone);
        OnLoading();

        var bLoaded = LoadBytesFromDisk(BlueBasics.Enums.ErrorReason.Load);
        if (bLoaded == null) { return; }

        Parse(bLoaded);

        RepairAfterParse();
        OnLoaded();
        CreateWatcher();
    }

    public void LoadFromStream(Stream stream) {
        OnLoading();
        byte[] bLoaded;
        using (BinaryReader r = new(stream)) {
            bLoaded = r.ReadBytes((int)stream.Length);
            r.Close();
        }

        if (bLoaded.Length > 4 && BitConverter.ToInt32(bLoaded, 0) == 67324752) {
            // Gezipte Daten-Kennung gefunden
            bLoaded = MultiUserFile.UnzipIt(bLoaded);
        }

        Parse(bLoaded);

        RepairAfterParse();
        OnLoaded();
        CreateWatcher();
    }

    public void Parse(byte[] bLoaded, ref int pointer, out DatabaseDataType type, ref long colKey, ref long rowKey, out string value, out int width, out int height) {
        int les;
        switch ((Routinen)bLoaded[pointer]) {
            case Routinen.CellFormat: {
                    type = (DatabaseDataType)bLoaded[pointer + 1];
                    les = NummerCode3(bLoaded, pointer + 2);
                    colKey = NummerCode3(bLoaded, pointer + 5);
                    rowKey = NummerCode3(bLoaded, pointer + 8);
                    var b = new byte[les];
                    Buffer.BlockCopy(bLoaded, pointer + 11, b, 0, les);
                    value = b.ToStringWin1252();
                    width = NummerCode2(bLoaded, pointer + 11 + les);
                    height = NummerCode2(bLoaded, pointer + 11 + les + 2);
                    pointer += 11 + les + 4;
                    break;
                }
            case Routinen.CellFormatUTF8: {
                    type = (DatabaseDataType)bLoaded[pointer + 1];
                    les = NummerCode3(bLoaded, pointer + 2);
                    colKey = NummerCode3(bLoaded, pointer + 5);
                    rowKey = NummerCode3(bLoaded, pointer + 8);
                    var b = new byte[les];
                    Buffer.BlockCopy(bLoaded, pointer + 11, b, 0, les);
                    value = b.ToStringUtf8();
                    width = NummerCode2(bLoaded, pointer + 11 + les);
                    height = NummerCode2(bLoaded, pointer + 11 + les + 2);
                    pointer += 11 + les + 4;
                    break;
                }
            case Routinen.CellFormatUTF8_V400: {
                    type = (DatabaseDataType)bLoaded[pointer + 1];
                    les = NummerCode3(bLoaded, pointer + 2);
                    colKey = NummerCode7(bLoaded, pointer + 5);
                    rowKey = NummerCode7(bLoaded, pointer + 12);
                    var b = new byte[les];
                    Buffer.BlockCopy(bLoaded, pointer + 19, b, 0, les);
                    value = b.ToStringUtf8();
                    width = NummerCode2(bLoaded, pointer + 19 + les);
                    height = NummerCode2(bLoaded, pointer + 19 + les + 2);
                    pointer += 19 + les + 4;
                    break;
                }
            case Routinen.DatenAllgemein: {
                    type = (DatabaseDataType)bLoaded[pointer + 1];
                    les = NummerCode3(bLoaded, pointer + 2);
                    colKey = -1;
                    rowKey = -1;
                    var b = new byte[les];
                    Buffer.BlockCopy(bLoaded, pointer + 5, b, 0, les);
                    value = b.ToStringWin1252();
                    width = 0;
                    height = 0;
                    pointer += 5 + les;
                    break;
                }
            case Routinen.DatenAllgemeinUTF8: {
                    type = (DatabaseDataType)bLoaded[pointer + 1];
                    les = NummerCode3(bLoaded, pointer + 2);
                    colKey = -1;
                    rowKey = -1;
                    var b = new byte[les];
                    Buffer.BlockCopy(bLoaded, pointer + 5, b, 0, les);
                    value = b.ToStringUtf8();
                    width = 0;
                    height = 0;
                    pointer += 5 + les;
                    break;
                }
            case Routinen.Column: {
                    type = (DatabaseDataType)bLoaded[pointer + 1];
                    les = NummerCode3(bLoaded, pointer + 2);
                    colKey = NummerCode3(bLoaded, pointer + 5);
                    rowKey = NummerCode3(bLoaded, pointer + 8);
                    var b = new byte[les];
                    Buffer.BlockCopy(bLoaded, pointer + 11, b, 0, les);
                    value = b.ToStringWin1252();
                    width = 0;
                    height = 0;
                    pointer += 11 + les;
                    break;
                }
            case Routinen.ColumnUTF8: {
                    type = (DatabaseDataType)bLoaded[pointer + 1];
                    les = NummerCode3(bLoaded, pointer + 2);
                    colKey = NummerCode3(bLoaded, pointer + 5);
                    rowKey = NummerCode3(bLoaded, pointer + 8);
                    var b = new byte[les];
                    Buffer.BlockCopy(bLoaded, pointer + 11, b, 0, les);
                    value = b.ToStringUtf8();
                    width = 0;
                    height = 0;
                    pointer += 11 + les;
                    break;
                }
            case Routinen.ColumnUTF8_V400: {
                    type = (DatabaseDataType)bLoaded[pointer + 1];
                    les = NummerCode3(bLoaded, pointer + 2);
                    colKey = NummerCode7(bLoaded, pointer + 5);
                    rowKey = NummerCode7(bLoaded, pointer + 12);
                    var b = new byte[les];
                    Buffer.BlockCopy(bLoaded, pointer + 19, b, 0, les);
                    value = b.ToStringUtf8();
                    width = 0;
                    height = 0;
                    pointer += 19 + les;
                    break;
                }
            default: {
                    type = (DatabaseDataType)0;
                    value = string.Empty;
                    width = 0;
                    height = 0;
                    Develop.DebugPrint(FehlerArt.Fehler, "Laderoutine nicht definiert: " + bLoaded[pointer]);
                    break;
                }
        }
    }

    public void Parse(byte[] data) {
        Column.ThrowEvents = false;
        Row.ThrowEvents = false;
        var pointer = 0;

        ColumnItem? column = null;
        RowItem? row = null;

        long colKey = 0;
        long rowKey = 0;

        #region Spalten merken und Variable leeren

        List<ColumnItem?> columnsOld = new();
        columnsOld.AddRange(Column);
        Column.Clear();

        #endregion

        #region Pendings merken und Variable leeren

        //var oldPendings = Works?.Where(thisWork => thisWork.State == ItemState.Pending).ToList();
        Works?.Clear();

        #endregion

        var b = data;

        do {
            if (pointer >= b.Length) { break; }

            Parse(b, ref pointer, out var art, ref colKey, ref rowKey, out var inhalt, out var x, out var y);

            #region Zeile suchen oder erstellen

            if (rowKey > -1) {
                row = Row.SearchByKey(rowKey);
                if (row == null) {
                    //row = Row.GenerateAndAdd(rowKey, string.Empty, false, false);
                    Row.SetValueInternal(DatabaseDataType.Comand_AddRow, rowKey, true);
                    row = Row.SearchByKey(rowKey);
                    row.IsInCache = true;
                }
            }

            #endregion

            #region Spalte suchen oder erstellen

            if (colKey > -1) {
                // Zuerst schauen, ob die Column schon (wieder) in der richtigen Collection ist
                column = Column.SearchByKey(colKey);
                if (column == null) {
                    // Column noch nicht gefunden. Schauen, ob sie vor dem Reload vorhanden war und gg. hinzufügen
                    foreach (var thisColumn in columnsOld) {
                        if (thisColumn != null && thisColumn.Key == colKey) {
                            column = thisColumn;
                        }
                    }
                    if (column != null) {
                        // Prima, gefunden! Noch die Collections korrigieren
                        Column.Add(column);
                        columnsOld.Remove(column);
                    } else {
                        // Nicht gefunden, als neu machen
                        Column.SetValueInternal(DatabaseDataType.Comand_AddColumn, colKey, true);
                        column = Column.SearchByKey(colKey);
                        column.IsInCache = true;
                        //column = Column.GenerateAndAdd(colKey);
                    }
                }
            }

            #endregion

            #region Bei verschlüsselten Datenbanken das Passwort abfragen

            if (art == DatabaseDataType.GlobalShowPass && !string.IsNullOrEmpty(inhalt)) {
                PasswordEventArgs e2 = new();
                OnNeedPassword(e2);
                //b = Cryptography.SimpleCrypt(b, e2.Password, -1, pointer, b.Length - 1);
                if (e2.Password != inhalt) {
                    SetReadOnly();
                    //MessageBox.Show("Zugriff verweigrt, Passwort falsch!", ImageCode.Kritisch, "OK");
                    break;
                }
            }

            #endregion

            var fehler = SetValueInternal(art, inhalt, column?.Key, row?.Key, x, y, true);

            if (art == DatabaseDataType.EOF) { break; }
            if (!string.IsNullOrEmpty(fehler)) {
                SetReadOnly();
                Develop.DebugPrint("Schwerer Datenbankfehler:<br>Version: " + DatabaseVersion + "<br>Datei: " + Filename + "<br>Meldung: " + fehler);
            }
        } while (true);

        Row.RemoveNullOrEmpty();
        Cell.RemoveOrphans();
        //Works?.AddRange(oldPendings);
        //oldPendings?.Clear();
        //ExecutePending();
        Column.ThrowEvents = true;
        Row.ThrowEvents = true;

        FirstColumn = Column[0].Name;

        if (IntParse(LoadedVersion.Replace(".", "")) > IntParse(DatabaseVersion.Replace(".", ""))) { SetReadOnly(); }
    }

    public override void RefreshColumnsData(List<ColumnItem>? columns) {
        if (columns == null || columns.Count == 0) { return; }

        foreach (var thisrow in columns) {
            thisrow.IsInCache = true;
        }
    }

    public override bool RefreshRowData(List<RowItem> row, bool refreshAlways) {
        if (row == null || row.Count == 0) { return false; }

        foreach (var thisrow in row) {
            thisrow.IsInCache = true;
        }

        return false;
    }

    public override bool Save() {
        if (ReadOnly) { return false; }
        if (string.IsNullOrEmpty(Filename)) { return false; }

        if (!HasPendingChanges) { return false; }

        var tmpFileName = WriteTempFileToDisk();

        if (string.IsNullOrEmpty(tmpFileName)) { return false; }

        if (FileExists(Backupdateiname())) {
            if (!DeleteFile(Backupdateiname(), false)) { return false; }
        }

        // Haupt-Datei wird zum Backup umbenannt
        if (!MoveFile(Filename, Backupdateiname(), false)) { return false; }

        // --- TmpFile wird zum Haupt ---
        MoveFile(tmpFileName, Filename, true);

        HasPendingChanges = false;
        return true;
    }

    public void SaveAsAndChangeTo(string newFileName) {
        if (string.Equals(newFileName, Filename, StringComparison.OrdinalIgnoreCase)) { Develop.DebugPrint(FehlerArt.Fehler, "Dateiname unterscheiden sich nicht!"); }
        Save(); // Original-Datei speichern, die ist ja dann weg.
        // Jetzt kann es aber immer noch sein, das PendingChanges da sind.
        // Wenn kein Dateiname angegeben ist oder bei Readonly wird die Datei nicht gespeichert und die Pendings bleiben erhalten!

        Filename = newFileName;

        var l = ToListOfByte();
        using FileStream x = new(newFileName, FileMode.Create, FileAccess.Write, FileShare.None);
        x.Write(l.ToArray(), 0, l.ToArray().Length);
        x.Flush();
        x.Close();
    }

    public override string UndoText(ColumnItem? column, RowItem? row) {
        if (Works == null || Works.Count == 0) { return string.Empty; }
        var cellKey = CellCollection.KeyOfCell(column, row);
        var t = "";
        for (var z = Works.Count - 1; z >= 0; z--) {
            if (Works[z] != null && Works[z].CellKey == cellKey) {
                t = t + Works[z].UndoTextTableMouseOver() + "<br>";
            }
        }
        t = t.Trim("<br>");
        t = t.Trim("<hr>");
        t = t.Trim("<br>");
        t = t.Trim("<hr>");
        return t;
    }

    internal static void SaveToByteList(ColumnItem c, ref List<byte> l) {
        var key = c.Key;

        SaveToByteList(l, DatabaseDataType.ColumnName, c.Name, key);
        SaveToByteList(l, DatabaseDataType.ColumnCaption, c.Caption, key);
        SaveToByteList(l, DatabaseDataType.ColumnFormat, ((int)c.Format).ToString(), key);
        SaveToByteList(l, DatabaseDataType.CaptionGroup1, c.CaptionGroup1, key);
        SaveToByteList(l, DatabaseDataType.CaptionGroup2, c.CaptionGroup2, key);
        SaveToByteList(l, DatabaseDataType.CaptionGroup3, c.CaptionGroup3, key);
        SaveToByteList(l, DatabaseDataType.MultiLine, c.MultiLine.ToPlusMinus(), key);
        SaveToByteList(l, DatabaseDataType.CellInitValue, c.CellInitValue, key);
        SaveToByteList(l, DatabaseDataType.SortAndRemoveDoubleAfterEdit, c.AfterEditQuickSortRemoveDouble.ToPlusMinus(), key);
        SaveToByteList(l, DatabaseDataType.DoUcaseAfterEdit, c.AfterEditDoUCase.ToPlusMinus(), key);
        SaveToByteList(l, DatabaseDataType.AutoCorrectAfterEdit, c.AfterEditAutoCorrect.ToPlusMinus(), key);
        SaveToByteList(l, DatabaseDataType.RoundAfterEdit, c.AfterEditRunden.ToString(), key);
        SaveToByteList(l, DatabaseDataType.AutoRemoveCharAfterEdit, c.AutoRemove, key);
        SaveToByteList(l, DatabaseDataType.SaveContent, c.SaveContent.ToPlusMinus(), key);
        SaveToByteList(l, DatabaseDataType.FilterOptions, ((int)c.FilterOptions).ToString(), key);
        SaveToByteList(l, DatabaseDataType.AutoFilterJoker, c.AutoFilterJoker, key);
        SaveToByteList(l, DatabaseDataType.IgnoreAtRowFilter, c.IgnoreAtRowFilter.ToPlusMinus(), key);
        SaveToByteList(l, DatabaseDataType.EditableWithTextInput, c.TextBearbeitungErlaubt.ToPlusMinus(), key);
        SaveToByteList(l, DatabaseDataType.SpellCheckingEnabled, c.SpellCheckingEnabled.ToPlusMinus(), key);
        SaveToByteList(l, DatabaseDataType.ShowMultiLineInOneLine, c.ShowMultiLineInOneLine.ToPlusMinus(), key);
        SaveToByteList(l, DatabaseDataType.ShowUndo, c.ShowUndo.ToPlusMinus(), key);
        SaveToByteList(l, DatabaseDataType.TextFormatingAllowed, c.FormatierungErlaubt.ToPlusMinus(), key);
        SaveToByteList(l, DatabaseDataType.ForeColor, c.ForeColor.ToArgb().ToString(), key);
        SaveToByteList(l, DatabaseDataType.BackColor, c.BackColor.ToArgb().ToString(), key);
        SaveToByteList(l, DatabaseDataType.LineStyleLeft, ((int)c.LineLeft).ToString(), key);
        SaveToByteList(l, DatabaseDataType.LineStyleRight, ((int)c.LineRight).ToString(), key);
        SaveToByteList(l, DatabaseDataType.EditableWithDropdown, c.DropdownBearbeitungErlaubt.ToPlusMinus(), key);
        SaveToByteList(l, DatabaseDataType.DropDownItems, c.DropDownItems.JoinWithCr(), key);
        SaveToByteList(l, DatabaseDataType.LinkedCellFilter, c.LinkedCellFilter.JoinWithCr(), key);
        SaveToByteList(l, DatabaseDataType.OpticalTextReplace, c.OpticalReplace.JoinWithCr(), key);
        SaveToByteList(l, DatabaseDataType.AutoReplaceAfterEdit, c.AfterEditAutoReplace.JoinWithCr(), key);
        SaveToByteList(l, DatabaseDataType.RegexCheck, c.Regex, key);
        SaveToByteList(l, DatabaseDataType.DropdownDeselectAllAllowed, c.DropdownAllesAbwählenErlaubt.ToPlusMinus(), key);
        SaveToByteList(l, DatabaseDataType.ShowValuesOfOtherCellsInDropdown, c.DropdownWerteAndererZellenAnzeigen.ToPlusMinus(), key);
        SaveToByteList(l, DatabaseDataType.ColumnQuickInfo, c.Quickinfo, key);
        SaveToByteList(l, DatabaseDataType.ColumnAdminInfo, c.AdminInfo, key);
        SaveToByteList(l, DatabaseDataType.ColumnContentWidth, c.ContentWidth.ToString(), key);
        SaveToByteList(l, DatabaseDataType.CaptionBitmapCode, c.CaptionBitmapCode, key);
        SaveToByteList(l, DatabaseDataType.AllowedChars, c.AllowedChars, key);
        SaveToByteList(l, DatabaseDataType.MaxTextLenght, c.MaxTextLenght.ToString(), key);
        SaveToByteList(l, DatabaseDataType.PermissionGroupsChangeCell, c.PermissionGroupsChangeCell.JoinWithCr(), key);
        SaveToByteList(l, DatabaseDataType.ColumnTags, c.Tags.JoinWithCr(), key);
        SaveToByteList(l, DatabaseDataType.EditAllowedDespiteLock, c.EditAllowedDespiteLock.ToPlusMinus(), key);
        SaveToByteList(l, DatabaseDataType.Suffix, c.Suffix, key);
        SaveToByteList(l, DatabaseDataType.LinkedDatabase, c.LinkedDatabaseFile, key);
        SaveToByteList(l, DatabaseDataType.ConstantHeightOfImageCode, c.ConstantHeightOfImageCode, key);
        SaveToByteList(l, DatabaseDataType.BehaviorOfImageAndText, ((int)c.BehaviorOfImageAndText).ToString(), key);
        SaveToByteList(l, DatabaseDataType.DoOpticalTranslation, ((int)c.DoOpticalTranslation).ToString(), key);
        SaveToByteList(l, DatabaseDataType.AdditionalFormatCheck, ((int)c.AdditionalFormatCheck).ToString(), key);
        SaveToByteList(l, DatabaseDataType.ScriptType, ((int)c.ScriptType).ToString(), key);
        SaveToByteList(l, DatabaseDataType.Prefix, c.Prefix, key);
        SaveToByteList(l, DatabaseDataType.KeyColumnKey, c.KeyColumnKey.ToString(), key);
        SaveToByteList(l, DatabaseDataType.ColumnKeyOfLinkedDatabase, c.LinkedCell_ColumnKeyOfLinkedDatabase.ToString(), key);
        SaveToByteList(l, DatabaseDataType.MakeSuggestionFromSameKeyColumn, c.VorschlagsColumn.ToString(), key);
        SaveToByteList(l, DatabaseDataType.ColumnAlign, ((int)c.Align).ToString(), key);
        SaveToByteList(l, DatabaseDataType.SortType, ((int)c.SortType).ToString(), key);
        //SaveToByteList(l, DatabaseDataType.ColumnTimeCode, c.TimeCode, key);
    }

    //public override void WaitEditable() => _muf.WaitEditable();
    internal static void SaveToByteList(List<byte> list, DatabaseDataType databaseDataType, string content, long columnKey) {
        var b = content.UTF8_ToByte();
        list.Add((byte)Routinen.ColumnUTF8_V400);
        list.Add((byte)databaseDataType);
        SaveToByteList(list, b.Length, 3);
        SaveToByteList(list, columnKey, 7);
        SaveToByteList(list, 0, 7); //Zeile-Unötig
        list.AddRange(b);
    }

    internal void SaveToByteList(List<byte> list, ColumnCollection c) {
        //Database.SaveToByteList(List, enDatabaseDataType.LastColumnKey, _LastColumnKey.ToString());
        foreach (var columnitem in c) {
            if (columnitem != null && !string.IsNullOrEmpty(columnitem.Name)) {
                SaveToByteList(columnitem, ref list);
            }
        }
    }

    internal void SaveToByteList(List<byte> l, CellCollection c) {
        c.RemoveOrphans();
        foreach (var thisString in c) {
            SaveToByteList(l, thisString);
        }
    }

    internal void SaveToByteList(List<byte> list, DatabaseDataType databaseDataType, string content) {
        var b = content.UTF8_ToByte();
        list.Add((byte)Routinen.DatenAllgemeinUTF8);
        list.Add((byte)databaseDataType);
        SaveToByteList(list, b.Length, 3);
        list.AddRange(b);
    }

    internal void SaveToByteList(List<byte> list, KeyValuePair<string, CellItem> cell) {
        if (string.IsNullOrEmpty(cell.Value.Value)) { return; }
        Cell.DataOfCellKey(cell.Key, out var tColumn, out var tRow);
        if (!tColumn.SaveContent) { return; }
        var b = cell.Value.Value.UTF8_ToByte();
        list.Add((byte)Routinen.CellFormatUTF8_V400);
        list.Add((byte)DatabaseDataType.Value_withSizeData);
        SaveToByteList(list, b.Length, 3);
        SaveToByteList(list, tColumn.Key, 7);
        SaveToByteList(list, tRow.Key, 7);
        list.AddRange(b);
        var contentSize = Cell.ContentSizeToSave(cell, tColumn);
        SaveToByteList(list, contentSize.Width, 2);
        SaveToByteList(list, contentSize.Height, 2);
    }

    protected override void AddUndo(string tableName, DatabaseDataType comand, long? columnKey, long? rowKey, string previousValue, string changedTo, string userName) {
        Works.Add(new WorkItem(comand, columnKey, rowKey, previousValue, changedTo, userName));
    }

    protected override void Dispose(bool disposing) {
        //_muf.Dispose();
        Works.Dispose();
        base.Dispose(disposing);
    }

    protected override void Initialize() {
        base.Initialize();
        Works.Clear();
    }

    protected override string SetValueInternal(DatabaseDataType type, string value, long? columnkey, long? rowkey, int width, int height, bool isLoading) {
        var r = base.SetValueInternal(type, value, columnkey, rowkey, width, height, isLoading);

        if (type == DatabaseDataType.UndoInOne) {
            Works.Clear();
            var uio = value.SplitAndCutByCr();
            for (var z = 0; z <= uio.GetUpperBound(0); z++) {
                WorkItem tmpWork = new(uio[z]);
                Works.Add(tmpWork);
            }
        }

        if (!isLoading && !ReadOnly) {
            HasPendingChanges = true;
        }

        return r;
    }

    private static int NummerCode2(byte[] b, int pointer) => (b[pointer] * 255) + b[pointer + 1];

    //protected override string SpecialErrorReason(ErrorReason mode) => _muf.ErrorReason(mode);
    private static int NummerCode3(byte[] b, int pointer) => (b[pointer] * 65025) + (b[pointer + 1] * 255) + b[pointer + 2];

    private static long NummerCode7(byte[] b, int pointer) {
        long nu = 0;
        for (var n = 0; n < 7; n++) {
            nu += b[pointer + n] * (long)Math.Pow(255, 6 - n);
        }
        return nu;
    }

    private static void SaveToByteList(List<byte> list, long numberToAdd, int byteCount) {
        //var tmp = numberToAdd;
        //var nn = byteCount;

        do {
            byteCount--;
            var te = (long)Math.Pow(255, byteCount);
            var mu = (byte)Math.Truncate((decimal)(numberToAdd / te));

            list.Add(mu);
            numberToAdd %= te;
        } while (byteCount > 0);

        //if (nn == 3) {
        //    if (NummerCode3(list.ToArray(), list.Count - nn) != tmp) { Debugger.Break(); }
        //}

        //if (nn == 7) {
        //    if (NummerCode7(list.ToArray(), list.Count - nn) != tmp) { Debugger.Break(); }
        //}
    }

    private string Backupdateiname() => string.IsNullOrEmpty(Filename) ? string.Empty : Filename.FilePath() + Filename.FileNameWithoutSuffix() + ".bak";

    /// <summary>
    /// Diese Routine lädt die Datei von der Festplatte. Zur Not wartet sie bis zu 5 Minuten.
    /// Hier wird auch nochmal geprüft, ob ein Laden überhaupt möglich ist.
    /// Es kann auch NULL zurück gegeben werden, wenn es ein Reload ist und die Daten inzwischen aktuell sind.
    /// </summary>
    /// <param name="checkmode"></param>
    /// <returns></returns>
    private byte[]? LoadBytesFromDisk(ErrorReason checkmode) {
        var startTime = DateTime.UtcNow;
        byte[] bLoaded;
        while (true) {
            try {
                var f = ErrorReason(checkmode);
                if (string.IsNullOrEmpty(f)) {
                    //var tmpLastSaveCode1 = GetFileInfo(Filename, true);
                    bLoaded = File.ReadAllBytes(Filename);
                    //tmpLastSaveCode2 = GetFileInfo(Filename, true);
                    //if (tmpLastSaveCode1 == tmpLastSaveCode2) { break; }
                    //f = "Datei wurde während des Ladens verändert.";
                    break;
                }

                if (DateTime.UtcNow.Subtract(startTime).TotalSeconds > 20) {
                    Develop.DebugPrint(FehlerArt.Info, f + "\r\n" + Filename);
                }

                Pause(0.5, false);
            } catch (Exception ex) {
                // Home Office kann lange blokieren....
                if (DateTime.UtcNow.Subtract(startTime).TotalSeconds > 300) {
                    Develop.DebugPrint(FehlerArt.Fehler, "Die Datei<br>" + Filename + "<br>konnte trotz mehrerer Versuche nicht geladen werden.<br><br>Die Fehlermeldung lautet:<br>" + ex.Message);
                    return null;
                }
            }
        }

        if (bLoaded.Length > 4 && BitConverter.ToInt32(bLoaded, 0) == 67324752) {
            // Gezipte Daten-Kennung gefunden
            bLoaded = MultiUserFile.UnzipIt(bLoaded);
        }
        return bLoaded;
    }

    private List<byte> ToListOfByte() {
        try {
            List<byte> l = new();
            // Wichtig, Reihenfolge und Länge NIE verändern!
            SaveToByteList(l, DatabaseDataType.Formatkennung, "BlueDatabase");
            SaveToByteList(l, DatabaseDataType.Version, DatabaseVersion);
            SaveToByteList(l, DatabaseDataType.Werbung, "                                                                    BlueDataBase - (c) by Christian Peter                                                                                        "); // Die Werbung dient als Dummy-Platzhalter, falls doch mal was vergessen wurde...
            // Passwörter ziemlich am Anfang speicher, dass ja keinen Weiteren Daten geladen werden können
            //if (string.IsNullOrEmpty(GlobalShowPass)) {
            //    SaveToByteList(l, DatabaseDataType.CryptionState, false.ToPlusMinus());
            //} else {
            //    SaveToByteList(l, DatabaseDataType.CryptionState, true.ToPlusMinus());
            //    SaveToByteList(l, DatabaseDataType.CryptionTest, "OK");
            //}
            SaveToByteList(l, DatabaseDataType.GlobalShowPass, GlobalShowPass);
            //SaveToByteList(l, DatabaseDataType.FileEncryptionKey, _fileEncryptionKey);
            SaveToByteList(l, DatabaseDataType.Creator, Creator);
            SaveToByteList(l, DatabaseDataType.CreateDateUTC, CreateDate);
            SaveToByteList(l, DatabaseDataType.Caption, Caption);
            //SaveToByteList(l, enDatabaseDataType.JoinTyp, ((int)_JoinTyp).ToString());
            //SaveToByteList(l, DatabaseDataType.VerwaisteDaten, ((int)_verwaisteDaten).ToString());
            SaveToByteList(l, DatabaseDataType.Tags, Tags.JoinWithCr());
            SaveToByteList(l, DatabaseDataType.PermissionGroupsNewRow, PermissionGroupsNewRow.JoinWithCr());
            SaveToByteList(l, DatabaseDataType.DatabaseAdminGroups, DatenbankAdmin.JoinWithCr());
            SaveToByteList(l, DatabaseDataType.GlobalScale, GlobalScale.ToString(Constants.Format_Float1));
            //SaveToByteList(l, DatabaseDataType.Ansicht, ((int)_ansicht).ToString());
            //SaveToByteList(l, DatabaseDataType.ReloadDelaySecond, ReloadDelaySecond.ToString());
            //SaveToByteList(l, enDatabaseDataType.ImportScript, _ImportScript);
            SaveToByteList(l, DatabaseDataType.RulesScript, RulesScript);
            //SaveToByteList(l, enDatabaseDataType.BinaryDataInOne, Bins.ToString(true));
            //SaveToByteList(l, enDatabaseDataType.FilterImagePfad, _filterImagePfad);
            SaveToByteList(l, DatabaseDataType.AdditionaFilesPath, AdditionaFilesPfad);
            SaveToByteList(l, DatabaseDataType.FirstColumn, FirstColumn);
            SaveToByteList(l, DatabaseDataType.RowQuickInfo, ZeilenQuickInfo);
            SaveToByteList(l, DatabaseDataType.StandardFormulaFile, StandardFormulaFile);
            SaveToByteList(l, Column);
            //Row.SaveToByteList(l);
            SaveToByteList(l, Cell);
            if (SortDefinition == null) {
                // Ganz neue Datenbank
                SaveToByteList(l, DatabaseDataType.SortDefinition, string.Empty);
            } else {
                SaveToByteList(l, DatabaseDataType.SortDefinition, SortDefinition.ToString());
            }
            //SaveToByteList(l, enDatabaseDataType.Rules_ALT, Rules.ToString(true));
            SaveToByteList(l, DatabaseDataType.ColumnArrangement, ColumnArrangements.ToString());
            SaveToByteList(l, DatabaseDataType.Layouts, Layouts.JoinWithCr());
            SaveToByteList(l, DatabaseDataType.AutoExport, Export.ToString(true));
            // Beim Erstellen des Undo-Speichers die Works nicht verändern, da auch bei einem nicht
            // erfolgreichen Speichervorgang der Datenbank-String erstellt wird.
            // Status des Work-Items ist egal, da es beim LADEN automatisch auf 'Undo' gesetzt wird.
            List<string> works2 = new();
            if (Works != null) {
                foreach (var thisWorkItem in Works) {
                    if (thisWorkItem != null) {
                        if (thisWorkItem.Comand != DatabaseDataType.Value_withoutSizeData) {
                            works2.Add(thisWorkItem.ToString());
                        } else {
                            if (thisWorkItem.LogsUndo(this)) {
                                works2.Add(thisWorkItem.ToString());
                            }
                        }
                    }
                }
            }

            SaveToByteList(l, DatabaseDataType.UndoCount, UndoCount.ToString());
            if (works2.Count > UndoCount) { works2.RemoveRange(0, works2.Count - UndoCount); }
            SaveToByteList(l, DatabaseDataType.UndoInOne, works2.JoinWithCr((int)(16581375 * 0.9)));
            SaveToByteList(l, DatabaseDataType.EOF, "END");
            return l;
        } catch {
            return ToListOfByte();
        }
    }

    private string WriteTempFileToDisk() {
        var f = ErrorReason(BlueBasics.Enums.ErrorReason.Save);
        if (!string.IsNullOrEmpty(f)) { return string.Empty; }

        var dataUncompressed = ToListOfByte().ToArray();
        var tmpFileName = TempFile(Filename.FilePath() + Filename.FileNameWithoutSuffix() + ".tmp-" + UserName().ToUpper());

        using FileStream x = new(tmpFileName, FileMode.Create, FileAccess.Write, FileShare.None);
        x.Write(dataUncompressed, 0, dataUncompressed.Length);
        x.Flush();
        x.Close();

        return tmpFileName;
    }

    #endregion

    //private void ChangeWorkItems(ItemState oldState, ItemState newState) {
    //    foreach (var thisWork in Works) {
    //        if (thisWork != null) {
    //            if (thisWork.State == oldState) { thisWork.State = newState; }
    //        }
    //    }
    //}

    //private void ExecutePending() {
    //    if (!_muf.IsLoading) { Develop.DebugPrint(FehlerArt.Fehler, "Nur während des Parsens möglich"); }

    //    var e2 = new MultiUserFileHasPendingChangesEventArgs();
    //    HasPendingChanges(null, e2);

    //    if (!e2.HasPendingChanges) { return; }
    //    // Erst die Neuen Zeilen / Spalten alle neutralisieren
    //    //var dummy = -1000;
    //    //foreach (var ThisPending in Works) {
    //    //    if (ThisPending.State == enItemState.Pending) {
    //    //        //if (ThisPending.Comand == enDatabaseDataType.dummyComand_AddRow) {
    //    //        //    dummy--;
    //    //        //    ChangeRowKeyInPending(ThisPending.RowKey, dummy);
    //    //        //}
    //    //        //if (ThisPending.Comand == enDatabaseDataType.AddColumnKeyInfo) {
    //    //        //    dummy--;
    //    //        //    ChangeColumnKeyInPending(ThisPending.ColKey, dummy);
    //    //        //}
    //    //    }
    //    //}
    //    //// Dann den neuen Zeilen / Spalten Tatsächlich eine neue ID zuweisen
    //    //foreach (var ThisPending in Works) {
    //    //    if (ThisPending.State == enItemState.Pending) {
    //    //        switch (ThisPending.Comand) {
    //    //            //case enDatabaseDataType.dummyComand_AddRow when _JoinTyp == enJoinTyp.Intelligent_zusammenfassen: {
    //    //            //        var Value = SearchKeyValueInPendingsOf(ThisPending.RowKey);
    //    //            //        var fRow = Row[Value];
    //    //            //        if (!string.IsNullOrEmpty(Value) && fRow != null) {
    //    //            //            ChangeRowKeyInPending(ThisPending.RowKey, fRow.Key);
    //    //            //        } else {
    //    //            //            ChangeRowKeyInPending(ThisPending.RowKey, Row.NextRowKey());
    //    //            //        }
    //    //            //        break;
    //    //            //    }
    //    //            //case enDatabaseDataType.dummyComand_AddRow:
    //    //            //    ChangeRowKeyInPending(ThisPending.RowKey, Row.NextRowKey());
    //    //            //    break;

    //    //            //case enDatabaseDataType.AddColumnKeyInfo:
    //    //            //    ChangeColumnKeyInPending(ThisPending.ColKey, Column.NextColumnKey);
    //    //            //    break;
    //    //        }
    //    //    }
    //    //}
    //    // Und nun alles ausführen!
    //    foreach (var thisPending in Works.Where(thisPending => thisPending.State == ItemState.Pending)) {
    //        if (thisPending.Comand == DatabaseDataType.ColumnName) {
    //            thisPending.ChangedTo = Column.Freename(thisPending.ChangedTo);
    //        }
    //        ExecutePending(thisPending);
    //    }
    //}

    //private void ExecutePending(WorkItem thisPendingItem) {
    //    if (thisPendingItem.State == ItemState.Pending) {
    //        //RowItem? row = null;
    //        //if (thisPendingItem.RowKey > -1) {
    //        //    row = Row.SearchByKey(thisPendingItem.RowKey);
    //        //    if (row == null) {
    //        //        if (thisPendingItem.Comand != DatabaseDataType.Comand_AddRow && thisPendingItem.User != UserName) {
    //        //            Develop.DebugPrint("Pending verworfen, Zeile gelöscht.<br>" + ConnectionData.TableName + "<br>" + thisPendingItem.ToString());
    //        //            return;
    //        //        }
    //        //    }
    //        //}
    //        //ColumnItem? col = null;
    //        //if (thisPendingItem.ColKey > -1) {
    //        //    col = Column.SearchByKey(thisPendingItem.ColKey);
    //        //    if (col == null) {
    //        //        //if (thisPendingItem.Comand != DatabaseDataType.AddColumnKeyInfo && thisPendingItem.User != UserName) {
    //        //        Develop.DebugPrint("Pending verworfen, Spalte gelöscht.<br>" + ConnectionData.TableName + "<br>" + thisPendingItem.ToString());
    //        //        return;
    //        //        //}
    //        //    }
    //        //}
    //        SetValueInternal(thisPendingItem.Comand, thisPendingItem.ChangedTo, thisPendingItem.ColKey, thisPendingItem.RowKey, 0, 0, true);
    //    }
    //}
}