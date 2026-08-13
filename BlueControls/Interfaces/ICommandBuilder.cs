// Licensed under AGPL-3.0; see License.md for disclaimer and details.

namespace BlueControls.Interfaces;

public interface ICommandBuilder : IHasKeyName {

    #region Methods

    string CommandDescription();

    QuickImage CommandImage();

    string GetCode(Form? form);

    #endregion
}