// Licensed under AGPL-3.0; see License.md for disclaimer and details.

namespace BlueTable.EventArgs;

public class FilterEventArgs : System.EventArgs {

    #region Constructors

    public FilterEventArgs(FilterItem? fi) => Filter = fi;

    #endregion

    #region Properties

    public FilterItem? Filter { get; }

    #endregion
}