// Licensed under AGPL-3.0; see License.md for disclaimer and details.

using BlueControls.Controls;

namespace BlueControls.Classes.ItemCollectionList.TableItems;

/// <summary>
/// Enthält Daten, wie eine Zeile angezeigt werden soll.
/// RowData kann mehrfach in einer Tabelle angezeigt werden.
/// Ein RowItem ist einzigartig, kann aber in mehreren RowData enthalten sein.
/// </summary>
public abstract class RowBackgroundListItem : AbstractListItem, IStyleable {

    #region Fields

    public static readonly Brush GrayBrush = new SolidBrush(Color.FromArgb(80, 200, 200, 200));
    public static readonly Brush GrayBrush2 = new SolidBrush(Color.FromArgb(150, 255, 255, 255));

    #endregion

    #region Constructors

    protected RowBackgroundListItem(string keyname, ColumnViewCollection? arrangement, string alignsToCaption) : base(keyname, true) {
        Arrangement = arrangement;
        AlignsToChapter = alignsToCaption;
    }

    #endregion

    #region Properties

    /// <summary>
    /// Großschreibung
    /// </summary>
    public string AlignsToChapter {
        get;
        private set {
            value = value.ToUpperInvariant();
            if (field == value) { return; }
            field = value;
            OnPropertyChanged();
        }
    }

    public ColumnViewCollection? Arrangement {
        get;
        set {
            if (field == value) { return; }

            field = value;
            Invalidate_UntrimmedCanvasSize();
            OnPropertyChanged();
        }
    }

    [DefaultValue(Constants.Win11)]
    public string SheetStyle {
        get;
        set {
            if (IsDisposed) { return; }

            if (field == value) { return; }

            field = value;
            OnPropertyChanged();
        }
    } = Constants.Win11;

    /// <summary>
    /// True: Erst Non Permanent, dann Permanent
    /// False: Alle der Reihe Nach
    /// </summary>
    protected abstract bool DoSpezialOrder { get; }

    #endregion

    #region Methods

    public virtual void Draw_Border(Graphics gr, ColumnViewItem viewItem, ColumnLineStyle lin, float xPos, float top, float bottom) => DrawLine(gr, lin, xPos, xPos, top, bottom);

    //      viewItem.GetRenderer(SheetStyle).Draw(gr, toDrawd, cellInThisTableRow, positionControl, cellInThisTableColumn.DoOpticalTranslation, (Alignment)cellInThisTableColumn.Alignx, _zoom);
    public virtual void Draw_ColumnBackGround(Graphics gr, ColumnViewItem viewItem, RectangleF positionControl, States state) {
        if (viewItem.IsDummyColumn) {
            gr.FillRectangle(GrayBrush, positionControl);
            return;
        }
        var brush = BackgroundFill.GetBrush(viewItem.BackColor_ColumnCell);
        lock (brush) { gr.FillRectangle(brush, positionControl); }
    }

    public virtual void Draw_ColumnContent(Graphics gr, ColumnViewItem viewItem, RectangleF positionControl, float scale, TranslationType translate, float offsetX, float offsetY, States state) { }

    public virtual void Draw_ColumnOverlay(Graphics gr, ColumnViewItem viewItem, RectangleF positionControl, States state) { }

    public virtual void Draw_LowerLine(Graphics gr, ColumnViewItem viewItem, ColumnLineStyle lin, float left, float right, float bottom) => DrawLine(gr, lin, left, right, bottom, bottom);

    public virtual void Draw_UpperLine(Graphics gr, ColumnLineStyle lin, float left, float right, float bottom) => DrawLine(gr, lin, left, right, bottom, bottom);

