// Licensed under MIT; see License.md for disclaimer, details, and extended user conditions.

using System.Collections.Concurrent;
using System.ComponentModel;
using System.IO;
using System.Threading;

namespace BlueBasics.Classes;

/// <summary>
/// Datei-Verwaltung mit Kopf-Datei (Layout-Kopf) und Daten-Chunks in einem
/// Arbeitsunterordner
/// gleichen Namens ohne Suffix, mit Benutzer-spezifischen Schreibdateien
/// (write-once). Gechunkt wird
/// ausschließlich über den NAMEN des Chunk-Wertes (von außen geliefert); eine
/// Erweiterung auf andere Chunk-Typen ist bewusst nicht vorgesehen. Jeder
/// Benutzer schreibt in seine eigene Datei
/// (yyyy-MM-dd-HH-mm-ss-fff_Username-Hash). Dateien älter als zwei Stunden werden
/// gelöscht (wenn mehrere vorhanden). Edit-Sperre: Neueste Datei unter
/// EditLockSeconds → nur der Ersteller darf bearbeiten. Jeder Chunk ist ein
/// Einweg (write-once): eine einmal gespeicherte Datei wird nie wieder
/// überschrieben. Vor dem Parsen wird FragmentRecord.HasValidEofMarker geprüft.
/// <para>
/// Alle Datei-Aktionen erhalten eine Info (den Chunk-Namen), sodass intern
/// entschieden wird, welche Chunk-Datei geprüft, geladen oder gespeichert wird.
/// Welcher Bereich des Fragment-Baums in welchen Chunk gehört, legen die
/// Areas-Regeln fest; geladene Daten-Chunks werden über den
/// Standard-Deserialize des Owners eingelesen.
/// </para>
/// </summary>
[EditorBrowsable(EditorBrowsableState.Never)]
public class SaveableFileChunk : SaveableFile {

    #region Layout

    /// <summary>
    /// ClassId des Chunk-Layouts; steht im Layout-Kopf der Kopf-Datei.
    /// </summary>
    public static string ClassId => "Chunk";

    /// <summary>
    /// Die ClassId des konkreten Layouts.
    /// </summary>
    protected override string LayoutId => ClassId;

    /// <summary>
    /// Anzeigename des Chunk-Layouts für Auswahldialoge.
    /// </summary>
    public override string ReadableText() => "Chunk basiert";

    /// <summary>
    /// Kurzinfo des Chunk-Layouts für Auswahldialoge.
    /// </summary>
    public override string QuickInfo => "Die Daten liegen als unveränderliche Chunk-Dateien im Arbeitsunterordner.\r\nEs werden nicht die kompletten Daten geladen. Bei Bedarf werden Daten nachgeladen.";

    #endregion

    #region Fields

    /// <summary>
    /// Wert in Stunden. Benutzer-Dateien, die älter sind, werden gelöscht
    /// (sofern mindestens FileCount neuere Dateien im gleichen Ordner existieren).
    /// </summary>
    public const int DeleteOldFilesAfterHours = 2;

    /// <summary>
    /// Chunk-ID für die Hauptdaten (Kopf, Struktur, Ansichten usw.).
    /// </summary>
    public static readonly string Chunk_MainData = "_MainData";

    /// <summary>
    /// Areas-Ziel „je Item verteilen“: Die Items der Sektion wandern je
    /// Chunk-ID in ihren eigenen Chunk (die Items müssen IHasChunkValue
    /// implementieren; die ID liefert IHasChunkValueExtension.ChunkId).
    /// </summary>
    public const string Area_Individual = "{individual}";

    /// <summary>
    /// Anzahl der zu behaltenden Dateien in einem Chunk-Ordner
    /// </summary>
    private const int FileCount = 5;

    /// <summary>
    /// chunkId (lowercase) → SHA256-Hash des kompletten Chunk-Inhalts.
    /// Verhindert unnötiges Schreiben bei unverändertem Inhalt (Write-once: keine
    /// identischen Duplikate erzeugen).
    /// </summary>
    private readonly ConcurrentDictionary<string, string> _lastContentHash = new(StringComparer.OrdinalIgnoreCase);

    /// <summary>
    /// chunkId (lowercase) → UTC-Zeitpunkt des letzten Zugriffs (Laden, Speichern,
    /// Refresh, Anzeige via TouchChunk). Wird genutzt, um ungenutzte Chunks bei
    /// Refresh zu überspringen.
    /// </summary>
    private readonly ConcurrentDictionary<string, DateTime> _lastUsed = new(StringComparer.OrdinalIgnoreCase);

    /// <summary>
    /// chunkId (lowercase) → Dateiname der zuletzt geparten oder gespeicherten Chunk-Datei.
    /// Chunks werden nach dem Parsen verworfen, nur der Dateiname wird für
    /// Aktualitätsprüfungen vorgehalten.
    /// </summary>
    private readonly ConcurrentDictionary<string, string> _processedFile = new(StringComparer.OrdinalIgnoreCase);

    /// <summary>
    /// Reentrancy-Sperre für Refresh: 0 = frei, 1 = läuft.
    /// </summary>
    private int _isRefreshingChunks;

    #endregion

    /// <summary>
    /// Bereichs-Zuordnung des Chunk-Layouts: Pfad (leafTypes von der Wurzel-Sektion
    /// bis zum Bereich; Steuer-Sätze wie ItemList/ItemAdd tragen keinen Bestandteil)
    /// → Ziel. Das Ziel ist eine Chunk-ID (lowercase) — der Block des Bereichs
    /// läuft dann in diesen Chunk — oder der Marker Area_Individual: Die Items der
    /// Sektion werden je Chunk-Wert verteilt. Beim Zusammensetzen entscheidet die
    /// Regel mit dem längsten passenden Pfad, Bereiche ohne Regel gehören in den
    /// Haupt-Chunk. Undo-Einträge nutzen dieselben Regeln über den Pfad ihres
    /// Eintrags (UndoEntryChunk); die ab Werk eingetragene Undo-History-Regel
    /// stellt die individuelle Verteilung als Fallback sicher.
    /// </summary>
    public Dictionary<byte[], string> Areas { get; } = new() { [new byte[] { (byte)FragmentTagType.UndoHistory }] = Area_Individual };

    /// <summary>
    /// Chunk-IDs, die bei jedem Start bzw. Refresh eingelesen werden; der Owner
    /// setzt die Liste. Der Setter garantiert den Haupt-Chunk (Chunk_MainData)
    /// an erster Stelle. Startup-Chunks enthalten vollständige Hauptinhalte;
    /// Chunks außerhalb der Liste werden nur bei Bedarf geladen (EnsureLoaded
    /// bzw. Daten-Fenster-Refresh) und enthalten nur Teil-Bestand.
    /// </summary>
    public List<string> StartupChunkIds {
        get;
        set {
            field = [.. value];
            field.RemoveAll(f => string.Equals(f, Chunk_MainData, StringComparison.OrdinalIgnoreCase));
            field.Insert(0, Chunk_MainData);
        }
    } = [Chunk_MainData];

    /// <summary>
    /// Die Items mit Chunk-Wert (IHasKeyName + IHasChunkValue); Quelle ist das
    /// gebundene Objekt (IHasChunkItems).
    /// </summary>
    private IEnumerable<IHasKeyName> ChunkItemSource => Owner is IHasChunkItems source ? source.ChunkItems : [];

    #region Constructors

    /// <summary>
    /// Erstellt eine Layout-Instanz ohne Datei-Bezug für die Aufzählung der Datei-Layouts.
    /// </summary>
    public SaveableFileChunk() { }

    public SaveableFileChunk(string filename) : base(filename) {
        InitialSavePending = true;
        SaveRequired = true;
    }

    #endregion

    #region Abstracts

