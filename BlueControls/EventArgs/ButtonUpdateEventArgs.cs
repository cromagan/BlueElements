// Licensed under AGPL-3.0; see License.md for disclaimer and details.

namespace BlueControls.EventArgs;

public class ButtonUpdateEventArgs : System.EventArgs {

    #region Constructors

    public ButtonUpdateEventArgs(bool isInForm, bool mouseOverChanged) {
        IsInForm = isInForm;
        MouseOverChanged = mouseOverChanged;
    }

    #endregion

    #region Properties

    public bool IsInForm { get; }

    public bool MouseOverChanged { get; }

    #endregion
}
