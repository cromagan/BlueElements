// Licensed under MIT; see License.md for disclaimer, details, and extended user conditions.

using BlueControls.Controls;
using BlueControls.ControlStrategies;
using BlueControls.EventArgs;
using BlueTable.ColumnFormats;
using System.Windows.Forms;

namespace BlueControls.TableElements;

public sealed class CaptionBarListItemTableElement : TableElement {

    #region Fields

    public static readonly int CaptionHeight = 22;

    /// <summary>
    /// Sichtbare Breite des Zeichenbereichs während des laufenden Paint-Vorgangs.
    /// </summary>
    private int paintVisibleWidth;

    private string prevCaptionGroup = string.Empty;

    private ColumnViewItem? prevViewItem;

    private ColumnViewItem? prevViewItemWithOtherCaption;
    private int prevViewItemWithOtherCaptionLe;

    #endregion

    #region Constructors

    public CaptionBarListItemTableElement(ColumnViewCollection? arrangement, int caption) : base(Identifier(caption), arrangement, string.Empty) {
        IgnoreYOffset = true;
        Caption = caption;
    }

    #endregion

    #region Properties

    public int Caption { get; }

    public BlueFont Font_Head_Default => Skin.GetBlueFont(SheetStyle, PadStyles.Emphasized);

    protected override bool DoSpezialOrder => false;

    #endregion

    #region Methods

    public static string Identifier(int captionRow) => $"CaptionBar{captionRow}";

    public override void Draw_Border(Graphics gr, ColumnViewItem viewItem, ColumnLineStyle lin, float xPos, float top, float bottom) {
        var newCaptionGroup = viewItem.Column?.CaptionGroup(Caption) ?? string.Empty;
        var isEdit = Arrangement?.Ansichtbearbeitung ?? false;

        if (isEdit) {
            base.Draw_Border(gr, viewItem, ColumnLineStyle.Dünn, xPos, top, bottom);
            return;
        }

        if (string.IsNullOrEmpty(newCaptionGroup) || string.IsNullOrEmpty(prevCaptionGroup)) {
            base.Draw_Border(gr, viewItem, lin, xPos, top, bottom);
        } else {
            base.Draw_Border(gr, viewItem, ColumnLineStyle.Ohne, xPos, top, bottom);
        }
    }

    public override void Draw_ColumnBackGround(Graphics gr, ColumnViewItem viewItem, RectangleF positionControl, States state, Brush? rowcolor) {
        base.Draw_ColumnBackGround(gr, viewItem, positionControl, state, rowcolor);
        gr.FillRectangle(TableHeadOverlayBrush, positionControl);
    }

    public override void Draw_ColumnContent(Graphics gr, ColumnViewItem viewItem, RectangleF positionControl, float scale, TranslationType translate, float offsetX, float offsetY, States state) {
        base.Draw_ColumnContent(gr, viewItem, positionControl, scale, translate, offsetX, offsetY, state);
        var newCaptionGroup = viewItem.Column?.CaptionGroup(Caption) ?? string.Empty;
        var isEdit = Arrangement?.Ansichtbearbeitung ?? false;

        if (isEdit) {
            Draw_Column_Head_Captions_Now(gr, positionControl, newCaptionGroup, scale, int.MaxValue);
        } else if (newCaptionGroup != prevCaptionGroup) {

            #region Ende einer Gruppierung gefunden

            if (!string.IsNullOrEmpty(prevCaptionGroup) && prevViewItem is { IsDisposed: false } && prevViewItemWithOtherCaption is not null) {
                Draw_Column_Head_Captions_Now(gr, positionControl, prevCaptionGroup, scale, paintVisibleWidth);
            }

            prevViewItemWithOtherCaption = viewItem;
            prevViewItemWithOtherCaptionLe = (int)positionControl.Left;

            #endregion
        }

        prevViewItem = viewItem;
        prevCaptionGroup = newCaptionGroup;
    }

    public override void Draw_LowerLine(Graphics gr, ColumnViewItem viewItem, ColumnLineStyle lin, float left, float right, float bottom) {
        var newCaptionGroup = viewItem.Column?.CaptionGroup(Caption) ?? string.Empty;
        var isEdit = Arrangement?.Ansichtbearbeitung ?? false;

        if (isEdit) {
            base.Draw_LowerLine(gr, viewItem, ColumnLineStyle.Dünn, left, right, bottom);
            return;
        }

        if (string.IsNullOrEmpty(newCaptionGroup)) {
            base.Draw_LowerLine(gr, viewItem, ColumnLineStyle.Ohne, left, right, bottom);
        } else {
            base.Draw_LowerLine(gr, viewItem, ColumnLineStyle.Kräftig, left, right, bottom);
        }
    }

