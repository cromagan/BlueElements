// Licensed under MIT; see License.md for disclaimer, details, and extended user conditions.

namespace BlueTable.EventArgs;

public class FilterEventArgs : System.EventArgs {

    #region Constructors

    public FilterEventArgs(FilterItem? fi) => Filter = fi;

    #endregion

    #region Properties

    public FilterItem? Filter { get; }

    #endregion
}