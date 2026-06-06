// Licensed under AGPL-3.0; see License.md for disclaimer and details.

using BlueBasics.Attributes;
using BlueBasics.Classes.FileSystemCaching;
using System.ComponentModel;
using System.IO;
using System.Text;
using static BlueBasics.ClassesStatic.Converter;
using static BlueBasics.ClassesStatic.Generic;
using static BlueTable.Classes.Chunk;

namespace BlueTable.Classes;

/// <summary>
/// Tabellen-Typ, der Zeilen auf Row-Chunks (eine .chk-Datei pro Hash/Name) verteilt
/// und zusätzlich alle Metadaten im Main-Chunk (.cfbdb) bündelt.
/// Es existieren KEINE separaten System-Chunks (_master/_vars/_uses/_unknown).
/// Änderungen werden in Echtzeit als Fragment-Dateien (.frg) je Chunk persistiert
/// und beim SaveInternal in frische Chunks konsolidiert.
/// </summary>
/// <remarks>
/// Datei-Layout:
///   [TableName].cfbdb                                  (Main-Chunk = Tabellen-Datei)
///   [TableName]\*.frg                                  (Main-Fragmente)
///   [TableName]\[ChunkID]\Chunk.chk                    (Row-Chunks)
///   [TableName]\[ChunkID]\*.frg                        (Row-Fragmente)
/// </remarks>
[Browsable(false)]
[FileSuffix(".cfbdb")]
[EditorBrowsable(EditorBrowsableState.Never)]
public class TableChunkFragments : TableChunk {

    #region Fields

    private readonly Dictionary<string, string> _fragmentFilenames = new(StringComparer.OrdinalIgnoreCase);

    /// <summary>
    /// Cache für bereits verarbeitete Fragment-Hashes, um Doppel-Anwendung zu verhindern.
    /// </summary>
    private readonly HashSet<string> _processedFragmentHashes = new(StringComparer.OrdinalIgnoreCase);

    /// <summary>
    /// Sperre, die alle Mutationen an der Writer-Map schützt.
    /// </summary>
    private readonly object _writerLock = new();

    private readonly Dictionary<string, StreamWriter> _writers = new(StringComparer.OrdinalIgnoreCase);

    /// <summary>
    /// Letzter Lade-Stand der Fragment-Daten — dient als Referenz-Zeitstempel für die Komplettierung.
    /// </summary>
    private DateTime _isInCache = new(0);

    /// <summary>
    /// Wird gesetzt, wenn die nächste Komplettierung ansteht.
    /// </summary>
    private bool _masterNeeded;

    #endregion

    #region Constructors

    public TableChunkFragments(string tablename) : base(tablename) { }

    public TableChunkFragments(string filename, Table? source) : base(filename, source) { }

    #endregion

    #region Properties

    /// <summary>
    /// True, wenn der nächste Komplettierungs-Zyklus ansteht.
    /// </summary>
    public override bool MasterNeeded => _masterNeeded;

    public override bool MultiUserPossible => true;

    /// <summary>
    /// Speichern ist erst erforderlich, wenn die Hauptdatei geändert wurde.
    /// Fragment-Änderungen landen direkt auf der Platte — sie zählen NICHT als Dirty-Marker.
    /// </summary>
    protected override bool SaveRequired => LastChange > LastSaveMainFileUtcDate;

    #endregion

    #region Methods

    public override string AcquireWriteAccess(TableDataType type, string? chunkValue) {
        // base lädt den Chunk (legt ihn ggf. an) und holt Schreibrechte auf der Chunk-Datei.
        if (base.AcquireWriteAccess(type, chunkValue) is { Length: > 0 } f) { return f; }

        var chunkId = GetChunkId(type, chunkValue ?? string.Empty);
        if (string.IsNullOrEmpty(chunkId)) { return "Fehlerhafter Chunk-Wert"; }

        if (GetOrCreateWriterForChunk(chunkId) is null) {
            return $"Fragment-Writer für Chunk '{chunkId}' konnte nicht initialisiert werden";
        }

        return string.Empty;
    }