    /// <summary>
    /// Übernimmt die gesamte Doppelklick-Logik für die Caption-Bar:
    /// prüft Ansichtbearbeitung und Admin-Berechtigung, wählt die
    /// Anker-Spalte (geklickt oder Fallback erstes Arrangement-Element),
    /// sammelt Suggestions aus allen Ebenen und startet die Editierung
    /// der Gruppen-Überschrift über TableView.BeginEdit.
    /// </summary>
    public override bool HandleDoubleClick(ColumnViewItem? mouseOverColumn, TableView tableView) {
        if (!(Arrangement?.Ansichtbearbeitung ?? false)) { return false; }
        if (Arrangement?.Table is not { IsDisposed: false } tb || !tb.IsAdministrator()) { return false; }
        if (Arrangement is not { IsDisposed: false } arrangement) { return false; }

        var anchor = mouseOverColumn is { IsDisposed: false } cvi && cvi.Column is { IsDisposed: false }
            ? cvi
            : arrangement.FirstOrDefault(x => x is { IsDisposed: false } && x.Column is { IsDisposed: false });
        if (anchor is not { IsDisposed: false } || anchor.Column is not { IsDisposed: false } col) { return false; }

        // Suggestions aus allen Ebenen der Arrangement-Spalten sammeln.
        List<string> suggestions = [];
        foreach (var thisC in arrangement) {
            if (thisC.Column is not { IsDisposed: false } c) { continue; }
            for (var z = 0; z < 3; z++) {
                if (c.CaptionGroup(z) is { Length: > 0 } g) { suggestions.AddIfNotExists(g); }
            }
        }

        var headPos = ControlPosition(tableView.Zoom, tableView.OffsetX, tableView.OffsetY);
        var colX = anchor.ControlColumnLeft(tableView.OffsetX);
        var colW = anchor.ControlColumnWidth();

        tableView.BeginEdit(
            TextBoxSuggestionsControlStrategy.ClassId,
            new Rectangle(colX, headPos.Y, colW, headPos.Height),
            col.CaptionGroup(Caption).Replace("\r", "\r\n"),
            v => ApplyCaptionGroup(tableView, anchor, v),
            TextMultilineColumnFormat.Instance,
            null,
            null,
            ItemsOf(suggestions),
            null,
            Font_Head_Default);
        return true;
    }

    public override void HandleMouseMove(ColumnViewItem? mouseOverColumn, TableView tableView, CanvasMouseEventArgs e) {
        if (mouseOverColumn is not { IsDisposed: false } cvi || e.Button != MouseButtons.None) {
            base.HandleMouseMove(mouseOverColumn, tableView, e);
            return;
        }

        var group = cvi.Column?.CaptionGroup(Caption) ?? string.Empty;
        if (Arrangement?.Ansichtbearbeitung ?? false) {
            tableView.QuickInfo = string.IsNullOrEmpty(group)
                ? $"Überschrift Ebene {Caption + 1}: leer (Doppelklick zum Bearbeiten)"
                : $"Überschrift Ebene {Caption + 1}: {group}\rDoppelklick zum Bearbeiten";
            return;
        }

        tableView.QuickInfo = string.IsNullOrEmpty(group) ? string.Empty : "Gruppierung: " + group;
    }

    public override int HeightInControl(ListBoxAppearance style, int columnWidth, Design itemdesign) => CaptionHeight;

    protected override Size ComputeUntrimmedCanvasSize(Design itemdesign) => new(CaptionHeight, CaptionHeight);

