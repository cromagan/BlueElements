// Licensed under AGPL-3.0; see License.md for disclaimer and details.

using BlueControls.Renderer;

namespace BlueControls.Classes.TableItems;

/// <summary>
/// Zeigt eine Spaltenkopf-Leiste mit Excel-ähnlichen Spaltenbuchstaben (A, B, C, ...)
/// oder fortlaufenden Zahlen. Höhe ist 16 Pixel (zoomabhängig). Verwendet den Button-Renderer
/// für die Optik. Dient als Drag/Drop-Quelle für die Spalten-Sortierung.
/// Die Beschriftung wird über <see cref="ColumnHeaderMode"/> gesteuert.
/// </summary>
///
public sealed class ColumnHeaderBarListItem : RowBackground {

    #region Fields

    public const int HeaderHeight = 16;
    public const string Identifier = "ColumnHeaderBarListItem";

    #endregion

    #region Constructors

    public ColumnHeaderBarListItem(ColumnViewCollection? arrangement) : base(Identifier, arrangement, string.Empty) => IgnoreYOffset = true;

    #endregion

    #region Properties

    public ColumnHeaderMode Mode { get; set; }

    protected override bool DoSpezialOrder => true;

    #endregion

    #region Methods

    /// <summary>
    /// Wandelt einen 0-basierten Spaltenindex in Excel-Spaltenbuchstaben um
    /// (0 -> A, 25 -> Z, 26 -> AA, ...).
    /// </summary>
    public static string ColumnLetter(int index) {
        var letter = string.Empty;
        index++;
        while (index > 0) {
            var remainder = (index - 1) % 26;
            letter = (char)('A' + remainder) + letter;
            index = (index - 1) / 26;
        }
        return letter;
    }

    public override void Draw_ColumnBackGround(Graphics gr, ColumnViewItem viewItem, RectangleF positionControl, States state, Brush? rowcolor) {
        base.Draw_ColumnBackGround(gr, viewItem, positionControl, state, rowcolor);
        // Button-Hintergrund
        Skin.Draw_Back(gr, Design.Button_AutoFilter, States.Standard, positionControl.ToRect(), null, false);
    }

    public override void Draw_ColumnContent(Graphics gr, ColumnViewItem viewItem, RectangleF positionControl, float scale, TranslationType translate, float offsetX, float offsetY, States state) {
        base.Draw_ColumnContent(gr, viewItem, positionControl, scale, translate, offsetX, offsetY, state);

        var text = HeaderText(viewItem);
        if (string.IsNullOrEmpty(text)) { return; }

        // Button-Renderer verwenden für die Optik
        var renderer = new Renderer_Button {
            Text_anzeigen = true,
            Bild_anzeigen = false,
            CheckStatus_anzeigen = false,
            Padding = new System.Windows.Forms.Padding(0)
        };
        renderer.Draw(gr, text, null, positionControl.ToRect(), translate, Alignment.Horizontal_Vertical_Center, scale, Design.Item_ListBox, state);
    }

    public override void Draw_LowerLine(Graphics gr, ColumnViewItem viewItem, ColumnLineStyle lin, float left, float right, float bottom) => base.Draw_LowerLine(gr, viewItem, ColumnLineStyle.Ohne, left, right, bottom);

    public override int HeightInControl(ListBoxAppearance style, int columnWidth, Design itemdesign) => HeaderHeight;

    public override string QuickInfoForColumn(ColumnViewItem cvi, int mouseXinColumn, int mouseYinColumn, float scale) {
        var text = HeaderText(cvi);
        return string.IsNullOrEmpty(text) ? string.Empty : "Spalte " + text;
    }

    protected override Size ComputeUntrimmedCanvasSize(Design itemdesign) => new(100, HeaderHeight);

    /// <summary>
    /// Liefert die Ansicht, die als Referenz für die Nummerierung genutzt wird.
    /// Bei "nach erster Ansicht" ist das <see cref="Table.ColumnArrangements"/>[1]
    /// (die Standard-Ansicht). Ist diese nicht vorhanden, wird null zurückgegeben.
    /// </summary>
    private ColumnViewCollection? FirstArrangement() {
        if (Arrangement?.Table is not { IsDisposed: false } tb) { return null; }
        var arrangements = tb.ColumnArrangements;
        if (arrangements.Count < 2) { return null; }
        return arrangements[1];
    }

    /// <summary>
    /// Berechnet den anzuzeigenden Text für die Spalte <paramref name="viewItem"/>
    /// abhängig von <see cref="Mode"/>. Die SYS_ROWSORTINDEX-Spalte wird dabei
    /// immer als nicht existent betrachtet und mit leerem Text angezeigt.
    /// </summary>
    private string HeaderText(ColumnViewItem viewItem) {
        if (Mode == ColumnHeaderMode.Ohne) { return string.Empty; }
        if (viewItem.Column is not { IsDisposed: false } col) { return string.Empty; }
        if (col.KeyName == SystemColumnKeys.RowSortIndex) { return string.Empty; }

        ColumnViewCollection? reference;
        bool useLetters;

        switch (Mode) {
            case ColumnHeaderMode.Buchstaben_ErsteAnsicht:
                reference = FirstArrangement();
                useLetters = true;
                break;

            case ColumnHeaderMode.Zahlen_ErsteAnsicht:
                reference = FirstArrangement();
                useLetters = false;
                break;

            case ColumnHeaderMode.Buchstaben_AktuelleAnsicht:
                reference = Arrangement;
                useLetters = true;
                break;

            case ColumnHeaderMode.Zahlen_AktuelleAnsicht:
                reference = Arrangement;
                useLetters = false;
                break;

            default:
                return string.Empty;
        }

        if (reference is null) { return string.Empty; }

        var logicalIndex = -1;
        foreach (var cvi in reference) {
            if (cvi is not { IsDisposed: false }) { continue; }
            if (cvi.Column is not { IsDisposed: false } c) { continue; }
            if (c.KeyName == SystemColumnKeys.RowSortIndex) { continue; }

            logicalIndex++;
            if (c == col) {
                return useLetters ? ColumnLetter(logicalIndex) : (logicalIndex + 1).ToString1();
            }
        }

        // Spalte ist nicht in der Referenz-Ansicht enthalten
        return string.Empty;
    }

    #endregion
}