    public void DrawLine(Graphics gr, ColumnLineStyle lin, float left, float right, float top, float bottom) {
        if (IsDisposed) { return; }

        try {
            switch (lin) {
                case ColumnLineStyle.Ohne:
                    break;

                case ColumnLineStyle.Dünn:
                    gr.DrawLine(Skin.PenLinieDünn, left, top, right, bottom);
                    break;

                case ColumnLineStyle.Kräftig:
                    gr.DrawLine(Skin.PenLinieKräftig, left, top, right, bottom);
                    break;

                case ColumnLineStyle.Dick:
                    gr.DrawLine(Skin.PenLinieDick, left, top, right, bottom);
                    break;

                case ColumnLineStyle.ShadowRight:
                    var c = Skin.Color_Border(Design.Table_Lines_Thick, States.Standard);
                    gr.DrawLine(Skin.PenLinieKräftig, left, top, right, bottom);
                    var sp1 = BorderDraw.GetPen(Color.FromArgb(80, c.R, c.G, c.B), 1);
                    var sp2 = BorderDraw.GetPen(Color.FromArgb(60, c.R, c.G, c.B), 1);
                    var sp3 = BorderDraw.GetPen(Color.FromArgb(40, c.R, c.G, c.B), 1);
                    var sp4 = BorderDraw.GetPen(Color.FromArgb(20, c.R, c.G, c.B), 1);
                    lock (sp1) { gr.DrawLine(sp1, left + 1, top, right + 1, bottom); }
                    lock (sp2) { gr.DrawLine(sp2, left + 2, top, right + 2, bottom); }
                    lock (sp3) { gr.DrawLine(sp3, left + 3, top, right + 3, bottom); }
                    lock (sp4) { gr.DrawLine(sp4, left + 4, top, right + 4, bottom); }
                    break;

                default:
                    Develop.DebugPrint(lin);
                    break;
            }
        } catch { }
    }

    public abstract string QuickInfoForColumn(ColumnViewItem cvi, int mouseXinColumn, int mouseYinColumn, float scale);

    public virtual bool HandleClick(ColumnViewCollection ca, ColumnViewItem clickedColumn, int mouseXinColumn, int mouseYinColumn, float zoom, TableView tableView) => false;

    protected override void Dispose(bool disposing) {
        if (disposing) {
            Arrangement = null;
        }
        base.Dispose(disposing);
    }

    protected override void DrawExplicit(Graphics gr, Rectangle visibleAreaControl, RectangleF positionControl, Design itemdesign, States state, bool drawBorderAndBack, bool translate, float offsetX, float offsetY, float zoom) {
        if (Arrangement == null) { return; }

        for (var du = 0; du < 2; du++) {
            foreach (var viewItem in Arrangement) {
                if (DoSpezialOrder && (viewItem.Permanent && du == 0 || !viewItem.Permanent && du == 1)) { continue; }
                if (viewItem.Column == null && !viewItem.IsDummyColumn) { continue; }

                var left = viewItem.ControlColumnLeft((int)offsetX);

                if (left > visibleAreaControl.Width) { continue; }
                if (left + (viewItem.ControlColumnWidth()) < 0) { continue; }

                var area = new Rectangle(left, (int)positionControl.Top, viewItem.ControlColumnWidth(), (int)positionControl.Height);

                if (Arrangement.Count == 1) { area = positionControl.ToRect(); }

                var t = viewItem.Column?.DoOpticalTranslation ?? TranslationType.Original_Anzeigen;
                if (!translate) { t = TranslationType.Original_Anzeigen; }

                if (!DoSpezialOrder) {
                    if (!viewItem.Permanent) {
                        area.X = Math.Max(area.X, Arrangement.ControlColumnsPermanentWidth());
                    }
                }

                if (state.HasFlag(States.Standard_HasFocus)) { state ^= States.Standard_HasFocus; }

                gr.SmoothingMode = SmoothingMode.None;
                Draw_ColumnBackGround(gr, viewItem, area, state);
                Draw_Border(gr, viewItem, viewItem.LineLeft, area.Left, area.Top, area.Bottom);
                Draw_Border(gr, viewItem, viewItem.LineRight, area.Right, area.Top, area.Bottom);
                Draw_UpperLine(gr, ColumnLineStyle.Ohne, area.Right, area.Left, area.Top);
                Draw_LowerLine(gr, viewItem, ColumnLineStyle.Dünn, area.Right, area.Left, area.Bottom - 1);
                Draw_ColumnContent(gr, viewItem, area, zoom, t, offsetX, offsetY, state);
                Draw_ColumnOverlay(gr, viewItem, area, state);
            }

            if (!DoSpezialOrder) { return; }
        }
    }

    protected override string GetCompareKey() => KeyName;

    #endregion
}