    /// <summary>
    /// Schließt nach dem Zeichnen aller Spalten die letzte Gruppierung ab.
    /// Diese endet an der rechten Kante der letzten Spalte und besitzt keine
    /// Folge-Spalte, die das Zeichnen im Spalten-Loop auslösen würde.
    /// </summary>
    protected override void DrawExplicit(Graphics gr, Rectangle visibleAreaControl, RectangleF positionControl, Design itemdesign, States state, bool drawBorderAndBack, bool translate, float offsetX, float offsetY, float zoom) {
        paintVisibleWidth = visibleAreaControl.Width;
        base.DrawExplicit(gr, visibleAreaControl, positionControl, itemdesign, state, drawBorderAndBack, translate, offsetX, offsetY, zoom);

        if (Arrangement is not { IsDisposed: false, Ansichtbearbeitung: false } arrangement) { return; }
        if (string.IsNullOrEmpty(prevCaptionGroup) || prevViewItem is not { IsDisposed: false } lastColumn || prevViewItemWithOtherCaption is null) { return; }

        var items = arrangement.RenderingItems.ToList();
        var lastIndex = items.IndexOf(lastColumn);
        if (lastIndex < 0) { return; }

        // Rechte Kante der Gruppierung — auch über noch nicht gezeichnete
        // (außerhalb des sichtbaren Bereichs liegende) Spalten hinaus suchen.
        var right = lastColumn.ControlColumnRight(offsetX);
        for (var i = lastIndex + 1; i < items.Count; i++) {
            if (items[i] is not { IsDisposed: false } item || item.Column?.CaptionGroup(Caption) != prevCaptionGroup) { break; }
            right = item.ControlColumnRight(offsetX);
        }

        // Endet die Gruppierung an lastColumn und wurde die Folge-Spalte bereits
        // gezeichnet, hat der Spalten-Loop die Überschrift schon abgeschlossen.
        if (right == lastColumn.ControlColumnRight(offsetX) && lastIndex + 1 < items.Count) {
            var next = items[lastIndex + 1];
            if (next is { IsDisposed: false } && next.ControlColumnLeft(offsetX) <= visibleAreaControl.Width && next.ControlColumnLeft(offsetX) + next.ControlColumnWidth() >= 0) { return; }
        }

        Draw_Column_Head_Captions_Now(gr, new RectangleF(right, positionControl.Top, 0, positionControl.Height), prevCaptionGroup, zoom, visibleAreaControl.Width);
    }

    /// <summary>
    /// Übernimmt die neue Überschrift der Caption-Gruppe (Ebene
    /// Caption). Commit-Callback aus
    /// HandleDoubleClick, der an
    /// TableView.BeginEdit übergeben wird.
    /// </summary>
    private void ApplyCaptionGroup(TableView tableView, ColumnViewItem? column, string value) {
        if (column?.Column is not { IsDisposed: false } col) { return; }

        var newGroup = value.Replace("\r\n", "\r").Trim();
        switch (Caption) {
            case 0:
                col.CaptionGroup1 = newGroup;
                break;

            case 1:
                col.CaptionGroup2 = newGroup;
                break;

            case 2:
                col.CaptionGroup3 = newGroup;
                break;

            default:
                Develop.DebugPrint("Ungültiger CaptionIndex: " + Caption);
                break;
        }
        tableView.InvalidateCurrentArrangement();
    }

    private void Draw_Column_Head_Captions_Now(Graphics gr, RectangleF positionControlOfNextItem, string prevCaptionGroup, float _zoom, int visibleWidth) {
        var isEdit = Arrangement?.Ansichtbearbeitung ?? false;

        RectangleF r;
        if (isEdit) {
            if (string.IsNullOrEmpty(prevCaptionGroup)) { return; }
            r = new RectangleF(positionControlOfNextItem.Left + 4, positionControlOfNextItem.Top, positionControlOfNextItem.Width - 8, positionControlOfNextItem.Height);
        } else {
            if (prevViewItemWithOtherCaptionLe >= positionControlOfNextItem.Left) { return; }
            r = new RectangleF(prevViewItemWithOtherCaptionLe, positionControlOfNextItem.Top, positionControlOfNextItem.Left - prevViewItemWithOtherCaptionLe, positionControlOfNextItem.Height);
        }

        var capTranslated = ColumnsHeadTableElement.CaptionTranslated(prevCaptionGroup);

        // Reicht die Gruppierung über den rechten Rand hinaus, den Text
        // mittig in der sichtbaren Restbreite einpassen.
        var visibleLeft = Math.Max(r.Left, 0);
        var visibleRight = Math.Min(r.Right, visibleWidth);
        if (visibleRight <= visibleLeft) { return; }
        var rText = new RectangleF(visibleLeft, r.Top, visibleRight - visibleLeft, r.Height);

        if (!isEdit) { gr.DrawRectangle(Skin.PenLinieKräftig, r); }
        Skin.Draw_FormatedText(gr, capTranslated, null, Alignment.Horizontal_Vertical_Center, rText.ToRect(), null, false, Font_Head_Default.Scale(_zoom), false);
    }

    #endregion
}