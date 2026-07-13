// Licensed under AGPL-3.0; see License.md for disclaimer and details.

using BlueControls.Classes.TableItems;

namespace BlueControls;

/// <content>
/// Chapter-Pfad-Hilfsroutinen für Tabellen (Chapter-Spalten). Als
/// Trennzeichen wird ausschließlich <see cref="RowCaptionListItem.Kapiteltrenner"/>
/// ('\') verwendet.
/// </content>
public static partial class Extensions {

    #region Methods

    /// <summary>
    /// Gibt die Tiefe des Pfads zurück (Anzahl Trenner).
    /// "A" = 0, "A\B" = 1, "A\B\C" = 2.
    /// </summary>
    public static int ChapterPathDepth(this string path) {
        if (string.IsNullOrEmpty(path)) { return 0; }

        var count = 0;
        foreach (var c in path) {
            if (c == RowCaptionListItem.Kapiteltrenner) { count++; }
        }
        return count;
    }

    /// <summary>
    /// Gibt alle Prefix-Pfade des Kapitels zurück, inkl. des Pfads selbst.
    /// Für "A\B\C" → ["A", "A\B", "A\B\C"].
    /// </summary>
    public static List<string> ChapterPathHierarchy(this string path) {
        var result = new List<string>();
        if (string.IsNullOrWhiteSpace(path)) { return result; }

        var segments = path.Split([RowCaptionListItem.Kapiteltrenner], StringSplitOptions.RemoveEmptyEntries);
        var current = string.Empty;
        foreach (var s in segments) {
            current = string.IsNullOrEmpty(current) ? s : current + RowCaptionListItem.Kapiteltrenner + s;
            result.Add(current);
        }
        return result;
    }

    /// <summary>
    /// Gibt nur das letzte Segment des Pfads zurück. Für "A\B\C" → "C".
    /// </summary>
    public static string ChapterPathLastName(this string path) {
        if (string.IsNullOrEmpty(path)) { return string.Empty; }

        var pos = path.LastIndexOf(RowCaptionListItem.Kapiteltrenner);
        return pos < 0 ? path : path[(pos + 1)..];
    }

    /// <summary>
    /// Normalisiert einen Kapitel-Pfad: führende und abschließende
    /// Trennzeichen werden entfernt. "  \A\B\  " wird zu "A\B".
    /// </summary>
    public static string ChapterPathNormalize(this string path) {
        if (string.IsNullOrEmpty(path)) { return string.Empty; }
        return path.Trim(RowCaptionListItem.Kapiteltrenner).Trim();
    }

    /// <summary>
    /// Gibt den Parent-Pfad zurück. Für "A\B" → "A". Für "A" → "".
    /// Das Ergebnis ist normalisiert (keine führenden/abschließenden Trennzeichen).
    /// </summary>
    public static string ChapterPathParent(this string path) {
        if (string.IsNullOrEmpty(path)) { return string.Empty; }

        var normalized = path.ChapterPathNormalize();
        if (string.IsNullOrEmpty(normalized)) { return string.Empty; }

        var pos = normalized.LastIndexOf(RowCaptionListItem.Kapiteltrenner);
        return pos <= 0 ? string.Empty : normalized[..pos];
    }

    #endregion
}