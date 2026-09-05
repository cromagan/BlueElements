// Licensed under AGPL-3.0; see License.md for disclaimer and details.

using BlueControls.ControlStrategies;
using BlueControls.Controls;
using BlueControls.EventArgs;
using BlueTable.EventArgs;
using System.Windows.Forms;

namespace BlueControls.TableElements;

/// <summary>
/// Enthält Daten, wie eine Zeile angezeigt werden soll.
/// RowData kann mehrfach in einer Tabelle angezeigt werden.
/// Ein RowItem ist einzigartig, kann aber in mehreren RowData enthalten sein.
/// </summary>
public sealed class RowTableElement : TableElement {

    #region Fields

    public static readonly SolidBrush BrushYellowTransparent = new(Color.FromArgb(180, 255, 255, 0));
    public static readonly HatchBrush RedStripesBrush1 = new HatchBrush(HatchStyle.DiagonalCross, Color.FromArgb(30, 255, 0, 0), Color.FromArgb(5, 255, 0, 0));
    public static readonly HatchBrush RedStripesBrush2 = new HatchBrush(HatchStyle.DarkVertical, Color.FromArgb(80, 255, 100, 0), Color.FromArgb(80, 255, 0, 0));

    private static readonly Brush BrushBrighten = new SolidBrush(Color.FromArgb(128, 255, 255, 255));
    private static readonly Brush BrushDarken = new SolidBrush(Color.FromArgb(128, 0, 0, 0));
    private static readonly Pen PenBrighten = new Pen(Color.FromArgb(128, 255, 255, 255));
    private static readonly Pen PenDarken = new Pen(Color.FromArgb(128, 0, 0, 0));

    private string? _heightWidthKey;
    private int _heightWidthValue = -1;

    #endregion

    #region Constructors

    public RowTableElement(RowItem row, string alignsToCaption, ColumnViewCollection? arrangement) : base(Identifier(row, alignsToCaption), arrangement, alignsToCaption.ChapterPathNormalize()) {
        Row = row;
        // Gleicher Indent wie die zugehörige Überschrift — die Items stehen
        // unter der Kapitel-Überschrift, nicht zusätzlich eingerückt.
        // Einheitliche Behandlung: immer Hierarchie-Tiefe als Indent.
        Indent = string.IsNullOrEmpty(alignsToCaption) ? 0 : alignsToCaption.ChapterPathDepth();
    }

    #endregion

    #region Properties

    public ColumnItem? Column { get; set; }

    /// <summary>
    /// Wahr, wenn diese Zeile aktuell angepinnt ist. Wird von der TableView
    /// beim Aufbau der sichtbaren Items gesetzt. Steuert sowohl die gelbe
    /// Hinterlegung als auch den Zustand des Pin-Buttons in der virtuellen
    /// Pin-Spalte.
    /// </summary>
    public bool MarkYellow {
        get;
        set {
            if (field == value) { return; }
            field = value;
            OnPropertyChanged();
        }
    }

    public RowItem Row { get; }

    protected override bool DoSpezialOrder => true;

    #endregion

    #region Methods

    public static void ColumnBackGround(Graphics gr, ColumnViewItem viewItem, RectangleF positionControl) {
        switch (viewItem.Column?.BackgroundStyle ?? ColumnBackgroundStyle.None) {
            case ColumnBackgroundStyle.None:
            case ColumnBackgroundStyle.Brighten:
            case ColumnBackgroundStyle.Darken:
                break;

            case ColumnBackgroundStyle.PopIn:

                for (var z = 0; z < 4; z++) {
                    gr.DrawLine(PenDarken, positionControl.Left + z, positionControl.Top + z, positionControl.Right - z, positionControl.Top + z);
                    gr.DrawLine(PenDarken, positionControl.Left + z, positionControl.Top + z, positionControl.Left + z, positionControl.Bottom - z);
                    gr.DrawLine(PenBrighten, positionControl.Right - z, positionControl.Top + z, positionControl.Right - z, positionControl.Bottom - z);
                    gr.DrawLine(PenBrighten, positionControl.Left + z, positionControl.Bottom - z, positionControl.Right - z, positionControl.Bottom - z);
                }
                break;

            case ColumnBackgroundStyle.PopOut:

                for (var z = 0; z < 4; z++) {
                    gr.DrawLine(PenBrighten, positionControl.Left + z, positionControl.Top + z, positionControl.Right - z, positionControl.Top + z);
                    gr.DrawLine(PenBrighten, positionControl.Left + z, positionControl.Top + z, positionControl.Left + z, positionControl.Bottom - z);
                    gr.DrawLine(PenDarken, positionControl.Right - z, positionControl.Top + z, positionControl.Right - z, positionControl.Bottom - z);
                    gr.DrawLine(PenDarken, positionControl.Left + z, positionControl.Bottom - z, positionControl.Right - z, positionControl.Bottom - z);
                }
                break;
        }
    }

