// Licensed under AGPL-3.0; see License.md for disclaimer and details.

namespace BlueBasics.Interfaces;

public interface IHasKeyName {

    #region Properties

    bool KeyIsCaseSensitive { get; }
    string KeyName { get; }

    #endregion
}