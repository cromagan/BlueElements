// Licensed under AGPL-3.0; see License.md for disclaimer and details.

namespace BlueTable.Enums;

/// <summary>
/// Definiert die bekannten System-Spaltennamen.
/// </summary>
public static class SystemColumnKeys {
    public const string RowState = "SYS_ROWSTATE";
    public const string DateChanged = "SYS_DATECHANGED";
    public const string Changer = "SYS_CHANGER";
    public const string DateCreated = "SYS_DATECREATED";
    public const string Creator = "SYS_CREATOR";
    public const string Correct = "SYS_CORRECT";
    public const string Locked = "SYS_LOCKED";
    public const string RowKey = "SYS_ROWKEY";
    public const string CellNote = "SYS_CELLNOTE";

    /// <summary>
    /// Benutzerdefinierte Sortierung. Ist diese Spalte vorhanden, wird die
    /// Tabellen-Sortierung fixiert und immer aufsteigend nach diesem Index sortiert.
    /// </summary>
    public const string RowSortIndex = "SYS_ROWSORTINDEX";

    // Veraltete Spalten (TODO: Entfernen)
    public const string RowColor_Obsolete = "SYS_ROWCOLOR";
    public const string Chapter_Obsolete = "SYS_CHAPTER";

    // Alte Alternativnamen
    public const string ChangeDate_Alt = "SYS_CHANGEDATE";
    public const string CreateDate_Alt = "SYS_CREATEDATE";
}
