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
using System.Windows.Forms;
using BlueBasics;
using BlueBasics.Enums;
using BlueBasics.Interfaces;
using BlueDatabase;
using BlueDatabase.EventArgs;

namespace BlueControls.Interfaces;

public interface IControlUsesRow : IDisposableExtendedWithEvent, IControlAcceptFilter {

    #region Properties

    public bool RowChangedHandled { get; set; }

    /// <summary>
    /// Bedeutet, dass kein Parent vorhanden ist - und der Filter anderweitig gesetzt wurde. Z.B. durch SetRow
    /// Wenn TRUE, sollte der Input Filter nicht mehr von den Parents verändert werden.
    /// </summary>
    public bool RowManualSeted { get; set; }

    public List<RowItem>? RowsInput { get; set; }

    #endregion

    #region Methods

    public void HandleRowsNow();

    public void Rows_Changed();

    public void Rows_Changing();

    public void RowsExternal_Added(object sender, RowChangedEventArgs e);

    public void RowsExternal_Removed(object sender, System.EventArgs e);

    #endregion
}

public static class IControlUsesRowExtension {
    //public static Database? Database(this IControlUsesRow icur) {
    //    if (icur.IsDisposed) { return null; }

    //    //if (icur.RowsInput == null) { icur.DoRows(mustbeDatabase, doEmptyFilterToo); }
    //    if (icur.RowsInput == null || icur.RowsInput.Count == 0) { return null; }

    //    return icur.RowsInput[0].Database;
    //}

    #region Methods

    public static Database? Database(this IControlUsesRow icur) {
        if (icur.RowsInput != null && icur.RowsInput.Count > 0) { return icur.RowsInput[0].Database; }
        var f = icur.GetInputFilter(null, false);
        return f?.Database;
    }

    public static void DoDispose(this IControlUsesRow icur) {
        icur.UnregisterEvents();
        icur.Rows_Changing();
        icur.Invalidate_Rows();
        icur.DisconnectChildParents(icur.Parents);
    }

    public static void DoRows(this IControlUsesRow icur, Database? mustbeDatabase, bool doEmptyFilterToo) {
        if (icur.RowManualSeted) { return; }

        var f = icur.GetInputFilter(mustbeDatabase, doEmptyFilterToo);

        if (f == null) {
            icur.RowsInput = new List<RowItem>();
            return;
        }

        icur.RowsInput = [.. f.Rows];

        if (icur.RowSingleOrNull() is RowItem r) {
            r.CheckRowDataIfNeeded();
        }

        //icur.RegisterEvents();
    }

    public static void Invalidate_Rows(this IControlUsesRow icur) {
        if (!icur.RowManualSeted) {
            icur.RowsInput = null;
        }

        icur.RowChangedHandled = false;
        icur.Invalidate();
    }

    public static void RegisterEvents(this IControlUsesRow icur) {
        if (icur.Database() is Database db) {
            db.Row.RowAdded += icur.RowsExternal_Added;
            db.Row.RowRemoved += icur.RowsExternal_Removed;
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

        var doAtabaseAfter = Database == null;

        if (row == item.RowSingleOrNull()) { return; }

        item.Rows_Changing();
        item.Invalidate_Rows();
        item.RowManualSeted = true;

        item.RowsInput = [];

        if (row?.Database is Database db && !db.IsDisposed) {
            item.RowsInput.Add(row);
            row.CheckRowDataIfNeeded();

            if (doAtabaseAfter) { item.RegisterEvents(); }
        }

        item.Rows_Changed();
    }

    public static void UnregisterEvents(this IControlUsesRow icur) {
        if (icur.Database() is Database db) {
            db.Row.RowAdded -= icur.RowsExternal_Added;
            db.Row.RowRemoved -= icur.RowsExternal_Removed;
        }
    }

    #endregion
}