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
using System;
using System.Globalization;
using System.Threading;

namespace BlueBasics.Interfaces;

public interface IMultiUserCapable {

    #region Properties

    int CockCount { get; set; }
    string Filename { get; }
    int EditTimeInMinutes => 10;
    bool UsesBlockFile => true;

    CachedBlockFile MyBlockFile => CachedBlockFile.For(Filename);

    private static readonly object _staticLock = new();

    #endregion

    #region Methods

    public static void RevokeWriteAccessAll() {
        foreach (var file in CachedFileSystem.GetAll<Classes.FileSystemCaching.CachedFile>()) {
            if (file is IMultiUserCapable mu) {
                mu.RevokeWriteAccess();
            }
        }
    }

    public bool AmIBlocker() {
        var bf = CachedBlockFile.For(Filename);
        if (bf.IsExpired) { return false; }

        try {
            return bf.Id == Generic.MyId && bf.User == Generic.UserName;
        } catch {
            return false;
        }
    }

    public bool GrantWriteAccess() {
        if (CockCount > 0) { return true; }
        if (!TryAcquireWriteAccess()) { return false; }
        CockCount++;
        return true;
    }

    private bool TryAcquireWriteAccess() {
        if (Develop.AllReadOnly) { return true; }

        var bf = CachedBlockFile.For(Filename);

        if (!bf.IsExpired) {
            if (AmIBlocker()) { return true; }
            return false;
        }

        lock (_staticLock) {
            bf = CachedBlockFile.For(Filename);
            if (!bf.IsExpired) {
                if (AmIBlocker()) { return true; }
                return false;
            }

            CachedBlockFile.GrantWriteAccessFor(Filename);
            try {
                Thread.Sleep(1);
                return AmIBlocker();
            } catch {
                return false;
            }
        }
    }

    public string CheckWriteAccess() {
        if (!UsesBlockFile) { return string.Empty; }

        var bf = CachedBlockFile.For(Filename);

        if (!bf.IsExpired) {
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

    public void RevokeWriteAccess() {
        if (CockCount <= 0) { return; }
        CockCount--;
        if (CockCount > 0) { return; }
        OnReleasingWriteAccess();
        if (!AmIBlocker()) { return; }
        var bf = CachedBlockFile.For(Filename);
        if (!bf.IsExpired) {
            CachedBlockFile.RevokeWriteAccessFor(Filename);
        }
    }

    void OnReleasingWriteAccess() { }

    #endregion
}
