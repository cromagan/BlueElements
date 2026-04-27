// Licensed under AGPL-3.0; see License.md for disclaimer and details.

using System.Text.Json.Nodes;

namespace BlueControls.EventArgs;

public class ViewEventArgs : System.EventArgs {

    #region Constructors

    public ViewEventArgs(string viewName, JsonObject viewData) {
        ViewName = viewName;
        ViewData = viewData;
    }

    #endregion

    #region Properties

    public string ViewName { get; }

    public JsonObject ViewData { get; }

    #endregion
}
