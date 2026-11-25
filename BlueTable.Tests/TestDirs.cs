// Licensed under MIT; see License.md for disclaimer, details, and extended user conditions.

using System.Globalization;
using System.IO;

namespace BlueTable.Tests;

/// <summary>
/// Verwaltet die Test-Ordner unter Windows-Temp; je Lauf und Layout ein eigener Unterordner.
/// </summary>
public static class TestDirs {

    #region Methods

    /// <summary>
    /// Legt einen neuen, eindeutigen Test-Ordner unter Windows-Temp an.
    /// </summary>
    public static string NewLayoutFolder(string layoutName) {
        var root = Path.Combine(Path.GetTempPath(), "BlueTableRoundtripTests");
        _ = Directory.CreateDirectory(root);

        var dir = Path.Combine(root, layoutName + "_" + DateTime.UtcNow.ToString("yyyy-MM-dd_HH-mm-ss-fff", CultureInfo.InvariantCulture));
        _ = Directory.CreateDirectory(dir);
        return dir;
    }

    /// <summary>
    /// True, wenn die Test-Ordner behalten werden sollen (Umgebungsvariable RTTEST_KEEP gesetzt).
    /// </summary>
    public static bool KeepRequested() {
        var value = Environment.GetEnvironmentVariable("RTTEST_KEEP");
        return value is { Length: > 0 };
    }

    /// <summary>
    /// Löscht den Test-Ordner; ein Fehlschlag wird ignoriert, weil Temp-Daten den Test nicht scheitern lassen dürfen.
    /// </summary>
    public static void TryDeleteFolder(string dir) {
        try {
            _ = IO.DeleteDir(dir, false);
        } catch {
            // Temp-Daten: Das Löschen darf den Test nicht scheitern lassen.
        }
    }

    #endregion
}