    public override bool AmITemporaryMaster(int ranges, int rangee, bool updateAllowed) {
        // Master-Infos liegen im Main-Chunk, kein separater Master-Chunk.
        if (updateAllowed) {
            if (LoadChunkWithChunkId(Chunk_MainData, false, Reason.RaiseEvents).IsFailed) { return false; }
        }

        // updateAllowed=false weitergeben, damit TableChunk.AmITemporaryMaster kein Load auslöst
        return base.AmITemporaryMaster(ranges, rangee, false);
    }

    public override bool BeSureToBeUpToDate(bool firstTime) {
        if (IsDisposed || !DropMessages) { return true; }
        if (string.IsNullOrEmpty(Filename)) { return true; }

        Develop.Message(ErrorType.Info, this, Caption, ImageCode.Tabelle, "Lade Chunks von '" + KeyName + "'", 0);

        var loaded = false;

        OnLoading();

        if (!firstTime) {
            var result = LoadChunkWithChunkId(Chunk_MainData, false, Reason.NoUndo_NoInvalidate);
            if (result.IsFailed) { return false; }
            loaded = result.Value is true;
        }

        Column.GetSystems();

        if (firstTime) {
            if (IO.CreateDirectory(ChunkFolder()).IsFailed) { return false; }
        }

        if (firstTime) {
            _isInCache = LastSaveMainFileUtcDate;
        }

        // Fragment-Dateien (Haupt-Ordner + alle Chunk-Unterordner) einlesen und anwenden.
        var loadedFragmentFiles = LoadAllFragments();

        // Komplettierung: Falls seit dem letzten Save mehr als TableFragments.DoComplete Minuten vergangen sind
        // und diese Instanz Master werden kann, Chunks neu schreiben und Fragmente aufräumen.
        DoWorkAfterLastChanges(loadedFragmentFiles, DateTime.UtcNow);

        if (loaded) { OnLoaded(firstTime, true); }

        TryToSetMeTemporaryMaster();

        return true;
    }

    public override void Freeze(string reason) {
        CloseAllWriters(deleteIfEmpty: true);
        base.Freeze(reason);
    }

    /// <summary>
    /// TableChunkFragments-spezifische Chunk-ID-Ermittlung.
    /// Es gibt KEINE System-Chunks — alle Metadaten landen im Main-Chunk.
    /// Überschreibt <see cref="TableChunk.GetChunkId"/>.
    /// </summary>
    public override string GetChunkId(TableDataType type, string chunkvalue) {
        if (type is TableDataType.Command_RemoveColumn
                or TableDataType.Command_AddColumnByName) { return Chunk_MainData.ToLowerInvariant(); }

        if (type == TableDataType.Command_NewStart) { return string.Empty; }

        if (type.IsObsolete()) { return string.Empty; }

        if (type.IsCellValue() || type is TableDataType.Undo or TableDataType.Command_AddRow or TableDataType.Command_RemoveRow) {
            return GetHashOrNameChunkId(this, chunkvalue, Chunk_MainData);
        }

        return Chunk_MainData.ToLowerInvariant();
    }

    public override string IsGenericEditable(bool isloading) {
        // Direkt zu TableFile.IsGenericEditable springen, um TableChunks System-Chunk-Prüfungen zu überspringen.
        if (((TableFile)this).IsGenericEditable(isloading) is { Length: > 0 } f) { return f; }

        // Nur Main-Chunk ist systemrelevant; Row-Chunks werden on-demand geladen.
        var chunk = CachedFileSystem.Get<Chunk>(ComputeChunkPath(Filename, Chunk_MainData));
        if (chunk is null || chunk.LoadFailed) {
            if (LoadChunkWithChunkId(Chunk_MainData, false, Reason.NoUndo_NoInvalidate).IsFailed) {
                return $"Interner Chunk-Fehler bei Chunk '{Chunk_MainData}'";
            }
            chunk = CachedFileSystem.Get<Chunk>(ComputeChunkPath(Filename, Chunk_MainData));
            if (chunk is null || chunk.LoadFailed) {
                return $"Interner Chunk-Fehler bei Chunk '{Chunk_MainData}'";
            }
        }

        return string.Empty;
    }

