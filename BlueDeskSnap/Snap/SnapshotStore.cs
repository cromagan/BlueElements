// Licensed under MIT; see License.md for disclaimer, details, and extended user conditions.

using System.Globalization;
using System.Text.Json.Nodes;
using static BlueBasics.ClassesStatic.IO;

namespace BlueDeskSnap.Snap;

/// <summary>
/// Ablage der Snapshots unter %appdocumentpath%\DeskSnaps - ein Ordner pro Snapshot mit Snapshot.json.
/// </summary>
public static class SnapshotStore {

    #region Fields

    private const string SnapshotFileName = "Snapshot.json";

    private const string LegacySnapshotFileName = "Snapshot.txt";

    private static readonly string _rootFolder = @"%appdocumentpath%\DeskSnaps".NormalizePath();

    #endregion

    #region Methods

    public static void DeleteFolder(string folder) {
        if (Directory.Exists(folder)) { Directory.Delete(folder, true); }
    }

    /// <summary>
    /// Legt eine vollständige Kopie des Snapshots unter neuem Namen ab.
    /// </summary>
    public static WindowSnapshot Copy(WindowSnapshot snapshot, string newName) {
        var copy = new WindowSnapshot {
            Name = newName,
            Created = DateTime.Now,
            Folder = NewSnapshotFolder(newName)
        };

        copy.Windows.AddRange(snapshot.Windows.Select(w => (WindowInfo)w.Clone()));
        copy.ActivePlugins.AddRange(snapshot.ActivePlugins);
        copy.PluginSettings.AddRange(snapshot.PluginSettings.Select(p => new QualityPluginState(p.KeyName, p.Settings?.DeepClone() as JsonObject) { Modified = p.Modified }));

        Save(copy);
        return copy;
    }

    public static WindowSnapshot? Load(string folder) {
        var file = Path.Combine(folder, SnapshotFileName);
        if (FileExists(file)) { return ParseableItem.NewByParsingJson<WindowSnapshot>(ReadAllText(file).ParseAsJsonObject()); }

        var legacyFile = Path.Combine(folder, LegacySnapshotFileName);
        return FileExists(legacyFile) ? ParseableItem.NewByParsing<WindowSnapshot>(ReadAllText(legacyFile)) : null;
    }

    public static List<WindowSnapshot> LoadAll() {
        List<WindowSnapshot> result = [];

        if (!Directory.Exists(_rootFolder)) { return result; }

        foreach (var folder in Directory.GetDirectories(_rootFolder)) {
            if (Load(folder) is { } snap) { result.Add(snap); }
        }

        return result;
    }

    public static string NewSnapshotFolder(string name) {
        var folder = Path.Combine(_rootFolder, DateTime.Now.ToString("yyyyMMdd_HHmmss", CultureInfo.InvariantCulture) + "_" + CleanFolderName(name));
        _ = Directory.CreateDirectory(folder);
        return folder;
    }

    public static void Save(WindowSnapshot snapshot) => WriteAllText(Path.Combine(snapshot.Folder, SnapshotFileName), snapshot.ParseableJson().ToJsonString(), Encoding.UTF8, false);

    /// <summary>
    /// Ersetzt die gespeicherten Einstellungen eines Plugins in allen Snaps.
    /// </summary>
    public static void UpdatePluginSettings(string pluginKeyName, JsonObject settings) {
        foreach (var snap in LoadAll()) {
            if (snap.PluginSettings.FirstOrDefault(s => string.Equals(s.KeyName, pluginKeyName, StringComparison.OrdinalIgnoreCase)) is { } existing) {
                existing.Settings = (JsonObject)settings.DeepClone();
                existing.Modified = DateTime.Now;
            } else {
                snap.PluginSettings.Add(new QualityPluginState(pluginKeyName, settings));
            }

            Save(snap);
        }
    }

    private static string CleanFolderName(string name) {
        var invalid = Path.GetInvalidFileNameChars();
        var sb = new StringBuilder();

        foreach (var c in name) {
            sb.Append(invalid.Contains(c) ? '_' : c);
        }

        var result = sb.ToString().Trim();
        return result.Length == 0 ? "Snapshot" : result;
    }

    #endregion
}
