// Licensed under MIT; see License.md for disclaimer, details, and extended user conditions.

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