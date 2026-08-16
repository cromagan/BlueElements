// Licensed under AGPL-3.0; see License.md for disclaimer and details.

using BlueControls.Controls;
using BlueControls.ControlStrategies;
using BlueTable.ColumnFormats;

namespace BlueControls.TableElements;

/// <summary>
/// Enthält Daten, wie eine Zeile angezeigt werden soll.
/// RowData kann mehrfach in einer Tabelle angezeigt werden.
/// Ein RowItem ist einzigartig, kann aber in mehreren RowData enthalten sein.
/// </summary>
public sealed class RowCaptionTableElement : TableElement {

    #region Fields

    /// <summary>
    /// Trennzeichen für Kapitel-Pfade. Kapitel werden NUR noch mit diesem
    /// Zeichen getrennt und ausgewertet. Kanonische Quelle für alle
    /// Chapter-Pfad-Routinen (<see cref="Extensions"/>).
    /// </summary>
    public const char Kapiteltrenner = '\\';

    #endregion

    #region Constructors

    public RowCaptionTableElement(string chapterText, ColumnViewCollection arrangement) : base(Identifier(chapterText), arrangement, chapterText.ChapterPathParent()) {
        ChapterText = chapterText.ChapterPathNormalize();
        // Hierarchie-Tiefe als Indent — einheitliche Behandlung aller Strukturen,
        // unabhängig vom Sortier-Modus (früher wurde NumberStyle flach dargestellt).
        Indent = ChapterText.ChapterPathDepth();
        IsExpanded = true;
    }

    #endregion

    #region Properties

    public string ChapterText { get; }

    public BlueFont Font_RowChapter => Skin.GetBlueFont(SheetStyle, PadStyles.Title);

    public bool IsExpanded { get; set; }

    /// <summary>
    /// Gibt an, ob dieses Kapitel per Doppelklick bearbeitet werden darf.
    /// Nur bei echten Kapiteln (nicht leer), mit vorhandener Kapitel-Spalte,
    /// außerhalb von TableChunk und wenn die Benutzerrechte es erlauben.
    /// </summary>
    internal bool CanEditChapter {
        get {
            if (Arrangement?.ColumnForChapter is not { IsDisposed: false } capCol) { return false; }
            if (Arrangement.Table is not { IsDisposed: false } tb) { return false; }
            if (tb is TableChunk) { return false; }
            if (string.IsNullOrEmpty(ChapterText)) { return false; }
            return tb.PermissionCheck(capCol.PermissionGroupsChangeCell, null, true);
        }
    }

    protected override bool DoSpezialOrder => true;

    #endregion

    #region Methods

    public static string Identifier(string caption) => $"CAP-{caption.ChapterPathNormalize().ToUpperInvariant()}";

    public override void Draw_Border(Graphics gr, ColumnViewItem viewItem, ColumnLineStyle lin, float xPos, float top, float bottom) { }

    public override void Draw_LowerLine(Graphics gr, ColumnViewItem viewItem, ColumnLineStyle lin, float left, float right, float bottom) => base.Draw_LowerLine(gr, viewItem, ColumnLineStyle.Dick, left, right, bottom);

    public override void Draw_UpperLine(Graphics gr, ColumnLineStyle lin, float left, float right, float bottom) => base.Draw_UpperLine(gr, ColumnLineStyle.Dick, left, right, bottom);

    /// <summary>
    /// Übernimmt die gesamte Doppelklick-Logik für ein Kapitel: prüft
    /// <see cref="CanEditChapter" /> und stellt sicher, dass der Klick nicht
    /// auf dem Pfeil-Button (Ein-/Ausklappen) lag. Startet anschließend die
    /// Kapitel-Editierung über <see cref="TableView.BeginEdit" />. Die Ermittlung
    /// der zum Block gehörenden Zeilen erfolgt über
    /// <see cref="TableView.GetChapterBlockRows" /> in der TableView, da sie
    /// auf deren sortierter Item-Liste basiert.
    /// </summary>
    public override bool HandleDoubleClick(ColumnViewItem? mouseOverColumn, TableView tableView) {
        if (!CanEditChapter) { return false; }
        if (IsArrowButtonHit(tableView.MouseDownData?.ControlX ?? 0, tableView.MouseDownData?.ControlY ?? 0,
                             tableView.Zoom, tableView.OffsetX, tableView.OffsetY)) { return false; }

        var blockRows = tableView.GetChapterBlockRows(this);
        if (blockRows is null) { return false; }

        var capPos = ControlPosition(tableView.Zoom, tableView.OffsetX, tableView.OffsetY);

        tableView.BeginEdit(
            TextBoxControlStrategy.ClassId,
            new Rectangle(0, capPos.Y, tableView.Width, capPos.Height),
            ChapterText.ChapterPathLastName(),
            v => ApplyChapter(tableView, blockRows, v),
            TextOneLineColumnFormat.Instance,
            null,
            null,
            null,
            null);
        return true;
    }

