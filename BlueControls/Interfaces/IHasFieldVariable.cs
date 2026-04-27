// Licensed under AGPL-3.0; see License.md for disclaimer and details.

using BlueScript.Variables;

namespace BlueControls.Interfaces;

/// <summary>
/// Für Steuerelemente in Connected Formula, die Variabelen für Skripte bereitstellen.
/// </summary>
public interface IHasFieldVariable {

    #region Properties

    string FieldName { get; }

    #endregion

    #region Methods

    Variable? GetFieldVariable();

    void SetValueFromVariable(Variable v);

    #endregion
}