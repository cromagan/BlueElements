// Licensed under AGPL-3.0; see License.md for disclaimer and details.

using BlueControls.Controls;

namespace BlueControls.Classes.ItemCollectionList.TableItems;

public sealed class EditBarListItem : RowBackgroundListItem {

    #region Fields

    public const int ButtonCount = 3;
    public const int ButtonSize = 16;
    public const string Identifier = "EditBarListItem";

    #endregion

    #region Constructors

    public EditBarListItem(ColumnViewCollection? arrangement) : base(Identifier, arrangement, string.Empty) => IgnoreYOffset = true;

    #endregion

    #region Properties

    protected override bool DoSpezialOrder => true;

    #endregion

    #region Methods

    public static EditButtonType GetButtonType(ColumnViewItem viewItem, int mouseXinColumn, float scale) {
        var bs = ButtonSize.CanvasToControl(scale);
        var totalW = ButtonCount * bs;
        var startX = (viewItem.ControlColumnWidth() - totalW) / 2;

        var relX = mouseXinColumn - startX;

        if (relX < 0 || relX >= totalW) { return EditButtonType.None; }

        var index = relX / bs;

        return index switch {
            0 => EditButtonType.MoveLeft,
            1 => EditButtonType.Hide,
            2 => EditButtonType.MoveRight,
            _ => EditButtonType.None
        };
    }

    public override void Draw_ColumnBackGround(Graphics gr, ColumnViewItem viewItem, RectangleF positionControl, States state) {
        base.Draw_ColumnBackGround(gr, viewItem, positionControl, state);
        gr.FillRectangle(GrayBrush, positionControl);
    }

    public override void Draw_ColumnContent(Graphics gr, ColumnViewItem viewItem, RectangleF positionControl, float scale, TranslationType translate, float offsetX, float offsetY, States state) {
        base.Draw_ColumnContent(gr, viewItem, positionControl, scale, translate, offsetX, offsetY, state);

        if (viewItem.Column is not { IsDisposed: false }) { return; }

        var (isFirst, isLast) = IsAtEdge(viewItem);

        var bs = ButtonSize.CanvasToControl(scale);
        var totalW = ButtonCount * bs;
        var startX = (int)((positionControl.Width - totalW) / 2);
        var startY = (int)((positionControl.Height - bs) / 2);

        var btnLeft = new Rectangle((int)positionControl.Left + startX, (int)positionControl.Top + startY, bs, bs);
        var btnHide = new Rectangle(btnLeft.Right, (int)positionControl.Top + startY, bs, bs);
        var btnRight = new Rectangle(btnHide.Right, (int)positionControl.Top + startY, bs, bs);

        if (!isFirst) {
            Skin.Draw_Back(gr, Design.Button_AutoFilter, States.Standard, btnLeft, null, false);
            Skin.Draw_Border(gr, Design.Button_AutoFilter, States.Standard, btnLeft);
            gr.DrawImageUnscaled(QuickImage.Get("Pfeil_Links|" + (bs - 4)), btnLeft.Left + 2, btnLeft.Top + 2);
        }

        Skin.Draw_Back(gr, Design.Button_AutoFilter, States.Standard, btnHide, null, false);
        Skin.Draw_Border(gr, Design.Button_AutoFilter, States.Standard, btnHide);
        gr.DrawImageUnscaled(QuickImage.Get(ImageCode.Kreuz, bs - 4), btnHide.Left + 2, btnHide.Top + 2);

        if (!isLast) {
            Skin.Draw_Back(gr, Design.Button_AutoFilter, States.Standard, btnRight, null, false);
            Skin.Draw_Border(gr, Design.Button_AutoFilter, States.Standard, btnRight);
            gr.DrawImageUnscaled(QuickImage.Get("Pfeil_Rechts|" + (bs - 4)), btnRight.Left + 2, btnRight.Top + 2);
        }
    }

    public override void Draw_LowerLine(Graphics gr, ColumnViewItem viewItem, ColumnLineStyle lin, float left, float right, float bottom) => base.Draw_LowerLine(gr, viewItem, ColumnLineStyle.Ohne, left, right, bottom);

