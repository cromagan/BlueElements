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

using BlueBasics.ClassesStatic;
using BlueBasics.Enums;
using BlueBasics.EventArgs;
using BlueBasics.Interfaces;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading;
using static BlueBasics.ClassesStatic.Generic;
using static BlueBasics.ClassesStatic.IO;
using Timer = System.Threading.Timer;

namespace BlueBasics.Classes;

public abstract class MultiUserFile : IDisposableExtended, IHasKeyName, IParseable, INotifyPropertyChanged {

    #region Fields

    private static readonly List<MultiUserFile> AllFiles = [];
    private readonly Timer _checker;
    private readonly SemaphoreSlim _loadSemaphore = new(1, 1);
    private readonly SemaphoreSlim _saveSemaphore = new(1, 1);
    private bool _checkedAndReloadNeed;
    private int _checkerTickCount = -5;
    private string _filename = string.Empty;
    private string _inhaltBlockdatei = string.Empty;
    private bool _initialLoadDone;
    private bool _isSaved = true;
    private string _lastSaveCode;
    private int _lockCount;
    private System.IO.FileSystemWatcher? _watcher;

    #endregion

    #region Constructors

    protected MultiUserFile() {
        AllFiles.Add(this);
        _checker = new Timer(Checker_Tick);
        _filename = string.Empty;// KEIN Filename. Ansonsten wird davon ausgegangen, dass die Datei gleich geladen wird.Dann können abgeleitete Klasse aber keine Initialisierung mehr vornehmen.
        ReCreateWatcher();
        _checkedAndReloadNeed = true;
        _lastSaveCode = string.Empty;
        _checker.Change(2000, 60000);
    }

    #endregion

    #region Events

    public event EventHandler<EditingEventArgs>? Editing;

    public event EventHandler? Loaded;

    public event PropertyChangedEventHandler? PropertyChanged;

    #endregion

    #region Properties

    public string CreateDate { get; private set; } = string.Empty;
    public string Creator { get; private set; } = string.Empty;

    /// <summary>
    /// Load benutzen
    /// </summary>
    public string Filename {
        get => _filename;
        private set {
            if (string.IsNullOrEmpty(value)) {
                _filename = string.Empty;
            } else {
                _filename = value.NormalizeFile();
            }
        }
    }

    /// <summary>
    /// Der FreezedReason kann niemals wieder rückgänig gemacht werden.
    /// Weil keine Undos mehr geladen werden, würde da nur Chaos entstehen.
    /// Um den FreezedReason zu setzen, die Methode Freeze benutzen.
    /// </summary>
    public string FreezedReason { get; private set; } = string.Empty;

    public bool IsDisposed { get; private set; }

    public bool IsFreezed => !string.IsNullOrEmpty(FreezedReason);

    public bool IsLoading {
        get {
            if (IsDisposed) { return false; }
            if (IsFreezed) { return false; }

            // Sofortiger Exit wenn bereits ein Save läuft (non-blocking check)
            if (!_loadSemaphore.Wait(0)) { return true; }
            _loadSemaphore.Release();
            return false;
        }
    }

    public bool IsSaving {
        get {
            if (IsDisposed) { return false; }
            if (IsFreezed) { return false; }

            // Sofortiger Exit wenn bereits ein Save läuft (non-blocking check)
            if (!_saveSemaphore.Wait(0)) { return true; }
            _saveSemaphore.Release();
            return false;
        }
    }

    public bool KeyIsCaseSensitive => false;

    /// <summary>
    /// Entspricht dem Dateinamen
    /// </summary>
    public string KeyName => Filename;

    public bool ReloadNeeded {
        get {
            if (string.IsNullOrEmpty(_filename)) { return false; }
            if (_checkedAndReloadNeed) { return true; }
            if (GetFileState(_filename, false, 0.5f) != _lastSaveCode) {
                _checkedAndReloadNeed = true;
                return true;
            }
            return false;
        }
    }

    public abstract string Type { get; }
    public abstract string Version { get; }

    #endregion

    #region Methods

    public static void FreezeAll(string reason) {
        var x = AllFiles.Count;
        foreach (var thisFile in AllFiles) {
            thisFile.Freeze(reason);
            if (x != AllFiles.Count) {
                // Die Auflistung wurde verändert! Selten, aber kann passieren!
                FreezeAll(reason);
                return;
            }
        }
    }

