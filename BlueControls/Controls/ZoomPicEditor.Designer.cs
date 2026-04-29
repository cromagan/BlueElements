// Licensed under AGPL-3.0; see License.md for disclaimer and details.

using System.ComponentModel;

namespace BlueControls.Controls;

partial class ZoomPicEditor {
    private IContainer components = null;

    protected override void Dispose(bool disposing) {
        if (disposing) {
            components?.Dispose();
        }
        base.Dispose(disposing);
    }

    #region Vom Komponenten-Designer generierter Code

    private void InitializeComponent() => components = new Container();

    #endregion
}
