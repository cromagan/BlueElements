// Licensed under AGPL-3.0; see License.md for disclaimer and details.

using BlueBasics.Classes;
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

        if (BlockFile.BlockerMessage(Filename) is { Length: > 0 } blocker) {
            return blocker;
        }

        BlockFile.AcquireWriteAccessFor(Filename);
        return IsMyLock() ? string.Empty : "Schreibrecht konnte nicht erworben werden";
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
    }

    #endregion
}