    public override bool HandleClick(ColumnViewCollection ca, ColumnViewItem clickedColumn, int mouseXinColumn, int mouseYinColumn, float zoom, TableView tableView) {
        if (tableView.Table is not { IsDisposed: false }) { return false; }

        var btnType = GetButtonType(clickedColumn, mouseXinColumn, zoom);
        if (btnType == EditButtonType.None) { return false; }

        var tcvc = ColumnViewCollection.ParseAll(tableView.Table);
        var currentIdx = -1;
        for (var i = 0; i < tcvc.Count; i++) {
            if (string.Equals(tcvc[i].KeyName, ca.KeyName, StringComparison.OrdinalIgnoreCase)) {
                currentIdx = i;
                break;
            }
        }
        if (currentIdx < 0) { return false; }

        var parsed = tcvc[currentIdx];
        var parsedViewItem = parsed[clickedColumn.Column];
        if (parsedViewItem == null) { return false; }
        var viewIdx = parsed.IndexOf(parsedViewItem);

        switch (btnType) {
            case EditButtonType.MoveLeft:
                if (viewIdx > 0) {
                    parsed.Swap(viewIdx, viewIdx - 1);
                }
                break;

            case EditButtonType.MoveRight:
                if (viewIdx < parsed.Count - 1) {
                    parsed.Swap(viewIdx, viewIdx + 1);
                }
                break;

            case EditButtonType.Hide:
                if (currentIdx == 0) {
                    if (Forms.MessageBox.Show($"Spalte <b>{clickedColumn.Column.Caption}</b> wirklich löschen?", ImageCode.Frage, "Löschen", "Abbrechen") != 0) { return false; }
                    tableView.Table.Column.Remove(clickedColumn.Column, "PowerEdit: Spalte gelöscht");
                    return true;
                }
                parsed.Remove(parsedViewItem);
                break;
        }

        tableView.Table.ColumnArrangements = tcvc.AsReadOnly();
        return true;
    }

    public override int HeightInControl(ListBoxAppearance style, int columnWidth, Design itemdesign) => ButtonSize + 4;

    public override string QuickInfoForColumn(ColumnViewItem cvi, int mouseXinColumn, int mouseYinColumn, float scale) {
        var bt = GetButtonType(cvi, mouseXinColumn, scale);
        var isView0 = cvi.Column?.Table is { IsDisposed: false } table &&
                      table.ColumnArrangements.Count > 0 &&
                      string.Equals(table.ColumnArrangements[0].KeyName, Arrangement?.KeyName, StringComparison.OrdinalIgnoreCase);

        var (isFirst, isLast) = IsAtEdge(cvi);

        return bt switch {
            EditButtonType.MoveLeft => isFirst ? string.Empty : "Spalte nach links verschieben",
            EditButtonType.Hide => isView0 ? "Spalte PERMANENT löschen" : "Spalte ausblenden",
            EditButtonType.MoveRight => isLast ? string.Empty : "Spalte nach rechts verschieben",
            _ => string.Empty
        };
    }

    protected override Size ComputeUntrimmedCanvasSize(Design itemdesign) => new(ButtonSize * ButtonCount, ButtonSize + 4);

    private (bool isFirst, bool isLast) IsAtEdge(ColumnViewItem viewItem) {
        if (Arrangement == null || viewItem.Column == null) { return (true, true); }
        var myIdx = Arrangement.IndexOf(viewItem);
        if (myIdx < 0) { return (true, true); }
        var isPermanent = viewItem.Permanent;
        var isFirst = true;
        var isLast = true;
        for (var i = 0; i < Arrangement.Count; i++) {
            var item = Arrangement[i];
            if (item?.Column == null) { continue; }
            if (item.Permanent != isPermanent) { continue; }
            if (i < myIdx) { isFirst = false; }
            if (i > myIdx) { isLast = false; }
        }
        return (isFirst, isLast);
    }

    #endregion
}