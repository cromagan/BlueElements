// Licensed under MIT; see License.md for disclaimer, details, and extended user conditions.

namespace BlueTable.Interfaces;

public interface IHasColumn : IErrorCheckable {

    #region Properties

    ColumnItem? Column { get; set; }

    /// <summary>
    /// Die Tabelle der Spalte; null, wenn keine gültige Spalte gesetzt ist.
    /// </summary>
    Table? Table => Column is { IsDisposed: false } column ? column.Table : null;

    #endregion
}