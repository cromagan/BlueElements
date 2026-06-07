// Licensed under AGPL-3.0; see License.md for disclaimer and details.

using System.Text.Json.Nodes;

namespace BlueControls.EventArgs;

public class JsonEventArgs : System.EventArgs, IHasKeyName {

    #region Constructors

    public JsonEventArgs(string keyName, JsonObject jsonData) {
        KeyName = keyName;
        JsonData = jsonData;
    }

    #endregion

    #region Properties

    public JsonObject JsonData { get; }
    public string KeyName { get; }

    #endregion
}