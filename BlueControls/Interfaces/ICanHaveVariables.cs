// Licensed under AGPL-3.0; see License.md for disclaimer and details.

using BlueScript.Variables;

namespace BlueControls.Interfaces;

/// <summary>
/// Wird verwendet, wenn das PadItem mit Variablen umgehen kann und sich dadurch die Anzeige ändert.
/// </summary>
public interface ICanHaveVariables {

    #region Methods

    bool ReplaceVariable(Variable variable);

    bool ResetVariables();

    #endregion
}

public static class CanHaveVariables {

    #region Methods

    public static void ParseVariables(this ICanHaveVariables obj, VariableCollection? variables) {
        obj.ResetVariables();
        if (variables is null) { return; }

        foreach (var thisV in variables) {
            obj.ReplaceVariable(thisV);
        }
    }

    #endregion
}