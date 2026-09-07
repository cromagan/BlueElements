// Licensed under MIT; see License.md for disclaimer, details, and extended user conditions.

namespace BlueTable.Classes;

/// <summary>
/// Virtuelle Spalte „Hinzufügen". Ein Klick auf den Kopf öffnet das
/// Dropdown zum Einblenden/Anlegen echter Spalten. Hat kein ColumnItem
/// und erscheint on-demand am Ende der Ansicht (Admin/Ansichtbearbeitung) —
/// sie wird nicht serialisiert.
/// </summary>
public sealed class AddColumnItem : ColumnViewItem {

    #region Fields

    public const int Width = 40;

    #endregion

    #region Constructors

    public AddColumnItem() : base((Table?)null) => IsExpanded = true;

    #endregion

    #region Properties

    public static string ClassId => "VIR_ADD";

    public override string Caption => "Neue Spalte";
    public override int FixedWidth => Width;

    public override string? Renderer => "TextOneLine";

    public override string RendererSettings => string.Empty;

    public override string? StorageKey => ClassId;

    #endregion

    #region Methods

    /// <summary>
    /// Zellen der Hinzufügen-Spalte haben keinen Inhalt — Klick und
    /// Dropdown erfolgen über den Spaltenkopf.
    /// </summary>
    public override string CellGetString(RowItem? row, bool isPinned) => string.Empty;

    #endregion
}