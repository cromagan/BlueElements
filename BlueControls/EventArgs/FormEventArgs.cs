// Licensed under AGPL-3.0; see License.md for disclaimer and details.

namespace BlueControls.EventArgs;

public class FormEventArgs : System.EventArgs {

    #region Constructors

    public FormEventArgs(System.Windows.Forms.Form form) => Form = form;

    #endregion

    #region Properties

    public System.Windows.Forms.Form Form { get; private set; }

    #endregion
}