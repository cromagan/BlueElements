// Licensed under AGPL-3.0; see License.md for disclaimer and details.

using BlueControls.Controls;

namespace BlueControls.Classes.TableItems;

public sealed class EditBarListItem : RowBackground {

    #region Fields

    public const int ButtonCount = 2;
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
            0 => EditButtonType.Hide,
            1 => EditButtonType.Permanent,
            _ => EditButtonType.None
        };
    }

    public override void Draw_ColumnBackGround(Graphics gr, ColumnViewItem viewItem, RectangleF positionControl, States state, Brush? rowcolor) {
        base.Draw_ColumnBackGround(gr, viewItem, positionControl, state, rowcolor);
        gr.FillRectangle(TableHeadOverlayBrush, positionControl);
    }

    public override void Draw_ColumnContent(Graphics gr, ColumnViewItem viewItem, RectangleF positionControl, float scale, TranslationType translate, float offsetX, float offsetY, States state) {
        base.Draw_ColumnContent(gr, viewItem, positionControl, scale, translate, offsetX, offsetY, state);

        if (viewItem.Column is not { IsDisposed: false }) { return; }

        var canToggle = CanTogglePermanent(viewItem);

        var bs = ButtonSize.CanvasToControl(scale);
        var totalW = ButtonCount * bs;
        var startX = (int)((positionControl.Width - totalW) / 2);
        var startY = (int)((positionControl.Height - bs) / 2);

        var btnHide = new Rectangle((int)positionControl.Left + startX, (int)positionControl.Top + startY, bs, bs);
        var btnPerm = new Rectangle(btnHide.Right, (int)positionControl.Top + startY, bs, bs);

        Skin.Draw_Back(gr, Design.Button_AutoFilter, States.Standard, btnHide, null, false);
        Skin.Draw_Border(gr, Design.Button_AutoFilter, States.Standard, btnHide);

        gr.DrawImageUnscaled(QuickImage.Get(IsView0(viewItem) ? ImageCode.Papierkorb : ImageCode.Kreuz, bs - 4), btnHide.Left + 2, btnHide.Top + 2);

        if (canToggle) {
            Skin.Draw_Back(gr, Design.Button_AutoFilter, States.Standard, btnPerm, null, false);
            Skin.Draw_Border(gr, Design.Button_AutoFilter, States.Standard, btnPerm);

            var imgCode = viewItem.Permanent
                ? QuickImage.Get(ImageCode.Pinnadel, bs - 4)
                : QuickImage.Get("Pinnadel|" + (bs - 4) + "||||||||75");

            gr.DrawImageUnscaled(imgCode, btnPerm.Left + 2, btnPerm.Top + 2);
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
        if (parsedViewItem is null) { return false; }

        switch (btnType) {
            case EditButtonType.Hide:
                if (currentIdx == 0 && clickedColumn.Column is { IsDisposed: false } deletedColumn) {
                    if (MessageBox.Show($"Spalte <b>{deletedColumn.Caption}</b> wirklich löschen?", ImageCode.Frage, "Löschen", "Abbrechen") != 0) { return false; }
                    tableView.Table.Column.Remove(deletedColumn, "PowerEdit: Spalte gelöscht");
                    foreach (var arr in tcvc) {
                        if (arr[deletedColumn] is { } vi) { arr.Remove(vi); }
                    }
                    tableView.Table.ColumnArrangements = tcvc.AsReadOnly();
                    return true;
                }
                parsed.Remove(parsedViewItem);
                break;

            case EditButtonType.Permanent:
                if (!CanTogglePermanent(parsedViewItem, parsed)) { return false; }
                parsedViewItem.Permanent = !parsedViewItem.Permanent;
                break;
        }

        tableView.Table.ColumnArrangements = tcvc.AsReadOnly();
        return true;
    }

    public override int HeightInControl(ListBoxAppearance style, int columnWidth, Design itemdesign) => ButtonSize + 4;

    public override string QuickInfoForColumn(ColumnViewItem cvi, int mouseXinColumn, int mouseYinColumn, float scale) {
        var bt = GetButtonType(cvi, mouseXinColumn, scale);
        var isView0 = IsView0(cvi);

        return bt switch {
            EditButtonType.Hide => isView0 ? "Spalte PERMANENT löschen" : "Spalte ausblenden",
            EditButtonType.Permanent => CanTogglePermanent(cvi)
                ? (cvi.Permanent ? "Spalte nicht mehr fixieren" : "Spalte fixieren")
                : string.Empty,
            _ => string.Empty
        };
    }

    protected override Size ComputeUntrimmedCanvasSize(Design itemdesign) => new(ButtonSize * ButtonCount, ButtonSize + 4);

    private static bool CanTogglePermanent(ColumnViewItem viewItem, ColumnViewCollection? arrangement) {
        if (arrangement is null || viewItem.Column is null) { return false; }

        var myIdx = arrangement.IndexOf(viewItem);
        if (myIdx < 0) { return false; }

        if (viewItem.Permanent) {
            // Nur die letzte permanente Spalte darf umgeschaltet werden:
            // keine weitere permanente Spalte darf dahinter liegen.
            for (var i = myIdx + 1; i < arrangement.Count; i++) {
                if (arrangement[i] is { Column: not null, Permanent: true }) { return false; }
            }
            return true;
        }

        // Nur die erste nicht-permanente Spalte darf umgeschaltet werden:
        // keine weitere nicht-permanente Spalte darf davor liegen.
        for (var i = 0; i < myIdx; i++) {
            if (arrangement[i] is { Column: not null, Permanent: false }) { return false; }
        }
        return true;
    }

    private bool CanTogglePermanent(ColumnViewItem viewItem) => CanTogglePermanent(viewItem, Arrangement);

    private bool IsView0(ColumnViewItem viewItem) {
        if (viewItem.Column?.Table is not { IsDisposed: false } table) { return false; }
        return table.ColumnArrangements.Count > 0 &&
               string.Equals(table.ColumnArrangements[0].KeyName, Arrangement?.KeyName, StringComparison.OrdinalIgnoreCase);
    }

    #endregion
}