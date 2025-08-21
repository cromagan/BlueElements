// Authors:
// Christian Peter
//
// Copyright (c) 2025 Christian Peter
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
using BlueDatabase.EventArgs;
using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading;

namespace BlueDatabase;

public class InvalidatedRowsManager {

    #region Fields

    // ConcurrentDictionary für threadsichere Sammlung der ungültigen Zeilen (Key = KeyName, Value = RowItem)
    private readonly ConcurrentDictionary<string, RowItem> _invalidatedRows = new ConcurrentDictionary<string, RowItem>();

    // Lock-Objekt nur für den Verarbeitungsstatus
    private readonly object _processingLock = new object();

    //// ConcurrentDictionary für threadsichere Nachverfolgung verarbeiteter Einträge
    //private readonly ConcurrentDictionary<string, bool> _processedRowIds = new ConcurrentDictionary<string, bool>();
    // Flag für laufende Verarbeitung
    private volatile bool _isProcessing = false;

    #endregion

    #region Events

    public event EventHandler<RowEventArgs>? RowChecked;

    #endregion

    #region Properties

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
    /// Fügt ein neues Row-Item zur Sammlung hinzu, wenn es nicht bereits vorhanden
    /// oder als verarbeitet markiert ist.
    /// </summary>
    /// <param name="rowItem">Das hinzuzufügende Row-Item</param>
    /// <returns>True wenn das Item hinzugefügt wurde, False wenn nicht</returns>
    public bool AddInvalidatedRow(RowItem? rowItem) {
        if (rowItem?.Database is not { IsDisposed: false } db) { return false; }

        // Ansosten ist Endloschleife mit Monitor
        if (!db.CanDoValueChangedScript(false)) { return false; }

        //// Prüfe, ob die Zeile bereits als verarbeitet markiert ist
        //if (_processedRowIds.ContainsKey(rowItem.KeyName)) {
        //    return false;
        //}

        // Prüfe, ob die Zeile bereits als verarbeitet markiert ist
        if (_invalidatedRows.ContainsKey(rowItem.KeyName)) {
            return false;
        }

        Develop.Message?.Invoke(ErrorType.Info, this, "Row", ImageCode.Zeile, $"Neuer Job (Offen: {_invalidatedRows.Count + 1}) durch neue invalide Zeile: {rowItem.CellFirstString()} der Tabelle {db.Caption}", 0);

        // Prüfe, ob die Zeile bereits in der Sammlung ist und füge sie hinzu, falls nicht
        return _invalidatedRows.TryAdd(rowItem.KeyName, rowItem);
    }

