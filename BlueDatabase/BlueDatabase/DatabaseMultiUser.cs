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

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using BlueBasics;
using BlueBasics.Enums;
using BlueBasics.EventArgs;
using BlueBasics.MultiUserFile;
using BlueDatabase.Enums;
using static BlueBasics.IO;

namespace BlueDatabase;

[Browsable(false)]
[EditorBrowsable(EditorBrowsableState.Never)]
public sealed class DatabaseMultiUser : DatabaseAbstract {

    #region Fields

    public List<WorkItem>? Works;
    private readonly MultiUserFile? _muf;
    private bool _loadScriptFired = false;

    #endregion

    #region Constructors

    public DatabaseMultiUser(ConnectionInfo ci, bool readOnly, NeedPassword? needPassword) : this(ci.AdditionalData, readOnly, false, ci.TableName) { }

    public DatabaseMultiUser(bool readOnly, string tablename) : this(string.Empty, readOnly, true, tablename) { }

    public DatabaseMultiUser(string filename, bool readOnly, bool create, string tablename) : base(tablename, readOnly) {
        _muf = new MultiUserFile();

        _muf.Loaded += _muf_Loaded;
        _muf.Loading += _muf_Loading;
        _muf.SavedToDisk += _muf_SavedToDisk;
        _muf.DiscardPendingChanges += DiscardPendingChanges;
        _muf.HasPendingChanges += _muf_HasPendingChanges;
        _muf.ParseExternal += Parse;
        _muf.ToListOfByte += ToListOfByte;
        _muf.ReloadDelaySecond = 180;
        _muf.Saving += _muf_Saving;

        Works = new List<WorkItem>();

        Initialize();

        if (!string.IsNullOrEmpty(filename)) {
            //DropConstructorMessage?.Invoke(this, new MessageEventArgs(enFehlerArt.Info, "Lade Datenbank aus Dateisystem: \r\n" + filename.FileNameWithoutSuffix()));
            _muf.Load(filename, create);
        }
    }

    #endregion

    #region Properties

    public static string DatabaseId => typeof(DatabaseMultiUser).Name;

    public override ConnectionInfo ConnectionData => new(TableName, this, DatabaseId, Filename);

    public string Filename => _muf?.Filename ?? string.Empty;

    public bool IsLoading => _muf?.IsLoading ?? false;

    public bool ReloadNeeded => _muf?.ReloadNeeded ?? false;

    #endregion

    #region Methods

    public override string AdditionalFilesPfadWhole() {
        var x = base.AdditionalFilesPfadWhole();
        if (!string.IsNullOrEmpty(x)) { return x; }

        if (!string.IsNullOrEmpty(Filename)) {
            var t = (Filename.FilePath() + "AdditionalFiles\\").CheckPath();
            if (DirectoryExists(t)) {
                AdditionalFilesPfadtmp = t;
                return t;
            }
        }
        AdditionalFilesPfadtmp = string.Empty;
        return string.Empty;
    }

    public override List<ConnectionInfo>? AllAvailableTables(List<DatabaseAbstract>? allreadychecked) {
        if (string.IsNullOrWhiteSpace(Filename)) { return null; } // Stream-Datenbank

        if (allreadychecked != null) {
            foreach (var thisa in allreadychecked) {
                if (thisa is Database db) {
                    if (string.Equals(db.Filename.FilePath(), Filename.FilePath())) { return null; }
                }
                if (thisa is DatabaseMultiUser dbm) {
                    if (string.Equals(dbm.Filename.FilePath(), Filename.FilePath())) { return null; }
                }
            }
        }

        //if (ignorePath != null) {
        //    foreach (var thisPf in ignorePath) {
        //        if (Filename.FilePath().StartsWith(thisPf, StringComparison.OrdinalIgnoreCase)) { return null; }
        //    }
        //}

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

        return new ConnectionInfo(SQLBackAbstract.MakeValidTableName(tableName.FileNameWithoutSuffix()), null, DatabaseId, f);
    }

    public void DiscardPendingChanges(object sender, System.EventArgs e) => ChangeWorkItems();