    /// <summary>
    ///
    /// </summary>
    /// <param name="mustSave">Falls TRUE wird zuvor automatisch ein Speichervorgang mit FALSE eingeleitet, um so viel wie möglich zu speichern - falls eine Datei blokiert ist.</param>
    public static void SaveAll(bool mustSave) {
        if (mustSave) { SaveAll(false); } // Beenden, was geht, dann erst der muss

        Develop.Message(ErrorType.Info, null, "Formulare", ImageCode.Diskette, "Speichere alle Formulare", 0);

        var x = AllFiles.Count;
        foreach (var thisFile in AllFiles) {
            thisFile?.Save(mustSave);
            if (x != AllFiles.Count) {
                // Die Auflistung wurde verändert! Selten, aber kann passieren!
                SaveAll(mustSave);
                return;
            }
        }

        Develop.Message(ErrorType.Info, null, "Formulare", ImageCode.Häkchen, "Formulare gespeichert", 0);
    }

    public static void UnlockAllHard() {
        var x = AllFiles.Count;
        foreach (var thisFile in AllFiles) {
            thisFile?.UnlockHard();
            if (x != AllFiles.Count) {
                // Die Auflistung wurde verändert! Selten, aber kann passieren!
                UnlockAllHard();
                return;
            }
        }
    }

