// Licensed under MIT; see License.md for disclaimer, details, and extended user conditions.

namespace BlueTable.Classes;

/// <summary>
/// Vom Kopf-Button-Klick anstoßbare UI-Aktionen; implementiert von TableView.
/// </summary>
public interface IColumnHeadButtonHost {

    #region Methods

    /// <summary>
    /// Öffnet den Spalten-Editor für die angegebene Spalte (inkl. Reparatur).
    /// </summary>
    /// <param name="column">Die zu bearbeitende Spalte.</param>
    void EditColumn(ColumnItem column);

    /// <summary>
    /// Zwingt die Ansicht zum Neuaufbau der Spalten.
    /// </summary>
    void InvalidateCurrentArrangement();

    /// <summary>
    /// Setzt die temporäre Sortierung zurück.
    /// </summary>
    void ResetSortDefinition();

    #endregion
}