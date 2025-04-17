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
public class DoItFeedback {

    #region Constructors

    public DoItFeedback() { }

    public DoItFeedback(Variable variable) {
        Variable = variable;
    }

    public DoItFeedback(string message, bool needsScriptFix, LogData? ld) {
        NeedsScriptFix = needsScriptFix;
        FailedReason = message;


        if (Failed) {
            Variable = null;
            ld?.AddMessage(message);
        }
    }

    public DoItFeedback(bool breakFired, bool endScript) : this() {
        BreakFired = breakFired;
        EndScript = endScript;
    }

    // Value type constructors
    public DoItFeedback(string valueString) : this(new VariableString(Variable.DummyName(), valueString)) { }

    public DoItFeedback(List<string>? list) : this(new VariableListString(list)) { }

    public DoItFeedback(Bitmap? bmp) : this(new VariableBitmap(bmp)) { }

    public DoItFeedback(bool value) : this(new VariableBool(value)) { }

    public DoItFeedback(double value) : this(new VariableFloat((float)value)) { }

    public DoItFeedback(float value) : this(new VariableFloat(value)) { }

    public DoItFeedback(IEnumerable<string> list) : this(new VariableListString(list)) { }

    #endregion

    #region Properties

    public bool BreakFired { get; protected set; } = false;
    public bool EndScript { get; protected set; } = false;
    public virtual bool Failed => NeedsScriptFix || !string.IsNullOrWhiteSpace(FailedReason);
    public string FailedReason { get; protected set; } = string.Empty;
    public bool NeedsScriptFix { get; protected set; } = false;
    public Variable? Variable { get; protected set; }

    #endregion

    #region Methods

    public static DoItFeedback AttributFehler(LogData ld, Method method, SplittedAttributesFeedback f) =>
        new("Befehl: " + method.Syntax + "\r\n" + f.FailedReason, f.NeedsScriptFix, ld);

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