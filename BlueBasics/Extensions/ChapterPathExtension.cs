// Licensed under AGPL-3.0; see License.md for disclaimer and details.

namespace BlueBasics;

/// <content>
/// Chapter-Pfad-Hilfsroutinen für Tabellen (Chapter-Spalten). Im Gegensatz
/// zu <c>ClassesStatic.IO.PathParent</c> akzeptieren diese Routinen sowohl
/// '\' als auch '/' als Trenner — analog zum Windows Datei-Explorer.
/// </content>
public static partial class Extensions {

    #region Methods

    /// <summary>
    /// Gibt die Tiefe des Pfads zurück (Anzahl Trenner).
    /// "A" = 0, "A\B" = 1, "A\B\C" = 2. Akzeptiert beide Separatoren.
    /// </summary>
    public static int ChapterPathDepth(this string path) {
        if (string.IsNullOrEmpty(path)) { return 0; }

        var count = 0;
        foreach (var c in path) {
            if (c is '\\' or '/') { count++; }
        }
        return count;
    }

    /// <summary>
    /// Gibt alle Prefix-Pfade des Kapitels zurück, inkl. des Pfads selbst.
    /// Für "A\B\C" → ["A", "A\B", "A\B\C"]. Akzeptiert beide Separatoren.
    /// </summary>
    public static List<string> ChapterPathHierarchy(this string path) {
        var result = new List<string>();
        if (string.IsNullOrWhiteSpace(path)) { return result; }

        var segments = path.Split(['\\', '/'], StringSplitOptions.RemoveEmptyEntries);
        var current = string.Empty;
        foreach (var s in segments) {
            current = string.IsNullOrEmpty(current) ? s : current + "\\" + s;
            result.Add(current);
        }
        return result;
    }

    /// <summary>
    /// Gibt nur das letzte Segment des Pfads zurück. Für "A\B\C" → "C".
    /// </summary>
    public static string ChapterPathLastName(this string path) {
        if (string.IsNullOrEmpty(path)) { return string.Empty; }

        var pos = Math.Max(path.LastIndexOf('\\'), path.LastIndexOf('/'));
        return pos < 0 ? path : path[(pos + 1)..];
    }

    /// <summary>
    /// Normalisiert einen Kapitel-Pfad: '/' wird zu '\', führende und
    /// abschließende Separatoren werden entfernt. "A/B\" wird zu "A\B".
    /// </summary>
    public static string ChapterPathNormalize(this string path) {
        if (string.IsNullOrEmpty(path)) { return string.Empty; }
        return path.Replace('/', '\\').Trim('\\').Trim();
    }

    /// <summary>
    /// Gibt den Parent-Pfad zurück. Für "A\B" → "A". Für "A" → "".
    /// Akzeptiert beide Separatoren als Eingabe, das Ergebnis verwendet '\' und
    /// ist normalisiert (keine führenden/abschließenden Separatoren).
    /// </summary>
    public static string ChapterPathParent(this string path) {
        if (string.IsNullOrEmpty(path)) { return string.Empty; }

        var normalized = path.ChapterPathNormalize();
        if (string.IsNullOrEmpty(normalized)) { return string.Empty; }

        var pos = normalized.LastIndexOf('\\');
        return pos <= 0 ? string.Empty : normalized[..pos];
    }

    #endregion
}