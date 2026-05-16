// Licensed under AGPL-3.0; see License.md for disclaimer and details.

namespace BlueControls.Editoren;

/// <summary>
/// Bestimmt das Verhalten eines EditorEasy-Controls, insbesondere bei eingebetteter Nutzung.
/// Wird automatisch durch die statischen Methoden EditCopy, EditItem und EditNew gesetzt.
/// </summary>
[Flags]
public enum EditorMode {
    OnlyShow = 0,
    EditNew = 1,
    EditCopy = 2,
    EditItem = 4
}