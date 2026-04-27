// Licensed under AGPL-3.0; see License.md for disclaimer and details.

using BlueBasics.Classes;

namespace BlueBasics.Interfaces;

public interface IReadableText {

    #region Methods

    string ReadableText();

    QuickImage? SymbolForReadableText();

    #endregion
}