    protected override void Dispose(bool disposing) {
        if (IsDisposed) { return; }

        CloseAllWriters(deleteIfEmpty: false);

        if (disposing) {
            UnMasterMe();
        }

        base.Dispose(disposing);
    }

    protected override bool LoadMainData() {
        // Tabellen-Ordner anlegen, damit Fragment-Dateien darin Platz haben.
        if (CachedFileSystem.FileExists(Filename)) {
            if (IO.CreateDirectory(ChunkFolder()).IsFailed) { return false; }
        }
        return base.LoadMainData();
    }

    /// <summary>
    /// Konsolidiert alle Row-Chunks + Main-Chunk aus dem aktuellen Tabellen-Zustand,
    /// schließt die Fragment-Writer und löscht die Fragment-Dateien.
    /// </summary>
    protected override string SaveInternal(DateTime setfileStateUtcDateTo) {
        if (IsDisposed) { return "Tabelle ist bereits freigegeben"; }

        // Vor dem Schreiben alle Writer flushen, damit keine Änderung verloren geht.
        FlushAllWriters();

        // useSystemChunks=false → nur Main + Row-Chunks, keine System-Sub-Chunks.
        var chunks = GenerateNewChunks(this, 0, setfileStateUtcDateTo, true, true, false);
        if (chunks is null) {
            return "Chunks konnten nicht generiert werden";
        }

        // Alle Chunks auf die Platte schreiben.
        foreach (var c in chunks) {
            var result = c.Save().GetAwaiter().GetResult();
            if (result.IsFailed) { return result.FailedReason; }
        }

        // Writer schließen und Fragment-Dateien dieses Prozesses entfernen.
        CloseAllWriters(deleteIfEmpty: false);
        DeleteOurFragmentFiles();

        OnInvalidateView();
        return string.Empty;
    }

    /// <summary>
    /// Schreibt eine einzelne Änderung in den Fragment-Writer des Ziel-Chunks.
    /// </summary>
    protected override string WriteValueToDiscOrServer(TableDataType type, string value, string column, RowItem? row, string user, DateTime datetimeutc, string comment) {
        if (base.WriteValueToDiscOrServer(type, value, column, row, user, datetimeutc, comment) is { Length: > 0 } f) { return f; }

        if (Develop.AllReadOnly) { return string.Empty; }

        var chunkId = GetChunkId(type, value ?? string.Empty);
        if (string.IsNullOrEmpty(chunkId)) { return string.Empty; }

        var writer = GetOrCreateWriterForChunk(chunkId);
        if (writer is null) { return "Schreibmodus deaktiviert"; }

        var l = new UndoItem(KeyName, type, column, row, string.Empty, value, user, datetimeutc, comment, "[Änderung in dieser Session]");

        try {
            lock (writer) {
                var line = l.ParseableItems().FinishParseable();
                writer.WriteLine(line);
            }
        } catch {
            Freeze("Netzwerkfehler!");
            return "Schreibfehler";
        }

        return string.Empty;
    }

    /// <summary>
    /// Schließt alle aktuell offenen Fragment-Writer. Optional werden die zugehörigen
    /// Fragment-Dateien gelöscht, falls sie nur unwichtige Inhalte hatten.
    /// </summary>
    private void CloseAllWriters(bool deleteIfEmpty) {
        List<KeyValuePair<string, StreamWriter>> toClose;
        lock (_writerLock) {
            toClose = [.. _writers];
            _writers.Clear();
        }

        foreach (var kvp in toClose) {
            try {
                kvp.Value.WriteLine("- EOF");
                kvp.Value.Flush();
            } catch { } finally {
                try { kvp.Value.Dispose(); } catch { }
            }
        }

        if (deleteIfEmpty) {
            // Nur eigene Fragment-Dateien löschen.
            DeleteOurFragmentFiles();
        }
    }