    public static void ColumnOverlay(Graphics gr, ColumnViewItem viewItem, RectangleF positionControl) {
        switch (viewItem.Column?.BackgroundStyle ?? ColumnBackgroundStyle.None) {
            case ColumnBackgroundStyle.None:
            case ColumnBackgroundStyle.PopIn:
            case ColumnBackgroundStyle.PopOut:
                break;

            case ColumnBackgroundStyle.Brighten:

                gr.FillRectangle(BrushBrighten, positionControl);
                break;

            case ColumnBackgroundStyle.Darken:
                gr.FillRectangle(BrushDarken, positionControl);
                break;
        }
    }

    public static string Identifier(RowItem row, string chapter) => chapter.ChapterPathNormalize().ToUpperInvariant() + RowCaptionTableElement.Kapiteltrenner + row.KeyName;

    public static string QuickInfoText(ColumnItem? col, string additionalText) {
        if (col?.Table is not { IsDisposed: false }) { return string.Empty; }

        var T = string.Empty;
        if (!string.IsNullOrEmpty(col.QuickInfo)) { T += col.QuickInfo; }

        if (col.Table.IsAdministrator()) {
            if (!string.IsNullOrEmpty(col.AdminInfo)) { T = T + "<br><br><b><u>Administrator-Info:</b></u><br>" + col.AdminInfo; }
            //if (col.ColumnTags.Count > 0) { T = T + "<br><br><b><u>Spalten-Tags:</b></u><br>" + col.ColumnTags.JoinWith("<br>"); }
            //T = T + "<br><br>" + ColumnEditor.ColumnUsage(col);
        }

        T = T.Trim();
        T = T.Trim("<br>");
        T = T.Trim();
        if (!string.IsNullOrEmpty(T) && !string.IsNullOrEmpty(additionalText)) {
            T = "<b><u>" + additionalText + "</b></u><br><br>" + T;
        }
        return T;
    }

    public static string UndoText(ColumnItem? column, RowItem? row) {
        if (column?.Table is not { IsDisposed: false } tb) { return string.Empty; }

        if (tb.Undo.Count == 0) { return string.Empty; }

        var cellKey = CellCollection.KeyOfCell(column, row);
        var sb = new System.Text.StringBuilder();
        for (var z = tb.Undo.Count - 1; z >= 0; z--) {
            if (tb.Undo[z] is { } undo && undo.CellKey == cellKey) {
                sb.Append(undo.UndoTextTableMouseOver());
                sb.Append("<br>");
            }
        }
        var t = sb.ToString();
        t = t.Trim("<br>");
        t = t.Trim("<hr>");
        t = t.Trim("<br>");
        t = t.Trim("<hr>");
        return t;
    }

    public override void Draw_ColumnBackGround(Graphics gr, ColumnViewItem viewItem, RectangleF positionControl, States state, Brush? rowcolor) {
        base.Draw_ColumnBackGround(gr, viewItem, positionControl, state, rowcolor);

        ColumnBackGround(gr, viewItem, positionControl);

        if (rowcolor is not null) {
            gr.FillRectangle(rowcolor, positionControl);
        }

        if (IsAdministrator()) {
            if (RowCollection.FailedRows.ContainsKey(Row)) {
                gr.FillRectangle(RedStripesBrush2, positionControl);
            } else if (Row.NeedsRowUpdate()) {
                gr.FillRectangle(RedStripesBrush1, positionControl);
            }
        }

        if (MarkYellow) {
            gr.FillRectangle(BrushYellowTransparent, positionControl);
        }
    }

