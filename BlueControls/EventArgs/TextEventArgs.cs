// Licensed under AGPL-3.0; see License.md for disclaimer and details.

namespace BlueControls.EventArgs;

public class TextEventArgs : System.EventArgs {

    #region Constructors

    public TextEventArgs(string txt) => Text = txt;

    #endregion

    #region Properties

    public string Text { get; }

    #endregion
}