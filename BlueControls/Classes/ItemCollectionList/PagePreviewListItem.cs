// Licensed under AGPL-3.0; see License.md for disclaimer and details.

using BlueControls.Classes.ItemCollectionPad;

namespace BlueControls.Classes.ItemCollectionList;

public class PagePreviewListItem : AbstractListItem {

    #region Fields

    private const int ConstCaptionHeight = 20;
    private const int ConstPadding = 8;
    private Bitmap? _tmpBmp;
    private string _caption;

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
        if (_tmpBmp is null) { return columnWidth + ConstCaptionHeight; }

        var sc = (float)_tmpBmp.Height / _tmpBmp.Width;
        if (sc > 1) { sc = 1; }

        return (int)(sc * (columnWidth - ConstPadding * 2)) + ConstPadding * 2 + ConstCaptionHeight;
    }

    public void RefreshPreview() {
        RemovePic();
    }

    protected override Size ComputeUntrimmedCanvasSize(Design itemdesign) {
        try {
            if (_tmpBmp is null) { GeneratePic(); }
            if (_tmpBmp is null) { return new Size(300, 300 + ConstPadding * 2 + ConstCaptionHeight); }

            var sc = (float)_tmpBmp.Height / _tmpBmp.Width;
            if (sc > 1) { sc = 1; }

            return new Size(300, (int)(sc * (300 - ConstPadding * 2)) + ConstPadding * 2 + ConstCaptionHeight);
        } catch {
            Develop.AbortAppIfStackOverflow();
            return ComputeUntrimmedCanvasSize(itemdesign);
        }
    }

    protected override void DrawExplicit(Graphics gr, Rectangle visibleAreaControl, RectangleF positionControl, Design itemdesign, States state, bool drawBorderAndBack, bool translate, float offsetX, float offsetY, float zoom) {
        if (_tmpBmp is null) { GeneratePic(); }

        if (drawBorderAndBack) {
            Skin.Draw_Back(gr, itemdesign, state, positionControl.ToRect(), null, false);
        }

        var paddedArea = new RectangleF(
            positionControl.Left + ConstPadding,
            positionControl.Top + ConstPadding,
            positionControl.Width - ConstPadding * 2,
            positionControl.Height - ConstPadding * 2 - ConstCaptionHeight);

        var whiteBack = new RectangleF(paddedArea.Left, paddedArea.Top, paddedArea.Width, paddedArea.Height);
        gr.FillRectangle(Brushes.White, whiteBack);

        if (_tmpBmp is not null) {
            var sc = (float)Math.Min(paddedArea.Width / (double)_tmpBmp.Width, paddedArea.Height / (double)_tmpBmp.Height);
            var imgW = _tmpBmp.Width * sc;
            var imgH = _tmpBmp.Height * sc;
            var imgRect = new RectangleF(
                paddedArea.Left + (paddedArea.Width - imgW) / 2,
                paddedArea.Top + (paddedArea.Height - imgH) / 2,
                imgW, imgH);

            gr.DrawImage(_tmpBmp, imgRect, new RectangleF(0, 0, _tmpBmp.Width, _tmpBmp.Height), GraphicsUnit.Pixel);

            gr.DrawRectangle(Pens.DarkGray, imgRect.Left, imgRect.Top, imgRect.Width, imgRect.Height);
        }

        gr.DrawRectangle(Pens.LightGray, whiteBack.Left, whiteBack.Top, whiteBack.Width, whiteBack.Height);

        var captionRect = new RectangleF(positionControl.Left, positionControl.Bottom - ConstCaptionHeight, positionControl.Width, ConstCaptionHeight);
        Skin.Draw_FormatedText(gr, _caption, null, Alignment.Horizontal_Vertical_Center, captionRect.ToRect(), itemdesign, state, null, false, false);

        if (drawBorderAndBack) {
            Skin.Draw_Border(gr, itemdesign, state, positionControl.ToRect());
        }
    }

    protected override string GetCompareKey() => KeyName;

    protected override void Dispose(bool disposing) {
        if (disposing) {
            RemovePic();
        }
        base.Dispose(disposing);
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
            Page.DrawToBitmap(_tmpBmp, zoomv, sliderX, sliderY);
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
