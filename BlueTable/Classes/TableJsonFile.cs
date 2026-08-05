// Licensed under AGPL-3.0; see License.md for disclaimer and details.

using System.ComponentModel;
using System.Text;
using System.Text.Json;

namespace BlueTable.Classes;

/// <summary>
/// JSON-basierte Variante von <see cref="TableFile" />. Lädt und speichert das
/// Hauptfile komplett als JSON über <see cref="Table.ParseableJson()" /> bzw.
/// <see cref="Table.ParseJson(JsonObject)" />. Im Gegensatz zu <see cref="TableFile" />
/// wird KEIN binäres Chunk-Format verwendet, dementsprechend ist auch kein
/// Multi-User-Zugriff möglich (<see cref="TableFile.MultiUserPossible" /> = false).
/// Für Multi-User-Betrieb <see cref="TableJsonFragments" /> verwenden.
/// </summary>
[EditorBrowsable(EditorBrowsableState.Never)]
public class TableJsonFile : TableFile {

    #region Constructors

    public TableJsonFile(string tablename) : base(tablename) { }

    public TableJsonFile(string filename, Table? source) : base(filename, source) { }

    #endregion

    #region Properties

    /// <summary>
    /// Bei JSON-Tabellen gibt es kein Chunk-System. Die Aktualität wird direkt
    /// über die FileInfo der Hauptdatei ermittelt.
    /// </summary>
    public override DateTime LastSaveMainFileUtcDate {
        get {
            if (string.IsNullOrEmpty(Filename)) { return new DateTime(2000, 1, 1, 0, 0, 0, DateTimeKind.Utc); }

            var fi = IO.GetFileInfo(Filename);
            return fi is { Exists: true } ? fi.LastWriteTimeUtc : new DateTime(2000, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        }
    }

    /// <summary>
    /// Ohne Chunk-System: Eine JSON-Datei gilt als "recently used", wenn sie
    /// existiert - die feingranulare LastUsed-Logik der Chunk-Klasse entfällt.
    /// </summary>
    public override bool IsRecentlyUsed => !string.IsNullOrEmpty(Filename) && IO.FileExists(Filename);

    #endregion

    #region Methods

    /// <summary>
    /// Lädt die Hauptdatei als JSON und übernimmt den Zustand über
    /// <see cref="Table.ParseJson(JsonObject)" />. Vor dem ersten Speichern
    /// (<see cref="TableFile.InitialSavePending" />) oder bei fehlender Datei
    /// wird kein Ladeversuch unternommen.
    /// </summary>
    protected override bool LoadMainData() {
        if (InitialSavePending) { return true; }

        if (!IO.FileExists(Filename)) { return true; }

        try {
            var json = IO.ReadAllText(Filename, Encoding.UTF8);
            if (string.IsNullOrWhiteSpace(json)) {
                Freeze("JSON-Datei ist leer");
                return false;
            }

            using var doc = JsonDocument.Parse(json);
            this.ParseJson(doc.RootElement);
        } catch (Exception ex) {
            Freeze("JSON-Ladefehler: " + ex.Message);
            return false;
        }

        return true;
    }

    /// <summary>
    /// Speichert die komplette Tabelle als formatiertes JSON. Verwendet
    /// <see cref="IO.SaveExtended" /> für Backup-Rotation (analog zu
    /// <see cref="TableFile.SaveFullFile" />, aber ohne Zip/EOF-Marker).
    /// </summary>
    protected override string SaveInternal() {
        var f = IsGenericEditable(false);
        if (!string.IsNullOrEmpty(f)) { return f; }

        try {
            var json = ParseableJson().ToJsonString(new JsonSerializerOptions { WriteIndented = true });
            var bytes = json.UTF8_ToByte();

            if (IO.CreateDirectory(Filename.FilePath()).IsFailed) {
                return "Verzeichnis konnte nicht erstellt werden.";
            }

            var result = IO.SaveExtended(Filename, bytes);
            if (result.IsFailed) { return result.FailedReason; }

            InitialSavePending = false;
            SaveRequired = false;
            OnInvalidateView();
            return string.Empty;
        } catch (Exception ex) {
            return ex.Message;
        }
    }

    #endregion
}
