// Licensed under AGPL-3.0; see License.md for disclaimer and details.

namespace BlueTable.Classes;

/// <summary>
/// Virtuelle Spalte „Hinzufügen". Ein Klick auf den Kopf öffnet das
/// Dropdown zum Einblenden/Anlegen echter Spalten. Hat kein
/// <see cref="ColumnItem" /> und kann vom Benutzer frei innerhalb der
/// Anordnung verschoben werden. Standardmäßig erscheint sie am Ende.
/// Setzt ViewType auf DummyColumn, damit bestehende IsDummyColumn-Prüfungen
/// weiterhin greifen. Erscheint entweder dauerhaft (als
/// <see cref="StorageKey"/> VIR_ADD in der Collection gespeichert) oder
/// on-demand während der Ansichtbearbeitung.
/// </summary>
public sealed class AddColumnItem : ColumnViewItem {

    #region Fields

    public const int Width = 40;

    #endregion

    #region Constructors

    internal AddColumnItem() : base((Table?)null) {
        ViewType = ViewType.DummyColumn;
        IsExpanded = true;
    }

    #endregion

    #region Properties

    public override string Caption => "Neue Spalte";
    public override int FixedWidth => Width;

    public override string? Renderer => "Bool";

    public override string RendererSettings => "{ClassId=\"Bool\", SymbolTrue=CheckBox_Checked, SymbolFalse=CheckBox}";

    public override string? StorageKey => "VIR_ADD";

    #endregion

    #region Methods

    /// <summary>
    /// Zellen der Hinzufügen-Spalte haben keinen Inhalt — Klick und
    /// Dropdown erfolgen über den Spaltenkopf.
    /// </summary>
    public override string CellGetString(RowItem? row, bool isPinned) => string.Empty;

    #endregion
}