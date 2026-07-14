// Licensed under AGPL-3.0; see License.md for disclaimer and details.

using BlueTable.EventArgs;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Threading;

namespace BlueTable.Classes;

public class InvalidatedRowsManager {

    #region Fields

    // ConcurrentDictionary für threadsichere Sammlung der ungültigen Zeilen (Key = KeyName, Value = RowItem)
    private readonly ConcurrentDictionary<string, RowItem> _invalidatedRows = new();

    // Lock-Objekt nur für den Verarbeitungsstatus
    private readonly object _processingLock = new();

    //// ConcurrentDictionary für threadsichere Nachverfolgung verarbeiteter Einträge
    //private readonly ConcurrentDictionary<string, bool> _processedRowIds = new ConcurrentDictionary<string, bool>();
    // Flag für laufende Verarbeitung
    private volatile bool _isProcessing;

    #endregion

    #region Delegates

    public delegate void DGDoUpdateRow(RowItem? masterRow, RowItem row, bool extendedAllowed);

    #endregion

    #region Events

    public event EventHandler<RowEventArgs>? RowChecked;

    #endregion

    #region Properties

    /// <summary>
    /// Eine Routine, die sich um das Update der Row kümmert. Kann evtl. umgeleitet werden. Ruft im Regelfall UpdateRowNow auf.
    /// </summary>
    public static DGDoUpdateRow DoUpdateRow { get; set; } = UpdateRowNow;

    /// <summary>
    /// Überprüft, ob gerade eine Verarbeitung stattfindet.
    /// Thread-sicher implementiert.
    /// </summary>
    public bool IsProcessing {
        get {
            lock (_processingLock) {
                return _isProcessing;
            }
        }
    }

    /// <summary>
    /// Gibt die aktuelle Anzahl der zu verarbeitenden Zeilen zurück.
    /// </summary>
    public int PendingRowsCount => _invalidatedRows.Count;

    #endregion

    #region Methods

    /// <summary>
    /// Eine Routine, die direkt das Update macht. Im Gegensatz zu DoUpdateRow - das ruft evl. eine Umleitung auf.
    /// </summary>
    /// <param name="masterRow"></param>
    /// <param name="row"></param>
    /// <param name="extendedAllowed"></param>
    public static void UpdateRowNow(RowItem? masterRow, RowItem row, bool extendedAllowed) {
        if (masterRow?.Table is not null) {
            row.UpdateRow(extendedAllowed, "Update von " + masterRow.ReadableText());
        } else {
            row.UpdateRow(extendedAllowed, "Normales Update");
        }
    }

    /// <summary>
    /// Fügt ein neues Row-Item zur Sammlung hinzu, wenn es nicht bereits vorhanden
    /// oder als verarbeitet markiert ist.
    /// </summary>
    /// <param name="rowItem">Das hinzuzufügende Row-Item</param>
    /// <returns>True wenn das Item hinzugefügt wurde, False wenn nicht</returns>
    public bool AddInvalidatedRow(RowItem? rowItem) {
        if (rowItem?.Table is not { IsDisposed: false } tb) { return false; }

        // Ansosten ist Endloschleife mit Monitor
        if (!tb.CanDoValueChangedScript(false)) { return false; }

        //// Prüfe, ob die Zeile bereits als verarbeitet markiert ist
        //if (_processedRowIds.ContainsKey(rowItem.KeyName)) {
        //    return false;
        //}

        // Prüfe, ob die Zeile bereits als verarbeitet markiert ist
        if (_invalidatedRows.ContainsKey(rowItem.KeyName)) {
            return false;
        }

        Develop.Message(ErrorType.Info, this, "Row", ImageCode.Zeile, $"Neuer Job (Offen: {_invalidatedRows.Count + 1}) durch neue invalide Zeile: {rowItem.CellFirstString()} der Tabelle {tb.Caption}", 0);

        // Prüfe, ob die Zeile bereits in der Sammlung ist und füge sie hinzu, falls nicht
        return _invalidatedRows.TryAdd(rowItem.KeyName, rowItem);
    }

