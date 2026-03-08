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
using BlueBasics.Enums;
using BlueBasics.Interfaces;
using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using static BlueBasics.ClassesStatic.Generic;
using static BlueBasics.ClassesStatic.IO;

namespace BlueBasics.Classes;

public abstract class MultiUserFile : CachedFile, IDisposableExtended {

    #region Fields

    /// <summary>
    /// Inhalt der Sperrdatei (zur Prüfung ob wir der Blocker sind).
    /// </summary>
    private string _inhaltBlockdatei = string.Empty;

    /// <summary>
    /// Zähler für Sperrvorgänge.
    /// </summary>
    private int _lockCount;

    #endregion

    #region Constructors

    protected MultiUserFile(string filename) : base(filename) {
        Invalidate(); // Beim Start als "stale" markieren
    }

    #endregion

    #region Properties

    /// <summary>
    /// MultiUserFiles werden nicht gezippt gespeichert.
    /// </summary>
    public override bool MustZipped => false;

    #endregion

    // -----------------------------------------------------------------------
    // Statische Block-Datei-Methoden
    // -----------------------------------------------------------------------

    #region Methods

    /// <summary>
    /// Erstellt eine Blockdatei (.blk) für die angegebene Datei mit dem übergebenen Inhalt.
    /// </summary>
    public static void CreateBlockFile(string filename, string content) {
        var blkName = GetBlockFilename(filename);
        DeleteFile(blkName, 20);
        WriteAllText(blkName, content, Constants.Win1252, false);
    }

    /// <summary>
    /// Liest den Inhalt der Blockdatei (.blk).
    /// </summary>
    public static string ReadBlockFileContent(string filename) {
        var blkName = GetBlockFilename(filename);
        return ReadAllText(blkName, Constants.Win1252);
    }

    /// <summary>
    /// Entsperrt alle Dateien vollständig.
    /// </summary>
    public static void UnlockAllHard() {
        foreach (var thisFile in CachedFileSystem.GetAll<MultiUserFile>()) {
            thisFile.UnlockHard();
        }
    }

    // -----------------------------------------------------------------------

    /// <summary>
    /// Prüft, ob Speichern aktuell erlaubt ist.
    /// </summary>
    public override bool IsSaveAbleNow() {
        if (!base.IsSaveAbleNow()) { return false; }
        if (AgeOfBlockFile(Filename) is >= 0 and <= 3600 && !AmIBlocker()) { return false; }
        return true;
    }

    /// <summary>
    /// Sperrt die Datei zur Bearbeitung.
    /// </summary>
    public bool LockEditing() {
        if (_lockCount > 0) { return true; }

        if (!IsAdministrator()) { return false; }

        if (AgeOfBlockFile(Filename) is < 0 or > 3600) {
            if (Develop.AllReadOnly) { return true; }

            var tmpInhalt = UserName + "\r\n" + DateTime.UtcNow.ToString5() + "\r\nThread: " + Environment.CurrentManagedThreadId + "\r\n" + Environment.MachineName;
            try {
                CreateBlockFile(Filename, tmpInhalt);
                _inhaltBlockdatei = tmpInhalt;
            } catch {
                return false;
            }

            Pause(1, false);

            // Nach dem Warten prüfen, ob wir immer noch der Blocker sind
            if (!AmIBlocker()) { return false; }
        }

        _lockCount++;
        return true;
    }

    /// <summary>
    /// Entsperrt die Datei zur Bearbeitung.
    /// </summary>
    public void UnlockEditing() {
        if (!AmIBlocker()) { return; }

        if (!IsSaved && IsSaveAbleNow()) {
            SaveExtended().GetAwaiter().GetResult();
        }

        _lockCount--;

        if (_lockCount > 0) { return; }

        UnlockHard();
    }

    /// <summary>
    /// Prüft, ob das aktuelle Objekt die Sperrung verwaltet.
    /// </summary>
    internal bool AmIBlocker() {
        if (AgeOfBlockFile(Filename) is < 0 or > 3600) { return false; }

        string inhalt;

        try {
            inhalt = ReadBlockFileContent(Filename);
        } catch {
            return false;
        }

        return _inhaltBlockdatei == inhalt;
    }

    /// <summary>
    /// Gibt das Alter der Blockdatei in Sekunden zurück.
    /// -1 wenn keine Blockdatei vorhanden ist.
    /// </summary>
    private static double AgeOfBlockFile(string filename) {
        var blkName = GetBlockFilename(filename);
        if (!FileExists(blkName)) { return -1; }
        var f = GetFileInfo(blkName);
        if (f == null) { return -1; }
        return Math.Max(0, DateTime.UtcNow.Subtract(f.CreationTimeUtc).TotalSeconds);
    }

    /// <summary>
    /// Gibt den Dateinamen der Blockdatei (.blk) für die angegebene Datei zurück.
    /// </summary>
    private static string GetBlockFilename(string filename) =>
        string.IsNullOrEmpty(filename) ? string.Empty :
        filename.FilePath() + filename.FileNameWithoutSuffix() + ".blk";

    /// <summary>
    /// Entsperrt die Datei vollständig.
    /// </summary>
    private void UnlockHard() {
        if (DeleteFile(GetBlockFilename(Filename), false)) {
            _inhaltBlockdatei = string.Empty;
            _lockCount = 0;
        }
    }

    #endregion
}