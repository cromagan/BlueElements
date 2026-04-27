// Licensed under AGPL-3.0; see License.md for disclaimer and details.

namespace BlueBasics.Interfaces;

public interface IErrorCheckable {

    #region Methods

    string ErrorReason();

    #endregion
}

public static class ErrorCheckableExtension {

    #region Methods

    public static bool IsOk(this IErrorCheckable item) => string.IsNullOrEmpty(item.ErrorReason());

    #endregion
}