    /// <summary>
    /// Erzeugt den kompletten Inhalt (Stempel + Daten + Abschluss-Marker) des Chunks
    /// mit der angegebenen ID. Der Chunk erhält nur die Bereiche, die ihm die
    /// Areas-Regeln zuordnen.
    /// </summary>
    protected virtual List<byte> BuildChunkContent(string chunkId, byte[] source) {
        var records = new List<(int Start, int End, FragmentRecord Record)>();
        using var ms = new MemoryStream(source);

        var recordStart = 0L;
        while (FragmentRecord.TryReadNext(ms) is { } record) {
            records.Add(((int)recordStart, (int)ms.Position, record));
            recordStart = ms.Position;
        }

        var result = new List<byte>();
        var itemIds = ItemChunkIds();
        var i = 0;

        // Datei-Stempel am Kopf unverändert übernehmen
        while (i < records.Count && records[i].Record.IsFileTag) {
            result.AddRange(source[records[i].Start..records[i].End]);
            i++;
        }

        // Nutzdaten regelbasiert übernehmen
        while (i < records.Count) {
            var (start, end, record) = records[i];

            if (record.TagType == FragmentTagType.Eof) {
                result.AddRange(source[start..end]);
                break;
            }

            if (record.Depth > 0) { i++; continue; } // Satz außerhalb der Wurzel-Ebene — überspringen

            // Undo-Eintrag: Das Ziel liefert die Areas-Regel des Eintrags-Pfades —
            // individuell verteilt folgt der Block dem ersten ItemPath-Item-Kenner;
            // Einträge ohne Kenner — Kopf- und Struktur-Änderungen — laufen in den
            // Haupt-Chunk (Details bei UndoEntryChunk).
            if (record.TagType == FragmentTagType.UndoHistory) {
                var entryEnd = i + 1;
                while (entryEnd < records.Count && records[entryEnd].Record.Depth > 0) { entryEnd++; }

                List<FragmentRecord> entry = [];
                for (var k = i + 1; k < entryEnd; k++) { entry.Add(records[k].Record); }

                if (string.Equals(UndoEntryChunk(entry, itemIds), chunkId, StringComparison.OrdinalIgnoreCase)) {
                    result.AddRange(source[start..(entryEnd < records.Count ? records[entryEnd].Start : source.Length)]);
                }

                i = entryEnd;
                continue;
            }

            var areaId = LookupArea([record.LeafType]) ?? Chunk_MainData.ToLowerInvariant();

            var individual = string.Equals(areaId, Area_Individual, StringComparison.OrdinalIgnoreCase);

            // Sammlung (ItemList eine Ebene tiefer): Die Items werden je Chunk-ID
            // (IHasChunkValueExtension.ChunkId) verteilt, Items ohne Chunk-Wert
            // im "_"-Chunk. Der Segmenteintritt wandert nur mit, wenn
            // mindestens ein Item in diesen Chunk gehört. Chunks mit verteilten
            // Items bekommen einen Scoped Listen-Kopf (ItemListChunk), dessen
            // Einlesen nur den Bestand dieses Chunks angleicht — leere verteilte
            // Chunks räumen so ihren Bestand selbst ab.
            if (i + 1 < records.Count && records[i + 1].Record.TagType == FragmentTagType.ItemList && individual) {
                var j = i + 2;
                var keepItems = new List<int>();
                var distributed = false;

                while (j < records.Count && records[j].Record.Depth > record.Depth + 1) {
                    var itemId = itemIds.TryGetValue(records[j].Record.KeyNameContent ?? string.Empty, out var id) ? id : Chunk_MainData.ToLowerInvariant();

                    distributed = true;

                    if (string.Equals(itemId, chunkId, StringComparison.OrdinalIgnoreCase)) { keepItems.Add(j); }

                    j = BlockEnd(records, j);
                }

                if (keepItems.Count > 0 || (distributed && !StartupChunkIds.Contains(chunkId, StringComparer.OrdinalIgnoreCase))) {
                    result.AddRange(source[start..records[i + 1].Start]); // Segmenteintritt

                    if (distributed) {
                        result.AddRange(ScopedListHead(records[i + 1].Record.Depth, chunkId));
                    } else {
                        result.AddRange(source[records[i + 1].Start..records[i + 1].End]); // Listen-Kopf
                    }

                    foreach (var itemIndex in keepItems) {
                        var next = BlockEnd(records, itemIndex);
                        result.AddRange(source[records[itemIndex].Start..(next < records.Count ? records[next].Start : source.Length)]);
                    }
                }

                i = j;
                continue;
            }

            // Fester Bereich: pfadbasiert aufteilen — die Regel mit dem längsten
            // passenden Pfad gewinnt, tiefere Bereiche wandern in deren Ziel und
            // erhalten die Struktur-Köpfe ihres Pfades mit.
            var areaEnd = i + 1;
            while (areaEnd < records.Count && records[areaEnd].Record.Depth > 0) { areaEnd++; }

            var parts = PartitionArea(records, source, i, areaEnd, []);
            if (parts.TryGetValue(chunkId, out var sectionBytes)) { result.AddRange(sectionBytes); }

            i = areaEnd;
        }

        return result;
    }

    /// <summary>
    /// Liefert das Ziel der Areas-Regel mit dem längsten passenden Pfad — der
    /// Regel-Pfad muss Präfix des gesuchten Pfades sein, tiefere Regeln übersteuern
    /// dadurch die Sektions-Regel (Match von maximaler Pfadlänge abwärts). null
    /// ohne Treffer.
    /// </summary>
    private string? LookupArea(List<byte> path) {
        foreach (var area in SortAreasByPathLength()) {
            if (area.Key.Length > path.Count || area.Value is not { Length: > 0 }) { continue; }

            var match = true;

            for (var p = 0; p < area.Key.Length; p++) {
                if (area.Key[p] != path[p]) { match = false; break; }
            }

            if (match) { return area.Value; }
        }

        return null;
    }

    /// <summary>
    /// Sortiert die Areas-Regeln manuell nach Pfad-Länge, längster zuerst;
    /// gleiche Länge bleiben in Eintrag-Reihenfolge.
    /// </summary>
    private List<KeyValuePair<byte[], string>> SortAreasByPathLength() {
        List<KeyValuePair<byte[], string>> sorted = [];

        foreach (var area in Areas) {
            var i = 0;

            while (i < sorted.Count && sorted[i].Key.Length >= area.Key.Length) { i++; }

            sorted.Insert(i, area);
        }

        return sorted;
    }

    /// <summary>
    /// Liefert die Chunk-ID (lowercase), in die ein Undo-Eintrag gehört: Die
    /// Areas-Regel des Eintrags-Pfades entscheidet — Empfänger-Typen der Sätze,
    /// Steuer-Sätze bleiben außen vor. Lautet die Regel auf individuelle
    /// Verteilung (oder fehlt sie ganz), entscheidet der erste ItemPath-Item-Kenner;
    /// Einträge ohne Kenner — Kopf- und Struktur-Änderungen — laufen über die
    /// Areas-Regel, ersatzweise in den Haupt-Chunk. BuildChunkContent und
    /// TrimUndosPerChunk verteilen damit identisch; der Verify-Prüf-Lauf
    /// (TableStress) nutzt dieselbe Regel zur Ziel-Ermittlung.
    /// </summary>
    public string UndoEntryChunk(List<FragmentRecord> entry, Dictionary<string, string> itemIds) {
        List<byte> path = [];

        foreach (var record in entry) {
            if (record.LeafType > 0) { path.Add(record.LeafType); }
        }

        var area = path.Count > 0 ? LookupArea(path) : null;

        if (area is not { Length: > 0 } || string.Equals(area, Area_Individual, StringComparison.OrdinalIgnoreCase)) {
            foreach (var record in entry) {
                if (record.TagType == FragmentTagType.ItemPath && record.DataType == FragmentDataType.String && itemIds.TryGetValue(record.StringContent, out var byItem)) { return byItem; }
            }

            return Chunk_MainData.ToLowerInvariant();
        }

        return area.ToLowerInvariant();
    }

