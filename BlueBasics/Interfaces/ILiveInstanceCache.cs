// Licensed under AGPL-3.0; see License.md for disclaimer and details.

using System.Collections.Concurrent;
using static BlueBasics.ClassesStatic.IO;

namespace BlueBasics.Interfaces;

/// <summary>
/// Vertrag für Datei-gebundene Objekte (z. B. <c>Chunk</c>, <c>ConnectedFormula</c>),
/// die ein eigenes Register lebender Instanzen verwalten. Die Race-Safe-Logik zum
/// Holen/Erzeugen einer Instanz liegt in <see cref="Classes.LiveInstanceCacheHelper" />.
/// </summary>
/// <typeparam name="T">Typ der Live-Instanz. Muss <see cref="IDisposableExtended" /> sein.</typeparam>
public interface ILiveInstanceCache<T> where T : class, IDisposableExtended {

    #region Properties

    /// <summary>
    /// Eigenes Register aller lebenden Instanzen, geordnet nach normalisiertem Dateinamen.
    /// Schlüsselseitig Case-Insensitive (z. B. <see cref="StringComparer.OrdinalIgnoreCase" />).
    /// </summary>
    static abstract ConcurrentDictionary<string, T> LiveInstances { get; }

    #endregion

    #region Methods

    /// <summary>
    /// Factory: Erzeugt aus dem normalisierten Dateinamen eine neue Instanz.
    /// Die Instanz (bzw. ihr Konstruktor) muss sich selbst in
    /// <see cref="LiveInstances" /> eintragen.
    /// </summary>
    static abstract T CreateInstance(string normalizedFileName);

    #endregion
}

// Licensed under AGPL-3.0; see License.md for disclaimer and details.

/// <summary>
/// Statische Hilfsmethoden für das Verwalten von Live-Instanz-Registern
/// für Datei-gebundene Objekte, die <see cref="ILiveInstanceCache{T}" />
/// implementieren (z. B. <c>Chunk</c>, <c>ConnectedFormula</c>).
/// </summary>
public static class LiveInstanceCacheHelper {

    #region Fields

    // Race-Condition-Schutz für parallele GetLiveInstance-Aufrufe: pro
    // normalisiertem Dateinamen ein eigenes Lock-Objekt, damit die Datei
    // nur von einem Thread geladen wird. Der zweite Aufrufer findet die
    // fertig konstruierte Instanz in LiveInstances vor.
    private static readonly Dictionary<string, object> _loadLocks = new();

    private static readonly object _loadLocksLocker = new object();

    #endregion

    #region Methods

    /// <summary>
    /// Holt eine bestehende oder erstellt eine neue Live-Instanz für den
    /// angegebenen Dateinamen. Registrierung und Factory werden über das
    /// <see cref="ILiveInstanceCache{T}" />-Interface bezogen.
    /// Gibt <c>null</c> zurück, wenn <paramref name="filename" /> leer ist oder die
    /// Datei nicht existiert.
    /// Race-Safe: konstruieren zwei Threads gleichzeitig eine Instanz für dieselbe
    /// Datei, serialisiert ein Per-Dateiname-Lock die Konstruktion. Die Datei
    /// wird nur einmal geladen, beide Aufrufer erhalten dieselbe Instanz.
    /// </summary>
    /// <typeparam name="T">Typ der Live-Instanz. Muss <see cref="ILiveInstanceCache{T}" /> implementieren.</typeparam>
    /// <param name="filename">Dateipfad der zu holenden Instanz. Wird normalisiert.</param>
    public static T? GetLiveInstance<T>(string filename) where T : class, IDisposableExtended, ILiveInstanceCache<T> {
        if (string.IsNullOrEmpty(filename)) { return null; }

        var normalizedFileName = filename.NormalizeFile();

        if (!FileExists(normalizedFileName)) { return null; }

        // Fast Path: bestehende lebende Instanz ohne Lock zurückgeben.
        // Der teure Konstruktions-/Lade-Pfad wird nur betreten, wenn wirklich
        // keine verwendbare Instanz existiert.
        if (T.LiveInstances.TryGetValue(normalizedFileName, out var existing)) {
            if (existing.IsDisposed) {
                T.LiveInstances.TryRemove(normalizedFileName, out _);
            } else {
                return existing;
            }
        }

        // Per-Dateiname-Lock ermitteln. Serialisiert parallele
        // Konstruktionsversuche für dieselbe Datei, damit die Factory
        // (und damit das Laden der Datei) nur einmal aufgerufen wird.
        object perNameLock;
        lock (_loadLocksLocker) {
            if (!_loadLocks.TryGetValue(normalizedFileName, out var lk)) {
                lk = new object();
                _loadLocks[normalizedFileName] = lk;
            }
            perNameLock = lk;
        }

        lock (perNameLock) {
            // Double-Check: ein anderer Thread könnte die Instanz inzwischen
            // konstruiert und eingetragen haben.
            if (T.LiveInstances.TryGetValue(normalizedFileName, out var raceWinner)) {
                if (raceWinner.IsDisposed) {
                    T.LiveInstances.TryRemove(normalizedFileName, out _);
                } else {
                    return raceWinner;
                }
            }

            // Neue Instanz erzeugen. Die Factory (bzw. der Konstruktor)
            // registriert sich selbst in LiveInstances.
            var created = T.CreateInstance(normalizedFileName);

            // Sicherheitshalber GetOrAdd: falls trotz Lock ein anderer Thread
            // eingetragen hat, gewinnt der bereits eingetragene. Die eigene
            // Instanz wird verworfen.
            var winner = T.LiveInstances.GetOrAdd(normalizedFileName, created);
            if (!ReferenceEquals(winner, created)) {
                created.Dispose();
            }
            return winner;
        }
    }

    #endregion
}