// Authors:
// Christian Peter
//
// Copyright (c) 2023 Christian Peter
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
using System;
using System.Collections.Generic;
using System.Drawing;

namespace BlueScript.Structures;

public struct DoItFeedback {

    #region Fields

    public readonly string ErrorMessage;
    public readonly int Line;
    public readonly Variable? Variable;

    #endregion

    #region Constructors

    public DoItFeedback(int line) {
        ErrorMessage = string.Empty;
        Variable = null;
        Line = line;
    }

    public DoItFeedback(string errormessage, int line) {
        ErrorMessage = errormessage;
        Variable = null;
        Line = line;
    }

    public DoItFeedback(Variable variable, int line) {
        ErrorMessage = string.Empty;
        Variable = variable;
        Line = line;
    }

    public DoItFeedback(string valueString, string errormessage, int line) {
        ErrorMessage = errormessage;
        Variable = new VariableString(Variable.DummyName(), valueString);
        Line = line;
    }

    public DoItFeedback(List<string>? list, int line) {
        ErrorMessage = string.Empty;
        Variable = new VariableListString(list);
        Line = line;
    }

    public DoItFeedback(Bitmap bmp, int line) {
        ErrorMessage = string.Empty;
        Variable = new VariableBitmap(bmp);
        Line = line;
    }

    public DoItFeedback(bool value, int line) {
        ErrorMessage = string.Empty;
        Variable = new VariableBool(value);
        Line = line;
    }

    public DoItFeedback(double value, int line) {
        ErrorMessage = string.Empty;
        Variable = new VariableFloat((float)value);
        Line = line;
    }

    public DoItFeedback(float value, int line) {
        ErrorMessage = string.Empty;
        Variable = new VariableFloat(value);
        Line = line;
    }

    public DoItFeedback(DateTime value, int line) {
        ErrorMessage = string.Empty;
        Variable = new VariableDateTime(value);
        Line = line;
    }

    public DoItFeedback(string[] list, int line) {
        ErrorMessage = string.Empty;
        Variable = new VariableListString(list);
        Line = line;
    }

    #endregion

    #region Methods

    public static DoItFeedback AttributFehler(Method method, SplittedAttributesFeedback f, int line) =>
        new(f.ErrorMessage + " > " + method.Syntax, line);

    public static DoItFeedback Falsch(int line) => new(false, line);

    public static DoItFeedback FalscherDatentyp(int line) => new("Falscher Datentyp.", line);

    public static DoItFeedback Klammerfehler(int line) => new("Fehler bei der Klammersetzung.", line);

    public static DoItFeedback Null(int line) => new(line);

    public static DoItFeedback Schreibgschützt(int line) => new("Variable ist schreibgeschützt.", line);

    public static DoItFeedback VerschiedeneTypen(Variable var1, Variable var2, int line) =>
        new("Variable '" + var1.KeyName + "' ist nicht der erwartete Typ {" + var2.MyClassId +
            "}, sondern {" + var1.MyClassId + "}", line);

    public static DoItFeedback Wahr(int line) => new(true, line);

    #endregion
}