    // Dieser Code wird hinzugefügt, um das Dispose-Muster richtig zu implementieren.
    public void Dispose() {
        // Ändern Sie diesen Code nicht. Fügen Sie Bereinigungscode in Dispose(bool disposing) weiter oben ein.
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    public void Dispose(bool disposing) {
        if (!IsDisposed) {
            AllFiles.Remove(this);
            if (disposing) {
                // Verwaltete Ressourcen (Instanzen von Klassen, Lists, Tasks,...)
                //_ = Save(false);
                //while (_pureBinSaver.IsBusy) { Pause(0.5, true); }
                //// https://stackoverflow.com/questions/2542326/proper-way-to-dispose-of-a-backgroundworker
                //_pureBinSaver.Dispose();
                _watcher?.Dispose();
                _checker.Dispose();
            }
            // Nicht verwaltete Ressourcen (Bitmap, Tabellenverbindungen, ...)
            IsDisposed = true;
        }
    }

    /// <summary>
    /// Friert die Tabelle komplett ein, nur noch Ansicht möglich.
    /// Setzt auch ReadOnly.
    /// </summary>
    /// <param name="reason"></param>
    public void Freeze(string reason) {
        if (string.IsNullOrEmpty(reason)) { reason = "Eingefroren"; }
        FreezedReason = reason;
    }

    public bool IsFileAllowedToLoad(string fileName) {
        foreach (var thisFile in AllFiles) {
            if (thisFile != null && string.Equals(thisFile.Filename, fileName, StringComparison.OrdinalIgnoreCase)) {
                thisFile.Save(true);
                Develop.DebugPrint(ErrorType.Warning, "Doppeltes Laden von " + fileName);
                return false;
            }
        }
        return true;
    }

    public void Load(string fileNameToLoad) {
        if (string.Equals(fileNameToLoad, _filename, StringComparison.OrdinalIgnoreCase)) { return; }
        if (!string.IsNullOrEmpty(_filename)) { Develop.DebugPrint(ErrorType.Error, "Geladene Dateien können nicht als neue Dateien geladen werden."); }
        if (string.IsNullOrEmpty(fileNameToLoad)) { Develop.DebugPrint(ErrorType.Error, "Dateiname nicht angegeben!"); }
        //fileNameToLoad = modConverter.SerialNr2Path(fileNameToLoad);

        if (!IsFileAllowedToLoad(fileNameToLoad)) { return; }
        if (!FileExists(fileNameToLoad)) {
            //if (createWhenNotExisting) {
            //    SaveAsAndChangeTo(fileNameToLoad);
            //} else {
            Develop.DebugPrint(ErrorType.Warning, "Datei existiert nicht: " + fileNameToLoad);  // ReadOnly deutet auf Backup hin, in einem anderne Verzeichnis (Linked)
            return;
            //}
        }

        Filename = fileNameToLoad;
        ReCreateWatcher();
        // Wenn ein Dateiname auf Nix gesetzt wird, z.B: bei Bitmap import
        while (!Load_Reload()) { }
    }

    /// <summary>
    /// Führt - falls nötig - einen Reload der Datei aus.
    /// Der Prozess wartet so lange, bis der Reload erfolgreich war.
    /// Ein bereits eventuell bestehender Ladevorgang wird abgewartet.
    /// </summary>
    /// <returns>Gibt TRUE zurück, wenn die am Ende der Routine die Datei auf dem aktuellesten Stand ist</returns>
    public bool Load_Reload() {
        //if (!string.IsNullOrEmpty(EditableErrorReason(EditableErrorReasonType.Load))) { return false; }

        if (!_loadSemaphore.Wait(0)) { return true; }

        try {
            if (_initialLoadDone && !ReloadNeeded) { return true; }

            var tmpFileInfo1 = GetFileState(_filename, false, 0.1f);
            var data = ReadAllText(_filename, Constants.Win1252);
            var tmpFileInfo2 = GetFileState(_filename, true, 30);
            if (tmpFileInfo1 != tmpFileInfo2) { return false; }
            if (data.Length < 10) { return false; }

            if (!this.Parse(data)) { return false; }

            _lastSaveCode = tmpFileInfo1; // initialize setzt zurück

            _initialLoadDone = true;
            _checkedAndReloadNeed = false;

            OnLoaded();

            return !ReloadNeeded;
        } catch {
            return false;
        } finally {
            _loadSemaphore.Release();
        }
    }

    public bool LockEditing() {
        if (_lockCount > 0) { return true; }

        if (!IsAdministrator()) { return false; }

        if (AgeOfBlockDatei() is < 0 or > 3600) {
            //if (AmIBlocker()) { return false; }

            if (Develop.AllReadOnly) { return true; }

            var tmpInhalt = UserName + "\r\n" + DateTime.UtcNow.ToString5() + "\r\nThread: " + Environment.CurrentManagedThreadId + "\r\n" + Environment.MachineName;
            // BlockDatei erstellen, aber noch kein muss. Evtl arbeiten 2 PC synchron, was beim langsamen Netz druchaus vorkommen kann.
            try {
                DeleteFile(Blockdateiname(), false);
                WriteAllText(Blockdateiname(), tmpInhalt, Constants.Win1252, false);
                _inhaltBlockdatei = tmpInhalt;
            } catch {
                return false;
            }

            // Kontrolle, ob kein Netzwerkkonflikt vorliegt
            Pause(1, false);
        }

        _lockCount++;
        return true;
    }

    public virtual List<string> ParseableItems() {
        List<string> result = [];

        result.ParseableAdd("Type", Type);
        result.ParseableAdd("Version", Version);
        result.ParseableAdd("CreateDate", CreateDate);
        result.ParseableAdd("CreateName", Creator);

        return result;
    }

    public virtual void ParseFinished(string parsed) {
    }

    public virtual bool ParseThis(string key, string value) {
        switch (key) {
            case "type":
                return true;

            case "version":
                return true;

            case "createdate":
                CreateDate = value.FromNonCritical();
                return true;

            case "createname":
                Creator = value.FromNonCritical();
                return true;
        }

        return false;
    }

    public bool Save(bool mustSave) {
        if (_isSaved) { return true; }

        if (IsFreezed) { return false; }
        if (IsLoading) { return false; }

        if (ReloadNeeded) { return false; }

        if (!LockEditing()) { return false; }

        var t = ProcessFile(TrySave, [_filename], false, mustSave ? 120 : 10) is true;

        if (t) {
            _isSaved = true;
            _checkedAndReloadNeed = false;
            _lastSaveCode = GetFileState(_filename, false, 1);
        }

        return t;
    }

    public bool SaveAs(string filename) => ProcessFile(TrySave, [filename], false, 120) is true;

    public void UnlockEditing() {
        if (!AmIBlocker()) { return; }

        Save(true);

        _lockCount--;

        if (_lockCount > 0) { return; }

        UnlockAllHard();
    }

    internal bool AmIBlocker() {
        if (AgeOfBlockDatei() is < 0 or > 3600) { return false; }

        string inhalt;

        try {
            inhalt = ReadAllText(Blockdateiname(), Constants.Win1252);
        } catch {
            return false;
        }

        return _inhaltBlockdatei == inhalt;
    }

    protected void OnEditing(EditingEventArgs e) => Editing?.Invoke(this, e);

    protected virtual void OnLoaded() => Loaded?.Invoke(this, System.EventArgs.Empty);

    protected void OnPropertyChanged([CallerMemberName] string propertyName = "unknown") {
        if (IsDisposed) { return; }
        if (IsSaving || IsLoading) { return; }

        if (_lockCount < 1) {
            if (!LockEditing()) {
                Develop.DebugPrint(ErrorType.Error, $"Keine Änderungen an der Datei '{_filename.FileNameWithoutSuffix()}' möglich ({propertyName})!");
                return;
            }
        }

        //Develop.CheckStackForOverflow();
        _isSaved = false;
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    private static string Backupdateiname(string filename) => string.IsNullOrEmpty(filename) ? string.Empty : filename.FilePath() + filename.FileNameWithoutSuffix() + ".bak";

    /// <summary>
    ///
    /// </summary>
    /// <returns>-1 wenn keine vorhanden ist, ansonsten das Alter in Sekunden</returns>
    private double AgeOfBlockDatei() {
        if (!FileExists(Blockdateiname())) { return -1; }
        var f = GetFileInfo(Blockdateiname());
        if (f == null) { return -1; }

        var sec = DateTime.UtcNow.Subtract(f.CreationTimeUtc).TotalSeconds;
        return Math.Max(0, sec); // ganz frische Dateien werden einen Bruchteil von Sekunden in der Zukunft erzeugt.
    }

    private string Blockdateiname() => string.IsNullOrEmpty(_filename) ? string.Empty : _filename.FilePath() + _filename.FileNameWithoutSuffix() + ".blk";

    private void Checker_Tick(object? state) {
        Develop.Message(ErrorType.Info, this, "Formulare", ImageCode.Information, $"Prüfe auf Aktualität des Formulares {Filename.FileNameWithSuffix()}", 0);

        if (IsDisposed) { return; }
        if (string.IsNullOrEmpty(_filename)) { return; }

        if (IsLoading || IsSaving) { return; }

        _checkerTickCount++;
        if (_checkerTickCount < 0) { return; }

        if (!_checkedAndReloadNeed && _isSaved) {
            _checkerTickCount = 0;
            return;
        }

        // Zeiten berechnen
        var reloadDelaySecond = 30;
        var saveDelaySecond = 10; // Soviele Sekunden können vergehen, bevor gespeichert werden muss. Muss größer sein, als Backup. Weil ansonsten der Backup-BackgroundWorker beendet wird

        var mustReload = ReloadNeeded;

        if (!mustReload && _isSaved) {
            _checkerTickCount = 0;
        } else if (!_isSaved) {
            if (_checkerTickCount > saveDelaySecond) { Save(false); }
        } else if (mustReload) {
            if (_checkerTickCount > reloadDelaySecond) { Load_Reload(); }
        }
    }

    private void CreateWatcher() {
        if (!string.IsNullOrEmpty(_filename)) {
            _watcher = new System.IO.FileSystemWatcher(_filename.FilePath()) {
                InternalBufferSize = 64 * 1024,
                IncludeSubdirectories = false,
                EnableRaisingEvents = true,
            };

            _watcher.Changed += Watcher_Changed;
            _watcher.Created += Watcher_Created;
            _watcher.Deleted += Watcher_Deleted;
            _watcher.Renamed += Watcher_Renamed;
            _watcher.Error += Watcher_Error;
        }
    }

    private void ReCreateWatcher() {
        RemoveWatcher();
        CreateWatcher();
    }

    private void RemoveWatcher() {
        try {
            if (_watcher != null) {
                _watcher.EnableRaisingEvents = false;
                _watcher.Changed -= Watcher_Changed;
                _watcher.Created -= Watcher_Created;
                _watcher.Deleted -= Watcher_Deleted;
                _watcher.Renamed -= Watcher_Renamed;
                _watcher.Error -= Watcher_Error;
                _watcher?.Dispose();
                _watcher = null;
            }
        } catch { }
    }

    private FileOperationResult TrySave(List<string> affectingFiles, params object?[] args) {
        if (IsDisposed) { return FileOperationResult.ValueFailed; }
        if (Develop.AllReadOnly) { return FileOperationResult.ValueFailed; }

        if (affectingFiles.Count != 1 || affectingFiles[0] is not { } filename) { return FileOperationResult.ValueFalse; }

        if (string.IsNullOrEmpty(filename)) { return FileOperationResult.ValueFalse; }

        if (DateTime.UtcNow.Subtract(Develop.LastUserActionUtc).TotalSeconds < 6) { return FileOperationResult.DoRetry; }

        // Sofortiger Exit wenn bereits ein Save läuft (non-blocking check)
        if (!_saveSemaphore.Wait(0)) { return FileOperationResult.DoRetry; }

        try {
            //string fileInfoBeforeSaving = GetFileState(filename, true);

            var dataUncompressed = ParseableItems().FinishParseable();

            if (dataUncompressed.Length < 10) { return FileOperationResult.DoRetry; }

            var tmpFileName = TempFile(filename.FilePath() + filename.FileNameWithoutSuffix() + ".tmp-" + UserName.ToUpperInvariant());

            if (!WriteAllText(tmpFileName, dataUncompressed, Constants.Win1252, false)) {
                // DeleteFile(TMPFileName, false); Darf nicht gelöscht werden. Datei konnte ja nicht erstell werden. also auch nix zu löschen
                return FileOperationResult.DoRetry;
            }

            // OK, nun gehts rund: Zuerst das Backup löschen
            if (FileExists(Backupdateiname(filename))) {
                if (!DeleteFile(Backupdateiname(filename), false)) { return FileOperationResult.DoRetry; }
            }

            if (FileExists(filename)) {
                // Haupt-Datei wird zum Backup umbenannt
                if (!MoveFile(filename, Backupdateiname(filename), false)) { return FileOperationResult.DoRetry; }
            }

            // --- TmpFile wird zum Haupt ---
            MoveFile(tmpFileName, filename, true);

            // --- nun Sollte alles auf der Festplatte sein, prüfen! ---
            if (ParseableItems().FinishParseable() != ReadAllText(filename, Constants.Win1252)) { return FileOperationResult.DoRetry; }

            return FileOperationResult.ValueTrue;
        } finally {
            _saveSemaphore.Release();
        }
    }

    private void UnlockHard() {
        if (DeleteFile(Blockdateiname(), false)) {
            _inhaltBlockdatei = string.Empty;
            _lockCount = 0;
        }
    }

    private void Watcher_Changed(object sender, System.IO.FileSystemEventArgs e) {
        if (IsDisposed) { return; }
        if (!string.Equals(e.FullPath, _filename, StringComparison.OrdinalIgnoreCase)) { return; }
        _checkedAndReloadNeed = true;
    }

    private void Watcher_Created(object sender, System.IO.FileSystemEventArgs e) {
        if (IsDisposed) { return; }
        if (!string.Equals(e.FullPath, _filename, StringComparison.OrdinalIgnoreCase)) { return; }
        _checkedAndReloadNeed = true;
    }

    private void Watcher_Deleted(object sender, System.IO.FileSystemEventArgs e) {
        if (IsDisposed) { return; }
        if (!string.Equals(e.FullPath, _filename, StringComparison.OrdinalIgnoreCase)) { return; }
        _checkedAndReloadNeed = true;
    }

    /// <summary>
    /// Im Verzeichnis wurden zu viele Änderungen gleichzeitig vorgenommen...
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void Watcher_Error(object sender, System.IO.ErrorEventArgs e) {
        if (IsDisposed) { return; }
        _checkedAndReloadNeed = true;
    }

    private void Watcher_Renamed(object sender, System.IO.RenamedEventArgs e) {
        if (IsDisposed) { return; }
        if (!string.Equals(e.FullPath, _filename, StringComparison.OrdinalIgnoreCase)) { return; }
        _checkedAndReloadNeed = true;
    }

    #endregion
}