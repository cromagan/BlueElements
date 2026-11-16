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
            if (file.Contains("\\obj\\", StringComparison.OrdinalIgnoreCase) || file.Contains("\\bin\\", StringComparison.OrdinalIgnoreCase)) { continue; }
            result.Add(file);
        }

        return result;
    }

    /// <summary>
    /// Liefert true, wenn die Datei gültiges UTF-8 enthält (mit oder ohne BOM).
    /// </summary>
    public static bool IsUtf8(string file) => IsValidUtf8(File.ReadAllBytes(file));

    /// <summary>
    /// Liefert true, wenn die Bytes gültiges UTF-8 sind; der UTF-8-BOM ist dabei erlaubt.
    /// NUL-Bytes (z. B. UTF-16 ohne BOM oder Binärdaten) führen zu false, damit solche Dateien nicht als Text umgeschrieben werden.
    /// </summary>
    public static bool IsValidUtf8(byte[] bytes) {
        if (bytes.AsSpan().IndexOf((byte)0) >= 0) { return false; }

        var i = 0;

        while (i < bytes.Length) {
            var b = bytes[i];

            if (b <= 0x7F) {
                i++;
            } else if (b is >= 0xC2 and <= 0xDF && i + 1 < bytes.Length && bytes[i + 1] is >= 0x80 and <= 0xBF) {
                i += 2;
            } else if (b is >= 0xE0 and <= 0xEF && i + 2 < bytes.Length && bytes[i + 1] is >= 0x80 and <= 0xBF && bytes[i + 2] is >= 0x80 and <= 0xBF) {
                i += 3;
            } else if (b is >= 0xF0 and <= 0xF4 && i + 3 < bytes.Length && bytes[i + 1] is >= 0x80 and <= 0xBF && bytes[i + 2] is >= 0x80 and <= 0xBF && bytes[i + 3] is >= 0x80 and <= 0xBF) {
                i += 4;
            } else {
                return false;
            }
        }

        return true;
    }

    #endregion
}
