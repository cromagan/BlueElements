// Authors:
// Christian Peter
//
// Copyright (c) 2024 Christian Peter
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

using System.Collections.Generic;
using BlueBasics;
using BlueBasics.Enums;
using BlueBasics.Interfaces;
using BlueDatabase;
using BlueDatabase.EventArgs;

namespace BlueControls.Interfaces;

public interface IControlUsesRow : IDisposableExtendedWithEvent, IControlAcceptFilter {

    #region Properties

    /// <summary>
    /// Bedeutet, dass kein Parent vorhanden ist - und der Filter anderweitig gesetzt wurde. Z.B. durch SetRow
    /// Wenn TRUE, sollte der Input Filter nicht mehr von den Parents verändert werden.
    /// </summary>
    public bool RowManualSeted { get; set; }

    public List<RowItem>? RowsInput { get; set; }

    #endregion

    #region Methods

    public void Rows_Changed();

    public void Rows_Changing();

    public void RowsExternal_Added(object sender, RowChangedEventArgs e);

    public void RowsExternal_Removed(object sender, System.EventArgs e);

    #endregion
}

public static class IControlUsesRowExtension {

    #region Methods

    public static Database? Database(this IControlUsesRow icur) {
        if (icur.IsDisposed) { return null; }

        //if (icur.RowsInput == null) { icur.DoRows(mustbeDatabase, doEmptyFilterToo); }
        if (icur.RowsInput == null || icur.RowsInput.Count == 0) { return null; }

        return icur.RowsInput[0].Database;
    }

    public static void DoDispose(this IControlUsesRow child) {
        child.Rows_Changing();
        child.Invalidate_Rows();
        child.DisconnectChildParents(child.Parents);
    }

    public static void DoRows(this IControlUsesRow icur, Database? mustbeDatabase, bool doEmptyFilterToo) {
        var f = icur.GetInputFilter(mustbeDatabase, doEmptyFilterToo);

        if (f == null) {
            icur.RowsInput = new List<RowItem>();
            return;
        }

        icur.RowsInput = [.. f.Rows];

        if (icur.RowSingleOrNull() is RowItem r) {
            r.CheckRowDataIfNeeded();
        }

        icur.RegisterEvents();
    }

    public static Database? FilterInput_Database(this IControlUsesRow icur) {
        var f = icur.GetInputFilter(null, false);
        return f?.Database;
    }

    public static void Invalidate_Rows(this IControlUsesRow child) {
        child.UnregisterEvents();
        child.RowsInput = null;
        child.Invalidate();
    }

    public static void RegisterEvents(this IControlUsesRow child) {
        if (child.Database() is Database db) {
            db.Row.RowAdded += child.RowsExternal_Added;
            db.Row.RowRemoved += child.RowsExternal_Removed;
        }
    }

    public static void RowsExternal_Changed(this IControlUsesRow child) {
        child.Invalidate_Rows();
        child.Rows_Changed();
    }

    public static RowItem? RowSingleOrNull(this IControlUsesRow icur) {
        if (icur.IsDisposed) { return null; }

        //if (icur.RowsInput == null) { icur.DoRows(mustbeDatabase, doEmptyFilterToo); }
        if (icur.RowsInput == null || icur.RowsInput.Count != 1) { return null; }

        return icur.RowsInput[0];
    }

    public static void SetToRow(this IControlUsesRow item, RowItem? row) {
        if (item.IsDisposed) { return; }
        if (item.Parents.Count > 0) {
            Develop.DebugPrint(FehlerArt.Fehler, "Element wird von Parents gesteuert!");
        }

        if (row != item.RowSingleOrNull()) { return; }

        item.Rows_Changing();

        item.RowManualSeted = true;

        item.UnregisterEvents();

        item.RowsInput = [];

        if (row?.Database == null) { return; }

        item.RowsInput.Add(row);

        //if (row?.Database is Database db && !db.IsDisposed) {
        //    var fc = new FilterCollection(db, "SetToRow");
        //    fc.Database = db;
        //    item.SetFilterInput(fc);
        //}

        //if (item.FilterInput != null) {
        //    item.FilterInput.Clear();
        //    if (row == null) {
        //        item.FilterInput.ChangeTo(new FilterItem(item.FilterInput?.Database, "SetRow"));
        //    } else {
        //item.FilterInput.ChangeTo(new FilterItem(row));
        //    }
        //}
        item.RegisterEvents();
        item.Rows_Changed();
    }

    public static void UnregisterEvents(this IControlUsesRow child) {
        if (child.Database() is Database db) {
            db.Row.RowAdded -= child.RowsExternal_Added;
            db.Row.RowRemoved -= child.RowsExternal_Removed;
        }
    }

    #endregion
}