// Licensed under MIT; see License.md for disclaimer, details, and extended user conditions.

namespace BlueControls.EventArgs;

public class FormEventArgs : System.EventArgs {

    #region Constructors

    public FormEventArgs(System.Windows.Forms.Form form) => Form = form;

    #endregion

    #region Properties

    public System.Windows.Forms.Form Form { get; private set; }

    #endregion
}