// Licensed under AGPL-3.0; see License.md for disclaimer and details.

using System.Drawing;

namespace BlueScript.Classes;

/// <summary>
/// Base feedback structure that represents the result of a script operation
/// </summary>
public class DoItFeedback {

    #region Constructors

    /// <summary>
    /// Vollständiger Konstruktor. Wird auch von Subklassen
    /// (<see cref="ScriptEndedFeedback" />, <see cref="DoItWithEndedPosFeedback" />)
    /// genutzt.
    /// </summary>
    public DoItFeedback(bool needsScriptFix, bool breakFired, bool returnFired, string failedReason, Variable? returnValue) {
        BreakFired = breakFired;
        ReturnFired = returnFired;

        FailedReason = failedReason;
        NeedsScriptFix = needsScriptFix;

        if (!Failed) {
            ReturnValue = returnValue;
        }
    }

    public DoItFeedback() { }

    public DoItFeedback(Variable variable) => ReturnValue = variable;

    public DoItFeedback(string failedReason, bool needsScriptFix) : this(needsScriptFix, false, false, failedReason, null) { }

    public DoItFeedback(string valueString) : this(new VariableString(Variable.DummyName(), valueString)) { }

    public DoItFeedback(List<string>? list) : this(new VariableListString(list)) { }

    public DoItFeedback(Bitmap? bmp) : this(new VariableBitmap(bmp)) { }

    public DoItFeedback(bool value) : this(new VariableBool(value)) { }

    public DoItFeedback(double value) : this(new VariableDouble(value)) { }

    public DoItFeedback(float value) : this(new VariableDouble(value)) { }

    public DoItFeedback(IEnumerable<string> list) : this(new VariableListString(list)) { }

    #endregion

    #region Properties

    public bool BreakFired { get; private set; }

    public virtual bool Failed => NeedsScriptFix || !string.IsNullOrWhiteSpace(FailedReason);
    public string FailedReason { get; private set; } = string.Empty;
    public bool NeedsScriptFix { get; private set; }
    public bool ReturnFired { get; private set; }
    public Variable? ReturnValue { get; private set; }

    #endregion

    #region Methods

    public static DoItFeedback AttributFehler(SplittedAttributesFeedback f) =>
        new(f.FailedReason, f.NeedsScriptFix);

    public static DoItFeedback Falsch() => new(false);

    public static DoItFeedback FalscherDatentyp() => new("Falscher Datentyp.", true);

    public static DoItFeedback InternerFehler() => new("Interner Programmierfehler. Admin verständigen.", true);

    public static DoItFeedback Null() => new();

    public static DoItFeedback Schreibgschützt() => new("Variable ist schreibgeschützt.", true);

    public static DoItFeedback TestModusInaktiv() => new("Im Testmodus deaktiviert.", true);

    public static DoItFeedback Wahr() => new(true);

    public static DoItFeedback WertKonnteNichtGesetztWerden(int atno) => new($"Der Wert das Attributes {atno + 1} konnte nicht gesetzt werden.", true);

    public virtual void ChangeFailedReason(string newfailedReason, bool needsScriptFix) {
        if (string.IsNullOrEmpty(newfailedReason)) { newfailedReason = "Allgemeiner Fehler"; }

        FailedReason = newfailedReason;
        NeedsScriptFix = needsScriptFix;
        ReturnValue = null;
    }

    public void ConsumeBreak() => BreakFired = false;

    public void ConsumeBreakAndReturn() {
        BreakFired = false;
        ReturnFired = false;
    }

    #endregion
}
