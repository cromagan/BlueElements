// Licensed under MIT; see License.md for disclaimer, details, and extended user conditions.

namespace BlueControls.EventArgs;

/// <summary>
/// Liefert die beiden Indizes, die bei einem Swap (Verschieben eines Items
/// nach oben/unten) getauscht wurden.
/// </summary>
public class SwapEventArgs(int index1, int index2) : System.EventArgs {

    #region Properties

    public int Index1 { get; } = index1;

    public int Index2 { get; } = index2;

    #endregion

}
