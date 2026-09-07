// Licensed under MIT; see License.md for disclaimer, details, and extended user conditions.

namespace BlueBasics.Interfaces;

public interface IReadableText {

    #region Methods

    string ReadableText();

    QuickImage? SymbolForReadableText();

    #endregion
}