    /// <summary>
    /// Schließt den Fragment-Writer für den angegebenen Chunk, falls vorhanden.
    /// </summary>
    private void CloseWriterForChunk(string chunkId) {
        StreamWriter? writer;
        lock (_writerLock) {
            if (!_writers.TryGetValue(chunkId, out writer)) { return; }
            _writers.Remove(chunkId);
        }

        if (writer is not null) {
            try {
                writer.WriteLine("- EOF");
                writer.Flush();
            } catch { } finally {
                try { writer.Dispose(); } catch { }
            }
        }
    }

    /// <summary>
    /// Regeneriert die angegebenen Chunks aus dem aktuellen Tabellen-Zustand, speichert sie,
    /// schließt die zugehörigen Fragment-Writer und löscht die eigenen Fragment-Dateien.
    /// Chunks, die NICHT in <paramref name="chunkIds"/> enthalten sind, werden invalidiert
    /// (sie werden beim nächsten Zugriff von der Platte nachgeladen).
    /// </summary>
    private void ConsolidateLoadedChunks(IList<string> chunkIds) {
        if (chunkIds.Count == 0) { return; }

        // Regeneriert alle Chunks (geladen + ungeladen) im Speicher. Nur die gewünschten
        // werden gespeichert; der Rest wird invalidiert, damit der Original-Zustand auf
        // der Platte erhalten bleibt.
        var chunks = GenerateNewChunks(this, 0, _isInCache, true, true, false);
        if (chunks is null) { return; }

        var wanted = new HashSet<string>(chunkIds, StringComparer.OrdinalIgnoreCase);
        foreach (var chunk in chunks) {
            if (wanted.Contains(chunk.KeyName)) {
                var result = chunk.Save().GetAwaiter().GetResult();
                if (result.IsFailed) {
                    Develop.DebugPrint(ErrorType.Warning, $"Konsolidierung von Chunk '{chunk.KeyName}' fehlgeschlagen: {result.FailedReason}");
                }
            } else {
                // Ungeladener Chunk: nicht überschreiben, beim nächsten Zugriff neu laden.
                chunk.Invalidate();
            }
        }

        // Pro konsolidiertem Chunk: Writer schließen + eigene Fragment-Datei löschen.
        foreach (var chunkId in chunkIds) {
            CloseWriterForChunk(chunkId);
            DeleteOurFragmentFilesForChunk(chunkId);
        }

        // Master-Timestamp fortschreiben (im Main-Chunk via Fragment-Writer).
        var t = LastSaveMainFileUtcDate;
        ChangeData(TableDataType.LastSaveMainFileUtcDate, null, t.ToString7(), _isInCache.ToString7());
        MasterMe();
    }

    /// <summary>
    /// Löscht alle Fragment-Dateien dieses Prozesses für die aktuelle Tabelle.
    /// </summary>
    private void DeleteOurFragmentFiles() {
        List<string> files;
        lock (_writerLock) {
            files = [.. _fragmentFilenames.Values];
            _fragmentFilenames.Clear();
        }

        foreach (var f in files) {
            try { IO.DeleteFile(f, false); } catch { }
        }
    }

    /// <summary>
    /// Löscht die Fragment-Dateien, die von dieser Instanz für den angegebenen Chunk angelegt wurden.
    /// </summary>
    private void DeleteOurFragmentFilesForChunk(string chunkId) {
        string? path;
        lock (_writerLock) {
            if (!_fragmentFilenames.TryGetValue(chunkId, out path)) { return; }
            _fragmentFilenames.Remove(chunkId);
        }

        if (!string.IsNullOrEmpty(path)) {
            try { IO.DeleteFile(path, false); } catch { }
        }
    }