    /// <summary>
    /// Verarbeitet alle ungültigen Zeilen. Verhindert parallele Aufrufe.
    /// Verarbeitet auch Zeilen, die während der Ausführung hinzugekommen sind.
    /// </summary>
    /// <param name="masterRow">Die Hauptzeile, falls vorhanden</param>
    /// <param name="extendedAllowed">Flag für erweiterte Verarbeitung</param>
    /// <param name="minutelyDelegate"></param>
    public void DoAllInvalidatedRows(RowItem? masterRow, bool extendedAllowed, Action? minutelyDelegate) {
        lock (_processingLock) {
            if (_isProcessing) { return; }
            _isProcessing = true;
        }
        try {
            if (!_invalidatedRows.IsEmpty) {
                Develop.Message(ErrorType.Info, this, "InvalidatetRowManager", ImageCode.Taschenrechner, $"Arbeite {_invalidatedRows.Count} invalide Zeilen ab", 0);
            } else {
                //Develop.Message(ErrorType.DevelopInfo, this, "InvalidatetRowManager", ImageCode.Taschenrechner, "Keine invaliden Zeilen bekannt", 0);
                return;
            }

            var totalProcessedCount = 0;
            var entriesBeforeProcessing = 0;
            var lastDelegateCall = DateTime.MinValue;
            var processingStart = Stopwatch.StartNew();

            // Verarbeite in einer Schleife, bis keine Einträge mehr vorhanden sind
            do {
                if (processingStart.ElapsedMilliseconds > 60 * 1000) {
                    Develop.Message(ErrorType.DevelopInfo, this, "InvalidatetRowManager", ImageCode.Taschenrechner, $"Abarbeitung nach 60s Timeout abgebrochen, {_invalidatedRows.Count} Zeilen verbleibend", 0);
                    break;
                }

                if (Develop.GetUserIdleSeconds() < 1) {
                    Develop.Message(ErrorType.DevelopInfo, this, "InvalidatetRowManager", ImageCode.Taschenrechner, $"Abarbeitung wegen User-Aktion abgebrochen, {_invalidatedRows.Count} Zeilen verbleibend", 0);
                    break;
                }
                // Prüfe, ob der Delegat aufgerufen werden soll (mindestens 1 Minute vergangen)
                var currentTime = DateTime.Now;
                if (minutelyDelegate is not null && currentTime - lastDelegateCall >= TimeSpan.FromMinutes(1)) {
                    minutelyDelegate();
                    lastDelegateCall = currentTime;
                }

                // Sammle alle aktuellen Schlüssel
                var keysToProcess = _invalidatedRows.Keys.ToList();
                if (keysToProcess.Count == 0) {
                    if (masterRow is { IsDisposed: false } mr && mr.Table is { IsDisposed: false } mrtb && mrtb.DropMessages) {
                        Develop.Message(ErrorType.DevelopInfo, mr, mrtb.Caption, ImageCode.Zeile, $"Alle Einträge abgearbeitet", 0);
                    }
                    break;
                }

                // Prüfe, ob neue Einträge hinzugekommen sind
                var newEntries = keysToProcess.Count - entriesBeforeProcessing;

                // Gib eine Meldung aus, wenn neue Einträge hinzugekommen sind
                if (newEntries > 0) {
                    Develop.Message(ErrorType.Info, this, "InvalidatetRowManager", ImageCode.Stern, $"{newEntries} neue Einträge zum Abarbeiten", 0);
                }

                // Anzahl der zu verarbeitenden Zeilen vor der Verarbeitung merken
                entriesBeforeProcessing = keysToProcess.Count;

                // Verarbeite alle Zeilen
                foreach (var key in keysToProcess) {
                    if (_invalidatedRows.TryGetValue(key, out var rowx) && rowx?.Table is { IsDisposed: false } tbl) {
                        var f = tbl.ExternalAbortScriptReasonExtended();
                        if (!string.IsNullOrEmpty(f)) {
                            Develop.Message(ErrorType.DevelopInfo, this, "InvalidatetRowManager", ImageCode.Taschenrechner, $"Abarbeitung invalider Zeilen abgebrochen: {f}", 0);
                            return;
                        }
                    }

                    // Versuche, die Zeile zu entfernen und zu verarbeiten
                    if (_invalidatedRows.TryRemove(key, out var row) && row is not null) {
                        // Verarbeite die Zeile
                        ProcessSingleRow(row, masterRow, extendedAllowed, totalProcessedCount + 1);
                        totalProcessedCount++;
                        OnRowChecked(new RowEventArgs(row));
                    }
                    // KEIN else-Zweig mehr: TryRemove() Fehlschlag ist normal bei Concurrency
                    // Andere Threads können das Item bereits verarbeitet haben
                }

                Thread.Sleep(10);     // Eine kurze Pause, um anderen Threads Zeit zu geben
            } while (true);

            Develop.Message(ErrorType.DevelopInfo, this, "InvalidatetRowManager", ImageCode.Taschenrechner, $"Abarbeitung invalider Zeilen fertig", 0);
        } catch (Exception ex) {
            Develop.Message(ErrorType.Warning, this, "InvalidatetRowManager", ImageCode.Taschenrechner, $"Abarbeitung invalider Zeilen unerwartet abgebrochen: {ex.ToString()}", 0);
        } finally {
            // Verarbeitung beenden, egal was passiert
            lock (_processingLock) {
                _isProcessing = false;
            }
        }
    }

