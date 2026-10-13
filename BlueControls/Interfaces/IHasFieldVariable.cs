// Licensed under MIT; see License.md for disclaimer, details, and extended user conditions.

using BlueControls.PadItems.Abstract;
using BlueScript.ScriptVariables;
using System.Windows.Forms;

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

public static class IHasFieldVariableExtension {

    #region Methods

    /// <summary>
    ///vField-Variablen-Quellen: Parent selbst (falls IHasFieldVariable) und
    /// alle Child-Controls, die IHasFieldVariable implementieren.
    /// </summary>
    /// <param name="control"></param>
    /// <returns></returns>
    public static List<IHasFieldVariable>? GetFieldSources(Control control) {
        List<IHasFieldVariable>? fieldSources = null;
        if (control.Parent is not null) {
            fieldSources = [];
            if (control.Parent is IHasFieldVariable hfvp) { fieldSources.Add(hfvp); }
            if (control.Parent.Controls is { } pControls) {
                foreach (var thisCon in pControls) {
                    if (thisCon is IHasFieldVariable hfv) { fieldSources.Add(hfv); }
                }
            }
        }

        return fieldSources;
    }

    public static List<IHasFieldVariable>? GetFieldSources(PadItem item) {
        List<IHasFieldVariable>? fieldSources = null;
        if (item.Parent is CollectionPadItem { IsDisposed: false } icpi) {
            fieldSources = [];
            foreach (var thisCon in icpi) {
                if (thisCon is IHasFieldVariable hfv) { fieldSources.Add(hfv); }
            }
        }
        return fieldSources;
    }

    internal static void WriteBackFieldSources(List<IHasFieldVariable>? fieldSources, VariableCollection? vars) {
        if (fieldSources is not null && vars is not null) {
            foreach (var hfv in fieldSources) {
                if (vars.GetByKey(hfv.FieldName) is { ReadOnly: false } v) {
                    hfv.SetValueFromVariable(v);
                }
            }
        }
    }

    #endregion
}