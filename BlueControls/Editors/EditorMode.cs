// Licensed under MIT; see License.md for disclaimer, details, and extended user conditions.

namespace BlueControls.Editoren;

/// <summary>
/// Bestimmt das Verhalten eines EditorEasy-Controls, insbesondere bei eingebetteter Nutzung.
/// Wird automatisch durch die statischen Methoden EditCopy und EditItem gesetzt.
/// </summary>
[Flags]
public enum EditorMode {
    OnlyShow = 0,
    EditCopy = 1,
    EditItem = 2
}