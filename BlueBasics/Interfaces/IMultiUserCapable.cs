// Authors:
// Christian Peter
//
// Copyright © 2026 Christian Peter
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

using BlueBasics.Classes.FileSystemCaching;
using BlueBasics.ClassesStatic;
using System.Threading;

namespace BlueBasics.Interfaces;

public interface IMultiUserCapable {

    #region Fields

    private static readonly object _staticLock = new();

    #endregion

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

        if (IsMyLock()) { return true; }
        if (Develop.AllReadOnly) { return true; }

        lock (_staticLock) {
            if (IsMyLock()) { return true; }

            CachedBlockFile.AcquireWriteAccessFor(Filename);
            try {
                Thread.Sleep(1);
                return IsMyLock();
            } catch {
            }
        }
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

        var del = IsExpired();

        if (IsMyLock()) {
            OnReleasingWriteAccess();
            del = true;
        }

        if (del) {
            IO.DeleteFile(CachedBlockFile.GetBlockFilename(Filename), false);
        }
    }

    #endregion
}