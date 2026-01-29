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

using BlueBasics.ClassesStatic;
using BlueControls.Controls;
using BlueTable.Classes;
using System;
using System.Collections.Specialized;

namespace BlueControls.Classes;

public static class Allgemein {

    #region Fields

    private static bool _serviceStarted;

    #endregion

    #region Methods

    public static void CheckMemory() {
        try {
            var availableMemoryGB = GC.GetTotalMemory(false) / 1024 / 1024 / 1024; // In MB
            var totalSystemMemoryGB = Environment.SystemPageSize * (double)Environment.WorkingSet / 1024 / 1024 / 1024;

            // Wenn mehr als 20% des Systemspeichers oder mehr als 1GB für diese Instanz verwendet wird
            if (availableMemoryGB > Math.Min(totalSystemMemoryGB * 0.2, 8)) {
                BlueFont.TrimAllCaches(1000, 100);
                Generic.CollectGarbage();
            }
        } catch {
            // Fallback, wenn Speicherabfrage fehlschlägt
            BlueFont.TrimAllCaches(1000, 100);
            Generic.CollectGarbage();
        }
    }

    public static void StartGlobalService() {
        if (_serviceStarted) { return; }
        _serviceStarted = true;
        Table.AllFiles.CollectionChanged += AllFiles_CollectionChanged;
        Controls.ConnectedFormula.ConnectedFormula.AllFiles.CollectionChanged += AllFiles_CollectionChanged;
    }

    private static void AllFiles_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e) {
        if (e.NewItems != null) {
            foreach (var thisit in e.NewItems) {
                if (thisit is Table db) {
                    db.AdditionalRepair += TableView.Table_AdditionalRepair;
                    db.CanDoScript += TableView.Table_CanDoScript;
                }
            }
        }

        //if (e.OldItems != null) {
        //    foreach (var thisit in e.OldItems) {
        //        if (thisit is Table db) {
        //            db.AdditionalRepair += TableView.Table_AdditionalRepair;
        //        }
        //    }
        //}

        if (e.Action == NotifyCollectionChangedAction.Reset) {
            Develop.DebugPrint_NichtImplementiert(true);
        }
    }

    #endregion

    //private static void DB_GenerateLayoutInternal(object sender, GenerateLayoutInternalEventArgs e) {
    //    if (e.Handled) { return; }
    //    e.Handled = true;
    //    if (e?.Row?.Table is not Table db) { return; }

    //    var pad = new ItemCollectionPadItem(e.LayoutId, e.Row.Table, e.Row.Key);
    //    pad.SaveAsBitmap(e.Filename);
    //}
}