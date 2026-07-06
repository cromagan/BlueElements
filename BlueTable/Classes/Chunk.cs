// Licensed under AGPL-3.0; see License.md for disclaimer and details.

using System.Collections.Concurrent;
using System.ComponentModel;
using System.IO;
using System.Threading;
using static BlueBasics.ClassesStatic.IO;

namespace BlueTable.Classes;

[EditorBrowsable(EditorBrowsableState.Never)]
public class Chunk : IDisposableExtended, IHasKeyName, IReadableText {

    #region Fields

    /// <summary>
    /// Zeitraum in Minuten, nach dem ein ungenutzter Chunk bei BeSureUpToDate übersprungen wird.
    /// Wird der Chunk wieder benötigt, laden die Daten automatisch neu.
    /// </summary>
    public const int SkipIfUnusedMinutes = 5;

    /// <summary>
    /// Eigenes Register aller lebenden Chunk-Instanzen, geordnet nach
    /// normalisiertem Dateinamen.
    /// </summary>
    public static readonly ConcurrentDictionary<string, Chunk> LiveInstances = new(StringComparer.OrdinalIgnoreCase);

    /// <summary>Synchronisierungsobjekt für thread-sichere Zugriffe auf _fileInfo und LoadFailed.</summary>
    private readonly object _lock = new();

    private volatile int _isDisposedFlag;

    #endregion

    #region Constructors

    /// <summary>
    /// Konstruktor für die direkte Erstellung (z.B. ChunkInsight) oder Factory-Erstellung
    /// über <see cref="Get(string)"/>. Leitet MainFileName und ChunkId aus dem vollständigen Dateipfad ab.
    /// </summary>
    public Chunk(string fullPath) {
        Filename = string.IsNullOrEmpty(fullPath) ? string.Empty : fullPath.NormalizeFile();
        LiveInstances[Filename] = this;

        var suffix = Filename.FileSuffix().ToLowerInvariant();

        if (suffix == "hbdb") {
            // .hbdb ist eine Begleitdatei zur .csv-Datei im gleichen Verzeichnis
            MainFileName = Filename.FilePath() + Filename.FileNameWithoutSuffix() + ".csv";
        } else {
            // .bdb/.mbdb/.tblh sind Hauptdateien — MainFileName ist die Datei selbst
            MainFileName = Filename;
        }

        KeyName = string.Equals(Filename, MainFileName, StringComparison.OrdinalIgnoreCase)
            ? TableFile.Chunk_MainData.ToLowerInvariant()
            : Filename.FileNameWithoutSuffix().ToLowerInvariant();
    }

    #endregion

    #region Properties

    public FileInfo? FileInfo {
        get {
            if (field is null) {
                field = GetFileInfo(Filename);
            }

            return field;
        }

        private set;
    }

    /// <summary>Der vollständige Dateipfad dieser Chunk-Datei.</summary>
    public string Filename { get; } = string.Empty;

    /// <summary>
    /// Der FreezedReason kann niemals wieder rückgängig gemacht werden.
    /// Um den FreezedReason zu setzen, die Methode <see cref="Freeze"/> benutzen.
    /// </summary>
    public string FreezedReason { get; private set; } = string.Empty;

    public bool IsDisposed => _isDisposedFlag == 1;

    public bool IsFreezed => !string.IsNullOrEmpty(FreezedReason);

    /// <summary>
    /// Gibt die Chunk-ID LOWERCASE zurück (z. B. "maindata", "variables", Hash-Wert).
    /// Für Hauptdateien (.bdb, .mbdb, .tblh) wird Chunk_MainData zurückgegeben,
    /// für Begleit- und Chunk-Dateien (z. B. .hbdb, .tblc) der Dateiname ohne Suffix.
    /// Wird im Konstruktor einmalig berechnet, da Filename und MainFileName unveränderlich sind.
    /// </summary>
    public string KeyName { get; }

