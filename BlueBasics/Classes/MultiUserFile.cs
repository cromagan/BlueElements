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

    /// <summary>
    /// Statische Liste aller geladenen Dateien.
    /// </summary>
    private static readonly List<MultiUserFile> AllFiles = [];

    /// <summary>
    /// Timer für periodische Überprüfungen und Speicherung.
    /// </summary>
    private readonly Timer _checker;

    /// <summary>
    /// Semaphore zum Synchronisieren von Ladevorgängen.
    /// </summary>
    private readonly SemaphoreSlim _loadSemaphore = new(1, 1);

    /// <summary>
    /// Semaphore zum Synchronisieren von Speichervorgängen.
    /// </summary>
    private readonly SemaphoreSlim _saveSemaphore = new(1, 1);

    /// <summary>
    /// Gibt an, ob eine Neuladeprüfung erforderlich ist.
    /// </summary>
    private bool _checkedAndReloadNeed;

    /// <summary>
    /// Zähler für Timer-Ticks.
    /// </summary>
    private int _checkerTickCount = -5;

    /// <summary>
    /// Dateiname der geladenen Datei.
    /// </summary>
    private string _filename = string.Empty;

    /// <summary>
    /// Inhalt der Sperrdatei.
    /// </summary>
    private string _inhaltBlockdatei = string.Empty;

    /// <summary>
    /// Gibt an, ob das initiale Laden abgeschlossen ist.
    /// </summary>
    private bool _initialLoadDone;

    /// <summary>
    /// Gibt an, ob die Datei gespeichert ist.
    /// </summary>
    private bool _isSaved = true;

    /// <summary>
    /// Letzter Speicherstatus der Datei.
    /// </summary>
    private string _lastSaveCode;

    /// <summary>
    /// Zähler für Sperrvorgänge.
    /// </summary>
    private int _lockCount;

    /// <summary>
    /// Dateisystem-Watcher für Dateiänderungen.
    /// </summary>
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
        _checker.Change(2000, 3 * 60 * 1000);
    }

    #endregion

    #region Events

    /// <summary>
    /// Ereignis, das beim Bearbeiten der Datei ausgelöst wird.
    /// </summary>
    public event EventHandler<EditingEventArgs>? Editing;

    /// <summary>
    /// Ereignis, das beim Laden der Datei ausgelöst wird.
    /// </summary>
    public event EventHandler? Loaded;

    /// <summary>
    /// Ereignis, das bei Eigenschaftsänderungen ausgelöst wird.
    /// </summary>
    public event PropertyChangedEventHandler? PropertyChanged;

    #endregion

    #region Properties

    /// <summary>
    /// Das Erstellungsdatum der Datei.
    /// </summary>
    public string CreateDate { get; private set; } = string.Empty;

    /// <summary>
    /// Der Ersteller der Datei.
    /// </summary>
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

    /// <summary>
    /// Gibt an, ob die Datei entsorgt wurde.
    /// </summary>
    public bool IsDisposed { get; private set; }

    /// <summary>
    /// Gibt an, ob die Datei eingefroren ist.
    /// </summary>
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

    /// <summary>
    /// Gibt an, ob der Schlüssel Groß- und Kleinschreibung berücksichtigt.
    /// </summary>
    public bool KeyIsCaseSensitive => false;

    /// <summary>
    /// Entspricht dem Dateinamen
    /// </summary>
    public string KeyName => Filename;

    /// <summary>
    /// Gibt an, ob eine Neuladeprüfung erforderlich ist.
    /// </summary>
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

    /// <summary>
    /// Der Dateityp.
    /// </summary>
    public abstract string Type { get; }

    /// <summary>
    /// Die Versionsnummer der Datei.
    /// </summary>
    public abstract string Version { get; }

    #endregion

    #region Methods

    /// <summary>
    /// Friert alle Dateien mit dem angegebenen Grund ein.
    /// </summary>
    /// <param name="reason">Der Grund für das Einfrieren.</param>
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

    /// <summary>
    /// Entsperrt alle Dateien vollständig.
    /// </summary>
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

    /// <summary>
    /// Entsorgt die Ressourcen der Datei.
    /// </summary>
    public void Dispose() {
        // Ändern Sie diesen Code nicht. Fügen Sie Bereinigungscode in Dispose(bool disposing) weiter oben ein.
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Entsorgt die verwalteten und/oder nicht verwalteten Ressourcen.
    /// </summary>
    /// <param name="disposing">True zum Entsorgen verwalteter Ressourcen.</param>
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

    /// <summary>
    /// Prüft, ob eine Datei geladen werden darf.
    /// </summary>
    /// <param name="fileName">Der Name der zu überprüfenden Datei.</param>
    /// <returns>True, wenn die Datei geladen werden darf; andernfalls false.</returns>
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

    /// <summary>
    /// Lädt eine Datei in das Objekt.
    /// </summary>
    /// <param name="fileNameToLoad">Der Dateipfad zum Laden.</param>
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
        while (!Load_Reload()) { Thread.Sleep(200); }
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

    /// <summary>
    /// Sperrt die Datei zur Bearbeitung.
    /// </summary>
    /// <returns>True, wenn die Sperrung erfolgreich war; andernfalls false.</returns>
    public bool LockEditing() {
        if (_lockCount > 0) { return true; }

        if (!IsAdministrator()) { return false; }

        if (AgeOfBlockDatei() is < 0 or > 3600) {
            //if (AmIBlocker()) { return false; }

            if (Develop.AllReadOnly) { return true; }

            var tmpInhalt = UserName + "\r\n" + DateTime.UtcNow.ToString5() + "\r\nThread: " + Environment.CurrentManagedThreadId + "\r\n" + Environment.MachineName;
            // BlockDatei erstellen, aber noch kein muss. Evtl arbeiten 2 PC synchron, was beim langsamen Netz druchaus vorkommen kann.
            try {
                DeleteFile(Blockdateiname(), 20);
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

    /// <summary>
    /// Gibt die Analyseergebnisse der Datei zurück.
    /// </summary>
    /// <returns>Eine Liste von Schlüssel-Wert-Paaren.</returns>
    public virtual List<string> ParseableItems() {
        List<string> result = [];

        result.ParseableAdd("Type", Type);
        result.ParseableAdd("Version", Version);
        result.ParseableAdd("CreateDate", CreateDate);
        result.ParseableAdd("CreateName", Creator);

        return result;
    }

    /// <summary>
    /// Wird aufgerufen, wenn die Analyse abgeschlossen ist.
    /// </summary>
    /// <param name="parsed">Die analysierten Daten.</param>
    public virtual void ParseFinished(string parsed) {
    }

    /// <summary>
    /// Verarbeitet ein Schlüssel-Wert-Paar während der Analyse.
    /// </summary>
    /// <param name="key">Der Schlüssel.</param>
    /// <param name="value">Der Wert.</param>
    /// <returns>True, wenn die Analyse erfolgreich war; andernfalls false.</returns>
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

    /// <summary>
    /// Speichert die Datei.
    /// </summary>
    /// <param name="mustSave">Ob die Datei erzwungen gespeichert werden soll.</param>
    /// <returns>True, wenn die Speicherung erfolgreich war; andernfalls false.</returns>
    public bool Save(bool mustSave) {
        if (_isSaved) { return true; }

        if (IsFreezed) { return false; }
        if (IsLoading) { return false; }

        if (ReloadNeeded) { return false; }

        if (!LockEditing()) { return false; }

        var t = ProcessFile(TrySave, [_filename], false, mustSave ? 120 : 10).IsSuccessful;

        if (t) {
            _isSaved = true;
            _checkedAndReloadNeed = false;
            _lastSaveCode = GetFileState(_filename, false, 1);
        }

        return t;
    }

    /// <summary>
    /// Speichert die Datei unter einem neuen Namen.
    /// </summary>
    /// <param name="filename">Der neue Dateipfad.</param>
    /// <returns>True, wenn die Speicherung erfolgreich war; andernfalls false.</returns>
    public bool SaveAs(string filename) => ProcessFile(TrySave, [filename], false, 120).IsSuccessful;

    /// <summary>
    /// Entsperrt die Datei zur Bearbeitung.
    /// </summary>
    public void UnlockEditing() {
        if (!AmIBlocker()) { return; }

        Save(true);

        _lockCount--;

        if (_lockCount > 0) { return; }

        UnlockHard();
    }

    /// <summary>
    /// Prüft, ob das aktuelle Objekt die Sperrung verwaltet.
    /// </summary>
    /// <returns>True, wenn das aktuelle Objekt die Sperrung hält; andernfalls false.</returns>
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

    /// <summary>
    /// Ruft das Editing-Ereignis auf.
    /// </summary>
    /// <param name="e">Die Ereignisargumente.</param>
    protected void OnEditing(EditingEventArgs e) => Editing?.Invoke(this, e);

    /// <summary>
    /// Ruft das Loaded-Ereignis auf.
    /// </summary>
    protected virtual void OnLoaded() => Loaded?.Invoke(this, System.EventArgs.Empty);

    /// <summary>
    /// Ruft das PropertyChanged-Ereignis auf.
    /// </summary>
    /// <param name="propertyName">Der Name der geänderten Eigenschaft.</param>
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

    /// <summary>
    /// Gibt den Namen der Sicherungsdatei zurück.
    /// </summary>
    /// <param name="filename">Der ursprüngliche Dateiname.</param>
    /// <returns>Der Name der Sicherungsdatei.</returns>
    private static string Backupdateiname(string filename) => string.IsNullOrEmpty(filename) ? string.Empty : filename.FilePath() + filename.FileNameWithoutSuffix() + ".bak";

    /// <summary>
    /// Gibt das Alter der Sperrdatei in Sekunden zurück.
    /// </summary>
    /// <returns>-1 wenn keine vorhanden ist, ansonsten das Alter in Sekunden</returns>
    private double AgeOfBlockDatei() {
        if (!FileExists(Blockdateiname())) { return -1; }
        var f = GetFileInfo(Blockdateiname());
        if (f == null) { return -1; }

        var sec = DateTime.UtcNow.Subtract(f.CreationTimeUtc).TotalSeconds;
        return Math.Max(0, sec); // ganz frische Dateien werden einen Bruchteil von Sekunden in der Zukunft erzeugt.
    }

    /// <summary>
    /// Gibt den Namen der Sperrdatei zurück.
    /// </summary>
    /// <returns>Der Name der Sperrdatei.</returns>
    private string Blockdateiname() => string.IsNullOrEmpty(_filename) ? string.Empty : _filename.FilePath() + _filename.FileNameWithoutSuffix() + ".blk";

    /// <summary>
    /// Timer-Tick für Überprüfung und automatisches Speichern.
    /// </summary>
    /// <param name="state">Der Timer-Zustand.</param>
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

    /// <summary>
    /// Erstellt einen Dateisystem-Watcher für die Datei.
    /// </summary>
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

    /// <summary>
    /// Erstellt einen neuen Dateisystem-Watcher neu.
    /// </summary>
    private void ReCreateWatcher() {
        RemoveWatcher();
        CreateWatcher();
    }

    /// <summary>
    /// Entfernt und entsorgt den Dateisystem-Watcher.
    /// </summary>
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

    /// <summary>
    /// Versucht die Datei zu speichern.
    /// </summary>
    /// <param name="affectingFiles">Die betroffenen Dateien.</param>
    /// <param name="args">Zusätzliche Argumente.</param>
    /// <returns>Das Ergebnis des Speichervorgangs.</returns>
    private OperationResult TrySave(List<string> affectingFiles, params object?[] args) {
        if (IsDisposed) { return OperationResult.Failed("Verworfen!"); }

        if (affectingFiles.Count != 1 || affectingFiles[0] is not { } filename) { return OperationResult.FailedInternalError; }

        if (string.IsNullOrEmpty(filename)) { return OperationResult.Failed("Kein Dateinname angekommen"); }

        if (DateTime.UtcNow.Subtract(Develop.LastUserActionUtc).TotalSeconds < 6) { return OperationResult.FailedRetryable("Benutzer-Aktion abwarten"); }

        // Sofortiger Exit wenn bereits ein Save läuft (non-blocking check)
        if (!_saveSemaphore.Wait(0)) { return OperationResult.FailedRetryable("Anderer Speichervorgang läuft"); }

        try {
            //string fileInfoBeforeSaving = GetFileState(filename, true);

            var dataUncompressed = ParseableItems().FinishParseable();

            if (dataUncompressed.Length < 10) { return OperationResult.FailedRetryable("Zu wenig Daten angekommen"); }

            var tmpFileName = TempFile(filename.FilePath() + filename.FileNameWithoutSuffix() + ".tmp-" + UserName.ToUpperInvariant());

            if (Develop.AllReadOnly) { return OperationResult.Success; }

            if (!WriteAllText(tmpFileName, dataUncompressed, Constants.Win1252, false)) {
                // DeleteFile(TMPFileName, false); Darf nicht gelöscht werden. Datei konnte ja nicht erstell werden. also auch nix zu löschen
                return OperationResult.FailedRetryable("Speicherfehler");
            }

            // OK, nun gehts rund: Zuerst das Backup löschen
            if (FileExists(Backupdateiname(filename))) {
                if (!DeleteFile(Backupdateiname(filename), false)) { return OperationResult.FailedRetryable("Backup konnte nicht gelöscht werden"); }
            }

            if (FileExists(filename)) {
                // Haupt-Datei wird zum Backup umbenannt
                if (!MoveFile(filename, Backupdateiname(filename), false)) { return OperationResult.FailedRetryable("Haupt-Datei konnte nicht zum Backup gemacht werden"); }
            }

            // --- TmpFile wird zum Haupt ---
            MoveFile(tmpFileName, filename, true);

            // --- nun Sollte alles auf der Festplatte sein, prüfen! ---
            if (ParseableItems().FinishParseable() != ReadAllText(filename, Constants.Win1252)) { return OperationResult.FailedRetryable("Datenschiefstand"); }

            return OperationResult.Success;
        } finally {
            _saveSemaphore.Release();
        }
    }

    /// <summary>
    /// Entsperrt die Datei vollständig.
    /// </summary>
    private void UnlockHard() {
        if (DeleteFile(Blockdateiname(), false)) {
            _inhaltBlockdatei = string.Empty;
            _lockCount = 0;
        }
    }

    /// <summary>
    /// Behandelt das Changed-Ereignis des Dateisystem-Watchers.
    /// </summary>
    /// <param name="sender">Der Sender des Ereignisses.</param>
    /// <param name="e">Die Ereignisargumente.</param>
    private void Watcher_Changed(object sender, System.IO.FileSystemEventArgs e) {
        if (IsDisposed) { return; }
        if (!string.Equals(e.FullPath, _filename, StringComparison.OrdinalIgnoreCase)) { return; }
        _checkedAndReloadNeed = true;
    }

    /// <summary>
    /// Behandelt das Created-Ereignis des Dateisystem-Watchers.
    /// </summary>
    /// <param name="sender">Der Sender des Ereignisses.</param>
    /// <param name="e">Die Ereignisargumente.</param>
    private void Watcher_Created(object sender, System.IO.FileSystemEventArgs e) {
        if (IsDisposed) { return; }
        if (!string.Equals(e.FullPath, _filename, StringComparison.OrdinalIgnoreCase)) { return; }
        _checkedAndReloadNeed = true;
    }

    /// <summary>
    /// Behandelt das Deleted-Ereignis des Dateisystem-Watchers.
    /// </summary>
    /// <param name="sender">Der Sender des Ereignisses.</param>
    /// <param name="e">Die Ereignisargumente.</param>
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

    /// <summary>
    /// Behandelt das Renamed-Ereignis des Dateisystem-Watchers.
    /// </summary>
    /// <param name="sender">Der Sender des Ereignisses.</param>
    /// <param name="e">Die Ereignisargumente.</param>
    private void Watcher_Renamed(object sender, System.IO.RenamedEventArgs e) {
        if (IsDisposed) { return; }
        if (!string.Equals(e.FullPath, _filename, StringComparison.OrdinalIgnoreCase)) { return; }
        _checkedAndReloadNeed = true;
    }

    #endregion
}