// Licensed under AGPL-3.0; see License.md for disclaimer and details.

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