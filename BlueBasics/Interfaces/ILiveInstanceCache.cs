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

    #region Methods

    /// <summary>
    /// Holt eine bestehende oder erstellt eine neue Live-Instanz für den
    /// angegebenen Dateinamen. Registrierung und Factory werden über das
    /// <see cref="ILiveInstanceCache{T}" />-Interface bezogen.
    /// Gibt <c>null</c> zurück, wenn <paramref name="filename" /> leer ist oder die
    /// Datei nicht existiert.
    /// Race-Safe: konstruieren zwei Threads gleichzeitig eine Instanz für dieselbe
    /// Datei, gewinnt der zuerst eingetragene. Die unterlegene Instanz wird verworfen.
    /// </summary>
    /// <typeparam name="T">Typ der Live-Instanz. Muss <see cref="ILiveInstanceCache{T}" /> implementieren.</typeparam>
    /// <param name="filename">Dateipfad der zu holenden Instanz. Wird normalisiert.</param>
    public static T? GetLiveInstance<T>(string filename) where T : class, IDisposableExtended, ILiveInstanceCache<T> {
        if (string.IsNullOrEmpty(filename)) { return null; }

        var normalizedFileName = filename.NormalizeFile();

        if (!FileExists(normalizedFileName)) { return null; }

        // Bestehende lebende Instanz zurückgeben
        if (T.LiveInstances.TryGetValue(normalizedFileName, out var existing)) {
            if (existing.IsDisposed) {
                T.LiveInstances.TryRemove(normalizedFileName, out _);
            } else {
                return existing;
            }
        }

        // Neue Instanz erzeugen. Die Factory (bzw. der Konstruktor) registriert in LiveInstances.
        var created = T.CreateInstance(normalizedFileName);

        // Race-Schutz: falls ein anderer Thread gleichzeitig konstruiert hat,
        // gewinnt der zuerst eingetragene. Die eigene Instanz wird verworfen.
        var winner = T.LiveInstances.GetOrAdd(normalizedFileName, created);
        if (!ReferenceEquals(winner, created)) {
            created.Dispose();
        }
        return winner;
    }

    #endregion
}