    public override void Draw_ColumnContent(Graphics gr, ColumnViewItem viewItem, RectangleF positionControl, float scale, TranslationType translate, float offsetX, float offsetY, States state) {
        base.Draw_ColumnContent(gr, viewItem, positionControl, scale, translate, offsetX, offsetY, state);

        if (Row.IsDisposed) { return; }

        // Weder echte noch virtuelle Spalte — nichts zu zeichnen.
        if (!viewItem.IsOk()) { return; }

        // Echte Spalten: spaltenspezifische Verarbeitung (Cursor, Notizen,
        // RowCheck). Virtuelle Spalten überspringen diesen Block.
        if (viewItem.Column is { IsDisposed: false } column) {
            if (!column.SaveContent && Table.ExecutingScriptThreadsAnyTable.Count == 0) {
                Row.CheckRow();
            }

            if (column == Column) {
                var _tmpCursorRect = new Rectangle((int)positionControl.X + 1, (int)positionControl.Y + 1, (int)positionControl.Width - 2, (int)positionControl.Height - 2);
                Skin.Draw_Back(gr, Design.Table_Cursor, state, _tmpCursorRect, null, false);
                Skin.Draw_Border(gr, Design.Table_Cursor, state, _tmpCursorRect);
            }

            if (column.Table is { IsDisposed: false }) {
                var note = CellNoteHelper.GetNoteData(column, Row);
                if (note.HasValue && note.Value.Text.Length > 0) {
                    var noteDesign = NoteEntry.DesignFor(note.Value.Symbol);
                    var noteRect = new Rectangle((int)positionControl.X + 1, (int)positionControl.Y + 1, (int)positionControl.Width - 2, (int)positionControl.Height - 2);
                    Skin.Draw_Border(gr, noteDesign, States.Standard, noteRect);
                    if (NoteEntry.GetQuickImage(note.Value.Symbol, 10.CanvasToControl(scale)) is { } icon) {
                        gr.DrawImageUnscaled(icon, (int)(positionControl.Right - icon.Width - 1), (int)positionControl.Top + 1);
                    }
                }
            }
        }

        var toDrawd = viewItem.CellGetString(Row, MarkYellow);

        var pax = 4.CanvasToControl(scale);
        var pay = 2.CanvasToControl(scale);
        positionControl.Inflate(-pax, -pay);

        if (state.HasFlag(States.Standard_HasFocus)) { state ^= States.Standard_HasFocus; }
        viewItem.GetRenderer(SheetStyle).Draw(gr, toDrawd, Row, positionControl.ToRect(), translate, (Alignment)viewItem.Align, scale, Design.Item_ListBox, state);
    }

    public override void Draw_ColumnOverlay(Graphics gr, ColumnViewItem viewItem, RectangleF positionControl, States state) {
        base.Draw_ColumnOverlay(gr, viewItem, positionControl, state);

        ColumnOverlay(gr, viewItem, positionControl);
    }

    public override void HandleMouseUp(ColumnViewItem? mouseOverColumn, TableView tableView, CanvasMouseEventArgs e) {
        if (mouseOverColumn is PinColumnItem) {
            if (tableView.PinnedRows.Contains(Row)) {
                tableView.PinRemove(Row);
            } else {
                tableView.PinAdd(Row);
            }
            tableView.Invalidate_CurrentArrangement();
            return;
        }

        if (mouseOverColumn?.Column is { IsDisposed: false } col && Row is { IsDisposed: false }) {
            // Instant-Action-Strategien (z. B. Tabellen-Skript-Knopf) führen den
            // einfachen Klick sofort aus — ohne CellClicked-Event und Mini-Toolbar.
            if (ControlStrategy.InstantActionClicked(col, Row)) { return; }

            tableView.OnCellClicked(new CellEventArgs(col, Row));
            tableView.Invalidate();

            // Mini-Toolbar anzeigen. Ob sie tatsächlich erscheint oder
            // bei einem erneuten Klick auf dieselbe Zelle ausgeblendet
            // bleibt, entscheidet MiniToolbarShow anhand des HotItems.
            tableView.ShowMiniToolbarAt(mouseOverColumn, this, Row);
        }
    }

    /// <summary>
    /// Startet die Inline-Editierung der angeklickten Zelle. Die gesamte
    /// Logik (Editability, LinkedCell-Auflösung, ControlStrategy, Position) liegt in
    /// der Basisklasse TableElement.BeginCellEdit.
    /// </summary>
    public override bool HandleDoubleClick(ColumnViewItem? mouseOverColumn, TableView tableView) {
        if (mouseOverColumn is null) { return false; }
        if (Row is not { IsDisposed: false }) { return false; }
        return BeginCellEdit(tableView, mouseOverColumn, this, Row, Row.ChunkValue);
    }

