// Licensed under MIT; see License.md for disclaimer, details, and extended user conditions.

namespace BlueControls.EventArgs;

public class AddItemEventArgs : System.EventArgs {

    #region Constructors

    public AddItemEventArgs(string text) => Text = text;

    #endregion

    #region Properties

    /// <summary>
    /// Wenn <c>true</cg>, wird das Item nicht automatisch hinzugefügt.
    /// Der Handler übernimmt die Erstellung selbst.
    /// </summary>
    public bool Cancel { get; set; }

    /// <summary>
    /// Der vom Benutzer eingegebene oder ausgewählte Text.
    /// </summary>
    public string Text { get; }

    #endregion
}
