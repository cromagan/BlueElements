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
using System.Collections.Concurrent;
using System.Linq;
using System.Threading;

namespace BlueDatabase;

public class InvalidatedRowsManager {

    #region Fields

    // ConcurrentDictionary für threadsichere Sammlung der ungültigen Zeilen (Key = KeyName, Value = RowItem)
    private readonly ConcurrentDictionary<string, RowItem> _invalidatedRows = new ConcurrentDictionary<string, RowItem>();

    // ConcurrentDictionary für threadsichere Nachverfolgung verarbeiteter Einträge
    private readonly ConcurrentDictionary<string, bool> _processedRowIds = new ConcurrentDictionary<string, bool>();

    // Lock-Objekt nur für den Verarbeitungsstatus
    private readonly object _processingLock = new object();

    // Flag für laufende Verarbeitung
    private volatile bool _isProcessing = false;

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
    public int PendingRowsCount {
        get {
            return _invalidatedRows.Count;
        }
    }

    /// <summary>
    /// Gibt die aktuelle Anzahl der bereits verarbeiteten Zeilen zurück.
    /// </summary>
    public int ProcessedRowsCount {
        get {
            return _processedRowIds.Count;
        }
    }

    #endregion

    #region Methods

    /// <summary>
    /// Fügt ein neues Row-Item zur Sammlung hinzu, wenn es nicht bereits vorhanden
    /// oder als verarbeitet markiert ist.
    /// </summary>
    /// <param name="rowItem">Das hinzuzufügende Row-Item</param>
    /// <returns>True wenn das Item hinzugefügt wurde, False wenn nicht</returns>
    public bool AddInvalidatedRow(RowItem? rowItem) {
        if (rowItem == null) { return false; }

        // Prüfe, ob die Zeile bereits als verarbeitet markiert ist
        if (_processedRowIds.ContainsKey(rowItem.KeyName)) {
            return false;
        }

        // Prüfe, ob die Zeile bereits in der Sammlung ist und füge sie hinzu, falls nicht
        return _invalidatedRows.TryAdd(rowItem.KeyName, rowItem);
    }

    /// <summary>
    /// Verarbeitet alle ungültigen Zeilen. Verhindert parallele Aufrufe.
    /// Verarbeitet auch Zeilen, die während der Ausführung hinzugekommen sind.
    /// </summary>
    /// <param name="masterRow">Die Hauptzeile, falls vorhanden</param>
    /// <param name="extendedAllowed">Flag für erweiterte Verarbeitung</param>
    public void DoAllInvalidatedRows(RowItem? masterRow, bool extendedAllowed) {
        lock (_processingLock) {
            if (_isProcessing) { return; }
            _isProcessing = true;
        }
        var fehlercount = 0;





        try {

            Develop.MonitorMessage?.Invoke("InvalidatetRowManager", "", $"Arbeite {_invalidatedRows.Keys.ToList().Count()} invalide Zeilen ab", 0);
            int totalProcessedCount = 0;
            var entriesBeforeProcessing = 0;

            // Verarbeite in einer Schleife, bis keine Einträge mehr vorhanden sind
            do {
                // Sammle alle aktuellen Schlüssel
                var keysToProcess = _invalidatedRows.Keys.ToList();
                if (keysToProcess.Count == 0) { break; }// Keine Einträge mehr vorhanden

                // Prüfe, ob neue Einträge hinzugekommen sind
                int newEntries = keysToProcess.Count - entriesBeforeProcessing;

                // Gib eine Meldung aus, wenn neue Einträge hinzugekommen sind
                if (newEntries > 0) {
                    masterRow?.OnDropMessage(ErrorType.Info, $"{newEntries} neue Einträge zum Abarbeiten ({keysToProcess.Count + _processedRowIds.Count} insgesamt)");
                }

                // Anzahl der zu verarbeitenden Zeilen vor der Verarbeitung merken
                entriesBeforeProcessing = keysToProcess.Count;

                // Verarbeite alle Zeilen
                foreach (var key in keysToProcess) {
                    // Versuche, die Zeile zu entfernen und zu verarbeiten
                    if (_invalidatedRows.TryRemove(key, out var row) && row != null) {
                        // Verarbeite die Zeile
                        ProcessSingleRow(row, masterRow, extendedAllowed, totalProcessedCount + 1);

                        // Markiere als verarbeitet
                        _processedRowIds[key] = true;

                        totalProcessedCount++;
                    } else {
                        masterRow?.OnDropMessage(ErrorType.Warning, $"Fehler beim Abarbeiten.");
                        Thread.Sleep(1000);
                        fehlercount++;
                        if (fehlercount > 20) {
                            masterRow?.OnDropMessage(ErrorType.Warning, $"Abbruch wegen zu vieler Fehler.");
                            break;
                        }
                    }
                }

                if (fehlercount > 20) { break; }

                Thread.Sleep(10);     // Eine kurze Pause, um anderen Threads Zeit zu geben
            } while (true);

            Develop.MonitorMessage?.Invoke("InvalidatetRowManager", "", $"InvalidatetRowManager fertig", 0);

        } catch {
            Develop.MonitorMessage?.Invoke("InvalidatetRowManager", "", $"InvalidatetRowManager unerwartet abgebrochen", 0);


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
        _invalidatedRows.TryRemove(rowItem.KeyName, out _);

        // Markiere die Zeile als verarbeitet
        return _processedRowIds.TryAdd(rowItem.KeyName, true);
    }

    /// <summary>
    /// Verarbeitet eine einzelne Zeile mit der tatsächlichen Verarbeitungslogik.
    /// </summary>
    /// <param name="row">Die zu verarbeitende Zeile</param>
    /// <param name="masterRow">Die Hauptzeile, falls vorhanden</param>
    /// <param name="extendedAllowed">Flag für erweiterte Verarbeitung</param>
    /// <param name="currentIndex">Aktueller Index für Statusmeldungen</param>
    private void ProcessSingleRow(RowItem row, RowItem? masterRow, bool extendedAllowed, int currentIndex) {
        if (row.Database is not { } db) { return; }

        if (row.NeedsRowUpdate(false, true)) {
            if (masterRow?.Database != null) {
                masterRow.OnDropMessage(ErrorType.Info, $"Nr. {currentIndex} von {PendingRowsCount}: Aktualisiere {db.Caption} / {row.CellFirstString()}");
                row.UpdateRow(extendedAllowed, true, "Update von " + masterRow.CellFirstString());
            } else {
                row.UpdateRow(extendedAllowed, true, "Normales Update");
            }
        }
    }

    #endregion
}