    /// <summary>
    /// Setzt die Tastatur-Aktionen auf der Cursor-Zelle um (Ctrl+X/C/V, F2,
    /// Delete). Aufgerufen aus TableView.OnKeyDown, sobald die
    /// dort behandelten Navigations-Tasten verarbeitet wurden.
    /// </summary>
    public override void HandleKeyDown(ColumnViewItem? cursorColumn, TableView tableView, KeyEventArgs e) {
        if (cursorColumn?.Column is not { IsDisposed: false } c) { return; }
        if (Row is not { IsDisposed: false }) { return; }

        switch (e.KeyCode) {
            case Keys.X:
                if (e.Modifiers == Keys.Control) {
                    CopyCellContent(cursorColumn, tableView);
                    TableView.NotEditableInfo(TableView.UserEdited(tableView, c.DefaultValueForColumn(), cursorColumn, this, true));
                }
                break;

            case Keys.C:
                if (e.Modifiers == Keys.Control) {
                    CopyCellContent(cursorColumn, tableView);
                }
                break;

            case Keys.V:
                if (e.Modifiers == Keys.Control) {
                    tableView.ContextMenu_ContentPaste(null, null);
                }
                break;

            case Keys.F2:
                _ = BeginCellEdit(tableView, cursorColumn, this, Row, Row.ChunkValue);
                break;

            case Keys.Delete:
                TableView.NotEditableInfo(TableView.UserEdited(tableView, c.DefaultValueForColumn(), cursorColumn, this, true));
                break;
        }
    }

    public override int HeightInControl(ListBoxAppearance style, int columnWidth, Design itemdesign) {
        if (IsDisposed || Row.IsDisposed || Arrangement is null) { return 18; }

        // columnWidth ist die Canvas-Gesamtbreite (von CalculateAllViewItems_CalculateYPosition
        // als arrangement.ControlColumnsWidth().ControlToCanvas(Zoom) übergeben).
        // ControlColumnsWidth() ist die Control-Gesamtbreite. Daraus lässt sich der
        // Zoom ableiten, um Control-Pixel → Canvas-Pixel zu konvertieren.
        var totalControlWidth = Arrangement.ControlColumnsWidth();
        var zoom = columnWidth > 0 && totalControlWidth > 0
            ? (float)totalControlWidth / columnWidth
            : 1f;

        // Cache-Schlüssel inkl. Zoom (bzw. canvas-Gesamtbreite), damit bei
        // Zoom-Änderung neu berechnet wird — auch wenn die Control-Breiten gleich bleiben.
        var key = BuildColumnWidthsKey(columnWidth);
        if (key == _heightWidthKey) { return _heightWidthValue; }

        var drawHeight = 18;

        foreach (var thisViewItem in Arrangement) {
            if (thisViewItem.Column is { IsDisposed: false } tmpc) {
                var renderer = thisViewItem.GetRenderer(SheetStyle);
                // ControlColumnWidth() ist in Control-Pixeln → Canvas-Pixel: / zoom.
                // 4 Canvas-Pixel Padding je Seite abziehen.
                var contentWidth = Math.Max(1, (int)(thisViewItem.ControlColumnWidth() / zoom) - 8);
                drawHeight = Math.Max(drawHeight, renderer.ContentSizeAtWidth(Row.CellGetString(tmpc), tmpc.DoOpticalTranslation, contentWidth).Height);
            }
        }

        drawHeight = Math.Min(drawHeight, 200);
        drawHeight = Math.Max(drawHeight + 4, 18);

        _heightWidthKey = key;
        _heightWidthValue = drawHeight;
        return drawHeight;
    }

