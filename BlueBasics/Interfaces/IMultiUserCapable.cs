// Licensed under AGPL-3.0; see License.md for disclaimer and details.

using BlueBasics.Classes;
using System.Collections.Concurrent;

namespace BlueBasics.Interfaces;

public interface IMultiUserCapable {

    #region Fields

    /// <summary>
    /// Registry aller Instanzen, die aktuell erfolgreich Schreibrechte erworben haben.
    /// Schlüssel ist der Dateipfad (<see cref="Filename"/>).
    /// Wird genutzt, um <see cref="RevokeWriteAccessAll"/> ohne Kenntnis des
    /// Caching-Systems auszuführen.
    /// </summary>
    private static readonly ConcurrentDictionary<string, IMultiUserCapable> _writeAccessHolders = new(StringComparer.OrdinalIgnoreCase);

    #endregion

    #region Properties

    string Filename { get; }
    bool UsesBlockFile => true;

    #endregion

    #region Methods

    public static void RevokeWriteAccessAll() {
        var snapshot = _writeAccessHolders.Values.ToList();
        Develop.EndLog($"RevokeWriteAccessAll: {snapshot.Count} Lock(s) zu entfernen");
        var done = 0;
        foreach (var mu in snapshot) {
            done++;
            Develop.EndLog($"RevokeWriteAccessAll: [{done}/{snapshot.Count}] Vor RevokeWriteAccess '{mu.Filename}'");
            mu.RevokeWriteAccess();
            Develop.EndLog($"RevokeWriteAccessAll: [{done}/{snapshot.Count}] Nach RevokeWriteAccess '{mu.Filename}'");
        }
        Develop.EndLog("RevokeWriteAccessAll: ENDE");
    }

    public string AcquireWriteAccess() {
        if (!UsesBlockFile) { return string.Empty; }
        if (Develop.AllReadOnly) { return string.Empty; }

        if (!IsMyLock()) {
            if (BlockFile.BlockerMessage(Filename) is { Length: > 0 } blocker) { return blocker; }

            BlockFile.AcquireWriteAccessFor(Filename);
            if (!IsMyLock()) { return "Schreibrecht konnte nicht erworben werden"; }
        }

        _writeAccessHolders.TryAdd(Filename, this);
        return string.Empty;
    }

    public string BlockerMessage() {
        if (!UsesBlockFile) { return string.Empty; }

        return BlockFile.BlockerMessage(Filename);
    }

    public bool IsMyLock() => !UsesBlockFile || BlockFile.IsMyLockFor(Filename);

    void OnReleasingWriteAccess();

    public void RevokeWriteAccess() {
        if (!UsesBlockFile) { return; }

        BlockFile.RevokeFor(Filename, OnReleasingWriteAccess);
        _writeAccessHolders.TryRemove(Filename, out _);
    }

    #endregion
}
