// Licensed under AGPL-3.0; see License.md for disclaimer and details.

namespace BlueBasics.Interfaces;

public interface IReadableTextWithKey : IReadableText, IHasKeyName {

    #region Properties

    string QuickInfo { get; }

    #endregion
}