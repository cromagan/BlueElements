// Licensed under AGPL-3.0; see License.md for disclaimer and details.

namespace BlueControls.Interfaces;

public interface IComandBuilder : IHasKeyName {

    #region Methods

    string ComandDescription();

    QuickImage ComandImage();

    string GetCode(Form? form);

    #endregion
}