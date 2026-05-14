// Licensed under AGPL-3.0; see License.md for disclaimer and details.

using BlueControls.Controls;
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
                Generic.TrimAllCaches();
                Generic.CollectGarbage();
            }
        } catch {
            // Fallback, wenn Speicherabfrage fehlschlägt
            Generic.TrimAllCaches();
            Generic.CollectGarbage();
        }
    }

    public static void StartGlobalService() {
        if (_serviceStarted) { return; }
        _serviceStarted = true;
        Table.AllFiles.CollectionChanged += AllFiles_CollectionChanged;
        //Controls.ConnectedFormula.ConnectedFormula.AllFiles.CollectionChanged += AllFiles_CollectionChanged;
    }

    private static void AllFiles_CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e) {
        if (e.NewItems != null) {
            foreach (var thisit in e.NewItems) {
                if (thisit is Table tb) {
                    tb.AdditionalRepair += TableView.Table_AdditionalRepair;
                    tb.CanDoScript += TableView.Table_CanDoScript;
                }
            }
        }

        //if (e.OldItems != null) {
        //    foreach (var thisit in e.OldItems) {
        //        if (thisit is Table tb) {
        //            tb.AdditionalRepair += TableView.Table_AdditionalRepair;
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
    //    if (e?.Row?.Table is not Table tb) { return; }

    //    var pad = new ItemCollectionPadItem(e.LayoutId, e.Row.Table, e.Row.Key);
    //    pad.SaveAsBitmap(e.Filename);
    //}
}