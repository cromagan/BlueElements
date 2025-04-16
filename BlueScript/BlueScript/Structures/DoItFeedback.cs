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

public readonly struct DoItFeedback {

    #region Constructors

    public DoItFeedback(Variable variable) {
        NeedsScriptFix = false;
        Variable = variable;
        FailedReason = string.Empty;
    }

    public DoItFeedback(string message, bool needsScriptFix, LogData? ld) {
        NeedsScriptFix = needsScriptFix;
        FailedReason = message;
        Variable = null;
        ld?.AddMessage(message);
    }

    public DoItFeedback(string valueString) {
        NeedsScriptFix = false;
        FailedReason = string.Empty;
        Variable = new VariableString(Variable.DummyName(), valueString);
    }

    public DoItFeedback(List<string>? list) {
        NeedsScriptFix = false;
        FailedReason = string.Empty;
        Variable = new VariableListString(list);
    }

    public DoItFeedback(Bitmap? bmp) {
        NeedsScriptFix = false;
        FailedReason = string.Empty;
        Variable = new VariableBitmap(bmp);
    }

    public DoItFeedback(bool value) {
        NeedsScriptFix = false;
        FailedReason = string.Empty;
        Variable = new VariableBool(value);
    }

    public DoItFeedback(double value) {
        NeedsScriptFix = false;
        FailedReason = string.Empty;
        Variable = new VariableFloat((float)value);
    }

    public DoItFeedback(float value) {
        NeedsScriptFix = false;
        FailedReason = string.Empty;
        Variable = new VariableFloat(value);
    }

    public DoItFeedback(string failedReason, bool breakFired, bool endScript) : this() {
        BreakFired = breakFired;
        EndScript = endScript;
        FailedReason = failedReason;
    }

    public DoItFeedback(IEnumerable<string> list) {
        Variable = new VariableListString(list);
    }

    public DoItFeedback() { }

    #endregion

    #region Properties

    public bool BreakFired { get; } = false;
    public bool EndScript { get; } = false;
    public bool Failed => NeedsScriptFix || !string.IsNullOrWhiteSpace(FailedReason);
    public string FailedReason { get; } = string.Empty;
    public bool NeedsScriptFix { get; } = false;
    public Variable? Variable { get; }

    #endregion

    #region Methods

    public static DoItFeedback AttributFehler(LogData ld, Method method, SplittedAttributesFeedback f) =>
        new("Befehl: " + method.Syntax + "\r\n" + f.ErrorMessage, true, ld);

    public static DoItFeedback Falsch() => new(false);

    public static DoItFeedback FalscherDatentyp(LogData ld) => new("Falscher Datentyp.", true, ld);

    public static DoItFeedback InternerFehler(LogData? ld) => new("Interner Programmierfehler. Admin verständigen.", true, ld);

    public static DoItFeedback KlammerFehler(LogData ld) => new("Fehler bei der Klammersetzung.", true, ld);

    public static DoItFeedback Null() => new();

    public static DoItFeedback Schreibgschützt(LogData ld) => new("Variable ist schreibgeschützt.", true, ld);

    public static DoItFeedback TestModusInaktiv(LogData ld) => new("Im Testmodus deaktiviert.", true, ld);

    public static DoItFeedback VerschiedeneTypen(LogData ld, Variable var1, Variable var2) =>
        new($"Variable '{var1.KeyName}' ist nicht der erwartete Typ {var2.MyClassId}, sondern {var1.MyClassId}", true, ld);

    public static DoItFeedback Wahr() => new(true);

    public static DoItFeedback WertKonnteNichtGesetztWerden(LogData ld, int atno) => new($"Der Wert das Attributes {atno + 1} konnte nicht gesetzt werden.", true, ld);

    #endregion
}