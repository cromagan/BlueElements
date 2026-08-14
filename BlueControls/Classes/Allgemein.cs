// Licensed under AGPL-3.0; see License.md for disclaimer and details.

using BlueControls.Controls;

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
                TrimAllCaches();
                CollectGarbage();
            }
        } catch {
            // Fallback, wenn Speicherabfrage fehlschlägt
            TrimAllCaches();
            CollectGarbage();
        }
    }

    public static void StartGlobalService() {
        if (_serviceStarted) { return; }
        _serviceStarted = true;
        Table.Added += Table_Added;

        // Bereits geladene Tabellen nachträglich behandeln, falls sie vor
        // StartGlobalService erzeugt wurden (Added würde sie sonst verpassen).
        foreach (var tb in Table.AllInstances()) {
            Table_Added(null, new LiveInstanceEventArgs<Table>(tb));
        }
    }

    private static void Table_Added(object? sender, LiveInstanceEventArgs<Table> e) {
        var tb = e.Instance;
        tb.AdditionalRepair += TableView.Table_AdditionalRepair;
        tb.CanDoScript += TableView.Table_CanDoScript;
    }

    #endregion
}