    public void Load_Reload() {
        if (!ReloadNeeded || IsDisposed) { return; }

        OnDropMessage(FehlerArt.Info, "Lade Änderungen der Datenbank: " + TableName);

        if (_muf?.Load_Reload() ?? false) {
            OnDropMessage(FehlerArt.Info, "Laden der Datenbank fehlgeschlagen: " + TableName);
        } else {
            OnDropMessage(FehlerArt.Info, string.Empty);
        }
    }

    public void Parse(object sender, MultiUserParseEventArgs e) {
        Database.Parse(e.Data, this, Works, null);
        ExecutePending();
    }

    public override void RefreshColumnsData(List<ColumnItem> columns) {
        if (columns == null || columns.Count == 0) { return; }

        foreach (var thiscol in columns) {
            if (thiscol != null) { thiscol.IsInCache = DateTime.UtcNow; }
        }
    }

    public override bool RefreshRowData(List<RowItem> row, bool refreshAlways) {
        if (row == null || row.Count == 0) { return false; }

        foreach (var thisrow in row) {
            thisrow.IsInCache = DateTime.UtcNow;
        }

        return false;
    }

    public override bool Save() {
        if (_muf == null || IsDisposed || ReadOnly || !HasPendingChanges) { return false; }

        if (_muf.IsLoading) { return false; }

        OnDropMessage(FehlerArt.Info, "Speichere Datenbank: " + TableName);

        var x = _muf?.Save(true) ?? false;

        if (x) {
            OnDropMessage(FehlerArt.Info, "Datenbank " + TableName + " erfolgreich gespeichert.");
        } else {
            OnDropMessage(FehlerArt.Info, "Datenbank " + TableName + " Speichervorgang nicht erfolgreich, neuer Versuch startet demnächst.");
        }

        return x;
    }

    public override string UndoText(ColumnItem? column, RowItem? row) => Database.UndoText(column, row, Works);

    public void UnlockHard() => _muf.UnlockHard();

    internal override string SetValueInternal(DatabaseDataType type, string value, string? columnName, long? rowkey, bool isLoading) {
        if (IsDisposed) { return "Datenbank verworfen!"; }

        var r = base.SetValueInternal(type, value, columnName, rowkey, isLoading);

        if (type == DatabaseDataType.UndoInOne) {
            Works.Clear();
            var uio = value.SplitAndCutByCr();
            for (var z = 0; z <= uio.GetUpperBound(0); z++) {
                WorkItem tmpWork = new(uio[z]) {
                    IsPending = false // Beim Erstellen des strings ist noch nicht sicher, ob gespeichter wird. Deswegen die alten "Pendings" zu Undos ändern.
                };
                Works.Add(tmpWork);
            }
        }

        if (!isLoading && !ReadOnly) {
            HasPendingChanges = true;
        }

        return r;
    }

    protected override void AddUndo(string tableName, DatabaseDataType comand, string? columnName, long? rowKey, string previousValue, string changedTo, string userName, string comment) => Works.Add(new WorkItem(comand, columnName, rowKey, previousValue, changedTo, userName));

    protected override void Dispose(bool disposing) {
        _muf?.Dispose();
        Works?.Clear();
        base.Dispose(disposing);
    }

    protected override void Initialize() {
        base.Initialize();
        _muf.ReloadDelaySecond = 600;
        Works?.Clear();
    }

    protected override void SetUserDidSomething() => _muf.SetUserDidSomething();

    private void _muf_HasPendingChanges(object sender, MultiUserFileHasPendingChangesEventArgs e) => e.HasPendingChanges = HasPendingChanges;

    //protected string SpecialErrorReason(ErrorReason mode) => _muf.ErrorReason(mode);
    private void _muf_Loaded(object sender, System.EventArgs e) {
        RepairAfterParse();
        OnLoaded();
        CreateWatcher();

        if (!_loadScriptFired) {
            _loadScriptFired = true;
            _ = ExecuteScript(EventTypes.database_loaded, string.Empty, true, null);
        }
    }

    private void _muf_Loading(object sender, System.EventArgs e) => OnLoading();

    private void _muf_SavedToDisk(object sender, System.EventArgs e) {
        if (IsDisposed) { return; }
        HasPendingChanges = false;
        ChangeWorkItems();
    }

    private void _muf_Saving(object sender, CancelEventArgs e) {
        if (e.Cancel) { return; }

        var x = ErrorReason(BlueBasics.Enums.ErrorReason.Save);

        e.Cancel = !string.IsNullOrEmpty(x);
    }

