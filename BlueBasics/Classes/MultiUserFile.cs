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
using BlueBasics.EventArgs;
using BlueBasics.Interfaces;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using static BlueBasics.ClassesStatic.Generic;
using static BlueBasics.ClassesStatic.IO;
using Timer = System.Threading.Timer;

namespace BlueBasics.Classes;

public abstract class MultiUserFile : CachedFile, IDisposableExtended, IHasKeyName, IParseable, INotifyPropertyChanged {

    #region Fields

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
    /// Zähler für Timer-Ticks.
    /// </summary>
    private int _checkerTickCount = -5;

    /// <summary>
    /// Inhalt der Sperrdatei.
    /// </summary>
    private string _inhaltBlockdatei = string.Empty;

    /// <summary>
    /// Gibt an, ob die Datei gespeichert ist.
    /// </summary>
    private bool _isSaved = true;

    /// <summary>
    /// Zähler für Sperrvorgänge.
    /// </summary>
    private int _lockCount;

    #endregion

    #region Constructors

    protected MultiUserFile(string filename) : base(filename) {
        _checker = new Timer(Checker_Tick);
        Invalidate(); // Beim Start als "stale" markieren, damit Load_Reload ausgelöst wird
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

            if (!_loadSemaphore.Wait(0)) { return true; }
            _loadSemaphore.Release();
            return false;
        }
    }

    public bool IsSaving {
        get {
            if (IsDisposed) { return false; }
            if (IsFreezed) { return false; }

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
        foreach (var thisFile in CachedFileSystem.GetAll<MultiUserFile>()) {
            thisFile.Freeze(reason);
        }
    }

    /// <summary>
    ///
    /// </summary>
    /// <param name="mustSave">Falls TRUE wird zuvor automatisch ein Speichervorgang mit FALSE eingeleitet, um so viel wie möglich zu speichern - falls eine Datei blokiert ist.</param>
    public static void SaveAll(bool mustSave) {
        if (mustSave) { SaveAll(false); } // Beenden, was geht, dann erst der muss

        Develop.Message(ErrorType.Info, null, "Formulare", ImageCode.Diskette, "Speichere alle Formulare", 0);

        foreach (var thisFile in CachedFileSystem.GetAll<MultiUserFile>()) {
            thisFile.Save(mustSave);
        }

        Develop.Message(ErrorType.Info, null, "Formulare", ImageCode.Häkchen, "Formulare gespeichert", 0);
    }

    /// <summary>
    /// Entsperrt alle Dateien vollständig.
    /// </summary>
    public static void UnlockAllHard() {
        foreach (var thisFile in CachedFileSystem.GetAll<MultiUserFile>()) {
            thisFile.UnlockHard();
        }
    }

    /// <summary>
    /// Entsorgt die Ressourcen der Datei.
    /// </summary>
    public override void Dispose() {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Entsorgt die verwalteten und/oder nicht verwalteten Ressourcen.
    /// </summary>
    /// <param name="disposing">True zum Entsorgen verwalteter Ressourcen.</param>
    public void Dispose(bool disposing) {
        if (!IsDisposed) {
            if (disposing) {
                _checker.Dispose();
                base.Dispose();
            }
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
    /// Führt - falls nötig - einen Reload der Datei aus.
    /// Der Prozess wartet so lange, bis der Reload erfolgreich war.
    /// Ein bereits eventuell bestehender Ladevorgang wird abgewartet.
    /// </summary>
    /// <returns>Gibt TRUE zurück, wenn die am Ende der Routine die Datei auf dem aktuellesten Stand ist</returns>
    public bool Load_Reload() {
        if (!_loadSemaphore.Wait(0)) { return true; }

        try {
            if (IsParsed && !IsStale()) { return true; }

            // CachedFile-Cache invalidieren, damit frisch von Platte gelesen wird
            Invalidate();

            // Lesen über CachedFile.GetContent() und Konvertierung nach Win1252
            var data = GetContentAsString(Constants.Win1252);
            if (data.Length < 10) { return false; }

            if (!this.Parse(data)) { return false; }

            IsParsed = true;

            OnLoaded();

            return !IsStale();
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
            if (Develop.AllReadOnly) { return true; }

            var tmpInhalt = UserName + "\r\n" + DateTime.UtcNow.ToString5() + "\r\nThread: " + Environment.CurrentManagedThreadId + "\r\n" + Environment.MachineName;
            try {
                DeleteFile(Blockdateiname(), 20);
                WriteAllText(Blockdateiname(), tmpInhalt, Constants.Win1252, false);
                _inhaltBlockdatei = tmpInhalt;
            } catch {
                return false;
            }

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

        if (IsStale()) { return false; }

        if (!LockEditing()) { return false; }

        var t = ProcessFile(TrySave, [Filename], false, mustSave ? 120 : 10).IsSuccessful;

        if (t) {
            _isSaved = true;
            Invalidate();
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
                Develop.DebugPrint(ErrorType.Error, $"Keine Änderungen an der Datei '{Filename.FileNameWithoutSuffix()}' möglich ({propertyName})!");
                return;
            }
        }

        _isSaved = false;
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    /// <summary>
    /// Liest den Dateiinhalt über CachedFile.GetContent() und konvertiert mit dem angegebenen Encoding.
    /// </summary>
    private string GetContentAsString(Encoding encoding) {
        var content = GetContent();
        if (content.Length == 0) { return string.Empty; }
        return encoding.GetString(content);
    }

    /// <summary>
    /// Gibt den Namen der Sicherungsdatei zurück.
    /// </summary>
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
        return Math.Max(0, sec);
    }

    /// <summary>
    /// Gibt den Namen der Sperrdatei zurück.
    /// </summary>
    private string Blockdateiname() => string.IsNullOrEmpty(Filename) ? string.Empty : Filename.FilePath() + Filename.FileNameWithoutSuffix() + ".blk";

    /// <summary>
    /// Timer-Tick für Überprüfung und automatisches Speichern.
    /// </summary>
    private void Checker_Tick(object? state) {
        Develop.Message(ErrorType.Info, this, "Formulare", ImageCode.Information, $"Prüfe auf Aktualität des Formulares {Filename.FileNameWithSuffix()}", 0);

        if (IsDisposed) { return; }
        if (string.IsNullOrEmpty(Filename)) { return; }

        if (IsLoading || IsSaving) { return; }

        _checkerTickCount++;
        if (_checkerTickCount < 0) { return; }

        if (!IsStale() && _isSaved) {
            _checkerTickCount = 0;
            return;
        }

        var reloadDelaySecond = 30;
        var saveDelaySecond = 10;

        var mustReload = IsStale();

        if (!mustReload && _isSaved) {
            _checkerTickCount = 0;
        } else if (!_isSaved) {
            if (_checkerTickCount > saveDelaySecond) { Save(false); }
        } else if (mustReload) {
            if (_checkerTickCount > reloadDelaySecond) { Load_Reload(); }
        }
    }

    /// <summary>
    /// Versucht die Datei zu speichern.
    /// </summary>
    private OperationResult TrySave(List<string> affectingFiles, params object?[] args) {
        if (IsDisposed) { return OperationResult.Failed("Verworfen!"); }

        if (affectingFiles.Count != 1 || affectingFiles[0] is not { } filename) { return OperationResult.FailedInternalError; }

        if (string.IsNullOrEmpty(filename)) { return OperationResult.Failed("Kein Dateinname angekommen"); }

        if (DateTime.UtcNow.Subtract(Develop.LastUserActionUtc).TotalSeconds < 6) { return OperationResult.FailedRetryable("Benutzer-Aktion abwarten"); }

        if (!_saveSemaphore.Wait(0)) { return OperationResult.FailedRetryable("Anderer Speichervorgang läuft"); }

        try {
            var dataUncompressed = ParseableItems().FinishParseable();

            if (dataUncompressed.Length < 10) { return OperationResult.FailedRetryable("Zu wenig Daten angekommen"); }

            var tmpFileName = TempFile(filename.FilePath() + filename.FileNameWithoutSuffix() + ".tmp-" + UserName.ToUpperInvariant());

            if (Develop.AllReadOnly) { return OperationResult.Success; }

            if (!WriteAllText(tmpFileName, dataUncompressed, Constants.Win1252, false)) {
                return OperationResult.FailedRetryable("Speicherfehler");
            }

            if (FileExists(Backupdateiname(filename))) {
                if (!DeleteFile(Backupdateiname(filename), false)) { return OperationResult.FailedRetryable("Backup konnte nicht gelöscht werden"); }
            }

            if (FileExists(filename)) {
                if (!MoveFile(filename, Backupdateiname(filename), false)) { return OperationResult.FailedRetryable("Haupt-Datei konnte nicht zum Backup gemacht werden"); }
            }

            MoveFile(tmpFileName, filename, true);

            Invalidate();
            if (ParseableItems().FinishParseable() != GetContentAsString(Constants.Win1252)) { return OperationResult.FailedRetryable("Datenschiefstand"); }

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

    #endregion
}