    /// <summary>
    /// UTC-Zeitpunkt der Konstruktion dieser Instanz. Wird von <see cref="IsChunkRecentlyUsed"/>
    /// ausgewertet, um kürzlich erzeugte Chunks beim Update zu bevorzugen.
    /// </summary>
    public DateTime LastUsed { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Gibt an, ob das Laden der Datei fehlgeschlagen ist.
    /// Wird auch gesetzt, wenn der geladene Inhalt kleiner als MinimumBytes ist.
    /// </summary>
    public bool LoadFailed { get; private set; }

    public string MainFileName { get; } = string.Empty;

    /// <summary>
    /// Mindestgröße des Inhalts in Bytes.
    /// IsSaveAbleNow und der Ladevorgang prüfen, ob der Inhalt diese Grenze erfüllt.
    /// Wird nach erfolgreichem Laden/Speichern über <see cref="SetMinLen"/> gesetzt.
    /// </summary>
    public int MinimumBytes { get; private set; }

    #endregion

    #region Methods

    /// <summary>
    /// Disposed alle lebenden Chunk-Instanzen. Wird beim Herunterfahren der Anwendung aufgerufen.
    /// </summary>
    public static void DisposeAll() {
        foreach (var chunk in LiveInstances.Values) {
            try { chunk.Dispose(); } catch { }
        }
    }

    /// <summary>
    /// Holt einen bestehenden oder erstellt einen neuen <see cref="Chunk"/> für den
    /// angegebenen Dateinamen. Nutzt das eigene <see cref="LiveInstances"/>-Register.
    /// Gibt <c>null</c> zurück, wenn die Datei nicht existiert.
    /// </summary>
    public static Chunk? Get(string filename) {
        var normalizedFileName = filename.NormalizeFile();

        if (!FileExists(normalizedFileName)) { return null; }

        // Bestehende lebende Instanz zurückgeben
        if (LiveInstances.TryGetValue(normalizedFileName, out var existing)) {
            if (existing.IsDisposed) {
                LiveInstances.TryRemove(normalizedFileName, out _);
            } else {
                return existing;
            }
        }

        // Neue Instanz erzeugen. Der Konstruktor registriert in LiveInstances.
        var created = new Chunk(normalizedFileName);

        // Race-Schutz: falls ein anderer Thread gleichzeitig konstruiert hat,
        // gewinnt der zuerst eingetragene. Die eigene Instanz wird verworfen.
        var winner = LiveInstances.GetOrAdd(normalizedFileName, created);
        if (!ReferenceEquals(winner, created)) {
            created.Dispose();
        }
        return winner;
    }

    /// <summary>
    /// Prüft, ob der Content den erwarteten CheckPoint enthält.
    /// System-Chunks (_maindata, _master, _vars, _uses, _rowdata) suchen nach ~^{KeyName}^~.
    /// Row-Chunks (Hash-basiert) geben true zurück.
    /// </summary>
    public static bool HasCheckPoint(byte[] content, string keyName) {
        if (content.Length < 12) { return false; }
        if (IsRowChunk(keyName)) { return true; }

        var searchText = $"~^{keyName.ToLowerInvariant()}^~".UTF8_ToByte();
        return content.AsSpan().IndexOf(searchText) >= 0;
    }

    /// <summary>
    /// Prüft, ob der Chunk mit der angegebenen Datei innerhalb von <see cref="SkipIfUnusedMinutes"/>
    /// erzeugt wurde. Wird in BeSureUpToDate aufgerufen, um Speicherzugriffe
    /// auf ungenutzte Chunks zu vermeiden.
    /// </summary>
    public static bool IsChunkRecentlyUsed(string filename) {
        var chunk = Get(filename);
        if (chunk is null) { return false; }
        return DateTime.UtcNow.Subtract(chunk.LastUsed).TotalMinutes < SkipIfUnusedMinutes;
    }

    /// <summary>
    /// Bestimmt, ob ein Chunk-Key ein Row-Chunk ist (Hash-basiert oder _rowdata).
    /// </summary>
    public static bool IsRowChunk(string keyName) =>
        !string.Equals(keyName, TableFile.Chunk_MainData, StringComparison.OrdinalIgnoreCase) &&
        !string.Equals(keyName, TableChunk.Chunk_MainDataLite, StringComparison.OrdinalIgnoreCase) &&
        !string.Equals(keyName, TableChunk.Chunk_Master, StringComparison.OrdinalIgnoreCase) &&
        !string.Equals(keyName, TableChunk.Chunk_Variables, StringComparison.OrdinalIgnoreCase) &&
        !string.Equals(keyName, TableChunk.Chunk_AdditionalUseCases, StringComparison.OrdinalIgnoreCase) &&
        !string.Equals(keyName, TableChunk.Chunk_UnknownData, StringComparison.OrdinalIgnoreCase);

    public static void SaveToByteList(List<byte> bytes, TableDataType tableDataType, string content) {
        var b = content.UTF8_ToByte();
        bytes.Add((byte)Routinen.DatenAllgemeinUTF8);
        bytes.Add((byte)tableDataType);
        SaveToByteList(bytes, b.Length, 3);
        bytes.AddRange(b);
    }

    public static void SaveToByteList(List<byte> bytes, long numberToAdd, int byteCount) {
        do {
            byteCount--;
            var divisor = (long)Math.Pow(255, byteCount);
            var digit = numberToAdd / divisor;

            if (digit > 255) { Develop.DebugError($"SaveToByteList overflow: {numberToAdd} passt nicht in {byteCount + 1} Byte(s), Max={divisor * 256 - 1}"); return; }

            bytes.Add((byte)digit);
            numberToAdd %= divisor;
        } while (byteCount > 0);
    }

    public static void SaveToByteList(List<byte> bytes, ColumnItem column, RowItem row) {
        if (column.Table is not { IsDisposed: false }) { return; }

        var cellContent = row.CellGetStringCore(column);
        if (string.IsNullOrEmpty(cellContent)) { return; }

        bytes.Add((byte)Routinen.CellFormatUTF8_V403);

        var columnKeyByte = column.KeyName.UTF8_ToByte();
        SaveToByteList(bytes, columnKeyByte.Length, 1);
        bytes.AddRange(columnKeyByte);

        var rowKeyByte = row.KeyName.UTF8_ToByte();
        SaveToByteList(bytes, rowKeyByte.Length, 1);
        bytes.AddRange(rowKeyByte);

        var cellContentByte = cellContent.UTF8_ToByte();
        SaveToByteList(bytes, cellContentByte.Length, 2);
        bytes.AddRange(cellContentByte);
    }

    public static void SaveToByteList(List<byte> bytes, TableDataType tableDataType, string content, string columnKey) {
        bytes.Add((byte)Routinen.ColumnUTF8_V401);
        bytes.Add((byte)tableDataType);

        var n = columnKey.UTF8_ToByte();
        SaveToByteList(bytes, n.Length, 1);
        bytes.AddRange(n);

        var b = content.UTF8_ToByte();
        SaveToByteList(bytes, b.Length, 3);
        bytes.AddRange(b);
    }

    /// <summary>Alle Spaltendaten außer Systeminfo</summary>
    public static void SaveToByteList(List<byte> bytes, ColumnItem c) {
        var name = c.KeyName;

        SaveToByteList(bytes, TableDataType.ColumnKey, c.KeyName, name);
        SaveToByteList(bytes, TableDataType.IsFirst, c.IsFirst.ToPlusMinus(), name);
        SaveToByteList(bytes, TableDataType.IsKeyColumn, c.IsKeyColumn.ToPlusMinus(), name);
        SaveToByteList(bytes, TableDataType.ColumnCaption, c.Caption, name);
        SaveToByteList(bytes, TableDataType.DefaultRenderer, c.DefaultRenderer, name);
        SaveToByteList(bytes, TableDataType.RendererSettings, c.RendererSettings, name);
        SaveToByteList(bytes, TableDataType.CaptionGroup1, c.CaptionGroup1, name);
        SaveToByteList(bytes, TableDataType.CaptionGroup2, c.CaptionGroup2, name);
        SaveToByteList(bytes, TableDataType.CaptionGroup3, c.CaptionGroup3, name);
        SaveToByteList(bytes, TableDataType.MultiLine, c.MultiLine.ToPlusMinus(), name);
        SaveToByteList(bytes, TableDataType.SortAndRemoveDoubleAfterEdit, c.AfterEditQuickSortRemoveDouble.ToPlusMinus(), name);
        SaveToByteList(bytes, TableDataType.DoUcaseAfterEdit, c.AfterEditDoUCase.ToPlusMinus(), name);
        SaveToByteList(bytes, TableDataType.AutoCorrectAfterEdit, c.AfterEditAutoCorrect.ToPlusMinus(), name);
        SaveToByteList(bytes, TableDataType.AfterEditRound, c.AfterEditRound.ToString1(), name);
        SaveToByteList(bytes, TableDataType.MaxCellLength, c.MaxCellLength.ToString1(), name);
        SaveToByteList(bytes, TableDataType.FixedColumnWidth, c.FixedColumnWidth.ToString1(), name);
        SaveToByteList(bytes, TableDataType.AfterEditAutoRemoveChar, c.AfterEditAutoRemoveChar, name);
        SaveToByteList(bytes, TableDataType.SaveContent, c.SaveContent.ToPlusMinus(), name);
        SaveToByteList(bytes, TableDataType.FilterOptions, ((int)c.FilterOptions).ToString1(), name);
        SaveToByteList(bytes, TableDataType.AutoFilterJoker, c.AutoFilterJoker, name);
        SaveToByteList(bytes, TableDataType.IgnoreAtRowFilter, c.IgnoreAtRowFilter.ToPlusMinus(), name);
        SaveToByteList(bytes, TableDataType.EditableWithTextInput, c.EditableWithTextInput.ToPlusMinus(), name);
        SaveToByteList(bytes, TableDataType.SpellCheckingEnabled, c.SpellCheckingEnabled.ToPlusMinus(), name);
        SaveToByteList(bytes, TableDataType.Relationship_to_First, c.Relationship_to_First.ToPlusMinus(), name);
        SaveToByteList(bytes, TableDataType.TextFormatingAllowed, c.TextFormatingAllowed.ToPlusMinus(), name);
        SaveToByteList(bytes, TableDataType.ForeColor, c.ForeColor.ToArgb().ToString1(), name);
        SaveToByteList(bytes, TableDataType.BackColor, c.BackColor.ToArgb().ToString1(), name);
        SaveToByteList(bytes, TableDataType.LineStyleLeft, ((int)c.LineStyleLeft).ToString1(), name);
        SaveToByteList(bytes, TableDataType.LineStyleRight, ((int)c.LineStyleRight).ToString1(), name);
        SaveToByteList(bytes, TableDataType.BackgroundStyle, ((long)c.BackgroundStyle).ToString1(), name);
        SaveToByteList(bytes, TableDataType.RelationType, ((int)c.RelationType).ToString1(), name);
        SaveToByteList(bytes, TableDataType.Value_for_Chunk, ((int)c.Value_for_Chunk).ToString1(), name);
        SaveToByteList(bytes, TableDataType.EditableWithDropdown, c.EditableWithDropdown.ToPlusMinus(), name);
        SaveToByteList(bytes, TableDataType.DropDownItems, string.Join('\r', c.DropDownItems), name);
        SaveToByteList(bytes, TableDataType.LinkedCellFilter, string.Join('\r', c.LinkedCellFilter), name);
        SaveToByteList(bytes, TableDataType.AutoReplaceAfterEdit, string.Join('\r', c.AfterEditAutoReplace), name);
        SaveToByteList(bytes, TableDataType.RegexCheck, c.RegexCheck, name);
        SaveToByteList(bytes, TableDataType.ValueRequired, c.ValueRequired.ToPlusMinus(), name);
        SaveToByteList(bytes, TableDataType.ShowValuesOfOtherCellsInDropdown, c.ShowValuesOfOtherCellsInDropdown.ToPlusMinus(), name);
        SaveToByteList(bytes, TableDataType.ColumnQuickInfo, c.QuickInfo, name);
        SaveToByteList(bytes, TableDataType.ColumnAdminInfo, c.AdminInfo, name);
        SaveToByteList(bytes, TableDataType.CaptionBitmapCode, c.CaptionBitmapCode, name);
        SaveToByteList(bytes, TableDataType.AllowedChars, c.AllowedChars, name);
        SaveToByteList(bytes, TableDataType.MaxTextLength, c.MaxTextLength.ToString1(), name);
        SaveToByteList(bytes, TableDataType.PermissionGroupsChangeCell, string.Join('\r', c.PermissionGroupsChangeCell), name);
        SaveToByteList(bytes, TableDataType.ColumnTags, string.Join('\r', c.ColumnTags), name);
        SaveToByteList(bytes, TableDataType.EditAllowedDespiteLock, c.EditAllowedDespiteLock.ToPlusMinus(), name);
        SaveToByteList(bytes, TableDataType.LinkedTableTableName, c.LinkedTableTableName, name);
        SaveToByteList(bytes, TableDataType.DoOpticalTranslation, ((int)c.DoOpticalTranslation).ToString1(), name);
        SaveToByteList(bytes, TableDataType.AdditionalFormatCheck, ((int)c.AdditionalFormatCheck).ToString1(), name);
        SaveToByteList(bytes, TableDataType.ScriptType, ((int)c.ScriptType).ToString1(), name);
        SaveToByteList(bytes, TableDataType.ColumnKeyOfLinkedTable, c.ColumnKeyOfLinkedTable, name);
        SaveToByteList(bytes, TableDataType.ColumnAlign, ((int)c.Align).ToString1(), name);
        SaveToByteList(bytes, TableDataType.SortType, ((int)c.SortType).ToString1(), name);
    }

    public static void SaveToByteList(List<byte> bytes, RowItem thisRow) {
        if (thisRow.Table is not { IsDisposed: false } tb) { return; }

        foreach (var thisColumn in tb.Column) {
            if (thisColumn.SaveContent) {
                SaveToByteList(bytes, thisColumn, thisRow);
            }
        }
    }

    public string ChunkFolder() {
        var folder = MainFileName.FilePath();
        var tablename = MainFileName.FileNameWithoutSuffix();

        return $"{folder}{tablename}\\";
    }

    /// <summary>
    /// Disposed die Instanz und trägt sie aus dem <see cref="LiveInstances"/>-Register aus.
    /// Nur austragen, wenn noch unsere Instanz hinterlegt ist. Bei Konstruktions-Races
    /// (zwei Instanzen für dieselbe Datei) würde sonst der Eintrag des Gewinners gelöscht.
    /// </summary>
    public void Dispose() {
        if (Interlocked.CompareExchange(ref _isDisposedFlag, 1, 0) != 0) { return; }

        LiveInstances.TryRemove(new KeyValuePair<string, Chunk>(Filename, this));

        Invalidate();

        GC.SuppressFinalize(this);
    }

    /// <summary>Friert die Datei ein. Kann nicht rückgängig gemacht werden.</summary>
    public void Freeze(string reason) {
        FreezedReason = string.IsNullOrEmpty(reason) ? "Eingefroren" : reason;
    }

    /// <summary>
    /// Setzt das gecachte FileInfo zurück, damit beim nächsten Zugriff die
    /// Metadaten frisch vom Dateisystem gelesen werden.
    /// </summary>
    public void Invalidate() {
        lock (_lock) {
            FileInfo = null;
        }
    }

    public string IsNowEditable() {
        if (IsDisposed) { return "Verworfen."; }
        if (IsFreezed) { return FreezedReason; }
        if (LoadFailed) { return "Datei wurde nicht korrekt geladen."; }

        return CanWriteFile(Filename, 2).FailedReason;
    }

    /// <summary>
    /// Prüft, ob Speichern aktuell erlaubt ist.
    /// Berücksichtigt: IsFreezed, IsDisposed, LoadFailed, MinimumBytes,
    /// gültigen EOF-Marker und CheckPoint.
    /// </summary>
    public string IsSaveAbleNow(byte[] content) {
        if (IsNowEditable() is { Length: > 0 } f) { return f; }

        if (content.Length < MinimumBytes) { return "Byte-Fehler"; }

        if (!TableFile.HasValidEofMarker(content)) {
            return "Kein gültiger EOF-Marker";
        }

        if (!HasCheckPoint(content, KeyName)) {
            return $"Chunk '{KeyName}' enthält keinen gültigen CheckPoint";
        }

        return string.Empty;
    }

    /// <summary>
    /// Liest den logischen Dateiinhalt frisch vom Dateisystem (auf Disk gezippt,
    /// hier entpackt). Der Inhalt wird NICHT im Speicher gehalten.
    /// Aktualisiert <see cref="LoadFailed"/>, <see cref="FileInfo"/> und
    /// <see cref="MinimumBytes"/> als Seiteneffekt.
    /// Schreiben erfolgt über <see cref="TableFile.Save(byte[])"/>.
    /// </summary>
    public byte[] LoadContent() {
        if (IsDisposed) { return []; }

        var (content, fileInfo, loadFailed) = ReadContentFromFileSystem();

        if (!loadFailed && content.Length > 0) {
            if (content.IsZipped()) {
                var unzipped = content.UnzipIt();
                if (unzipped is null) {
                    loadFailed = true;
                } else {
                    content = unzipped;
                }
            }

            if (!loadFailed && MinimumBytes > 0 && content.Length < MinimumBytes) {
                loadFailed = true;
            }
        }

        if (loadFailed) { content = []; }

        lock (_lock) {
            LoadFailed = loadFailed;
            FileInfo = fileInfo ?? new FileInfo(Filename);
        }

        if (!loadFailed) { SetMinLen(content.Length); }

        return content;
    }

    public string ReadableText() => $"Chunk '{KeyName}'";

    public QuickImage? SymbolForReadableText() => QuickImage.Get(ImageCode.Puzzle, 16);

    public override string ToString() => KeyName;

    private (byte[] Content, FileInfo? FileInfo, bool LoadFailed) ReadContentFromFileSystem() {
        try {
            var retries = 0;
            do {
                var fileInfo1 = GetFileInfo(Filename, false, 0.1f);
                if (fileInfo1 is null) { return ([], null, false); }

                var content = ReadAllBytes(Filename, 20).Value as byte[] ?? [];
                var fileInfo2 = GetFileInfo(Filename, false, 2f);
                if (fileInfo2 is not null &&
                    fileInfo1.LastWriteTime == fileInfo2.LastWriteTime &&
                    fileInfo1.Length == fileInfo2.Length) { return (content, fileInfo2, false); }

                retries++;
            } while (retries < 20);

            return ([], null, true); // Datei ändert sich ständig, Laden fehlgeschlagen
        } catch {
            return ([], null, true);
        }
    }

    private void SetMinLen(int length) => MinimumBytes = (int)(length * 0.3);

    #endregion
}