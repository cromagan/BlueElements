// Licensed under AGPL-3.0; see License.md for disclaimer and details.

using BlueControls.Controls;
using BlueControls.Extended_Text;

namespace BlueControls.Classes.TableItems;

/// <summary>
/// Enthält Daten, wie eine Zeile angezeigt werden soll.
/// RowData kann mehrfach in einer Tabelle angezeigt werden.
/// Ein RowItem ist einzigartig, kann aber in mehreren RowData enthalten sein.
/// </summary>
public sealed class RowCaptionListItem : RowBackground {

    #region Fields

    /// <summary>
    /// Trennzeichen für Kapitel-Pfade. Kapitel werden NUR noch mit diesem
    /// Zeichen getrennt und ausgewertet. Kanonische Quelle für alle
    /// Chapter-Pfad-Routinen (<see cref="Extensions"/>).
    /// </summary>
    public const char Kapiteltrenner = '\\';

    #endregion

    #region Constructors

    public RowCaptionListItem(string chapterText, ColumnViewCollection arrangement) : base(Identifier(chapterText), arrangement, chapterText.ChapterPathParent()) {
        ChapterText = chapterText.ChapterPathNormalize();
        // NumberStyle (SYS_ROWSORTINDEX): Kapitel-Trenner wird ignoriert —
        // flache Anzeige ohne Einrückung. Sonst Hierarchie-Tiefe als Indent.
        Indent = IsNumberStyle(arrangement) ? 0 : ChapterText.ChapterPathDepth();
        IsExpanded = true;
    }

    #endregion

    #region Properties

    public string ChapterText { get; }

    public BlueFont Font_RowChapter => Skin.GetBlueFont(SheetStyle, PadStyles.Title);

    public bool IsExpanded { get; set; }

    /// <summary>
    /// Gibt an, ob dieses Kapitel per Doppelklick bearbeitet werden darf.
    /// Nur bei echten Kapiteln (nicht Angepinnt), mit vorhandener
    /// Kapitel-Spalte, außerhalb von TableChunk und wenn die Benutzerrechte
    /// es erlauben. Das Ohne-Kapitel (-?-) ist ebenfalls bearbeitbar —
    /// es repräsentiert leere Kapitel-Werte.
    /// </summary>
    internal bool CanEditChapter {
        get {
            if (Arrangement?.ColumnForChapter is not { IsDisposed: false } capCol) { return false; }
            if (Arrangement.Table is not { IsDisposed: false } tb) { return false; }
            if (tb is TableChunk) { return false; }
            if (ChapterText == TableView.DummyPinned) { return false; }
            return tb.PermissionCheck(capCol.PermissionGroupsChangeCell, null, true);
        }
    }

    /// <summary>
    /// Letztes Pfad-Segment ist -?- (Ohne), d.h. die Zeilen haben kein
    /// oder nur den Parent-Pfad als Kapitel-Wert.
    /// </summary>
    internal bool IsOhneChapter => string.Equals(ChapterText.ChapterPathLastName(), TableView.Ohne, StringComparison.OrdinalIgnoreCase);

    protected override bool DoSpezialOrder => true;

    #endregion

    #region Methods

    public static string Identifier(string caption) => $"CAP-{caption.ChapterPathNormalize().ToUpperInvariant()}";

    public override void Draw_Border(Graphics gr, ColumnViewItem viewItem, ColumnLineStyle lin, float xPos, float top, float bottom) { }

    public override void Draw_LowerLine(Graphics gr, ColumnViewItem viewItem, ColumnLineStyle lin, float left, float right, float bottom) => base.Draw_LowerLine(gr, viewItem, ColumnLineStyle.Dick, left, right, bottom);

    public override void Draw_UpperLine(Graphics gr, ColumnLineStyle lin, float left, float right, float bottom) => base.Draw_UpperLine(gr, ColumnLineStyle.Dick, left, right, bottom);

    public override int HeightInControl(ListBoxAppearance style, int columnWidth, Design itemdesign) => 40;

    public override string QuickInfoForColumn(ColumnViewItem cvi, int mouseXinColumn, int mouseYinColumn, float scale) {
        var displayText = IsNumberStyle(Arrangement) ? ChapterText : ChapterText.ChapterPathLastName();
        if (CanEditChapter) {
            if (IsOhneChapter) { displayText = "(leer)"; }
            return $"{displayText}\rDoppelklick zum Bearbeiten";
        }

        return displayText;
    }

    /// <summary>
    /// Ermittelt, ob das Arrangement im NumberStyle läuft (SYS_ROWSORTINDEX
    /// aktiv). In diesem Modus wird der Kapitel-Trenner '\' ignoriert —
    /// Kapitel werden flach auf einer Ebene dargestellt.
    /// </summary>
    internal static bool IsNumberStyle(ColumnViewCollection? arrangement)
        => arrangement?.Table is { IsDisposed: false } tb
           && tb.Column.SysRowSortIndex is { IsDisposed: false };

    /// <summary>
    /// Pfeil-Button-Rechteck in Control-Koordinaten (absolut).
    /// Der Button ist horizontal festgepinnt (ignoriert offsetX) und am
    /// linken Rand des eingerückten Bereichs positioniert (Indent beachten!).
    /// </summary>
    internal Rectangle ArrowButtonRect(float zoom, float offsetX, float offsetY) {
        var controlPos = ControlPosition(zoom, offsetX, offsetY);
        var p2 = 2.CanvasToControl(zoom);
        var p20 = IndentWidth.CanvasToControl(zoom);
        var indentOffset = p20 * Indent;
        var rowHeight = controlPos.Height;
        var size = Math.Min(p20, rowHeight - p2 * 2);
        var buttonY = controlPos.Top + (rowHeight - size) / 2;
        // Button ist horizontal festgepinnt — offsetX abziehen, damit die
        // Hit-Test-Position mit der gezeichneten (ungescrollten) Position übereinstimmt.
        return new Rectangle(controlPos.X - (int)offsetX + indentOffset + p2, buttonY, size, size);
    }

