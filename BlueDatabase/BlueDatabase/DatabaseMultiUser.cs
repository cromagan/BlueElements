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
using BlueDatabase.Enums;
using BlueDatabase.EventArgs;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using static BlueBasics.Converter;

namespace BlueDatabase;

[Browsable(false)]
[EditorBrowsable(EditorBrowsableState.Never)]
public sealed class DatabaseMultiUser : DatabaseAbstract {

    #region Fields

    public ListExt<WorkItem>? Works;

    private readonly BlueBasics.MultiUserFile.MultiUserFile? _muf;

    #endregion

    #region Constructors

    public DatabaseMultiUser(bool readOnly, string tablename) : this(string.Empty, readOnly, true, tablename) { }

    public DatabaseMultiUser(string filename, bool readOnly, bool create, string tablename) : base(tablename, readOnly) {
        _muf = new BlueBasics.MultiUserFile.MultiUserFile();

        _muf.Loaded += _muf_Loaded;
        _muf.Loading += _muf_Loading;
        _muf.SavedToDisk += _muf_SavedToDisk;
        _muf.DiscardPendingChanges += DiscardPendingChanges;
        _muf.HasPendingChanges += _muf_HasPendingChanges;
        _muf.ParseExternal += Parse;
        _muf.ToListOfByte += ToListOfByte;
        _muf.ReloadDelaySecond = 180;

        Develop.StartService();

        Works = new ListExt<WorkItem>();

        Initialize();

        if (!string.IsNullOrEmpty(filename)) {
            //DropConstructorMessage?.Invoke(this, new MessageEventArgs(enFehlerArt.Info, "Lade Datenbank aus Dateisystem: \r\n" + filename.FileNameWithoutSuffix()));
            _muf.Load(filename, create);
        }

        AllFiles.Add(this);
    }

    #endregion

    #region Properties

    public static string DatabaseID { get => "BlueDatabaseMultiUser"; }

    public override ConnectionInfo ConnectionData => new(TableName, this, DatabaseID, Filename);

    public string Filename => _muf?.Filename ?? string.Empty;

    public bool IsLoading => _muf?.IsLoading ?? false;

    public bool ReloadNeeded => _muf?.ReloadNeeded ?? false;

    #endregion

    #region Methods

    public override List<ConnectionInfo>? AllAvailableTables(List<DatabaseAbstract>? allreadychecked, List<string>? ignorePath) {
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

        if (ignorePath != null) {
            foreach (var thisPF in ignorePath) {
                if (Filename.FilePath().StartsWith(thisPF, StringComparison.OrdinalIgnoreCase)) { return null; }
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

    public void DiscardPendingChanges(object sender, System.EventArgs e) => ChangeWorkItems();

    public void Load_Reload() => _muf?.Load_Reload();

    public void Parse(object sender, MultiUserParseEventArgs e) {
        Database.Parse(e.Data, this, Works);
        ExecutePending();
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

    public override bool Save() => _muf?.Save(true) ?? false;

    public override string UndoText(ColumnItem? column, RowItem? row) => Database.UndoText(column, row, Works);

    public void UnlockHard() => _muf.UnlockHard();

    internal override string SetValueInternal(DatabaseDataType type, string value, long? columnkey, long? rowkey, int width, int height, bool isLoading) {
        var r = base.SetValueInternal(type, value, columnkey, rowkey, width, height, isLoading);

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

    protected override void AddUndo(string tableName, DatabaseDataType comand, long? columnKey, long? rowKey, string previousValue, string changedTo, string userName, string comment) {
        Works.Add(new WorkItem(comand, columnKey, rowKey, previousValue, changedTo, userName));
    }

    protected override void Dispose(bool disposing) {
        _muf.Dispose();
        Works.Dispose();
        base.Dispose(disposing);
    }

    protected override void Initialize() {
        base.Initialize();
        _muf.ReloadDelaySecond = 600;
        Works.Clear();
    }

    protected override void SetUserDidSomething() => _muf.SetUserDidSomething();

    //protected string SpecialErrorReason(ErrorReason mode) => _muf.ErrorReason(mode);

    private void _muf_HasPendingChanges(object sender, MultiUserFileHasPendingChangesEventArgs e) {
        e.HasPendingChanges = HasPendingChanges;
    }

    private void _muf_Loaded(object sender, System.EventArgs e) {
        OnLoaded();

        RepairAfterParse();
        CreateWatcher();
    }

    private void _muf_Loading(object sender, System.EventArgs e) {
        OnLoading();
    }

    private void _muf_SavedToDisk(object sender, System.EventArgs e) {
        if (IsDisposed) { return; }
        HasPendingChanges = false;
        ChangeWorkItems();
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
        foreach (var thisPending in Works.Where(thisPending => thisPending.IsPending)) {
            if (thisPending.Comand == DatabaseDataType.ColumnName) {
                thisPending.ChangedTo = Column.Freename(thisPending.ChangedTo);
            }
            ExecutePending(thisPending);
        }
    }

    private void ExecutePending(WorkItem thisPendingItem) {
        if (thisPendingItem.IsPending) {
            //RowItem? row = null;
            //if (thisPendingItem.RowKey > -1) {
            //    row = Row.SearchByKey(thisPendingItem.RowKey);
            //    if (row == null) {
            //        if (thisPendingItem.Comand != DatabaseDataType.Comand_AddRow && thisPendingItem.User != UserName) {
            //            Develop.DebugPrint("Pending verworfen, Zeile gelöscht.<br>" + ConnectionData.TableName + "<br>" + thisPendingItem.ToString());
            //            return;
            //        }
            //    }
            //}
            //ColumnItem? col = null;
            //if (thisPendingItem.ColKey > -1) {
            //    col = Column.SearchByKey(thisPendingItem.ColKey);
            //    if (col == null) {
            //        //if (thisPendingItem.Comand != DatabaseDataType.AddColumnKeyInfo && thisPendingItem.User != UserName) {
            //        Develop.DebugPrint("Pending verworfen, Spalte gelöscht.<br>" + ConnectionData.TableName + "<br>" + thisPendingItem.ToString());
            //        return;
            //        //}
            //    }
            //}
            SetValueInternal(thisPendingItem.Comand, thisPendingItem.ChangedTo, thisPendingItem.ColKey, thisPendingItem.RowKey, 0, 0, true);
        }
    }

    private void ToListOfByte(object sender, MultiUserToListEventArgs e) {
        e.Data = Database.ToListOfByte(this, Works).ToArray();
    }

    #endregion
}