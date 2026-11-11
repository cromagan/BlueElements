// Licensed under MIT; see License.md for disclaimer, details, and extended user conditions.

using System.Text.Json.Nodes;
using static BlueBasics.ClassesStatic.IO;

namespace BlueDeskSnap.Snap;

/// <summary>
/// Ablage aller Desktop-Snaps in der einen Datei %appdocumentpath%\DeskSnaps\DeskSnaps.json - die Reihenfolge im Array ist die Listenreihenfolge.
/// </summary>
public static class SnapshotStore {

    #region Fields

    private static readonly string _file = @"%appdocumentpath%\DeskSnaps\DeskSnaps.json".NormalizeFile();

    private static readonly string _rootFolder = @"%appdocumentpath%\DeskSnaps".NormalizePath();

    #endregion

    #region Methods

    /// <summary>
    /// Legt eine vollständige Kopie des Snaps unter neuem Namen ans Ende der Ablage.
    /// </summary>
    public static WindowSnapshot Copy(WindowSnapshot snapshot, string newName) {
        var copy = new WindowSnapshot {
            Id = Generic.GetUniqueKey(),
            Name = newName,
            Created = DateTime.Now
        };

        copy.Windows.AddRange(snapshot.Windows.Select(w => (WindowInfo)w.Clone()));
        copy.ActivePlugins.AddRange(snapshot.ActivePlugins);
        copy.PluginSettings.AddRange(snapshot.PluginSettings.Select(p => new QualityPluginState(p.KeyName, p.Settings?.DeepClone() as JsonObject) { Modified = p.Modified }));

        Save(copy);
        return copy;
    }

    /// <summary>
    /// Entfernt den Snap endgültig aus der Ablage.
    /// </summary>
    public static void Delete(string id) {
        var all = LoadFile();
        if (all.RemoveAll(s => string.Equals(s.Id, id, StringComparison.OrdinalIgnoreCase)) == 0) { return; }
        SaveAll(all);
    }

    public static WindowSnapshot? Load(string id) => SnapshotStore.LoadFile().FirstOrDefault(s => string.Equals(s.Id, id, StringComparison.OrdinalIgnoreCase));



    public static void Save(WindowSnapshot snapshot) {
        var all = LoadFile();
        var existing = all.FindIndex(s => string.Equals(s.Id, snapshot.Id, StringComparison.OrdinalIgnoreCase));

        if (existing >= 0) {
            all[existing] = snapshot; // Die Position in der Reihenfolge bleibt erhalten, z. B. beim Umbenennen
        } else {
            all.Add(snapshot);
        }

        SaveAll(all);
    }

    /// <summary>
    /// Speichert die Snaps in genau dieser Reihenfolge ab.
    /// </summary>
    public static void SaveOrder(List<WindowSnapshot> ordered) => SaveAll(ordered);

    /// <summary>
    /// Ersetzt die gespeicherten Einstellungen eines Plugins in allen Snaps.
    /// </summary>
    public static void UpdatePluginSettings(string pluginKeyName, JsonObject settings) {
        var all = LoadFile();

        foreach (var snap in all) {
            if (snap.PluginSettings.FirstOrDefault(s => string.Equals(s.KeyName, pluginKeyName, StringComparison.OrdinalIgnoreCase)) is { } existing) {
                existing.Settings = (JsonObject)settings.DeepClone();
                existing.Modified = DateTime.Now;
            } else {
                snap.PluginSettings.Add(new QualityPluginState(pluginKeyName, settings));
            }
        }

        SaveAll(all);
    }

    public static List<WindowSnapshot> LoadFile() {
        List<WindowSnapshot> result = [];

        if (JsonNode.Parse(ReadAllText(_file)) is not JsonArray array) { return result; }

        foreach (var element in array) {
            if (element is JsonObject obj && ParseableItem.NewByParsingJson<WindowSnapshot>(obj) is { } snap) { result.Add(snap); }
        }

        return result;
    }

    private static void SaveAll(List<WindowSnapshot> snaps) {
        _ = Directory.CreateDirectory(_rootFolder);
        if (Directory.Exists(_file)) { _ = DeleteDir(_file, false); } // Ein gleichnamiger Ordner blockiert die Datei
        JsonArray array = [];
        foreach (var snap in snaps) { array.Add(snap.ParseableJson()); }
        WriteAllText(_file, array.ToJsonString(), Encoding.UTF8, false);
    }

    #endregion
}