    /// <summary>
    /// Teilt den Block einer Sektion pfadbasiert auf: Jeder Bereich läuft in den
    /// Chunk der Areas-Regel mit dem längsten passenden Pfad. Struktur-Köpfe
    /// (Segmenteintritt, Listen-Kopf, Item-Add) wandern mit in jeden Ziel-Chunk,
    /// damit der Teil-Bestand dort lesbar bleibt; bei aufgeteilten Listen wird der
    /// Listen-Kopf zum Scoped-Kopf (ItemListChunk).
    /// </summary>
    private Dictionary<string, List<byte>> PartitionArea(List<(int Start, int End, FragmentRecord Record)> records, byte[] source, int i, int end, List<byte> parentPath) {
        var (Start, End, Record) = records[i];

        // Steuer-Sätze (ItemList, ItemAdd, …) tragen keinen Pfad-Bestandteil
        List<byte> path;

        if (Record.LeafType != 0) {
            path = [.. parentPath, Record.LeafType];
        } else {
            path = parentPath;
        }

        var headBytes = source[Start..End];
        Dictionary<string, List<byte>>? parts = null;
        var j = i + 1;

        // Kinder: Sätze genau eine Ebene tiefer, je als Block-Unit
        while (j < end && records[j].Record.Depth == Record.Depth + 1) {
            var childEnd = BlockEnd(records, j);

            foreach (var kvp in PartitionArea(records, source, j, childEnd, path)) {
                if (parts is null) { parts = new Dictionary<string, List<byte>>(StringComparer.OrdinalIgnoreCase); }

                if (!parts.TryGetValue(kvp.Key, out var list)) {
                    parts[kvp.Key] = list = [];
                }

                list.AddRange(kvp.Value);
            }

            j = childEnd;
        }

        // Blatt-Bereich: Kopf samt Block komplett in sein Ziel
        if (parts is null) {
            var blockEnd = end < records.Count ? records[end].Start : source.Length;

            return new Dictionary<string, List<byte>>(StringComparer.OrdinalIgnoreCase) { [LookupArea(path) ?? Chunk_MainData.ToLowerInvariant()] = [.. source[Start..blockEnd]] };
        }

        // Kopf je Ziel-Chunk voranstellen: unverändert, wenn der komplette
        // Teilbaum in einem Chunk landet, sonst als Scoped-Kopf (nur Listen)
        if (parts.Count == 1) {
            parts.First().Value.InsertRange(0, headBytes);
            return parts;
        }

        foreach (var key in parts.Keys) {
            parts[key].InsertRange(0, Record.TagType == FragmentTagType.ItemList ? ScopedListHead(Record.Depth, key) : headBytes);
        }

        return parts;
    }

    /// <summary>
    /// Erzeugt einen Scoped Listen-Kopf (ItemListChunk, Content = Chunk-ID), dessen
    /// Einlesen nur den Bestand dieses Chunks angleicht.
    /// </summary>
    private static List<byte> ScopedListHead(int depth, string chunkId) {
        using var head = new MemoryStream();
        _ = FragmentRecord.Append(head, depth, FragmentTagType.ItemListChunk, chunkId);
        return [.. head.ToArray()];
    }

    /// <summary>
    /// Index des ersten Satzes, der nicht mehr zum Block an der angegebenen
    /// Position gehört (nächster Kopf derselben Ebene oder ein Satz darüber).
    /// </summary>
    private static int BlockEnd(List<(int Start, int End, FragmentRecord Record)> records, int itemIndex) {
        var depth = records[itemIndex].Record.Depth;
        var i = itemIndex + 1;

        while (i < records.Count && records[i].Record.Depth > depth) { i++; }

        return i;
    }

    /// <summary>
    /// Inhalt ab dem ersten Nutzdaten-Satz (Datei-Stempel am Kopf ausgenommen);
    /// Vergleichsbasis des Unverändert-Checks, da die Stempel je Speicherung wechseln.
    /// </summary>
    private static byte[] ChunkCompareBasis(byte[] content) {
        using var ms = new MemoryStream(content);

        var start = 0L;
        while (FragmentRecord.TryReadNext(ms) is { IsFileTag: true }) { start = ms.Position; }

        return content[(int)start..];
    }

    #endregion

    #region Properties

    /// <summary>
    /// Wert in Sekunden. Eine Benutzer-Datei, die jünger ist, blockiert andere Benutzer.
    /// Nur der Ersteller der Datei darf in diesem Zeitraum bearbeiten.
    /// </summary>
    public static int EditLockSeconds { get; set; } = 300;

    /// <summary>
    /// Multi-User-Zugriff ist über das Chunk-Layout möglich.
    /// </summary>
    public override bool MultiUserPossible => true;

