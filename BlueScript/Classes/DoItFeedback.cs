// Licensed under AGPL-3.0; see License.md for disclaimer and details.

using System.Drawing;

namespace BlueScript.Classes;

/// <summary>
/// Base feedback structure that represents the result of a script operation
/// </summary>
public class DoItFeedback {

    #region Constructors

    public DoItFeedback(bool needsScriptFix, bool breakFired, bool returnFired, string failedReason, Variable? returnValue, LogData? ld) {
        BreakFired = breakFired;
        ReturnFired = returnFired;

        FailedReason = failedReason;
        NeedsScriptFix = needsScriptFix;

        if (Failed) {
            ld?.AddMessage(failedReason);
        } else {
            ReturnValue = returnValue;
        }
    }

    public DoItFeedback() { }

    public DoItFeedback(Variable variable) => ReturnValue = variable;

    public DoItFeedback(string failedReason, bool needsScriptFix, LogData? ld) : this(needsScriptFix, false, false, failedReason, null, ld) { }

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

    public static DoItFeedback AttributFehler(LogData? ld, SplittedAttributesFeedback f) =>
        new(f.FailedReason, f.NeedsScriptFix, ld);

    public static DoItFeedback Falsch() => new(false);

    public static DoItFeedback FalscherDatentyp(LogData ld) => new("Falscher Datentyp.", true, ld);

    public static DoItFeedback InternerFehler(LogData? ld) => new("Interner Programmierfehler. Admin verständigen.", true, ld);

    public static DoItFeedback Null() => new();

    public static DoItFeedback Schreibgschützt(LogData ld) => new("Variable ist schreibgeschützt.", true, ld);

    public static DoItFeedback TestModusInaktiv(LogData ld) => new("Im Testmodus deaktiviert.", true, ld);

    public static DoItFeedback Wahr() => new(true);

    public static DoItFeedback WertKonnteNichtGesetztWerden(LogData ld, int atno) => new($"Der Wert das Attributes {atno + 1} konnte nicht gesetzt werden.", true, ld);

    public virtual void ChangeFailedReason(string newfailedReason, bool needsScriptFix, LogData? ld) {
        if (string.IsNullOrEmpty(newfailedReason)) { newfailedReason = "Allgemeiner Fehler"; }

        FailedReason = newfailedReason;
        ld?.AddMessage(newfailedReason);
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