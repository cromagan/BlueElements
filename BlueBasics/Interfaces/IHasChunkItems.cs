// Licensed under MIT; see License.md for disclaimer, details, and extended user conditions.

namespace BlueBasics.Interfaces;

/// <summary>
/// Liefert die Items, die einen Chunk-Wert tragen (IHasKeyName + IHasChunkValue);
/// Quelle der Chunk-Verteilung im Chunk-Datei-Layout.
/// </summary>
public interface IHasChunkItems {

    /// <summary>
    /// Die Items mit Chunk-Wert; Einträge ohne IHasChunkValue bleiben unberücksichtigt.
    /// </summary>
    IEnumerable<IHasKeyName> ChunkItems { get; }
}
