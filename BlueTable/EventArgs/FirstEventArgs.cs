// Licensed under AGPL-3.0; see License.md for disclaimer and details.

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