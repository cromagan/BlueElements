// Licensed under AGPL-3.0; see License.md for disclaimer and details.

namespace BlueTable.Classes;

/// <summary>
/// Virtuelle Spalte zum Anpinnen einzelner Zeilen. Hat kein eigenes
/// <see cref="ColumnItem" /> und kann vom Benutzer frei innerhalb der
/// Anordnung verschoben werden. Standardmäßig erscheint sie am Anfang.
/// Jede Zeile zeigt ein Pin-Symbol (Renderer_Bool), dessen Zustand
/// (angepinnt/nicht angepinnt) über die TableView gesteuert wird.
/// Erscheint entweder dauerhaft (als <see cref="StorageKey"/> VIR_PIN
/// in der Collection gespeichert) oder on-demand, sobald mindestens
/// eine Zeile angepinnt ist.
/// </summary>
public sealed class PinColumnItem : ColumnViewItem {

    #region Constructors

    public PinColumnItem() : base((Table?)null) { }

    #endregion

    #region Properties

    public static string ClassId => "VIR_PIN";

    public override string Caption => "Pin";

    public override int FixedWidth => 24;

    public override string? Renderer => "Bool";
    public override string RendererSettings => "{ClassId=\"Bool\", SymbolTrue=Checkbox_Checked, SymbolFalse=Checkbox}";
    public override string? StorageKey => PinColumnItem.ClassId;

    #endregion

    #region Methods

    /// <summary>
    /// Liefert den Bool-Wert für den Pin-Zustand. Der tatsächliche Zustand
    /// (angepinnt/nicht angepinnt) ist ansichtspezifisch und wird beim
    /// Zeichnen aus MarkYellow (RowListItem) bestimmt. Dieser Default
    /// deckt den nicht-angepinnten Fall ab.
    /// </summary>
    public override string CellGetString(RowItem? row, bool isPinned) => isPinned ? "+" : "-";

    #endregion
}