    /// <summary>
    /// Führt die Komplettierung durch: nur die Chunks, die aktuell geladen sind und Fragmente
    /// enthalten, werden regeneriert, gespeichert und ihre Fragment-Dateien gelöscht.
    /// Nicht geladene Chunks werden invalidiert, damit sie bei nächstem Zugriff von der
    /// Platte nachgeladen werden. Die Aushandlung, ob diese Instanz komplettieren darf,
    /// läuft über <see cref="AmITemporaryMaster"/>.
    /// </summary>
    private void DoWorkAfterLastChanges(List<string>? loadedFiles, DateTime startTimeUtc) {
        if (Generic.Ending) { return; }
        if (!string.IsNullOrEmpty(IsGenericEditable(false))) { return; }
        if (loadedFiles is not { Count: >= 1 }) { return; }

        _masterNeeded = DateTime.UtcNow.Subtract(LastSaveMainFileUtcDate).TotalMinutes > TableFragments.DoComplete;

        #region Bei Bedarf Chunks konsolidieren

        if (_masterNeeded && AmITemporaryMaster(MasterTry, MasterUntil, true)) {
            Develop.Message(ErrorType.Info, this, Caption, ImageCode.Tabelle, "Komplettiere geladene Chunks: " + KeyName, 0);

            // Welche Chunks wurden geladen und haben Fragmente?
            var loadedChunkIds = GetLoadedChunkIdsWithFragments(loadedFiles);
            if (loadedChunkIds.Count > 0) {
                ConsolidateLoadedChunks(loadedChunkIds);
            }

            _masterNeeded = false;
            OnInvalidateView();
            _processedFragmentHashes.Clear();
        }

        #endregion

        if (DateTime.UtcNow.Subtract(startTimeUtc).TotalSeconds > TableFragments.AbortFragmentDeletion) { return; }

        #region Veraltete Fragment-Dateien entfernen

        foreach (var thisf in loadedFiles) {
            var f = thisf.FileNameWithoutSuffix();
            if (f.Length > 19) {
                var da = f[^19..];

                if (DateTimeTryParse(da, out var d2)) {
                    if (DateTime.UtcNow.Subtract(d2).TotalMinutes > TableFragments.DeleteFragmentsAfter &&
                         LastSaveMainFileUtcDate.Subtract(d2).TotalMinutes > TableFragments.DeleteFragmentsAfter) {
                        Develop.Message(ErrorType.Info, this, Caption, ImageCode.Tabelle, "Räume Fragmente auf: " + thisf.FileNameWithoutSuffix(), 0);
                        IO.DeleteFile(thisf, false);
                        if (DateTime.UtcNow.Subtract(startTimeUtc).TotalSeconds > TableFragments.AbortFragmentDeletion) { break; }
                    }
                }
            }
        }

        #endregion
    }

    /// <summary>
    /// Flush über alle aktuell offenen Writer.
    /// </summary>
    private void FlushAllWriters() {
        lock (_writerLock) {
            foreach (var w in _writers.Values) {
                try {
                    w.Flush();
                } catch { }
            }
        }
    }

    /// <summary>
    /// Liefert den Fragment-Pfad für den angegebenen Chunk.
    /// Main-Chunk: [TableName]\[Tablename]-[MachineName]-[Timestamp].frg
    /// Row-Chunk:  [TableName]\[ChunkID]\[ChunkID]-[MachineName]-[Timestamp].frg
    /// </summary>
    private string? FragmentPathForChunk(string chunkId) {
        if (string.IsNullOrEmpty(Filename)) { return null; }

        var folder = Filename.FilePath();
        var tablename = Filename.FileNameWithoutSuffix();

        string dir;
        string prefix;
        if (string.Equals(chunkId, Chunk_MainData, StringComparison.OrdinalIgnoreCase)) {
            dir = $"{folder}{tablename}\\";
            prefix = tablename;
        } else {
            dir = $"{folder}{tablename}\\{chunkId.ToLowerInvariant()}\\";
            prefix = chunkId.ToLowerInvariant();
        }

        if (IO.CreateDirectory(dir).IsFailed) { return null; }

        return IO.TempFile(dir, $"{prefix}-{Environment.MachineName}-{DateTime.UtcNow.ToString4()}", TableFragments.SuffixOfFragments);
    }

