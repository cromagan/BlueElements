// Licensed under AGPL-3.0; see License.md for disclaimer and details.

using System.ComponentModel;
using System.Text;

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
    /// Ohne Chunk-System: Eine JSON-Datei gilt als "recently used", wenn sie
    /// existiert - die feingranulare LastUsed-Logik der Chunk-Klasse entfällt.
    /// </summary>
    public override bool IsRecentlyUsed => !string.IsNullOrEmpty(Filename) && IO.FileExists(Filename);

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

    #endregion

    #region Methods

    /// <summary>
    /// Lädt die Hauptdatei als JSON und übernimmt den Zustand über
    /// <see cref="Table.ParseJson(JsonObject)" />. Vor dem ersten Speichern
    /// (<see cref="TableFile.InitialSavePending" />) oder bei fehlender Datei
    /// wird kein Ladeversuch unternommen.
    /// </summary>
    /// <remarks>
    /// Die Collection-Implementierungen von <c>ParseJson</c>
    /// (<see cref="ColumnCollection.ParseJson(JsonObject)" /> bzw.
    /// <see cref="RowCollection.ParseJson(JsonObject)" />) legen bewusst KEINE
    /// neuen Spalten oder Zeilen an — sie sind für Partial-Updates gedacht und
    /// aktualisieren nur bereits existierende Elemente. Beim Laden einer
    /// kompletten Datei müssen die Strukturen deshalb zuerst explizit erzeugt
    /// werden (analog zum binären <see cref="TableFile.LoadMainData" />, der
    /// Spalten/Zeilen über ExecuteCommand anlegt). Erst danach darf
    /// <c>ParseJson</c> die Eigenschaften, Zellwerte und Sub-Bäume übernehmen.
    /// </remarks>
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

            // JsonObject aufbauen (Keys auf Kleinschreibung normalisiert),
            // damit es sowohl zur Struktur-Anlage als auch anschließend zum
            // Setzen aller Eigenschaften über ParseJson genutzt werden kann.
            JsonObject root = new();
            foreach (var pair in doc.RootElement.EnumerateObject()) {
                root[pair.Name.ToLowerInvariant()] = pair.Value.ToJsonNode();
            }

            // Reihenfolge: Erst Spalten anlegen, damit Zeilen deren Zellwerte
            // referenzieren können. Systemspalten werden beim ersten Add durch
            // GetSystems() automatisch erzeugt und beim späteren ParseJson
            // aktualisiert.
            if (root["columns"] is JsonArray cols) {
                foreach (var item in cols) {
                    if (item is not JsonObject jo) { continue; }
                    if (jo.GetString("key", string.Empty) is not { Length: > 0 } key) { continue; }
                    if (Column[key] is { IsDisposed: false }) { continue; }
                    var error = Column.ExecuteCommand(TableDataType.Command_AddColumnByName, key, ChangeFlags.IgnoreFreeze);
                    if (!string.IsNullOrEmpty(error)) {
                        Freeze("JSON-Ladefehler (Spalte): " + error);
                        return false;
                    }
                }
            }

            if (root["rows"] is JsonArray rows) {
                foreach (var item in rows) {
                    if (item is not JsonObject jo) { continue; }
                    if (jo.GetString("key", string.Empty) is not { Length: > 0 } key) { continue; }
                    if (Row.GetByKey(key) is { IsDisposed: false }) { continue; }
                    var result = Row.ExecuteCommand(TableDataType.Command_AddRow, key, ChangeFlags.IgnoreFreeze, null, null);
                    if (result.IsFailed) {
                        Freeze("JSON-Ladefehler (Zeile): " + result.FailedReason);
                        return false;
                    }
                }
            }

            // Jetzt, da alle Spalten und Zeilen existieren, dürfen ParseJson die
            // Eigenschaften, Zellwerte und Sub-Bäume (Cells, SortDefinition,
            // UniqueValues, EventScript, ...) übernehmen.
            this.ParseJson(root);
            this.ParseFinishedJson(root);
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