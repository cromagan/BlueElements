// Licensed under MIT; see License.md for disclaimer, details, and extended user conditions.

using System.Globalization;
using static BlueBasics.ClassesStatic.IO;

namespace BlueDeskSnap.Snap;

/// <summary>
/// Ablage der Snapshots unter %appdocumentpath%\DeskSnaps - ein Ordner pro Snapshot mit Snapshot.json und Screenshot.png.
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
