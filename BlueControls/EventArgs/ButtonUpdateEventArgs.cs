// Licensed under MIT; see License.md for disclaimer, details, and extended user conditions.

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