    public override int HeightInControl(ListBoxAppearance style, int columnWidth, Design itemdesign) => 40;

    public override string QuickInfoForColumn(ColumnViewItem cvi, int mouseXinColumn, int mouseYinColumn, float scale) {
        var displayText = ChapterText.ChapterPathLastName();
        if (CanEditChapter) {
            return $"{displayText}\rDoppelklick zum Bearbeiten";
        }

        return displayText;
    }

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
        var tmp = ChapterText.ChapterPathLastName();

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

    /// <summary>
    /// Übernimmt das Umbenennen oder Löschen dieses Kapitels. Es werden nur
    /// die Zeilen des übergebenen Blocks aktualisiert (lokal, nicht global
    /// über alle Zeilen mit gleichem Namen). Unterpfade werden rekursiv
    /// mitgeführt, sodass die Hierarchie erhalten bleibt. Commit-Callback
    /// aus <see cref="HandleDoubleClick" />, der an
    /// <see cref="TableView.BeginEdit" /> übergeben wird.
    /// </summary>
    private void ApplyChapter(TableView tableView, List<RowItem> blockRows, string value) {
        if (Arrangement?.ColumnForChapter is not { IsDisposed: false } capCol) { return; }
        if (Arrangement.Table is not { IsDisposed: false }) { return; }

        var oldChapter = ChapterText;
        var parentPath = oldChapter.ChapterPathParent();

        // newChapter: nur das letzte Segment ersetzen (Parent bleibt erhalten).
        // Einheitliche Behandlung — unabhängig vom NumberStyle.
        var newLastName = value.Replace("\r\n", "\r").ChapterPathNormalize();
        var newChapter = string.IsNullOrEmpty(parentPath)
            ? newLastName
            : parentPath + Kapiteltrenner + newLastName;

        if (!string.IsNullOrEmpty(newLastName) && newChapter != oldChapter) {
            // Kapitel umbenennen. Auch Unterpfade aktualisieren, damit
            // die Hierarchie erhalten bleibt (z. B. "A\B" → "A\C" ändert
            // auch "A\B\D" zu "A\C\D").
            foreach (var tableRow in blockRows) {
                if (tableRow is not { IsDisposed: false }) { continue; }
                var values = tableRow.CellGetList(capCol);
                var changed = false;
                for (var i = 0; i < values.Count; i++) {
                    var valueNorm = values[i].ChapterPathNormalize();
                    if (valueNorm == oldChapter) {
                        values[i] = newChapter;
                        changed = true;
                    } else if (valueNorm.StartsWith(oldChapter + Kapiteltrenner, StringComparison.Ordinal)) {
                        // Suffix hinter dem Prefix unverändert übernehmen.
                        values[i] = newChapter + valueNorm[oldChapter.Length..];
                        changed = true;
                    }
                }

                if (changed) {
                    tableRow.CellSet(capCol, values, "Kapitel umbenannt: " + oldChapter + " → " + newChapter);
                }
            }
        } else if (string.IsNullOrEmpty(newLastName)) {
            // Kapitel löschen: passenden Wert aus den Zellen entfernen,
            // sodass die Zeilen ohne Kapitel auf der obersten Ebene erscheinen.
            foreach (var tableRow in blockRows) {
                if (tableRow is not { IsDisposed: false }) { continue; }
                var values = tableRow.CellGetList(capCol);
                var changed = false;
                for (var i = 0; i < values.Count; i++) {
                    if (values[i].ChapterPathNormalize() == oldChapter) {
                        values[i] = string.Empty;
                        changed = true;
                    }
                }
                if (changed) {
                    tableRow.CellSet(capCol, values, "Kapitel entfernt: " + oldChapter);
                }
            }
        }
        tableView.Invalidate_AllViewItems(true);
    }

    #endregion
}