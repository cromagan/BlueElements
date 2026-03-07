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
using System.Threading.Tasks;
using static BlueBasics.ClassesStatic.Generic;
using static BlueBasics.ClassesStatic.IO;

namespace BlueBasics.Classes;

public abstract class MultiUserFile : CachedFile, IDisposableExtended {

    #region Fields

    /// <summary>
    /// Semaphore zum Synchronisieren von Ladevorgängen.
    /// </summary>
    private readonly SemaphoreSlim _loadSemaphore = new(1, 1);

    /// <summary>
    /// Semaphore zum Synchronisieren von Speichervorgängen.
    /// </summary>
    private readonly SemaphoreSlim _saveSemaphore = new(1, 1);

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
    /// MultiUserFiles werden nicht gezippt gespeichert.
    /// </summary>
    public override bool MustZipped => false;

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
    public static void FreezeAll(string reason) {
        foreach (var thisFile in CachedFileSystem.GetAll<MultiUserFile>()) {
            thisFile.Freeze(reason);
        }
    }

    /// <summary>
    /// Speichert alle MultiUserFiles.
    /// </summary>
    /// <param name="mustSave">Falls TRUE wird zuvor automatisch ein Speichervorgang mit FALSE eingeleitet.</param>
    public static void SaveAll(bool mustSave) {
        if (mustSave) { SaveAll(false); }

        Develop.Message(ErrorType.Info, null, "Formulare", ImageCode.Diskette, "Speichere alle Formulare", 0);

        foreach (var thisFile in CachedFileSystem.GetAll<MultiUserFile>()) {
            if (!thisFile.IsSaved && thisFile.IsSaveAbleNow()) {
                Task.Run(() => thisFile.DoExtendedSave()).GetAwaiter().GetResult();
            }
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
        if (!IsDisposed) {
            base.Dispose();
        }
    }

    /// <summary>
    /// Liefert die zu speichernden Bytes: ParseableItems() serialisiert nach Win1252.
    /// </summary>
    protected override byte[] GetContent() {
        var text = ParseableItems().FinishParseable();
        if (text.Length < 10) { return []; }
        return Constants.Win1252.GetBytes(text);
    }

    /// <summary>
    /// Prüft, ob Speichern aktuell erlaubt ist.
    /// </summary>
    public override bool IsSaveAbleNow() {
        if (!base.IsSaveAbleNow()) { return false; }
        if (IsLoading) { return false; }
        return true;
    }

    /// <summary>
    /// Speichert die Datei über DoExtendedSave mit Serialisierung.
    /// </summary>
    public override async Task<string> DoExtendedSave() {
        if (!_saveSemaphore.Wait(0)) { return "Anderer Speichervorgang läuft"; }

        try {
            if (DateTime.UtcNow.Subtract(Develop.LastUserActionUtc).TotalSeconds < 6) { return "Benutzer-Aktion abwarten"; }

            return await base.DoExtendedSave().ConfigureAwait(false);
        } finally {
            _saveSemaphore.Release();
        }
    }

    /// <summary>
    /// Führt - falls nötig - einen Reload der Datei aus.
    /// </summary>
    public bool Load_Reload() {
        if (!_loadSemaphore.Wait(0)) { return true; }

        try {
            if (IsParsed && !IsStale()) { return true; }

            Invalidate();

            // Lesen über CachedFile.Content und Konvertierung nach Win1252
            var raw = Content;
            if (raw.Length == 0) { return false; }
            var data = Constants.Win1252.GetString(raw);
            if (data.Length < 10) { return false; }

            // Inline-Parse (ohne IParseable-Extension)
            if (data.GetAllTags() is not { } tags) { return false; }
            foreach (var pair in tags) {
                ParseThis(pair.Key.ToLowerInvariant(), pair.Value);
            }
            ParseFinished(data);

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
    public bool LockEditing() {
        if (_lockCount > 0) { return true; }

        if (!IsAdministrator()) { return false; }

        if (CachedTextFile.AgeOfBlockFile(Filename) is < 0 or > 3600) {
            if (Develop.AllReadOnly) { return true; }

            var tmpInhalt = UserName + "\r\n" + DateTime.UtcNow.ToString5() + "\r\nThread: " + Environment.CurrentManagedThreadId + "\r\n" + Environment.MachineName;
            try {
                CachedTextFile.CreateBlockFile(Filename, tmpInhalt);
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
    /// Gibt die serialisierbaren Elemente zurück.
    /// </summary>
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
    public virtual void ParseFinished(string parsed) {
    }

    /// <summary>
    /// Verarbeitet ein Schlüssel-Wert-Paar während der Analyse.
    /// </summary>
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
    /// Entsperrt die Datei zur Bearbeitung.
    /// </summary>
    public void UnlockEditing() {
        if (!AmIBlocker()) { return; }

        if (!IsSaved && IsSaveAbleNow()) {
            Task.Run(() => DoExtendedSave()).GetAwaiter().GetResult();
        }

        _lockCount--;

        if (_lockCount > 0) { return; }

        UnlockHard();
    }

    /// <summary>
    /// Prüft, ob das aktuelle Objekt die Sperrung verwaltet.
    /// </summary>
    internal bool AmIBlocker() {
        if (CachedTextFile.AgeOfBlockFile(Filename) is < 0 or > 3600) { return false; }

        string inhalt;

        try {
            inhalt = CachedTextFile.ReadBlockFileContent(Filename);
        } catch {
            return false;
        }

        return _inhaltBlockdatei == inhalt;
    }

    /// <summary>
    /// Ruft das Editing-Ereignis auf.
    /// </summary>
    protected void OnEditing(EditingEventArgs e) => Editing?.Invoke(this, e);

    /// <summary>
    /// Ruft das Loaded-Ereignis auf.
    /// </summary>
    protected virtual void OnLoaded() => Loaded?.Invoke(this, System.EventArgs.Empty);

    /// <summary>
    /// Ruft das PropertyChanged-Ereignis auf und markiert die Datei als ungespeichert.
    /// </summary>
    protected void OnPropertyChanged([CallerMemberName] string propertyName = "unknown") {
        if (IsDisposed) { return; }
        if (IsSaving || IsLoading) { return; }

        if (_lockCount < 1) {
            if (!LockEditing()) {
                Develop.DebugPrint(ErrorType.Error, $"Keine Änderungen an der Datei '{Filename.FileNameWithoutSuffix()}' möglich ({propertyName})!");
                return;
            }
        }

        MarkDirty();
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    /// <summary>
    /// Entsperrt die Datei vollständig.
    /// </summary>
    private void UnlockHard() {
        if (CachedFileSystem.DeleteBlockFile(Filename)) {
            _inhaltBlockdatei = string.Empty;
            _lockCount = 0;
        }
    }

    #endregion
}
