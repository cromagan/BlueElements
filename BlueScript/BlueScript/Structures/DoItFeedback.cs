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
        AllOk = true;
        Variable = variable;
        NotSuccesfulReason = string.Empty;
    }

    public DoItFeedback(LogData? ld, string errormessage) {
        AllOk = false;
        Variable = null;
        NotSuccesfulReason = string.Empty;
        ld?.AddMessage(errormessage);
    }

    public DoItFeedback(string valueString) {
        AllOk = true;
        NotSuccesfulReason = string.Empty;
        Variable = new VariableString(Variable.DummyName(), valueString);
    }

    public DoItFeedback(List<string>? list) {
        AllOk = true;
        NotSuccesfulReason = string.Empty;
        Variable = new VariableListString(list);
    }

    public DoItFeedback(Bitmap? bmp) {
        AllOk = true;
        NotSuccesfulReason = string.Empty;
        Variable = new VariableBitmap(bmp);
    }

    public DoItFeedback(bool value) {
        AllOk = true;
        NotSuccesfulReason = string.Empty;
        Variable = new VariableBool(value);
    }

    public DoItFeedback(double value) {
        AllOk = true;
        NotSuccesfulReason = string.Empty;
        Variable = new VariableFloat((float)value);
    }

    public DoItFeedback(float value) {
        AllOk = true;
        NotSuccesfulReason = string.Empty;
        Variable = new VariableFloat(value);
    }

    public DoItFeedback(bool breakFired, bool endScript, string notsuccesfulreason) : this() {
        BreakFired = breakFired;
        EndScript = endScript;
        NotSuccesfulReason = notsuccesfulreason;
    }

    public DoItFeedback(IEnumerable<string> list) {
        AllOk = true;
        NotSuccesfulReason = string.Empty;
        Variable = new VariableListString(list);
    }

    public DoItFeedback() {
        AllOk = true;
        NotSuccesfulReason = string.Empty;
        Variable = null;
    }

    #endregion

    #region Properties

    public string NotSuccesfulReason { get; }

    public bool AllOk { get; }
    public bool BreakFired { get; } = false;
    public bool EndScript { get; } = false;
    public Variable? Variable { get; }
    public bool Succesful => string.IsNullOrWhiteSpace(NotSuccesfulReason);

    #endregion

    #region Methods

    public static DoItFeedback AttributFehler(LogData ld, Method method, SplittedAttributesFeedback f) =>
        new(ld, "Befehl: " + method.Syntax + "\r\n" + f.ErrorMessage);

    public static DoItFeedback Falsch() => new(false);

    public static DoItFeedback FalscherDatentyp(LogData ld) => new(ld, "Falscher Datentyp.");

    public static DoItFeedback InternerFehler(LogData? ld) => new(ld, "Interner Programmierfehler. Admin verständigen.");

    public static DoItFeedback KlammerFehler(LogData ld) => new(ld, "Fehler bei der Klammersetzung.");

    public static DoItFeedback TestModusInaktiv(LogData ld) => new(ld, "Im Testmodus deaktiviert.");

    public static DoItFeedback Null() => new();

    public static DoItFeedback Schreibgschützt(LogData ld) => new(ld, "Variable ist schreibgeschützt.");

    public static DoItFeedback VerschiedeneTypen(LogData ld, Variable var1, Variable var2) =>
        new(ld, $"Variable '{var1.KeyName}' ist nicht der erwartete Typ {var2.MyClassId}, sondern {var1.MyClassId}");

    public static DoItFeedback Wahr() => new(true);

    public static DoItFeedback WertKonnteNichtGesetztWerden(LogData ld, int atno) => new(ld, $"Der Wert das Attributes {atno + 1} konnte nicht gesetzt werden.");

    #endregion
}