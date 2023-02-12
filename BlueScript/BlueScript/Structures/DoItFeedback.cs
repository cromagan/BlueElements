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

using System;
using System.Collections.Generic;
using System.Drawing;
using BlueScript.Methods;
using BlueScript.Variables;

namespace BlueScript.Structures;

public struct DoItFeedback {

    #region Fields

    public readonly string ErrorMessage;

    public readonly int Line;
    public readonly Variable? Variable;

    #endregion

    #region Constructors

    public DoItFeedback(CanDoFeedback? infos, Script? s) {
        ErrorMessage = string.Empty;
        Variable = null;
        Line = Script.Line(s?.ScriptText, infos?.ContinueOrErrorPosition);
    }

    public DoItFeedback(CanDoFeedback? infos, Script? s, string errormessage) {
        ErrorMessage = errormessage;
        Variable = null;
        Line = Script.Line(s?.ScriptText, infos?.ContinueOrErrorPosition);
    }

    public DoItFeedback(CanDoFeedback? infos, Script? s, Variable variable) {
        ErrorMessage = string.Empty;
        Variable = variable;
        Line = Script.Line(s?.ScriptText, infos?.ContinueOrErrorPosition);
    }

    public DoItFeedback(string errormessage, int line) {
        ErrorMessage = errormessage;
        Variable = null;
        Line = line;
    }

    public DoItFeedback(CanDoFeedback? infos, Script? s, string valueString, string errormessage) {
        ErrorMessage = errormessage;
        Variable = new VariableString(Variable.DummyName(), valueString);
        Line = Script.Line(s?.ScriptText, infos?.ContinueOrErrorPosition);
    }

    public DoItFeedback(CanDoFeedback? infos, Script? s, List<string>? list) {
        ErrorMessage = string.Empty;
        Variable = new VariableListString(list);
        Line = Script.Line(s?.ScriptText, infos?.ContinueOrErrorPosition);
    }

    public DoItFeedback(CanDoFeedback? infos, Script? s, Bitmap bmp) {
        ErrorMessage = string.Empty;
        Variable = new VariableBitmap(bmp);
        Line = Script.Line(s?.ScriptText, infos?.ContinueOrErrorPosition);
    }

    public DoItFeedback(CanDoFeedback? infos, Script? s, bool value) {
        ErrorMessage = string.Empty;
        Variable = new VariableBool(value);
        Line = Script.Line(s?.ScriptText, infos?.ContinueOrErrorPosition);
    }

    public DoItFeedback(CanDoFeedback? infos, Script? s, double value) {
        ErrorMessage = string.Empty;
        Variable = new VariableFloat((float)value);
        Line = Script.Line(s?.ScriptText, infos?.ContinueOrErrorPosition);
    }

    public DoItFeedback(CanDoFeedback? infos, Script? s, float value) {
        ErrorMessage = string.Empty;
        Variable = new VariableFloat(value);
        Line = Script.Line(s?.ScriptText, infos?.ContinueOrErrorPosition);
    }

    public DoItFeedback(CanDoFeedback? infos, Script? s, DateTime value) {
        ErrorMessage = string.Empty;
        Variable = new VariableDateTime(value);
        Line = Script.Line(s?.ScriptText, infos?.ContinueOrErrorPosition);
    }

    public DoItFeedback(CanDoFeedback? infos, Script? s, string[] list) {
        ErrorMessage = string.Empty;
        Variable = new VariableListString(list);
        Line = Script.Line(s?.ScriptText, infos?.ContinueOrErrorPosition);
    }

    #endregion

    #region Methods

    public static DoItFeedback AttributFehler(CanDoFeedback? infos, Script? s, Method method, SplittedAttributesFeedback f) =>
        new(infos, s, f.ErrorMessage + " > " + method.Syntax);

    public static DoItFeedback Falsch(CanDoFeedback? infos, Script s) => new(infos, s, false);

    public static DoItFeedback FalscherDatentyp(CanDoFeedback? infos, Script s) => new(infos, s, "Falscher Datentyp.");

    public static DoItFeedback Klammerfehler(CanDoFeedback? infos, Script s) => new(infos, s, "Fehler bei der Klammersetzung.");

    public static DoItFeedback Null(CanDoFeedback? infos, Script s) => new(infos, s);

    public static DoItFeedback Schreibgschützt(CanDoFeedback? infos, Script s) => new(infos, s, "Variable ist schreibgeschützt.");

    public static DoItFeedback VerschiedeneTypen(CanDoFeedback? infos, Script? s, Variable var1, Variable var2) =>
        new(infos, s, "Variable '" + var1.KeyName + "' ist nicht der erwartete Typ {" + var2.MyClassId +
                      "}, sondern {" + var1.MyClassId + "}");

    public static DoItFeedback Wahr(CanDoFeedback? infos, Script s) => new(infos, s, true);

    #endregion
}