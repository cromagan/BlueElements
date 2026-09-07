// Licensed under MIT; see License.md for disclaimer, details, and extended user conditions.

namespace BlueControls.EventArgs;

public class TextEventArgs : System.EventArgs {

    #region Constructors

    public TextEventArgs(string txt) => Text = txt;

    #endregion

    #region Properties

    public string Text { get; }

    #endregion
}