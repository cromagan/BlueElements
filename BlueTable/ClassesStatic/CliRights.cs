// Licensed under MIT; see License.md for disclaimer, details, and extended user conditions.

namespace BlueTable.ClassesStatic;

/// <summary>
/// Die per Kommandozeile (CLI) erlaubten Aktionen. Die CLI vergleicht ausschließlich
/// diese Texte mit der Tabelle (<see cref="BlueTable.Enums.TableDataType.CliRights"/>).
/// 'Change permitted cell values' wendet die Spalten-Rechte zusätzlich wie für einen
/// Benutzer an (#CLI bzw. #User: <CLI-Name>), alle anderen Rechte ignoriieren die Benutzergruppen.
/// </summary>
public static class CliRights {

    #region Fields

    public const string AddColumn = "Add column";

    public const string AddRow = "Create row";

    public const string ChangeAllCellValues = "Change all cell values";

    public const string ChangePermittedCellValues = "Change permitted cell values";

    public const string ChangeColumnArrangement = "Change column arrangement";

    public const string DeleteColumn = "Delete column";

    public const string DeleteRow = "Delete row";

    public const string EditColumnHead = "Edit column head";

    public const string EditScript = "Edit script";

    public const string EditTableHead = "Edit table head";

    public const string ExecuteScript = "Execute script";

    public const string LockRow = "Lock row";

    public const string MoveRows = "Move rows";

    public const string RemoveRowLock = "Remove row lock";

    /// <summary>
    /// Alle bekannten CLI-Rechte in Anzeige-Reihenfolge.
    /// </summary>
    public static readonly List<string> AllRights =
    [
        AddRow,
        DeleteRow,
        MoveRows,
        ChangeAllCellValues,
        ChangePermittedCellValues,
        LockRow,
        RemoveRowLock,
        EditScript,
        ExecuteScript,
        AddColumn,
        DeleteColumn,
        ChangeColumnArrangement,
        EditTableHead,
        EditColumnHead
    ];

    /// <summary>
    /// Repariert gegebene Rechte-Texte: Groß-/Kleinschreibung wird kanonisiert, unbekannte Texte fallen weg.
    /// </summary>
    public static List<string> Repair(IEnumerable<string> rights) => [.. rights
        .Select(v => AllRights.FirstOrDefault(r => r.Equals(v, StringComparison.OrdinalIgnoreCase)))
        .OfType<string>()];

    #endregion
}
