// Licensed under AGPL-3.0; see License.md for disclaimer and details.

using BlueControls.Classes.ItemCollectionPad;

namespace BlueControls.Classes.ItemCollectionList;

public class PagePreviewListItem : AbstractListItem {

    #region Fields

    private const int ConstBadgeHPad = 6;
    private const int ConstBadgeVPad = 2;
    private const int ConstPadding = 8;
    private const int ConstTopPadding = 4;
    private string _caption;
    private Bitmap? _tmpBmp;

    #endregion

    #region Constructors

    public PagePreviewListItem(ItemCollectionPadItem page) : base(page.KeyName, true) {
        _caption = page.Caption;
        Page = page;
    }

    #endregion

    #region Properties

    public string Caption {
        get => _caption;
        set {
            if (_caption == value) { return; }
            _caption = value;
            OnPropertyChanged();
        }
    }

    public ItemCollectionPadItem Page { get; }

    #endregion

    #region Methods

    public override int HeightInControl(ListBoxAppearance style, int columnWidth, Design itemdesign) {
        if (_tmpBmp is null) { GeneratePic(); }
        if (_tmpBmp is null) { return columnWidth + ConstTopPadding + ConstPadding; }

        var badgeH = (int)Math.Ceiling(BadgeSize().Height);

        var sc = (float)_tmpBmp.Height / _tmpBmp.Width;
        if (sc > 1) { sc = 1; }

        return (int)(sc * (columnWidth - ConstPadding * 2)) + ConstTopPadding + badgeH * 2 / 3 + ConstPadding;
    }

    public void RefreshPreview() {
        RemovePic();
    }

    protected override Size ComputeUntrimmedCanvasSize(Design itemdesign) {
        try {
            if (_tmpBmp is null) { GeneratePic(); }
            if (_tmpBmp is null) { return new Size(300, 300 + ConstTopPadding + ConstPadding); }

            var badgeH = (int)Math.Ceiling(BadgeSize().Height);

            var sc = (float)_tmpBmp.Height / _tmpBmp.Width;
            if (sc > 1) { sc = 1; }

            return new Size(300, (int)(sc * (300 - ConstPadding * 2)) + ConstTopPadding + badgeH * 2 / 3 + ConstPadding);
        } catch {
            Develop.AbortAppIfStackOverflow();
            return ComputeUntrimmedCanvasSize(itemdesign);
        }
    }

    protected override void Dispose(bool disposing) {
        if (disposing) {
            RemovePic();
        }
        base.Dispose(disposing);
    }

    protected override void DrawExplicit(Graphics gr, Rectangle visibleAreaControl, RectangleF positionControl, Design itemdesign, States state, bool drawBorderAndBack, bool translate, float offsetX, float offsetY, float zoom) {
        if (_tmpBmp is null) { GeneratePic(); }

        if (drawBorderAndBack) {
            Skin.Draw_Back(gr, itemdesign, state, positionControl.ToRect(), null, false);
        }

        // Weißes Vorschaufeld: oben Platz für das zu 2/3 darüber stehende Titel-Badge plus Top-Padding.
        var badge = BadgeSize();
        var topOffset = ConstTopPadding + badge.Height * 2f / 3f;
        var previewArea = new RectangleF(
            positionControl.Left + ConstPadding,
            positionControl.Top + topOffset,
            positionControl.Width - ConstPadding * 2,
            positionControl.Height - topOffset - ConstPadding);

        gr.FillRectangle(Brushes.White, previewArea);

        RectangleF imgRect = RectangleF.Empty;

        if (_tmpBmp is not null) {
            var sc = (float)Math.Min(previewArea.Width / (double)_tmpBmp.Width, previewArea.Height / (double)_tmpBmp.Height);
            var imgW = _tmpBmp.Width * sc;
            var imgH = _tmpBmp.Height * sc;
            imgRect = new RectangleF(
                previewArea.Left + (previewArea.Width - imgW) / 2,
                previewArea.Top + (previewArea.Height - imgH) / 2,
                imgW, imgH);

            gr.DrawImage(_tmpBmp, imgRect, new RectangleF(0, 0, _tmpBmp.Width, _tmpBmp.Height), GraphicsUnit.Pixel);

            gr.DrawRectangle(Pens.DarkGray, imgRect.Left, imgRect.Top, imgRect.Width, imgRect.Height);
        }

        gr.DrawRectangle(Pens.LightGray, previewArea.Left, previewArea.Top, previewArea.Width, previewArea.Height);

        // Titel als Badge mittig über dem Bild, zu 1/3 überlappend und nur so breit wie die Schrift.
        if (!imgRect.IsEmpty) {
            var badgeRect = new RectangleF(
                imgRect.Left + (imgRect.Width - badge.Width) / 2,
                imgRect.Top - badge.Height * 2f / 3f,
                badge.Width,
                badge.Height);

            Skin.Draw_Back(gr, Design.Form_QuickInfo, States.Standard, badgeRect.ToRect(), null, false);
            Skin.Draw_FormatedText(gr, _caption, null, Alignment.Horizontal_Vertical_Center, badgeRect.ToRect(), Design.Form_QuickInfo, States.Standard, null, false, false);
            Skin.Draw_Border(gr, Design.Form_QuickInfo, States.Standard, badgeRect.ToRect());
        }

        if (drawBorderAndBack) {
            Skin.Draw_Border(gr, itemdesign, state, positionControl.ToRect());
        }
    }

    protected override string GetCompareKey() => KeyName;

    private SizeF BadgeSize() {
        var font = Skin.GetBlueFont(Design.Form_QuickInfo, States.Standard);
        var s = font.MeasureString(_caption);
        return new SizeF(s.Width + ConstBadgeHPad * 2, s.Height + ConstBadgeVPad * 2);
    }

    private void GeneratePic() {
        if (Page is not { IsDisposed: false }) {
            _tmpBmp = QuickImage.Get(ImageCode.Warnung, 128);
            return;
        }

        try {
            var canvasUsedArea = Page.CanvasUsedArea.ToRect();
            if (canvasUsedArea.Width <= 0 || canvasUsedArea.Height <= 0) {
                _tmpBmp = QuickImage.Get(ImageCode.Warnung, 64);
                return;
            }

            var maxSize = 300;
            var internalZoom = Math.Min((float)maxSize / canvasUsedArea.Width, (float)maxSize / canvasUsedArea.Height);
            internalZoom = Math.Min(1, internalZoom);

            var bmpW = (int)(canvasUsedArea.Width * internalZoom);
            var bmpH = (int)(canvasUsedArea.Height * internalZoom);
            if (bmpW <= 0 || bmpH <= 0) {
                _tmpBmp = QuickImage.Get(ImageCode.Warnung, 64);
                return;
            }

            _tmpBmp = new Bitmap(bmpW, bmpH);
            var zoomv = ItemCollectionPadItem.ZoomFitValue(canvasUsedArea, _tmpBmp.Size);
            var sliderX = -canvasUsedArea.Left * zoomv;
            var sliderY = -canvasUsedArea.Top * zoomv;
            Page.DrawToBitmap(_tmpBmp, zoomv, sliderX, sliderY, false);
        } catch {
            _tmpBmp = QuickImage.Get(ImageCode.Warnung, 64);
        }
    }

    private void RemovePic() {
        _tmpBmp?.Dispose();
        _tmpBmp = null;
    }

    #endregion
}