    public override void HandleMouseMove(ColumnViewItem? mouseOverColumn, TableView tableView, CanvasMouseEventArgs e) {
        if (mouseOverColumn is not { IsDisposed: false } cvi || e.Button != MouseButtons.None) {
            base.HandleMouseMove(mouseOverColumn, tableView, e);
            return;
        }

        var qt = string.Empty;

        if (cvi is PinColumnItem) {
            qt = MarkYellow ? "Zeile nicht mehr anpinnen" : "Zeile anpinnen";
        } else if (cvi.Column is { IsDisposed: false } column && column.Table is { IsDisposed: false } tb) {
            if (IsAdministrator() && RowCollection.FailedRows.ContainsKey(Row)) {
                qt = Row.LastFailedReason();
            } else {
                var note = CellNoteHelper.GetNoteData(column, Row);
                if (note.HasValue && note.Value.Text.Length > 0) {
                    qt = $"<u><imagecode={NoteEntry.ImageCodeFor(note.Value.Symbol)}|16> <b>Notiz:</b></u><br>{note.Value.Text}";
                } else if (column.RelationType == RelationType.CellValues) {
                    if (column.LinkedTable is null) {
                        qt = "Verknüpfung zur Ziel-Tabelle fehlerhaft.";
                    } else {
                        var (lcolumn, _, info, _) = Row.LinkedCellData(column, true, false);
                        if (lcolumn is { } lc) { qt = QuickInfoText(lc, column.ReadableText() + " bei " + lc.ReadableText() + ":"); }

                        if (!string.IsNullOrEmpty(info) && tb.IsAdministrator()) {
                            if (string.IsNullOrEmpty(qt)) { qt += "\r\n"; }
                            qt += "Verlinkungs-Status: " + info;
                        }
                    }
                } else if (tb.IsAdministrator()) {
                    qt = UndoText(column, Row);
                }
            }
        }

        tableView.QuickInfo = qt;
    }

    protected override Size ComputeUntrimmedCanvasSize(Design itemdesign) {
        if (IsDisposed || Row.IsDisposed || Arrangement is null) { return new(16, 16); }

        // An HeightInControl delegieren, damit Spaltenbreiten-basierte Höhen-
        // berechnung (inkl. ScaleToFit) konsistent verwendet wird.
        return new(100, HeightInControl(ListBoxAppearance.Listbox, 0, itemdesign));
    }

    protected override void DrawExplicit(Graphics gr, Rectangle visibleAreaControl, RectangleF positionControl, Design itemdesign, States state, bool drawBorderAndBack, bool translate, float offsetX, float offsetY, float zoom) {
        base.DrawExplicit(gr, visibleAreaControl, positionControl, itemdesign, state, drawBorderAndBack, translate, offsetX, offsetY, zoom);
        if (Column is null) { return; }

        // positionControl ist bereits um den Indent nach rechts verschoben
        // (controlIndented). Der Cursor-Rahmen soll die gesamte Zeile von ganz
        // links bis ganz rechts umfassen — daher den Indent-Offset wieder
        // zur Breite addieren.
        var indentOffset = IndentWidth.CanvasToControl(zoom) * Indent;
        var _tmpCursorRect = positionControl.ToRect();
        var pen = BorderDraw.GetPen(Skin.Color_Border(Design.Table_Cursor, state).SetAlpha(180), 1);
        lock (pen) { gr.DrawRectangle(pen, new Rectangle(-1, _tmpCursorRect.Top, _tmpCursorRect.Width + indentOffset + 2, _tmpCursorRect.Height - 1)); }
    }

    private string BuildColumnWidthsKey(int canvasTotalWidth) {
        if (Arrangement is null) { return string.Empty; }

        var sb = new System.Text.StringBuilder();
        sb.Append(canvasTotalWidth).Append(':');
        foreach (var cvi in Arrangement) {
            if (cvi.Column is { IsDisposed: false }) {
                sb.Append(cvi.ControlColumnWidth()).Append('|');
            }
        }

        return sb.ToString();
    }

    /// <summary>
    /// Kopiert den Inhalt der Cursor-Zelle in die Zwischenablage und zeigt
    /// die Bestätigung an der Zell-Position an. Gemeinsame Logik für
    /// Ctrl+C (Kopieren) und Ctrl+X (Ausschneiden).
    /// </summary>
    private void CopyCellContent(ColumnViewItem cursorColumn, TableView tableView) {
        var cp = ControlPosition(tableView.Zoom, tableView.OffsetX, tableView.OffsetY);
        TableView.CopyToClipboard(cursorColumn.Column, Row, true, tableView.PointToScreen(new Point(cursorColumn.ControlColumnRight(tableView.OffsetX), cp.Y)));
    }

    #endregion
}