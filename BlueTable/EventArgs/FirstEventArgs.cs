// Licensed under MIT; see License.md for disclaimer, details, and extended user conditions.

namespace BlueTable.EventArgs;

public class FirstEventArgs : System.EventArgs {

    #region Constructors

    public FirstEventArgs(bool isFirst, bool affectingHead) {
        IsFirst = isFirst;
        AffectingHead = affectingHead;
    }

    #endregion

    #region Properties

    public bool AffectingHead { get; }
    public bool IsFirst { get; }

    #endregion
}