    /// <summary>
    /// Ermittelt, welche Chunk-IDs aktuell geladen sind UND Fragmente haben.
    /// </summary>
    private List<string> GetLoadedChunkIdsWithFragments(List<string> loadedFragmentFiles) {
        var result = new List<string>();
        if (string.IsNullOrEmpty(ChunkFolder())) { return result; }

        var loadedFolders = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (var file in loadedFragmentFiles) {
            var folder = Path.GetDirectoryName(file) ?? string.Empty;
            loadedFolders.Add(folder.TrimEnd('\\'));
        }

        // Main-Chunk: nur aufnehmen, wenn er geladen ist UND Fragmente hat
        if (IsChunkLoaded(Chunk_MainData) && loadedFolders.Contains(ChunkFolder().TrimEnd('\\'))) {
            result.Add(Chunk_MainData);
        }

        // Row-Chunks: pro Unterordner prüfen
        if (Directory.Exists(ChunkFolder())) {
            foreach (var subDir in Directory.EnumerateDirectories(ChunkFolder())) {
                var normalized = subDir.TrimEnd('\\');
                if (!loadedFolders.Contains(normalized)) { continue; }
                var chunkId = Path.GetFileName(normalized);
                if (IsChunkLoaded(chunkId)) { result.Add(chunkId); }
            }
        }

        return result;
    }

    /// <summary>
    /// Liefert den StreamWriter für den angegebenen Chunk und legt ihn bei Bedarf an.
    /// </summary>
    private StreamWriter? GetOrCreateWriterForChunk(string chunkId) {
        StreamWriter? writer;
        lock (_writerLock) {
            if (_writers.TryGetValue(chunkId, out writer) && writer is not null) { return writer; }
        }

        var path = FragmentPathForChunk(chunkId);
        if (string.IsNullOrEmpty(path)) { return null; }

        try {
            var fileStream = new System.IO.FileStream(path, System.IO.FileMode.Append, System.IO.FileAccess.Write, System.IO.FileShare.Read);
            var newWriter = new StreamWriter(fileStream, Encoding.UTF8) { AutoFlush = true };
            newWriter.WriteLine("- Version " + Table.TableVersion);
            newWriter.WriteLine("- Filename " + Filename);
            newWriter.WriteLine("- User " + UserName);
            newWriter.WriteLine("- Chunk " + chunkId);
            newWriter.WriteLine(new UndoItem(KeyName, TableDataType.Command_NewStart, string.Empty, string.Empty, string.Empty, path.FileNameWithoutSuffix(), UserName, DateTime.UtcNow, " Dummy - systembedingt benötigt", "[Änderung in dieser Session]").ParseableItems().FinishParseable());

            lock (_writerLock) {
                if (_writers.TryGetValue(chunkId, out writer) && writer is not null) {
                    // Anderer Thread war schneller — den soeben erstellten wieder schließen.
                    try { newWriter.Dispose(); } catch { }
                    return writer;
                }
                _writers[chunkId] = newWriter;
                _fragmentFilenames[chunkId] = path;
                writer = newWriter;
            }
        } catch (Exception ex) {
            Develop.DebugPrint(ErrorType.Warning, $"Fragment-Writer für Chunk '{chunkId}' konnte nicht erstellt werden: {ex.Message}");
            return null;
        }

        return writer;
    }

    /// <summary>
    /// Prüft, ob der Chunk mit der angegebenen ID bereits im Speicher geladen ist.
    /// </summary>
    private bool IsChunkLoaded(string chunkId) {
        if (string.IsNullOrEmpty(Filename)) { return false; }

        var path = ComputeChunkPath(Filename, chunkId);
        var chunk = CachedFileSystem.Get<Chunk>(path);
        return chunk is not null && !chunk.LoadFailed && !chunk.NeedsLoading();
    }

    /// <summary>
    /// Prüft, ob die gegebene Fragment-Datei zu dieser Instanz gehört.
    /// </summary>
    private bool IsOurFragment(string file) {
        lock (_writerLock) {
            foreach (var own in _fragmentFilenames.Values) {
                if (own.Equals(file, StringComparison.OrdinalIgnoreCase)) { return true; }
            }
        }
        return false;
    }

