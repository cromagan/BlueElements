// Licensed under MIT; see License.md for disclaimer, details, and extended user conditions.

using BlueControls;

namespace BluePaint;

public partial class ScreenshotTool {

    #region Constructors

    public ScreenshotTool() : base() => InitializeComponent();

    #endregion

    #region Methods

    public override void ToolFirstShown() {
        DoScreenShot();
        OnZoomFit();
    }

    public override string ReadableText() => "Screenshot";

    public override QuickImage? SymbolForReadableText() => QuickImage.Get(ImageCode.Kamera, 16);

    private void DoScreenShot() {
        OnHideMainWindow();
        Pause(1, true);
        var pic = ScreenShot.GrabArea(null);
        OnOverridePic(pic.Area, true);
        OnShowMainWindow();
    }

    private void NeuerScreenshot_Click(object sender, EventArgs e) {
        DoScreenShot();
        OnZoomFit();
    }

    #endregion
}