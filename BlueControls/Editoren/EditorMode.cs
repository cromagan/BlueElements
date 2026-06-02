// Licensed under AGPL-3.0; see License.md for disclaimer and details.

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