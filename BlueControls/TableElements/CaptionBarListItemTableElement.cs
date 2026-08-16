// Licensed under AGPL-3.0; see License.md for disclaimer and details.

using BlueControls.Controls;
using BlueControls.ControlStrategies;
using BlueTable.ColumnFormats;

namespace BlueControls.TableElements;

public sealed class CaptionBarListItemTableElement : TableElement {

    #region Fields

    public static readonly int CaptionHeight = 22;

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

    public int Caption { get; private set; }

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
            Draw_Column_Head_Captions_Now(gr, positionControl, newCaptionGroup, scale);
        } else if (newCaptionGroup != prevCaptionGroup) {

            #region Ende einer Gruppierung gefunden

            if (!string.IsNullOrEmpty(prevCaptionGroup) && prevViewItem is { IsDisposed: false } && prevViewItemWithOtherCaption is not null) {
                Draw_Column_Head_Captions_Now(gr, positionControl, prevCaptionGroup, scale);
            }

            prevViewItemWithOtherCaption = viewItem;
            prevViewItemWithOtherCaptionLe = (int)positionControl.Left;

            #endregion
        }

        prevViewItem = viewItem;
        prevCaptionGroup = newCaptionGroup;

        if (!isEdit) {
            // Zeichen-Routine für das letzte Element aufrufen
            if (!string.IsNullOrEmpty(prevCaptionGroup) && prevViewItem is { IsDisposed: false } && prevViewItemWithOtherCaption is not null) {
                Draw_Column_Head_Captions_Now(gr, Rectangle.Empty, prevCaptionGroup, scale);
            }
        }
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
    /// der Gruppen-Überschrift über <see cref="TableView.BeginEdit" />.
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
            null);
        return true;
    }

    /// <summary>
    /// Übernimmt die neue Überschrift der Caption-Gruppe (Ebene
    /// <see cref="Caption" />). Commit-Callback aus
    /// <see cref="HandleDoubleClick" />, der an
    /// <see cref="TableView.BeginEdit" /> übergeben wird.
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
        tableView.Invalidate_CurrentArrangement();
    }

    public override int HeightInControl(ListBoxAppearance style, int columnWidth, Design itemdesign) => CaptionHeight;

    public override string QuickInfoForColumn(ColumnViewItem cvi, int mouseXinColumn, int mouseYinColumn, float scale) {
        var group = cvi.Column?.CaptionGroup(Caption) ?? string.Empty;
        if (Arrangement?.Ansichtbearbeitung ?? false) {
            return string.IsNullOrEmpty(group)
                ? $"Überschrift Ebene {Caption + 1}: leer (Doppelklick zum Bearbeiten)"
                : $"Überschrift Ebene {Caption + 1}: {group}\rDoppelklick zum Bearbeiten";
        }
        return string.IsNullOrEmpty(group) ? string.Empty : "Gruppierung: " + group;
    }

    protected override Size ComputeUntrimmedCanvasSize(Design itemdesign) => new(CaptionHeight, CaptionHeight);

    private void Draw_Column_Head_Captions_Now(Graphics gr, RectangleF positionControlOfNextItem, string prevCaptionGroup, float _zoom) {
        var isEdit = Arrangement?.Ansichtbearbeitung ?? false;

        if (isEdit) {
            if (string.IsNullOrEmpty(prevCaptionGroup)) { return; }
            var capTranslated = ColumnsHeadTableElement.CaptionTranslated(prevCaptionGroup);
            var r = new RectangleF(positionControlOfNextItem.Left + 4, positionControlOfNextItem.Top, positionControlOfNextItem.Width - 8, positionControlOfNextItem.Height);
            Skin.Draw_FormatedText(gr, capTranslated, null, Alignment.Horizontal_Vertical_Center, r.ToRect(), null, false, Font_Head_Default.Scale(_zoom), false);
            return;
        }

        if (prevViewItemWithOtherCaptionLe < positionControlOfNextItem.Left) {
            var capTranslated = ColumnsHeadTableElement.CaptionTranslated(prevCaptionGroup);

            var r = new RectangleF(prevViewItemWithOtherCaptionLe, positionControlOfNextItem.Top, positionControlOfNextItem.Left - prevViewItemWithOtherCaptionLe, positionControlOfNextItem.Height);
            //gr.FillRectangle(new SolidBrush(prevViewItemWithOtherCaption.BackColor_ColumnHead), r);
            //gr.FillRectangle(new SolidBrush(Color.FromArgb(80, 200, 200, 200)), r);
            gr.DrawRectangle(Skin.PenLinieKräftig, r);
            Skin.Draw_FormatedText(gr, capTranslated, null, Alignment.Horizontal_Vertical_Center, r.ToRect(), null, false, Font_Head_Default.Scale(_zoom), false);
        }
    }

    #endregion
}