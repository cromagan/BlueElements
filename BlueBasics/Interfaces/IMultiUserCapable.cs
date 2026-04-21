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

using BlueBasics.Classes;
using BlueBasics.Classes.FileSystemCaching;
using BlueBasics.ClassesStatic;
using System;
using System.Globalization;
using System.Threading;

namespace BlueBasics.Interfaces;

public interface IMultiUserCapable {

    #region Fields

    private static readonly object _staticLock = new();

    #endregion

    #region Properties

    int CockCount { get; set; }
    int EditTimeInMinutes => 10;
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

    public OperationResult AcquireFullWriteAccess(string baseEditabilityCheck) {
        if (!string.IsNullOrEmpty(baseEditabilityCheck)) {
            return OperationResult.Failed(baseEditabilityCheck);
        }

        if (!UsesBlockFile) { return OperationResult.Success; }
        if (AcquireWriteAccess()) { return OperationResult.Success; }
        return OperationResult.Failed("Schreibrecht konnte nicht erworben werden");
    }

    public bool AcquireWriteAccess() {
        if (CockCount > 0) { return true; }
        if (!TryAcquireWriteAccess()) { return false; }
        CockCount++;
        return true;
    }

    public bool AmIBlocker() {
        if (!UsesBlockFile) { return false; }

        var bf = CachedBlockFile.For(Filename);
        if (bf == null || bf.IsDisposed) { return false; }
        if (bf.IsExpired) { return false; }

        _ = bf.Content;

        var remainingMinutes = EditTimeInMinutes - DateTime.UtcNow.Subtract(bf.TimeUtc).TotalMinutes;
        if (remainingMinutes <= 0) { return false; }

        try {
            return bf.Id == Generic.MyId && bf.User == Generic.UserName;
        } catch {
            return false;
        }
    }

    public string CheckWriteAccess() {
        if (!UsesBlockFile) { return string.Empty; }

        var bf = CachedBlockFile.For(Filename);
        if (bf == null || bf.IsDisposed) { return string.Empty; }

        if (!bf.IsExpired) {
            _ = bf.Content;
            var remainingMinutes = EditTimeInMinutes - DateTime.UtcNow.Subtract(bf.TimeUtc).TotalMinutes;
            if (remainingMinutes > 0) {
                var t = bf.TimeUtc.AddMinutes(EditTimeInMinutes).ToLocalTime().ToString("HH:mm:ss", CultureInfo.InvariantCulture);

                if (bf.User != Generic.UserName) {
                    return $"Aktueller Bearbeiter: {bf.User} noch bis {t}";
                }

                if (bf.App != Develop.AppExe()) {
                    return $"Anderes Programm bearbeitet: {bf.App.FileNameWithoutSuffix()} noch bis {t}";
                }

                if (bf.MachineName != Environment.MachineName) {
                    return $"Anderer Computer bearbeitet: {bf.MachineName} - {bf.User} noch bis {t}";
                }

                if (bf.Id != Generic.MyId) {
                    return $"Ein anderer Prozess auf diesem PC bearbeitet noch bis {t}.";
                }
            }
        }

        return string.Empty;
    }

    public string IsNowEditableWithBlockFile(string baseResult) {
        if (!string.IsNullOrEmpty(baseResult)) { return baseResult; }
        return CheckWriteAccess();
    }

    public bool IsSaveAbleNowWithBlockFile(bool baseResult) {
        if (!baseResult) { return false; }
        if (!UsesBlockFile) { return true; }
        return AmIBlocker();
    }

    void OnReleasingWriteAccess() { }

    public void RevokeWriteAccess() {
        if (CockCount <= 0) { return; }
        CockCount--;
        if (CockCount > 0) { return; }

        OnReleasingWriteAccess();

        if (!AmIBlocker()) { return; }

        var bf = CachedBlockFile.For(Filename);
        if (bf == null || bf.IsDisposed) { return; }
        if (!bf.IsExpired) {
            CachedBlockFile.RevokeWriteAccessFor(Filename);
        }
    }

    private bool HasActiveLock() {
        if (!UsesBlockFile) { return false; }

        var bf = CachedBlockFile.For(Filename);
        if (bf == null || bf.IsDisposed) { return false; }
        if (bf.IsExpired) { return false; }

        _ = bf.Content;
        var remainingMinutes = EditTimeInMinutes - DateTime.UtcNow.Subtract(bf.TimeUtc).TotalMinutes;
        return remainingMinutes > 0;
    }

    private bool TryAcquireWriteAccess() {
        if (Develop.AllReadOnly) { return true; }
        if (!UsesBlockFile) { return true; }

        if (AmIBlocker()) { return true; }
        if (HasActiveLock()) { return false; }

        lock (_staticLock) {
            if (AmIBlocker()) { return true; }
            if (HasActiveLock()) { return false; }

            CachedBlockFile.AcquireWriteAccessFor(Filename);
            try {
                Thread.Sleep(1);
                return AmIBlocker();
            } catch {
                return false;
            }
        }
    }

    #endregion
}