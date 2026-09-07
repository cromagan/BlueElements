// Licensed under MIT; see License.md for disclaimer, details, and extended user conditions.

namespace BlueControls.Interfaces;

public interface ICommandBuilder : IHasKeyName {

    #region Methods

    string CommandDescription();

    QuickImage CommandImage();

    string GetCode(Form? form);

    #endregion
}