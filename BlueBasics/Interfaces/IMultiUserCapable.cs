// Licensed under AGPL-3.0; see License.md for disclaimer and details.

using BlueBasics.Classes.FileSystemCaching;

namespace BlueBasics.Interfaces;

public interface IMultiUserCapable {

    #region Properties

    string Filename { get; }
    bool UsesBlockFile => true;

    #endregion

    #region Methods

    public static void RevokeWriteAccessAll() {
        foreach (var file in CachedFileSystem.GetAll<CachedFile>()) {
            if (file is IMultiUserCapable mu) {
                mu.RevokeWriteAccess();
            }
        }
    }

    public string AcquireWriteAccess() {
        if (!UsesBlockFile) { return string.Empty; }
        if (Develop.AllReadOnly) { return string.Empty; }
        if (IsMyLock()) { return string.Empty; }

        if (CachedBlockFile.BlockerMessage(Filename) is { Length: > 0 } blocker) {
            return blocker;
        }

        CachedBlockFile.AcquireWriteAccessFor(Filename);
        return IsMyLock() ? string.Empty : "Schreibrecht konnte nicht erworben werden";
    }

    public string BlockerMessage() {
        if (!UsesBlockFile) { return string.Empty; }

        return CachedBlockFile.BlockerMessage(Filename);
    }

    public bool IsMyLock() => !UsesBlockFile || (CachedBlockFile.For(Filename, false)?.IsMyLock ?? false);

    void OnReleasingWriteAccess();

    public void RevokeWriteAccess() {
        if (!UsesBlockFile) { return; }

        var blockFile = CachedBlockFile.For(Filename, false);
        if (blockFile is null) { return; }

        lock (blockFile) {
            if (!blockFile.IsMyLock) { return; }
            OnReleasingWriteAccess();
            IO.DeleteFile(CachedBlockFile.GetBlockFilename(Filename), false);
        }
    }

    #endregion
}