    private void ChangeWorkItems() {
        foreach (var thisWork in Works) {
            if (thisWork != null) {
                thisWork.IsPending = false;
            }
        }
    }

    private void ExecutePending() {
        if (!_muf.IsLoading) { Develop.DebugPrint(FehlerArt.Fehler, "Nur während des Parsens möglich"); }

        if (!HasPendingChanges) { return; }
        // Erst die Neuen Zeilen / Spalten alle neutralisieren
        //var dummy = -1000;
        //foreach (var ThisPending in Works) {
        //    if (ThisPending.State == enItemState.Pending) {
        //        //if (ThisPending.Comand == enDatabaseDataType.dummyComand_AddRow) {
        //        //    dummy--;
        //        //    ChangeRowKeyInPending(ThisPending.RowKey, dummy);
        //        //}
        //        //if (ThisPending.Comand == enDatabaseDataType.AddColumnKeyInfo) {
        //        //    dummy--;
        //        //    ChangeColumnKeyInPending(ThisPending.ColKey, dummy);
        //        //}
        //    }
        //}
        //// Dann den neuen Zeilen / Spalten Tatsächlich eine neue ID zuweisen
        //foreach (var ThisPending in Works) {
        //    if (ThisPending.State == enItemState.Pending) {
        //        switch (ThisPending.Comand) {
        //            //case enDatabaseDataType.dummyComand_AddRow when _JoinTyp == enJoinTyp.Intelligent_zusammenfassen: {
        //            //        var Value = SearchKeyValueInPendingsOf(ThisPending.RowKey);
        //            //        var fRow = Row[Value];
        //            //        if (!string.IsNullOrEmpty(Value) && fRow != null) {
        //            //            ChangeRowKeyInPending(ThisPending.RowKey, fRow.Key);
        //            //        } else {
        //            //            ChangeRowKeyInPending(ThisPending.RowKey, Row.NextRowKey());
        //            //        }
        //            //        break;
        //            //    }
        //            //case enDatabaseDataType.dummyComand_AddRow:
        //            //    ChangeRowKeyInPending(ThisPending.RowKey, Row.NextRowKey());
        //            //    break;

        //            //case enDatabaseDataType.AddColumnKeyInfo:
        //            //    ChangeColumnKeyInPending(ThisPending.ColKey, Column.NextColumnKey);
        //            //    break;
        //        }
        //    }
        //}
        // Und nun alles ausführen!
        foreach (var thisPending in Works) {
            if (thisPending.IsPending) {
                if (thisPending.Comand == DatabaseDataType.ColumnName) {
                    thisPending.ChangedTo = Column.Freename(thisPending.ChangedTo);
                }

                ExecutePending(thisPending);
            }
        }
    }

    private void ExecutePending(WorkItem thisPendingItem) {
        if (thisPendingItem.IsPending) {
            //RowItem? row = null;
            //if (thisPendingItem.RowKey > -1) {
            //    row = Row.SearchByKey(thisPendingItem.RowKey);
            //    if (row == null) {
            //        if (thisPendingItem.Comand != DatabaseDataType.Comand_AddRow && thisPendingItem.User != UserName) {
            //            Develop.DebugPrint("Pending verworfen, Zeile gelöscht.<br>" + ConnectionData.TableName + "<br>" + thisPendingItem.ToString(false));
            //            return;
            //        }
            //    }
            //}
            //ColumnItem? col = null;
            //if (thisPendingItem.ColKey > -1) {
            //    col = Column.SearchByKey(thisPendingItem.ColKey);
            //    if (col == null) {
            //        //if (thisPendingItem.Comand != DatabaseDataType.AddColumnKeyInfo && thisPendingItem.User != UserName) {
            //        Develop.DebugPrint("Pending verworfen, Spalte gelöscht.<br>" + ConnectionData.TableName + "<br>" + thisPendingItem.ToString(false));
            //        return;
            //        //}
            //    }
            //}
            _ = SetValueInternal(thisPendingItem.Comand, thisPendingItem.ChangedTo, thisPendingItem.ColName, thisPendingItem.RowKey, true);
        }
    }

    private void ToListOfByte(object sender, MultiUserToListEventArgs e) => e.Data = Database.ToListOfByte(this, Works)?.ToArray();

    #endregion
}