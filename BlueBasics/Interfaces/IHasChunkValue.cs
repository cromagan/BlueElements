// Licensed under MIT; see License.md for disclaimer, details, and extended user conditions.

using BlueBasics.Classes.Formats;

namespace BlueBasics.Interfaces;

/// <summary>
/// Kennzeichnet Objekte mit einem Chunk-Wert, der über den Namen den
/// Chunk-Unterordner bestimmt.
/// </summary>
public interface IHasChunkValue {

    /// <summary>
    /// Der Chunk-Wert des Objekts; leer, wenn das Objekt keinem Daten-Chunk zugeordnet ist.
    /// </summary>
    string ChunkValue { get; }
}

/// <summary>
/// Statische Erweiterungsmethoden für IHasChunkValue.
/// </summary>
public static class IHasChunkValueExtension {

    #region Methods

    /// <summary>
    /// Die Chunk-ID (lowercase, max. 12 Zeichen), in die das Objekt gehört; ohne
    /// Chunk-Wert der "_"-Chunk. Verworfene Objekte liefern eine leere Zeichenfolge.
    /// </summary>
    public static string ChunkId(this IHasChunkValue item) {
        if (item is IDisposableExtended { IsDisposed: true }) { return string.Empty; }

        return ChunkId(item.ChunkValue);
    }

    /// <summary>
    /// Wandelt einen Chunk-Wert in die Chunk-ID (lowercase, max. 12 Zeichen) um;
    /// ohne verwertbaren Wert der "_"-Chunk.
    /// </summary>
    public static string ChunkId(string chunkValue) {
        var t = SystemNameFormat.MakeValid(chunkValue).TrimStart('_');
        return string.IsNullOrEmpty(t) ? "_" : t.Left(12).ToLowerInvariant();
    }

    #endregion
}
