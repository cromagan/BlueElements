// Licensed under MIT; see License.md for disclaimer, details, and extended user conditions.

using BlueScript.ScriptVariables;

namespace BlueControls.Interfaces;

/// <summary>
/// Für Steuerelemente in Connected Formula, die Variabelen für Skripte bereitstellen.
/// </summary>
public interface IHasFieldVariable {

    #region Properties

    string FieldName { get; }

    #endregion

    #region Methods

    ScriptVariable? GetFieldVariable();

    void SetValueFromVariable(ScriptVariable v);

    #endregion
}