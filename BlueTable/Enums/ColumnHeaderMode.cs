// Licensed under AGPL-3.0; see License.md for disclaimer and details.

namespace BlueTable.Enums;

/// <summary>
/// Bestimmt, wie die Spaltenkopf-Leiste (A/B/C-Leiste) beschriftet wird.
/// </summary>
public enum ColumnHeaderMode {
    Ohne = 0,
    Buchstaben_ErsteAnsicht = 1,
    Zahlen_ErsteAnsicht = 2,
    Buchstaben_AktuelleAnsicht = 3,
    Zahlen_AktuelleAnsicht = 4
}