    /// <summary>
    /// Verarbeitet alle ungültigen Zeilen. Verhindert parallele Aufrufe.
    /// Verarbeitet auch Zeilen, die während der Ausführung hinzugekommen sind.
    /// </summary>
    /// <param name="masterRow">Die Hauptzeile, falls vorhanden</param>
    /// <param name="extendedAllowed">Flag für erweiterte Verarbeitung</param>
    public void DoAllInvalidatedRows(RowItem? masterRow, bool extendedAllowed, Action? minutelyDelegate) {
        lock (_processingLock) {
            if (_isProcessing) { return; }
            _isProcessing = true;
        }
        try {
            if (_invalidatedRows.Count > 0) {
                Develop.Message?.Invoke(ErrorType.Info, this, "InvalidatetRowManager", ImageCode.Taschenrechner, $"Arbeite {_invalidatedRows.Count} invalide Zeilen ab", 0);
            } else {
                //Develop.Message?.Invoke(ErrorType.DevelopInfo, this, "InvalidatetRowManager", ImageCode.Taschenrechner, "Keine invaliden Zeilen bekannt", 0);
                return;
            }

            var totalProcessedCount = 0;
            var entriesBeforeProcessing = 0;
            var lastDelegateCall = DateTime.MinValue;

            // Verarbeite in einer Schleife, bis keine Einträge mehr vorhanden sind
            do {
                // Prüfe, ob der Delegat aufgerufen werden soll (mindestens 1 Minute vergangen)
                var currentTime = DateTime.Now;
                if (minutelyDelegate != null && currentTime - lastDelegateCall >= TimeSpan.FromMinutes(1)) {
                    minutelyDelegate();
                    lastDelegateCall = currentTime;
                }

                // Sammle alle aktuellen Schlüssel
                var keysToProcess = _invalidatedRows.Keys.ToList();
                if (keysToProcess.Count == 0) {
                    masterRow?.DropMessage(ErrorType.DevelopInfo, $"Alle Einträge abgearbeitet");
                    break;
                }

                // Prüfe, ob neue Einträge hinzugekommen sind
                var newEntries = keysToProcess.Count - entriesBeforeProcessing;

                // Gib eine Meldung aus, wenn neue Einträge hinzugekommen sind
                if (newEntries > 0) {
                    Develop.Message?.Invoke(ErrorType.Info, this, "InvalidatetRowManager", ImageCode.Stern, $"{newEntries} neue Einträge zum Abarbeiten", 0);
                }

                // Anzahl der zu verarbeitenden Zeilen vor der Verarbeitung merken
                entriesBeforeProcessing = keysToProcess.Count;

                // Verarbeite alle Zeilen
                foreach (var key in keysToProcess) {
                    // Versuche, die Zeile zu entfernen und zu verarbeiten
                    if (_invalidatedRows.TryRemove(key, out var row) && row != null) {
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

            Develop.Message?.Invoke(ErrorType.DevelopInfo, this, "InvalidatetRowManager", ImageCode.Taschenrechner, $"Abarbeitung invalider Zeilen fertig", 0);
        } catch {
            Develop.Message?.Invoke(ErrorType.Warning, this, "InvalidatetRowManager", ImageCode.Taschenrechner, $"Abarbeitung invalider Zeilen unerwartet abgebrochen", 0);
        } finally {
            // Verarbeitung beenden, egal was passiert
            lock (_processingLock) {
                _isProcessing = false;
            }
        }
    }

    /// <summary>
    /// Markiert eine Zeile als verarbeitet, ohne sie tatsächlich zu verarbeiten.
    /// Entfernt die Zeile aus der Liste der zu verarbeitenden Einträge, falls vorhanden.
    /// </summary>
    /// <param name="rowItem">Die als verarbeitet zu markierende Zeile</param>
    /// <returns>True wenn die Zeile erfolgreich markiert wurde, False wenn nicht</returns>
    public bool MarkAsProcessed(RowItem? rowItem) {
        if (rowItem == null) { return false; }

        // Versuche die Zeile aus der Liste der zu verarbeitenden zu entfernen
        return _invalidatedRows.TryRemove(rowItem.KeyName, out _);

        //// Markiere die Zeile als verarbeitet
        //return _processedRowIds.TryAdd(rowItem.KeyName, true);
    }

    private void OnRowChecked(RowEventArgs e) {
        RowChecked?.Invoke(this, e);
    }

    /// <summary>
    /// Verarbeitet eine einzelne Zeile mit der tatsächlichen Verarbeitungslogik.
    /// </summary>
    /// <param name="row">Die zu verarbeitende Zeile</param>
    /// <param name="masterRow">Die Hauptzeile, falls vorhanden</param>
    /// <param name="extendedAllowed">Flag für erweiterte Verarbeitung</param>
    /// <param name="currentIndex">Aktueller Index für Statusmeldungen</param>
    private void ProcessSingleRow(RowItem row, RowItem? masterRow, bool extendedAllowed, int currentIndex) {
        if (row.Database is not { IsDisposed: false } db) { return; }

        if (!extendedAllowed && row.NeedsRowInitialization()) {
            masterRow?.DropMessage(ErrorType.Info, $"Nr. {currentIndex}  (Offen: {_invalidatedRows.Count + 1}): Zeile {db.Caption} / {row.CellFirstString()} abbruch, benötigt Initialisierung");
            return;
        }

        if (!row.NeedsRowUpdate()) {
            masterRow?.DropMessage(ErrorType.Info, $"Nr. {currentIndex} (Offen: {_invalidatedRows.Count + 1}): Zeile {db.Caption} / {row.CellFirstString()} bereits aktuell");
            return;
        }

        masterRow?.DropMessage(ErrorType.Info, $"Nr. {currentIndex} (Offen: {_invalidatedRows.Count + 1}): Aktualisiere {db.Caption} / {row.CellFirstString()}");

        if (masterRow?.Database != null) {
            _ = row.UpdateRow(extendedAllowed, true, "Update von " + masterRow?.CellFirstString());
        } else {
            _ = row.UpdateRow(extendedAllowed, true, "Normales Update");
        }
    }

    #endregion
}