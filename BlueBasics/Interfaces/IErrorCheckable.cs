// Licensed under MIT; see License.md for disclaimer, details, and extended user conditions.

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