    /// <summary>
    /// Entfernt die Zeile aus der Liste der zu verarbeitenden Einträge, falls vorhanden.
    /// </summary>
    /// <param name="rowItem">Die zu entfernende Zeile</param>
    public void MarkAsProcessed(RowItem? rowItem) {
        if (rowItem is null) { return; }

        // Versuche die Zeile aus der Liste der zu verarbeitenden zu entfernen
        _invalidatedRows.TryRemove(rowItem.KeyName, out _);
    }

    private void OnRowChecked(RowEventArgs e) => RowChecked?.Invoke(this, e);

    /// <summary>
    /// Verarbeitet eine einzelne Zeile mit der tatsächlichen Verarbeitungslogik.
    /// </summary>
    /// <param name="row">Die zu verarbeitende Zeile</param>
    /// <param name="masterRow">Die Hauptzeile, falls vorhanden</param>
    /// <param name="extendedAllowed">Flag für erweiterte Verarbeitung</param>
    /// <param name="currentIndex">Aktueller Index für Statusmeldungen</param>
    private void ProcessSingleRow(RowItem row, RowItem? masterRow, bool extendedAllowed, int currentIndex) {
        if (row.Table is not { IsDisposed: false } tb) { return; }

        if (!extendedAllowed && row.NeedsRowInitialization()) {
            if (masterRow is { IsDisposed: false } mr && mr.Table is { IsDisposed: false } mrtb && mrtb.DropMessages) {
                Develop.Message(ErrorType.Info, mr, mrtb.Caption, ImageCode.Zeile, $"Nr. {currentIndex}  (Offen: {_invalidatedRows.Count + 1}): Zeile {tb.Caption} / {row.ReadableText()} abbruch, benötigt Initialisierung", 0);
            }
            return;
        }

        if (!row.NeedsRowUpdate()) {
            if (masterRow is { IsDisposed: false } mr2 && mr2.Table is { IsDisposed: false } mrtb2 && mrtb2.DropMessages) {
                Develop.Message(ErrorType.Info, mr2, mrtb2.Caption, ImageCode.Zeile, $"Nr. {currentIndex} (Offen: {_invalidatedRows.Count + 1}): Zeile {tb.Caption} / {row.ReadableText()} bereits aktuell", 0);
            }
            return;
        }

        if (masterRow is { IsDisposed: false } mr3 && mr3.Table is { IsDisposed: false } mrtb3 && mrtb3.DropMessages) {
            Develop.Message(ErrorType.Info, mr3, mrtb3.Caption, ImageCode.Zeile, $"Nr. {currentIndex} (Offen: {_invalidatedRows.Count + 1}): Aktualisiere {tb.Caption} / {row.ReadableText()}", 0);
        }

        DoUpdateRow?.Invoke(masterRow, row, extendedAllowed);
    }

    #endregion
}