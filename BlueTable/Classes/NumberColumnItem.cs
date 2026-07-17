// Licensed under AGPL-3.0; see License.md for disclaimer and details.

namespace BlueTable.Classes;

/// <summary>
/// Virtuelle Spalte, die die Zeilennummer anzeigt — optisch identisch zur
/// (später zu entfernenden) Systemspalte SYS_ROWSORTINDEX. Hat kein eigenes
/// <see cref="ColumnItem" /> und kann vom Benutzer frei innerhalb der
/// Anordnung verschoben werden. Standardmäßig erscheint sie direkt hinter Pin.
/// Der angezeigte Wert wird aus SysRowSortIndex gelesen. Erscheint
/// ausschließlich dauerhaft, sobald sie als <see cref="StorageKey"/>
/// VIR_NUMBER in der Collection steht.
/// </summary>
public sealed class NumberColumnItem : ColumnViewItem {

    #region Constructors

    public NumberColumnItem() : base((Table?)null) { }

    #endregion

    #region Properties

    public static string ClassId => "VIR_NUMBER";

    public override string Caption => "Zeile";

    public override int FixedWidth => 40;

    public override string? Renderer => "Button";

    public override string RendererSettings => "{ClassId=\"Button\", ShowPic=-, ShowText=+, ShowCheckState=-, Padding={-4, -2}}";

    public override string? StorageKey => NumberColumnItem.ClassId;

    #endregion

    #region Methods

    /// <summary>
    /// Liefert die Zeilennummer aus SysRowSortIndex. Ohne aktiven
    /// Sortierungsindex wird leer zurückgegeben (nichts gezeichnet).
    /// </summary>
    public override string CellGetString(RowItem? row, bool isPinned) {
        if (row?.Table is not { IsDisposed: false } tb) { return string.Empty; }
        if (tb.Column.SysRowSortIndex is not { IsDisposed: false } sortCol) { return "?"; }
        return row.CellGetString(sortCol);
    }

    #endregion
}