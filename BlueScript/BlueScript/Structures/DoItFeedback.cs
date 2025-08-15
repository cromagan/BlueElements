// Authors:
// Christian Peter
//
// Copyright (c) 2025 Christian Peter
// https://github.com/cromagan/BlueElements
//
// License: GNU Affero General Public License v3.0
// https://github.com/cromagan/BlueElements/blob/master/LICENSE
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL
// THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING
// FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER
// DEALINGS IN THE SOFTWARE.

#nullable enable

using BlueScript.Methods;
using BlueScript.Variables;
using System.Collections.Generic;
using System.Drawing;

namespace BlueScript.Structures;

/// <summary>
/// Base feedback structure that represents the result of a script operation
/// </summary>
public class DoItFeedback : CurrentPosition {

    #region Constructors

    public DoItFeedback(bool needsScriptFix, bool breakFired, bool returnFired, string failedReason, Variable? returnValue, CurrentPosition? cp) : base(cp) {
        BreakFired = breakFired;
        ReturnFired = returnFired;

        FailedReason = failedReason;
        NeedsScriptFix = needsScriptFix;

        if (!Failed) {
            ReturnValue = returnValue;
        }
    }

    public DoItFeedback() : base() { }

    public DoItFeedback(CurrentPosition cp) : base(cp) { }

    public DoItFeedback(Variable variable, CurrentPosition cp) : base(cp) {
        ReturnValue = variable;
    }

    public DoItFeedback(string failedReason, bool needsScriptFix, CurrentPosition? cp) : this(needsScriptFix, false, false, failedReason, null, cp) { }

    public DoItFeedback(string valueString, CurrentPosition cp) : this(new VariableString(Variable.DummyName(), valueString), cp) { }

    public DoItFeedback(List<string>? list, CurrentPosition cp) : this(new VariableListString(list), cp) { }

    public DoItFeedback(Bitmap? bmp, CurrentPosition cp) : this(new VariableBitmap(bmp), cp) { }

    public DoItFeedback(bool value, CurrentPosition cp) : this(new VariableBool(value), cp) { }

    public DoItFeedback(double value, CurrentPosition cp) : this(new VariableDouble(value), cp) { }

    public DoItFeedback(float value, CurrentPosition cp) : this(new VariableDouble(value), cp) { }

    public DoItFeedback(IEnumerable<string> list, CurrentPosition cp) : this(new VariableListString(list), cp) { }

    #endregion

    #region Properties

    public bool BreakFired { get; private set; } = false;
    public virtual bool Failed => NeedsScriptFix || !string.IsNullOrWhiteSpace(FailedReason);
    public string FailedReason { get; private set; } = string.Empty;

    public bool NeedsScriptFix { get; } = false;

    public bool ReturnFired { get; private set; } = false;
    public Variable? ReturnValue { get; private set; } = null;

    #endregion

    #region Methods

    public static DoItFeedback AttributFehler(CurrentPosition? cp, Method method, SplittedAttributesFeedback f) =>
        new("Befehl: " + method.Syntax + "\r\n" + f.FailedReason, f.NeedsScriptFix, cp);

    public static DoItFeedback Falsch(CurrentPosition cp) => new(false, cp);

    public static DoItFeedback FalscherDatentyp(CurrentPosition cp) => new("Falscher Datentyp.", true, cp);

    public static DoItFeedback InternerFehler(CurrentPosition? cp) => new("Interner Programmierfehler. Admin verständigen.", true, cp);

    public static DoItFeedback KlammerFehler(CurrentPosition cp) => new("Fehler bei der Klammersetzung.", true, cp);

    public static DoItFeedback Null(CurrentPosition cp) => new(cp);

    public static DoItFeedback Schreibgschützt(CurrentPosition cp) => new("Variable ist schreibgeschützt.", true, cp);

    public static DoItFeedback TestModusInaktiv(CurrentPosition cp) => new("Im Testmodus deaktiviert.", true, cp);

    public static DoItFeedback Wahr(CurrentPosition cp) => new(true, cp);

    public static DoItFeedback WertKonnteNichtGesetztWerden(CurrentPosition cp, int atno) => new($"Der Wert das Attributes {atno + 1} konnte nicht gesetzt werden.", true, cp);

    public void ChangeFailedReason(string newfailedReason) {
        if (string.IsNullOrEmpty(newfailedReason)) { newfailedReason = "Allgemeiner Fehler"; }

        FailedReason = newfailedReason;
        ReturnValue = null;
    }

    public void ConsumeBreak() {
        BreakFired = false;
    }

    public void ConsumeBreakAndReturn() {
        BreakFired = false;
        ReturnFired = false;
    }

    #endregion
}