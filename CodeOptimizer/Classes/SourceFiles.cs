// Licensed under MIT; see License.md for disclaimer, details, and extended user conditions.

namespace CodeOptimizer.Classes;

/// <summary>
/// Liefert alle .cs-Dateien eines Ordners, ohne die obj-Ordner.
/// </summary>
public static class SourceFiles {

    #region Methods

    public static List<string> Enumerate(string rootPath) {
        if (!Directory.Exists(rootPath)) { return []; }

        var result = new List<string>();

        foreach (var file in Directory.EnumerateFiles(rootPath, "*.cs", SearchOption.AllDirectories)) {
            if (file.Contains("\\obj\\", StringComparison.OrdinalIgnoreCase)) { continue; }
            result.Add(file);
        }

        return result;
    }

    #endregion
}
