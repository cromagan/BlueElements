// Licensed under MIT; see License.md for disclaimer, details, and extended user conditions.

namespace BlueTable.ClassesStatic;

/// <summary>
/// Die per Kommandozeile (CLI) erlaubten Aktionen. Die CLI vergleicht ausschließlich
/// diese Texte mit der Tabelle (<see cref="BlueTable.Enums.TableDataType.CliRights"/>) —
/// die Benutzergruppen-Rechte der Tabelle werden von der CLI ignoriert.
/// </summary>
public static class CliRights {

    #region Fields

    public const string AddColumn = "Add column";

    public const string AddRow = "Create row";

    public const string ChangeCellValues = "Change cell values";

    public const string DeleteColumn = "Delete column";

    public const string DeleteRow = "Delete row";

    public const string EditScript = "Edit script";

    public const string EditTableHead = "Edit table head";

    public const string ExecuteScript = "Execute script";

    public const string RemoveRowLock = "Remove row lock";

    /// <summary>
    /// Alle bekannten CLI-Rechte in Anzeige-Reihenfolge.
    /// </summary>
    public static readonly List<string> AllRights =
    [
        AddRow,
        DeleteRow,
        ChangeCellValues,
        RemoveRowLock,
        EditScript,
        ExecuteScript,
        AddColumn,
        DeleteColumn,
        EditTableHead
    ];

    #endregion
}