    internal void EditChapter(TableView tableView) {
        if (Arrangement?.ColumnForChapter is not { IsDisposed: false }) { return; }
        if (tableView.Table is not { IsDisposed: false }) { return; }

        // Die Zeilen des zusammenhängenden Blocks erfassen, damit beim
        // Umbenennen nur dieser Block — und nicht alle Zeilen mit dem
        // gleichen Kapitel-Namen in der gesamten Tabelle — geändert wird.
        var blockRows = tableView.GetChapterBlockRows(this);

        var capPos = ControlPosition(tableView.Zoom, tableView.OffsetX, tableView.OffsetY);

        var bt = tableView.BTB;
        bt.GetStyleFrom(ColumnFormatHolder_TextOneLine.Instance);
        bt.MultiLine = false;
        // Im Normalfall wird nur das letzte Pfad-Segment bearbeitet (analog zum
        // Windows Explorer). Im NumberStyle ist der Kapitel-Trenner ohne
        // Bedeutung — der gesamte Wert wird bearbeitet.
        // Bei -?- (Ohne) leere Textbox anzeigen, da -?- "kein Kapitel" bedeutet.
        bt.Text = IsOhneChapter ? string.Empty
                  : IsNumberStyle(Arrangement) ? ChapterText
                  : ChapterText.ChapterPathLastName();
        bt.Location = new Point(0, capPos.Y);
        bt.Size = new Size(tableView.Width, capPos.Height);
        bt.Tag = (List<object?>)[null, this, "ChapterEdit", blockRows];
        bt.Verhalten = SteuerelementVerhalten.Scrollen_ohne_Textumbruch;
        bt.Visible = true;
        bt.BringToFront();
        bt.Focus();
    }

    /// <summary>
    /// Prüft, ob der übergebene Punkt (in Control-Koordinaten) auf dem
    /// Pfeil-Button links liegt.
    /// </summary>
    internal bool IsArrowButtonHit(int controlX, int controlY, float zoom, float offsetX, float offsetY) {
        var rect = ArrowButtonRect(zoom, offsetX, offsetY);
        return rect.Contains(controlX, controlY);
    }

    protected override Size ComputeUntrimmedCanvasSize(Design itemdesign) => new(40, 40);

    protected override void DrawExplicit(Graphics gr, Rectangle visibleAreaControl, RectangleF positionControl, Design itemdesign, States state, bool drawBorderAndBack, bool translate, float offsetX, float offsetY, float zoom) {
        base.DrawExplicit(gr, visibleAreaControl, positionControl, itemdesign, state, drawBorderAndBack, translate, offsetX, offsetY, zoom);

        if (Arrangement is null) { return; }

        var fontScaled = Font_RowChapter.Scale(zoom);
        // Nur das letzte Pfad-Segment anzeigen — die Hierarchie wird über
        // Indent optisch dargestellt (analog zum Windows Datei-Explorer).
        // Im NumberStyle wird der Trenner ignoriert: der gesamte Kapitel-Wert
        // ist flach und wird vollständig angezeigt.
        var tmp = IsNumberStyle(Arrangement) ? ChapterText : ChapterText.ChapterPathLastName();

        var p2 = 2.CanvasToControl(zoom);
        var p5 = 5.CanvasToControl(zoom);
        var p14 = 14.CanvasToControl(zoom);
        var p20 = IndentWidth.CanvasToControl(zoom);

        // Stark verblasster Hintergrund für die gesamte Zeile (scrollt mit)
        gr.FillRectangle(new SolidBrush(Skin.Color_Back(Design.Table_And_Pad, States.Standard).SetAlpha(120)), positionControl);

        // Pfeil-Button und Text sind horizontal festgepinnt — sie ignorieren
        // den Scroll-Offset und bleiben an ihrer Position stehen. Nur der
        // Hintergrund (und die Spalten) scrollt.
        var pinnedX = (int)(positionControl.X - offsetX);
        var rowHeight = (int)positionControl.Height;
        var buttonSize = Math.Min(p20, rowHeight - p2 * 2);
        var buttonRect = new Rectangle(pinnedX + p2, (int)(positionControl.Top + (rowHeight - buttonSize) / 2), buttonSize, buttonSize);

        var arrowState = IsExpanded ? States.Standard : States.Checked;
        var etxt = new ExtText(Design.Button_CheckBox, arrowState);
        Button.DrawButton(null, gr, Design.Button_OpenClose, arrowState, null, Alignment.Horizontal_Vertical_Center, false, etxt, string.Empty, buttonRect, false);

        // Pfeil-Icon zentriert im Button
        var arrowCode = IsExpanded ? "MinusZeichen2" : "PlusZeichen2";
        gr.DrawImageUnscaled(QuickImage.Get(arrowCode + "|" + p14), buttonRect.X + (buttonRect.Width - p14) / 2, buttonRect.Y + (buttonRect.Height - p14) / 2);

        // Wort daneben ohne Rahmen
        var si = fontScaled.MeasureString(tmp);
        var textX = buttonRect.Right + p5;
        var textY = (int)(positionControl.Top + (rowHeight - si.Height) / 2);
        fontScaled.DrawString(gr, tmp, textX, textY);
    }

    protected override string GetCompareKey() => ChapterText;

    #endregion
}