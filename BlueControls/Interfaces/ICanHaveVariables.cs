// Licensed under MIT; see License.md for disclaimer, details, and extended user conditions.

using BlueScript.ScriptVariables;

namespace BlueControls.Interfaces;

/// <summary>
/// Wird verwendet, wenn das PadItem mit Variablen umgehen kann und sich dadurch die Anzeige ändert.
/// </summary>
public interface ICanHaveVariables {

    #region Methods

    bool ReplaceVariable(ScriptVariable variable);

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