    /// <summary>
    /// Datum/Uhrzeit der letzten Speicherung der Hauptdatei (UTC).
    /// </summary>
    public override DateTime LastSaveMainFileUtcDate {
        get {
            if (string.IsNullOrEmpty(Filename)) { return new DateTime(2000, 1, 1, 0, 0, 0, DateTimeKind.Utc); }

            var fi = IO.GetFileInfo(Filename);
            if (fi is { Exists: true }) { return fi.LastWriteTimeUtc; }

            return new DateTime(2000, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        }
    }

    /// <summary>
    /// Das Objekt gilt als kürzlich verwendet, wenn mindestens ein Chunk
    /// kürzlich verwendet wurde.
    /// </summary>
    public override bool IsRecentlyUsed {
        get {
            if (_lastUsed.IsEmpty) { return false; }
            var threshold = DateTime.UtcNow.AddMinutes(-SkipIfUnusedMinutes);
            foreach (var kvp in _lastUsed) {
                if (kvp.Value > threshold) { return true; }
            }
            return false;
        }
    }

    #endregion

    #region Methods

    /// <summary>
    /// Öffnet und lädt die Kopf-Datei und richtet den Chunk-Arbeitsordner ein.
    /// </summary>
    public override void LoadFromFile(string info, string freeze) {
        if (!PrepareLoad(info, out var fileNameToLoad)) { return; }

        Filename = fileNameToLoad;

        LoadCore(fileNameToLoad, freeze);
    }

    /// <summary>
    /// Stellt sicher, dass alle relevanten Chunks aktuell sind: Die Startup-Chunks
    /// des Owners (StartupChunkIds) werden immer geprüft, Daten-Chunks über das
    /// Fenster-Refresh.
    /// </summary>
    public override bool Refresh(bool firstTime) {
        if (IsDisposed || !DropMessages) { return true; }
        if (string.IsNullOrEmpty(Filename)) { return true; }

        // Reentrancy-Sperre: Timer sowie UI-Aktionen können Refresh zeitgleich
        // aufrufen. Ohne Sperre würden sich die Aufrufe überlappen.
        if (Interlocked.CompareExchange(ref _isRefreshingChunks, 1, 0) != 0) {
            Develop.Message(ErrorType.Info, this, KeyName, ImageCode.Tabelle, $"Aktualisierung von '{KeyName}' läuft bereits — übersprungen, um eine Überlagerung zu vermeiden", 0);
            return true;
        }

        try {
            Develop.Message(ErrorType.Info, this, KeyName, ImageCode.Tabelle, "Lade Chunks von '" + KeyName + "'", 0);

            if (firstTime) {
                if (IO.CreateDirectory(BaseChunkFolder()).IsFailed) { return false; }
            }

            var loaded = false;
            var ok = true;

            // Während des Neuladens sind Schlüssel kurzzeitig nicht auflösbar.
            PauseDataReload();
            try {
                OnLoading();

                // Startup-Chunks laut Owner: werden immer geprüft (kein
                // SkipIfUnusedMinutes-Skip). LoadChunkWithChunkId kehrt bei
                // unveränderten Dateien schnell zurück (Already-Current-Check via
                // Dateiname-Vergleich). Der Haupt-Chunk steht vorne in der Liste
                // und muss geladen sein, bevor die übrigen Bereiche folgen.
                foreach (var chunkId in StartupChunkIds) {
                    var result = LoadChunkWithChunkId(chunkId, false);

                    if (string.Equals(chunkId, Chunk_MainData, StringComparison.OrdinalIgnoreCase) && result.IsFailed) {
                        Develop.Message(ErrorType.Warning, this, KeyName, ImageCode.Tabelle, $"Haupt-Chunk von '{KeyName}' konnte nicht geladen werden: {result.FailedReason}", 0);
                        return false;
                    }

                    if (result.IsFailed) {
                        Develop.Message(ErrorType.Warning, this, KeyName, ImageCode.Tabelle, $"Chunk '{chunkId}' von '{KeyName}' konnte nicht geladen werden: {result.FailedReason}", 0);
                    }

                    loaded = loaded || result.Value is true;
                    ok = ok && result.IsSuccessful;
                }

                OnRefreshDataSystems();

                loaded = loaded || RefreshLoadedChunks(firstTime);

                if (loaded) { OnLoaded(firstTime); }
            } finally {
                ResumeDataReload();
            }

            return ok;
        } finally {
            Interlocked.Exchange(ref _isRefreshingChunks, 0);
        }
    }

    /// <summary>
    /// Periodischer Tick: lädt Chunks nach und speichert bedarfsgesteuert.
    /// </summary>
    public override void CheckerTick() {
        if (!SaveRequired || !MaySaveNow) { return; }

        // Speichern wenn nötig (der Chunk-Writer prüft die Edit-Sperren je Chunk)
        _ = Save();
    }

    /// <summary>
    /// Liefert die Item-Schlüssel mit ihrer Chunk-ID (lowercase).
    /// </summary>
    private Dictionary<string, string> ItemChunkIds() {
        var ids = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        foreach (var item in ChunkItemSource) {
            if (item.KeyName is not { Length: > 0 } key || item is not IHasChunkValue hasChunkValue) { continue; }

            if (hasChunkValue.ChunkId() is not { Length: > 0 } id) { continue; }

            ids[key] = id;
        }

        return ids;
    }

    /// <summary>
    /// Liefert die Chunk-IDs (lowercase, sortiert-distinkt) aller Chunk-Items;
    /// die IDs liefert IHasChunkValueExtension.ChunkId.
    /// </summary>
    public List<string> KnownChunkIds() {
        List<string> ids = [];

        foreach (var item in ChunkItemSource) {
            if (item is not IHasChunkValue hasChunkValue) { continue; }

            if (hasChunkValue.ChunkId() is not { Length: > 0 } id) { continue; }

            ids.Add(id);
        }

        return ids.SortedDistinctList();
    }

    /// <summary>
    /// Setzt den LastUsed-Stempel des Chunks mit dem angegebenen Namen.
    /// </summary>
    public override void TouchChunk(string chunkValue) {
        if (chunkValue is not { Length: > 0 }) { return; }

        _lastUsed[IHasChunkValueExtension.ChunkId(chunkValue)] = DateTime.UtcNow;
    }

    /// <summary>
    /// Nimmt einen kompletten Undo-Eintrag entgegen und bucht ihn samt
    /// Fingerabdruck in die Änderungs-Historie. Während des Einspielens fremder
    /// Daten und vor der Erstanlage wird still verworfen.
    /// </summary>
    public override void LogUndoEntry(IReadOnlyList<FragmentRecord> entry) {
        if (IsDisposed || entry is not { Count: > 0 }) { return; }
        if (InitialSavePending || IsDataReloading) { return; }

        lock (UndoLock) {
            AddUndoEntry(entry);
        }
    }

    /// <summary>
    /// Verteilt die Änderungs-Historie analog zur Areas-Berechnung auf die
    /// Ziel-Chunks — Ziel eines Eintrags ist wie in BuildChunkContent der Chunk
    /// der Areas-Regel seines Pfades (individuell: der Chunk des ersten
    /// ItemPath-Item-Kenners, sonst der Haupt-Chunk) — und kürzt je Ziel-Chunk
    /// auf die neuesten MaxUndos Sätze. Geliefert wird die Union; BuildChunkContent
    /// verteilt sie beim Zerlegen identisch.
    /// </summary>
    private List<FragmentRecord> TrimUndosPerChunk() {
        List<FragmentRecord> all;
        lock (UndoLock) {
            all = [.. Undos];
        }

        var itemIds = ItemChunkIds();
        var perChunk = new Dictionary<string, List<FragmentRecord>>(StringComparer.OrdinalIgnoreCase);

        var entryStart = 0;
        while (entryStart < all.Count) {
            var entryEnd = entryStart + 1;
            while (entryEnd < all.Count && all[entryEnd].Depth != 0) { entryEnd++; }

            var target = UndoEntryChunk(all.GetRange(entryStart, entryEnd - entryStart), itemIds);

            if (!perChunk.TryGetValue(target, out var list)) {
                list = [];
                perChunk[target] = list;
            }

            list.AddRange(all.GetRange(entryStart, entryEnd - entryStart));
            entryStart = entryEnd;
        }

        List<FragmentRecord> result = [];
        foreach (var chunk in perChunk.Values) { result.AddRange(chunk.TrimUndos()); }

        return result;
    }

    /// <summary>
    /// Stellt sicher, dass der Chunk mit dem angegebenen Namen geladen ist.
    /// </summary>
    public override OperationResult EnsureLoaded(string chunk, bool trustProcessedFile) {
        return LoadChunkWithChunkId(IHasChunkValueExtension.ChunkId(chunk), trustProcessedFile);
    }

    /// <summary>
    /// Lädt alle verfügbaren Daten-Chunks nach (Komplett-Scan); einzelne Chunks
    /// lädt EnsureLoaded gezielt nach. Das Nachladen parst jede Chunk-Datei
    /// zwangsweise in kanonischer (alphabetischer) Reihenfolge: Zeilen-getriggerte
    /// Einzel-Ladevorgänge können Chunks vorher angewendet haben — nur der Zwang
    /// hält die Reihenfolge definiert, und der zeitstempelbewusste Merge in den
    /// Items entscheidet, welcher Kopien-Stand obsiegt.
    /// </summary>
    public override bool EnsureDataLoaded(bool oldest, int count, string info) {
        var ok = true;

        foreach (var chunkId in GetAvailableChunkIds()) {
            ok &= LoadChunkForced(chunkId).IsSuccessful;
        }

        return ok;
    }

    /// <summary>
    /// Liefert die dateirelevanten Gründe, warum das Objekt aktuell nicht
    /// editierbar ist; das Chunk-Arbeitsverzeichnis muss beschreibbar sein.
    /// </summary>
    public override string EditableError(bool isloading) {
        var f = base.EditableError(isloading);
        if (!string.IsNullOrEmpty(f)) { return f; }

        // Frische Erstanlage: Der Ordner fehlt noch und wird vom ersten Save() angelegt.
        if (!IO.DirectoryExists(BaseChunkFolder())) { return string.Empty; }

        var opr = IO.CanWriteInDirectory(BaseChunkFolder());

        return opr.IsFailed ? opr.FailedReason : string.Empty;
    }

    /// <summary>
    /// Prüft die Edit-Sperre des Chunks mit dem angegebenen Namen; ohne Namen
    /// wird der Haupt-Chunk geprüft.
    /// </summary>
    public override string IsEditable(string chunk) {
        if (chunk is not { Length: > 0 }) { return CheckEditLock(Chunk_MainData.ToLowerInvariant()); }

        return CheckEditLock(IHasChunkValueExtension.ChunkId(chunk));
    }

    /// <summary>
    /// Tiefe Prüfung vor der Bearbeitung des Chunks mit dem angegebenen Namen:
    /// Edit-Sperre prüfen und den Chunk nachladen.
    /// </summary>
    public override string PrepareEdit(string chunk) {
        var chunkId = chunk is { Length: > 0 } ? IHasChunkValueExtension.ChunkId(chunk) : Chunk_MainData.ToLowerInvariant();

        if (CheckEditLock(chunkId) is { Length: > 0 } f) { return f; }

        var result = LoadChunkWithChunkId(chunkId, false);

        return result.IsFailed ? result.FailedReason : string.Empty;
    }

    /// <summary>
    /// Löscht alle Chunk-Ordner, die nicht zum immer geladenen Stamm
    /// (StartupChunkIds) gehören.
    /// </summary>
    public override void Reorganize() {
        var chunkFolder = BaseChunkFolder();
        if (!IO.DirectoryExists(chunkFolder)) { return; }

        var startupIds = StartupChunkIds.ToHashSet(StringComparer.OrdinalIgnoreCase);

        foreach (var subDir in IO.GetDirectories(chunkFolder)) {
            var chunkId = subDir.TrimEnd('\\').FileNameWithSuffix().ToLowerInvariant();
            if (startupIds.Contains(chunkId)) { continue; }

            IO.DeleteDir(subDir, false);
            _processedFile.TryRemove(chunkId, out _);
            _lastUsed.TryRemove(chunkId, out _);
            _lastContentHash.TryRemove(chunkId, out _);
        }
    }

    /// <summary>
    /// Speichert alle Chunks direkt auf die Festplatte.
    /// Nach dem Speichern werden alle generierten Daten verworfen.
    /// Jeder Chunk ist ein Einweg — eine einmal gespeicherte Datei wird nie
    /// wieder überschrieben. Die Kopf-Datei trägt nur den Layout-Kopf (Lite).
    /// <para>
    /// Jeder Daten-Chunk wird vor der Generierung geladen — der Inhalt ist ein
    /// vollständiger Snapshot aus dem Speicher. Wandert ein Wert in einen Chunk,
    /// muss der Ziel-Chunk also vorher geladen sein.
    /// </para>
    /// <para>
    /// Die Editierbarkeit wird PRO CHUNK geprüft: Chunks, die von einem anderen
    /// Benutzer aktiv gesperrt sind, werden übersprungen und ihre Änderungen
    /// verworfen. Alle bearbeitbaren Chunks werden normal gespeichert. Das
    /// Chunk-Arbeitsverzeichnis muss beschreibbar sein (EditableError).
    /// </para>
    /// </summary>
    public override OperationResult Save() {
        if (!SaveRequired) { return OperationResult.Success; }
        if (IsDisposed) { return OperationResult.Failed("Objekt ist bereits freigegeben"); }

        PauseTimer();

        try {
            // Nur die Basis-Checks — NICHT die System-Chunks-Verifikation aus
            // CheckFileEditable. Jene würde die gesamte Speicherung blockieren,
            // sobald ein einziger System-Chunk (z.B. Master) gelocked ist.
            // Die Per-Chunk-Prüfung erfolgt in AddChunk.
            if (EditableError(false) is { Length: > 0 } f) { return OperationResult.Failed(f); }

            EnsureHeadFileExists();

            var x = LastChangeUtc;

            var timestamp = GenerateChunkTimestamp();

            // Daten-Chunks ermitteln: alle Chunks mit aktuellem Item-Bestand plus
            // alle Chunks mit bestehendem Ordner — Letztere bekommen ohne Bestand
            // nur ihren Schluss-Kopf (Scoped-Listenkopf ohne Items) geschrieben,
            // damit das Einlesen deren Altbestand räumt und keine verwaisten Ordner
            // bleiben. Ordner ohne bisherigen Inhalt bleiben außen vor (keine
            // Phantom-Ordner).
            var dataChunkIds = KnownChunkIds();

            foreach (var thisChunkId in GetAvailableChunkIds()) {
                if (!dataChunkIds.Contains(thisChunkId)) { dataChunkIds.Add(thisChunkId); }
            }

            // Ein Daten-Chunk wird nur gespeichert, wenn er vorher geladen wurde —
            // der Inhalt wird aus dem Fragment-Baum geschnitten. Wandert ein Wert
            // in einen noch nicht geladenen Chunk, wird dieser zuerst geladen,
            // sonst fehlen seine übrigen Items im Snapshot und würden beim
            // nächsten Einlesen als veraltet entfernt. Lädt der Chunk nicht, wird
            // er übersprungen — SaveRequired bleibt aktiv, der nächste Versuch
            // holt ihn nach.
            var loadFailedChunkIds = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            foreach (var thisChunkId in dataChunkIds) {
                if (_processedFile.ContainsKey(thisChunkId)) { continue; }

                var loadResult = LoadChunkWithChunkId(thisChunkId, false);

                if (loadResult.IsFailed) {
                    loadFailedChunkIds.Add(thisChunkId);
                    Develop.Message(ErrorType.Warning, null, KeyName, ImageCode.Diskette, $"Chunk '{thisChunkId}' von '{KeyName}' konnte nicht geladen werden — Speicherung übersprungen: {loadResult.FailedReason}", 0);
                }
            }

            // Vollständiger Fragment-Baum des Speicherstands; Quelle aller Chunk-Inhalte.
            // Die Historie geht je Ziel-Chunk verteilt und gekürzt ein.
            var source = GenerateContentToSave(TrimUndosPerChunk()).ToArray();

            // path → vollständiger Inhalt, fertig zum Schreiben
            var chunkData = new Dictionary<string, List<byte>>(StringComparer.OrdinalIgnoreCase);
            // chunkId (lowercase) → Hash des Voll-Inhalts, aktualisiert nach erfolgreichem Speichern
            var newHashes = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            // chunkIds mit echten Änderungen (für EditLock Re-Check vor dem Schreiben)
            var changedChunkIds = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            // true, wenn mindestens ein Daten-Chunk durch die Edit-Sperre blockiert
            // wurde. In diesem Fall bleibt SaveRequired true, damit der nächste
            // Speicherversuch die ungespeicherten Änderungen erneut versucht.
            var dataChunksSkipped = false;

            void AddChunk(string chunkId) {
                var idLower = chunkId.ToLowerInvariant();

                // Per-Chunk-Editierbarkeit: ist der Chunk durch einen anderen
                // Benutzer gesperrt, wird er übersprungen — die anderen Chunks
                // werden weiter gespeichert. SaveRequired bleibt aktiv, damit die
                // ungespeicherten Änderungen beim nächsten Versuch erneut
                // gespeichert werden (und nicht beim nächsten Refresh gelöscht
                // werden).
                if (CheckEditLock(chunkId) is { Length: > 0 }) {
                    dataChunksSkipped = true;
                    return;
                }

                // Vollständigen Inhalt aufbauen (inkl. Stempel und Abschluss-Marker)
                var fullContent = BuildChunkContent(chunkId, source);
                if (fullContent.Count == 0) { return; }

                var fullHash = Generic.GetSHA256HashString(ChunkCompareBasis(fullContent.ToArray()));
                newHashes[idLower] = fullHash;

                // Unchanged-Check: bei identischem Inhalt keine neue Datei schreiben
                if (_lastContentHash.TryGetValue(idLower, out var storedHash) && storedHash == fullHash) { return; }

                chunkData[$"{GetChunkFolder(chunkId)}{timestamp}.tblc"] = fullContent;

                changedChunkIds.Add(idLower);
            }

            // System-Chunks laut Startup-Liste; der Haupt-Chunk steht (garantiert
            // durch den StartupChunkIds-Setter) vorne in der Liste.
            foreach (var chunkId in StartupChunkIds) {
                AddChunk(chunkId);
            }

            foreach (var thisChunkId in dataChunkIds) {
                if (loadFailedChunkIds.Contains(thisChunkId)) { dataChunksSkipped = true; continue; }

                AddChunk(thisChunkId);
            }

            if (x != LastChangeUtc) { return OperationResult.Failed("Daten wurden während der Chunk-Generierung geändert"); }

            // Re-Check: Es wird vorab kein Claim geschrieben, daher kann ein anderer
            // Benutzer zwischenzeitlich einen der zu schreibenden Chunks gesperrt haben.
            // In diesem Fall Speichern abbrechen und einfrieren, statt fremde Änderungen zu
            // überschreiben. changedChunkIds enthält nur Chunks mit echten inhaltlichen Änderungen.
            foreach (var chunkId in changedChunkIds) {
                if (CheckEditLock(chunkId) is { Length: > 0 } lockCheck) {
                    Freeze(lockCheck);
                    return OperationResult.Failed(lockCheck);
                }
            }

            // Direkt auf Festplatte schreiben.
            // Chunks sind write-once — jede Datei ist neu und wird nie überschrieben.
            Develop.Message(ErrorType.Info, null, KeyName, ImageCode.Diskette, $"Speichere {chunkData.Count} Chunks von '{KeyName}'", 2);

            foreach (var kvp in chunkData) {
                var path = kvp.Key;

                if (IO.CreateDirectory(path.FilePath()).IsFailed) {
                    Freeze($"Verzeichnis für '{path}' konnte nicht erstellt werden");
                    return OperationResult.Failed($"Verzeichnis für '{path}' konnte nicht erstellt werden");
                }

                // Chunk-Dateien werden gezippt gespeichert
                var zipped = kvp.Value.ToArray().ZipIt();
                if (zipped is null || zipped.Length == 0) {
                    Freeze($"Komprimierung fehlgeschlagen für '{path}'");
                    return OperationResult.Failed($"Komprimierung fehlgeschlagen für '{path}'");
                }

                var writeResult = IO.WriteAllBytes(path, zipped);
                if (writeResult.IsFailed) {
                    Freeze(writeResult.FailedReason);
                    return OperationResult.Failed(writeResult.FailedReason);
                }

                // Nach dem Speichern: nur den Dateinamen tracken, Daten verwerfen
                var folder = path.FilePath();
                var chunkId = folder.TrimEnd('\\').FileNameWithSuffix().ToLowerInvariant();
                _processedFile[chunkId] = path;
                _lastUsed[chunkId] = DateTime.UtcNow;
                CleanupOldFilesInFolder(GetChunkFilesOrderedByTime(folder).ToArray());
            }

            // Hashes nach erfolgreichem Speichern aktualisieren
            foreach (var kvp2 in newHashes) {
                _lastContentHash[kvp2.Key] = kvp2.Value;
            }

            // SaveRequired nur zurücksetzen, wenn alle Daten-Chunks gespeichert
            // wurden. Wurden Chunks durch die Edit-Sperre blockiert, bleibt
            // SaveRequired true — der nächste Speicherversuch (Checker-Tick)
            // versucht es erneut, sobald die Sperre abgelaufen ist.
            SaveRequired = dataChunksSkipped;
            InitialSavePending = false;
            MainLoadDone = true; // die Chunks entsprechen jetzt dem Speicherstand

            return OperationResult.Success;
        } finally {
            OnInvalidateView();
            ResumeTimer();
        }
    }

    /// <summary>
    /// Lädt den Haupt-Chunk aus dem Arbeitsverzeichnis.
    /// </summary>
    protected override bool LoadMainDataCore() {
        if (IO.CreateDirectory(BaseChunkFolder()).IsFailed) { return false; }

        var result = LoadChunkWithChunkId(Chunk_MainData, false);
        if (result.IsFailed) { return false; }

        // Bei geladenen Daten muss der Hauptchunk vorhanden sein.
        // Fehlt er (Ordner leer), sind die Daten korrupt.
        if (!_processedFile.ContainsKey(Chunk_MainData.ToLowerInvariant())) {
            Freeze("Hauptchunk der Daten fehlt");
            return false;
        }

        // System- und Daten-Chunks folgen über den Startup-Refresh bzw. das
        // bedarfsgesteuerte Nachladen (StartupChunkIds bzw. EnsureLoaded).
        InitialSavePending = false; // vollständiger Stand aus dem Haupt-Chunk geladen
        return true;
    }

    /// <summary>
    /// Dateiberechtigte Prüfung nach dem gelungenen Einlesen: Das Chunk-
    /// Arbeitsverzeichnis muss beschreibbar sein.
    /// </summary>
    protected override void CheckWritePermissionAfterLoad(string filename) {
        var opr = IO.CanWriteInDirectory(BaseChunkFolder());

        if (opr.IsFailed) { Freeze(opr.FailedReason); }
    }

    /// <summary>
    /// Generiert den Timestamp-String für Chunk-Dateinamen im Format
    /// yyyy-MM-dd-HH-mm-ss-fff_Username-Hash.
    /// </summary>
    internal string GenerateChunkTimestamp() =>
        $"{DateTime.UtcNow:yyyy-MM-dd-HH-mm-ss-fff}_{UserName}-{MyId}";

    /// <summary>
    /// Extrahiert das UTC-Datum aus einem Bereichs-Dateinamen
    /// (Format: yyyy-MM-dd-HH-mm-ss-fff_Username-Hash.Suffix).
    /// Millisekunden sind verpflichtend. Liefert null, wenn das Datum nicht geparst werden kann.
    /// </summary>
    internal static DateTime? ExtractDateFromFileName(string filePath) {
        var fileName = filePath.FileNameWithoutSuffix();
        if (fileName.Length < 23) { return null; }
        var datePart = fileName[..23];
        if (DateTime.TryParseExact(datePart, "yyyy-MM-dd-HH-mm-ss-fff", CultureInfo.InvariantCulture, DateTimeStyles.None, out var result)) {
            return DateTime.SpecifyKind(result, DateTimeKind.Utc);
        }
        return null;
    }

    /// <summary>
    /// Löscht alle Chunk-Dateien in der übergebenen Liste, die älter als
    /// DeleteOldFilesAfterHours Stunden sind, sofern mindestens FileCount neuere Dateien
    /// vorhanden sind. Dateien mit ungültigem Datumsformat werden ebenfalls gelöscht.
    /// Die Liste muss nach Datum absteigend (neueste zuerst) sortiert sein.
    /// </summary>
    private static void CleanupOldFilesInFolder(string[] files) {
        if (files.Length <= FileCount) { return; }

        var threshold = DateTime.UtcNow.AddHours(-DeleteOldFilesAfterHours);

        for (var i = files.Length - 1; i >= FileCount; i--) {
            var file = files[i];
            var fileDate = ExtractDateFromFileName(file);
            if (!fileDate.HasValue || fileDate.Value < threshold) {
                try { IO.DeleteFile(file, false); } catch { }
            }
        }
    }

    /// <summary>
    /// Extrahiert den 3-stelligen Machine/Instance-Hash aus einem Chunk-Dateinamen
    /// (Format: yyyy-MM-dd-HH-mm-ss-fff_Username-Hash). Gibt string.Empty zurück,
    /// wenn kein Hash vorhanden ist (alte Dateien).
    /// </summary>
    private static string ExtractMachineInstanceHashFromFileName(string filePath) {
        var fileName = filePath.FileNameWithoutSuffix();
        var idx = fileName.IndexOf('_');
        if (idx <= 0) { return string.Empty; }
        var userAndHash = fileName[(idx + 1)..];
        var sepIdx = userAndHash.LastIndexOf('-');
        // Hash ist genau 3 Zeichen am Ende, eingeleitet durch einen Bindestrich.
        // sepIdx muss >= 0 sein, sonst hat userAndHash keinen Bindestrich (alte Datei).
        return sepIdx >= 0 && sepIdx == userAndHash.Length - 4 ? userAndHash[(sepIdx + 1)..] : string.Empty;
    }

    /// <summary>
    /// Extrahiert den Benutzernamen aus einem Chunk-Dateinamen
    /// (Format: yyyy-MM-dd-HH-mm-ss-fff_Username-Hash). Der angehängte
    /// Machine/Instance-Hash wird abgeschnitten. Bei alten Dateien ohne Hash
    /// wird der komplette Teil nach dem ersten Unterstrich zurückgegeben.
    /// </summary>
    private static string ExtractUserNameFromFileName(string filePath) {
        var fileName = filePath.FileNameWithoutSuffix();
        var idx = fileName.IndexOf('_');
        if (idx <= 0) { return fileName; }
        var userAndHash = fileName[(idx + 1)..];
        var sepIdx = userAndHash.LastIndexOf('-');
        // Hash ist genau 3 Zeichen am Ende, eingeleitet durch einen Bindestrich.
        // sepIdx muss >= 0 sein, sonst hat userAndHash keinen Bindestrich (alte Datei).
        return sepIdx >= 0 && sepIdx == userAndHash.Length - 4 ? userAndHash[..sepIdx] : userAndHash;
    }

    protected static DateTime GetChunkDirLastWriteUtc(string dir) {
        try {
            return IO.DirectoryExists(dir) ? new DirectoryInfo(dir).LastWriteTimeUtc : DateTime.MaxValue;
        } catch {
            return DateTime.MaxValue;
        }
    }

    /// <summary>
    /// Listet alle Chunk-Dateien im Ordner auf, sortiert nach Datum im Dateinamen
    /// (neueste zuerst). Dateien ohne gültiges Datum werden ans Ende sortiert.
    /// </summary>
    private static IEnumerable<string> GetChunkFilesOrderedByTime(string folder) {
        if (!IO.DirectoryExists(folder)) { return []; }

        try {
            return IO.GetFiles(folder)
                .Where(f => string.Equals(f.FileSuffix(), "tblc", StringComparison.OrdinalIgnoreCase))
                .OrderByDescending(f => ExtractDateFromFileName(f) ?? DateTime.MinValue);
        } catch {
            return [];
        }
    }

    /// <summary>
    /// [Datenordner]\[Name]\
    /// </summary>
    protected string BaseChunkFolder() => $"{Filename.FilePath()}{Filename.FileNameWithoutSuffix()}\\";

    /// <summary>
    /// Prüft die Edit-Sperre für einen Chunk: Wenn die neueste Datei im Ordner
    /// jünger als EditLockSeconds ist und nicht vom aktuellen Benutzer stammt,
    /// wird das Bearbeiten blockiert.
    /// </summary>
    protected string CheckEditLock(string chunkId) {
        var folder = GetChunkFolder(chunkId);
        if (!IO.DirectoryExists(folder)) { return string.Empty; }

        if (GetChunkFilesOrderedByTime(folder).FirstOrDefault() is not { } newestFile) { return string.Empty; }

        var fileDate = ExtractDateFromFileName(newestFile);
        if (!fileDate.HasValue) { return "Datums-Fehler in Dateien"; }

        if (DateTime.UtcNow.Subtract(fileDate.Value).TotalSeconds >= EditLockSeconds) {
            return string.Empty;
        }

        if (IsFileFromCurrentUser(newestFile)) { return string.Empty; }

        var creator = ExtractUserNameFromFileName(newestFile);
        var creatorHash = ExtractMachineInstanceHashFromFileName(newestFile);
        var creatorDisplay = string.IsNullOrEmpty(creatorHash) ? creator : $"{creator} - {creatorHash}";
        var lockUntil = fileDate.Value.AddSeconds(EditLockSeconds).ToLocalTime();
        return $"Der Chunk '{chunkId}' wird bis {lockUntil:HH:mm} Uhr von '{creatorDisplay}' bearbeitet";
    }

    /// <summary>
    /// Listet alle Chunk-IDs aus dem Dateisystem auf — jeder Ordner ist ein
    /// Chunk (bis auf Altlasten-Ordner).
    /// </summary>
    protected List<string> GetAvailableChunkIds() {
        if (string.IsNullOrEmpty(Filename)) { return []; }

        var chunkFolder = BaseChunkFolder();
        if (!IO.DirectoryExists(chunkFolder)) { return []; }

        // Ordnernamen (= Chunk-IDs) direkt als Chunk-Werte verwenden.
        var allChunkIds = new List<string>();
        foreach (var subDir in IO.GetDirectories(chunkFolder)) {
            var chunkId = subDir.TrimEnd('\\').FileNameWithSuffix().ToLowerInvariant();
            allChunkIds.Add(chunkId); 
        }

        return allChunkIds.OrderBy(c => c).ToList();
    }

    /// <summary>
    /// [Datenordner]\[Name]\[Chunk]\
    /// </summary>
    protected string GetChunkFolder(string chunkId) => $"{BaseChunkFolder()}{chunkId.ToLowerInvariant()}\\";

    /// <summary>
    /// Prüft, ob die gegebene Chunk-Datei vom aktuellen Benutzer erstellt wurde.
    /// Der Machine/Instance-Hash dient nur der Anzeige: Auch eine neue Instanz
    /// (neue Session, neuer Prozess) darf die eigenen Dateien ihres Benutzers
    /// weiterbearbeiten; die Edit-Sperre schützt ausschließlich vor fremden
    /// Benutzern.
    /// </summary>
    private bool IsFileFromCurrentUser(string filePath) {
        var creator = ExtractUserNameFromFileName(filePath);
        return string.Equals(creator, UserName, StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Lädt einen Chunk direkt von der Festplatte.
    /// Nach dem Parsen wird der Chunk-Inhalt verworfen — nur der Dateiname wird
    /// für Aktualitätsprüfungen gemerkt.
    /// </summary>
    /// <param name="chunkId">Chunk-ID (wird auf Lowercase normalisiert).</param>
    /// <param name="trustProcessedFile">True: bereits geladene Chunks (Eintrag in _processedFile)
    /// erzeugen keinen Verzeichniszugriff mehr — der erste Load bleibt unberührt,
    /// der Reload läuft ausschließlich über RefreshLoadedChunks. False: immer prüfen.</param>
    /// <returns>Ob ein Load stattgefunden hat. False heißt, es ist so alles in Ordnung gewesen. Fehler können mit IsFailed abgefragt werden.</returns>
    protected OperationResult LoadChunkWithChunkId(string chunkId, bool trustProcessedFile) {
        if (string.IsNullOrEmpty(chunkId)) { return OperationResult.Failed("Keine ID angekommen"); }
        chunkId = chunkId.ToLowerInvariant();

        // Reload ignorieren: Der Chunk ist geladen und wird nicht erneut von der
        // Festplatte geprüft. Aktualität liefert RefreshLoadedChunks (Fenster).
        if (trustProcessedFile && _processedFile.ContainsKey(chunkId)) {
            _lastUsed[chunkId] = DateTime.UtcNow;
            Develop.Diagnose("ChunkLoad", $"{chunkId} skip-verarbeitet");
            return OperationResult.SuccessFalse;
        }

        var folder = GetChunkFolder(chunkId);

        // Neueste Datei im Ordner ermitteln — entscheidend für den Multi-User-Abgleich.
        // Ein anderer Benutzer konnte zwischenzeitlich eine neuere Datei ablegen.
        var chunkFiles = IO.DirectoryExists(folder) ? GetChunkFilesOrderedByTime(folder).ToArray() : [];
        string? newestFile = chunkFiles.Length > 0 ? chunkFiles[0] : null;

        // Bereits aktuell? (gleiche Datei wie zuletzt verarbeitet)
        if (newestFile is not null &&
            _processedFile.TryGetValue(chunkId, out var lastFile) &&
            string.Equals(lastFile, newestFile, StringComparison.OrdinalIgnoreCase)) {
            _lastUsed[chunkId] = DateTime.UtcNow;
            return OperationResult.SuccessFalse;
        }

        if (!IO.DirectoryExists(folder)) {
            // Ohne Ordner gibt es nichts zu laden — und es wird auch keiner angelegt:
            // Das Speichern erzeugt Ordner nur für Chunks mit Inhalt; so bleiben
            // Phantom-Ordner für unbekannte Chunk-IDs aus.
            // _lastUsed aktualisieren, damit nicht-existierende Chunks in Refresh
            // nicht permanent übersprungen werden, sondern nach SkipIfUnusedMinutes neu geprüft werden.
            _lastUsed[chunkId] = DateTime.UtcNow;
            Develop.Diagnose("ChunkLoad", $"{chunkId} ohne-ordner");
            return OperationResult.SuccessFalse;
        }

        if (newestFile is null) {
            // Alle Dateien des Chunks wurden gelöscht — Tracking-Eintrag entfernen,
            // damit veraltete Daten nicht als "aktuell" gelten.
            _processedFile.TryRemove(chunkId, out _);
            _lastUsed[chunkId] = DateTime.UtcNow;
            Develop.Diagnose("ChunkLoad", $"{chunkId} ohne-datei");
            return OperationResult.SuccessFalse;
        }

        // Raw bytes direkt von der Festplatte lesen
        return LoadChunkFile(chunkId, newestFile);
    }

    /// <summary>
    /// Liest und parst die neueste Datei eines Chunks zwangsweise — ohne die
    /// "Bereits aktuell"-Abkürzung. Das zeilen-getriggerte Laden während des
    /// Master-Parsens wendet Chunks vorab an; das Voll-Laden (EnsureDataLoaded)
    /// muss jeden Chunk zwangsweise in kanonischer Reihenfolge anwenden, damit
    /// Altstände in Zeilen-Kopien stets vom neuesten Stand überholt werden.
    /// </summary>
    private OperationResult LoadChunkForced(string chunkId) {
        var folder = GetChunkFolder(chunkId);

        if (!IO.DirectoryExists(folder)) { return OperationResult.SuccessFalse; }

        if (GetChunkFilesOrderedByTime(folder).FirstOrDefault() is not { } newestFile) { return OperationResult.SuccessFalse; }

        return LoadChunkFile(chunkId, newestFile);
    }

    /// <summary>
    /// Speicherzeit der Chunk-Datei, die gerade geparst wird bzw. zuletzt geparst
    /// wurde. Das Angleichen (UpdateChunk-Aufrufer mit Zugriff auf die Verwaltung)
    /// nutzt sie als Stichtag: Zeilen, die jünger als die Datei sind, darf der
    /// Datei-Bestand nicht entfernen — ihr Fehlen in der Datei ist erwartet.
    /// </summary>
    public DateTime CurrentChunkFileTimeUtc { get; private set; } = DateTime.MinValue;

    /// <summary>
    /// Liest die neueste Chunk-Datei, parst ihren Inhalt in den Owner und
    /// aktualisiert das Tracking (verarbeitete Datei, Hashes, alte Dateien).
    /// </summary>
    private OperationResult LoadChunkFile(string chunkId, string newestFile) {
        var folder = GetChunkFolder(chunkId);
        var chunkFiles = GetChunkFilesOrderedByTime(folder).ToArray();

        CurrentChunkFileTimeUtc = ExtractDateFromFileName(newestFile) ?? DateTime.MinValue;

        Develop.Message(ErrorType.Info, this, KeyName, ImageCode.Tabelle, $"Lade Chunk '{chunkId}' von '{KeyName}'", 1);

        if (IO.ReadAllBytes(newestFile, 5).Value is not byte[] rawBytes || rawBytes.Length == 0) {
            return OperationResult.Failed($"Chunk '{chunkId}' konnte nicht gelesen werden");
        }

        // Für Parsing entpacken (Chunk-Dateien werden gezippt gespeichert)
        byte[] chunkContent;
        if (rawBytes.IsZipped()) {
            chunkContent = rawBytes.UnzipIt() ?? [];
            if (chunkContent.Length == 0) {
                return OperationResult.Failed($"Chunk '{chunkId}' konnte nicht entpackt werden");
            }
        } else {
            chunkContent = rawBytes;
        }

        var isMain = StartupChunkIds.Contains(chunkId, StringComparer.OrdinalIgnoreCase);

        // Während des Einspielens fremder Inhalte keine Undo-Einträge verbuchen
        PauseDataReload();
        try {
            // Startup-Chunks enthalten vollständige Hauptinhalte und werden
            // über den Inhalts-Seam (ParseMainContent) eingelesen
            if (isMain) {
                using var ms = new MemoryStream(chunkContent);

                if (!ParseMainContent(ms)) { return OperationResult.Failed($"Chunk '{chunkId}' Parsen fehlgeschlagen"); }
            } else {
                // Teil-Bestand: Datei-Stempel überspringen; der Inhalt wird über den
                // Standard-Deserialize des Owners eingelesen — dessen Klassen-Switch
                // entscheidet, welche Bereiche (Zeilen, Variablen, …) ihn übernehmen.
                using var ms = new MemoryStream(chunkContent);

                var start = 0L;
                while (FragmentRecord.TryReadNext(ms) is { IsFileTag: true }) { start = ms.Position; }

                ms.Position = start;

                if (!Owner.Deserialize(ms, 0)) { return OperationResult.Failed($"Chunk '{chunkId}' Parsen fehlgeschlagen"); }
            }
        } finally {
            ResumeDataReload();
        }

        // Chunk nach dem Parsen verwerfen - nur Dateiname und Hash tracken
        _processedFile[chunkId] = newestFile;
        _lastUsed[chunkId] = DateTime.UtcNow;
        _lastContentHash[chunkId] = Generic.GetSHA256HashString(ChunkCompareBasis(chunkContent));

        CleanupOldFilesInFolder(chunkFiles);

        Develop.Diagnose("ChunkLoad", $"{chunkId} geladen {newestFile.FileNameWithSuffix()} aus {chunkFiles.Length} Dateien");
        return new OperationResult(true);
    }

    /// <summary>
    /// Aktualisiert alle bekannten Daten-Chunks (Einträge in _processedFile):
    /// Prüft für jeden, ob eine neuere Datei im Ordner existiert und lädt
    /// diese ggf. neu. Neue Chunk-Ordner anderer Benutzer werden hier nicht
    /// entdeckt — das übernimmt GetAvailableChunkIds via LoadRows.
    /// Chunks, deren LastUsed-Stempel älter als SkipIfUnusedMinutes ist,
    /// werden übersprungen, sofern <paramref name="firstTime"/> false ist.
    /// Da Chunks write-once sind, reicht der Dateiname-Vergleich.
    /// </summary>
    private bool RefreshLoadedChunks(bool firstTime) {
        if (string.IsNullOrEmpty(Filename)) { return false; }

        var loaded = false;

        // Gezielter Zugriff pro bekanntem Chunk (alle im Speicher befindlichen
        // Chunk-IDs aus _processedFile). Die neueste Datei wird direkt mit der
        // zuletzt verarbeiteten verglichen.
        foreach (var chunkId in _processedFile.Keys.ToList()) {
            // SkipIfUnusedMinutes-Fenster: Niemand schaut den Chunk mehr an — kein Refresh nötig.
            if (!firstTime && _lastUsed.TryGetValue(chunkId, out var lastUsed)
                && DateTime.UtcNow.Subtract(lastUsed).TotalMinutes >= SkipIfUnusedMinutes) { continue; }

            var folder = GetChunkFolder(chunkId);
            if (!IO.DirectoryExists(folder)) { continue; }

            // Neueste Datei ermitteln und mit der zuletzt verarbeiteten vergleichen.
            if (GetChunkFilesOrderedByTime(folder).FirstOrDefault() is not { } newestFile) { continue; }

            var processedFile = _processedFile[chunkId];

            // Reload wenn eine andere Datei die neueste ist. Da Chunks write-once
            // sind, bedeutet ein anderer Dateiname zwingend anderen Inhalt.
            if (string.Equals(processedFile, newestFile, StringComparison.OrdinalIgnoreCase)) { continue; }

            var result = LoadChunkWithChunkId(chunkId, false);
            if (result.IsFailed) {
                Develop.DebugPrint(ErrorType.Warning, $"Chunk '{chunkId}' Refresh fehlgeschlagen: {result.FailedReason}");
            }
            loaded = loaded || result.Value is true;
        }

        return loaded;
    }


    #endregion
}