// Licensed under AGPL-3.0; see License.md for disclaimer and details.

using BlueBasics.Classes.FileSystemCaching;
using System.Threading;

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

    public bool AcquireWriteAccess() {
        if (!UsesBlockFile) { return true; }
        if (Develop.AllReadOnly) { return true; }
        if (IsMyLock()) { return true; }

        if (CachedBlockFile.BlockerMessage(Filename) is { Length: > 0 }) {
            return false;
        }

        CachedBlockFile.AcquireWriteAccessFor(Filename);
        try {
            Thread.Sleep(1);
            return IsMyLock();
        } catch { }
        return false;
    }

    public string BlockerMessage() {
        if (!UsesBlockFile) { return string.Empty; }

        return CachedBlockFile.BlockerMessage(Filename);
    }

    public bool IsExpired() => CachedBlockFile.IsExpired(Filename);

    public bool IsMyLock() => !UsesBlockFile || CachedBlockFile.IsMyLock(Filename);

    void OnReleasingWriteAccess();

    public void RevokeWriteAccess() {
        if (!UsesBlockFile) { return; }
        if (!IsMyLock()) { return; }

        OnReleasingWriteAccess();
        IO.DeleteFile(CachedBlockFile.GetBlockFilename(Filename), false);
    }

    #endregion
}