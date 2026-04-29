// Licensed under AGPL-3.0; see License.md for disclaimer and details.

using BlueControls;

namespace BluePaint;

public partial class Tool_Screenshot {

    #region Constructors

    public Tool_Screenshot() : base() => InitializeComponent();

    #endregion

    #region Methods

    public override void ToolFirstShown() {
        DoScreenShot();
        OnZoomFit();
    }

    private void DoScreenShot() {
        OnHideMainWindow();
        Generic.Pause(1, true);
        var pic = ScreenShot.GrabArea(null);
        OnOverridePic(pic.Area, true);
        OnShowMainWindow();
    }

    private void NeuerScreenshot_Click(object sender, System.EventArgs e) {
        DoScreenShot();
        OnZoomFit();
    }

    #endregion
}