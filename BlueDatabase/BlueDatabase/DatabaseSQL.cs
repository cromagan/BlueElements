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
using BlueDatabase.Enums;
using System;
using System.ComponentModel;

namespace BlueDatabase;

[Browsable(false)]
[EditorBrowsable(EditorBrowsableState.Never)]
public sealed class DatabaseSQL : DatabaseAbstract {

    #region Fields

    public readonly SQLBackAbstract _sql;

    #endregion

    #region Constructors

    public DatabaseSQL(SQLBackAbstract sql, bool readOnly, string tablename) : base(tablename, readOnly) {
        AllFiles.Add(this);

        _sql = sql;

        Develop.StartService();

        Initialize();

        if (sql != null) {
            //DropConstructorMessage?.Invoke(this, new MessageEventArgs(enFehlerArt.Info, "Lade Datenbank aus Dateisystem: \r\n" + tablename.FileNameWithoutSuffix()));
            LoadFromSQLBack();
        }
        RepairAfterParse();
    }

    #endregion

    #region Properties

    public override string ConnectionID => _sql.ConnectionID(TableName);

    public override string Filename => _sql.Filename;

    public override bool IsLoading { get; protected set; }
    public override bool ReloadNeeded { get; }
    public override bool ReloadNeededSoft { get; }

    #endregion

    #region Methods

    public override void BlockReload(bool crashIsCurrentlyLoading) { }

    public override void Load_Reload() { }

    public override bool Save(bool mustSave) => _sql.ConnectionOk;

    public override string SetValueInternal(DatabaseDataType type, string value, ColumnItem? column, RowItem? row, int width, int height) {
        _sql?.CheckIn(TableName, type, value, column, row, width, height);
        return base.SetValueInternal(type, value, column, row, width, height);
    }

    public override string UndoText(ColumnItem? column, RowItem? row) => string.Empty;

    public override void UnlockHard() { }

    public override void WaitEditable() { }

    internal void AddColumn(string columnname, SQLBackAbstract sql) {
        var x = new ColumnItem(this, columnname, Column.NextColumnKey());

        var l = sql.GetStylDataAll(TableName.FileNameWithoutSuffix(), columnname);
        if (l != null && l.Count > 0) {
            foreach (var thisstyle in l) {
                Enum.TryParse(thisstyle.Key, out DatabaseDataType t);
                x.SetValueInternal(t, thisstyle.Value);
            }
        }

        Column.Add(x);
    }

    protected override void AddUndo(string tableName, DatabaseDataType comand, ColumnItem? column, long rowKey, string previousValue, string changedTo, string userName) {
        _sql.AddUndo(TableName, comand, column, rowKey, previousValue, changedTo, UserName);
    }

    protected override DatabaseAbstract? GetOtherTable(string tablename, bool readOnly) {
        if (!SQLBackAbstract.IsValidTableName(tablename)) {
            return null;
        }

        return new DatabaseSQL(_sql, readOnly, tablename);
    }

    protected override void SetUserDidSomething() { }

    protected override string SpecialErrorReason(ErrorReason mode) => string.Empty;

    private void LoadFromSQLBack() {
        IsLoading = true;

        #region Spalten erstellen

        var cols = _sql.GetColumnNames(TableName.ToUpper());
        cols.Remove("RK");

        foreach (var thisCol in cols) {
            AddColumn(thisCol, _sql);
        }

        Column.GetSystems();

        #endregion

        #region Datenbank Eigenschaften laden

        var l = _sql.GetStylDataAll(TableName, string.Empty);
        if (l != null && l.Count > 0) {
            foreach (var thisstyle in l) {
                Enum.TryParse(thisstyle.Key, out DatabaseDataType t);
                SetValueInternal(t, thisstyle.Value, null, null, -1, -1);
            }
        }

        #endregion

        #region  Alle Zellen laden

        _sql.LoadAllCells(TableName, Row);

        #endregion

        IsLoading = false;

        //RepairAfterParse(null, null);
    }

    #endregion
}