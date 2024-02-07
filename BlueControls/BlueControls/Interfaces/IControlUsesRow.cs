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

namespace BlueControls.Interfaces;

public interface IControlUsesRow : IDisposableExtendedWithEvent, IControlAcceptFilter {

    #region Properties

    public List<RowItem>? RowsInput { get; set; }
    public bool RowsInputChangedHandled { get; set; }

    /// <summary>
    /// Bedeutet, dass kein Parent vorhanden ist - und der Filter anderweitig gesetzt wurde. Z.B. durch SetRow
    /// Wenn TRUE, sollte der Input Filter nicht mehr von den Parents verändert werden.
    /// </summary>
    public bool RowsInputManualSeted { get; set; }

    #endregion

    #region Methods

    public void RowsInput_Changed();

    #endregion
}

public static class ControlUsesRowExtension {
    //public static Database? Database(this IControlUsesRow icur) {
    //    if (icur.IsDisposed) { return null; }

    //    //if (icur.RowsInput == null) { icur.DoRows(mustbeDatabase, doEmptyFilterToo); }
    //    if (icur.RowsInput == null || icur.RowsInput.Count == 0) { return null; }

    //    return icur.RowsInput[0].Database;
    //}

    #region Methods

    public static Database? Database(this IControlUsesRow icur) {
        if (icur.RowsInput != null && icur.RowsInput.Count > 0) { return icur.RowsInput[0].Database; }
        using var f = icur.GetInputFilter(null, false);
        return f?.Database;
    }

    public static void DoDispose(this IControlUsesRow icur) {
        //icur.RowsInput_Changing();
        icur.Invalidate_RowsInput();
        icur.DisconnectChildParents(icur.Parents);
    }

    public static void DoRows(this IControlUsesRow icur, Database? mustbeDatabase, bool doEmptyFilterToo) {
        if (icur.RowsInputManualSeted) { return; }

        using var f = icur.GetInputFilter(mustbeDatabase, doEmptyFilterToo);

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

    public static void FilterInput_RowsChanged(this IControlUsesRow icaf) {
        icaf.Invalidate_RowsInput();
    }

    public static void Invalidate_RowsInput(this IControlUsesRow icur) {
        if (!icur.RowsInputManualSeted) {
            icur.RowsInput = null;
        }

        icur.RowsInputChangedHandled = false;
        icur.Invalidate();
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

        //item.RowsInput_Changing();
        item.Invalidate_RowsInput();
        item.RowsInputManualSeted = true;

        item.RowsInput = [];

        if (row?.Database is Database db && !db.IsDisposed) {
            item.RowsInput.Add(row);
            row.CheckRowDataIfNeeded();

            if (doAtabaseAfter) { item.RegisterEvents(); }
        }

        item.RowsInput_Changed();
    }

    #endregion
}