    /// <summary>
    /// Lädt alle Fragment-Dateien (Hauptordner + Chunk-Unterordner) und wendet sie auf den
    /// aktuellen Tabellen-Zustand an. Es werden NUR Fragmente von Chunks berücksichtigt,
    /// die bereits geladen sind. Bereits verarbeitete Hashes werden ignoriert.
    /// </summary>
    /// <returns>Liste der eingelesenen Fragment-Dateien (Pfade).</returns>
    private List<string>? LoadAllFragments() {
        if (string.IsNullOrEmpty(ChunkFolder())) { return null; }

        try {
            var folders = new List<string>();

            // Hauptordner: nur verarbeiten, wenn der Main-Chunk geladen ist.
            if (IsChunkLoaded(Chunk_MainData)) { folders.Add(ChunkFolder()); }

            // Row-Chunk-Unterordner: nur verarbeiten, wenn der jeweilige Chunk geladen ist.
            if (Directory.Exists(ChunkFolder())) {
                foreach (var subDir in Directory.EnumerateDirectories(ChunkFolder())) {
                    var chunkId = Path.GetFileName(subDir.TrimEnd('\\'));
                    if (IsChunkLoaded(chunkId)) { folders.Add(subDir); }
                }
            }

            var files = new List<string>();
            var ok = true;
            foreach (var folder in folders) {
                var fromFolder = LoadFragmentsFromFolder(folder);
                if (fromFolder is null) { ok = false; } else { files.AddRange(fromFolder); }
            }
            return ok ? files : null;
        } catch (Exception ex) {
            Develop.DebugPrint(ErrorType.Warning, $"Fehler beim Laden der Fragmente: {ex.Message}");
            return null;
        }
    }

    private List<string>? LoadFragmentsFromFolder(string folder) {
        if (string.IsNullOrEmpty(folder) || !Directory.Exists(folder)) { return null; }

        var loadedFiles = new List<string>();
        try {
            foreach (var file in Directory.EnumerateFiles(folder, "*.frg", SearchOption.TopDirectoryOnly)) {
                if (IsOurFragment(file)) { continue; }

                loadedFiles.Add(file);
                var content = CachedFileSystem.Get<CachedTextFile>(file)?.GetContentAsString(Encoding.UTF8) ?? string.Empty;
                foreach (var line in content.SplitAndCutByCr()) {
                    if (string.IsNullOrEmpty(line) || line.StartsWith('-')) { continue; }

                    var hash = line.GetMD5Hash();
                    if (_processedFragmentHashes.Contains(hash)) { continue; }

                    try {
                        var undo = new UndoItem(line);
                        if (undo.DateTimeUtc <= LastSaveMainFileUtcDate) {
                            _processedFragmentHashes.Add(hash);
                            continue;
                        }

                        if (undo.TableName != KeyName) { continue; }

                        var col = Column[undo.ColName];
                        var row = Row.GetByKey(undo.RowKey);

                        var reason = Reason.IgnoreFreeze | Reason.NoUndo_NoInvalidate;
                        var err = SetValueInternal(undo.Command, col, row, undo.ChangedTo, undo.User, undo.DateTimeUtc, reason);
                        if (!string.IsNullOrEmpty(err)) {
                            Develop.DebugPrint(ErrorType.Warning, $"Fragment konnte nicht angewendet werden: {err}");
                        }
                        _processedFragmentHashes.Add(hash);
                    } catch (Exception ex) {
                        Develop.DebugPrint(ErrorType.Warning, $"Fragment-Zeile konnte nicht verarbeitet werden: {ex.Message}");
                    }
                }
            }
        } catch (Exception ex) {
            Develop.DebugPrint(ErrorType.Warning, $"Fehler beim Lesen der Fragment-Dateien in '{folder}': {ex.Message}");
            return null;
        }
        return loadedFiles;
    }

    #endregion
}