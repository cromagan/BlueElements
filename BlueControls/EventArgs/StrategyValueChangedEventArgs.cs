// Licensed under AGPL-3.0; see License.md for disclaimer and details.

namespace BlueControls.EventArgs;

public class StrategyValueChangedEventArgs : System.EventArgs {

    #region Constructors

    public StrategyValueChangedEventArgs(string value, bool updateControls = false) {
        Value = value;
        UpdateControls = updateControls;
    }

    #endregion

    #region Properties

    public string Value { get; }
    public bool UpdateControls { get; }

    #endregion
}
