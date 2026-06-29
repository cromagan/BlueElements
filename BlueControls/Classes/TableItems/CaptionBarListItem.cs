// Licensed under AGPL-3.0; see License.md for disclaimer and details.

using BlueControls.Controls;

namespace BlueControls.Classes.TableItems;

public sealed class CaptionBarListItem : RowBackground {

    #region Fields

    public static readonly int CaptionHeight = 22;

    private string prevCaptionGroup = string.Empty;

    private ColumnViewItem? prevViewItem;

    private ColumnViewItem? prevViewItemWithOtherCaption;
    private int prevViewItemWithOtherCaptionLe;

    #endregion

    #region Constructors

    public CaptionBarListItem(ColumnViewCollection? arrangement, int caption) : base(Identifier(caption), arrangement, string.Empty) {
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
        gr.FillRectangle(GrayBrush, positionControl);
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

    internal void EditCaptionGroup(ColumnViewItem viewItem, TableView tableView) {
        if (viewItem.Column is not { IsDisposed: false } col) { return; }
        if (tableView.Table is not { IsDisposed: false }) { return; }
        if (Arrangement is null) { return; }

        var headPos = ControlPosition(tableView.Zoom, tableView.OffsetX, tableView.OffsetY);
        var colX = viewItem.ControlColumnLeft(tableView.OffsetX);
        var colW = viewItem.ControlColumnWidth();

        // Alle Überschriften-Texte aller Ebenen sammeln (für die Dropdown-Auswahl).
        // Auch die Werte der anderen Ebenen werden angeboten.
        List<string> suggestions = [];
        foreach (var thisC in Arrangement) {
            if (thisC.Column is not { IsDisposed: false } c) { continue; }
            for (var z = 0; z < 3; z++) {
                if (c.CaptionGroup(z) is { Length: > 0 } g) { suggestions.AddIfNotExists(g); }
            }
        }

        var bt = tableView.BTS;
        bt.GetStyleFrom(ColumnFormatHolder_TextMultiline.Instance);
        bt.MultiLine = true;
        bt.Suggestions = suggestions.AsReadOnly();
        bt.Text = col.CaptionGroup(Caption).Replace("\r", "\r\n");
        bt.TextboxSize = new Size(colW, headPos.Height);
        bt.Location = new Point(colX, headPos.Y);
        bt.Size = new Size(colW, bt.GetEstimatedHeight(colW, headPos.Height));
        bt.Tag = (List<object?>)[viewItem, this, "CaptionGroupEdit", Caption];
        bt.Verhalten = SteuerelementVerhalten.Scrollen_ohne_Textumbruch;
        bt.Visible = true;
        bt.BringToFront();
        bt.Focus();
    }

    protected override Size ComputeUntrimmedCanvasSize(Design itemdesign) => new(CaptionHeight, CaptionHeight);

    private void Draw_Column_Head_Captions_Now(Graphics gr, RectangleF positionControlOfNextItem, string prevCaptionGroup, float _zoom) {
        var isEdit = Arrangement?.Ansichtbearbeitung ?? false;

        if (isEdit) {
            if (string.IsNullOrEmpty(prevCaptionGroup)) { return; }
            var capTranslated = ColumnsHeadListItem.CaptionTranslated(prevCaptionGroup);
            var r = new RectangleF(positionControlOfNextItem.Left + 4, positionControlOfNextItem.Top, positionControlOfNextItem.Width - 8, positionControlOfNextItem.Height);
            Skin.Draw_FormatedText(gr, capTranslated, null, Alignment.Horizontal_Vertical_Center, r.ToRect(), null, false, Font_Head_Default.Scale(_zoom), false);
            return;
        }

        if (prevViewItemWithOtherCaptionLe < positionControlOfNextItem.Left) {
            var capTranslated = ColumnsHeadListItem.CaptionTranslated(prevCaptionGroup);

            var r = new RectangleF(prevViewItemWithOtherCaptionLe, positionControlOfNextItem.Top, positionControlOfNextItem.Left - prevViewItemWithOtherCaptionLe, positionControlOfNextItem.Height);
            //gr.FillRectangle(new SolidBrush(prevViewItemWithOtherCaption.BackColor_ColumnHead), r);
            //gr.FillRectangle(new SolidBrush(Color.FromArgb(80, 200, 200, 200)), r);
            gr.DrawRectangle(Skin.PenLinieKräftig, r);
            Skin.Draw_FormatedText(gr, capTranslated, null, Alignment.Horizontal_Vertical_Center, r.ToRect(), null, false, Font_Head_Default.Scale(_zoom), false);
        }
    }

    #endregion
}